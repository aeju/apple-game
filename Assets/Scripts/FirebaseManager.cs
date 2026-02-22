using UnityEngine;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;

#if !UNITY_WEBGL || UNITY_EDITOR
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
#endif

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void FirebaseBridge_Init();

    [DllImport("__Internal")]
    private static extern void FirebaseBridge_SubmitScore(string nickname, int score, int combo);

    [DllImport("__Internal")]
    private static extern void FirebaseBridge_LoadLeaderboard(int limit);

    private bool _jsInitialized = false;
    private Action<List<LeaderboardEntry>> _pendingCallback;
#endif

#if !UNITY_WEBGL || UNITY_EDITOR
    private bool _isInitialized = false;
    private string _userId = null;
    private FirebaseFirestore _db;
    private FirebaseAuth _auth;
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("[FirebaseManager] WebGL build - Initializing via JS bridge");
        FirebaseBridge_Init();
#elif !UNITY_WEBGL || UNITY_EDITOR
        Debug.Log("[FirebaseManager] Initializing Firebase (non-WebGL platform)");
        InitializeFirebase();
#endif
    }

    // ─── JS 콜백 (WebGL에서 SendMessage로 호출됨) ───

    public void OnJsInitialized(string result)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (result == "ok")
        {
            _jsInitialized = true;
            Debug.Log("[FirebaseManager] JS Firebase initialized successfully!");
        }
        else
        {
            Debug.LogError("[FirebaseManager] JS Firebase init failed: " + result);
        }
#endif
    }

    public void OnJsLeaderboardLoaded(string json)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("[FirebaseManager] JS leaderboard data received: " + json);

        var entries = new List<LeaderboardEntry>();

        try
        {
            var wrapper = JsonUtility.FromJson<JsEntryArray>("{\"items\":" + json + "}");
            if (wrapper.items != null)
            {
                foreach (var item in wrapper.items)
                {
                    entries.Add(new LeaderboardEntry
                    {
                        nickname = item.nickname,
                        score = item.score,
                        combo = item.combo,
                        timestamp = item.timestamp
                    });
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[FirebaseManager] JSON parse error: " + e.Message);
        }

        Debug.Log($"[FirebaseManager] Parsed {entries.Count} entries from JS");
        _pendingCallback?.Invoke(entries);
        _pendingCallback = null;
#endif
    }

    // ─── 공통 API ───

    public void SubmitScore(string nickname, int score, int combo)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!_jsInitialized)
        {
            Debug.LogWarning("[FirebaseManager] JS not initialized. Score not submitted.");
            return;
        }
        Debug.Log($"[FirebaseManager] Submitting score via JS: {nickname} - {score}");
        FirebaseBridge_SubmitScore(nickname, score, combo);

#elif !UNITY_WEBGL || UNITY_EDITOR
        if (!_isInitialized)
        {
            Debug.LogWarning("[FirebaseManager] Firebase not initialized. Score not submitted.");
            return;
        }

        var collection = _db.Collection("leaderboard");
        var docData = new Dictionary<string, object>
        {
            { "nickname", nickname },
            { "score", score },
            { "combo", combo },
            { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() },
            { "userId", _userId }
        };

        collection.AddAsync(docData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
                Debug.Log($"[FirebaseManager] Score submitted: {nickname} - {score}");
            else
                Debug.LogError($"[FirebaseManager] Failed to submit: {task.Exception}");
        });
#endif
    }

    public void LoadLeaderboard(Action<List<LeaderboardEntry>> callback, int limit = 10)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!_jsInitialized)
        {
            Debug.LogWarning("[FirebaseManager] JS not initialized.");
            callback?.Invoke(null);
            return;
        }
        _pendingCallback = callback;
        FirebaseBridge_LoadLeaderboard(limit);

#elif !UNITY_WEBGL || UNITY_EDITOR
        if (!_isInitialized)
        {
            Debug.LogWarning("[FirebaseManager] Firebase not initialized.");
            callback?.Invoke(null);
            return;
        }

        var query = _db.Collection("leaderboard").OrderByDescending("score").Limit(limit);
        query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"[FirebaseManager] Load failed: {task.Exception}");
                callback?.Invoke(null);
                return;
            }

            var entries = new List<LeaderboardEntry>();
            foreach (var doc in task.Result.Documents)
            {
                var d = doc.ToDictionary();
                entries.Add(new LeaderboardEntry
                {
                    nickname = d.ContainsKey("nickname") ? d["nickname"].ToString() : "Unknown",
                    score = d.ContainsKey("score") ? Convert.ToInt32(d["score"]) : 0,
                    combo = d.ContainsKey("combo") ? Convert.ToInt32(d["combo"]) : 0,
                    timestamp = d.ContainsKey("timestamp") ? Convert.ToInt64(d["timestamp"]) : 0
                });
            }

            Debug.Log($"[FirebaseManager] Loaded {entries.Count} entries");
            callback?.Invoke(entries);
        });
#endif
    }

    // ─── Non-WebGL 초기화 ───

#if !UNITY_WEBGL || UNITY_EDITOR
    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                _db = FirebaseFirestore.DefaultInstance;
                _auth = FirebaseAuth.DefaultInstance;
                Debug.Log("[FirebaseManager] Firebase initialized!");
                SignInAnonymously();
            }
            else
            {
                Debug.LogError($"[FirebaseManager] Dependency error: {task.Result}");
            }
        });
    }

    private void SignInAnonymously()
    {
        _auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("[FirebaseManager] Auth failed: " + task.Exception);
                return;
            }
            _userId = task.Result.User.UserId;
            _isInitialized = true;
            Debug.Log($"[FirebaseManager] Signed in: {_userId}");
        });
    }
#endif

    // ─── WebGL JSON 파싱용 ───

    [Serializable]
    private class JsEntryArray
    {
        public JsEntry[] items;
    }

    [Serializable]
    private class JsEntry
    {
        public string nickname;
        public int score;
        public int combo;
        public long timestamp;
    }
}

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

// Firebase SDK 임포트
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

/// <summary>
/// Firebase 연동 관리자 (Firestore 사용)
/// WebGL 빌드용 - 온라인 랭킹 시스템
/// </summary>
public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    private bool _isInitialized = false;
    private string _userId = null;
    private FirebaseFirestore _db;
    private FirebaseAuth _auth;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // WebGL 빌드용 - Firebase SDK 초기화 스킵
        Debug.Log("[FirebaseManager] WebGL build - using local leaderboard only");
        // InitializeFirebase(); // 주석 처리
    }

    /// <summary>
    /// Firebase 초기화 (익명 인증 포함)
    /// </summary>
    private void InitializeFirebase()
    {
        Debug.Log("[FirebaseManager] Initializing Firebase...");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                _db = FirebaseFirestore.DefaultInstance;
                _auth = FirebaseAuth.DefaultInstance;
                
                Debug.Log("[FirebaseManager] Firebase initialized successfully!");
                
                // 익명 로그인
                SignInAnonymously();
            }
            else
            {
                Debug.LogError($"[FirebaseManager] Could not resolve Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    /// <summary>
    /// 익명 로그인
    /// </summary>
    private void SignInAnonymously()
    {
        _auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("[FirebaseManager] SignInAnonymously was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("[FirebaseManager] SignInAnonymously encountered an error: " + task.Exception);
                return;
            }

            FirebaseUser newUser = task.Result.User;
            _userId = newUser.UserId;
            _isInitialized = true;
            Debug.Log($"[FirebaseManager] User signed in successfully: {_userId}");
        });
    }

    /// <summary>
    /// 점수를 랭킹에 제출
    /// </summary>
    public void SubmitScore(string nickname, int score, int combo)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[FirebaseManager] Firebase not initialized. Score not submitted.");
            return;
        }

        var entry = new LeaderboardEntry(nickname, score, combo);
        
        var collection = _db.Collection("leaderboard");
        var docData = new Dictionary<string, object>
        {
            { "nickname", entry.nickname },
            { "score", entry.score },
            { "combo", entry.combo },
            { "timestamp", entry.timestamp },
            { "userId", _userId }
        };

        collection.AddAsync(docData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"[FirebaseManager] Score submitted to Firebase: {nickname} - {score}");
            }
            else
            {
                Debug.LogError($"[FirebaseManager] Failed to submit score: {task.Exception}");
            }
        });
    }

    /// <summary>
    /// 상위 N개의 랭킹 불러오기
    /// </summary>
    public void LoadLeaderboard(Action<List<LeaderboardEntry>> callback, int limit = 10)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[FirebaseManager] Firebase not initialized.");
            callback?.Invoke(null);
            return;
        }

        var collection = _db.Collection("leaderboard");
        var query = collection.OrderByDescending("score").Limit(limit);

        query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"[FirebaseManager] Failed to load leaderboard: {task.Exception}");
                callback?.Invoke(null);
                return;
            }

            var entries = new List<LeaderboardEntry>();
            foreach (var document in task.Result.Documents)
            {
                var data = document.ToDictionary();
                var entry = new LeaderboardEntry
                {
                    nickname = data.ContainsKey("nickname") ? data["nickname"].ToString() : "Unknown",
                    score = data.ContainsKey("score") ? Convert.ToInt32(data["score"]) : 0,
                    combo = data.ContainsKey("combo") ? Convert.ToInt32(data["combo"]) : 0,
                    timestamp = data.ContainsKey("timestamp") ? Convert.ToInt64(data["timestamp"]) : 0
                };
                entries.Add(entry);
            }

            Debug.Log($"[FirebaseManager] Loaded {entries.Count} leaderboard entries from Firebase");
            callback?.Invoke(entries);
        });
    }
}

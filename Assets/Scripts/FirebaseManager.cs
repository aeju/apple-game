using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Firebase 연동 관리자 (Firestore 사용)
/// 
/// 설치 방법:
/// 1. Firebase Console에서 프로젝트 생성
/// 2. Unity 앱 등록 및 google-services.json 다운로드
/// 3. Unity Firebase SDK 설치 (Package Manager 또는 .unitypackage)
/// 4. FirebaseFirestore, FirebaseAuth 패키지 임포트
/// 5. 이 스크립트의 주석을 해제하고 사용
/// 
/// 현재는 Firebase SDK가 없어도 컴파일 에러가 나지 않도록 
/// 주석 처리된 상태로 제공됩니다.
/// </summary>
public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    private bool _isInitialized = false;
    private string _userId = null;

    // Firebase SDK 설치 후 아래 주석을 해제하세요
    /*
    private Firebase.Firestore.FirebaseFirestore _db;
    private Firebase.Auth.FirebaseAuth _auth;
    */

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFirebase();
    }

    /// <summary>
    /// Firebase 초기화 (익명 인증 포함)
    /// </summary>
    private void InitializeFirebase()
    {
        Debug.Log("[FirebaseManager] Initializing Firebase...");

        // Firebase SDK 설치 후 아래 주석을 해제하세요
        /*
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                _db = Firebase.Firestore.FirebaseFirestore.DefaultInstance;
                _auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
                
                // 익명 로그인
                SignInAnonymously();
            }
            else
            {
                Debug.LogError($"[FirebaseManager] Could not resolve Firebase dependencies: {dependencyStatus}");
            }
        });
        */

        // 임시: Firebase 없이도 작동하도록
        Debug.LogWarning("[FirebaseManager] Firebase SDK not installed. Using placeholder mode.");
        _isInitialized = false;
    }

    /// <summary>
    /// 익명 로그인
    /// </summary>
    private void SignInAnonymously()
    {
        // Firebase SDK 설치 후 아래 주석을 해제하세요
        /*
        _auth.SignInAnonymouslyAsync().ContinueWith(task => {
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

            Firebase.Auth.FirebaseUser newUser = task.Result;
            _userId = newUser.UserId;
            _isInitialized = true;
            Debug.Log($"[FirebaseManager] User signed in successfully: {_userId}");
        });
        */
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

        // Firebase SDK 설치 후 아래 주석을 해제하세요
        /*
        var collection = _db.Collection("leaderboard");
        var docData = new Dictionary<string, object>
        {
            { "nickname", entry.nickname },
            { "score", entry.score },
            { "combo", entry.combo },
            { "timestamp", entry.timestamp },
            { "userId", _userId }
        };

        collection.AddAsync(docData).ContinueWith(task => {
            if (task.IsCompleted)
            {
                Debug.Log($"[FirebaseManager] Score submitted: {nickname} - {score}");
            }
            else
            {
                Debug.LogError($"[FirebaseManager] Failed to submit score: {task.Exception}");
            }
        });
        */

        Debug.Log($"[FirebaseManager] (Placeholder) Score submitted: {nickname} - {score}");
    }

    /// <summary>
    /// 상위 N개의 랭킹 불러오기
    /// </summary>
    public void LoadLeaderboard(Action<List<LeaderboardEntry>> callback, int limit = 10)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[FirebaseManager] Firebase not initialized. Loading placeholder data.");
            callback?.Invoke(GetPlaceholderData());
            return;
        }

        // Firebase SDK 설치 후 아래 주석을 해제하세요
        /*
        var collection = _db.Collection("leaderboard");
        var query = collection.OrderByDescending("score").Limit(limit);

        query.GetSnapshotAsync().ContinueWith(task => {
            if (task.IsFaulted)
            {
                Debug.LogError($"[FirebaseManager] Failed to load leaderboard: {task.Exception}");
                callback?.Invoke(GetPlaceholderData());
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

            // 메인 스레드에서 콜백 실행
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                callback?.Invoke(entries);
            });
        });
        */

        // 임시 데이터
        callback?.Invoke(GetPlaceholderData());
    }

    /// <summary>
    /// 플레이스홀더 데이터 (Firebase 없을 때)
    /// </summary>
    private List<LeaderboardEntry> GetPlaceholderData()
    {
        return new List<LeaderboardEntry>
        {
            new LeaderboardEntry { nickname = "Cat_Master", score = 2500, combo = 50 },
            new LeaderboardEntry { nickname = "Paw_King", score = 2100, combo = 45 },
            new LeaderboardEntry { nickname = "Meow_Pro", score = 1850, combo = 40 },
            new LeaderboardEntry { nickname = "Nyan_Chan", score = 1650, combo = 38 },
            new LeaderboardEntry { nickname = "Purr_Lord", score = 1420, combo = 35 },
            new LeaderboardEntry { nickname = "Kitty_Ace", score = 1280, combo = 32 },
            new LeaderboardEntry { nickname = "Whisker_99", score = 1050, combo = 28 },
            new LeaderboardEntry { nickname = "Fluffy_Cat", score = 890, combo = 25 },
            new LeaderboardEntry { nickname = "Shadow_Paw", score = 720, combo = 22 },
            new LeaderboardEntry { nickname = "Tiger_Claw", score = 650, combo = 20 },
        };
    }
}

/// <summary>
/// 메인 스레드에서 코드를 실행하기 위한 헬퍼 (Firebase 콜백용)
/// Firebase SDK 설치 후 필요시 사용
/// </summary>
/*
public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher _instance;
    private Queue<Action> _executionQueue = new Queue<Action>();

    public static UnityMainThreadDispatcher Instance()
    {
        if (_instance == null)
        {
            var go = new GameObject("UnityMainThreadDispatcher");
            _instance = go.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }
        return _instance;
    }

    public void Enqueue(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }

    private void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }
}
*/

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 로컬 랭킹 관리 (PlayerPrefs 사용)
/// Firebase 없이도 작동하는 로컬 리더보드
/// </summary>
public class LocalLeaderboardManager : MonoBehaviour
{
    public static LocalLeaderboardManager Instance { get; private set; }

    private const string LEADERBOARD_KEY = "LocalLeaderboard";
    private const int MAX_ENTRIES = 100; // 최대 100개 저장 (상위 10개만 표시)

    private List<LeaderboardEntry> _leaderboard = new List<LeaderboardEntry>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        
        LoadLeaderboard();
    }

    /// <summary>
    /// 점수 제출
    /// </summary>
    public void SubmitScore(string nickname, int score, int combo)
    {
        var entry = new LeaderboardEntry(nickname, score, combo);
        _leaderboard.Add(entry);

        // 점수 순으로 정렬 (내림차순)
        _leaderboard = _leaderboard.OrderByDescending(e => e.score).ToList();

        // 최대 개수 유지
        if (_leaderboard.Count > MAX_ENTRIES)
        {
            _leaderboard = _leaderboard.Take(MAX_ENTRIES).ToList();
        }

        SaveLeaderboard();
        Debug.Log($"[LocalLeaderboard] Score submitted: {nickname} - {score} (Total entries: {_leaderboard.Count})");
    }

    /// <summary>
    /// 상위 N개 랭킹 가져오기
    /// </summary>
    public List<LeaderboardEntry> GetTopEntries(int count = 10)
    {
        return _leaderboard.Take(count).ToList();
    }

    /// <summary>
    /// 랭킹 저장
    /// </summary>
    private void SaveLeaderboard()
    {
        var wrapper = new LeaderboardWrapper { entries = _leaderboard };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(LEADERBOARD_KEY, json);
        PlayerPrefs.Save();
        Debug.Log($"[LocalLeaderboard] Leaderboard saved ({_leaderboard.Count} entries)");
    }

    /// <summary>
    /// 랭킹 불러오기
    /// </summary>
    private void LoadLeaderboard()
    {
        if (PlayerPrefs.HasKey(LEADERBOARD_KEY))
        {
            string json = PlayerPrefs.GetString(LEADERBOARD_KEY);
            var wrapper = JsonUtility.FromJson<LeaderboardWrapper>(json);
            _leaderboard = wrapper.entries ?? new List<LeaderboardEntry>();
            Debug.Log($"[LocalLeaderboard] Leaderboard loaded ({_leaderboard.Count} entries)");
        }
        else
        {
            Debug.Log("[LocalLeaderboard] No saved leaderboard found, starting fresh");
            _leaderboard = new List<LeaderboardEntry>();
        }
    }

    /// <summary>
    /// 랭킹 초기화 (개발/테스트용)
    /// </summary>
    public void ClearLeaderboard()
    {
        _leaderboard.Clear();
        PlayerPrefs.DeleteKey(LEADERBOARD_KEY);
        PlayerPrefs.Save();
        Debug.Log("[LocalLeaderboard] Leaderboard cleared");
    }

    /// <summary>
    /// 내 랭킹 위치 찾기
    /// </summary>
    public int GetMyRank(int score)
    {
        int rank = 1;
        foreach (var entry in _leaderboard)
        {
            if (score > entry.score)
                return rank;
            rank++;
        }
        return rank;
    }
}

/// <summary>
/// JSON 직렬화를 위한 래퍼 클래스
/// </summary>
[System.Serializable]
public class LeaderboardWrapper
{
    public List<LeaderboardEntry> entries;
}

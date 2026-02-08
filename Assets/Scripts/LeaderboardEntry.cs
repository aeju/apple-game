using System;

/// <summary>
/// 랭킹 엔트리 데이터 구조
/// </summary>
[Serializable]
public class LeaderboardEntry
{
    public string nickname;
    public int score;
    public int combo;
    public long timestamp; // Unix timestamp
    
    public LeaderboardEntry()
    {
    }
    
    public LeaderboardEntry(string nickname, int score, int combo)
    {
        this.nickname = nickname;
        this.score = score;
        this.combo = combo;
        this.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}

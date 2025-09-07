using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private static PlayerStats instance = null;
    public static PlayerStats Instance()
    {
        return instance;
    }

    public static string Name;
    public static int Score;
    public TMP_Text scoreText;

    // --- debounce so we don't POST too often ---
    [SerializeField] float minPushInterval = 0.25f; // seconds
    float _lastPushTime = -999f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AssignScoreText(TMP_Text tMP_Text)
    {
        scoreText = tMP_Text;
    }

    public void AddToScore(int addition)
    {
        Score += addition;
        if (scoreText != null)
        {
            scoreText.text = Score.ToString();
        }

        // Push to server, but not more often than minPushInterval
        if (Time.unscaledTime - _lastPushTime >= minPushInterval)
        {
            _lastPushTime = Time.unscaledTime;
            var client = LeaderboardClient.Instance ?? FindAnyObjectByType<LeaderboardClient>();
            client?.PushScoreNow(Score);
        }
    }

    void OnDestroy()
    {
        instance = null;
    }
}

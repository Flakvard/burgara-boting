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
    }

    void OnDestroy()
    {
        instance = null;
    }
}

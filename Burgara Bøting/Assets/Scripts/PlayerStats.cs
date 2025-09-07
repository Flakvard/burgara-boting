using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private static PlayerStats instance;
    public static PlayerStats Instance()
    {
        return instance;
    }

    public static string Name;
    public static int Score;
    public static TextMeshPro tmpScoreText;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void AddToScore(int addition)
    {
        Score += addition;
        tmpScoreText.text = Score.ToString();
    }

    public static void AssignScoreBoard(TextMeshPro textMeshPro)
    {
        tmpScoreText = textMeshPro;
    }

    void OnDestroy()
    {
        instance = null;
    }
}

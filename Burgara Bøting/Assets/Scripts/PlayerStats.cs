using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static string Name;
    public static int Score;
    public static TextMeshPro tmpScoreText;

    public static void AddToScore(int addition)
    {
        Score += addition;
        tmpScoreText.text = Score.ToString();
    }

    public static void AssignScoreBoard(TextMeshPro textMeshPro)
    {
        tmpScoreText = textMeshPro;
    }
}

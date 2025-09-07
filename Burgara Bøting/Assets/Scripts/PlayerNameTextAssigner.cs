using TMPro;
using UnityEngine;

public class PlayerNameTextAssigner : MonoBehaviour
{
    public TMP_Text NameText;
    public TMP_Text ScoreText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NameText.text = PlayerStats.Name;
        ScoreText.text = PlayerStats.Score.ToString();
        PlayerStats.Instance().AssignScoreText(ScoreText);
    }
}

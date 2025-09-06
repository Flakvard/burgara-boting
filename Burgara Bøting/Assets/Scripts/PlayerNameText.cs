using TMPro;
using UnityEngine;

public class PlayerNameText : MonoBehaviour
{
    public TextMeshProUGUI myDynamicText;
    public TextMeshProUGUI myDynamicStats;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myDynamicText.text = "Hello World";
        myDynamicStats.text = "0";
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerStats.Name is not null)
        {
            myDynamicText.text = PlayerStats.Name;
            myDynamicStats.text = PlayerStats.Score.ToString();
        }
        else
        {
            myDynamicText.text = "Hello World";
            myDynamicStats.text = "0";
        }
        
    }
}

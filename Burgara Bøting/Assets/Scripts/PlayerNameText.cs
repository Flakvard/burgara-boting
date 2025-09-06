using TMPro;
using UnityEngine;

public class PlayerNameText : MonoBehaviour
{
    public TextMeshProUGUI myDynamicText;
    public TextMeshProUGUI myDynamicStats;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (myDynamicStats is not null && myDynamicText is not null)
        {
            if (myDynamicText.text is not null)
                    myDynamicText.text = "Hello World";
            if(myDynamicStats.text is not null)
                myDynamicStats.text = "0";
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (myDynamicText is not null && myDynamicStats is not null)
        {
            if (PlayerStats.Name is not null)
                {
                    if (myDynamicText.text is not null)
                        myDynamicText.text = PlayerStats.Name;
                    if (myDynamicStats.text is not null)
                        myDynamicStats.text = PlayerStats.Score.ToString();
                }
                else
                {
                    if (myDynamicText.text is not null)
                        myDynamicText.text = "Hello World";
                    if (myDynamicStats.text is not null)
                        myDynamicStats.text = "0";
                }
            
        }
    }
}

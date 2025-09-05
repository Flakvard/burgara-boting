using System.Collections.Generic;
using UnityEngine;

public class NotaGenerator : MonoBehaviour
{
    public float width = 4f;
    public float height = 8f;

    public GameObject botNode;

    private List<List<GameObject>> botNodes2D = new List<List<GameObject>>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector2 centerPos = transform.position;
        int hIndex = 0;
        for(float h = 0; h <= height; h += 0.5f)
        {
            botNodes2D.Add(new List<GameObject>());
            float innerOffset = hIndex%2 == 0 ? 0.25f:0;
            for(float w = 0; w <= width - innerOffset; w += 0.5f)
            {
                GameObject newBotNode = Instantiate(botNode, centerPos + new Vector2(-(width/2)+w+innerOffset,-(height/2)+h), Quaternion.identity);
                botNodes2D[hIndex].Add(newBotNode);
            }
            hIndex++;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

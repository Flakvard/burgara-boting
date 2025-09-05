using UnityEngine;

public class NotaGenerator : MonoBehaviour
{
    public float width = 4f;
    public float height = 8f;

    public GameObject botNode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector2 centerPos = transform.position;

        for(float h = 0; h <= height; h += 0.5f)
        {
            for(float w = 0; w <= width; w += 0.5f)
            {

                // GameObject newBotNode = Instantiate(botNode, centerPos);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

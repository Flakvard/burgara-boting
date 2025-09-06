using System.Collections.Generic;
using UnityEngine;

public class NotaGenerator : MonoBehaviour
{
    public float width = 4f;
    public float height = 8f;

    public GameObject botNode;
    public GameObject botLine;

    private List<List<NotaNode>> botNodes2D = new List<List<NotaNode>>();

    public NotaShredder notaShredder;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector2 centerPos = transform.position;
        int hIndex = 0;

        // Create nodes
        for (float h = 0; h <= height; h += 0.5f)
        {
            botNodes2D.Add(new List<NotaNode>());
            float innerOffset = hIndex % 2 == 0 ? 0.25f : 0;
            for (float w = 0; w <= width - innerOffset; w += 0.5f)
            {
                GameObject newBotNode = Instantiate(botNode, centerPos + new Vector2(-(width / 2) + w + innerOffset, -(height / 2) + h), Quaternion.identity);
                NotaNode notaNode = newBotNode.GetComponent<NotaNode>();
                botNodes2D[hIndex].Add(notaNode);
            }
            hIndex++;
        }

        // Create lines between
        for (int y = 0; y < botNodes2D.Count - 1; y++)
        {
            for (int x = 0; x < botNodes2D[y].Count; x++)
            {
                if (y % 2 == 0)
                {
                    ConnectAWire(y, x, y + 1, x);
                    ConnectAWire(y, x, y + 1, x + 1);
                }
                else
                {
                    //index 0 only connect to first
                    if (x == 0)
                    {
                        ConnectAWire(y, x, y + 1, x);
                    }
                    //index last only connect to last
                    else if (x == botNodes2D[y].Count - 1)
                    {
                        ConnectAWire(y, x, y + 1, x - 1);
                    }
                    else
                    {
                        ConnectAWire(y, x, y + 1, x - 1);
                        ConnectAWire(y, x, y + 1, x);
                    }
                }
            }
        }

        notaShredder.Shred();
    }

    private void ConnectAWire(int y1, int x1, int y2, int x2)
    {
        GameObject lineGO = Instantiate(botLine);
        NotaLine notaLine = lineGO.GetComponent<NotaLine>();
        notaLine.Connect(botNodes2D[y1][x1], botNodes2D[y2][x2]);
    }
}

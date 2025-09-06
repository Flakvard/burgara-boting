using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NotaNode : MonoBehaviour
{
    private List<NotaLine> notaLines = new List<NotaLine>();

    public void ConnectLine(NotaLine notaLine)
    {
        notaLines.Add(notaLine);
    }

    public void ShredLines()
    {
        // foreach (NotaLine notaLine in notaLines)
        // {
        //     notaLine.Shred();
        // }

        while (notaLines.Count > 0)
        {
            notaLines[0].Shred();
        }
        // notaLines.Clear();
    }

    public bool RemoveLine(NotaLine notaLine)
    {
        return notaLines.Remove(notaLine);
    }

    public int GetNotaLinesAmount()
    {
        return notaLines.Count;
    }

    public bool HasConnectedWithNode(NotaNode otherNode)
    {
        foreach (NotaLine notaLine in notaLines)
        {
            if (notaLine.GetNotaNodesConnected().Contains(otherNode))
            {
                return true;
            }
        }
        return false;
    }
}

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

    public int ShredLines()
    {
        int shredAmount = 0;

        while (notaLines.Count > 0)
        {
            notaLines[0].Shred();
            shredAmount++;
        }

        return shredAmount;
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

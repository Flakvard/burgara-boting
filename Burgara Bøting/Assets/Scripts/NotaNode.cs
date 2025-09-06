using System.Collections.Generic;
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
        foreach (NotaLine notaLine in notaLines)
        {
            Destroy(notaLine.gameObject);
        }
        notaLines.Clear();
    }
}

using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class NotaLine : MonoBehaviour
{

    [SerializeField]
    NotaNode notaNode1, notaNode2;

    public void Connect(NotaNode notaNode1, NotaNode notaNode2)
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();

        this.notaNode1 = notaNode1;
        this.notaNode2 = notaNode2;

        notaNode1.ConnectLine(this);
        notaNode2.ConnectLine(this);

        lineRenderer.SetPositions(new Vector3[] {
            notaNode1.transform.position,
            notaNode2.transform.position
        });
    }

    public void Shred()
    {
        if (!notaNode1.RemoveLine(this))
            Debug.LogError("Remove line 1 unsuccessful");
        if (!notaNode2.RemoveLine(this))
            Debug.LogError("Remove line 2 unsuccessful");

        Destroy(gameObject);
    }

    public NotaNode[] GetNotaNodesConnected()
    {
        return new NotaNode[] { notaNode1, notaNode2 };
    }
}

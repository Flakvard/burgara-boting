using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(LineRenderer))]
public class GameDragInteraction : MonoBehaviour
{
    private Camera mainCamera;
    private bool isDragging = false;
    private List<NotaNode> touchedNotaNodes = new List<NotaNode>();
    private int sinceLastSafeNotaNode = 0;
    // private LineRenderer DragLineRenderer => GetComponent<LineRenderer>();
    private LineRenderer myLineRenderer;
    public GameObject notaLineGO;

    void Awake()
    {
        mainCamera = Camera.main;
        myLineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        // Mouse or Touch position
        Vector2 pointerPos = GetPointerWorldPosition();

        // Begin drag
        if (Mouse.current.leftButton.wasPressedThisFrame || Touchscreen.current?.primaryTouch.press.wasPressedThisFrame == true)
        {
            isDragging = true;
        }

        // Dragging
        if (isDragging)
        {
            if (touchedNotaNodes.Count > 0)
            {
                myLineRenderer.SetPosition(myLineRenderer.positionCount - 1, pointerPos);
            }

            Collider2D hit = Physics2D.OverlapPoint(pointerPos);
            if (hit != null)
            {
                // Debug.Log("Dragged over: " + hit.name);
                Debug.Log($"CUR Count: {touchedNotaNodes.Count}");

                // var shredder = hit.GetComponent<NotaNode>();
                // if (shredder != null)
                // {
                //     shredder.ShredLines();
                // }

                NotaNode currentNotaNode = hit.GetComponent<NotaNode>();

                print("FEED");
                if (currentNotaNode != null)
                {
                    print("FEED INSIDE");
                    int touchedNotaNodesAmount = touchedNotaNodes.Count;
                    if (touchedNotaNodesAmount == 0 && currentNotaNode.GetNotaLinesAmount() >= 2)
                    {
                        // Player is able to begin here...
                        touchedNotaNodes.Add(currentNotaNode);
                        myLineRenderer.positionCount = 2;
                        myLineRenderer.SetPositions(new Vector3[] { currentNotaNode.transform.position, pointerPos });
                    }

                    if (touchedNotaNodesAmount > 0 && currentNotaNode != touchedNotaNodes[touchedNotaNodesAmount - 1])
                    {
                        // we have entered the next node!!

                        NotaNode lastNotaNode = touchedNotaNodes[touchedNotaNodesAmount - 1];

                        // if we are not bøting horizontal
                        // And the distance is within the neighbours
                        if (Mathf.Abs(lastNotaNode.transform.position.y - currentNotaNode.transform.position.y) > 0.01f &&
                         Vector2.Distance(lastNotaNode.transform.position, currentNotaNode.transform.position) <= 0.8f &&
                         !currentNotaNode.HasConnectedWithNode(lastNotaNode)

                        //   &&
                        //  touchedNotaNodes.Select((touchedNotaNode, index) =>
                        //  {
                        //      return touchedNotaNode == currentNotaNode;
                        //  }).ToArray().Length == 0
                        )
                        {
                            if (sinceLastSafeNotaNode == 0)
                            {
                                TempConnectNodes(currentNotaNode, pointerPos);

                                sinceLastSafeNotaNode++;
                            }
                            else
                            {
                                if (currentNotaNode.GetNotaLinesAmount() >= 2)
                                {
                                    TempConnectNodes(currentNotaNode, pointerPos);

                                    sinceLastSafeNotaNode = 0;
                                }
                            }
                        }
                    }
                }
            }
        }

        // End drag
        if (Mouse.current.leftButton.wasReleasedThisFrame || Touchscreen.current?.primaryTouch.press.wasReleasedThisFrame == true)
        {
            // Make the new lines that has been bøtt
            for (int i = 1; i < touchedNotaNodes.Count - sinceLastSafeNotaNode; i++)
            {
                GameObject notaLineInstance = Instantiate(notaLineGO);
                NotaLine notaLine = notaLineInstance.GetComponent<NotaLine>();
                if (notaLine != null)
                {
                    notaLine.Connect(touchedNotaNodes[i - 1], touchedNotaNodes[i]);
                }
            }

            isDragging = false;
            sinceLastSafeNotaNode = 0;
            touchedNotaNodes.Clear();
            // myLineRenderer.SetPositions(new Vector3[] { });
            myLineRenderer.positionCount = 0;
        }
    }

    private Vector2 GetPointerWorldPosition()
    {
        Vector2 screenPos;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else
        {
            screenPos = Mouse.current.position.ReadValue();
        }

        return mainCamera.ScreenToWorldPoint(screenPos);
    }

    private void TempConnectNodes(NotaNode currentNotaNode, Vector3 pointerPos)
    {
        touchedNotaNodes.Add(currentNotaNode);
        int touchedNotaNodesAmount = touchedNotaNodes.Count;

        myLineRenderer.SetPosition(touchedNotaNodesAmount, currentNotaNode.transform.position);

        int linesAmount = myLineRenderer.positionCount;
        Vector3[] linePositions = new Vector3[linesAmount];
        myLineRenderer.GetPositions(linePositions);

        myLineRenderer.positionCount = linePositions.Length + 1;
        myLineRenderer.SetPositions(linePositions.Concat(new Vector3[] { pointerPos }).ToArray());
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

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
                NotaNode currentNotaNode = hit.GetComponent<NotaNode>();

                if (currentNotaNode != null)
                {
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
                        &&
                        !DoesNewSegmentIntersectExistingOnes(lastNotaNode.transform.position, currentNotaNode.transform.position)

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
            int scoreToAdd = 0;

            // Make the new lines that has been bøtt
            for (int i = 1; i < touchedNotaNodes.Count - sinceLastSafeNotaNode; i++)
            {
                GameObject notaLineInstance = Instantiate(notaLineGO);
                NotaLine notaLine = notaLineInstance.GetComponent<NotaLine>();
                if (notaLine != null)
                {
                    notaLine.Connect(touchedNotaNodes[i - 1], touchedNotaNodes[i]);
                    scoreToAdd += 10;
                }
            }

            isDragging = false;
            sinceLastSafeNotaNode = 0;
            touchedNotaNodes.Clear();
            // myLineRenderer.SetPositions(new Vector3[] { });
            myLineRenderer.positionCount = 0;
            PlayerStats.AddToScore(scoreToAdd);
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

    private bool DoesNewSegmentIntersectExistingOnes(Vector2 newStart, Vector2 newEnd)
    {
        // Loop through all segments already created in touchedNotaNodes
        for (int i = 0; i < touchedNotaNodes.Count - 2; i++)
        {
            Vector2 a = touchedNotaNodes[i].transform.position;
            Vector2 b = touchedNotaNodes[i + 1].transform.position;

            if (DoLinesIntersect(a, b, newStart, newEnd))
            {
                return true;
            }
        }

        return false;
    }

    // Helper to check line intersection between (p1-p2) and (q1-q2)
    private bool DoLinesIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        float o1 = Orientation(p1, p2, q1);
        float o2 = Orientation(p1, p2, q2);
        float o3 = Orientation(q1, q2, p1);
        float o4 = Orientation(q1, q2, p2);

        if (o1 != o2 && o3 != o4)
            return true;

        return false;
    }

    private float Orientation(Vector2 a, Vector2 b, Vector2 c)
    {
        float val = (b.y - a.y) * (c.x - b.x) - (b.x - a.x) * (c.y - b.y);
        if (Mathf.Abs(val) < Mathf.Epsilon) return 0f; // Colinear
        return (val > 0f) ? 1f : 2f; // Clockwise or counterclockwise
    }

}

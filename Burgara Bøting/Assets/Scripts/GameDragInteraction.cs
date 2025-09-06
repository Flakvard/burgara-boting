using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GameDragInteraction : MonoBehaviour
{
    private Camera mainCamera;
    private HashSet<Collider2D> touchedColliders = new HashSet<Collider2D>();

    private bool isDragging = false;

    void Awake()
    {
        mainCamera = Camera.main;
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
            Collider2D hit = Physics2D.OverlapPoint(pointerPos);
            if (hit != null && !touchedColliders.Contains(hit))
            {
                touchedColliders.Add(hit);
                Debug.Log("Dragged over: " + hit.name);

                var shredder = hit.GetComponent<NotaNode>();
                if (shredder != null)
                {
                    shredder.ShredLines();
                }
            }
        }

        // End drag
        if (Mouse.current.leftButton.wasReleasedThisFrame || Touchscreen.current?.primaryTouch.press.wasReleasedThisFrame == true)
        {
            isDragging = false;
            touchedColliders.Clear();
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
}

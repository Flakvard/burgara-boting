using System.Collections.Generic;
using System.Collections;
using UnityEngine;

// [RequireComponent(typeof(Collider2D))]
public class NotaShredder : MonoBehaviour
{
    public BotingTracker botingTracker;

    public void Shred()
    {
        botingTracker.ResetInfo();
        // Collider2D collider2D = GetComponent<Collider2D>();
        Collider2D[] colliders2D = GetComponentsInChildren<Collider2D>();
        Collider2D collider2D = colliders2D[Random.Range(0, colliders2D.Length)];
        collider2D.enabled = true;

        // Create a contact filter that matches everything
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true; // or false, depending on whether you care about triggers
        filter.SetLayerMask(Physics2D.DefaultRaycastLayers); // or customize the layer mask
        filter.useLayerMask = true;

        // List to store results
        List<Collider2D> results = new List<Collider2D>();

        // Find overlapping colliders
        collider2D.Overlap(filter, results);

        int linesShrededAmount = 0;

        // Loop through and do something
        foreach (Collider2D col in results)
        {
            // Debug.Log("Overlapping: " + col.name);

            // Example: destroy the overlapping GameObject
            // Destroy(col.gameObject);
            NotaNode notaNode = col.GetComponent<NotaNode>();
            if (notaNode != null)
            {
                linesShrededAmount += notaNode.ShredLines();
            }
        }

        botingTracker.SetWireAmountMissing(linesShrededAmount);

        collider2D.gameObject.SetActive(false);
    }

    public void WaitThenShred(float seconds)
    {
        StartCoroutine(WaitShred(seconds));
    }

    IEnumerator WaitShred(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Shred();
    }
}

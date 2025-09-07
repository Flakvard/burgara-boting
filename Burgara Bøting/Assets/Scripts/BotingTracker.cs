using UnityEngine;
using UnityEngine.Events;

public class BotingTracker : MonoBehaviour
{
    private int wireAmountMissing;
    private int currentWireFixed;

    public UnityEvent onTrolBott;

    public void SetWireAmountMissing(int amount)
    {
        wireAmountMissing = amount;
    }

    public void AddWiresFixed(int addition)
    {
        currentWireFixed += addition;

        if (currentWireFixed >= wireAmountMissing)
        {
            onTrolBott.Invoke();
        }
    }

    public int GetPercentComplete()
    {
        return (int)((float)currentWireFixed / wireAmountMissing * 100f);
    }

    public void ResetInfo()
    {
        wireAmountMissing = 0;
        currentWireFixed = 0;
    }
}

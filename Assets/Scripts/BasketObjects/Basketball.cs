using UnityEngine;

public class Basketball : MonoBehaviour
{
    bool hasScored = false;

    public bool GetHasScored()
    {
        return hasScored;
    }

    public void SetHasScored(bool value)
    {
        hasScored = value;
    }
}

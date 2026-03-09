using TMPro;
using UnityEngine;

public class XRInputKeyboard : MonoBehaviour
{
    public TMP_InputField input;

    public void OnSelect()
    {
        TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
    }
}
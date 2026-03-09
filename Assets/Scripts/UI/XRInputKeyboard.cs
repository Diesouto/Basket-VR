using UnityEngine;
using TMPro;

public class QuestKeyboardHandler : MonoBehaviour
{
    public TMP_InputField inputField;

#if UNITY_ANDROID && !UNITY_EDITOR
    private TouchScreenKeyboard keyboard;
#endif

    private void Awake()
    {
        if (inputField == null)
            inputField = GetComponent<TMP_InputField>();

        // Show keyboard when field is selected
        inputField.onSelect.AddListener(OnInputSelected);
    }

    private void OnDestroy()
    {
        inputField.onSelect.RemoveListener(OnInputSelected);
    }

    private void OnInputSelected(string _)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Open Meta Quest system keyboard
        keyboard = TouchScreenKeyboard.Open(
            inputField.text,
            TouchScreenKeyboardType.Default,
            false, // autocorrection
            inputField.multiLine, // allow multiline if needed
            false, // secure
            false  // alert
        );
#endif
    }

    private void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (keyboard != null && keyboard.active)
        {
            inputField.text = keyboard.text;
        }
#endif
    }
}

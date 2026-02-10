using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimations : MonoBehaviour
{
    [SerializeField] private InputActionProperty gripAction;
    [SerializeField] private InputActionProperty triggerAction;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        float gripValue = gripAction.action.ReadValue<float>();
        float triggerValue = triggerAction.action.ReadValue<float>();
        animator.SetFloat("Grip", gripValue);
        animator.SetFloat("Trigger", triggerValue);
    }
}

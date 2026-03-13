using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class RayBehaviour : MonoBehaviour
{
    [SerializeField] private XRRayInteractor rayInteractor;
    [SerializeField] private InputActionProperty tpAction;

    private void Start()
    {
        rayInteractor.gameObject.SetActive(false);
        tpAction.action.performed += ActivateRay;
    }

    private void ActivateRay(InputAction.CallbackContext obj)
    {
        rayInteractor.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (tpAction.action.WasReleasedThisFrame())
        {
            rayInteractor.gameObject.SetActive(false);
        }
    }
}

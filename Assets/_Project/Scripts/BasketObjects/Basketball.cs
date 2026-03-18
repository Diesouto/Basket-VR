using Unity.Netcode;
using UnityEngine;

public class Basketball : NetworkBehaviour
{
    [SerializeField] float timeToDespawn = 6f;

    public NetworkVariable<bool> hasScored = new NetworkVariable<bool>();
    BasketballCart ownerCart;

    void OnEnable()
    {
        Invoke(nameof(ReturnToPool), timeToDespawn);
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    public void Initialize(BasketballCart cart)
    {
        ownerCart = cart;
        hasScored.Value = false;
    }

    public BasketballCart GetOwnerCart()
    {
        return ownerCart;
    }

    public bool GetHasScored()
    {
        return hasScored.Value;
    }

    public void SetHasScored(bool value)
    {
        hasScored.Value = value;
    }

    public void ResetBall()
    {
        hasScored.Value = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void ReturnToPool()
    {
        if (ownerCart == null)
        {
            Debug.LogWarning("Ball has no owner cart.");
            return;
        }

        ownerCart.ReturnBall(this);
    }
}
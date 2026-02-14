using UnityEngine;

public class Basketball : MonoBehaviour
{
    [SerializeField] float timeToDespawn = 6f;

    bool hasScored = false;
    IBasketballOwner owner;

    void OnEnable()
    {
        Invoke(nameof(ReturnToPool), timeToDespawn);
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    public void Initialize(IBasketballOwner ownerCart)
    {
        owner = ownerCart;
    }

    public bool GetHasScored()
    {
        return hasScored;
    }

    public void SetHasScored(bool value)
    {
        hasScored = value;
    }

    public void ResetBall()
    {
        hasScored = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void ReturnToPool()
    {
        if (owner == null)
        {
            Debug.LogWarning("Ball has no cart/owner reference.");
            return;
        }

        owner.ReturnBall(this);
    }
}

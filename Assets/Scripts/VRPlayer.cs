using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class VRPlayer : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 velocity;

    public float gravity = -9.81f;
    public float groundStickForce = -2f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleGravity();
    }

    void HandleGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            // Small downward force keeps player snapped to ground
            velocity.y = groundStickForce;
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
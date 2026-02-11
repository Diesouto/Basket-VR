using Unity.Netcode;
using UnityEngine;

public class NETThrow : NetworkBehaviour
{
    [SerializeField] private GameObject tomatoPrefab;
    [SerializeField] private float throwForce;

    void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.T)) {
            Throw_ServerRPC();
        }
    }

    [ServerRpc]
    void Throw_ServerRPC()
    {
        GameObject tomato = Instantiate(tomatoPrefab, transform.position + transform.forward * 2, Quaternion.identity);
        NetworkObject networkObject = tomato.GetComponent<NetworkObject>();
        networkObject.Spawn();

        Rigidbody rb = tomato.GetComponent<Rigidbody>();
        rb.AddForce(Vector3.up * throwForce);
        rb.AddForce(transform.forward * throwForce);
    }
}

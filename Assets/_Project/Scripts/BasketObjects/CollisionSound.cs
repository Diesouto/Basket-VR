using Unity.Netcode;
using UnityEngine;

public class CollisionSound : NetworkBehaviour
{
    [SerializeField] private AudioClip collisionSound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float minVelocityToPlay = 0.1f;

    private void OnCollisionEnter(Collision collision)
    {
        TryPlaySound(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryPlaySound(other.gameObject);
    }

    private void TryPlaySound(GameObject obj)
    {
        Basketball basketball = obj.GetComponent<Basketball>();
        if (basketball == null) return;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null || rb.linearVelocity.magnitude < minVelocityToPlay) return;

        // ✅ Play locally
        PlaySoundLocal();

        // ✅ Optional: Play for all clients
        if (IsServer)
        {
            PlaySoundClientRpc();
        }
        else if (IsOwner)
        {
            // Notify server to play for everyone
            PlaySoundServerRpc();
        }
    }

    private void PlaySoundLocal()
    {
        if (audioSource != null)
            audioSource.PlayOneShot(collisionSound);
        else
            AudioSource.PlayClipAtPoint(collisionSound, transform.position);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlaySoundServerRpc()
    {
        PlaySoundClientRpc();
    }

    [ClientRpc]
    private void PlaySoundClientRpc()
    {
        PlaySoundLocal();
    }
}
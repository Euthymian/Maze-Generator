using UnityEngine;

public class BallController : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float speed;

    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    public void Stop()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void ResetAt(Vector3 position)
    {
        Stop();
        rb.position = position;
    }

    public void Launch(float xDirection, float zDirection)
    {
        Vector3 direction = new Vector3(xDirection, 0f, zDirection).normalized;
        rb.linearVelocity = direction * speed;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Paddle"))
            return;

        float hitOffset =
            (transform.position.x - collision.transform.position.x)
            / collision.collider.bounds.extents.x;

        float xDirection = Mathf.Clamp(hitOffset, -1f, 1f) * 0.8f;
        float zDirection = transform.position.z < collision.transform.position.z
            ? -1f
            : 1f;

        Vector3 direction = new Vector3(xDirection, 0f, zDirection).normalized;
        rb.linearVelocity = direction * speed;
    }
}

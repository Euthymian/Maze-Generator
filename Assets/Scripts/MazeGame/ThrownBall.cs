using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public sealed class ThrownBall : MonoBehaviour
{
    [SerializeField] private AudioClip wallImpact;
    [SerializeField] private AudioClip floorImpact;
    [SerializeField] private AudioClip enemyImpact;
    [SerializeField, Min(0.1f)] private float lifetime = 8f;

    private MazeScore score;
    private bool spent;

    public void Launch(Vector3 velocity, Collider ownerCollider, MazeScore ownerScore)
    {
        score = ownerScore;
        if (ownerCollider != null)
            Physics.IgnoreCollision(GetComponent<SphereCollider>(), ownerCollider);

        GetComponent<Rigidbody>().linearVelocity = velocity;
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (spent || collision.contactCount == 0)
            return;

        ContactPoint contact = collision.GetContact(0);
        var enemy = collision.collider.GetComponentInParent<EnemyController>();
        if (enemy != null && enemy.TryHit())
        {
            spent = true;
            gameObject.SetActive(false);
            Destroy(gameObject);
            if (score != null)
                score.AddHit();
            AudioSource.PlayClipAtPoint(enemyImpact, contact.point);
            return;
        }

        // Ignore contact from a ball settling on the floor or rolling into another ball.
        if (enemy == null && collision.relativeVelocity.sqrMagnitude >= 0.64f &&
            !collision.collider.TryGetComponent<ThrownBall>(out _))
        {
            AudioClip impact = contact.normal.y > 0.5f ? floorImpact : wallImpact;
            AudioSource.PlayClipAtPoint(impact, contact.point, 0.65f);
        }
    }
}

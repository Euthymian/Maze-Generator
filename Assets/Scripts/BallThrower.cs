using UnityEngine;

public sealed class BallThrower : MonoBehaviour
{
    [SerializeField] private ThrownBall ballPrefab;
    [SerializeField, Min(0f)] private float throwSpeed = 12f;

    private Collider ownerCollider;
    private MazeScore score;

    private void Awake()
    {
        ownerCollider = GetComponentInParent<Collider>();
        score = FindFirstObjectByType<MazeScore>();
    }

    public void ThrowBall()
    {
        ThrownBall ball = Instantiate(ballPrefab, transform.position, transform.rotation);
        ball.Launch(transform.forward * throwSpeed, ownerCollider, score);
    }
}

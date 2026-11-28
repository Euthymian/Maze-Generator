using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private int playerWhoScores = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BallController>() != null)
            gameManager.ScorePoint(playerWhoScores);
    }

}

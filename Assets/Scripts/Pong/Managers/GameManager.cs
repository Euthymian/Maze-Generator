using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int winningScore = 5;
    [SerializeField] private float pauseBetweenPoints = 1.5f;
    [SerializeField] private float serveDelay = 1.5f;
    [SerializeField] private float minPaddleX = -5f;
    [SerializeField] private float maxPaddleX = 5f;
    [SerializeField, Range(0f, 1f)] private float maxHorizontalBias = 1.0f;
    
    [SerializeField] private BallController ball;
    [SerializeField] private Transform player1ServePoint;
    [SerializeField] private Transform player2ServePoint;
    [SerializeField] private Transform player1Paddle;
    [SerializeField] private Transform player2Paddle;
    
    [SerializeField] private TMP_Text player1ScoreText;
    [SerializeField] private TMP_Text player2ScoreText;
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private const int player1 = 1;
    private const int player2 = 2;
    private Vector3 player1StartPosition;
    private Vector3 player2StartPosition;
    private int player1Score;
    private int player2Score;
    private bool gameOver;
    

    private void Start()
    {
        ConfigureOpponent();
        player1StartPosition = player1Paddle.position;
        player2StartPosition = player2Paddle.position;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        UpdateScoreText();

        if (winnerText != null)
            winnerText.gameObject.SetActive(false);
        
        var firstServer = Random.value < 0.5f ? player1 : player2;

        StartCoroutine(ServeFromPlayerSide(firstServer));
    }
    
    private void ConfigureOpponent()
    {
        Player2Controller humanController =
            player2Paddle.GetComponent<Player2Controller>();

        Player2AI aiController =
            player2Paddle.GetComponent<Player2AI>();

        if (humanController != null)
            humanController.enabled = !GameModeSettings.VsComputer;

        if (aiController != null)
            aiController.enabled = GameModeSettings.VsComputer;
    }

    public void ScorePoint(int playerNum)
    {
        if (gameOver) return;
        
        ball.Stop();
        
        switch (playerNum)
        {
            case player1:
                player1Score++;
                break;
            case player2:
                player2Score++;
                break;
            default:
                Debug.Log("Player num issue\nnumber:  " + playerNum);
                throw new ArgumentOutOfRangeException(nameof(playerNum), "PlayerNum can only be 1 or 2");
        }

        UpdateScoreText();
        CheckGameOver(playerNum);
        
        if (gameOver) return;
        
        StartCoroutine(ServeAgain(playerNum));
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        StopAllCoroutines();
        
        gameOver = false;
        player1Score = 0;
        player2Score = 0;
        UpdateScoreText();
        
        if (winnerText != null)
        {
            winnerText.gameObject.SetActive(false);
        }
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        player1Paddle.position = player1StartPosition;
        player2Paddle.position = player2StartPosition;
        
        ball.Stop();
        
        int firstServer = Random.value < 0.5f ? player1 : player2;
        StartCoroutine(ServeFromPlayerSide(firstServer));
    }
    
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void CheckGameOver(int playerNum)
    {
        if (player1Score < winningScore && player2Score < winningScore) return;
        
        gameOver = true;

        if (winnerText != null)
        {
            winnerText.text = $"Player {playerNum} wins!";
            winnerText.gameObject.SetActive(true);
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }
    
    private IEnumerator ServeAgain(int playerWhoScored)
    {
        yield return new WaitForSeconds(pauseBetweenPoints);

        StartCoroutine(ServeFromPlayerSide(playerWhoScored));
    }


    private IEnumerator ServeFromPlayerSide(int playerNum)
    {
        Transform servePoint;
        Transform paddle;

        switch (playerNum)
        {
            case player1:
                servePoint = player1ServePoint;
                paddle = player1Paddle;
                break;
            case player2:
                servePoint = player2ServePoint;
                paddle  = player2Paddle;
                break;
            default:
                Debug.Log("Player num issue\nnumber:  " + playerNum);
                throw new ArgumentOutOfRangeException(nameof(playerNum), "PlayerNum can only be 1 or 2");
        }
        
        player1Paddle.position = player1StartPosition;
        player2Paddle.position = player2StartPosition;
        ball.ResetAt(servePoint.position);
        
        yield return new WaitForSeconds(serveDelay);
        
        float paddleSide = Mathf.InverseLerp(
            minPaddleX,
            maxPaddleX,
            paddle.position.x
        ) * 2f - 1f;

        // A paddle farther to one side sends the ball toward the other side.
        float xDirection = -paddleSide * maxHorizontalBias;
        float zDirection = playerNum == player1 ? 1f : -1f;

        
        ball.Launch(xDirection, zDirection);
    }
    
    private void UpdateScoreText()
    {
        player1ScoreText.text = player1Score.ToString();
        player2ScoreText.text = player2Score.ToString();
    }
}

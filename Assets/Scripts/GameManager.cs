using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scoring")]
    public int player1Score, player2Score;
    public int winScore = 5;

    [Header("UI")]
    public Color player1Color = Color.red;
    public Color player2Color = Color.blue;
    public float scorePopScale = 1.5f;
    public float scorePopDuration = 0.25f;

    [Header("References")]
    public TextMeshProUGUI scoreTextP1, scoreTextP2;
    public GameObject winPanel;
    public PuckController puck;
    public PlayerController player1;
    public PlayerController player2;
    public GameObject goalFlashEffectPrefab;
    [SerializeField] private Transform goal1Transform;
    [SerializeField] private Transform goal2Transform;
    [SerializeField] private UIManager uiManager;


    void Awake() => Instance = this;

    public void AddScore(int scoringPlayer)
    {
        TextMeshProUGUI targetScoreText;
        Transform goalTransform;

        if (scoringPlayer == 1)
        {
            player1Score++;
            targetScoreText = scoreTextP1;
            goalTransform = goal2Transform;
        }
        else // Player 2 scored
        {
            player2Score++;
            targetScoreText = scoreTextP2;
            goalTransform = goal1Transform;
        }

        UpdateScoreUI();

        // Start the pop animation
        if (targetScoreText != null)
        {
            StartCoroutine(ScorePopEffect(targetScoreText));
        }

        // Check for win condition
        if (player1Score >= winScore || player2Score >= winScore)
        {
            EndGame(scoringPlayer);
        }
        else
        {
            AudioManager.Instance.PlayGoal();
            
            // Spawn VFX on the correct goal
            if (goalFlashEffectPrefab != null && goalTransform != null)
            {
                // Ensure the effect always points inwards towards the table
                float yRotation = (goalTransform == goal1Transform) ? 0f : 180f;
                Quaternion effectRotation = Quaternion.Euler(0, yRotation, 0);
                Instantiate(goalFlashEffectPrefab, goalTransform.position, effectRotation);
            }

            StartCoroutine(ResetRound());
        }
    }

    private IEnumerator ScorePopEffect(TextMeshProUGUI scoreText)
    {
        Vector3 originalScale = scoreText.transform.localScale;
        Vector3 targetScale = originalScale * scorePopScale;
        float halfDuration = scorePopDuration / 2;
        float timer = 0f;

        // Scale up
        while (timer < halfDuration)
        {
            scoreText.transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / halfDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        // Scale down
        timer = 0f;
        while (timer < halfDuration)
        {
            scoreText.transform.localScale = Vector3.Lerp(targetScale, originalScale, timer / halfDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        scoreText.transform.localScale = originalScale;
    }

    private IEnumerator ResetRound()
    {
        Rigidbody puckRb = puck.GetComponent<Rigidbody>();
        Collider puckCollider = puck.GetComponent<Collider>();

        // Disable physics on the puck to prevent phantom collisions
        puckRb.isKinematic = true;
        puckCollider.enabled = false;

        // Wait one physics frame
        yield return new WaitForFixedUpdate();

        // Now reset positions while physics is off
        player1.ResetPosition();
        player2.ResetPosition();
        puck.transform.position = puck.StartPosition;


        // Re-enable the puck's physics
        puckCollider.enabled = true;
        puckRb.isKinematic = false;

        // Now that physics is on, safely reset velocities
        puckRb.linearVelocity = Vector3.zero;
        puckRb.angularVelocity = Vector3.zero;
    }

    void UpdateScoreUI()
    {
        scoreTextP1.text = player1Score.ToString();
        scoreTextP2.text = player2Score.ToString();
    }

    void EndGame(int winningPlayer)
    {
        uiManager.SetGameActive(false); // Tell the UI Manager the game is over
        AudioManager.Instance.PlayGameOver();
        
        // Spawn VFX on the correct goal for the final score
        Transform goalTransform = (winningPlayer == 1) ? goal2Transform : goal1Transform;
        if (goalFlashEffectPrefab != null && goalTransform != null)
        {
            // Ensure the effect always points inwards towards the table
            float yRotation = (goalTransform == goal1Transform) ? 0f : 180f;
            Quaternion effectRotation = Quaternion.Euler(0, yRotation, 0);
            Instantiate(goalFlashEffectPrefab, goalTransform.position, effectRotation);
        }

        Time.timeScale = 0;
        winPanel.SetActive(true);

        TextMeshProUGUI winText = winPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (winText != null)
        {
            if (winningPlayer == 1)
            {
                winText.text = "Red Wins!";
                winText.color = player1Color;
            }
            else
            {
                winText.text = "Blue Wins!";
                winText.color = player2Color;
            }
        }
    }
}

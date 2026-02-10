using UnityEngine;
using TMPro;
using System.Collections;

// This is the central "brain" of the game. It manages the game state, score,
// win conditions, and the main game loop between rounds.
// It uses a singleton pattern to be easily accessible by other scripts.
public class GameManager : MonoBehaviour
{
    // Static instance of the GameManager for easy access.
    public static GameManager Instance;

    [Header("Scoring")]
    public int player1Score;
    public int player2Score;
    public int winScore = 5;

    [Header("UI")]
    public Color player1Color = Color.red;
    public Color player2Color = Color.blue;
    public float scorePopScale = 1.5f;
    public float scorePopDuration = 0.25f;

    [Header("References")]
    public TextMeshProUGUI scoreTextP1;
    public TextMeshProUGUI scoreTextP2;
    public GameObject winPanel;
    public PuckController puck;
    public PlayerController player1;
    public PlayerController player2;
    public GameObject goalFlashEffectPrefab;
    [SerializeField] private Transform goal1Transform;
    [SerializeField] private Transform goal2Transform;
    [SerializeField] private UIManager uiManager;

    private bool isGameOver = false;


    void Awake()
    {
        Instance = this;
    }

    // This method is called by the PuckController's trigger when a goal is scored.
    public void AddScore(int scoringPlayer)
    {
        // Prevent extra scoring after the win condition is reached (VR keeps time running).
        if (isGameOver) return;

        TextMeshProUGUI targetScoreText;
        Transform goalTransform;

        // Determine which player scored and update their score.
        if (scoringPlayer == 1)
        {
            player1Score++;
            targetScoreText = scoreTextP1;
            goalTransform = goal2Transform; // Player 1 scores in Player 2's goal.
        }
        else // Player 2 scored
        {
            player2Score++;
            targetScoreText = scoreTextP2;
            goalTransform = goal1Transform; // Player 2 scores in Player 1's goal.
        }

        UpdateScoreUI();

        // Trigger the score pop animation.
        if (targetScoreText != null)
        {
            StartCoroutine(ScorePopEffect(targetScoreText));
        }

        // Check if the score reaches the win condition.
        if (player1Score >= winScore || player2Score >= winScore)
        {
            isGameOver = true;
            EndGame(scoringPlayer);
        }
        else
        {
            // If the game is not over, play the standard goal sound and reset for the next round.
            AudioManager.Instance.PlayGoal();
            
            // Play the goal flash effect.
            if (goalFlashEffectPrefab != null && goalTransform != null)
            {
                float yRotation = (goalTransform == goal1Transform) ? 0f : 180f;
                Quaternion effectRotation = Quaternion.Euler(0, yRotation, 0);
                Instantiate(goalFlashEffectPrefab, goalTransform.position, effectRotation);
            }

            StartCoroutine(ResetRound());
        }
    }
    
    // Coroutine to animate the scale of the score text for a "pop" effect.
    private IEnumerator ScorePopEffect(TextMeshProUGUI scoreText)
    {
        Vector3 originalScale = scoreText.transform.localScale;
        Vector3 targetScale = originalScale * scorePopScale;
        float halfDuration = scorePopDuration / 2;
        float timer = 0f;

        // Scale up over the first half of the duration.
        while (timer < halfDuration)
        {
            scoreText.transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / halfDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        // Scale back down over the second half.
        timer = 0f;
        while (timer < halfDuration)
        {
            scoreText.transform.localScale = Vector3.Lerp(targetScale, originalScale, timer / halfDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        scoreText.transform.localScale = originalScale;
    }

    // This coroutine resets the playfield after a goal is scored.
    private IEnumerator ResetRound()
    {
        Rigidbody puckRb = puck.GetComponent<Rigidbody>();
        Collider puckCollider = puck.GetComponent<Collider>();

        // We temporarily make the puck "kinematic" and disable its collider.
        // This is a robust way to prevent phantom collisions during the reset process.
        // a simple reset caused bugs (like the puck moving on respawn)
        puckRb.isKinematic = true;
        puckCollider.enabled = false;

        // Wait for the next physics update frame before moving everything.
        yield return new WaitForFixedUpdate();

        // Reset positions now that physics is temporarily paused for the puck.
        if (player1 != null) player1.ForceRelease();
        if (player2 != null) player2.ForceRelease();

        player1.ResetPosition();
        player2.ResetPosition();
        puck.transform.position = puck.StartPosition;

        // Re-enable the puck's physics components.
        puckCollider.enabled = true;
        puckRb.isKinematic = false;

        // Now that the puck is a dynamic object again, we can safely reset its velocity.
        puckRb.linearVelocity = Vector3.zero;
        puckRb.angularVelocity = Vector3.zero;
    }

    // Updates the text elements with the current scores.
    void UpdateScoreUI()
    {
        if(scoreTextP1 != null) scoreTextP1.text = player1Score.ToString();
        if(scoreTextP2 != null) scoreTextP2.text = player2Score.ToString();
    }

    // Handles the end-of-game sequence.
    void EndGame(int winningPlayer)
    {
        isGameOver = true;

        // Tell the UIManager that the game is no longer active, which disables pausing.
        uiManager.SetGameActive(false); 
        AudioManager.Instance.PlayGameOver();
        
        Transform goalTransform = (winningPlayer == 1) ? goal2Transform : goal1Transform;
        if (goalFlashEffectPrefab != null && goalTransform != null)
        {
            float yRotation = (goalTransform == goal1Transform) ? 0f : 180f;
            Quaternion effectRotation = Quaternion.Euler(0, yRotation, 0);
            Instantiate(goalFlashEffectPrefab, goalTransform.position, effectRotation);
        }

        // In VR, we keep timeScale at 1 so hands can still move to click "Restart".
        // Instead of pausing time, we stop the puck and prevent further goal triggers.
        Time.timeScale = 1;
        if (puck != null)
        {
            var puckRb = puck.GetComponent<Rigidbody>();
            var puckCollider = puck.GetComponent<Collider>();
            if (puckRb != null)
            {
                puckRb.linearVelocity = Vector3.zero;
                puckRb.angularVelocity = Vector3.zero;
                puckRb.isKinematic = true;
            }
            if (puckCollider != null)
            {
                puckCollider.enabled = false;
            }
        }
        winPanel.SetActive(true);

        // Update the win text to show the correct winner and color.
        // how we know who won, we passed in the winningPlayer int, which is either 1 or 2
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

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

    [Header("References")]
    public TextMeshProUGUI scoreTextP1, scoreTextP2;
    public GameObject winPanel;
    public PuckController puck;
    public PlayerController player1;
    public PlayerController player2;
    public GameObject goalFlashEffectPrefab;
    [SerializeField] private Transform goal1Transform;
    [SerializeField] private Transform goal2Transform;


    void Awake() => Instance = this;

    public void AddScore(int scoringPlayer)
    {
        if (scoringPlayer == 1)
        {
            player1Score++;
        }
        else // Player 2 scored
        {
            player2Score++;
        }

        UpdateScoreUI();

        // Check for win condition BEFORE playing any sounds
        if (player1Score >= winScore || player2Score >= winScore)
        {
            EndGame(scoringPlayer);
        }
        else
        {
            // If it's not a winning goal, play the normal goal sound and reset
            AudioManager.Instance.PlayGoal();
            
            // Spawn VFX on the correct goal
            Transform goalTransform = (scoringPlayer == 1) ? goal2Transform : goal1Transform;
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

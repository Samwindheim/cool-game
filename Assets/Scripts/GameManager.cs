using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scoring")]
    public int player1Score, player2Score;
    public int winScore = 5;

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
        AudioManager.Instance.PlayGoal();

        Transform goalTransform;

        if (scoringPlayer == 1)
        {
            player1Score++;
            goalTransform = goal2Transform; // Player 1 scores in Goal 2's area
        }
        else // Player 2 scored
        {
            player2Score++;
            goalTransform = goal1Transform; // Player 2 scores in Goal 1's area
        }

        // Spawn the goal flash effect
        if (goalFlashEffectPrefab != null && goalTransform != null)
        {
            GameObject effect = Instantiate(goalFlashEffectPrefab, goalTransform.position, goalTransform.rotation);
            Destroy(effect, 2f);
        }

        UpdateScoreUI();

        if (player1Score >= winScore || player2Score >= winScore)
            EndGame(scoringPlayer);
        else
        {
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
        Time.timeScale = 0;
        winPanel.SetActive(true);

        TextMeshProUGUI winText = winPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (winText != null)
        {
            winText.text = "Player " + winningPlayer + " Wins!";
        }
    }
}

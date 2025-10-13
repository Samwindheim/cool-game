using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int player1Score, player2Score;
    public TextMeshProUGUI scoreTextP1, scoreTextP2;
    public GameObject winPanel;
    public PuckController puck;
    public PlayerController player1;
    public PlayerController player2;
    public int winScore = 5;

    void Awake() => Instance = this;

    public void AddScore(int player)
    {
        AudioManager.Instance.PlayGoal();

        if (player == 1) player1Score++;
        else player2Score++;

        // UpdateScoreUI(); // Temporarily commented out

        if (player1Score >= winScore || player2Score >= winScore)
            EndGame();
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
        // scoreTextP1.text = player1Score.ToString();
        // scoreTextP2.text = player2Score.ToString();
    }

    void EndGame()
    {
        Time.timeScale = 0;
        winPanel.SetActive(true);
    }
}

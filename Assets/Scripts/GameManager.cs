using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int player1Score, player2Score;
    public TextMeshProUGUI scoreTextP1, scoreTextP2;
    public GameObject winPanel;
    public PuckController puck;
    public int winScore = 5;

    void Awake() => Instance = this;

    public void AddScore(int player)
    {
        if (player == 1) player1Score++;
        else player2Score++;

        UpdateScoreUI();

        if (player1Score >= winScore || player2Score >= winScore)
            EndGame();
        else
            puck.ResetPuck();
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

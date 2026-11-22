using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mostra o painel de Game Over e a pontuação final.
/// Não decide nada sobre o estado do jogo: avisa o GameManager por eventos.
/// Colocar num objeto fixo (ex.: "GameOverManager"), NÃO no próprio painel,
/// porque o painel é desativado.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [Header("Painel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text finalScoreText;

    [Header("Botões")]
    [SerializeField] private Button restartButton;
    [Tooltip("Opcional")]
    [SerializeField] private Button mainMenuButton;

    /// <summary>Disparado ao clicar em RESTART (o GameManager escuta).</summary>
    public event Action RestartRequested;

    /// <summary>Disparado ao clicar em MAIN MENU (o GameManager escuta).</summary>
    public event Action MainMenuRequested;

    private void Awake()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(() => RestartRequested?.Invoke());

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() => MainMenuRequested?.Invoke());

        Hide();
    }

    public void Show(int finalScore)
    {
        if (titleText != null)
            titleText.text = "GAME OVER";

        if (finalScoreText != null)
            finalScoreText.text = "PONTUAÇÃO FINAL: " + finalScore;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void Hide()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Affiche le panneau de fin de partie et le score final.
/// Ne décide pas de l'état du jeu : avertit le GameManager par des événements.
/// À placer sur un objet permanent (p. ex. « GameOverManager »), PAS sur le panneau,
/// car celui-ci est désactivé.
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

    /// <summary>Déclenché lors d'un clic sur RESTART (le GameManager s'y abonne).</summary>
    public event Action RestartRequested;

    /// <summary>Déclenché lors d'un clic sur MAIN MENU (le GameManager s'y abonne).</summary>
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

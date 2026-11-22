using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Controla os botões da UI com inputs definidos por si (Input System):
///  - Atalhos diretos: cada botão pode ter uma ação própria que o "clica".
///  - Navegação: ações Cima/Baixo mudam o botão selecionado e a ação Confirmar clica nele.
/// Só reage a botões visíveis (painel ativo) e interativos.
/// </summary>
public class MenuInputController : MonoBehaviour
{
    [Serializable]
    public class ButtonBinding
    {
        [Tooltip("Só para organização no Inspector")]
        public string name;

        [Tooltip("Opcional: atalho direto que clica neste botão")]
        public InputActionReference action;

        public Button button;
    }

    [Header("Botões (a ordem da lista é a ordem de navegação)")]
    [SerializeField] private List<ButtonBinding> bindings = new List<ButtonBinding>();

    [Header("Navegação pelos botões")]
    [SerializeField] private InputActionReference navigateUpAction;
    [SerializeField] private InputActionReference navigateDownAction;
    [SerializeField] private InputActionReference submitAction;

    [Tooltip("Opcional: se atribuído, a navegação fica desligada durante o jogo (estado Playing)")]
    [SerializeField] private GameManager gameManager;

    [Header("Aspeto do botão selecionado")]
    [Min(1f)]
    [SerializeField] private float selectedScale = 1.12f;

    private readonly List<InputAction> enabledByController = new List<InputAction>();
    private readonly Dictionary<Button, Vector3> baseScales = new Dictionary<Button, Vector3>();
    private readonly List<Button> visible = new List<Button>();
    private Button current;

    private void Awake()
    {
        foreach (ButtonBinding binding in bindings)
        {
            if (binding.button != null && !baseScales.ContainsKey(binding.button))
                baseScales.Add(binding.button, binding.button.transform.localScale);
        }
    }

    private void OnEnable()
    {
        foreach (ButtonBinding binding in bindings)
            EnableAction(binding.action);

        EnableAction(navigateUpAction);
        EnableAction(navigateDownAction);
        EnableAction(submitAction);
    }

    private void OnDisable()
    {
        foreach (InputAction action in enabledByController)
            action.Disable();

        enabledByController.Clear();

        current = null;
        ApplyHighlight();
    }

    private void Update()
    {
        // 1) Atalhos diretos
        foreach (ButtonBinding binding in bindings)
        {
            if (binding.button == null || !Pressed(binding.action))
                continue;

            if (IsUsable(binding.button))
                binding.button.onClick.Invoke();
        }

        // 2) Navegação
        bool navigationActive = gameManager == null || !gameManager.IsPlaying;

        CollectVisibleButtons();

        if (!navigationActive || visible.Count == 0)
        {
            current = null;
            ApplyHighlight();
            return;
        }

        // Se o botão selecionado desapareceu (mudou de painel), volta ao primeiro visível.
        if (current == null || !visible.Contains(current))
            current = visible[0];

        if (Pressed(navigateUpAction))
            MoveSelection(-1);

        if (Pressed(navigateDownAction))
            MoveSelection(1);

        if (Pressed(submitAction) && current != null)
            current.onClick.Invoke();

        ApplyHighlight();
    }

    // ------------------------------------------------------------------

    private void CollectVisibleButtons()
    {
        visible.Clear();

        foreach (ButtonBinding binding in bindings)
        {
            if (binding.button != null && IsUsable(binding.button) && !visible.Contains(binding.button))
                visible.Add(binding.button);
        }
    }

    private void MoveSelection(int direction)
    {
        if (visible.Count == 0)
            return;

        int index = Mathf.Max(0, visible.IndexOf(current));
        index = (index + direction + visible.Count) % visible.Count;
        current = visible[index];
    }

    // Aumenta o botão selecionado e repõe o tamanho dos outros.
    private void ApplyHighlight()
    {
        foreach (KeyValuePair<Button, Vector3> pair in baseScales)
        {
            if (pair.Key == null)
                continue;

            pair.Key.transform.localScale =
                pair.Key == current ? pair.Value * selectedScale : pair.Value;
        }
    }

    private static bool IsUsable(Button button)
    {
        return button.gameObject.activeInHierarchy && button.interactable;
    }

    private static bool Pressed(InputActionReference reference)
    {
        return reference != null && reference.action != null && reference.action.WasPressedThisFrame();
    }

    private void EnableAction(InputActionReference reference)
    {
        InputAction action = reference != null ? reference.action : null;

        if (action != null && !action.enabled)
        {
            action.Enable();
            enabledByController.Add(action);
        }
    }
}
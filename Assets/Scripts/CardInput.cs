using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardInput : NetworkBehaviour
{
    [SerializeField] private TMP_InputField inputField;

    [SyncVar(hook = nameof(OnTextChanged))]
    private string syncedText;

    private void Start()
    {
        inputField.onValueChanged.AddListener(OnLocalTextChanged);
    }

    private void OnEnable()
    {
        inputField.onValidateInput += ValidateInput;
    }

    private void OnDisable()
    {
        inputField.onValidateInput -= ValidateInput;
    }

    private char ValidateInput(string text, int index, char character)
    {
        return char.IsDigit(character) ? character : '\0';
    }

    private void OnLocalTextChanged(string value)
    {
        if (isOwned)
            CmdUpdateText(value);
    }

    [Command]
    private void CmdUpdateText(string value)
    {
        syncedText = value;
    }

    private void OnTextChanged(string oldValue, string newValue)
    {
        if (!isOwned)
            inputField.SetTextWithoutNotify(newValue);
    }

    public override void OnStartClient()
    {
        inputField.interactable = false;
    }

    public void SetInputEnabled(bool enabled)
    {
        if (!isOwned) return;

        inputField.interactable = enabled;

        if (enabled)
        {
            // Automatically focus the field
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            inputField.ActivateInputField();
        }
        else
        {
            // Remove focus
            inputField.DeactivateInputField();
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public string GetInput()
    {
        return syncedText;
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMatchmakingController : MonoBehaviour
{
    [SerializeField] private Button queueButton;
    [SerializeField] private GameAPIClient gameAPIClient;

    private void Awake()
    {
        SetButtonToJoin();
    }

    private void SetButtonToJoin()
    {
        queueButton.GetComponentInChildren<TMP_Text>().text = "Find Match";
        queueButton.onClick.RemoveAllListeners();
        queueButton.onClick.AddListener(gameAPIClient.JoinQueue);
        queueButton.onClick.AddListener(SetButtonToLeave);
    }

    private void SetButtonToLeave()
    {
        queueButton.GetComponentInChildren<TMP_Text>().text = "Leave Queue";
        queueButton.onClick.RemoveAllListeners();
        queueButton.onClick.AddListener(gameAPIClient.LeaveQueue);
        queueButton.onClick.AddListener(SetButtonToJoin);
    }
}

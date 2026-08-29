using Mirror;
using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreManager : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnScore1Changed))] private int score1;
    private GameObject card1;
    [SerializeField] private TextMeshPro scoreBoard1;
    private bool liftedCard1 = false;

    [SyncVar(hook = nameof(OnScore2Changed))] private int score2;
    private GameObject card2;
    [SerializeField] private TextMeshPro scoreBoard2;
    private bool liftedCard2 = false;



    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        CardPhysics card = other.GetComponent<CardPhysics>();
        if (card == null) { Debug.LogWarning("[ScoreManager] Non-card entered table"); return; }
        if (card.isPlayer1)
        {
            card1 = card.gameObject;
            Debug.Log("Card1 is on Table");
        }
        else 
        { 
            card2 = card.gameObject;
            Debug.Log("Card2 is on Table");
        }

        

        StartCoroutine(CheckForScoring());
    }

    [ServerCallback]
    private void OnTriggerExit(Collider other)
    {
        CardPhysics card = other.GetComponent<CardPhysics>();
        if (card == null) { Debug.LogWarning("[ScoreManager] Non-card exited table"); return; }

        if (card.isPlayer1)
        {
            card1 = null;
            liftedCard1 = true;
            Debug.Log("Card1 is off Table");
        }
        else
        {
            card2 = null;
            liftedCard2 = true;
            Debug.Log("Card2 is off Table");
        }

    }

    [ServerCallback]
    private IEnumerator CheckForScoring()
    {
        if (card1 == null || card2 == null)
        {
            yield break;
        }
        Debug.Log("[ScoreManager] Both cards on table. Checking...");

        //Make sure both cards have been lifted
        if (!liftedCard1 || !liftedCard2)
        {
            Debug.Log("Cards have not both been lifted yet");
            yield break;
        }

        //Make sure numbers were inputed
        CardInput input1 = card1.GetComponent<CardInput>();
        CardInput input2 = card2.GetComponent<CardInput>();
        if (input2 == null || input1 == null) yield break;
        int val1;
        if (!int.TryParse(input1.GetInput(), out val1)) yield break;
        int val2;
        if (!int.TryParse(input2.GetInput(), out val2)) yield break;

        //Lock cards
        CardPhysics card1Physics = card1.GetComponent<CardPhysics>();
        CardPhysics card2Physics = card2.GetComponent<CardPhysics>();
        if (card2Physics == null || card1Physics == null)
        {
            yield break;
        }
        card1Physics.locked = true;
        card2Physics.locked = true;
        Debug.Log("[ScoreManager] Locking cards...");

        yield return new WaitForSeconds(2f);
        CalculateRound();
        card1Physics.locked = false;
        card2Physics.locked = false;
        liftedCard1 = false;
        liftedCard2 = false;
    }

    private void CalculateRound()
    {
        if (card1 == null) { Debug.LogWarning("[ScoreManager] card1 is null"); return; }
        if (card2 == null) { Debug.LogWarning("[ScoreManager] card2 is null"); return; }

        CardInput input1 = card1.GetComponent<CardInput>();
        if (input1 == null) { Debug.LogWarning("[ScoreManager] card1 has no CardInput"); return; }
        CardInput input2 = card2.GetComponent<CardInput>();
        if (input2 == null) { Debug.LogWarning("[ScoreManager] card2 has no CardInput"); return; }

        int val1;
        if (!int.TryParse(input1.GetInput(), out val1)) { Debug.LogWarning("[ScoreManager] input1 has invalid string"); return; }
        int val2;
        if (!int.TryParse(input2.GetInput(), out val2)) { Debug.LogWarning("[ScoreManager] input2 has invalid string"); return; }

        if (val1 < 0) { Debug.LogWarning("[ScoreManager] val1 is negative."); return; }
        if (val2 < 0) { Debug.LogWarning("[ScoreManager] val2 is negative."); return; }

        int difference = Mathf.Abs(val1 - val2);
        int winner;

        if (difference == 0)
        {
            winner = 0;
        }
        else if (difference > 5)
        {
            winner = val1 > val2 ? 2 : 1;
        }
        else
        {
            winner = val1 > val2 ? 1 : 2;
        }

        if (winner == 1) score1++;
        if (winner == 2) score2++;
    }

    private void OnScore1Changed(int oldValue, int newValue)
    {
        scoreBoard1.text = newValue.ToString();
    }

    private void OnScore2Changed(int oldValue, int newValue)
    {
        scoreBoard2.text = newValue.ToString();
    }
}
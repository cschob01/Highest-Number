using Mirror;
using System;
using System.Collections;
using System.Net;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.Audio.ProcessorInstance;

public class GameAPIClient : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;

    private string baseUrl = "https://ancient-grass-8f91.carlos-schober.workers.dev";
    private bool isPolling = false;

    //Revert after testing: This version will store player ID between plays
    //private string PlayerId
    //{
    //    get
    //    {
    //        if (!PlayerPrefs.HasKey("playerId"))
    //        {
    //            PlayerPrefs.SetString("playerId", System.Guid.NewGuid().ToString());
    //            PlayerPrefs.Save();
    //        }
    //        return PlayerPrefs.GetString("playerId");
    //    }
    //}

    private string playerId;

    // TEMP: New key generated each play
    private string PlayerId
    {
        get
        {
            if (string.IsNullOrEmpty(playerId))
            {
                playerId = System.Guid.NewGuid().ToString();
            }
            return playerId;
        }
    }

    [Serializable]
    private class QueueStatusResponse
    {
        public string status;
        public string address;
    }

    public void JoinGame()
    {
        StartCoroutine(PostRequest("/join"));
    }

    public void GetCount()
    {
        StartCoroutine(GetRequest("/count"));
    }

    public void DeployServer()
    {
        StartCoroutine(PostRequest("/deploy"));
    }

    public void JoinQueue()
    {
        StartCoroutine(PostJsonRequest("/queue/join", $"{{\"playerId\":\"{PlayerId}\"}}"));
        StartPolling();
    }

    public void LeaveQueue()
    {
        isPolling = false;
        StartCoroutine(PostJsonRequest("/queue/leave", $"{{\"playerId\":\"{PlayerId}\"}}"));
    }

    public void CheckQueueStatus()
    {
        StartCoroutine(GetRequest($"/queue/status?playerId={PlayerId}"));
    }

    // Polls /queue/status every 2 seconds until a match is ready, then connects.
    private void StartPolling()
    {
        if (isPolling) return;
        isPolling = true;
        StartCoroutine(PollQueueStatus());
    }

    IEnumerator PollQueueStatus()
    {
        while (isPolling)
        {
            UnityWebRequest req = UnityWebRequest.Get($"{baseUrl}/queue/status?playerId={PlayerId}");
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                QueueStatusResponse response = JsonUtility.FromJson<QueueStatusResponse>(req.downloadHandler.text);
                Debug.Log("Queue status: " + response.status);
                statusText.text = "Queue status: " + response.status;

                if (response.status == "ready" && !string.IsNullOrEmpty(response.address))
                {
                    isPolling = false;
                    ConnectToMatch(response.address);
                    yield break;
                }
            }
            else
            {
                Debug.LogWarning("Status check failed: " + req.error);
                statusText.text = "Status check failed: " + req.error;
            }

            yield return new WaitForSeconds(2f);
        }
    }

    private void ConnectToMatch(string address)
    {
        Debug.Log("Connecting to match server: " + address);
        statusText.text = "Connecting to match server: " + address;

        NetworkManager.singleton.StartClient(new Uri(address));
    }

    IEnumerator PostRequest(string endpoint)
    {
        UnityWebRequest req = UnityWebRequest.PostWwwForm(baseUrl + endpoint, "");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + req.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error: " + req.error);
            Debug.LogError("Response body: " + req.downloadHandler.text);
        }
    }

    IEnumerator PostJsonRequest(string endpoint, string jsonBody)
    {
        UnityWebRequest req = new UnityWebRequest(baseUrl + endpoint, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + req.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error: " + req.error);
            Debug.LogError("Response body: " + req.downloadHandler.text);
        }
    }

    IEnumerator GetRequest(string endpoint)
    {
        UnityWebRequest req = UnityWebRequest.Get(baseUrl + endpoint);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + req.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error: " + req.error);
        }
    }
}
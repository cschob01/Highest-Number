using Mirror;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager
{
    [Header("Edgegap")]
    [SerializeField] private float shutdownDelay = 30f;

    private Coroutine shutdownCoroutine;
    private float shutdownTimer = 0f;

    private bool secondSpawn = false;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        NetworkPlayer player = conn.identity.GetComponent<NetworkPlayer>();

        if (player != null)
        {
            player.SetCardPlayer(!secondSpawn);
            secondSpawn = !secondSpawn;
        }
        else Debug.LogWarning("[CustomNetworkManager] NewtorkPlayer not found on network identity for " + conn.identity);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);

        Debug.Log("[CustomNetworkManager] Server Disconnect");

        // Check whether anyone is still connected.
        if (NetworkServer.connections.Count == 0)
        {
            if (shutdownCoroutine != null)
                StopCoroutine(shutdownCoroutine);

            shutdownCoroutine = StartCoroutine(ShutdownWhenEmpty());
        }
        else if (shutdownCoroutine != null)
        {
            StopCoroutine(shutdownCoroutine);
            shutdownCoroutine = null;
        }
    }

    private IEnumerator ShutdownWhenEmpty()
    {
        Debug.Log(
            $"[CustomNetworkManager] Server is empty. " +
            $"Deployment will terminate in {shutdownDelay} seconds."
        );

        shutdownTimer = 0f;
        while(shutdownTimer < shutdownDelay)
        {
            shutdownTimer += Time.deltaTime;
            if (NetworkServer.connections.Count != 0)
            {
                shutdownCoroutine = null;
                Debug.Log("[CustomNetworkManager] Player rejoined, abandoning termination.");
                yield break;
            }

            yield return null;
        }

        Debug.Log("[CustomNetworkManager] Server still empty. Terminating deployment.");

        StartCoroutine(TerminateEdgegapDeployment());

        shutdownCoroutine = null;
    }

    private IEnumerator TerminateEdgegapDeployment()
    {
        Debug.Log("[CustomNetworkManager] Terminating Deployment...");

        string deleteUrl = Environment.GetEnvironmentVariable("ARBITRIUM_DELETE_URL");
        string deleteToken = Environment.GetEnvironmentVariable("ARBITRIUM_DELETE_TOKEN");

        if (string.IsNullOrEmpty(deleteUrl))
        {
            Debug.LogError("[CustomNetworkManager] ARBITRIUM_DELETE_URL was not provided.");
            yield break;
        }

        if (string.IsNullOrEmpty(deleteToken))
        {
            Debug.LogError("[CustomNetworkManager] ARBITRIUM_DELETE_TOKEN was not provided.");
            yield break;
        }

        Debug.Log("[CustomNetworkManager] Requesting deployment termination...");

        using (UnityWebRequest request = UnityWebRequest.Delete(deleteUrl))
        {
            request.SetRequestHeader("Authorization", deleteToken);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[CustomNetworkManager] Deployment termination requested.");
            }
            else
            {
                Debug.LogError(
                    $"[CustomNetworkManager] Failed to terminate deployment: {request.error}"
                );
            }
        }
    }
}
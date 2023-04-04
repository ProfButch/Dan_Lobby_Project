using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Unity.Netcode;
public class NetworkHandler : NetworkBehaviour
{
    private static NetworkHandler _instance;
    public static NetworkHandler Singleton {
        get {
            return _instance;
        }
    }

    public NetworkList<It4080.PlayerData> allPlayers;

    public void Awake() {
        // allPlayers must be initialized even though we might be destroying
        // this instance.  Errors occur if we do not.
        allPlayers = new NetworkList<It4080.PlayerData>();
        // This isn't working as expected.  If you place another GameData in a
        // later scene, it causes an error.  I suspect this has something to
        // do with the NetworkList but I have not verified that yet.  It causes
        // Network related errors.
        if (_instance == null) {
            _instance = this;
            DontDestroyOnLoad(this);
        } else if (_instance != this) {
            Destroy(this);
        }
    }

    public override void OnNetworkSpawn() {
        if (IsServer) {
            NetworkManager.Singleton.OnClientConnectedCallback += ServerOnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += ServerOnClientDisconnected;
            allPlayers.Add(new It4080.PlayerData(NetworkManager.LocalClientId, Color.blue, true));
        }
    }

    public int FindPlayerIndex(ulong clientId) {
        var idx = 0;
        var found = false;
        while (idx < allPlayers.Count && !found) {
            if (allPlayers[idx].clientId == clientId) {
                found = true;
            } else {
                idx += 1;
            }
        }
        if (!found) {
            idx = -1;
        }
        return idx;
    }

    private void ServerOnClientConnected(ulong clientId)
    {
        AddPlayerToList(clientId);
    }

    private void ServerOnClientDisconnected(ulong clientId)
    {
        RemovePlayerFromList(clientId);
    }

    public void AddPlayerToList(ulong clientId) {
        allPlayers.Add(new It4080.PlayerData(clientId, Color.blue, false));
    }

    public void RemovePlayerFromList(ulong clientId) {
        int index = FindPlayerIndex(clientId);
        if (index != -1) {
            allPlayers.RemoveAt(index);
        }
    }

    // This can cause runtime errors since you could give it a clientId
    // That does not exist.  It4080 is not nullable, so deal with it
    // however you like.
    public It4080.PlayerData GetPlayerFromList(ulong clientId) {
        int index = FindPlayerIndex(clientId);
        return allPlayers[index];
    }
}
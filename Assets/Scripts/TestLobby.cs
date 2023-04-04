using System.Collections;
using System.Collections.Generic;
using It4080;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
//using UnityEngine.UIElements;
using UnityEngine.UI;
/// <summary>
/// Need to get inital clear working, then I think remove from list 
/// </summary>
public class TestLobby : NetworkBehaviour
{
    public Button btnStart;
    public ConnectedPlayers connPlayers;
    public Image kicktext; 
    void Start()
    {
        initialClear();
        kicktext.gameObject.SetActive(false);
        
    }

    public void initialClear()
    {
        connPlayers.Clear();
        PopulateConnectedPlayersUsingPlayerDataList(NetworkHandler.Singleton.allPlayers);
        
    }
    public override void OnNetworkSpawn()
    {
        initialClear();
        if (IsClient)
        {
            NetworkHandler.Singleton.allPlayers.OnListChanged += ClientOnAllPlayersChanged;
            PopulateConnectedPlayersUsingPlayerDataList(NetworkHandler.Singleton.allPlayers);
            if (IsHost)
            {
                Debug.Log("Disconnect");
                NetworkManager.OnClientDisconnectCallback += ClientOnDisconnect;
            }
        }

        if (IsServer)
        {
            Debug.Log("list changed ");
            NetworkHandler.Singleton.allPlayers.OnListChanged += LogNetworkListEvent;
            
            
        }

       btnStart.gameObject.SetActive(false);
    }
    

    public PlayerCard AddPlayerCard(ulong clientId)
    {
        It4080.PlayerCard newCard = connPlayers.AddPlayer("temp", clientId);
        string you = "";
        string what = "";
        
        newCard.ShowKick(IsServer);
        if (IsServer)
        {
            newCard.KickPlayer += ServerOnKickButtonPressed;
        }

        if (clientId == NetworkManager.LocalClientId)
        {
            you = "(you)";
            newCard.SetReady(true);
            newCard.ReadyToggled += ClientOnlyReadyToggled;
            
        }
        else
        {
            you = "";
            newCard.SetReady(false);
        }

        if (clientId == NetworkManager.ServerClientId)
        {
            what = "Host";
            newCard.SetReady(false);
            newCard.ShowKick(false);
            
        }
        else
        {
            what = "Player";
        }
        newCard.SetPlayerName($"{what} {clientId}{you}");
        return newCard;
    }

    private void LogNetworkListEvent(NetworkListEvent<It4080.PlayerData> changeEvent)
    {
        Debug.Log($"Player data changed:");
        Debug.Log($"    Change Type:  {changeEvent.Type}");
        Debug.Log($"    Value:        {changeEvent.Value}");
        Debug.Log($"        {changeEvent.Value.clientId}");
        Debug.Log($"        {changeEvent.Value.isReady}");
        Debug.Log($"    Prev Value:   {changeEvent.PreviousValue}");
        Debug.Log($"        {changeEvent.PreviousValue.clientId}");
        Debug.Log($"        {changeEvent.PreviousValue.isReady}");
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void RequestSetReadyServerRpc(bool isReady, ServerRpcParams rpcParams = default) {
        Debug.Log("setting ready");
        ulong clientId = rpcParams.Receive.SenderClientId;
        int playerIndex = NetworkHandler.Singleton.FindPlayerIndex(clientId);
        It4080.PlayerData info = NetworkHandler.Singleton.allPlayers[playerIndex];
        info.isReady = isReady;
        NetworkHandler.Singleton.allPlayers[playerIndex] = info;
        Debug.Log("RPC call to refresh");
        listRefresh();
        initialClear();
        PopulateConnectedPlayersUsingPlayerDataList(NetworkHandler.Singleton.allPlayers);
    }

    public bool EnableStartIfAllReady(NetworkList<It4080.PlayerData> players)
    {
        var count = 0;
        var len = NetworkHandler.Singleton.allPlayers.Count;
        foreach (It4080.PlayerData p in players)
        {
            if (p.isReady)
                count++;
        }

        if (len == count)
        {
            btnStart.gameObject.SetActive(true);
            Debug.Log("button should be working");
            return true;
          
        }
        else return false; 
    }


    private void PopulateConnectedPlayersUsingPlayerDataList(NetworkList<It4080.PlayerData> players)
    {
        foreach (It4080.PlayerData p in players)
        {
         var card = AddPlayerCard(p.clientId);
           card.SetReady(p.isReady);
            string status = "Not Ready";
            if (p.isReady)
            {
                status = "READY!!";
            }

           card.SetStatus(status);
        }
    }

    public void listRefresh()
    {
        Debug.Log("time 2 refesh");
        initialClear();
        PopulateConnectedPlayersUsingPlayerDataList(NetworkHandler.Singleton.allPlayers);
    }

    private void ClientOnDisconnect(ulong clientId)
    {
        kicktext.gameObject.SetActive(true);
        connPlayers.gameObject.SetActive(false);
        
    }

    private void ClientOnAllPlayersChanged(NetworkListEvent<PlayerData> changeEvent)
    {
      PopulateConnectedPlayersUsingPlayerDataList(NetworkHandler.Singleton.allPlayers);
        if (IsHost)
        {
        EnableStartIfAllReady(NetworkHandler.Singleton.allPlayers);
        }
    }
    

    private void ClientOnlyReadyToggled(bool isReady)
    {
        RequestSetReadyServerRpc(isReady);
    }

    private void ServerOnKickButtonPressed(ulong clientId)
    {
        Debug.Log($"Kicking {clientId}");
        NetworkManager.DisconnectClient(clientId);
       NetworkHandler.Singleton.RemovePlayerFromList(clientId);
       kicktext.gameObject.SetActive(true);
       Debug.Log("kick image should be up");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

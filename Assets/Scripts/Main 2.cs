using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Main : NetworkBehaviour 
{
    public It4080.NetworkSettings netSettings;
    // Start is called before the first frame update
    public Button _btnStart; 
    void Start()
    {
        netSettings.startServer += NetSettingsOnServerStart;
        netSettings.startHost += NetSettingsOnHostStart;
        netSettings.startClient += NetSettingsOnClientStart;
        netSettings.setStatusText("Not Connected");
       //chat.SystemMessage("hello world");
     //  chat.sendMessage += ChatOnSendMessage;
     //  It4080.Chat.ChatMessage msg = new It4080.Chat.ChatMessage();
     //  msg.message = "foobar"; 
      //   chat.ShowMessage(msg);
      _btnStart = GameObject.Find("StartGameBttn").GetComponent<Button>();
      _btnStart.onClick.AddListener(BtnStartGameOnClick);
    
      _btnStart.gameObject.SetActive(false); 
      


    }

    private void BtnStartGameOnClick()
    {
        GotoLobby();
    }
    private void StartGame()
    {
        NetworkManager.SceneManager.LoadScene("Arena1", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    private void GotoLobby()
    {
        NetworkManager.SceneManager.LoadScene("TestLobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    private void ChatOnSendMessage(It4080.Chat.ChatMessage msg)
    {
      //  chatserver.RequestSendChatMessageServerRpc(msg.message);
    }

    private void setupTransport(IPAddress ip, ushort port)
    {
        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.ConnectionData.Address = ip.ToString();
        utp.ConnectionData.Port = port;
    }

    private void startClient(IPAddress ip, ushort port)
    {
        NetworkManager.Singleton.OnClientConnectedCallback += ClientOnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HostOnClientDisconnected;
        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
      utp.ConnectionData.Address = ip.ToString();
      utp.ConnectionData.Port = port;
      NetworkManager.Singleton.StartClient();
      netSettings.setStatusText("Connecting to host/Server");
      netSettings.hide();
      Debug.Log("started client");


    }

  ////  private string MakeWelcomeMessage(ulong clientId)
   // {
      //  return $"Welcome to the server, you are player {clientId}. Have a good time.";
   // }

    private void startHost(IPAddress ip, ushort port)
    {
        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.ConnectionData.Address = ip.ToString();
        utp.ConnectionData.Port = port;
        
        NetworkManager.Singleton.OnServerStarted += HostOnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HostOnClientConnected; 
        NetworkManager.Singleton.OnClientDisconnectCallback += HostOnClientDisconnected;  
        
        netSettings.setStatusText("Starting Host");
        
        NetworkManager.Singleton.StartHost();
        netSettings.hide();
        _btnStart.gameObject.SetActive(true);
        Debug.Log("started host");
    }

    private void startServer(IPAddress ip, ushort port)
    {
        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.ConnectionData.Address = ip.ToString();
        utp.ConnectionData.Port = port;

        NetworkManager.Singleton.OnServerStarted += HostOnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HostOnClientConnected; 
        NetworkManager.Singleton.OnClientDisconnectCallback += HostOnClientDisconnected; 
        
        netSettings.setStatusText("Starting Server");
        
        NetworkManager.Singleton.StartServer(); 
        netSettings.hide();
        Debug.Log("started server");
    }
    private void printIs(string msg)
    {
        Debug.Log($"server:{IsServer} host:{IsHost} client:{IsClient} owner:{IsOwner}");
    } 
    // ----------
    //Events 
    private void HostOnServerStarted()
    {
        printIs("host/server started event");
        if (IsHost)
        {
            netSettings.setStatusText($"Host Running. We are client {NetworkManager.Singleton.LocalClientId}");
          //  chat.enabled = true;
            //chat.enable(true);
        }
        else
        {
            netSettings.setStatusText("Server Running");
        }
     //   chat.SystemMessage("Server/Host Started");
    }
    private void HostOnClientConnected(ulong clientId)
    {
        Debug.Log($"Client Connected: {clientId}");
       //chatserver.SendSystemMessageServerRpc($"Client {clientId} connected.");
      // chatserver.SendSystemMessageServerRpc(MakeWelcomeMessage(clientId), clientId);
    }
    private void HostOnClientDisconnected(ulong clientId)
    {
      //  chatserver.SendSystemMessageServerRpc($"Client {clientId} disconnected");
      //  Debug.Log($"Client Disconnected: {clientId}");
     //  chatserver.SendChatMessageClientRpc($"Client {clientId} disconnected");
    }

    private void ClientOnClientConnected(ulong clientId)
    {
        netSettings.setStatusText($"Connected as {clientId}");
       // chat.enabled = true; 
        //chat.enable(true);
    }

    private void ClientOnClientDisconnect(ulong clientId)
    {
      //  chat.SystemMessage("disconnected from server");
        netSettings.setStatusText("Connection Lost");
        netSettings.show();
       // chat.enable(false);
    }
    private void NetSettingsOnClientStart(IPAddress ip, ushort port)
    {
        startClient(ip,port);
        Debug.Log("Starting Client");
    }
    private void NetSettingsOnHostStart(IPAddress ip, ushort port)
    {
        startHost(ip, port);
        Debug.Log("Starting host");
    }
    private void NetSettingsOnServerStart(IPAddress ip, ushort port)
    {
        startServer(ip,port);
        Debug.Log("Starting Server");
    }

    
    
    //Update 
    void Update()
    { 
        
    }
}

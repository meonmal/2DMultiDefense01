using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public partial class Net_Manager : MonoBehaviour
{
    /// <summary>
    /// Join Code를 직접 입력해서 참가하는 함수.
    /// </summary>
    public async void JoinGameWithCode(string inputJoinCode)
    {
        if (string.IsNullOrWhiteSpace(inputJoinCode))
        {
            Debug.Log("유효하지 않은 Join Code입니다.");
            return;
        }

        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(inputJoinCode);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("게임 접속 실패 : " + e);
        }
    }

    /// <summary>
    /// 랜덤 매칭 시작.
    /// 로비가 있으면 참가하고, 없으면 새로 만든다.
    /// </summary>
    public async void StartMatchmaking()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("로그인되지 않았습니다.");
            return;
        }

        currentLobby = await FindAvailableLobby();

        if (currentLobby == null)
        {
            await CreateNewLobby();
        }
        else
        {
            await JoinLobby(currentLobby.Id);
        }
    }

    /// <summary>
    /// 참가 가능한 로비를 찾는다.
    /// </summary>
    private async Task<Lobby> FindAvailableLobby()
    {
        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            if (queryResponse.Results != null && queryResponse.Results.Count > 0)
            {
                return queryResponse.Results[0];
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("로비 찾기 실패 : " + e);
        }

        return null;
    }

    /// <summary>
    /// 새 로비를 만들고, Relay까지 세팅한 뒤 Host를 시작한다.
    /// </summary>
    private async Task CreateNewLobby()
    {
        try
        {
            // 2인 게임이라면 Host를 제외한 나머지 자리 수는 1명.
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // 호스트도 반드시 Relay 정보를 Transport에 넣어줘야 한다.
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            // 로비를 만들 때 joinCode를 Data에 같이 저장해야
            // 나중에 참가한 클라이언트가 꺼내 쓸 수 있다.
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {
                        "joinCode",
                        new DataObject(DataObject.VisibilityOptions.Member, joinCode)
                    }
                }
            };

            currentLobby = await LobbyService.Instance.CreateLobbyAsync("랜덤 매칭방", maxPlayers, options);

            joinCodeText.text = joinCode;

            Debug.Log("새로운 방 생성됨 : " + currentLobby.Id);
            Debug.Log("Relay Join Code : " + joinCode);

            StartHost();
        }
        catch (System.Exception e)
        {
            Debug.LogError("로비 생성 실패 : " + e);
        }
    }

    /// <summary>
    /// 기존 로비에 참가한다.
    /// 로비에 저장된 joinCode를 이용해서 Relay 접속 후 Client 시작.
    /// </summary>
    private async Task JoinLobby(string lobbyId)
    {
        try
        {
            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            if (currentLobby.Data == null || !currentLobby.Data.ContainsKey("joinCode"))
            {
                Debug.LogError("로비에 joinCode 데이터가 없음");
                return;
            }

            string joinCode = currentLobby.Data["joinCode"].Value;

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            StartClient();
        }
        catch (System.Exception e)
        {
            Debug.LogError("로비 참가 실패 : " + e);
        }
    }

    /// <summary>
    /// Host 시작.
    /// 성공하면 접속 관련 콜백을 등록한다.
    /// </summary>
    private void StartHost()
    {
        bool isSuccess = NetworkManager.Singleton.StartHost();
        Debug.Log("호스트 시작 결과 : " + isSuccess);

        if (!isSuccess)
            return;

        RegisterNetworkCallbacks();
    }

    /// <summary>
    /// Client 시작.
    /// </summary>
    private void StartClient()
    {
        bool isSuccess = NetworkManager.Singleton.StartClient();
        Debug.Log("클라이언트 시작 결과 : " + isSuccess);
    }

    /// <summary>
    /// 네트워크 콜백 등록.
    /// </summary>
    private void RegisterNetworkCallbacks()
    {
        RemoveNetworkCallbacks();

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    /// <summary>
    /// 네트워크 콜백 해제.
    /// </summary>
    private void RemoveNetworkCallbacks()
    {
        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    /// <summary>
    /// 어떤 클라이언트가 접속했을 때 실행된다.
    /// Host 자신도 포함해서 Count가 올라간다.
    /// </summary>
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"클라이언트 접속 : {clientId}");
        OnPlayerJoined();
    }

    /// <summary>
    /// 어떤 클라이언트가 연결 해제되었을 때 실행된다.
    /// </summary>
    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"클라이언트 연결 해제 : {clientId}");
    }
}

using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class Net_Manager : MonoBehaviour
{
    public Button StartMatchButton;
    public Button JoinMatchButton;
    public TextMeshProUGUI joinCodeText;
    public TMP_InputField fieldText;

    private Lobby currentLobby;

    /// <summary>
    /// async : 비동기 -> 동시에 일어나지 않는다.
    /// 요청이 완료될 때 까지 결과값이 나오지 않는다. 
    /// </summary>
    private async void Start()
    {
        // 제일 먼저 유니티 서비스에 대한 초기화 작업을 한다.
        await UnityServices.InitializeAsync();
        // 만약 유니티 서비스에 로그인이 되어 있지 않다면
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            // 유니티 서비스에 다시 로그인을 실행한다.
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        StartMatchButton.onClick.AddListener(() => StartMatchmaking());
        JoinMatchButton.onClick.AddListener(() => JoinGameWithCode(fieldText.text));
    }

    public async void JoinGameWithCode(string inputJoinCode)
    {
        if (string.IsNullOrEmpty(inputJoinCode))
        {
            Debug.Log("유효하지 않은 Join Code입니다.");
            return;
        }

        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(inputJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData);

            StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log("게임에 접속 실패: " + e);
        }
    }

    /// <summary>
    /// 랜덤 매칭 버튼에 연결할 함수.
    /// </summary>
    public async void StartMatchmaking()
    {
        // 로그인이 되어 있지 않다면 해당 함수는 실행을 종료한다.
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("로그인되지 않았습니다.");
            return;
        }

        // 로그인이 되어 있다면 currentLobby를 찾는 작업을 한다.
        currentLobby = await FindAvailableLobby();

        // 로비가 없으면 새로운 로비를 생성한다.
        if(currentLobby == null)
        {
            // 새로운 방을 만든 사람은 Host가 될 것이다.
            await CreateNewLobby();
        }
        // 만약 로비가 있으면 그 로브에 접근한다.
        else
        {
            // 이미 있는 방에 들어간 사람은 Client가 된다.
            await JoinLobby(currentLobby.Id);
        }
    }

    /// <summary>
    /// 로비를 찾는 함수.
    /// </summary>
    /// <returns></returns>
    private async Task<Lobby> FindAvailableLobby()
    {
        // 예외 처리
        // 일단은 try를 실행하고 실패하면 catch문을 실행한다.
        try
        {
            // LobbyService를 통해 Query에 저장된 로비를 가져온다.
            var queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            // 가져온 로비가 0보다 클 경우
            if(queryResponse.Results.Count > 0)
            {
                // 가져온 로비 중에서 가장 첫번째 로비를 반환한다.
                return queryResponse.Results[0];
            }
        }
        catch(LobbyServiceException e)
        {
            Debug.Log("로비 찾기 실해" + e);
        }
        // 로비를 찾는 것을 실해했다면
        // StartMatchmaking() 함수 안에 있는 if문이 실행된다.
        return null;
    }

    /// <summary>
    /// 찾는 로비가 없을 경우 실행될 함수.
    /// </summary>
    /// <returns></returns>
    private async Task CreateNewLobby()
    {
        try
        {
            // UGS 안에 있는 LobbyService를 통해 로비를 생성한다. 첫번째는 이름, 두번째는 최대 플레이어를 설정한다.
            currentLobby = await LobbyService.Instance.CreateLobbyAsync("랜덤 매칭방", 2);
            Debug.Log("새로운 방 생성됨 : " + currentLobby.Id);
            await AllocateRelayServerAndJoin(currentLobby);
            // 이렇게 방을 생성한 생성자는 Host가 된다.
            StartHost();
        }
        catch(LobbyServiceException e)
        {
            Debug.Log("로비 생성 실패." + e);
        }
    }

    /// <summary>
    /// 로비를 찾으면 실행된다.
    /// 쉽게 말해 로비가 0보다 큰 경우이다.
    /// </summary>
    /// <param name="lobbyId"></param>
    /// <returns></returns>
    private async Task JoinLobby(string lobbyId)
    {
        try
        {
            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            string joinCode = currentLobby.Data["joinCode"].Value;

            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData);

            StartClient();
        }
        catch (System.Exception e)
        {
            Debug.Log("로비 참가 실패 : " + e);
        }
    }

    /// <summary>
    /// 방을 생성하면 그 방의 정보를 Relay에 저장한다.
    /// </summary>
    /// <param name="lobby"></param>
    /// <returns></returns>
    private async Task AllocateRelayServerAndJoin(Lobby lobby)
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(lobby.MaxPlayers);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            joinCodeText.text = joinCode;
            Debug.Log("Relay 서버 할당 완료. Join Code : " + joinCode);
        }
        catch(RelayServiceException e)
        {
            Debug.Log("Relay 할당 실패" + e);
        }
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("호스트가 시작되었습니다.");
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("클라이언트가 시작되었습니다.");
    }
}

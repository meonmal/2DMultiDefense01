using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public partial class Net_Manager : MonoBehaviour
{
    [Header("UI")]
    public Button StartMatchButton;
    public Button JoinMatchButton;
    public TextMeshProUGUI joinCodeText;
    public TMP_InputField fieldText;

    [Header("Network")]
    private Lobby currentLobby;
    private const int maxPlayers = 2;
    private string gamePlaySceneName = "GamePlayScene";

    /// <summary>
    /// 유니티 서비스 초기화 및 익명 로그인.
    /// 버튼 이벤트도 여기서 연결한다.
    /// </summary>
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        StartMatchButton.onClick.AddListener(StartMatchmaking);
        JoinMatchButton.onClick.AddListener(() => JoinGameWithCode(fieldText.text));
    }

    /// <summary>
    /// 오브젝트가 파괴될 때 콜백 해제.
    /// </summary>
    private void OnDestroy()
    {
        RemoveNetworkCallbacks();
    }
}

using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class Net_Manager : MonoBehaviour
{
    /// <summary>
    /// 플레이어가 입장했을 때 현재 인원을 검사한다.
    /// maxPlayers에 도달하면 Host가 모든 플레이어를 게임 씬으로 이동시킨다.
    /// </summary>
    private void OnPlayerJoined()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton이 없음");
            return;
        }

        int playerCount = NetworkManager.Singleton.ConnectedClients.Count;

        Debug.Log($"현재 접속 인원 : {playerCount} / 최대 인원 : {maxPlayers}");

        if (!NetworkManager.Singleton.IsHost)
            return;

        if (playerCount >= maxPlayers)
        {
            ChangeSceneForAllPlayers();
        }
    }

    /// <summary>
    /// Host가 모든 플레이어를 게임 씬으로 이동시킨다.
    /// NetworkManager의 SceneManager를 통해 전환해야 클라이언트도 같이 이동한다.
    /// </summary>
    private void ChangeSceneForAllPlayers()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton이 없음");
            return;
        }

        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.LogWarning("Host가 아닌데 씬 전환을 시도함");
            return;
        }

        if (NetworkManager.Singleton.SceneManager == null)
        {
            Debug.LogError("SceneManager가 null임. NetworkManager의 Enable Scene Management를 체크해라.");
            return;
        }

        Debug.Log($"씬 전환 시작 : {gamePlaySceneName}");

        NetworkManager.Singleton.SceneManager.LoadScene(gamePlaySceneName, LoadSceneMode.Single);
    }
}

using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class CharacterSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject spawnPrefab;
    [SerializeField]
    private Monster spawnMonsterPrefab;

    /// <summary>
    /// 나누어진 칸들을 관리할 리스트
    /// </summary>
    private List<Vector2> spawnList = new List<Vector2>();
    private List<bool> spawnListArray = new List<bool>();
    public static List<Vector2> moveList = new List<Vector2>();

    private void Start()
    {
        GridStart();

        for(int i = 0; i< transform.childCount; i++)
        {
            moveList.Add(transform.GetChild(i).position);
        }

        StartCoroutine(SpawnMonsterCoroutine());
    }

    private IEnumerator SpawnMonsterCoroutine()
    {
        var go = Instantiate(spawnMonsterPrefab, moveList[0], Quaternion.identity);

        GameManager.Instance.AddMonster(go);

        yield return new WaitForSeconds(1f);

        StartCoroutine(SpawnMonsterCoroutine());
    }

    private void GridStart()
    {
        SpriteRenderer parentSprite = GetComponent<SpriteRenderer>();
        // 메인이 되는 스프라이트의 좌우 크기
        float parentWidth = parentSprite.bounds.size.x;
        // 메인이 되는 스프라이트의 상하 크기
        float parentHeight = parentSprite.bounds.size.y;

        float xCount = transform.localScale.x / 6;
        float yCount = transform.localScale.y / 3;

        // 상하 3개
        for (int row = 0; row < 3; row++)
        {
            // 좌우 6개
            for (int col = 0; col < 6; col++)
            {
                // 생성된 오브젝트의 시작 위치를 왼쪽부터 시작하게 만들려고 한다.
                // 예를 들어 지금 Spawner의 x좌표 길이가 5다.
                // 그럼 (-5 / 2) = -2.5 부터 한 칸이 시작되게 만드는 것이다.
                float xPos = (-parentWidth / 2) + (col * xCount) + (xCount / 2);
                float yPos = (parentHeight / 2) - (row * yCount) + (yCount / 2);

                // 나눈 칸들을 spawnList에 넣는다.
                spawnList.Add(new Vector2(xPos, yPos + transform.position.y - yCount));
                spawnListArray.Add(false);
            }
        }
    }

    public void Summon()
    {
        if(GameManager.Instance.Money < GameManager.Instance.SummonCount)
        {
            return;
        }

        GameManager.Instance.Money -= GameManager.Instance.SummonCount;
        GameManager.Instance.SummonCount += 2;

        int positionValue = -1;
        var go = Instantiate(spawnPrefab);
        for(int i = 0; i<spawnListArray.Count; i++)
        {
            if (spawnListArray[i] == false)
            {
                positionValue = i;
                spawnListArray[i] = true;
                break;
            }
        }

        go.transform.position = spawnList[positionValue];
    }
}

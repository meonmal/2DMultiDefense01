using TMPro;
using UnityEngine;

public class UIMain : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI MonsterCount;
    [SerializeField]
    private TextMeshProUGUI Money;
    [SerializeField]
    private TextMeshProUGUI Summon;

    [SerializeField]
    private Animator MoneyAnimator;

    private void Start()
    {
        GameManager.Instance.OnMoneyUp += MoneyAnim;
    }

    private void Update()
    {
        MonsterCount.text = GameManager.Instance.monsters.Count.ToString() + " / 100";
        Money.text = GameManager.Instance.Money.ToString();
        Summon.text = GameManager.Instance.SummonCount.ToString();

        Summon.color = GameManager.Instance.Money >= GameManager.Instance.SummonCount ? Color.white : Color.red;
    }

    private void MoneyAnim()
    {
        MoneyAnimator.SetTrigger("Get");
    }
}

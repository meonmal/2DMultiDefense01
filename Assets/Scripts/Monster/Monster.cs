using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Processors;
using UnityEngine.UI;

public class Monster : Character
{
    public int hp = 0;
    public int maxHp;
    private int targetValue;
    private bool isDead = false;

    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private HitText hitText;
    [SerializeField]
    private Image mFill;
    [SerializeField]
    private Image mFillDeco;

    public override void Start()
    {
        hp = maxHp;
        base.Start();
    }

    private void Update()
    {
        mFillDeco.fillAmount = Mathf.Lerp(mFillDeco.fillAmount, mFill.fillAmount, Time.deltaTime * 2.0f);

        if (isDead)
        {
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, CharacterSpawner.moveList[targetValue], Time.deltaTime * moveSpeed);

        if (Vector2.Distance(transform.position, CharacterSpawner.moveList[targetValue]) <= 0.0f)
        {
            targetValue++;
            spriteRenderer.flipX = targetValue >= 3 ? true : false;
            if(targetValue >= 4)
            {
                targetValue = 0;
            }
        }
    }

    public void GetDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        hp -= damage;
        mFill.fillAmount = (float)hp / (float)maxHp;
        Instantiate(hitText, transform.position, Quaternion.identity).Init(damage);

        if(hp <= 0)
        {
            isDead = true;
            GameManager.Instance.GetMoney(1);
            GameManager.Instance.ReMoveMonster(this);
            gameObject.layer = LayerMask.NameToLayer("Default");
            StartCoroutine(DeadCoroutine());
            AnimatorChange("Dead", true);
        }
    }

    private IEnumerator DeadCoroutine()
    {
        float alpha = 1.0f;

        while (spriteRenderer.color.a > 0.0f)
        {
            alpha -= Time.deltaTime;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);

            yield return null;
        }

        Destroy(gameObject);
    }
}

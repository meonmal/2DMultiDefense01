using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

public class Monster : Character
{
    public int hp = 0;
    private int targetValue;
    private bool isDead = false;

    [SerializeField]
    private float moveSpeed;

    public override void Start()
    {
        base.Start();
    }

    private void Update()
    {
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

        if(hp <= 0)
        {
            isDead = true;
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

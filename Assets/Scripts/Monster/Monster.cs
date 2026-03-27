using Unity.VisualScripting;
using UnityEngine;

public class Monster : Character
{
    private int targetValue;

    [SerializeField]
    private float moveSpeed;

    public override void Start()
    {
        base.Start();
    }

    private void Update()
    {
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
}

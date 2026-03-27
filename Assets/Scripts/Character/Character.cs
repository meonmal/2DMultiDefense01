using UnityEngine;

public class Character : MonoBehaviour
{
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;

    public virtual void Start()
    {
        animator = transform.GetChild(0).GetComponent<Animator>();
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }
}

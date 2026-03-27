using System.Collections;
using TMPro;
using UnityEngine;

public class HitText : MonoBehaviour
{
    [SerializeField]
    private float speed;
    [SerializeField]
    private float riseDuration;
    [SerializeField]
    private float fadeDuraction;

    public Vector3 offset = new Vector3(0, 2, 0);

    public TextMeshPro damageText;
    private Color textColor;

    public void Init(int damage)
    {
        damageText.text = damage.ToString();
        textColor = damageText.color;
        StartCoroutine(MoveAndFade());
    }

    private IEnumerator MoveAndFade()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + offset;

        float elapsedTime = 0;

        while(elapsedTime < riseDuration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / riseDuration);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        elapsedTime = 0;

        while(elapsedTime < fadeDuraction)
        {
            textColor.a = Mathf.Lerp(1, 0, elapsedTime / fadeDuraction);
            damageText.color = textColor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(this.gameObject);
    }
}

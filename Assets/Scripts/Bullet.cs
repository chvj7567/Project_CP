using UnityEngine;
using UnityEngine.UI;

public class Bullet : MonoBehaviour
{
    public AttackCat cat;
    public Image attackImg;
    public RectTransform targetRectTransform;
    private RectTransform followerRectTransform;

    public float followSpeed = 2f;

    private void Awake()
    {
        followerRectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (targetRectTransform != null)
        {
            if (targetRectTransform.GetComponent<Monster>().GetHp() <= 0)
            {
                CHMMain.Resource.Destroy(gameObject);
                return;
            }

            Vector2 targetPosition = targetRectTransform.anchoredPosition;
            Vector2 followerPosition = followerRectTransform.anchoredPosition;

            float distance = Vector2.Distance(targetPosition, followerPosition);
            
            Vector2 direction = (targetPosition - followerPosition).normalized;
            followerRectTransform.up = direction;

            if (distance > 5f)
            {
                /*Vector2 newPosition = Vector2.Lerp(followerPosition, targetPosition, Time.deltaTime * followSpeed);
                followerRectTransform.anchoredPosition = newPosition;*/

                followerRectTransform.anchoredPosition += direction * followSpeed * Time.deltaTime * 100;
            }
            else
            {
                targetRectTransform.GetComponent<Monster>().TakeDamage(cat.attackPower);
                CHMMain.Resource.Destroy(gameObject);
            }
        }
    }
}

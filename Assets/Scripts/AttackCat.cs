using DG.Tweening;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class AttackCat : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] Image attackImg;
    [SerializeField] Monster target;
    [SerializeField] float attackDelay;

    float timeSinceLastAttack = 0f;

    void Start()
    {
        gameObject.UpdateAsObservable().Subscribe(_ =>
        {
            if (timeSinceLastAttack < attackDelay)
            {
                timeSinceLastAttack += Time.deltaTime;
            }
            else
            {
                timeSinceLastAttack = 0f;

                Attack(target);
            }
        });
    }

    public void Attack(Monster target)
    {
        if (target.GetHp() <= 0) return;

        var my2DPos = GetComponent<Image>().rectTransform.anchoredPosition;

        //attackImg.gameObject.GetOrAddComponent<CHPoolable>();
        
        var attackImage = CHMMain.Resource.Instantiate(attackImg.gameObject, transform.parent);
        var rectTransform = attackImage.GetComponent<RectTransform>();
        
        rectTransform.localScale = Vector3.one;
        rectTransform.anchoredPosition = my2DPos;

        var bullet = attackImage.gameObject.GetOrAddComponent<Bullet>();
        bullet.canvas = canvas;
        bullet.targetRectTransform = target.GetComponent<RectTransform>();
    }
}

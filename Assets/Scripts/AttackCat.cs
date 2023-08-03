using DG.Tweening;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Collections.Generic;

public class AttackCat : MonoBehaviour
{
    [SerializeField] Image attackImg;
    [SerializeField] List<Monster> targetList;
    [SerializeField] float attackDelay;
    [SerializeField] public int attackPower;
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

                for (int i = 0; i < targetList.Count; ++i)
                {
                    if (targetList[i].GetHp() <= 0)
                    {
                        targetList.Remove(targetList[i]);
                    }
                    else
                    {
                        Attack(targetList[i]);
                        break;
                    }
                }
            }
        });
    }

    public void SetTarget(Monster target)
    {
        targetList.Add(target);
    }

    public void Attack(Monster target)
    {
        if (target.GetHp() <= 0) return;

        var my2DPos = GetComponent<Image>().rectTransform.anchoredPosition;

        //attackImg.gameObject.GetOrAddComponent<CHPoolable>();
        
        var attackImage = CHMMain.Resource.Instantiate(attackImg.gameObject, transform.parent);
        var rectTransform = attackImage.GetComponent<RectTransform>();
        
        rectTransform.localScale = new Vector3(3f, 3f, 3f);
        rectTransform.anchoredPosition = my2DPos;

        var bullet = attackImage.gameObject.GetOrAddComponent<Bullet>();
        bullet.cat = this;
        bullet.targetRectTransform = target.GetComponent<RectTransform>();
    }
}

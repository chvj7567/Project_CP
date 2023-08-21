using DG.Tweening;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Collections.Generic;

public class AttackCat : MonoBehaviour
{
    [SerializeField] RectTransform myRectTransform;
    [SerializeField] public Image attackImg;
    [SerializeField] List<Monster> targetList;
    [SerializeField] public float attackDelay;
    [SerializeField] public int attackPower;
    [SerializeField] public float attackSpeed;
    float timeSinceLastAttack = 0f;

    private void Awake()
    {
        myRectTransform = GetComponent<RectTransform>();
    }

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

                Monster target = null;
                float minDistance = 9999f;
                for (int i = 0; i < targetList.Count; ++i)
                {
                    if (targetList[i].GetHp() <= 0)
                    {
                        targetList.Remove(targetList[i]);
                    }
                    else
                    {
                        var distance = Vector2.Distance(myRectTransform.anchoredPosition, targetList[i].rectTransform.anchoredPosition);
                        if (minDistance > distance)
                        {
                            minDistance = distance;
                            target = targetList[i];
                        }
                        
                        break;
                    }
                }

                if (target != null)
                {
                    Attack(target);
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

        var my2DPos = myRectTransform.anchoredPosition;

        attackImg.gameObject.GetOrAddComponent<CHPoolable>();
        
        var attackImage = CHMMain.Resource.Instantiate(attackImg.gameObject, transform.parent);
        var rectTransform = attackImage.GetComponent<RectTransform>();
        
        rectTransform.localScale = new Vector3(3f, 3f, 3f);
        rectTransform.anchoredPosition = my2DPos;

        var bullet = attackImage.gameObject.GetOrAddComponent<Bullet>();
        bullet.cat = this;
        bullet.targetRectTransform = target.GetComponent<RectTransform>();
    }
}

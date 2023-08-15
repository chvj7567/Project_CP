using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class Spawner : MonoBehaviour
{
    [SerializeField] Transform spawnParent;
    [SerializeField] GameObject spawnObj;
    [SerializeField] List<AttackCat> attackCatList = new List<AttackCat>();
    [SerializeField] float spawnTime = 15f;
    [SerializeField] int firstHp = 100;
    [SerializeField] float firstTime = 60f;
    [SerializeField, ReadOnly] public ReactiveProperty<bool> OnSpawn = new ReactiveProperty<bool>();

    int index = 0;
    int increaseHp = 100;
    float decreaseTime = -0.5f;

    int curHp = 0;

    bool isSpawn = false;
    IEnumerator cor;

    public List<AttackCat> GetAttackCatList()
    {
        return attackCatList;
    }

    void Start()
    {
        cor = Spawn();
        spawnObj.SetActive(false);

        OnSpawn.Subscribe(_ =>
        {
            if(_ == true)
            {
                if (isSpawn == false)
                {
                    StartCoroutine(cor);
                }
            }
            else
            {
                if (isSpawn == true)
                {
                    isSpawn = false;
                    StopCoroutine(cor);
                }
            }
        });

        StartCoroutine(cor);
    }

    IEnumerator Spawn()
    {
        isSpawn = true;
        if (spawnObj != null)
        {
            while (true)
            {
                var copyObj = CHMMain.Resource.Instantiate(spawnObj, spawnParent);
                if (copyObj != null)
                {
                    copyObj.SetActive(true);
                    var copyMonster = copyObj.GetComponent<Monster>();
                    if (copyMonster != null)
                    {
                        var pos = copyMonster.rectTransform.anchoredPosition;
                        copyMonster.rectTransform.anchoredPosition = new Vector2(pos.x, Random.Range(pos.y - 150, pos.y));

                        if (index >= 10)
                        {
                            copyMonster.SetHp(curHp * 2);
                            spawnTime = 10f;
                        }
                        else
                        {
                            curHp = firstHp + (index * increaseHp);
                            copyMonster.SetHp(curHp);
                        }

                        copyMonster.Move(firstTime + (index * decreaseTime));

                        for (int i = 0; i < attackCatList.Count; ++i)
                        {
                            attackCatList[i].SetTarget(copyMonster);
                        }

                        ++index;
                    }
                }

                yield return new WaitForSeconds(spawnTime);
            }
        }
    }
}

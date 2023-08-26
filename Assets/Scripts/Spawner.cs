using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class Spawner : MonoBehaviour
{
    [SerializeField] Transform spawnParent;
    [SerializeField] GameObject spawnObj;
    [SerializeField] List<AttackCat> attackCatList = new List<AttackCat>();
    [SerializeField] float spawnTime;
    [SerializeField] public ReactiveProperty<bool> OnSpawn = new ReactiveProperty<bool>();
    [SerializeField, ReadOnly] int index = 0;

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
        //Spawn2();
    }

    IEnumerator Spawn()
    {
        isSpawn = true;

        if (spawnObj != null)
        {
            var stage = PlayerPrefs.GetInt("stage");
            Debug.Log($"Start {stage}Stage");

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
                        ++index;

                        var monsterInfo = CHMMain.Json.GetMonsterInfo(stage, index);
                        if (monsterInfo == null)
                            break;

                        copyMonster.SetHp(monsterInfo.hp);
                        copyMonster.Move(monsterInfo.moveTime);

                        Debug.Log($"Index:{index}, Hp:{monsterInfo.hp}, MoveTime:{monsterInfo.moveTime}");

                        for (int i = 0; i < attackCatList.Count; ++i)
                        {
                            attackCatList[i].SetTarget(copyMonster);
                        }
                    }
                }

                yield return new WaitForSeconds(spawnTime);
            }
        }
    }

    public void Spawn2()
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

                ++index;

                var monsterInfo = CHMMain.Json.GetMonsterInfo(1, index);
                if (monsterInfo == null)
                    return;

                copyMonster.SetHp(monsterInfo.hp);
                copyMonster.Move(monsterInfo.moveTime);

                for (int i = 0; i < attackCatList.Count; ++i)
                {
                    attackCatList[i].SetTarget(copyMonster);
                }
            }
        }
    }
}

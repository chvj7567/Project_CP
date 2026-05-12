using System;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using static Defines;
using static Infomation;

public class GPBossController
{
    public ReactiveProperty<int> hp = new ReactiveProperty<int>();

    GPBoard _board;
    StageInfo _stageInfo;
    Image _bossHpImage;
    CHTMPro _bossHpText;
    CHTMPro _hpText;
    GameObject _normalBossObj;
    GameObject _angryBossObj;
    GameObject _cryBossObj;
    bool _bossSkill;

    public void Init(
        GPBoard board,
        StageInfo stageInfo,
        Data.Login loginData,
        Image bossHpImage,
        CHTMPro bossHpText,
        CHTMPro hpText,
        GameObject normalBossObj,
        GameObject angryBossObj,
        GameObject cryBossObj,
        ReactiveProperty<int> curScore,
        MonoBehaviour owner)
    {
        _board = board;
        _stageInfo = stageInfo;
        _bossHpImage = bossHpImage;
        _bossHpText = bossHpText;
        _hpText = hpText;
        _normalBossObj = normalBossObj;
        _angryBossObj = angryBossObj;
        _cryBossObj = cryBossObj;

        hp.Subscribe(_ => { if (_ >= 0) _hpText.SetText(hp); }).AddTo(owner);
        hp.Value = loginData.hp;

        Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .Subscribe(_ => hp.Value -= 1)
            .AddTo(owner);

        curScore.Subscribe(_ =>
        {
            var fillAmount = (_stageInfo.targetScore - _) / (float)_stageInfo.targetScore;
            _bossHpImage.DOFillAmount(fillAmount, .5f);
            var bossHp = Mathf.Max(0, _stageInfo.targetScore - _);
            _bossHpText.SetText(bossHp);

            if (!_bossSkill && fillAmount <= .5f)
            {
                _bossSkill = true;
                _normalBossObj.SetActive(false);
                _angryBossObj.SetActive(true);

                CHMUI.Instance.ShowUI(EUI.UIAlarm, new UIAlarmArg { stringID = 78 });

                int coolTime;
                int mod = _stageInfo.stage % 10;
                if (mod == 0) { coolTime = 10; Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(coolTime)).Subscribe(_ => { BossSkill(1); BossSkill(2); BossSkill(3); }).AddTo(owner); }
                else if (mod >= 6) { coolTime = 10 - mod + 10; Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(coolTime)).Subscribe(_ => { BossSkill(1); BossSkill(2); }).AddTo(owner); }
                else { coolTime = 10 - mod + 10; Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(coolTime)).Subscribe(_ => BossSkill(1)).AddTo(owner); }
            }
        }).AddTo(owner);
    }

    public void OnClear()
    {
        _normalBossObj.SetActive(false);
        _angryBossObj.SetActive(false);
        _cryBossObj.SetActive(true);
    }

    public void BossSkill(int type)
    {
        var blockHp = UnityEngine.Random.Range(0, 10);
        if (blockHp == 0) blockHp = -1;

        int w, h;
        do
        {
            w = UnityEngine.Random.Range(0, _board.boardSize);
            h = UnityEngine.Random.Range(0, _board.boardSize);
        } while (!_board.boardArr[w, h].IsNormalBlock());

        EBlockState block;
        if (type == 1)
            block = (EBlockState)UnityEngine.Random.Range((int)EBlockState.Wall, (int)EBlockState.Potal + 1);
        else if (type == 2)
            block = (EBlockState)UnityEngine.Random.Range((int)EBlockState.WallCreator, (int)EBlockState.PotalCreator + 1);
        else
            block = (EBlockState)UnityEngine.Random.Range((int)EBlockState.CatBox1, (int)EBlockState.CatBox5 + 1);

        _board.boardArr[w, h].changeBlockState = block;
        _board.boardArr[w, h].changeHp = blockHp;
    }
}

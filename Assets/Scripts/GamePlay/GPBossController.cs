using System;
using DG.Tweening;
using UniRx;
using UnityEngine;
using ChvjUnityInfra;
using UnityEngine.UI;
using static Defines;
using static Infomation;

public class GPBossController
{
    public ReactiveProperty<int> hp = new ReactiveProperty<int>();

    GPBoard _board;
    StageInfo _stageInfo;
    Image _bossHpImage;
    CHText _bossHpText;
    CHText _hpText;
    GameObject _normalBossObj;
    GameObject _angryBossObj;
    GameObject _cryBossObj;
    bool _bossSkill;
    bool _isCleared;
    Tween _hitReactionTween;

    // 보스 HP가 자동으로 1씩 줄어드는 주기(초)
    const int BossHpDrainIntervalSeconds = 1;
    // 보스 HP 게이지 채움 트윈 시간(초)
    const float BossHpFillDuration = 0.5f;
    // 보스가 분노(스킬) 상태로 전환되는 HP 비율
    const float BossSkillHpThreshold = 0.5f;
    // 보스 스킬로 생성되는 블록 HP의 난수 상한 (0~상한-1, 0은 -1로 보정)
    const int BossSkillBlockMaxHp = 10;
    // 스테이지 그룹 크기 (stage % 그룹 크기로 그룹 내 위치를 계산)
    const int StageGroupSize = 10;
    // 보스 스킬 발동 기본 쿨타임(초)
    const int BossSkillBaseCooldownSeconds = 10;
    // 그룹 내 위치(mod)가 이 값 이상이면 보스가 스킬을 2개 사용
    const int BossMultiSkillModThreshold = 6;

    public void Init(
        GPBoard board,
        StageInfo stageInfo,
        Data.Login loginData,
        Image bossHpImage,
        CHText bossHpText,
        CHText hpText,
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

        Observable.Timer(TimeSpan.FromSeconds(BossHpDrainIntervalSeconds), TimeSpan.FromSeconds(BossHpDrainIntervalSeconds))
            .Subscribe(_ => hp.Value -= 1)
            .AddTo(owner);

        curScore.Subscribe(_ =>
        {
            float fillAmount = (_stageInfo.targetScore - _) / (float)_stageInfo.targetScore;
            _bossHpImage.DOFillAmount(fillAmount, BossHpFillDuration);
            int bossHp = Mathf.Max(0, _stageInfo.targetScore - _);
            _bossHpText.SetText(bossHp);

            if (!_bossSkill && fillAmount <= BossSkillHpThreshold)
            {
                _bossSkill = true;
                _normalBossObj.SetActive(false);
                _angryBossObj.SetActive(true);

                CHMUI.Instance.ShowUI(EUI.UIAlarm, new UIAlarmArg { stringID = 78 });

                int coolTime;
                int mod = _stageInfo.stage % StageGroupSize;
                if (mod == 0) { coolTime = BossSkillBaseCooldownSeconds; Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(coolTime)).Subscribe(_ => { BossSkill(EBossSkillType.Wall); BossSkill(EBossSkillType.Creator); BossSkill(EBossSkillType.CatBox); }).AddTo(owner); }
                else if (mod >= BossMultiSkillModThreshold) { coolTime = StageGroupSize - mod + BossSkillBaseCooldownSeconds; Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(coolTime)).Subscribe(_ => { BossSkill(EBossSkillType.Wall); BossSkill(EBossSkillType.Creator); }).AddTo(owner); }
                else { coolTime = StageGroupSize - mod + BossSkillBaseCooldownSeconds; Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(coolTime)).Subscribe(_ => BossSkill(EBossSkillType.Wall)).AddTo(owner); }
            }
        }).AddTo(owner);
    }

    // 공격 도착 시 잠깐 우는 이미지로 전환 후 복귀
    const float HitReactionDuration = 0.3f;

    public void ShowHitReaction()
    {
        if (_isCleared) return;

        _hitReactionTween?.Kill();
        _normalBossObj.SetActive(false);
        _angryBossObj.SetActive(false);
        _cryBossObj.SetActive(true);

        _hitReactionTween = DOVirtual.DelayedCall(HitReactionDuration, () =>
        {
            if (_isCleared) return;
            _cryBossObj.SetActive(false);
            (_bossSkill ? _angryBossObj : _normalBossObj).SetActive(true);
            _hitReactionTween = null;
        });
    }

    public void OnClear()
    {
        _isCleared = true;
        _hitReactionTween?.Kill();
        _normalBossObj.SetActive(false);
        _angryBossObj.SetActive(false);
        _cryBossObj.SetActive(true);
    }

    public void BossSkill(EBossSkillType type)
    {
        int blockHp = UnityEngine.Random.Range(0, BossSkillBlockMaxHp);
        if (blockHp == 0) blockHp = -1;

        int w, h;
        do
        {
            w = UnityEngine.Random.Range(0, _board.boardSize);
            h = UnityEngine.Random.Range(0, _board.boardSize);
        } while (!_board.boardArr[w, h].IsNormalBlock());

        EBlockState block;
        if (type == EBossSkillType.Wall)
            block = (EBlockState)UnityEngine.Random.Range((int)EBlockState.Wall, (int)EBlockState.Potal + 1);
        else if (type == EBossSkillType.Creator)
            block = (EBlockState)UnityEngine.Random.Range((int)EBlockState.WallCreator, (int)EBlockState.PotalCreator + 1);
        else
            block = (EBlockState)UnityEngine.Random.Range((int)EBlockState.CatBox1, (int)EBlockState.CatBox5 + 1);

        _board.boardArr[w, h].changeBlockState = block;
        _board.boardArr[w, h].changeHp = blockHp;
    }
}

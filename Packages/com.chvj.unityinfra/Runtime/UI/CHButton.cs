using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;

namespace ChvjUnityInfra
{
    [RequireComponent(typeof(Button))]
    [MovedFrom(false, null, null, "ButtonEx")]
    public class CHButton : MonoBehaviour
    {
        /// <summary>
        /// 클릭 사운드 재생을 게임 측에 위임하는 정적 hook.
        /// 게임 부팅 시 한 번만 등록: CHButton.ClickSoundHook = () => CHMSound.Instance.Play(EAudio.Click);
        /// </summary>
        public static Action ClickSoundHook;

        private Button _button;
        private TMP_Text _text;

        private bool _initialize = false;

        public bool Interactable
        {
            get
            {
                Init();
                return _button.interactable;
            }
            set
            {
                Init();
                _button.interactable = value;
            }
        }

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            if (_initialize)
                return;

            _initialize = true;
            _button = GetComponent<Button>();
            _text = GetComponentInChildren<TMP_Text>();

            // 클릭 SFX는 Button.onClick에 1회 등록한다.
            // 이렇게 두면 OnClick(callback) / Button.onClick.AddListener / OnClickAsObservable 어떤 경로로
            // 핸들러를 붙여도 클릭 사운드가 1회 발화한다 (OnClick(callback)에서는 hook을 호출하지 않음).
            if (_button != null)
            {
                _button.onClick.AddListener(() => ClickSoundHook?.Invoke());
            }
        }

        public void SetText(string text)
        {
            Init();

            if (_text == null)
                return;

            _text.text = text;
        }

        /// <summary>
        /// 콜백을 버튼에 등록. 버튼 GameObject가 파괴되면 listener도 같이 정리됨 (Unity 기본 동작).
        /// 명시적으로 listener를 제거해야 하면 CompositeDisposable 오버로드 사용.
        /// </summary>
        public void OnClick(Action callback)
        {
            Init();

            // 클릭 SFX는 Init의 자동 listener가 처리하므로 여기서는 콜백만 등록.
            _button.onClick.AddListener(() => callback?.Invoke());
        }

        /// <summary>
        /// 콜백을 버튼에 등록. disposable.Clear() 시 listener 명시적으로 제거.
        /// UIBase의 closeDisposable 등 명시적 정리가 필요한 케이스에 사용.
        /// </summary>
        public void OnClick(Action callback, CompositeDisposable disposable)
        {
            Init();

            UnityAction action = () => callback?.Invoke();
            _button.onClick.AddListener(action);
            disposable.Add(() =>
            {
                if (_button != null)
                {
                    _button.onClick.RemoveListener(action);
                }
            });
        }
    }
}

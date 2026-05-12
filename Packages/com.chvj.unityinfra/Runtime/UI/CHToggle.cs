using System;
using UnityEngine;
using UnityEngine.UI;

namespace ChvjUnityInfra
{
    [RequireComponent(typeof(Toggle))]
    public class CHToggle : MonoBehaviour
    {
        /// <summary>
        /// 값 변경 시 사운드 재생을 게임 측에 위임하는 정적 hook.
        /// 게임 부팅 시 한 번만 등록: CHToggle.ChangeSoundHook = () => AudioManager.Instance.Play(EAudio.Click);
        /// </summary>
        public static Action ChangeSoundHook;

        [NonSerialized]
        public Toggle toggle;

        private bool _first = true;

        private void Start()
        {
            toggle = GetComponent<Toggle>();

            toggle.onValueChanged.AddListener(_ =>
            {
                if (_first)
                {
                    _first = false;
                    return;
                }

                ChangeSoundHook?.Invoke();
            });
        }
    }
}

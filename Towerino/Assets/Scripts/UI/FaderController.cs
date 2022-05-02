using System;
using UnityEngine;

namespace Towerino
{
    public class FaderController : MonoBehaviour
    {
        public Vector2 FadeInOutDuration { get { return _fadeInOutDuration; } }

        [SerializeField]
        private CanvasGroup _faderCanvas = null;
        [SerializeField, Tooltip("X: Fade in duration, Y: Fade out duration")]
        private Vector2 _fadeInOutDuration = Vector2.zero;

        private bool _isFading;
        private float _waitTime, _currentTime, _start, _end;
        private FadeState _state = FadeState.inactive;
        private Action _cbAction;
        private enum FadeState { inactive, fadeOut, fadeIn }

        public void FadeIn(Action cbAction = null)
        {
            if (_isFading) return;

            if (!gameObject.activeInHierarchy) gameObject.SetActive(true);

            _state = FadeState.fadeIn;
            _currentTime = _start = 0;
            _end = 1;
            _waitTime = _fadeInOutDuration.x;

            if (_faderCanvas.alpha == 1) _currentTime = _waitTime;

            if (cbAction != null) _cbAction = cbAction;

            _isFading = true;
        }

        public void FadeOut()
        {
            if (_isFading) return;

            _state = FadeState.fadeOut;
            _start = 1;
            _currentTime = _end = 0;
            _waitTime = _fadeInOutDuration.y;

            if (_faderCanvas.alpha == 0) _currentTime = _waitTime;
        }

        private void Awake() { DontDestroyOnLoad(gameObject); }

        private void Update()
        {
            if (_state == FadeState.inactive) return;

            _currentTime += Time.deltaTime;
            _faderCanvas.alpha = Mathf.Lerp(_start, _end, _currentTime / _waitTime);

            if (_currentTime >= _waitTime) FadeCompleted();
        }

        private void FadeCompleted()
        {
            if (_state == FadeState.fadeOut) gameObject.SetActive(false);
            else if (_cbAction != null)
            {
                _cbAction.Invoke();
                _cbAction = null;
            }
            _state = FadeState.inactive;
            _isFading = false;
        }
    }
}

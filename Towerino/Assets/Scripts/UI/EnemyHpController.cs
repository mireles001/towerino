using UnityEngine;
using UnityEngine.UI;

namespace Towerino
{
    public class EnemyHpController : MonoBehaviour
    {
        [SerializeField]
        private Image _hpBar = null;
        [SerializeField]
        private Vector2 _offset = Vector2.zero;

        private RectTransform _rt;
        private CanvasGroup _canvas;
        private EnemyController _target;

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
            _canvas = GetComponent<CanvasGroup>();
            _canvas.alpha = 0;
        }

        private void Update()
        {
            if (_target == null) return;

            _hpBar.fillAmount = _target.HpPercentage;

            if (_canvas.alpha == 0 && _target.HpPercentage < 1)
            {
                _canvas.alpha = 1;
            }

            Vector2 screenPos = GameMaster.Instance.Gameplay.GamePlayCamera.WorldToScreenPoint(_target.transform.position);
            _rt.position = screenPos + _offset;
        }

        public EnemyHpController TurnOn(EnemyController target)
        {
            gameObject.SetActive(true);
            _target = target;
            _canvas.alpha = 0;

            return this;
        }

        public void TurnOff()
        {
            gameObject.SetActive(false);
            GameMaster.Instance.Gameplay.ActivePoolingSystem.ReturnObject(gameObject);
        }
    }
}

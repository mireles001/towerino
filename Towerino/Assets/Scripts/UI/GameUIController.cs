using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Towerino
{
    // User interface handler, it doesn't have Update. It is only a call to action funtions holder.
    public class GameUIController : MonoBehaviour
    {
        public bool IsBuySellModalOpen { get; private set; }

        [SerializeField]
        private float _panelTweenDuration = 0.25f;

        [SerializeField, Space, Header("Main Wrappers")]
        private GameObject _quitPanel = null;
        [SerializeField]
        private GameObject _buySellPanel = null;

        [SerializeField, Space, Header("Buy Panel")]
        private GameObject _buyPanel = null;
        [SerializeField]
        private Button _buyBallista = null;
        [SerializeField]
        private TMP_Text _ballistaPrice = null;
        [SerializeField]
        private Button _buyCannon = null;
        [SerializeField]
        private TMP_Text _cannonPrice = null;
        [SerializeField]
        private Button _buyFirebomb = null;
        [SerializeField]
        private TMP_Text _firebombPrice = null;

        [SerializeField, Space, Header("Sell Panel")]
        private GameObject _sellPanel = null;
        [SerializeField]
        private GameObject _ballistaImage = null;
        [SerializeField]
        private GameObject _cannonImage = null;
        [SerializeField]
        private GameObject _firebombImage = null;
        [SerializeField]
        private TMP_Text _sellPrice = null;
        [SerializeField]
        private TMP_Text _sellTowerName = null;

        [SerializeField, Space, Header("Life and Timer")]
        private CanvasGroup _healthPanel = null;
        [SerializeField]
        private GameObject[] _healthHearts = new GameObject[0];
        [SerializeField]
        private CanvasGroup _timerPanel = null;
        [SerializeField]
        private Image _timerBar = null;
        [SerializeField]
        private Color _startColor = Color.cyan;
        [SerializeField]
        private Color _endColor = Color.cyan;
        [SerializeField]
        private TMP_Text _levelWaveText = null;
        [SerializeField]
        private TMP_Text _moneyText = null;
        [SerializeField]
        private CanvasGroup _announcer = null;
        [SerializeField]
        private TMP_Text _announcerLevel = null;
        [SerializeField]
        private TMP_Text _announcerWave = null;

        [SerializeField, Space, Header("HUD Prefabs")]
        private GameObject _enemyHpBar = null;
        [SerializeField]
        private Transform _enemiesHpContainer = null;

        private bool _quitToggle, _quitToggleTweening;
        private GameController _main;

        // Only used when Game scene is loaded. Hides, sets in 0 or inactive everything that 
        // needs to be hidden at the beginning.
        private void Start()
        {
            _quitPanel.transform.localScale = Vector3.zero;
            _quitPanel.SetActive(false);

            _buySellPanel.transform.localScale = Vector3.zero;
            _buySellPanel.SetActive(false);

            _healthPanel.alpha = 0;
            _timerPanel.alpha = 0;
            _announcer.alpha = 0;

            _buyBallista.onClick.AddListener(delegate { _main.BuyTower((TowerType.ballistaTower)); });
            _buyCannon.onClick.AddListener(delegate { _main.BuyTower((TowerType.cannonTower)); });
            _buyFirebomb.onClick.AddListener(delegate { _main.BuyTower((TowerType.fireBombTower)); });
        }

        public GameUIController StartUp(GameController main)
        {
            _main = main;
            TowerData data;

            data = (TowerData)_main.GetTowerData(TowerType.ballistaTower);
            _ballistaPrice.text = $"${data.BuyPrice}";
            data = (TowerData)_main.GetTowerData(TowerType.cannonTower);
            _cannonPrice.text = $"${data.BuyPrice}";
            data = (TowerData)_main.GetTowerData(TowerType.fireBombTower);
            _firebombPrice.text = $"${data.BuyPrice}";

            return this;
        }

        public void ToggleQuit()
        {
            if (_quitToggleTweening) return;

            _quitToggleTweening = true;
            _quitToggle = !_quitToggle;

            if (_quitToggle) _quitPanel.SetActive(true);

            Vector3 goTo = _quitToggle ? Vector3.one : Vector3.zero;
            LeanTweenType easeType = _quitToggle ? LeanTweenType.easeOutBack : LeanTweenType.easeInQuad;

            LeanTween.scale(_quitPanel, goTo, _panelTweenDuration).setEase(easeType).setOnComplete(() =>
            {
                if (!_quitToggle) _quitPanel.SetActive(false);
                _quitToggleTweening = false;
            });
        }

        public void OpenBuySell(TowerBaseController towerBase)
        {
            if (towerBase.HasTower)
            {
                _buyPanel.SetActive(false);
                _sellPanel.SetActive(true);
                TowerData data = (TowerData)_main.GetTowerData(towerBase.TowerType);

                _ballistaImage.SetActive(data.TowerType == TowerType.ballistaTower);
                _cannonImage.SetActive(data.TowerType == TowerType.cannonTower);
                _firebombImage.SetActive(data.TowerType == TowerType.fireBombTower);

                _sellPrice.text = $"${data.SellPrice}";
                _sellTowerName.text = $"{data.Name}";
            }
            else
            {
                _sellPanel.SetActive(false);
                _buyPanel.SetActive(true);
            }

            if (!IsBuySellModalOpen)
            {
                _buySellPanel.SetActive(true);
                LeanTween.scale(_buySellPanel, Vector3.one, _panelTweenDuration).setEase(LeanTweenType.easeOutBack);
            }
            else
            {
                LeanTween.scale(_buySellPanel, Vector3.one * 1.1f, _panelTweenDuration / 2).setOnComplete(() =>
                {
                    LeanTween.scale(_buySellPanel, Vector3.one, _panelTweenDuration / 2).setEase(LeanTweenType.easeOutQuad);
                });
            }
            
            IsBuySellModalOpen = true;
        }

        public void CloseBuySell()
        {
            IsBuySellModalOpen = false;

            LeanTween.scale(_buySellPanel, Vector3.zero, _panelTweenDuration).setEase(LeanTweenType.easeInQuad).setOnComplete(() => { _buySellPanel.SetActive(false); });
        }

        public void ShowHeadStartTimer()
        {
            _timerBar.rectTransform.localScale = Vector3.one;
            LeanTween.value(_timerPanel.gameObject, 0, 1, 0.33f).setOnUpdate((float val) => { _timerPanel.alpha = val; });
        }

        public void HideHeadStartTimer()
        {
            LeanTween.value(_timerPanel.gameObject, 1, 0, 0.33f).setOnUpdate((float val) => { _timerPanel.alpha = val; });
        }

        public void UpdateHeadStartTimer(float progress)
        {
            _timerBar.rectTransform.localScale = new Vector3(Mathf.Lerp(1, 0, progress), 1, 1);
            _timerBar.color = Color.Lerp(_startColor, _endColor, progress);
        }

        public void ShowHealthMeter()
        {
            _healthPanel.alpha = 0;
            LeanTween.value(_healthPanel.gameObject, 0, 1, 1).setOnUpdate((float val) => { _healthPanel.alpha = val; });
        }

        public void HideHealthMeter() { _healthPanel.alpha = 0; }

        public void UpdateHealthMeter(int currentHelth)
        {
            for (int i = 0; i < _healthHearts.Length; i++)
            {
                _healthHearts[i].SetActive(i < currentHelth);
            }
        }

        public void UpdateMoney(int val) { _moneyText.text = $"${val}"; }

        public void UpdateLevelText(string val) { _levelWaveText.text = val; }

        public void ShowAnnouncer(int level, int wave)
        {
            _announcerLevel.text = $"Level {level}";
            _announcerWave.text = $"Wave {wave}";

            _announcer.alpha = 0;
            _announcer.gameObject.SetActive(true);
            LeanTween.value(_announcer.gameObject, 0, 1, 0.4f).setOnUpdate((float val) =>
            {
                _announcer.alpha = val;
            }).setOnComplete(() =>
            {
                LeanTween.value(_announcer.gameObject, 1, 0, 0.6f).setOnUpdate((float val) =>
                {
                    _announcer.alpha = val;
                }).setOnComplete(() =>
                {
                    _announcer.gameObject.SetActive(false);
                }).setDelay(1);
            });
        }

        // TODO: Creating floating +$$$ UI
        public void RewardMoney(int money, Vector3 position)
        {
            Debug.Log("Add floating +$$$ on top of focused item");
        }

        public void SetupEnemyHp(EnemyController enemy)
        {
            enemy.SetHpBar(GameMaster.Instance.Gameplay.ActivePoolingSystem.GetObject(_enemyHpBar, _enemiesHpContainer.transform).GetComponent<EnemyHpController>().TurnOn(enemy));
        }
    }
}

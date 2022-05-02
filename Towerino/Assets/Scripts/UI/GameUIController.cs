using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Towerino
{
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
        private CanvasGroup _health = null;
        [SerializeField]
        private TMP_Text _levelWaveText = null;
        [SerializeField]
        private TMP_Text _moneyText = null;

        private bool _quitToggle, _quitToggleTweening;
        private GameController _main;

        private void Start()
        {
            _quitPanel.transform.localScale = Vector3.zero;
            _quitPanel.SetActive(false);

            _buySellPanel.transform.localScale = Vector3.zero;
            _buySellPanel.SetActive(false);

            _buyBallista.onClick.AddListener(delegate { BuyButtonHandler(TowerType.ballistaTower); });
            _buyCannon.onClick.AddListener(delegate { BuyButtonHandler(TowerType.cannonTower); });
            _buyFirebomb.onClick.AddListener(delegate { BuyButtonHandler(TowerType.fireBombTower); });
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

        private void BuyButtonHandler(TowerType towerType)
        {
            _main.BuyTower(towerType);
        }

        public void ShowHeadStartTimer()
        {

        }

        public void HideHeadStartTimer()
        {

        }

        public void UpdateHeadStartTimer(float progress)
        {

        }

        public void ShowHealthMeter()
        {

        }

        public void HideHealthMeter()
        {

        }

        public void UpdateHealthMeter(int currentHelth)
        {

        }

        public void UpdateMoney(int val)
        {
            _moneyText.text = $"${val}";
        }

        public void UpdateLevelText(string val)
        {
            _levelWaveText.text = val;
        }

        public void RewardMoney(int money, Vector3 position)
        {

        }
    }
}

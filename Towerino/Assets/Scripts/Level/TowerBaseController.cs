using UnityEngine;

namespace Towerino
{
    // Tower base are the small available spots in the map for the player to build towers
    // This controller is the one in charge of keeping a collider for the player to click on.
    public class TowerBaseController : MonoBehaviour
    {
        public bool HasTower { get { return _currentTower != null; } }
        public TowerType TowerType { get { return _currentTower.TowerType; } }

        [SerializeField]
        private GameObject _baseMesh = null;
        [SerializeField]
        private GameObject _selectFx = null;
        [SerializeField]
        private BoxCollider _collider = null;
        [SerializeField]
        private float _colliderSizeIncreased = 2;

        private TowerController _currentTower;
        private Vector3 _center, _size;

        private void Start()
        {
            _center = _collider.center;
            _size = _collider.size;
            _baseMesh.transform.localScale = Vector3.one;
            _selectFx.transform.localScale = Vector3.one - Vector3.up;
        }

        public void Select()
        {
            ModifySelectedFx(true);
        }

        public void Deselect()
        {
            ModifySelectedFx(false);
        }

        // When the player BUYS a tower we call this function, increasing the collider size to
        // it matches the tower size and (very important) TurnOn the new tower!
        public void SetTower(TowerController tower)
        {
            _collider.center = _center + Vector3.up * (_colliderSizeIncreased / 2);
            _collider.size = _size + Vector3.up * _colliderSizeIncreased;
            _currentTower = tower;
            _currentTower.TurnOn(this);
            Deselect();
        }

        // Player SOLD tower, we TurnOff tower, returning it to the PoolingSystem, and then release itself.
        public void UnsetTower()
        {
            _collider.center = _center;
            _collider.size = _size;

            _currentTower.TurnOff();
            _currentTower = null;
            Deselect();
        }

        // Returns a vector where this transform is with a vertical increase of two metters
        // (will be used in Reward money HUD elements)
        public Vector3 GetHUDPosition()
        {
            if (_currentTower)
            {
                return _currentTower.transform.position + Vector3.up * 2;
            }
            else return transform.position;
        }

        // FX cylinder to show visual indicator when tower base is selected.
        private void ModifySelectedFx(bool isSelected)
        {
            if (isSelected)
            {
                LeanTween.scale(_selectFx, Vector3.one, 0.2f).setEase(LeanTweenType.easeOutQuad);
            }
            else
            {
                LeanTween.scale(_selectFx, Vector3.one - Vector3.up, 0.15f).setEase(LeanTweenType.easeInQuad);
            }
        }
    }
}

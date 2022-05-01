using UnityEngine;

namespace Towerino
{
    public class DestinationController : MonoBehaviour
    {
        [SerializeField] private LevelController _levelController = null;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag.Equals("Enemy"))
            {
                EnemyController enemy = other.gameObject.GetComponent<EnemyController>();

                if (enemy != null && !enemy.ReachedDestination)
                    _levelController.EnemyReachedDestination(enemy.SetReachedDestination(true));
            }
        }
    }
}

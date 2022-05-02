using UnityEngine;

namespace Towerino
{
    public class DestinationController : MonoBehaviour
    {
        [SerializeField]
        private LevelController _levelController = null;

        // Destination checks if enemies are entering triggering zone
        // This is much better than having each enemy checking things backwards.
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

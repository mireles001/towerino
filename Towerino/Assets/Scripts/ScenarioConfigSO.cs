using UnityEngine;

namespace Towerino
{
    // Simple lighting variables to use in each different lvl
    [CreateAssetMenu(fileName = "NewScenarioConfig", menuName = "Towerino/ScenarioConfig")]
    public class ScenarioConfigSO : ScriptableObject
    {
        public Vector3 lightRotation;
        public Color lightColor;
        [ColorUsage(true, true)] public Color skyColor;
        [ColorUsage(true, true)] public Color midColor;
        [ColorUsage(true, true)] public Color lowColor;
    }
}

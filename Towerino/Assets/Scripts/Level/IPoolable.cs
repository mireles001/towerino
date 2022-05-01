using UnityEngine;

namespace Towerino
{
    public interface IPoolable
    {
        GameObject SetIndex(int i);
        int GetIndex();
    }
}

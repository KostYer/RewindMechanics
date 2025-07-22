using UnityEngine;

namespace RewindSystem
{
    public abstract class Rewindable: MonoBehaviour
    {
        public virtual void StartRewind()
        {
        }
        
        public virtual void StopRewind()
        {
        }
    }
}
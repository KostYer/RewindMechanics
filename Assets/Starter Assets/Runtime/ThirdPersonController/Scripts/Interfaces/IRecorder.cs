using System.Collections.Generic;

namespace StarterAssets.Interfaces
{
    public interface IRecorder<TSnapshot>
    {
        void StartRecording();
        void StopRecording();
        void Clear();
        List<TSnapshot> GetSnapshots();
    }
}
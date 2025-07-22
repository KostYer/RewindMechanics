using System.Collections.Generic;

namespace StarterAssets.Interfaces
{
    public interface IRecorder<TSnapshot>
    {
        float MaxDuration { get; }
        void StartRecording();
        void StopRecording();
        void Clear();
        List<TSnapshot> GetSnapshots();
    }
}
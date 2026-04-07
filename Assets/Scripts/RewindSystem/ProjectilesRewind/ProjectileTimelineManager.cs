using System.Collections;
using System.Collections.Generic;
using Abilities;
using Recorders;
using StarterAssets.ScriptableObjects;
using UnityEngine;

public class ProjectileTimelineManager : MonoBehaviour
{
    [SerializeField] private RewindSettingsSO _rewindSettings;

    private readonly Dictionary<int, ProjectileTrack> _tracksById = new();
    private int _nextTrackId = 1;

    public ProjectileTrack RegisterProjectile(Projectile projectile, Rigidbody rb, float spawnTime)
    {
        int trackId = _nextTrackId++;
        var recorder = new RigidbodyRecorder(rb, _rewindSettings.MaxTimeRecord);

        var track = new ProjectileTrack(trackId, spawnTime, recorder, projectile);
        _tracksById.Add(trackId, track);

        recorder.StartRecording();

        return track;
    }

    public void MarkProjectileDestroyed(int trackId, float deathTime)
    {
        if (!_tracksById.TryGetValue(trackId, out var track))
        {
            Debug.LogError($"ProjectileTrack not found. Id: {trackId}");
            return;
        }

        if (track.IsDestroyed)
        {
            return;
        }

        track.MarkDestroyed(deathTime);
        track.Recorder.StopRecording();
        track.ClearRuntimeProjectile();
    }

    public bool TryGetTrack(int trackId, out ProjectileTrack track)
    {
        return _tracksById.TryGetValue(trackId, out track);
    }
}
 

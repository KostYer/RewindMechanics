using Abilities;
using Recorders;
using UnityEngine;

public class ProjectileTrack : MonoBehaviour
{
    public int Id { get; }
        
    public float SpawnTime { get; }
    public float? DeathTime { get; private set; }

    public RigidbodyRecorder Recorder { get; }

    public Projectile RuntimeProjectile { get; private set; }

    public bool IsDestroyed => DeathTime.HasValue;

    public ProjectileTrack(int id, float spawnTime, RigidbodyRecorder recorder, Projectile runtimeProjectile)
    {
        Id = id;
        SpawnTime = spawnTime;
        Recorder = recorder;
        RuntimeProjectile = runtimeProjectile;
    }

    public void MarkDestroyed(float deathTime)
    {
        DeathTime = deathTime;
    }

    public void BindRuntimeProjectile(Projectile projectile)
    {
        RuntimeProjectile = projectile;
    }

    public void ClearRuntimeProjectile()
    {
        RuntimeProjectile = null;
    }

    public bool ShouldExistAt(float time)
    {
        if (time < SpawnTime)
        {
            return false;
        }

        if (!DeathTime.HasValue)
        {
            return true;
        }

        return time <= DeathTime.Value;
    }
}


using UnityEngine;

namespace Snapshots
{
    public struct RbSnapshot
    {
            public float Time;
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Velocity;
            public Vector3 AngularVelocity;

            public RbSnapshot(Rigidbody rb)
            {
                Time = 0;
                Position = rb.position;
                Rotation = rb.rotation;
                Velocity = rb.velocity;
                AngularVelocity = rb.angularVelocity;
            }
    }
}
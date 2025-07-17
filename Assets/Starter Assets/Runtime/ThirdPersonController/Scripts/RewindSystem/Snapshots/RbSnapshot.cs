using UnityEngine;

namespace Snapshots
{
    public struct RbSnapshot
    {
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Velocity;
            public Vector3 AngularVelocity;

            public RbSnapshot(Rigidbody rb)
            {
                Position = rb.position;
                Rotation = rb.rotation;
                Velocity = rb.velocity;
                AngularVelocity = rb.angularVelocity;
            }
    }
}
using UnityEngine;

namespace Ascension.Networking
{

    public class NetworkTransform
    {
        public System.Int32 PropertyIndex;
        public Transform Render;
        public Transform Simulate;
        public System.Func<AscensionEntity, Vector3, Vector3> Clamper = (entity, position) => position;

        public DoubleBuffer<Vector3> RenderDoubleBufferPosition;
        public DoubleBuffer<Quaternion> RenderDoubleBufferRotation;

        public Transform Transform
        {
            get { return Simulate; }
        }

        public Vector3 Position
        {
            get { return RenderDoubleBufferPosition.Current; }
        }

        public Quaternion Rotation
        {
            get { return RenderDoubleBufferRotation.Current; }
        }

        public void SetExtrapolationClamper(System.Func<AscensionEntity, Vector3, Vector3> clamper)
        {
            NetAssert.NotNull(clamper);
            Clamper = clamper;
        }

        [System.Obsolete("For setting the transform to replicate in Attached use the new IState.SetTransforms method instead, for changing the transform after it's been set use the new ChangeTransforms method")]
        public void SetTransforms(Transform simulate)
        {
            ChangeTransforms(simulate, null);
        }

        [System.Obsolete("For setting the transform to replicate in Attached use the new IState.SetTransforms method instead, for changing the transform after it's been set use the new ChangeTransforms method")]
        public void SetTransforms(Transform simulate, Transform render)
        {
            ChangeTransforms(simulate, render);
        }

        public void ChangeTransforms(Transform simulate)
        {
            ChangeTransforms(simulate, null);
        }

        public void ChangeTransforms(Transform simulate, Transform render)
        {
            if (render)
            {
                Render = render;
                RenderDoubleBufferPosition = DoubleBuffer<Vector3>.InitBuffer(simulate.position);
                RenderDoubleBufferRotation = DoubleBuffer<Quaternion>.InitBuffer(simulate.rotation);
            }
            else {
                Render = null;
                RenderDoubleBufferPosition = DoubleBuffer<Vector3>.InitBuffer(Vector3.zero);
                RenderDoubleBufferRotation = DoubleBuffer<Quaternion>.InitBuffer(Quaternion.identity);
            }

            Simulate = simulate;
        }

        internal void SetTransformsInternal(Transform simulate, Transform render)
        {
            if (render)
            {
                Render = render;
                RenderDoubleBufferPosition = DoubleBuffer<Vector3>.InitBuffer(simulate.position);
                RenderDoubleBufferRotation = DoubleBuffer<Quaternion>.InitBuffer(simulate.rotation);
            }
            else {
                Render = null;
                RenderDoubleBufferPosition = DoubleBuffer<Vector3>.InitBuffer(Vector3.zero);
                RenderDoubleBufferRotation = DoubleBuffer<Quaternion>.InitBuffer(Quaternion.identity);
            }

            Simulate = simulate;
        }
    }
}

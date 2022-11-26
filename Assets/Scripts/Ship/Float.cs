using UnityEngine;

namespace Ship {
    public class Float : MonoBehaviour {
        public Rigidbody rb;

        public float maxDepth = 1f;
        public float displacementForce = 1f;
        public float floatCount = 1f;
        public float waterDrag = 1f;
        public float waterAngularDrag = 1f;
        LowPolyWater.LowPolyWater _water;

        void Awake() {
            _water = FindObjectOfType<LowPolyWater.LowPolyWater>();
        }

        void FixedUpdate() {
            var position = transform.position;
            rb.AddForceAtPosition(Physics.gravity / floatCount, position, ForceMode.Acceleration);
            var waveYAt = _water.GetWaveYAt(position);
            if (!(position.y < waveYAt)) return;
            var displacement = Mathf.Clamp01((waveYAt - position.y) / maxDepth) * displacementForce;
            rb.AddForceAtPosition(new Vector3(0f, -1 * Physics.gravity.y * displacement, 0f), position,
                ForceMode.Acceleration);
            rb.AddForce(displacement * -rb.velocity * waterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
            rb.AddTorque(displacement * -rb.angularVelocity * waterAngularDrag * Time.fixedDeltaTime,
                ForceMode.VelocityChange);
        }
    }
}
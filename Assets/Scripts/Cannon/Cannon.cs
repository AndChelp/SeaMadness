using Common;
using Mirror;
using UnityEngine;

namespace Cannon
{
    [RequireComponent(typeof(LineRenderer))]
    public class Cannon : MonoBehaviour
    {
        private const float MaxRotationAngle = 60f;

        private LineRenderer _lineRenderer;

        private float _launchSpeed = 50f;
        private float _g = 9.81f;
        private double _lastShotTime;

        public Transform shipTransform;
        public Transform launchPointTransform;
        public GameObject cannonball;

        public Side side;
        public float rotationSpeed = 1f;
        public float maxAimDistance = 100f;
        public float minAimDistance = 10f;
        public double cooldown = 2;

        private Rigidbody _shipRigidbody;

        private void Awake()
        {
            _shipRigidbody = shipTransform.GetComponent<Rigidbody>();
            _lineRenderer = GetComponent<LineRenderer>();
            _g = -Physics.gravity.y;
            var x = maxAimDistance + 0.25001f;
            var y = -launchPointTransform.position.y;
            _launchSpeed = Mathf.Sqrt(9.81f * (y + Mathf.Sqrt(x * x + y * y)));
        }

        public void InitCannonball(uint netId)
        {
            if (!(NetworkTime.time - _lastShotTime >= cooldown)) return;
            var newBall = Instantiate(cannonball, launchPointTransform.position, launchPointTransform.rotation);
            newBall.GetComponent<CannonBall>().ownerNetId = netId;
            var forward = launchPointTransform.forward;
            var shipRigidbodyVelocity = (side == Side.Left ? -1 : 1) * Vector3.Dot(shipTransform.forward, forward) *
                                        _shipRigidbody.velocity;
            newBall.GetComponent<Rigidbody>().velocity = forward * _launchSpeed + shipRigidbodyVelocity;
            _lastShotTime = NetworkTime.time;
        }

        public void ShowPredicateLine(bool showPredicate)
        {
            _lineRenderer.enabled = showPredicate;
        }

        public bool IsShowPredicateLine()
        {
            return _lineRenderer.enabled;
        }

        public bool CanAim(Vector3 point)
        {
            var distance = Vector3.Distance(point, transform.position);
            if (distance < minAimDistance || distance > maxAimDistance) return false;
            var direction = (point - transform.position).normalized;
            direction.y = 0;
            var angle = Quaternion.Angle(shipTransform.rotation, Quaternion.LookRotation(direction));
            return side == Side.Left && angle > (180f - MaxRotationAngle) ||
                   side == Side.Right && angle < MaxRotationAngle;
        }

        public void Aim(Vector3 point)
        {
            var launchPosition = launchPointTransform.position;
            var targetDistance = point - launchPosition;
            var x = new Vector2(targetDistance.x, targetDistance.z).magnitude;
            var y = -launchPosition.y;
            var s2 = _launchSpeed * _launchSpeed;
            var r = s2 * s2 - _g * (_g * x * x + 2f * y * s2);
            var tanTheta = (s2 - Mathf.Sqrt(r)) / (_g * x);
            var cosTheta = Mathf.Cos(Mathf.Atan(tanTheta));
            var sinTheta = cosTheta * tanTheta;
            var direction = Vector3.MoveTowards(transform.rotation * Vector3.forward, targetDistance,
                rotationSpeed * Time.fixedDeltaTime);
            direction.y = 0;
            DrawPredicate(cosTheta, sinTheta, direction, launchPosition, point);
            Turn(direction, sinTheta);
        }

        private void DrawPredicate(float cosTheta, float sinTheta, Vector3 direction, Vector3 launchPosition,
            Vector3 targetPosition)
        {
            _lineRenderer.positionCount = (int) (Vector3.Distance(targetPosition, launchPosition) * 0.2f);
            for (var i = 0; i < _lineRenderer.positionCount; i++)
            {
                var t = i / 10f;
                var dx = _launchSpeed * cosTheta * t;
                var dy = _launchSpeed * sinTheta * t - 0.5f * _g * t * t;
                var next = launchPosition + new Vector3(direction.x * dx, dy, direction.z * dx);
                _lineRenderer.SetPosition(i, next);
            }
        }

        private void Turn(Vector3 direction, float angle)
        {
            var lookRotation = Quaternion.LookRotation(direction) *
                               Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.left);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotationSpeed);
        }
    }
}
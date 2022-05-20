using System;
using Cannon.Cannonball;
using Common;
using Manager;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Cannon {
    [RequireComponent(typeof(LineRenderer))]
    public class Cannon : MonoBehaviour {
        private const float MaxRotationAngle = 60f;
        private const float RotationSpeed = 10f;

        private LineRenderer _lineRenderer;

        private float _g = 9.81f;
        private double _lastShotTime;


        public Transform shipTransform;
        public Transform launchPointTransform;
        public ParticleSystem shotExplosionParticles;

        public Side side;
        public float minAimDistance = 10f;

        private Rigidbody _shipRb;

        private Quaternion _initRotation;
        private AbstractCannonball _currentCb;
        public int currentCbRId { get; private set; }

        public bool isCharged;

        private void Awake() {
            _shipRb = shipTransform.GetComponent<Rigidbody>();
            _lineRenderer = GetComponent<LineRenderer>();
            _g = -Physics.gravity.y;
            _initRotation = transform.rotation;
        }

        public void LaunchCannonball(uint netId, int cbRId) {
            //if (!(NetworkTime.time - _lastShotTime >= _currentCannonball.cooldown)) return;
            var cannonball = ResourceManager.Instance.GetCannonball(cbRId);
            Debug.Log("LaunchCannonball " + cannonball.GetType().Name + ", ownerId = " + netId);
            var newBall = Instantiate(cannonball, launchPointTransform.position, launchPointTransform.rotation);
            newBall.GetComponent<AbstractCannonball>().ownerNetId = netId;
            var forward = launchPointTransform.forward;
            var shipRbVelocity = (side == Side.Left ? -1 : 1) * Vector3.Dot(shipTransform.forward, forward) *
                                 _shipRb.velocity;
            var component = newBall.GetComponent<Rigidbody>();
            component.velocity = forward * cannonball.velocity + shipRbVelocity;
            component.angularVelocity =
                new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10));
            _lastShotTime = NetworkTime.time;
            shotExplosionParticles.Play();
            isCharged = false;
        }


        public void ShowPredicateLine(bool showPredicate) {
            _lineRenderer.enabled = showPredicate;
        }

        public bool IsShowPredicateLine() {
            return _lineRenderer.enabled;
        }

        public bool IsInDiapason(Vector3 point) {
            var distance = Vector3.Distance(point, transform.position);
            if (distance < minAimDistance){
                return false;
            }

            var direction = (point - transform.position).normalized;
            direction.y = 0;
            return ByAngle(direction);
        }

        public bool CanAim(Vector3 point) {
            if (!isCharged){
                return false;
            }

            var distance = Vector3.Distance(point, transform.position);
            if (distance < minAimDistance || distance > _currentCb.maxDistance) return false;
            var direction = (point - transform.position).normalized;
            direction.y = 0;
            return ByAngle(direction);
        }

        private bool ByAngle(Vector3 direction) {
            var angle = Quaternion.Angle(shipTransform.rotation * _initRotation, Quaternion.LookRotation(direction));
            return angle < MaxRotationAngle;
        }

        public void Aim(Vector3 point) {
            var launchPosition = launchPointTransform.position;
            var targetDistance = point - launchPosition;
            var x = new Vector2(targetDistance.x, targetDistance.z).magnitude;
            var y = -launchPosition.y;
            var s2 = _currentCb.velocity * _currentCb.velocity;
            var r = s2 * s2 - _g * (_g * x * x + 2f * y * s2);
            var tanTheta = (s2 - Mathf.Sqrt(r)) / (_g * x);
            var cosTheta = Mathf.Cos(Mathf.Atan(tanTheta));
            var sinTheta = cosTheta * tanTheta;
            var direction = Vector3.MoveTowards(transform.rotation * Vector3.forward, targetDistance,
                RotationSpeed * Time.fixedDeltaTime);
            direction.y = 0;
            DrawPredicate(cosTheta, sinTheta, direction, launchPosition, point);
            Turn(direction, sinTheta);
        }

        private void DrawPredicate(float cosTheta, float sinTheta, Vector3 direction, Vector3 launchPosition,
            Vector3 targetPosition) {
            _lineRenderer.positionCount = (int) (Vector3.Distance(targetPosition, launchPosition) * 0.2f);
            for (var i = 0; i < _lineRenderer.positionCount; i++){
                var t = i / 10f;
                var dx = _currentCb.velocity * cosTheta * t;
                var dy = _currentCb.velocity * sinTheta * t - 0.5f * _g * t * t;
                var next = launchPosition + new Vector3(direction.x * dx, dy, direction.z * dx);
                _lineRenderer.SetPosition(i, next);
            }
        }

        private void Turn(Vector3 direction, float angle) {
            var lookRotation = Quaternion.LookRotation(direction) *
                               Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.left);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, RotationSpeed);
        }

        public int DischargeCannonball() {
            isCharged = false;
            return currentCbRId;
        }

        public int RechargeCannonball(int cbRId) {
            currentCbRId = cbRId;
            _currentCb = ResourceManager.Instance.GetCannonball(cbRId);
            isCharged = true;
            Debug.Log("Recharging: " + _currentCb.GetType().Name);
            return currentCbRId;
        }
    }
}
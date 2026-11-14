using PrimeTween;
using UnityEngine;

namespace Game.Character
{
    /// <summary>
    /// Smooth top-down orthographic camera rig with PrimeTween impulse shake on a child transform.
    /// </summary>
    public sealed class TopDownCameraRig : MonoBehaviour
    {
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private Camera _camera;
        [SerializeField] private float _followSmoothTime = 0.12f;
        [SerializeField] private float _orthographicSize = 7.5f;

        private Transform _target;
        private Vector3 _velocity;
        private Tween _shakeTween;
        private Tween _zoomTween;

        public Camera MainCamera => _camera;

        public void Construct(Transform target)
        {
            _target = target;
            if (_camera != null)
            {
                _camera.orthographic = true;
                _camera.orthographicSize = _orthographicSize;
            }

            SnapToTarget();
        }

        private void OnDestroy()
        {
            _shakeTween.Stop();
            _zoomTween.Stop();
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            Vector3 desired = new Vector3(_target.position.x, _target.position.y, -10f);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, _followSmoothTime);
        }

        public void SnapToTarget()
        {
            if (_target == null)
            {
                return;
            }

            _velocity = Vector3.zero;
            transform.position = new Vector3(_target.position.x, _target.position.y, -10f);
        }

        public void Shake(float strength = 0.25f, float duration = 0.22f)
        {
            if (_cameraTransform == null)
            {
                return;
            }

            _shakeTween.Stop();
            _cameraTransform.localPosition = Vector3.zero;
            _shakeTween = Tween.ShakeLocalPosition(
                _cameraTransform,
                new ShakeSettings(new Vector3(strength, strength, 0f), duration, frequency: 24f));
        }

        public void AnimateOrthographicSize(float targetSize, float duration)
        {
            if (_camera == null)
            {
                return;
            }

            _zoomTween.Stop();
            _zoomTween = Tween.CameraOrthographicSize(_camera, targetSize, duration, Ease.InOutSine);
        }
    }
}

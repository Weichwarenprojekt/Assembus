﻿using System.Collections;
using UnityEngine;

namespace MainScreen
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        /// <summary>
        ///     Time frame for double click detection
        /// </summary>
        private const float TimeBetweenClicks = 0.25f;

        /// <summary>
        ///     Rotation speed of the camera
        /// </summary>
        private const float RotationSpeed = 200f;

        /// <summary>
        ///     The factor between scroll speed and distance of the camera
        /// </summary>
        private const float ScrollFactor = 0.05f;

        /// <summary>
        ///     Reference to the main camera
        /// </summary>
        private Camera _cam;

        /// <summary>
        ///     Distance from the camera to the object
        /// </summary>
        private float _cameraDistance = 200f;

        /// <summary>
        ///     The transform object of the camera
        /// </summary>
        private Transform _camTransform;

        /// <summary>
        ///     Point the camera rotates around
        /// </summary>
        private Vector3 _centerPoint;

        /// <summary>
        ///     Allow coroutine for double click detection
        /// </summary>
        private bool _coroutineAllowed = true;

        /// <summary>
        ///     Time when left mouse button is clicked first time
        /// </summary>
        private float _firstLeftClickTime;

        /// <summary>
        ///     Left mouse click counter for double click detection
        /// </summary>
        private int _leftClickCounter;

        /// <summary>
        ///     Camera position of previous frame. Used to calculate the new rotation
        /// </summary>
        private Vector3 _prevPosition;

        /// <summary>
        ///     Scroll/Zoom speed of the camera
        /// </summary>
        private float _scrollSpeed = 20f;

        /// <summary>
        ///     Called on the frame when script is enabled
        /// </summary>
        private void Start()
        {
            _cam = Camera.main;
            if (!(_cam is null)) _camTransform = _cam.transform;
            _centerPoint = new Vector3(0, 0, 0);

            // this is needed! without this the camera "snaps" to another location on first right click
            StoreLastMousePosition();
            CalculateNewCameraTransform();
        }

        /// <summary>
        ///     Called after all update methods have been called
        /// </summary>
        private void LateUpdate()
        {
            // store position when right mouse button is clicked
            if (Input.GetMouseButtonDown(1)) StoreLastMousePosition();

            // when right mouse button is held rotate the camera
            if (Input.GetMouseButton(1)) CalculateNewCameraTransform();

            // Focus camera if game object is double clicked
            if (Input.GetMouseButtonUp(0))
                _leftClickCounter += 1;

            if (_leftClickCounter == 1 && _coroutineAllowed)
            {
                _firstLeftClickTime = Time.time;
                StartCoroutine(DoubleClickDetection());
            }

            // detect scrolling
            if (Input.mouseScrollDelta.y != 0) Zoom(Input.mouseScrollDelta.y);
        }

        /// <summary>
        ///     Zoom camera on the given object
        /// </summary>
        /// <param name="parent">The object that shall be shown</param>
        public void ZoomOnObject(GameObject parent)
        {
            // Calculate the bounds of the game object
            var bounds = new Bounds(parent.transform.position, Vector3.zero);
            foreach (var r in parent.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds);
            var objectSizes = bounds.max - bounds.min;

            // Calculate the camera distance
            var objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
            _cameraDistance = 0.5f * (objectSize / Mathf.Tan(0.5f * Mathf.Deg2Rad * _cam.fieldOfView) + objectSize);
            _camTransform.position = bounds.center - _cameraDistance * _camTransform.forward;

            // Set the focus to the middle
            SetFocus(bounds.center);

            // Recalculate the scroll speed
            _scrollSpeed = ScrollFactor * _cameraDistance;
        }

        /// <summary>
        ///     Coroutine, detect double clicking
        /// </summary>
        private IEnumerator DoubleClickDetection()
        {
            _coroutineAllowed = false;
            while (Time.time < _firstLeftClickTime + TimeBetweenClicks)
            {
                if (_leftClickCounter == 2)
                {
                    UpdateCameraFocus();
                    break;
                }

                yield return new WaitForEndOfFrame();
            }

            _leftClickCounter = 0;
            _firstLeftClickTime = 0f;
            _coroutineAllowed = true;
        }

        /// <summary>
        ///     Set focus on clicked game object
        /// </summary>
        private void UpdateCameraFocus()
        {
            if (_cam is null) return;

            var ray = _cam.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out var hit, 10000)) return;

            var transformGameObject = hit.transform.gameObject.GetComponent<Renderer>().bounds.center;

            SetFocus(transformGameObject);
            StoreLastMousePosition();
            CalculateNewCameraTransform();
        }

        /// <summary>
        ///     Calculates the new camera position based on the mouse position
        /// </summary>
        private void CalculateNewCameraTransform()
        {
            var direction = _prevPosition - _cam.ScreenToViewportPoint(Input.mousePosition);

            _cam.transform.position = _centerPoint;

            _cam.transform.Rotate(Vector3.right, direction.y * RotationSpeed);
            _cam.transform.Rotate(Vector3.up, -direction.x * RotationSpeed, Space.World);
            _cam.transform.Translate(new Vector3(0, 0, -_cameraDistance));

            _prevPosition = _cam.ScreenToViewportPoint(Input.mousePosition);
        }

        /// <summary>
        ///     Stores the current mouse position in view port space when called
        /// </summary>
        private void StoreLastMousePosition()
        {
            _prevPosition = _cam.ScreenToViewportPoint(Input.mousePosition);
        }

        /// <summary>
        ///     Calculates camera distance
        /// </summary>
        private void Zoom(float delta)
        {
            // calculate camera distance
            _cameraDistance -= delta * _scrollSpeed;
            if (_cameraDistance < 0) _cameraDistance = 0;

            // apply camera distance
            StoreLastMousePosition();
            CalculateNewCameraTransform();
        }

        /// <summary>
        ///     Sets focus on given Vector3
        /// </summary>
        /// <param name="position">Vector3 to center on</param>
        private void SetFocus(Vector3 position)
        {
            _centerPoint = position;
        }
    }
}
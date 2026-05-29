using UnityEngine;
using UnityEngine.InputSystem;

namespace SazenGames.Skeleton
{
    public class TacticalCameraController : MonoBehaviour
    {
        class CameraState
        {
            public float yaw;
            public float pitch;
            public float roll;
            public float x;
            public float y;
            public float z;

            public void SetFromTransform(Transform t)
            {
                pitch = t.eulerAngles.x;
                yaw = t.eulerAngles.y;
                roll = t.eulerAngles.z;
                x = t.position.x;
                y = t.position.y;
                z = t.position.z;
            }

            public void Translate(Vector3 translation)
            {
                Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;

                x += rotatedTranslation.x;
                y += rotatedTranslation.y;
                z += rotatedTranslation.z;
            }

            public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
            {
                yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
                pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
                roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);
                
                x = Mathf.Lerp(x, target.x, positionLerpPct);
                y = Mathf.Lerp(y, target.y, positionLerpPct);
                z = Mathf.Lerp(z, target.z, positionLerpPct);
            }

            public void UpdateTransform(Transform t)
            {
                t.eulerAngles = new Vector3(pitch, yaw, roll);
                t.position = new Vector3(x, y, z);
            }
        }
        
        CameraState m_TargetCameraState = new CameraState();
        CameraState m_InterpolatingCameraState = new CameraState();

        [Header("Movement Settings")]
        [Tooltip("Time it takes to interpolate camera position 99% of the way to the target."), Range(0.001f, 1f)]
        [SerializeField] private float m_PositionLerpTime = 0.5f;

        [Tooltip("Camera Movement multiplicator for Translations"), Range(0.5f, 15f)]
        [SerializeField] private float m_TranslationSpeed = 5f;

        [Header("Rotation Settings")]
        [Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
        [SerializeField] private AnimationCurve m_RotationSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

        [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]
        [SerializeField] private float m_RotationLerpTime = 0.5f;

        [Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
        [SerializeField] private bool m_InvertY = false;

        Camera m_CameraComponent;

        private bool m_focusActionsSetUp = false;
        private bool m_ignoreInput = false;

        private InputActionMap m_CameraMovementMap;
        private InputAction m_InputActionCameraUp;
        private InputAction m_InputActionCameraDown;
        private InputAction m_InputActionCameraLeft;
        private InputAction m_InputActionCameraRight;
        private InputAction m_InputActionCameraForward;
        private InputAction m_InputActionCameraBackward;
        private InputAction m_InputActionCameraButton;
        private InputAction m_InputActionCameraRotationDelta;
        private InputAction m_InputActionCameraPointer;

        private Vector3 m_InputDirection;
        private Vector2 m_CameraPointerPosition;

        public Ray GetPointerScreenToRay()
        {
            return m_CameraComponent.ScreenPointToRay(m_CameraPointerPosition);
        }

        private void OnEnable()
        {
            m_TargetCameraState.SetFromTransform(transform);
            m_InterpolatingCameraState.SetFromTransform(transform);
            m_InputDirection = new Vector3();

            m_CameraMovementMap = InputSystem.actions.FindActionMap("CameraMovement");
            m_CameraMovementMap.Enable();

            m_InputActionCameraUp = m_CameraMovementMap.FindAction("Up");
            m_InputActionCameraDown = m_CameraMovementMap.FindAction("Down");
            m_InputActionCameraLeft = m_CameraMovementMap.FindAction("Left");
            m_InputActionCameraRight = m_CameraMovementMap.FindAction("Right");
            m_InputActionCameraForward = m_CameraMovementMap.FindAction("Forward");
            m_InputActionCameraBackward = m_CameraMovementMap.FindAction("Backward");
            m_InputActionCameraButton = m_CameraMovementMap.FindAction("CameraButton");
            m_InputActionCameraRotationDelta = m_CameraMovementMap.FindAction("CameraRotationDelta");
            m_InputActionCameraPointer = m_CameraMovementMap.FindAction("Point");

            m_CameraPointerPosition = Vector2.zero;
        }

        private void OnDisable()
        {
            m_CameraMovementMap?.Disable();
        }

        private void Start()
        {
            if (!m_focusActionsSetUp)
            {
#if UNITY_EDITOR
                var ignoreInput = new InputAction(binding: "/Keyboard/escape");
                ignoreInput.performed += context => m_ignoreInput = true;
                ignoreInput.Enable();

                var enableInput = new InputAction(binding: "/Mouse/leftButton");
                enableInput.performed += context => m_ignoreInput = false;
                enableInput.Enable();

                UnityEngine.Cursor.visible = true;
#endif
            }

            m_focusActionsSetUp = true;

            m_CameraComponent = gameObject.GetComponent<Camera>();
        }

        private void CheckPointerInputActions()
        {
            if (m_InputActionCameraPointer != null)
            {
                //Debug.Log("pointer input action : " + m_InputActionCameraPointer.ReadValue<Vector2>());
                m_CameraPointerPosition = m_InputActionCameraPointer.ReadValue<Vector2>();
            }
        }

        private void CheckCamTranslationInputActions()
        {
            m_InputDirection = Vector3.zero;

            if (m_ignoreInput)
            {
                return;
            }

            if (m_InputActionCameraUp != null && m_InputActionCameraUp.IsPressed())
            {
                m_InputDirection += Vector3.up;
            }
            if (m_InputActionCameraDown != null && m_InputActionCameraDown.IsPressed())
            {
                m_InputDirection += Vector3.down;
            }
            if (m_InputActionCameraLeft != null && m_InputActionCameraLeft.IsPressed())
            {
                m_InputDirection += Vector3.left;
            }
            if (m_InputActionCameraRight != null && m_InputActionCameraRight.IsPressed())
            {
                m_InputDirection += Vector3.right;
            }
            if (m_InputActionCameraForward != null && m_InputActionCameraForward.IsPressed())
            {
                m_InputDirection += Vector3.forward;
            }
            if (m_InputActionCameraBackward != null && m_InputActionCameraBackward.IsPressed())
            {
                m_InputDirection += Vector3.back;
            }
        }

        private Vector3 GetInputTranslationDirection()
        {
            return m_InputDirection * m_TranslationSpeed;
        }

        private void CheckCamRotationInputActions()
        {
            if (m_ignoreInput)
            {
                return;
            }

            if (m_InputActionCameraButton != null && m_InputActionCameraButton.IsPressed() && m_InputActionCameraRotationDelta != null)
            {
                Vector2 rotationAxisMovement = m_InputActionCameraRotationDelta.ReadValue<Vector2>();
                rotationAxisMovement.y *= m_InvertY ? 1 : -1;

                var rotSensitivityFactor = m_RotationSensitivityCurve.Evaluate(rotationAxisMovement.magnitude);
                m_TargetCameraState.yaw += rotationAxisMovement.x * rotSensitivityFactor;
                m_TargetCameraState.pitch += rotationAxisMovement.y * rotSensitivityFactor;
            }
        }

        //        public void OnExit(InputValue value)
        //        {
        //            if (!m_ignoreInput && value.isPressed)
        //            {
        //                Application.Quit();

        //#if UNITY_EDITOR
        //                UnityEditor.EditorApplication.isPlaying = false;
        //#endif
        //            }
        //        }


        private void Update()
        {
            CheckPointerInputActions();

            CheckCamTranslationInputActions();

            var translation = GetInputTranslationDirection() * Time.deltaTime;
            m_TargetCameraState.Translate(translation);

            CheckCamRotationInputActions();

            var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / m_PositionLerpTime) * Time.deltaTime);
            var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / m_RotationLerpTime) * Time.deltaTime);
            
            m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);
            m_InterpolatingCameraState.UpdateTransform(transform);
        }
    }

}
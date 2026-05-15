using UnityEngine; using UnityEngine.InputSystem; using UnityEngine.UIElements;  namespace SazenGames.Skeleton {     public class SimpleCameraController : MonoBehaviour     {         class CameraState         {             public float yaw;             public float pitch;             public float roll;             public float x;             public float y;             public float z;              public void SetFromTransform(Transform t)             {                 pitch = t.eulerAngles.x;                 yaw = t.eulerAngles.y;                 roll = t.eulerAngles.z;                 x = t.position.x;                 y = t.position.y;                 z = t.position.z;             }              public void Translate(Vector3 translation)             {                 Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;                  x += rotatedTranslation.x;                 y += rotatedTranslation.y;                 z += rotatedTranslation.z;             }              public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)             {                 yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);                 pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);                 roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);                                  x = Mathf.Lerp(x, target.x, positionLerpPct);                 y = Mathf.Lerp(y, target.y, positionLerpPct);                 z = Mathf.Lerp(z, target.z, positionLerpPct);             }              public void UpdateTransform(Transform t)             {                 t.eulerAngles = new Vector3(pitch, yaw, roll);                 t.position = new Vector3(x, y, z);             }         }                  CameraState m_TargetCameraState = new CameraState();         CameraState m_InterpolatingCameraState = new CameraState();          [Header("Movement Settings")]         [Tooltip("Time it takes to interpolate camera position 99% of the way to the target."), Range(0.001f, 1f)]         public float positionLerpTime = 0.2f;          [Header("Rotation Settings")]         [Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]         public AnimationCurve m_RotationSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));          [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]         public float rotationLerpTime = 0.01f;          [Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]         public bool invertY = false;           private bool m_focusActionsSetUp = false;         private bool m_ignoreInput = false;

        private InputActionMap m_CameraMovementMap;
        private InputAction m_InputActionCameraUp;
        private InputAction m_InputActionCameraDown;
        private InputAction m_InputActionCameraLeft;
        private InputAction m_InputActionCameraRight;
        private InputAction m_InputActionCameraForward;
        private InputAction m_InputActionCameraBackward;
        private InputAction m_InputActionCameraButton;
        private InputAction m_InputActionCameraRotationAxis;

        private Vector3 m_inputDirection;          void OnEnable()         {             m_TargetCameraState.SetFromTransform(transform);             m_InterpolatingCameraState.SetFromTransform(transform);             m_inputDirection = new Vector3();

#if ENABLE_INPUT_SYSTEM
            m_CameraMovementMap = InputSystem.actions.FindActionMap("CameraMovement");
            m_CameraMovementMap.Enable();

            m_InputActionCameraUp = m_CameraMovementMap.FindAction("Up");
            m_InputActionCameraDown = m_CameraMovementMap.FindAction("Down");
            m_InputActionCameraLeft = m_CameraMovementMap.FindAction("Left");
            m_InputActionCameraRight = m_CameraMovementMap.FindAction("Right");
            m_InputActionCameraForward = m_CameraMovementMap.FindAction("Forward");
            m_InputActionCameraBackward = m_CameraMovementMap.FindAction("Backward");
            m_InputActionCameraButton = m_CameraMovementMap.FindAction("CameraButton");
            m_InputActionCameraRotationAxis = m_CameraMovementMap.FindAction("CameraRotationAxis");
#endif         }

        void OnDisable()         {
#if ENABLE_INPUT_SYSTEM
            m_CameraMovementMap?.Disable();
#endif         }          private void Start()         {             if (!m_focusActionsSetUp)             {
#if UNITY_EDITOR                 var ignoreInput = new InputAction(binding: "/Keyboard/escape");                 ignoreInput.performed += context => m_ignoreInput = true;                 ignoreInput.Enable();                  var enableInput = new InputAction(binding: "/Mouse/leftButton");                 enableInput.performed += context => m_ignoreInput = false;                 enableInput.Enable();
#endif             }              m_focusActionsSetUp = true;         }  
#if !ENABLE_INPUT_SYSTEM         Vector3 GetInputTranslationDirection()         {             Vector3 direction = new Vector3();             if (Input.GetKey(KeyCode.W))             {                 direction += Vector3.forward;             }             if (Input.GetKey(KeyCode.S))             {                 direction += Vector3.back;             }             if (Input.GetKey(KeyCode.A))             {                 direction += Vector3.left;             }             if (Input.GetKey(KeyCode.D))             {                 direction += Vector3.right;             }             if (Input.GetKey(KeyCode.Q))             {                 direction += Vector3.down;             }             if (Input.GetKey(KeyCode.E))             {                 direction += Vector3.up;             }              return direction;         } #else          private void CheckTranslationInputActions()         {
            m_inputDirection = Vector3.zero;
             if (m_ignoreInput)             {                 return;             }              if (m_InputActionCameraUp != null && m_InputActionCameraUp.IsPressed())             {                 m_inputDirection += Vector3.up;             }             if (m_InputActionCameraDown != null && m_InputActionCameraDown.IsPressed())             {                 m_inputDirection += Vector3.down;             }             if (m_InputActionCameraLeft != null && m_InputActionCameraLeft.IsPressed())             {                 m_inputDirection += Vector3.left;             }             if (m_InputActionCameraRight != null && m_InputActionCameraRight.IsPressed())             {                 m_inputDirection += Vector3.right;             }             if (m_InputActionCameraForward != null && m_InputActionCameraForward.IsPressed())             {                 m_inputDirection += Vector3.forward;             }             if (m_InputActionCameraBackward != null && m_InputActionCameraBackward.IsPressed())             {                 m_inputDirection += Vector3.back;             }         }          Vector3 GetInputTranslationDirection()         {             return m_inputDirection;         }

        private void CheckRotationInputActions()         {             if (m_ignoreInput)             {                 return;             }              if (m_InputActionCameraButton != null && m_InputActionCameraButton.IsPressed() && m_InputActionCameraRotationAxis != null)             {                 Vector2 rotationAxisMovement = m_InputActionCameraRotationAxis.ReadValue<Vector2>();
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

#endif

        void Update()         {
            // Rotation
            //if (Input.GetMouseButton(1))
            //{
            //    var mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * (invertY ? 1 : -1));

            //    var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);

            //    m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
            //    m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
            //}


#if ENABLE_INPUT_SYSTEM             CheckTranslationInputActions();
          //  CheckRotationInputActions();
#endif             var translation = GetInputTranslationDirection() * Time.deltaTime;              m_TargetCameraState.Translate(translation);              var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);             var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);             
            m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);              m_InterpolatingCameraState.UpdateTransform(transform);         }     }  }
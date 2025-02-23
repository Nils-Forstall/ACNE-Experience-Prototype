using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class Player : MonoBehaviour
{
    [SerializeField, Min(0f)]
    public float movementSpeed = .25f, rotationSpeed = 0.25f;

    float detectionRange = 0.5f;
    [SerializeField]
    float startingVerticalEyeAngle = 10f;

   private bool isColliding = false; // Tracks collision state

    [SerializeField]
    public float MovementScalingFactor = 0.001f;

    [SerializeField]
    public float CameraScalingFactor = 0.001f;

    private CharacterController characterController;
    private Transform eye;
    private Vector2 eyeAngles;


    [SerializeField]     // for controller
    public string portName = "/dev/tty.usbserial-1140";
    public int baudRate = 115200;

    private SerialPort serialPort;
    private Thread serialThread;
    private bool isRunning = true;
    private float moveSpeed = 0f;
    private float cameraSpeed = 0f;
    private bool serialAvailable = false; // Check if controller exists

    private bool isMovingForward;
    private bool isMovingBackward;

    private bool isTurningLeft;
    private bool isTurningRight;

    // private readonly object dataLock = new object();

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        eye = transform.GetChild(0);
    }

    void Start()
    {
        TryOpenSerialPort();
    }

    public void StartNewGame(Vector3 position)
    {
        eyeAngles.x = Random.Range(0f, 360f);
        eyeAngles.y = startingVerticalEyeAngle;
        characterController.enabled = false;
        transform.localPosition = position;
        characterController.enabled = true;
    }

    void UpdateEyeAngles()
    {

        // Use serial camera speed if available, otherwise fallback to keyboard input
        float horizontalInput = serialAvailable ? cameraSpeed * CameraScalingFactor * rotationSpeed : Input.GetAxis("Horizontal") * rotationSpeed;

        eyeAngles.x += horizontalInput;
        // eye.localRotation = Quaternion.Euler(eyeAngles.y, eyeAngles.x, 0f);
        transform.rotation = Quaternion.Euler(0f, eyeAngles.x, 0f);

        // Determine turning direction based on input
        if (horizontalInput > 0) // Turning Right
        {
            if (!isTurningRight)
            {
                isTurningRight = true;
                isTurningLeft = false;
                TurnRight();
            }
        }
        else if (horizontalInput < 0) // Turning Left
        {
            if (!isTurningLeft)
            {
                isTurningLeft = true;
                isTurningRight = false;
                TurnLeft();
            }
        }
        else // No input, stop the sound
        {
            if (isTurningLeft || isTurningRight)
            {
                isTurningLeft = false;
                isTurningRight = false;
                StopTurnAudio();
            }
        }
    }

    void UpdatePosition()
    {
        // Use serial movement speed if available, otherwise fallback to keyboard input
        float forwardMovement = serialAvailable ? 
        moveSpeed * MovementScalingFactor * movementSpeed / 3 : Input.GetAxis("Vertical") * movementSpeed * 3;

        Vector3 movement = eye.forward * forwardMovement;
        characterController.Move(movement * Time.deltaTime);

        if (forwardMovement != 0)
        {
            if (!isMovingForward)
            {
                isMovingForward = true;
                isMovingBackward = false;
                PlayForwardAudio();
            }
        }
        else if (forwardMovement < 0)
        {
            if (!isMovingBackward)
            {
                isMovingBackward = true;
                isMovingForward = false;
                PlayBackwardAudio();
            }
        }
        else
        {
            StopAudio();
        }
    }

    void PlayForwardAudio()
    {
        AudioManager.Instance.PlaySound("ForwardBackward");
    }

    void PlayBackwardAudio()
    {
        AudioManager.Instance.PlaySound("ForwardBackward");
    }

      void TurnRight() {
        AudioManager.Instance.PlaySound("Tank Movement");
    }

    void TurnLeft() {
        AudioManager.Instance.PlaySound("Tank Movement");
    }


void StopTurnAudio() {
    AudioManager.Instance.StopSound("Tank Movement");
                isTurningLeft = false;
        isTurningRight = false;
}
    void StopAudio()
    {
        AudioManager.Instance.StopSound("ForwardBackward");
        isMovingForward = false;
        isMovingBackward = false;

    }

    // Serial Communication Setup
    void TryOpenSerialPort()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = 1;
            serialPort.Open();
            serialAvailable = true;
            Debug.Log("Serial Port Opened: " + portName);

            serialThread = new Thread(ReadSerialData);
            serialThread.IsBackground = true;
            serialThread.Start();
        }
        catch (System.Exception e)
        {
            serialAvailable = false; // Fall back to keyboard input
            Debug.LogWarning("Could not open serial port, using keyboard input instead: " + e.Message);
        }
    }

    void ReadSerialData()
    {
        while (isRunning)
        {
            if (serialPort == null || !serialPort.IsOpen) return;

            try
            {
                if (serialPort.BytesToRead > 0)
                {
                    // Read and clean input
                    string dataLine = serialPort.ReadLine().Trim();
      
                    if (string.IsNullOrEmpty(dataLine)) continue;

                    string[] tokens = dataLine.Split(',');
                    if (tokens.Length < 2) continue;


                    // Parse move speed
                    if (float.TryParse(tokens[0].Trim(), out float parsedMoveSpeed))
                    {
                        moveSpeed = parsedMoveSpeed;
                    }

                    Debug.Log("Data line - Move: " + moveSpeed);

                    // Parse camera speed
                    if (float.TryParse(tokens[1].Trim(), out float parsedCameraSpeed))
                    {
                        cameraSpeed = parsedCameraSpeed;
                    }

                    Debug.Log("Data line - Camera: " + cameraSpeed);

                    
                }
            }
            catch (System.TimeoutException) { }
            catch (System.Exception e)
            {
                Debug.LogError("Serial Read Error: " + e.Message);
            }
        }
    }

    private void Update() {
        DetectCameraCollision();
        UpdateEyeAngles();
        UpdatePosition();
    }


private void DetectCameraCollision()
{
    // Find the main camera
    Camera playerCamera = Camera.main;
    if (playerCamera == null) return;

    // Cast a ray from the camera's position in the forward direction
    Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
    RaycastHit hit;

    // Debug.Log($"Checking for collision");
    if (Physics.Raycast(ray, out hit, detectionRange))
    {
        if (!isColliding) // Only play collision sound once per collision event
        {
            
            Debug.Log($"Collision: Player collided with {hit.collider.gameObject.name} (Obstacle)");
            AudioManager.Instance.StopSound("ForwardBackward");
            AudioManager.Instance.PlaySound("Collision");
            isColliding = true;
        }
    }
    else
    {
        // If no collision detected, resume movement sound
        if (isColliding)
        {
            AudioManager.Instance.StopSound("Collision");
            AudioManager.Instance.PlaySound("ForwardBackward");
            isColliding = false;
        }
    }
}


    public Vector3 Move()
    {
        return transform.localPosition;
    }

    void OnDestroy()
    {
        isRunning = false;
        if (serialThread != null && serialThread.IsAlive)
        {
            serialThread.Join();
        }
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            Debug.Log("Serial Port Closed.");
        }
    }
}

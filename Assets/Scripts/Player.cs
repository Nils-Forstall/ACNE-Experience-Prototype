using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class Player : MonoBehaviour
{
    [SerializeField, Min(0f)]
    public float movementSpeed = 4f, rotationSpeed = 180f;

    [SerializeField]
    float startingVerticalEyeAngle = 10f;

    private CharacterController characterController;
    private Transform eye;
    private Vector2 eyeAngles;

    // Serial communication variables
    public string portName = "/dev/cu.usbserial-AQ02O6OD";
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

    private readonly object dataLock = new object();

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

    void Update()
    {
        float currentMoveSpeed = 0f;
        float currentCameraSpeed = 0f;

        // Retrieve shared data with thread safety
        lock (dataLock)
        {
            currentMoveSpeed = moveSpeed;
            currentCameraSpeed = cameraSpeed;
        }

        UpdateEyeAngles(currentCameraSpeed);
        UpdatePosition(currentMoveSpeed);

        Debug.Log("MoveSpeed: " + currentMoveSpeed + " | CameraSpeed: " + currentCameraSpeed);
    }

    void UpdateEyeAngles(float currentCameraSpeed)
{
    float rotationDelta = rotationSpeed * Time.deltaTime;

    // Use serial camera speed if available, otherwise fallback to keyboard input
    float horizontalInput = serialAvailable ? currentCameraSpeed : Input.GetAxis("Horizontal");

    eyeAngles.x += rotationDelta * horizontalInput;
    eye.localRotation = Quaternion.Euler(eyeAngles.y, eyeAngles.x, 0f);
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

    void UpdatePosition(float currentMoveSpeed)
    {
        // Use serial movement speed if available, otherwise fallback to keyboard input
        float forwardMovement = serialAvailable ? currentMoveSpeed * movementSpeed : Input.GetAxis("Vertical") * movementSpeed;
		Debug.Log(forwardMovement);

        Vector3 movement = eye.forward * forwardMovement;
        characterController.Move(movement * Time.deltaTime);

        if (forwardMovement > 0)
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
        AudioManager.Instance.PlaySound("footsteps-01");
    }

    void PlayBackwardAudio()
    {
        AudioManager.Instance.PlaySound("footsteps-02");
    }

      void TurnRight() {
        AudioManager.Instance.PlaySound("Tank Movement right");
    }

    void TurnLeft() {
        AudioManager.Instance.PlaySound("Tank Movement left");
    }


void StopTurnAudio() {
    AudioManager.Instance.StopSound("Tank Movement left");
        AudioManager.Instance.StopSound("Tank Movement right");
                isTurningLeft = false;
        isTurningRight = false;
}
    void StopAudio()
    {
        AudioManager.Instance.StopSound("footsteps-01");
        AudioManager.Instance.StopSound("footsteps-02");
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
                        lock (dataLock)
                        {
                            moveSpeed = parsedMoveSpeed;
                        }
                    }

                    // Parse camera speed
                    if (float.TryParse(tokens[1].Trim(), out float parsedCameraSpeed))
                    {
                        lock (dataLock)
                        {
                            cameraSpeed = parsedCameraSpeed;
                        }
                    }
                }
            }
            catch (System.TimeoutException) { }
            catch (System.Exception e)
            {
                Debug.LogError("Serial Read Error: " + e.Message);
            }
        }
    }

    public Vector3 Move()
    {
        float currentMoveSpeed = 0f;
        lock (dataLock)
        {
            currentMoveSpeed = moveSpeed;
        }
        UpdateEyeAngles(currentMoveSpeed);
        UpdatePosition(currentMoveSpeed);
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

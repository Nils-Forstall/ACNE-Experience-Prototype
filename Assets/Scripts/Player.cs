using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class Player : MonoBehaviour
{
    [SerializeField, Min(0f)]
    public float movementSpeed = 30f, rotationSpeed = 5f;

    [SerializeField]
    public string collisionAudio = "Collision1";

    [SerializeField]
    public string deadEndAudio = "Warning1";

    float detectionRange = 0.5f;

    [SerializeField]
    float startingVerticalEyeAngle = 10f;

   private bool isColliding = false; // Tracks collision state

    [SerializeField]
    public float MovementScalingFactor = 1f;

    [SerializeField]
    public float CameraScalingFactor = 0.01f;

    private CharacterController characterController;
    private Transform eye;
    private Vector2 eyeAngles;


    [SerializeField]     // for controller

    public string portName1 = "/dev/tty.usbserial-AQ02O6OD";  // For moveSpeed
    public string portName2 = "/dev/tty.usbserial-AQ02OE6D"; 
    public int baudRate = 115200;

    private SerialPort serialPort1;
    private SerialPort serialPort2;
    private Thread serialThread1;
    private Thread serialThread2;
    private bool isRunning = true;
    private float moveSpeed = 0f;
    private float cameraSpeed = 0f;
    private bool serialAvailable = true; // Check if controller exists

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

        Debug.Log("dataLine from moveSpeed: " + moveSpeed);

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
        AudioManager.Instance.PlaySound("walking");
    }

    void PlayBackwardAudio()
    {
        AudioManager.Instance.PlaySound("walking");
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
            serialPort1 = new SerialPort(portName1, baudRate);
            serialPort1.ReadTimeout = 1;
            serialPort1.Open();
            serialThread1 = new Thread(ReadSerialData1);
            serialThread1.IsBackground = true;
            serialThread1.Start();
            Debug.Log("Opening port1");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Could not open serial port1: " + e.Message);
            serialAvailable = false;
        }
        try
        {
            serialPort2 = new SerialPort(portName2, baudRate);
            serialPort2.ReadTimeout = 1;
            serialPort2.Open();
            serialThread2 = new Thread(ReadSerialData2);
            serialThread2.IsBackground = true;
            serialThread2.Start();
            Debug.Log("Opening port2");
            
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Could not open serial port2: " + e.Message);
            serialAvailable = false;
        }

    }
    void ReadSerialData1()
    {
        while (isRunning)
        {
            if (serialPort1 == null || !serialPort1.IsOpen)
                return;
            try
            {
                if (serialPort1.BytesToRead > 0)
                {
                    string dataLine = serialPort1.ReadLine();
                    Debug.Log("dataLine from port1: " + dataLine);
                    if (string.IsNullOrEmpty(dataLine))
                        continue;
                    if (float.TryParse(dataLine, out float parsedValue))
                    {
                        moveSpeed = parsedValue;
                    }
                }
            }
            catch (System.TimeoutException) { }
            catch (System.Exception e)
            {
                Debug.LogError("Serial Read Error (Move): " + e.Message);
            }
        }
    }
    // Reads raw serial data for cameraSpeed.
    void ReadSerialData2()
    {
        while (isRunning)
        {
            if (serialPort2 == null || !serialPort2.IsOpen)
                return;
            try
            {
                if (serialPort2.BytesToRead > 0)
                {
                    
                    string dataLine = serialPort2.ReadLine();
                    Debug.Log("dataLine from port2" + dataLine);
                    if (string.IsNullOrEmpty(dataLine))
                        continue;
                    if (float.TryParse(dataLine, out float parsedValue))
                    {
                        cameraSpeed = -parsedValue;
                    }
                }
            }
            catch (System.TimeoutException) { }
            catch (System.Exception e)
            {
                Debug.LogError("Serial Read Error (Camera): " + e.Message);
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

    bool isSendingData1 = false;
    // Cast a ray from the camera's position in the forward direction
    Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
    RaycastHit hit;
    

    if (Physics.Raycast(ray, out hit, detectionRange))
    {
        if (!isColliding) // Only play collision sound once per collision event
        {
            Debug.Log($"Collision: Player collided with {hit.collider.gameObject.name} (Obstacle)");
            AudioManager.Instance.StopSound("ForwardBackward");
            AudioManager.Instance.PlaySound(collisionAudio);
            if (!isSendingData1 && !isMovingBackward)
            {
                isSendingData1 = true;
                SendSerialData1();
            }
            isColliding = true;
        }
    }
    else
    {
        // If no collision detected, resume movement sound
        if (isColliding)
        {
            isSendingData1 = false;
            AudioManager.Instance.StopSound(collisionAudio);
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
        if (serialThread1 != null && serialThread1.IsAlive)
        {
            serialThread1.Join();
        }
        if (serialThread2 != null && serialThread2.IsAlive)
        {
            serialThread2.Join();
        }
        if (serialPort1 != null && serialPort1.IsOpen)
        {
            serialPort1.Close();
            Debug.Log("Serial Port1 Closed.");
        }
        if (serialPort2 != null && serialPort2.IsOpen)
        {
            serialPort2.Close();
            Debug.Log("Serial Port2 Closed.");
        }
    }

        void SendSerialData1()
    {
            float data1 = -0.25f;
            serialPort1.WriteLine(data1.ToString());
            Debug.Log("Sent data on Serial Port1: " + data1);
    }
    void SendSerialData2()
    {
            float data2 = 10.0f;
            serialPort2.WriteLine(data2.ToString());
            Debug.Log("Sent data on Serial Port2: " + data2);
    }
}

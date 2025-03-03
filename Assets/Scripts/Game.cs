using TMPro;
using Unity.Collections;
using System.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

using static Unity.Mathematics.math;

using Random = UnityEngine.Random;

public class Game : MonoBehaviour
{
    // Singleton instance
    private static Game instance;

    // Public property to access the instance
    public static Game Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Game>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    instance = singletonObject.AddComponent<Game>();
                    singletonObject.name = typeof(Game).ToString() + " (Singleton)";
                }
            }
            return instance;
        }
    }

    // Ensure the instance is created in Awake
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    [SerializeField]
    MazeVisualization visualization;

    [SerializeField]
    bool startPlaying; 
    int2 mazeSize = int2(13, 13);

    [SerializeField, Tooltip("Use zero for random seed.")]
    int seed;

    [SerializeField, Range(0f, 1f)]
    float
        pickLastProbability = 0.5f,
        openDeadEndProbability = 0.5f,
        openArbitraryProbability = 0.5f;

    [SerializeField]
    Player player;

    [SerializeField]
    Agent[] agents;

    [SerializeField]
    TextMeshPro displayText;

    [SerializeField]
    TextMeshPro guidanceText;

    Maze maze;

    Scent scent;

    List<int2> solutionPath;

    bool isPlaying;

    MazeCellObject[] cellObjects;

    void createMaze()
    {
        maze = new Maze(mazeSize);

        scent = new Scent(maze);
        new FindDiagonalPassagesJob
        {
            maze = maze
        }.ScheduleParallel(
            maze.Length, maze.SizeEW, new GenerateMazeJob
            {
                maze = maze,
                seed = seed != 0 ? seed : Random.Range(1, int.MaxValue),
                pickLastProbability = pickLastProbability,
                openDeadEndProbability = openDeadEndProbability,
                openArbitraryProbability = openArbitraryProbability
            }.Schedule()
        ).Complete();


        if (cellObjects == null || cellObjects.Length != maze.Length)
        {
            cellObjects = new MazeCellObject[maze.Length];
        }
        visualization.Visualize(maze, cellObjects);

        if (seed != 0)
        {
            Random.InitState(seed);
        }

    }

    void StartNewGame()
    {
        createMaze();
        player.SetMaze(maze);
        isPlaying = true;
        displayText.gameObject.SetActive(false);
        
        
        MazeSolver solver = new MazeSolver(maze);
        int startIndex = maze.CoordinatesToIndex(new int2(0, 0)); // Start at (0,0)
        int endIndex = maze.CoordinatesToIndex(new int2(mazeSize.x - 1, mazeSize.y - 1)); // Goal at (maxX, maxY)
        solutionPath = solver.Solve(startIndex, endIndex);

        while (solutionPath.Count == 0)
        {
            createMaze();
            solutionPath = solver.Solve(startIndex, endIndex);
            isPlaying = false;
        }

        solver.Dispose();


        foreach (var step in solutionPath)
        {
            // Debug.Log($"Path Step: {step}");
        }


                // add soundtrack
        AudioManager.Instance.PlaySound("Press Space to Start");
        
        player.StartNewGame(maze.CoordinatesToWorldPosition(int2(Random.Range(0, mazeSize.x / 4), Random.Range(0, mazeSize.y / 4))));

        int2 halfSize = mazeSize / 2;
        for (int i = 0; i < agents.Length; i++)
        {
            var coordinates =
                int2(Random.Range(0, mazeSize.x), Random.Range(0, mazeSize.y));
            if (coordinates.x < halfSize.x && coordinates.y < halfSize.y)
            {
                if (Random.value < 0.5f)
                {
                    coordinates.x += halfSize.x;
                }
                else
                {
                    coordinates.y += halfSize.y;
                }
            }
            agents[i].StartNewGame(maze, coordinates);
        }
    }

    void Update()
    {
        if (isPlaying)
        {
            UpdateGame();
        } else {
            if (!startPlaying) {
                AudioManager.Instance.PlaySound("Press Space to Start");
                startPlaying = true;
                Debug.Log("Press space to start");
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartNewGame();
                startPlaying = false;
                AudioManager.Instance.PlaySound("Game Begin");
                AudioManager.Instance.StopSound("Press Space to Start");
                UpdateGame();
            }
        }
        
    }

	public bool getIsPlaying()
	{
		return isPlaying;
	}

    void UpdateGame()
    {
        Vector3 playerPosition = player.Move();
        NativeArray<float> currentScent = scent.Disperse(maze, playerPosition);

        // Guide the player towards the solution path
        UpdateGuidance(playerPosition);

        for (int i = 0; i < agents.Length; i++)
        {
            Vector3 agentPosition = agents[i].Move(currentScent);
            if (new Vector2(agentPosition.x - playerPosition.x, agentPosition.z - playerPosition.z).sqrMagnitude < 1f)
            {
                EndGame(agents[i].TriggerMessage);
                AudioManager.Instance.StopAllSounds();
                AudioManager.Instance.PlaySound("Death Sound");
                AudioManager.Instance.PlaySound("You Died");
                return;
            }
        }
    }
    void UpdateGuidance(Vector3 playerPosition)
    {
        if (solutionPath == null || solutionPath.Count == 0) return;

        int2 playerCoords = maze.WorldPositionToCoordinates(playerPosition);
        float closestDistance = float.MaxValue;
        int2 closestCell = playerCoords;
        int nextStepIndex = 0;

        // **Find the next best cell on the solution path (avoiding walls)**
        for (int i = 0; i < solutionPath.Count; i++)
        {
            float distance = math.distance(playerCoords, solutionPath[i]);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCell = solutionPath[i];
                nextStepIndex = i;
            }
        }

        // **Look ahead for a safer movement direction**
        int2 targetCell = closestCell;
        if (nextStepIndex + 1 < solutionPath.Count)
        {
            int2 lookAheadCell = solutionPath[nextStepIndex + 1];
            if (!maze.IsWall(lookAheadCell)) // Ensure it's not a wall
            {
                targetCell = lookAheadCell;
            }
        }

        // Convert target cell to world space
        Vector3 targetWorldPosition = maze.CoordinatesToWorldPosition(targetCell);
        Vector3 targetDirection = (targetWorldPosition - playerPosition).normalized;

        // Get player's current facing direction
        Vector3 playerForward = player.transform.forward;
        float angleToTarget = Vector3.SignedAngle(playerForward, targetDirection, Vector3.up);

        // Normalize angle to shortest turn
        angleToTarget = (angleToTarget + 360f) % 360f;
        if (angleToTarget > 180f) angleToTarget -= 360f;

        float rotationTolerance = 10f; // Angle threshold for minor corrections
        float backwardThreshold = 150f; // Only move backward if the path is nearly behind you
        string guidanceMessage = "";

        // **Check for movement collisions before suggesting movement**
        bool canMoveForward = !maze.IsWall(targetCell);
        bool canMoveBackward = !maze.IsWall(maze.WorldPositionToCoordinates(playerPosition - playerForward));

        // **Avoid collisions by prioritizing turns before movement**
        if (math.abs(angleToTarget) <= rotationTolerance && canMoveForward)
        {
            guidanceMessage = "Move Forward ↑";
        }
        else if (math.abs(angleToTarget) >= backwardThreshold && canMoveBackward)
        {
            guidanceMessage = "Move Backward ↓";
        }
        else if (angleToTarget > 0)
        {
            guidanceMessage = "Turn Right ↻";
        }
        else
        {
            guidanceMessage = "Turn Left ↺";
        }

        // Display guidance message
        // Debug.Log(guidanceMessage);

        // **Check if the player has reached the goal**
        int2 goalPosition = solutionPath[solutionPath.Count - 1];
        if (math.distance(playerCoords, goalPosition) < 1f)
        {
            guidanceText.text = "🎉 Congratulations! You reached the goal!";
            Debug.Log("Player has reached the solution spot!");
            // isPlaying = false; // Stop game logic
        }
    }




void EndGame(string message)
{
    isPlaying = false;
    OnDestroy();

    displayText.text = message;
    displayText.gameObject.SetActive(true);

    // Stop agents
    for (int i = 0; i < agents.Length; i++)
    {
        agents[i].EndGame();
    }

      // Clean up objects
    if (cellObjects != null)
    {
        for (int i = 0; i < cellObjects.Length; i++)
        {
            if (cellObjects[i] != null)
            {
                Destroy(cellObjects[i].gameObject);
            }
        }
    }

    
}

    void OnDestroy()
    {
        maze.Dispose();
        scent.Dispose();
    }
}
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct MazeSolver
{
    Maze maze;
    NativeArray<int> cameFrom;

    public MazeSolver(Maze maze)
    {
        this.maze = maze;
        cameFrom = new NativeArray<int>(maze.Length, Allocator.Persistent);
        for (int i = 0; i < cameFrom.Length; i++)
        {
            cameFrom[i] = -1; // -1 indicates an unvisited node
        }
    }

    public void Dispose()
    {
        if (cameFrom.IsCreated)
        {
            cameFrom.Dispose();
        }
    }

    public List<int2> Solve(int startIndex, int endIndex)
    {
        var frontier = new Queue<int>();
        frontier.Enqueue(startIndex);
        cameFrom[startIndex] = startIndex;

        while (frontier.Count > 0)
        {
            int current = frontier.Dequeue();

            if (current == endIndex)
            {
                break; // Found the exit
            }

            // Check all possible directions
            TryMove(current, maze.StepN, frontier);
            TryMove(current, maze.StepE, frontier);
            TryMove(current, maze.StepS, frontier);
            TryMove(current, maze.StepW, frontier);
        }

        // Reconstruct the path
        return ReconstructPath(startIndex, endIndex);
    }

    private void TryMove(int current, int step, Queue<int> frontier)
    {
        int neighbor = current + step;
        if (neighbor >= 0 && neighbor < maze.Length && cameFrom[neighbor] == -1)
        {
            // Ensure there is a passage (not a wall)
            if ((maze.GetCell(current) & maze.GetCell(neighbor)) != 0)
            {
                frontier.Enqueue(neighbor);
                cameFrom[neighbor] = current;
            }
        }
    }

    private List<int2> ReconstructPath(int startIndex, int endIndex)
    {
        var path = new List<int2>();
        int current = endIndex;

        while (current != startIndex)
        {
            if (current == -1) return new List<int2>(); // No path found
            path.Add(maze.IndexToCoordinates(current));
            current = cameFrom[current];
        }

        path.Reverse();
        return path;
    }
}

using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using AccessibilityPrototype;

using static Unity.Mathematics.math;

public struct Maze
{
	int2 size;

	[NativeDisableParallelForRestriction]
	NativeArray<MazeFlags> cells;

	public MazeFlags this[int index]
	{
		get => cells[index];
		set => cells[index] = value;
	}

	public int Length => cells.Length;

	public int SizeEW => size.x;

	public int SizeNS => size.y;

	public int StepN => size.x;

	public int StepE => 1;

	public int StepS => -size.x;

	public int StepW => -1;

	public Maze(int2 size)
	{
		this.size = size;
		cells = new NativeArray<MazeFlags>(size.x * size.y, Allocator.Persistent);
	}

	public void Dispose()
	{
		if (cells.IsCreated)
		{
			cells.Dispose();
		}
	}

	public MazeFlags Set(int index, MazeFlags mask) =>
		cells[index] = cells[index].With(mask);

	public MazeFlags Unset(int index, MazeFlags mask) =>
		cells[index] = cells[index].Without(mask);

	public int2 IndexToCoordinates(int index)
	{
		int2 coordinates;
		coordinates.y = index / size.x;
		coordinates.x = index - size.x * coordinates.y;
		return coordinates;
	}

	public Vector3 CoordinatesToWorldPosition(int2 coordinates, float y = 0f) =>
		new Vector3(
			2f * coordinates.x + 1f - size.x,
			y,
			2f * coordinates.y + 1f - size.y
		);

	public bool IsWall(int2 coordinates)
	{
		// Check if coordinates are out of bounds
		if (coordinates.x < 0 || coordinates.x >= size.x || coordinates.y < 0 || coordinates.y >= size.y)
			return true; // Out-of-bounds is a wall

		int index = CoordinatesToIndex(coordinates);

		// Check if the cell exists (we assume uninitialized or empty cells are walls)
		return index < 0 || index >= cells.Length || cells[index] == 0;
	}

	public bool isDirectionOpen(int2 playerCoords, CardinalDirection direction)
	{
		// Find the maze flag for the direction
		// Don't use IsWall() in this method, instead use the maze flags directly
		int index = CoordinatesToIndex(playerCoords);
		MazeFlags flags = cells[index];	
		if (direction == CardinalDirection.North)
		{
			return (flags & MazeFlags.PassageN) != 0;
		}
		else if (direction == CardinalDirection.East)
		{
			return (flags & MazeFlags.PassageE) != 0;
		}
		else if (direction == CardinalDirection.South)
		{
			return (flags & MazeFlags.PassageS) != 0;
		}
		else if (direction == CardinalDirection.West)
		{
			return (flags & MazeFlags.PassageW) != 0;
		}
		return false;
	}



	public Vector3 IndexToWorldPosition(int index, float y = 0f) =>
		CoordinatesToWorldPosition(IndexToCoordinates(index), y);

	public int CoordinatesToIndex(int2 coordinates) =>
		coordinates.y * size.x + coordinates.x;

	public int2 WorldPositionToCoordinates(Vector3 position) => int2(
		(int)((position.x + size.x) * 0.5f),
		(int)((position.z + size.y) * 0.5f)
	);

	public int WorldPositionToIndex(Vector3 position) =>
		CoordinatesToIndex(WorldPositionToCoordinates(position));
}

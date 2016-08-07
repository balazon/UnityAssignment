using System;

// Author: Alex Regueiro.
// Originally published at <http://noldorin.com/blog/2009/09/hungarian-algorithm-in-csharp/>.
// Based on implementation described at <http://www.public.iastate.edu/~ddoty/HungarianAlgorithm.html>.
// Version 1.2, released 24th October 2009.
public class HungarianAlgorithmAlex : IAssignmentSolver
{
	public bool IsMaxSolver()
	{
		return false;
	}
	/// <summary>
	/// Finds the optimal assignments for a matrix of agents and costed tasks.
	/// </summary>
	/// <param name="weights">A cost matrix; each row contains elements that represent the associated weights of each
	/// task for the agent.</param>
	/// <returns>An array of assignments; element <em>i</em> is the index of the assigned task (column) for agent
	/// (row) <em>i</em>.</returns>
	public void Solve(int[] weights, int n, out int[] result)
	{
		for (int i = 0; i < n; i++)
		{
			var min = int.MaxValue;
			for (int j = 0; j < n; j++)
				min = Math.Min(min, weights[i * n + j]);
			for (int j = 0; j < n; j++)
				weights[i * n + j] -= min;
		}

		var masks = new byte[n, n];
		var rowsCovered = new bool[n];
		var colsCovered = new bool[n];
		for (int i = 0; i < n; i++)
		{
			for (int j = 0; j < n; j++)
			{
				if (weights[i * n + j] == 0 && !rowsCovered[i] && !colsCovered[j])
				{
					masks[i, j] = 1;
					rowsCovered[i] = true;
					colsCovered[j] = true;
				}
			}
		}
		ClearCovers(rowsCovered, colsCovered, n, n);

		var path = new Location[n * n];
		Location pathStart = default(Location);
		var step = 1;
		while (step != -1)
		{
			switch (step)
			{
				case 1:
					step = RunStep1(weights, masks, rowsCovered, colsCovered, n, n);
					break;
				case 2:
					step = RunStep2(weights, masks, rowsCovered, colsCovered, n, n, ref pathStart);
					break;
				case 3:
					step = RunStep3(weights, masks, rowsCovered, colsCovered, n, n, path, pathStart);
					break;
				case 4:
					step = RunStep4(weights, masks, rowsCovered, colsCovered, n, n);
					break;
			}
		}

		result = new int[n];
		for (int i = 0; i < n; i++)
		{
			for (int j = 0; j < n; j++)
			{
				if (masks[i, j] == 1)
				{
					result[i] = j;
					break;
				}
			}
		}

		return;
	}

	private static int RunStep1(int[] weights, byte[,] masks, bool[] rowsCovered, bool[] colsCovered, int w, int h)
	{
		for (int i = 0; i < h; i++)
		{
			for (int j = 0; j < w; j++)
			{
				if (masks[i, j] == 1)
					colsCovered[j] = true;
			}
		}
		var colsCoveredCount = 0;
		for (int j = 0; j < w; j++)
		{
			if (colsCovered[j])
				colsCoveredCount++;
		}
		if (colsCoveredCount == h)
			return -1;
		else
			return 2;
	}

	private static int RunStep2(int[] weights, byte[,] masks, bool[] rowsCovered, bool[] colsCovered, int w, int h,
		ref Location pathStart)
	{
		Location loc;
		while (true)
		{
			loc = FindZero(weights, masks, rowsCovered, colsCovered, w, h);
			if (loc.Row == -1)
			{
				return 4;
			}
			else
			{
				masks[loc.Row, loc.Column] = 2;
				var starCol = FindStarInRow(masks, w, loc.Row);
				if (starCol != -1)
				{
					rowsCovered[loc.Row] = true;
					colsCovered[starCol] = false;
				}
				else
				{
					pathStart = loc;
					return 3;
				}
			}
		}
	}

	private static int RunStep3(int[] weights, byte[,] masks, bool[] rowsCovered, bool[] colsCovered, int w, int h,
		Location[] path, Location pathStart)
	{
		var pathIndex = 0;
		path[0] = pathStart;
		while (true)
		{
			var row = FindStarInColumn(masks, h, path[pathIndex].Column);
			if (row == -1)
				break;
			pathIndex++;
			path[pathIndex] = new Location(row, path[pathIndex - 1].Column);
			var col = FindPrimeInRow(masks, w, path[pathIndex].Row);
			pathIndex++;
			path[pathIndex] = new Location(path[pathIndex - 1].Row, col);
		}
		ConvertPath(masks, path, pathIndex + 1);
		ClearCovers(rowsCovered, colsCovered, w, h);
		ClearPrimes(masks, w, h);
		return 1;
	}

	private static int RunStep4(int[] weights, byte[,] masks, bool[] rowsCovered, bool[] colsCovered, int w, int h)
	{
		var minValue = FindMinimum(weights, rowsCovered, colsCovered, w, h);
		for (int i = 0; i < h; i++)
		{
			for (int j = 0; j < w; j++)
			{
				if (rowsCovered[i])
					weights[i * w + j] += minValue;
				if (!colsCovered[j])
					weights[i * w + j] -= minValue;
			}
		}
		return 2;
	}

	private static void ConvertPath(byte[,] masks, Location[] path, int pathLength)
	{
		for (int i = 0; i < pathLength; i++)
		{
			if (masks[path[i].Row, path[i].Column] == 1)
				masks[path[i].Row, path[i].Column] = 0;
			else if (masks[path[i].Row, path[i].Column] == 2)
				masks[path[i].Row, path[i].Column] = 1;
		}
	}

	private static Location FindZero(int[] weights, byte[,] masks, bool[] rowsCovered, bool[] colsCovered,
		int w, int h)
	{
		for (int i = 0; i < h; i++)
		{
			for (int j = 0; j < w; j++)
			{
				if (weights[i * w + j] == 0 && !rowsCovered[i] && !colsCovered[j])
					return new Location(i, j);
			}
		}
		return new Location(-1, -1);
	}

	private static int FindMinimum(int[] weights, bool[] rowsCovered, bool[] colsCovered, int w, int h)
	{
		var minValue = int.MaxValue;
		for (int i = 0; i < h; i++)
		{
			for (int j = 0; j < w; j++)
			{
				if (!rowsCovered[i] && !colsCovered[j])
					minValue = Math.Min(minValue, weights[i * w + j]);
			}
		}
		return minValue;
	}

	private static int FindStarInRow(byte[,] masks, int w, int row)
	{
		for (int j = 0; j < w; j++)
		{
			if (masks[row, j] == 1)
				return j;
		}
		return -1;
	}

	private static int FindStarInColumn(byte[,] masks, int h, int col)
	{
		for (int i = 0; i < h; i++)
		{
			if (masks[i, col] == 1)
				return i;
		}
		return -1;
	}

	private static int FindPrimeInRow(byte[,] masks, int w, int row)
	{
		for (int j = 0; j < w; j++)
		{
			if (masks[row, j] == 2)
				return j;
		}
		return -1;
	}

	private static void ClearCovers(bool[] rowsCovered, bool[] colsCovered, int w, int h)
	{
		for (int i = 0; i < h; i++)
			rowsCovered[i] = false;
		for (int j = 0; j < w; j++)
			colsCovered[j] = false;
	}

	private static void ClearPrimes(byte[,] masks, int w, int h)
	{
		for (int i = 0; i < h; i++)
		{
			for (int j = 0; j < w; j++)
			{
				if (masks[i, j] == 2)
					masks[i, j] = 0;
			}
		}
	}

	

	private struct Location
	{
		public int Row;
		public int Column;

		public Location(int row, int col)
		{
			this.Row = row;
			this.Column = col;
		}
	}
}
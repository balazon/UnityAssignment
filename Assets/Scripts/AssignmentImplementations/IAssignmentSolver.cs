using UnityEngine;
using System.Collections;

public interface IAssignmentSolver
{
	void Solve(int[] weights, int n, out int[] result);

	bool IsMaxSolver();

}

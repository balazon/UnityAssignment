using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;




public class Assignment : MonoBehaviour {

	public static Assignment Instance{ get; protected set; }


	protected IAssignmentSolver assignSolver;

	System.Diagnostics.Stopwatch stopWatch;

	bool maximizeWeight;

	void Awake()
	{
		Instance = this;
		assignSolver = new HungarianAlgorithmAlex();


		stopWatch = System.Diagnostics.Stopwatch.StartNew();
		stopWatch.Reset();

		maximizeWeight = false;
    }

	public void SetMaximizeWeight(bool value)
	{
		maximizeWeight = value;
	}

	public void AssignUnitsToTargets(Unit[] units, Vector3[] targetPositions)
	{
		int n = units.Length;
		int[] weights = new int[n * n];

		int maxSolverMultiplier = (assignSolver.IsMaxSolver() ^ maximizeWeight) ? -1 : 1;
		for(int i = 0; i < units.Length; i++)
		{
			var unitPos = units[i].transform.position;
			for (int j = 0; j < targetPositions.Length; j++)
			{
				var targetPos = targetPositions[j];
				weights[i * n + j] = (int)((targetPos - unitPos).sqrMagnitude) * maxSolverMultiplier;
			}
		}

		int[] res;

		
		stopWatch.Reset();
		stopWatch.Start();
        assignSolver.Solve(weights, n, out res);
		stopWatch.Stop();
		Debug.LogFormat("solve millis: {0}", stopWatch.ElapsedMilliseconds);

		for (int i = 0; i < n; i++)
		{
			var agent = units[i].GetComponent<NavMeshAgent>();
			agent.SetDestination(targetPositions[res[i]]);
		}
	}


	

	
}

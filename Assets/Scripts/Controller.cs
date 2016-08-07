using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class Controller : MonoBehaviour {


	protected Grid grid;
	protected Plane gridBasePlane;


	System.Diagnostics.Stopwatch stopWatch;

	protected string _message;
	public string Message
	{
		get { return _message; }
		protected set
		{
			_message = value;
			if (EventManager.Instance != null)
			{
				EventManager.Instance.Notify();
			}
		}
	}

	void Awake()
	{
		Message = "";

		grid = GameObject.Find("Grid").GetComponent<Grid>();
		gridBasePlane = new Plane(Vector3.up, 0.0f);

		stopWatch = System.Diagnostics.Stopwatch.StartNew();
		stopWatch.Reset();
	}

	void Start()
	{
		EditStart();
	}
	
	// Update is called once per frame
	void Update ()
	{
		bool add = false;
		if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
		{
			add = Input.GetMouseButton(0);
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			float enter = 0.0f;
			if (gridBasePlane.Raycast(ray, out enter))
			{
				grid.Clicked(ray.GetPoint(enter), add);
			}
		}

	}

	public void EditStart()
	{
		Debug.Log("edit start");
		grid.LoadImage("start");
		Message = "Editing Start points";
	}

	public void EditEnd()
	{
		Debug.Log("edit end");
		grid.LoadImage("end");
		Message = "Editing End points";
	}

	public void Clear()
	{
		grid.Clear(true);
		Unit[] oldUnits = GameObject.FindObjectsOfType<Unit>();
		foreach (var unit in oldUnits)
		{
			ObjectPool.Instance.ReturnUnit(unit);
		}
	}

	public void MaximizeDistance(bool value)
	{
		Assignment.Instance.SetMaximizeWeight(value);
	}


	public void Go()
	{

		Unit[] oldUnits = GameObject.FindObjectsOfType<Unit>();
		foreach(var unit in oldUnits)
		{
			ObjectPool.Instance.ReturnUnit(unit);
		}

		
		var startPositions = grid.GetImagePositions("start");
		var targetPositions = grid.GetImagePositions("end");
		if (startPositions.Count != targetPositions.Count)
		{
			Message = "Error: start and end point count must match";
            return;
		}

		grid.Clear();
		Message = "Simulating";

		List<Unit> units = new List<Unit>();

		stopWatch.Reset();
		stopWatch.Start();
		//create/place units to startPositions
		foreach (var pos in startPositions)
		{
			var unit = ObjectPool.Instance.CreateUnit(pos, Quaternion.identity);
			units.Add(unit);
		}
		stopWatch.Stop();

		Debug.LogFormat("creating units millis: {0}", stopWatch.ElapsedMilliseconds);


		// set their target to endPositions
		Assignment.Instance.AssignUnitsToTargets(units.ToArray(), targetPositions.ToArray());
	}

	
}

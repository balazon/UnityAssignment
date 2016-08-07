using UnityEngine;
using System.Collections;

public class ObjectPool : MonoBehaviour {

	public GameObject unitPrefab;

	public static ObjectPool Instance { get; protected set; }

	void Awake()
	{
		Instance = this;
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public Unit CreateUnit(Vector3 position, Quaternion rotation)
	{
		var go = Instantiate(unitPrefab, position, rotation) as GameObject;
		return go.GetComponent<Unit>();
	}

	public void ReturnUnit(Unit unit)
	{
		Destroy(unit.gameObject);
	}
}

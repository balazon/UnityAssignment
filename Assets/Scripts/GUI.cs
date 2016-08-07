using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GUI : MonoBehaviour {

	Grid grid;
	Text pointNumber;

	Controller controller;
	Text message;
	

	void Awake()
	{
		grid = GameObject.Find("Grid").GetComponent<Grid>();
		pointNumber = GameObject.Find("PointNumber").GetComponent<Text>();

		controller = GameObject.Find("Controller").GetComponent<Controller>();
		message = GameObject.Find("Message").GetComponent<Text>();
		
	}

	// Use this for initialization
	void Start () {
		EventManager.ModelChanged += UpdateGUI;
		UpdateGUI();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void UpdateGUI()
	{
		pointNumber.text = "" + grid.PointCount;
		message.text = controller.Message;
    }
}

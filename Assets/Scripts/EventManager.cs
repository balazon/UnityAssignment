using UnityEngine;
using System.Collections;

public class EventManager : MonoBehaviour
{
	public delegate void VoidDelegate();
	public static event VoidDelegate ModelChanged;

	public static EventManager Instance { get; protected set; }

	void Awake()
	{
		Instance = this;
	}

	public void Notify()
	{
		if(ModelChanged != null)
		{
			ModelChanged();
		}
	}

}
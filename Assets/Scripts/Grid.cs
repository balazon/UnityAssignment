using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

	public int w = 10;
	public int h = 10;

	float cellWidth = 1.0f;
	float cellHeight = 1.0f;

	public GameObject cellQuadPrefab;

	protected GameObject[] cells;

	protected Dictionary<string, bool[]> images;

	bool[] currentImageData;

	protected GameObject gridBase;

	protected int _pointCount;
	public int PointCount
	{
		get { return _pointCount; }
		protected set
		{
			_pointCount = value;
			if (EventManager.Instance != null)
			{
				EventManager.Instance.Notify();
			}
		}
	}

	protected Vector3 GridOrigin
	{
		get { return gridBase.transform.position - new Vector3(w * cellWidth * 0.5f, 0, h * cellHeight * 0.5f); }
	}

	// Use this for initialization
	void Start () {
	
	}

	void Awake()
	{
		images = new Dictionary<string, bool[]>();
		cells = new GameObject[w * h];

		//LoadImage("start");

		//PointCount = 0;
    }

	void OnValidate()
	{
		gridBase = GameObject.Find("GridBase");
		gridBase.transform.localScale = new Vector3(w, h, 1.0f);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//public GetImage(string key)
	//{
		
	//}

	public List<Vector3> GetImagePositions(string key)
	{
		List<Vector3> list = new List<Vector3>();
		if (!images.ContainsKey(key))
		{
			return list;
		}
		var image = images[key];
		var gridOrigin = GridOrigin;

		for (int j = 0; j < h; j++)
		{
			for (int i = 0; i < w; i++)
			{
				if(image[j * w + i])
				{
					list.Add(gridOrigin + new Vector3(i + cellWidth * 0.5f, 0.01f, j + cellHeight * 0.5f));
				}
			}
		}
		
		return list;
    }

	public void LoadImage(string key)
	{
		var gridOrigin = GridOrigin;
		PointCount = 0;
		if (!images.ContainsKey(key))
		{
			
			currentImageData = new bool[w * h];
			Clear(true);
			for (int i = 0; i < w * h; i++)
			{
				currentImageData[i] = false;
			}
			images.Add(key, currentImageData);
            return;
		}

		currentImageData = images[key];
		
        for (int j = 0; j < h; j++)
		{
			for (int i = 0; i < w; i++)
			{
				if(currentImageData[j * w + i])
				{
					PointCount++;
				}
				if(cells[j * w + i] == null && currentImageData[j * w + i])
				{
					var go = Instantiate(cellQuadPrefab, gridOrigin + new Vector3(i + cellWidth * 0.5f, 0.01f, j + cellHeight * 0.5f), cellQuadPrefab.transform.rotation) as GameObject;
					cells[j * w + i] = go;
				}
				else if(cells[j * w + i] != null && !currentImageData[j * w + i])
				{
					Destroy(cells[j * w + i]);
					cells[j * w + i] = null;
				}

			}
		}
	}

	public void Clear(bool wipeData = false)
	{
		if(currentImageData == null)
		{
			wipeData = false;
		}
		if(wipeData)
		{
			PointCount = 0;
		}
		for (int i = 0; i < w * h; i++)
		{
			if(wipeData)
			{
				currentImageData[i] = false;
			}
			if (cells[i] != null)
			{
				Destroy(cells[i]);
				cells[i] = null;
			}
		}
	}

	public void Clicked(Vector3 planePosition, bool add)
	{
		var gridOrigin = GridOrigin;
        var relativePosition = planePosition - gridOrigin;

		
		int i = (int)(relativePosition.x / cellWidth);
		int j = (int)(relativePosition.z / cellHeight);
		int index = j * w + i;

		if (i < 0 || i >= w || j < 0 || j >= h)
		{
			return;
		}

		if(add && cells[index] == null)
		{
			var go = Instantiate(cellQuadPrefab, gridOrigin + new Vector3(i + cellWidth * 0.5f, 0.01f, j + cellHeight * 0.5f), cellQuadPrefab.transform.rotation) as GameObject;
			cells[index] = go;
			currentImageData[index] = true;
			PointCount++;
		}
		else if(!add && cells[index] != null)
		{
			Destroy(cells[index]);
			cells[index] = null;
			currentImageData[index] = false;
			PointCount--;
		}

    }
}

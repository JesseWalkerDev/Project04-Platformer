using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class CameraFocusZone : MonoBehaviour
{
	static public List<CameraFocusZone> allZones = new List<CameraFocusZone>();
	
	public Vector3 cameraFocusPoint;
	public BoxCollider2D boundingBox;
	
	/*
	public Vector2 min
	{
		get { return boundingBox.bounds.min; }
		set { transform.position = value; }
	}
	public Vector2 max
	{
		get { return boundingBox.bounds.max; }
		set { boundingBox.size = value - min; }
	}
	*/
	
	// Start is called before the first frame update
	void Start()
	{
		boundingBox = gameObject.GetComponent<BoxCollider2D>();
		CameraFocusZone.allZones.Add(this);
	}

	// Update is called once per frame
	void Update()
	{
		
	}
}

[CustomEditor(typeof(CameraFocusZone))]
public class CameraFocusZoneEditor : Editor
{
	public void OnSceneGUI()
	{
		CameraFocusZone t = target as CameraFocusZone;
		Transform tr = t.transform;
		Vector3 pos = tr.position;
		Color color = new Color(1, 0.8f, 0.4f, 1);
		
		Handles.color = color;
		GUI.color = color;
		
		//Handles.Label(pos, t.cameraFocusPoint.ToString("F1"));
		Quaternion _q = Quaternion.identity;
		Handles.TransformHandle(ref t.cameraFocusPoint, ref _q);
	}
}
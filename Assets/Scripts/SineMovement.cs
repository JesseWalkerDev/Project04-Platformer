using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public class SineMovement : MonoBehaviour
{
	public Transform target;
	public bool x = true;
	public bool y = false;
	public float xRange = 5f;
	public float yRange = 5f;
	public float speed = 0.1f;
	
	void Update()
	{
		Vector3 orbit = new Vector3(
			x ? Mathf.Cos(Time.time * speed) * xRange / 2f : 0f,
			y ? Mathf.Sin(Time.time * speed) * yRange / 2f : 0f,
			0
		);
		target.position = transform.position + orbit;
	}
}

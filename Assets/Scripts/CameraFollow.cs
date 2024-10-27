using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class CameraFollow : MonoBehaviour
{
	public static int checkPointCount = 0;
	
	public Transform target;
	public Vector3 zoneTarget;
	public float snappiness = 0.02f;
	public float transitionLength = 0.5f;
	
	private List<CameraFocusZone> visitedZones = new();
	
	//private float transitionTime = 0.0f;
	//private 
	
	// Start is called before the first frame update
	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		bool zoneTarget = false;
		List<CameraFocusZone> zones = CameraFocusZone.allZones;
		foreach (CameraFocusZone zone in zones)
		{
			if (!zone) { continue; }
			if (zone.GetComponent<BoxCollider2D>().bounds.Contains(target.transform.position))
			{
				zoneTarget = true;
				Vector3 vector = Vector3.Lerp(transform.position, zone.cameraFocusPoint, snappiness);//easeInOutCubic());
				
				vector.z = transform.position.z;
				transform.position = vector;
				
				PlayerController player = target.gameObject.GetComponent<PlayerController>();
				player.checkPoint = zone.playerRespawnPoint;
				
				if (!visitedZones.Contains(zone))
				{
					checkPointCount ++;
					visitedZones.Add(zone);
				}
			}
		}
		
		if (!zoneTarget)
		{
			Vector3 vector = Vector3.Lerp(transform.position, target.position, snappiness);
			vector.z = transform.position.z;
			transform.position = vector;
		}
		
	}
	
	float easeInOutCubic(float x)
	{
		return x < 0.5f ? 4f * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
	}
}

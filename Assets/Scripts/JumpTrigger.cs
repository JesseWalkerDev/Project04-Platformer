using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpTrigger : MonoBehaviour
{
	public float maxInactiveTime = 2f;
	public float timeSinceJump = 0f;
	public bool active
	{
		get { return timeSinceJump > maxInactiveTime;}
	}
	
	public float jumpForce = 11f;

	// Start is called before the first frame update
	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		timeSinceJump += Time.deltaTime;
		if (active)
			GetComponent<SpriteRenderer>().color = Color.white;
		else
			GetComponent<SpriteRenderer>().color = Color.blue;
	}

	public void Jump(PlayerController player)
	{
		timeSinceJump = 0f;
		
		Vector2 jumpDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		jumpDirection.Normalize();
		jumpDirection *= 11f;
		if (jumpDirection.y > -0.9f)
			jumpDirection.y += 5f;
		
		Rigidbody2D rigidbody = player.GetComponent<Rigidbody2D>();
		rigidbody.velocity = jumpDirection;
	}
}

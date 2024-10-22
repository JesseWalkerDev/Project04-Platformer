using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	Rigidbody2D rBody;
	SpriteRenderer sprite;
	
	[Range(0f, 1f)]
	public float drag = 0.04f;
	public float acceleration = 13f;
	public float topSpeed = 8f;
	public float maxJumpTime = 0.2f;
	
	private bool grounded = false;
	private List<GameObject> jumpables = new List<GameObject>();
	private bool jumping = false;
	private float jumpTime = 0f;
	
	//TODO: Add a frozen mode that other objects can toggle (jump triggers will freeze the player for a few frames to give the player time to input a direction)
	
	// Start is called before the first frame update
	void Start()
	{
		rBody = gameObject.GetComponent<Rigidbody2D>();
		sprite = gameObject.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update()
	{
		if (jumpables.Count > 0)
			sprite.color = Color.red;
		else
			sprite.color = Color.white;
		
		if (Input.GetKeyDown(KeyCode.C))
		{
			// Activate jumpable
			if (jumpables.Count > 0)
			{
				JumpTrigger jumpTrigger = jumpables[0].GetComponent<JumpTrigger>();
				if (jumpTrigger.active)
					jumpTrigger.Jump(this);
			}
			// Start a jump
			else if (grounded)
			{
				rBody.velocity = new Vector2(rBody.velocity.x, 10);
				jumping = true;
				jumpTime = 0f;
			}
		}
	}
	
	void FixedUpdate()
	{
		// Move
		rBody.AddForce(new Vector2(Input.GetAxisRaw("Horizontal") * acceleration, 0));
		
		// Drag
		if (grounded & Input.GetAxisRaw("Horizontal") == 0f)
			rBody.velocity = new Vector2(rBody.velocity.x * (1f - drag), rBody.velocity.y);
		
		// Gravity
		rBody.AddForce(Vector2.down * 30);
		
		// End a jump
		//if (Input.GetKeyUp(KeyCode.C))
		//	jumping = false;
		
		// Jump
		if (Input.GetKey(KeyCode.C))
		{
			// Continue a jump
			if (jumping & jumpTime < maxJumpTime)
			{
				rBody.velocity = new Vector2(rBody.velocity.x, 10);
				jumpTime += Time.fixedDeltaTime;
			}
		}
		else
			jumping = false;
	}
	
	void OnCollisionStay2D(Collision2D collisionInfo)
    {
		grounded = false;
        foreach (ContactPoint2D contact in collisionInfo.contacts)
        {
        	// Debug-draw all contact points and normals
            Debug.DrawRay(contact.point, contact.normal * 0.5f, Color.red);
			if (contact.normal.y >= 0.5)
				grounded = true;
        }
    }
	
	void OnTriggerEnter2D(Collider2D collider)
	{
		if (collider.gameObject.layer == LayerMask.NameToLayer("Jumpable"))
			jumpables.Add(collider.gameObject);
	}
	
	void OnTriggerExit2D(Collider2D collider)
	{
		jumpables.Remove(collider.gameObject);
	}
	
	void OnCollisionExit2D()
	{
		grounded = false;
	}
}

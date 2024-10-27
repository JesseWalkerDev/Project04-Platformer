using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public Rigidbody2D rbody;
	public SpriteRenderer sprite;
	public Animator animator;
	
	[Range(0f, 1f)]
	public float drag = 0.04f;
	public float wallSlideDrag = 0.16f;
	public float acceleration = 0.1f;
	public float topSpeed = 8f;
	public float maxJumpTime = 0.2f;
	public float maxBufferTime = 0.2f;
	[HideInInspector]
	public Vector2 checkPoint;
	
	private bool grounded = false;
	private bool wallSlidingLeft = false;
	private bool wallSlidingRight = false;
	private bool wallJumping = false;
	private List<GameObject> jumpables = new();
	private bool jumping = false;
	private float jumpTime = 0f;
	private float bufferTime = 0f;
	private Vector2 previousVelocity = Vector2.zero;
	
	private bool jumpInputDown { get {return Input.GetKeyDown(KeyCode.C) | Input.GetKeyDown(KeyCode.Space);} }
	private bool jumpInput { get {return Input.GetKey(KeyCode.C) | Input.GetKey(KeyCode.Space);} }
	private bool jumpInputUp { get {return Input.GetKeyUp(KeyCode.C) | Input.GetKeyUp(KeyCode.Space);} }
	private float horizontalInput { get {return Input.GetAxisRaw("Horizontal");} }
	
	//TODO: Add a frozen mode that other objects can toggle (jump triggers will freeze the player for a few frames to give the player time to input a direction)
	
	void Start()
	{
		rbody = gameObject.GetComponent<Rigidbody2D>();
		sprite = gameObject.GetComponent<SpriteRenderer>();
		animator = gameObject.GetComponent<Animator>();
		
		checkPoint = transform.position;
	}
	
	void Update()
	{
		if (wallSlidingLeft)
			sprite.color = Color.cyan;
		else if (wallSlidingRight)
			sprite.color = Color.green;
		else
			sprite.color = Color.white;
		
		// Begin a buffer
		if (jumpInputDown | (bufferTime > 0f & bufferTime < maxBufferTime))
			bufferTime += Time.deltaTime; // probably shouldn't do this
		
		// Activate jumpable
		if (jumpables.Count > 0 & (jumpInputDown | bufferTime > 0f)) // Input can be buffered indefinitely
		{
			JumpTrigger jumpTrigger = jumpables[0].GetComponent<JumpTrigger>();
			if (jumpTrigger.active)
				jumpTrigger.Jump(this);
				bufferTime = 0f;
		}
		else if (jumpInputDown | (bufferTime > 0f & bufferTime < maxBufferTime)) // Input can be buffered until maxBufferTime
		{
			// Start a jump
			if (grounded | wallSlidingLeft | wallSlidingRight)
			{
				jumping = true;
				jumpTime = 0f;
				bufferTime = 0f;
			}
			// Regular jump
			if (grounded)
			{
				rbody.velocity = new Vector2(rbody.velocity.x, 10);
			}
			// Left wall jump
			else if (wallSlidingLeft)
			{
				rbody.velocity = new Vector2(topSpeed, 10);
				wallJumping = true;
			}
			// Right wall jump
			else if (wallSlidingRight)
			{
				rbody.velocity = new Vector2(-topSpeed, 10);
				wallJumping = true;
			}
		}
		
		// Continue a buffer
		if (jumpInput & bufferTime > 0f)
			bufferTime += Time.deltaTime;
		// End a buffer
		if (jumpInputUp)
			bufferTime = 0f;
		
		// Update visuals
		animator.SetFloat("xSpeed", Math.Abs(rbody.velocity.x));
		animator.SetBool("grounded", grounded);
		if (horizontalInput < 0f)
			sprite.flipX = true;
		else if (horizontalInput > 0f)
			sprite.flipX = false;
	}
	
	void FixedUpdate()
	{	
		// Move
		if (!(wallJumping & jumpTime < maxJumpTime)) // Cannot immediately move towards wall after wall jumping
		{
			float xVelocity = rbody.velocity.x;
			float horizontalSpeed = Mathf.Clamp(
				xVelocity + horizontalInput * acceleration,
				Mathf.Min(xVelocity, -topSpeed),
				Mathf.Max(xVelocity, topSpeed)
			);
			rbody.velocity = new Vector2(horizontalSpeed, rbody.velocity.y);
		}
		
		// Drag
		if (grounded & horizontalInput == 0f)
			rbody.velocity = new Vector2(rbody.velocity.x * (1f - drag), rbody.velocity.y);
		
		// Wall slide drag
		if (((wallSlidingLeft & horizontalInput < 0f) | (wallSlidingRight & horizontalInput > 0f)) & rbody.velocity.y < 0f)
			rbody.velocity = new Vector2(rbody.velocity.x, rbody.velocity.y * (1f - wallSlideDrag));
		
		// Gravity
		rbody.AddForce(Vector2.down * 30);
		
		// Continue a jump
		if (jumpInput)
		{
			
			if (jumping & jumpTime < maxJumpTime)
			{
				rbody.velocity = new Vector2(rbody.velocity.x, 10);
				jumpTime += Time.fixedDeltaTime;
			}
		}
		// End a jump
		else
		{
			jumping = false;
			wallJumping = false;
		}
		
		previousVelocity = rbody.velocity;
	}
	
	void OnCollisionEnter2D(Collision2D collision)
	{
		CeilingCut(collision);
	}
	
	void OnCollisionStay2D(Collision2D collision)
    {
        grounded = false;
		wallSlidingLeft = false;
		wallSlidingRight = false;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Debug-draw all contact points and normals

            Debug.DrawRay(contact.point, contact.normal * 0.5f, Color.red);
            if (contact.normal.y >= 0.5)
                grounded = true;
			if (contact.normal.x > 0.8)
                wallSlidingLeft = true;
			if (contact.normal.x < -0.8)
                wallSlidingRight = true;
        }
		if (grounded)
		{
			wallSlidingLeft = false;
			wallSlidingRight = false;
		}
		
        CeilingCut(collision);
    }
	
    void OnCollisionExit2D()
	{
		grounded = false; // This is probably stupid
		wallSlidingLeft = false;
		wallSlidingRight = false;
	}
	
	void OnTriggerEnter2D(Collider2D collider)
	{
		// Come into contact with a jump trigger
		if (collider.gameObject.layer == LayerMask.NameToLayer("Jumpable"))
			jumpables.Add(collider.gameObject);
		// Come into contact with a hazard
		else if (collider.gameObject.layer == LayerMask.NameToLayer("Hazard"))
		{
			rbody.position = checkPoint;
			rbody.velocity = Vector2.zero;
		}
	}
	
	void OnTriggerExit2D(Collider2D collider)
	{
		jumpables.Remove(collider.gameObject);
	}
	
	private void CeilingCut(Collision2D collision)
	{
		bool ceilingHit = false;
		foreach (ContactPoint2D contact in collision.contacts)
		{
			if (contact.normal.y < -0.5)
			{
				// Look for gap in ceiling
				float squeezing = 0.5f;
				Vector2 origin = transform.position;
				Vector2 size = new Vector2(squeezing, 1f); // * transform.lossyScale
				RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, Vector2.up, 1f, LayerMask.GetMask("Ground"));
				if (hit)
				{
					ceilingHit = true;
				}
				else
				{
					// Test size of gap
					origin.y += 1f;
					float distance = 0.5f;// - size.x / 2.0f;
					RaycastHit2D left = Physics2D.BoxCast(origin, size, 0f, Vector2.left, distance, LayerMask.GetMask("Ground"));
					RaycastHit2D right = Physics2D.BoxCast(origin, size, 0f, Vector2.right, distance, LayerMask.GetMask("Ground"));
					float leftPoint = left ? left.point.x : origin.x - (0.5f + (squeezing / 2.0f));
					float rightPoint = right ? right.point.x : origin.x + (0.5f + (squeezing / 2.0f));
					// Squeeze player into gap
					if (rightPoint - leftPoint > 1.0f)
					{
						//Debug.Log("Cut");
						rbody.position = new(Mathf.Clamp(rbody.position.x, leftPoint + 0.5f, rightPoint - 0.5f), rbody.position.y + 0.1f);
						rbody.velocity = previousVelocity;
					}
					else if (rightPoint - leftPoint > 0.9f)
					{
						//Debug.Log("Squeeze");
						rbody.position = new((leftPoint + 0.5f + rightPoint - 0.5f) / 2.0f, rbody.position.y + 0.1f);
						rbody.velocity = previousVelocity;
					}
					else
						//Debug.Log("Squeeze/Cut Failed");
						ceilingHit = true;
				}
				break;
			}
		}
		// Hit ceiling
		if (ceilingHit)
		{
			jumping = false;
			//rbody.velocity = new(rbody.velocity.x, -10f);
			//Debug.Log("Ceiling Hit");
		}
	}

}

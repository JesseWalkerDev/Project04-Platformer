using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
	Rigidbody2D rbody;
	SpriteRenderer sprite;
	
	[Range(0f, 1f)]
	public float drag = 0.04f;
	public float acceleration = 13f;
	public float topSpeed = 8f;
	public float maxJumpTime = 0.2f;
	
	private bool grounded = false;
	private List<GameObject> jumpables = new();
	private bool jumping = false;
	private float jumpTime = 0f;
	private Vector2 previousVelocity = Vector2.zero;
	
	//TODO: Add a frozen mode that other objects can toggle (jump triggers will freeze the player for a few frames to give the player time to input a direction)
	
	void Start()
	{
		rbody = gameObject.GetComponent<Rigidbody2D>();
		sprite = gameObject.GetComponent<SpriteRenderer>();
	}
	
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
				rbody.velocity = new Vector2(rbody.velocity.x, 10);
				jumping = true;
				jumpTime = 0f;
			}
		}
	}
	
	void FixedUpdate()
	{
		// Move
		rbody.AddForce(new Vector2(Input.GetAxisRaw("Horizontal") * acceleration, 0));
		
		// Drag
		if (grounded & Input.GetAxisRaw("Horizontal") == 0f)
			rbody.velocity = new Vector2(rbody.velocity.x * (1f - drag), rbody.velocity.y);
		
		// Gravity
		rbody.AddForce(Vector2.down * 30);
		
		// End a jump
		//if (Input.GetKeyUp(KeyCode.C))
		//	jumping = false;
		
		// Jump
		if (Input.GetKey(KeyCode.C))
		{
			// Continue a jump
			if (jumping & jumpTime < maxJumpTime)
			{
				rbody.velocity = new Vector2(rbody.velocity.x, 10);
				jumpTime += Time.fixedDeltaTime;
			}
		}
		else
			jumping = false;
		
		previousVelocity = rbody.velocity;
	}
	
	void OnCollisionEnter2D(Collision2D collision)
	{
		CeilingCut(collision);
	}
	
	void OnCollisionStay2D(Collision2D collision)
    {
        grounded = false;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Debug-draw all contact points and normals

            Debug.DrawRay(contact.point, contact.normal * 0.5f, Color.red);
            if (contact.normal.y >= 0.5)
                grounded = true;
        }
		
        CeilingCut(collision);
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
		// Bounce off ceiling
		if (ceilingHit)
		{
			jumping = false;
			//rbody.velocity = new(rbody.velocity.x, -10f);
			//Debug.Log("Ceiling Hit");
		}
    }

    void OnCollisionExit2D()
	{
		grounded = false;
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
	
	
}

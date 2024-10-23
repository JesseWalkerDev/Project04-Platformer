using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpTrigger : MonoBehaviour
{
	public SpriteRenderer spriteRenderer;
	public List<Sprite> sprites = new();
	public float jumpStrength = 11f;
	public float maxInactiveTime = 2f;
	
	private float timeSinceJump;
	private Vector2 jumpDirection;
	
	public bool active
	{
		get { return timeSinceJump > maxInactiveTime;}
	}
	
	private float animationValue
	{
		get
		{
			float t = Mathf.Min(timeSinceJump / maxInactiveTime, 1f);
			return 4 * (t - Mathf.Sqrt(t)) + 1;
		}
	}
	
	// Start is called before the first frame update
	void Start()
	{
		timeSinceJump = maxInactiveTime;
	}

	// Update is called once per frame
	void Update()
	{
		timeSinceJump += Time.deltaTime;
		/*
		if (active)
			spriteRenderer.color = Color.white;
		else
			spriteRenderer.color = Color.blue;
		*/
		
		//animator.SetBool("Active", active);
		spriteRenderer.sprite = sprites[Mathf.FloorToInt((1f - animationValue) * sprites.Count)];
		spriteRenderer.transform.localPosition = jumpDirection * -1.5f * (1 - animationValue);
	}

	public void Jump(PlayerController player)
	{
		timeSinceJump = 0f;
		
		jumpDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		jumpDirection.Normalize();
		Vector2 jumpForce = jumpDirection * jumpStrength;
		if (jumpForce.y > -0.9f)
			jumpForce.y += 5f;
		
		Rigidbody2D rigidbody = player.GetComponent<Rigidbody2D>();
		rigidbody.velocity = jumpForce;
	}
}

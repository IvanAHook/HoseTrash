using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour
{
	public KeyCode Left;
	public KeyCode Right;
	public KeyCode Jump;
	public KeyCode Hose;

	private float _speed = 5;
	private float _jumpForce = 240;
	private bool _facingRight = true;

	[SerializeField]private ParticleSystem _water;

	private Rigidbody2D _rigidBody;

	void Awake()
	{
		_water.Stop();
	}

	void Start ()
	{
		_rigidBody = GetComponent<Rigidbody2D>();
	}
	
	void FixedUpdate ()
	{
		var currentVelocity = _rigidBody.velocity;
		currentVelocity.x = 0;
		//_water.Stop ();
		if (Input.GetKey(Left))
		{
			//IMovement.MoveLeft();
			currentVelocity.x = -1 * _speed;
		}
		if (Input.GetKey(Right))
		{
			//IMovement.MoveRight();
			currentVelocity.x = 1 * _speed;
		}
		if (Input.GetKeyDown(Jump))
		{
			_rigidBody.AddForce(new Vector2 (0, _jumpForce));
		}
		if (Input.GetKeyDown(Hose))
		{
			//Hose
			_water.Play();
		}
		if (Input.GetKeyUp(Hose))
		{
			_water.Stop();
		}
		_rigidBody.velocity = currentVelocity;
	}

	private void Flip()
	{
		_facingRight = !_facingRight;
		var scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;
	}

	private bool IsGrounded()
	{
		RaycastHit hit;
		float distance = 1f;
		Vector3 dir = new Vector3(0, -1);

		if(Physics.Raycast(transform.position, dir, out hit, distance))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

}

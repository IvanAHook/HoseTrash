using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour
{
	public KeyCode Left;
	public KeyCode Right;
	public KeyCode Jump;
	public KeyCode Hose;

	private float _speed = 240;
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
		if (Input.GetKey(Left))
		{
			Debug.Log("Left");
			if (_facingRight) Flip();
			//IMovement.MoveLeft();
			currentVelocity.x = -1 * _speed * Time.deltaTime;
		}
		if (Input.GetKey(Right))
		{
			Debug.Log("Right");
			if (!_facingRight) Flip();
			//IMovement.MoveRight();
			currentVelocity.x = 1 * _speed * Time.deltaTime;
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
		Debug.Log("Flip");
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
		return false;

	}

}

using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour
{
	public KeyCode Left;
	public KeyCode Right;
	public KeyCode Hose;

	private RigidBody2D rigidBody;

	void Start ()
	{
		rigidBody = GetComponent<RigidBody2D>();
	}
	
	void Update ()
	{
		var currentVelocity = rigidBody.velocity;
		currentVelocity.x = 0;
		if (Input.GetKeyDown(Left))
		{
			//IMovement.MoveLeft();
			currentVelocity.x = -1;
		}
		if (Input.GetKeyDown(Right))
		{
			//IMovement.MoveRight();
			currentVelocity.x = 1;
		}
		if (Input.GetKeyDown(Hose))
		{
			//Hose
		}
		rigidBody.velocity = currentVelocity;
	}
}

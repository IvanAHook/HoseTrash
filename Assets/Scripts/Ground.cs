using UnityEngine;
using System.Collections;

public class Ground : MonoBehaviour {

	private bool _watered;

	void OnParticleCollision(GameObject other)
	{
		if (!_watered)
		{
			_watered = true;
			GetComponent<SpriteRenderer>().color = Color.blue;
		}
	}
}

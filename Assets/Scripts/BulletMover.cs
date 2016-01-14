using UnityEngine;
using System.Collections;

public class BulletMover : MonoBehaviour 
{
	public float speed;

	private void Start()
	{
		GetComponent<Rigidbody>().velocity = speed * transform.up;
	}
}

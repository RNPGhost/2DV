using UnityEngine;
using System.Collections;

public class NetworkBulletMovement : Photon.MonoBehaviour {

	private Rigidbody rb;
	
	void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}
	
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			stream.SendNext(rb.position);
			stream.SendNext(rb.velocity);
			stream.SendNext(rb.rotation);
		}
		if (stream.isReading)
		{
			rb.position = (Vector3)stream.ReceiveNext();
			rb.velocity = (Vector3)stream.ReceiveNext();
			rb.rotation = (Quaternion)stream.ReceiveNext();
		}
	}
}

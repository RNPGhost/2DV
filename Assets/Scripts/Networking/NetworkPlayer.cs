using UnityEngine;
using System.Collections;

public class NetworkPlayer : Photon.MonoBehaviour {

	public GameObject myCamera;

	void Start () 
	{
		if (photonView.isMine)
		{
			gameObject.name = "Me";
			myCamera.SetActive(true);
			GetComponent<PlayerMovement>().enabled = true;
			GetComponent<GunController>().enabled = true;
			GetComponent<Rigidbody>().useGravity = true;
		}
		else
		{
			gameObject.name = "Network Player";
			GetComponent<Rigidbody>().isKinematic = true;
		}
	}
}

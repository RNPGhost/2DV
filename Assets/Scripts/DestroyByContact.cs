using UnityEngine;
using System.Collections;

public class DestroyByContact : Photon.MonoBehaviour 
{
	
	GameObject gameControllerObject;
	GameController gameController;

	private void Start()
	{
		gameControllerObject = GameObject.FindGameObjectWithTag("GameController");
		gameController = gameControllerObject.GetComponent<GameController>();
	}

	private void OnTriggerEnter(Collider other) 
	{
		if (other.tag == "Boundary")
		{
			return;
		}

		// if my bullet hits my player, don't do anything
		// if someone else's bullet hits my player, report I've been hit to everyone
		PhotonView otherPhotonView = other.GetComponent<PhotonView>();
		if (other.tag == "Player" && otherPhotonView != null && otherPhotonView.isMine)
		{
			if (photonView.isMine)
			{
				return;
			}

			gameController.GetComponent<PhotonView>().RPC ("Hit", PhotonTargets.All, otherPhotonView.viewID);
		}

		Destroy(gameObject);
	}
}

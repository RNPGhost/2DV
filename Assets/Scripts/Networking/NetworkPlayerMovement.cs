using UnityEngine;
using System.Collections;

public class NetworkPlayerMovement : Photon.MonoBehaviour {

	private Rigidbody rb;
	private float lastSynchronizationTime = 0f;
	private float syncDelay = 0f;
	private float syncTime = 0f;
	private Vector3 syncStartPosition;
	private Vector3 syncEndPosition;
	private Quaternion syncStartRotation;
	private Quaternion syncEndRotation;

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		syncStartPosition = rb.position;
		syncEndPosition = rb.position;
		syncStartRotation = rb.rotation;
		syncEndRotation = rb.rotation;
	}

	void Update()
	{
		if (!photonView.isMine)
		{
			SyncedMovement();
		}
	}
	
	private void SyncedMovement()
	{
		syncTime += Time.deltaTime;
		rb.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
		rb.rotation = Quaternion.Lerp(syncStartRotation, syncEndRotation, syncTime/ syncDelay);
	}
	
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			stream.SendNext(rb.position);
			stream.SendNext(rb.rotation);
		}
		if (stream.isReading)
		{
			syncEndPosition = (Vector3)stream.ReceiveNext();
			syncEndRotation = (Quaternion)stream.ReceiveNext();

			syncTime = 0f;
			syncDelay = Time.time - lastSynchronizationTime;
			lastSynchronizationTime = Time.time;

			syncStartPosition = rb.position;
			syncStartRotation = rb.rotation;
		}
	}
}

using UnityEngine;
using System.Collections;

public class GunController : MonoBehaviour {

	public Transform gunBarrelEnd;
	public GameObject bullet;
	public float fireRate;
	public float initialCeaseFireTime;
	public Camera cam;
	public GameObject gun;
	public Transform hipFirePosition;
	public Transform player;

	private float nextFire;
	private GameController gameController;

	void Start()
	{
		GameObject gameControllerObject = GameObject.FindGameObjectWithTag("GameController");
		gameController = gameControllerObject.GetComponent<GameController>();

		nextFire = Time.time + initialCeaseFireTime;
	}

	// Update is called once per frame
	void Update () 
	{
		if (gameController.Paused) { return; }

		UpdateGunOrientation();

		if (Input.GetButton("Fire1") && Time.time > nextFire)
		{
			nextFire = Time.time + 1f / fireRate;
			gameController.GetComponent<PhotonView>().RPC ("BulletFired", PhotonTargets.All, null);
			PhotonNetwork.Instantiate(bullet.name, gunBarrelEnd.position + bullet.transform.lossyScale.y * gun.transform.forward, gunBarrelEnd.rotation, 0);
		}
	}

	void UpdateGunOrientation()
	{
		Ray crosshairRay = new Ray();
		RaycastHit crosshairRayHit;
		float rayRange = 1000;
		Vector3 target;

		crosshairRay.origin = cam.transform.position;
		crosshairRay.direction = cam.transform.forward;

		if (Physics.Raycast (crosshairRay, out crosshairRayHit, rayRange)) 
		{
			target = crosshairRayHit.point;
		}
		else
		{
			target = crosshairRay.origin + rayRange * crosshairRay.direction;
		}

		gun.transform.position = hipFirePosition.position;
		gun.transform.rotation = Quaternion.LookRotation(target - hipFirePosition.position);
    }
}

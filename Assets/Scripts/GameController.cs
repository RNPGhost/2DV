using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameController : Photon.MonoBehaviour {

	public float slowMotionFactor;
	public float slowMotionDuration;
	public GameObject playerPrefab;
	public GameObject player;
	public Transform player1SpawnPoint;
	public Transform player2SpawnPoint;
	public float deathCamTime;
	public float restartLevelTime;
	public GameObject inGameGUI;
	public GameObject notificationGUI;
	public GameObject background;
	public Text topText;
	public Text bottomText;

	private bool paused;
	private bool noShotsFired;
	private bool slowMotion;
	private float slowMotionStartSpeed;
	private float initialFixedDeltaTime;
	private bool firstHit;
	private string roomName;

    public bool Paused
    {
        get { return paused; }
    }

    // LEVEL INITIALIZATION

    private void Start()
    {
        ResetFlags();
        InitializeTime();

        ActivateNotificationGUI(true, "2DV", "the slow motion wallrunning fps");
    }

    private void ResetFlags()
    {
        noShotsFired = true;
        firstHit = true;
        paused = false;
    }

    private void InitializeTime()
    {
        slowMotionStartSpeed = 1 / slowMotionFactor;
        initialFixedDeltaTime = Time.fixedDeltaTime;

        ResetTime();
    }

    private void ResetTime()
    {
        slowMotion = false;
        Time.timeScale = 1;
        UpdateFixedDeltaTime();
    }

    // ROOMS

    public void OnJoinRoom(string roomNameString)
    {
        roomName = roomNameString;

        if (PhotonNetwork.playerList.Length == 2)
        {
            photonView.RPC("RestartLevel", PhotonTargets.MasterClient, null);
        }
        else
        {
            ActivateNotificationGUI(true, roomName, "waiting for 2nd player");
        }
    }

    public void PlayerDisconnected()
    {
        photonView.RPC("ResetLevel", PhotonTargets.All, null);
        ActivateNotificationGUI(true, roomName, "player disconnected - waiting for 2nd player");
    }

    // LEVEL INITIALIZATION

    [PunRPC]
    void RestartLevel()
    {
        StartCoroutine(RestartLevelCoroutine());
    }

    private IEnumerator RestartLevelCoroutine()
    {
        photonView.RPC("ResetLevel", PhotonTargets.All, null);

        yield return new WaitForSeconds(restartLevelTime);

        photonView.RPC("InitializePlayer", PhotonTargets.All, null);
    }

    [PunRPC]
    void ResetLevel()
    {
        ActivateNotificationGUI(true, "Initializing Level", "please wait");

        if (PhotonNetwork.player.isMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }

        ResetFlags();
        ResetTime();
    }

    // PLAYER INITIALIZATION

    [PunRPC]
    void InitializePlayer()
    {
        Transform playerSpawnPoint;
        if (PhotonNetwork.player.isMasterClient)
        {
            playerSpawnPoint = player1SpawnPoint;
        }
        else
        {
            playerSpawnPoint = player2SpawnPoint;
        }

        player = PhotonNetwork.Instantiate(
            playerPrefab.name, playerSpawnPoint.position, playerSpawnPoint.rotation, 0) as GameObject;

        // Prevent the camera from seeing the player's body
        int playerBodyLayer = LayerMask.NameToLayer("PlayerBody");
        SetLayerRecursively(player.transform.Find("Body Parts").gameObject, playerBodyLayer);
        player.GetComponent<PlayerMovement>().InitializeCamera(playerBodyLayer);

        LockCursor();
        ActivateInGameGUI();
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (null == obj)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // SLOW MOTION INITIALIZATION

    [PunRPC]
    public void BulletFired()
    {
        if (noShotsFired)
        {
            noShotsFired = false;
            InitiateSlowMotion();
        }
    }

    private void InitiateSlowMotion()
    {
        slowMotion = true;
        Time.timeScale = slowMotionStartSpeed;
        UpdateFixedDeltaTime();
    }

    // PLAYER HIT

    [PunRPC]
    public void Hit(int photonViewID)
    {
        if (firstHit)
        {
            firstHit = false;
            ResetTime();

            if (player != null && player.GetComponent<PhotonView>().viewID == photonViewID)
            {
                StartCoroutine(LosingCoroutine());
            }
            else
            {
                ActivateNotificationGUI(false, "You Win", "waiting to restart level");
            }
        }
    }

    private IEnumerator LosingCoroutine()
    {
        ActivateNotificationGUI(false, "You Died", "waiting to restart level");

        PhotonNetwork.Destroy(player);

        yield return new WaitForSeconds(deathCamTime);

        photonView.RPC("RestartLevel", PhotonTargets.MasterClient, null);
    }

    // GUI

    private void ActivateInGameGUI()
    {
        ClearGUI();
        inGameGUI.SetActive(true);
    }

	private void ActivateNotificationGUI(bool backgroundEnabled, string topTextString, string bottomTextString)
	{
		ClearGUI();
		background.SetActive(backgroundEnabled);
		topText.text = topTextString;
		bottomText.text = bottomTextString;
		notificationGUI.SetActive(true);
	}

    private void ClearGUI()
    {
        inGameGUI.SetActive(false);
        notificationGUI.SetActive(false);
        topText.text = "";
        bottomText.text = "";
        background.SetActive(false);
    }

    // UPDATE
    
	private void Update()
	{
		if (slowMotion)
		{
			if (Time.timeScale >= 1)
			{
				Time.timeScale = 1;
				slowMotion = false;
			}
			else
			{
				Time.timeScale += (1 - slowMotionStartSpeed) * Time.deltaTime / slowMotionDuration;
				UpdateFixedDeltaTime();
			}
		}
	}
    
    private void UpdateFixedDeltaTime()
    {
        Time.fixedDeltaTime = initialFixedDeltaTime * Time.timeScale;
    }
}

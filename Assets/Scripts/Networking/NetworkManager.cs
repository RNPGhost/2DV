using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour {

	private const string gameVersion = "6.0";
	private GameController gameController;
	private RoomInfo[] roomsList;
	private bool inLobby = false;
	private string roomName;

	void Start()
	{
		GameObject gameControllerObject = GameObject.FindGameObjectWithTag("GameController");
		gameController = gameControllerObject.GetComponent<GameController>();
		PhotonNetwork.ConnectUsingSettings(gameVersion);
	}

	void OnGUI()
	{
		if (PhotonNetwork.connected && inLobby && PhotonNetwork.room == null)
		{
			// Create Room
			if (GUI.Button(new Rect(100, 100, 250, 100), "Create New Room"))
			{
				if (roomsList == null || roomsList.Length == 0)
				{
					roomName = "0";
				}
				else
				{
					roomName = (HighestRoomNumber() + 1).ToString();
				}

				PhotonNetwork.CreateRoom(roomName, new RoomOptions() { maxPlayers = 2 }, TypedLobby.Default);
			}
			
			// Join Room
			if (roomsList != null)
			{
				for (int i = 0; i < roomsList.Length; i++)
				{
					if (GUI.Button(new Rect(100, 250 + (110 * i), 250, 100), "Join " + GetRoomDisplayName(roomsList[i].name)))
					{
						roomName = roomsList[i].name;
						PhotonNetwork.JoinRoom(roomsList[i].name);
					}
				}
			}
		}
	}
	
	void OnReceivedRoomListUpdate()
	{
		roomsList = PhotonNetwork.GetRoomList();
	}

	void OnJoinedLobby()
	{
		inLobby = true;
	}

	void OnJoinedRoom()
	{
		gameController.OnJoinRoom(GetRoomDisplayName(roomName));
	}

	void OnPhotonPlayerDisconnected (PhotonPlayer otherPlayer)
	{
		gameController.PlayerDisconnected();
	}

	int HighestRoomNumber()
	{
		bool firstRoom = true;
		int highestRoomNumber = -1;

		foreach (RoomInfo room in roomsList)
		{
			int roomNumber = Int32.Parse(room.name);
			if (roomNumber < highestRoomNumber || firstRoom)
			{
				highestRoomNumber = roomNumber;
				firstRoom = false;
			}
		}
		return highestRoomNumber;
	}

	string GetRoomDisplayName(string name)
	{
		return ("Room " + name);
	}
}

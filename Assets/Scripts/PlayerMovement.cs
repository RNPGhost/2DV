using UnityEngine;
using System.Collections;

[RequireComponent(typeof (Rigidbody))]
[RequireComponent(typeof (CapsuleCollider))]
public class PlayerMovement : MonoBehaviour {

	public float speed;
	public float jumpForce;
	public float jumpStartupLength;

	public Camera cam;
	public MouseLook mouseLook = new MouseLook();
    
	private GameController gameController;
	private Rigidbody rb;
	private float yRotation;
	private Vector3 groundNormal = new Vector3(0f, 1f, 0f);
	private Vector3 contactNormal; // contactNormal must always have a positive y value
    private ArrayList newCollisions = new ArrayList();
	private ArrayList exitedObjects = new ArrayList();
	private ArrayList collisions = new ArrayList();
	private bool contactsChanged = false;

	private bool jump, onMoveEnabledSurface;
	private float jumpStartupTimer = 0;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		contactNormal = groundNormal;
		GameObject gameControllerObject = GameObject.FindWithTag("GameController");
		gameController = gameControllerObject.GetComponent<GameController>();
		mouseLook.Init (transform, cam.transform);
	}
    
	public void InitializeCamera(int playerLayer)
	{
		cam.cullingMask = playerLayer;
	}

    // Update is called every frame
	private void Update()
	{
        mouseLook.LookRotation(transform, cam.transform);
        jump = Input.GetButton("Jump");
	}

    // FixedUpdate is called every physics update
	private void FixedUpdate()
	{
		if (gameController.Paused) 
		{ 
			rb.velocity = Vector3.zero;
			return; 
		}

		if (contactsChanged)
		{
			UpdateContacts();
		}

		Vector2 input = GetInput();

		if (jumpStartupTimer > 0)
		{
			jumpStartupTimer -= Time.deltaTime;
		}
		else 
		{
			if (onMoveEnabledSurface)
			{
				Vector3 desiredMove = cam.transform.forward*input.y + cam.transform.right*input.x; 
				desiredMove = Vector3.ProjectOnPlane(desiredMove, groundNormal).normalized;
				
				Vector3 projectedInput = new Vector3(desiredMove.x, desiredMove.y, desiredMove.z);
				
				// projects the movement onto a plane half way between the ground and the contact plane
				// then onto the contact plan itself
				// this allows projection of input onto vertical walls
				if (contactNormal.normalized != groundNormal) 
				{
					desiredMove = Vector3.ProjectOnPlane(desiredMove, (groundNormal + contactNormal).normalized);
					desiredMove = Vector3.ProjectOnPlane(desiredMove, contactNormal);
				}
				
				desiredMove = desiredMove.normalized * speed;
				
				if (jump)
				{
					Vector3 jumpDirection = (groundNormal + projectedInput).normalized;
					desiredMove = jumpForce * jumpDirection;
					jumpStartupTimer = jumpStartupLength;
				}
				
				rb.velocity = desiredMove;
			}
		}

	}

	private Vector2 GetInput()
	{
		return new Vector2
        {
            x = Input.GetAxisRaw("Horizontal"),
            y = Input.GetAxisRaw("Vertical")
        };
    }

	private void UpdateContacts()
	{
		// Add any new collisions detected to the list of collisions
		ArrayList copyOfNewCollisions = new ArrayList(newCollisions);

		foreach (Collision collision in copyOfNewCollisions)
		{
			collisions.Add(collision);
			newCollisions.Remove(collision);
		}

		// Remove any collisions with objects that have been exited
		ArrayList copyOfExitedObjects = new ArrayList(exitedObjects);
		ArrayList copyOfCollisions = new ArrayList(collisions);

        foreach (GameObject gameObject in copyOfExitedObjects)
		{
			foreach (Collision collision in copyOfCollisions)
			{
				if (collision.gameObject == gameObject)
				{
					collisions.Remove(collision);
				}
			}
			exitedObjects.Remove(gameObject);
		}

		// Calculate new contact normal
		Vector3 newContactNormal = Vector3.zero;
		bool onMESurface = false;

		foreach (Collision collision in collisions)
		{
            Vector3 collisionNormal = Vector3.zero;
            foreach (ContactPoint contact in collision.contacts)
			{
                collisionNormal += contact.normal;
			}
		    
            if (collisionNormal != Vector3.zero && Vector3.Angle(collisionNormal, groundNormal) < 90.1f)
            {
                newContactNormal += collisionNormal.normalized;
                onMESurface = true;
            }
        }

		onMoveEnabledSurface = onMESurface;

		if (onMESurface)
		{
			contactNormal = newContactNormal.normalized;
		}

		contactsChanged = false;
	}

	private void OnCollisionEnter(Collision collision)
	{
		jumpStartupTimer = 0;
		newCollisions.Add(collision);
		contactsChanged = true;
	}

	private void OnCollisionExit(Collision collision)
	{
		exitedObjects.Add(collision.gameObject);
		contactsChanged = true;
	}
}

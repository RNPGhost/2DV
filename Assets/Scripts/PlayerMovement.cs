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
    public void Update()
	{
        mouseLook.LookRotation(transform, cam.transform);
        jump = Input.GetButton("Jump");
	}

    // FixedUpdate is called every physics update
    public void FixedUpdate()
	{
		if (gameController.Paused) 
		{ 
			rb.velocity = Vector3.zero;
			return; 
		}

		if (contactsChanged)
		{
			UpdateContactNormal();
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

	private void UpdateContactNormal()
	{
		// Calculate new contact normal
		Vector3 newContactNormal = Vector3.zero;
		bool onMESurface = false;

        lock (collisions.SyncRoot)
        {
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
        }

        onMoveEnabledSurface = onMESurface;

		if (onMESurface)
		{
			contactNormal = newContactNormal.normalized;
		}

		contactsChanged = false;
	}

    public void OnCollisionEnter(Collision collision)
	{
	    lock (collisions.SyncRoot)
	    {
            collisions.Add(collision);
        }

        jumpStartupTimer = 0;
        contactsChanged = true;
    }

    public void OnCollisionExit(Collision collision)
	{
        lock (collisions.SyncRoot)
        {
            Debug.Log(collisions.Count);
            ArrayList collisionsToRemove = new ArrayList();
            foreach (Collision c in collisions)
            {
                if (c.gameObject == collision.gameObject)
                {
                    collisionsToRemove.Add(c);
                }
            }
            foreach (Collision r in collisionsToRemove)
            {
                collisions.Remove(r);
            }
            collisionsToRemove.Clear();
            Debug.Log(collisions.Count);
        }

        contactsChanged = true;
    }
}

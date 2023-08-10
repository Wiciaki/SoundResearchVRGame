using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;
using System.Text;

public class SimpleCapsuleWithStickMovement : MonoBehaviour
{
	public bool EnableLinearMovement = true;
	public bool EnableRotation = true;
	public bool HMDRotatesPlayer = true;
	public bool RotationEitherThumbstick = false;
	public float RotationAngle = 45.0f;
	public float Speed = 0.0f;
	public OVRCameraRig CameraRig;

	private bool ReadyToSnapTurn;
	private Rigidbody _rigidbody;

	public event Action CameraUpdated;
	public event Action PreCharacterMove;

	private bool started;
	private bool finished;
	private GameObject lab1, lab2;

    private float timeCounter;

    private float distanceCounter;

    public static bool isMeasuring;

    private Vector3 lastPos, currentPos;

    private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
		if (CameraRig == null) CameraRig = GetComponentInChildren<OVRCameraRig>();
	}

    bool isCurrentlyColliding;

    void OnTriggerEnter(Collider col)
    {
		if (col.gameObject.name.StartsWith("Plane"))
		{
			isCurrentlyColliding = true;

			if (col.gameObject.CompareTag("Finish"))
			{
				finished = true;
			}
		}
    }

    void OnTriggerExit(Collider col)
    {
		if (col.gameObject.name.StartsWith("Plane"))
		{
			isCurrentlyColliding = false;
		}
    }

    void Start()
	{
		lab1 = GameObject.Find("Lab1");
        lab2 = GameObject.Find("Lab2");
		lab2.SetActive(false);
    }

    private void Update()
    {
        currentPos = transform.position;

        if (isMeasuring)
        {
            distanceCounter += Vector3.Distance(currentPos, lastPos);
            timeCounter += Time.deltaTime;
        }

        lastPos = currentPos;
    }

    private void FixedUpdate()
	{
		if (!started)
		{
            if (!ButtonAPressed())
            {
				return;
            }

			this.started = true;
			GameObject.Find("Okienko1").SetActive(false);
        }

		if (isCurrentlyColliding && ButtonAPressed())
		{
			if (lab1.activeInHierarchy)
			{
                lab1.SetActive(false);
                lab2.SetActive(true);
                GameObject.Find("Okienko2").SetActive(false);
                isMeasuring = true;
            }

			if (finished)
			{
				finished = false;
				SaveResearch();
				Application.Quit();
				return;
			}
		}

        if (CameraUpdated != null) CameraUpdated();
        if (PreCharacterMove != null) PreCharacterMove();

        if (HMDRotatesPlayer) RotatePlayerToHMD();
		if (EnableLinearMovement) StickMovement();
		if (EnableRotation) SnapTurn();
	}

	bool ButtonAPressed()
	{
		return OVRInput.Get(OVRInput.RawButton.A);
    }

	void SaveResearch()
	{
		var sb = new StringBuilder();

		sb.AppendLine("Czas przejscia: " + timeCounter);
		sb.AppendLine("Ilosc kolizji: " + CollisionSound.counter);
		sb.AppendLine("Dlugosc drogi: " + distanceCounter);
		sb.AppendLine();
		sb.AppendLine("=================");
		sb.AppendLine();
		sb.Append(CollisionSound.collisions.ToString());

		var fileName = DateTime.UtcNow.ToString("yyyy-MM-dd HH-mm") + ".txt";

        File.WriteAllText(Path.Combine(Application.persistentDataPath, fileName), sb.ToString());
    }

    void RotatePlayerToHMD()
    {
		Transform root = CameraRig.trackingSpace;
		Transform centerEye = CameraRig.centerEyeAnchor;

		Vector3 prevPos = root.position;
		Quaternion prevRot = root.rotation;

		transform.rotation = Quaternion.Euler(0.0f, centerEye.rotation.eulerAngles.y, 0.0f);

		root.SetPositionAndRotation(prevPos, prevRot);
    }

	void StickMovement()
	{
		Quaternion ort = CameraRig.centerEyeAnchor.rotation;
		Vector3 ortEuler = ort.eulerAngles;
		ortEuler.z = ortEuler.x = 0f;
		ort = Quaternion.Euler(ortEuler);

		Vector3 moveDir = Vector3.zero;
		Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
		moveDir += ort * (primaryAxis.x * Vector3.right);
		moveDir += ort * (primaryAxis.y * Vector3.forward);
		//_rigidbody.MovePosition(_rigidbody.transform.position + moveDir * Speed * Time.fixedDeltaTime);
		_rigidbody.MovePosition(_rigidbody.position + moveDir * Speed * Time.fixedDeltaTime);
	}

	void SnapTurn()
	{
		if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft) ||
			(RotationEitherThumbstick && OVRInput.Get(OVRInput.Button.PrimaryThumbstickLeft)))
		{
			if (ReadyToSnapTurn)
			{
				ReadyToSnapTurn = false;
				transform.RotateAround(CameraRig.centerEyeAnchor.position, Vector3.up, -RotationAngle);
			}
		}
		else if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight) ||
			(RotationEitherThumbstick && OVRInput.Get(OVRInput.Button.PrimaryThumbstickRight)))
		{
			if (ReadyToSnapTurn)
			{
				ReadyToSnapTurn = false;
				transform.RotateAround(CameraRig.centerEyeAnchor.position, Vector3.up, RotationAngle);
			}
		}
		else
		{
			ReadyToSnapTurn = true;
		}
	}
}

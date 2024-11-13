using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiRollBar : MonoBehaviour {

    public WheelCollider LeftWheel;
    public WheelCollider RightWheel;
    public float AntiRoll = 5000.0f;

	private Rigidbody car;

	void Start(){
		car = GetComponent<Rigidbody> ();
	}

	void FixedUpdate ()
	{
		WheelHit hit;
		float tLeft = 1.0f;
		float tRight = 1.0f;


		bool gLeft = LeftWheel.GetGroundHit (out hit);
		if (gLeft) {
            tLeft = (-LeftWheel.transform.InverseTransformPoint (hit.point).y - LeftWheel.radius) / LeftWheel.suspensionDistance;
		}

		bool groundedR = RightWheel.GetGroundHit (out hit);
		if (groundedR) {
            tRight = (-RightWheel.transform.InverseTransformPoint (hit.point).y - RightWheel.radius) / RightWheel.suspensionDistance;
		}

		float antiRollForce = (tLeft - tRight) * AntiRoll;

		if (gLeft)
			car.AddForceAtPosition (LeftWheel.transform.up * -antiRollForce, LeftWheel.transform.position);

		if (groundedR)
			car.AddForceAtPosition (RightWheel.transform.up * antiRollForce, RightWheel.transform.position);
	}
}

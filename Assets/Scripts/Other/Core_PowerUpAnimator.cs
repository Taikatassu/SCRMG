using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_PowerUpAnimator : MonoBehaviour {

    Transform floatingPart;
    Transform rotatingPart;
    Transform tiltingPart;
    Vector3 originalPosition;
    Vector3 newPosition;
    float originalY;
    float floatingDistance = 0.25f;
    float floatingSpeed = 2f;

    void Awake()
    {
        floatingPart = transform.GetChild(0);
        rotatingPart = floatingPart.GetChild(0);
        tiltingPart = rotatingPart.GetChild(0);
        originalPosition = floatingPart.position;
        originalY = floatingPart.position.y;
    }

    void OnEnable ()
    {
        floatingPart.position = originalPosition;
        newPosition = originalPosition;
    }
	
	void Update () {
        rotatingPart.Rotate(new Vector3(0, 0.25f, 0));
        tiltingPart.Rotate(new Vector3(0.25f, 0, 0));
        newPosition.y = originalY + floatingDistance * Mathf.Sin(floatingSpeed * Time.time);
        floatingPart.position = newPosition;
    }
}

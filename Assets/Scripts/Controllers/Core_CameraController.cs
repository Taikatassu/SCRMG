using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_CameraController : MonoBehaviour {

    /* TODO: Camera sway
     *      Smoother and more "organic" camera movement (currently no slerp or smooth)
     */

    Transform target;
    Transform cameraParent;
    float distance = 0;
    float height = 20.0f;

    Vector3 wantedPosition;

    bool freeMove = false;

	void Start () {
        cameraParent = transform.parent;
	}
	
	void LateUpdate () {

        if (!freeMove)
        {
            wantedPosition = target.position;
            wantedPosition.z -= distance;
            wantedPosition.y += height;

            cameraParent.position = wantedPosition;
            transform.LookAt(target.position);
        }
	}

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}

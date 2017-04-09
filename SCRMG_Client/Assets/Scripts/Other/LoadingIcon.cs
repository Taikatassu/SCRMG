using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingIcon : MonoBehaviour {

    Transform spinningIcon;
    float rotationSpeed = 3;

    private void OnEnable()
    {
        spinningIcon = transform.GetChild(0);
        spinningIcon.rotation = Quaternion.identity;
    }

    private void FixedUpdate()
    {
        spinningIcon.Rotate(new Vector3(0, 0, -rotationSpeed));
    }
}

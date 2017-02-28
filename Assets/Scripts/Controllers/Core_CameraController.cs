using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_CameraController : MonoBehaviour {

    /* TODO: Camera sway
     *      Smoother and more "organic" camera movement (currently no slerp or smooth)
     * Spectate mode (and free movement?)
     */

    Core_Toolbox toolbox;
    Core_EventManager em;

    Transform target;
    Transform cameraParent;

    Vector3 wantedPosition;
    float followDistance = 0;
    float followHeight = 20.0f;

    float spectatingHeight = 45;
    Vector3 spectatingRotation = new Vector3(90, 0, 0);
    float lerpTime = 1;
    Vector3 originalPosition;
    Vector3 spectatePosition;
    float timeStartedLerping;
    
    bool spectateMode = false;
    bool movingToSpectate = false;

    int myShipIndex = 0;

    void Awake()
    {
        toolbox = FindObjectOfType<Core_Toolbox>();
        em = toolbox.GetComponentInChildren<Core_EventManager>();
    }

	void Start () {
        cameraParent = transform.parent;
	}

    private void OnEnable()
    {
        em.OnShipDead += OnShipDead;
        em.OnGameRestart += OnGameRestart;
    }

    private void OnDisable()
    {
        em.OnShipDead -= OnShipDead;
        em.OnGameRestart -= OnGameRestart;
    }
	
	void LateUpdate () {

        if (!spectateMode)
        {
            wantedPosition = target.position;
            wantedPosition.z -= followDistance;
            wantedPosition.y += followHeight;

            cameraParent.position = wantedPosition;
            transform.LookAt(target.position);
        }

        if (movingToSpectate)
        {
            //TODO: Lerp to spectate position & rotation
            cameraParent.position = spectatePosition;
            transform.rotation = Quaternion.Euler(spectatingRotation);
            movingToSpectate = false;
        }
	}

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetMyShipIndex(int newShipIndex)
    {
        myShipIndex = newShipIndex;
    }

    public void SetSpectateMode(bool state)
    {
        spectateMode = true;
        movingToSpectate = true;

        timeStartedLerping = Time.time;
        originalPosition = cameraParent.position;
        spectatePosition = new Vector3(0, spectatingHeight, 0);    
    }

    private void OnShipDead(int shipIndex)
    {
        if (shipIndex == myShipIndex)
        {
            SetSpectateMode(true);
        }
    }

    private void OnGameRestart()
    {
        Debug.Log("CameraController: OnGameRestart");
        Destroy(cameraParent.gameObject);
    }
}

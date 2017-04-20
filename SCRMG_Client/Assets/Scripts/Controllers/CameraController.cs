using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    /* TODO: Camera sway
     *      Smoother and more "organic" camera movement (currently no slerp or smooth)
     * Spectate mode (and free movement?)
     */

    #region References & variables
    //References
    Toolbox toolbox;
    EventManager em;
    GlobalVariableLibrary lib;
    Transform target;
    Transform cameraParent;
    //Variables coming from within the script
    Vector3 wantedPosition;
    Vector3 spectatePosition;
    Vector3 spectatingRotation = new Vector3(90, 0, 0);
    //Vector3 originalPosition;
    //float timeStartedLerping;
    int myShipIndex = -1;
    bool spectateMode = false;
    bool movingToSpectate = false;
    //Variables coming from GlobalVariableLibrary
    //float lerpTime = 1;
    float spectatingHeight = -1;
    float followDistance = -1;
    float followHeight = -1;
    #endregion

    #region Initialization
    #region Awake & GetStats
    void Awake()
    {
        toolbox = FindObjectOfType<Toolbox>();
        em = toolbox.GetComponentInChildren<EventManager>();
        lib = toolbox.GetComponent<GlobalVariableLibrary>();
        GetStats();

        cameraParent = transform.parent;
    }

    void GetStats()
    {
        spectatingHeight = lib.shipVariables.cameraSpectatingHeight;
        followDistance = lib.shipVariables.cameraFollowDistance;
        followHeight = lib.shipVariables.cameraFollowHeight;
    }
    #endregion

    #region OnEnable & OnDisable
    private void OnEnable()
    {
        em.OnShipDead += OnShipDead;
        em.OnMatchEndedByServer += OnMatchEndedByServer;
    }

    private void OnDisable()
    {
        em.OnShipDead -= OnShipDead;
        em.OnMatchEndedByServer -= OnMatchEndedByServer;
    }
    #endregion

    #region Subscribers
    private void OnShipDead(int shipIndex, int killerIndex, float lifetime)
    {
        if (shipIndex == myShipIndex)
        {
            SetSpectateMode(true);
        }
    }

    private void OnMatchEndedByServer(string winnerName, bool localPlayerWins)
    {
        SetSpectateMode(true);
    }
    #endregion
    #endregion

    #region LateUpdate
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
    #endregion

    #region Setters
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

        //timeStartedLerping = Time.time;
        //originalPosition = cameraParent.position;
        spectatePosition = new Vector3(0, spectatingHeight, 0);    
    }
    #endregion

}

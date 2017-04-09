using System.Collections;
using System.Collections.Generic;
using Server;
using UnityEngine;

public class ProtoServerShipController : MonoBehaviour {

    static ProtoServerShipManager shipManager;
    public Vector3 newMovementPosition;
    NetworkServer serverScript;

    void Awake()
    {
        shipManager = FindObjectOfType<ProtoServerShipManager>();
        serverScript = FindObjectOfType<NetworkServer>();
    }
    
    void FixedUpdate()
    {
        //if (serverScript.GetConnectedStatus())
        //{
        //    //TODO: Add a check to see if the game is in match view (do not try to move the ship when in main menu)
        //    //Debug.Log("shipController, shipInfoList.Count: " + shipManager.shipInfoList.Count);
        //    if (shipManager.shipInfoList.Count > 0)
        //    {
        //        if(transform.position != shipManager.shipInfoList[0].shipPosition)
        //        {
        //            Debug.Log("shipController, moving ship position");
        //            transform.position = shipManager.shipInfoList[0].shipPosition;
        //            //if (shipManager.shipInfoList[0].shipPosition != null)
        //            //{
        //            //    Debug.Log("shipInfoList element 0 shipPosition is not null, moving ship");
        //            //}
        //        }
        //    }
        //    else
        //    {
        //        //Debug.Log("shipController, shipInfoList empty");
        //    }
        //}

        
    }
	
}

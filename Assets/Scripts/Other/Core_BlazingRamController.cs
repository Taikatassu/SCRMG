using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_BlazingRamController : MonoBehaviour {

    Core_Toolbox toolbox;
    Core_EventManager em;
    Core_GlobalVariableLibrary lib;
    Core_ShipController myShipController;
    float blazingRamDamage = -1;
    float ramDamageDealCooldown = 0.1f;
    bool onDamageDealCooldown = false;
    string shipTag = "Ship";
    string environmentTag = "Environment";

    private void Awake()
    {
        toolbox = FindObjectOfType<Core_Toolbox>();
        em = toolbox.GetComponent<Core_EventManager>();
        lib = toolbox.GetComponent<Core_GlobalVariableLibrary>();
        GetStats();
    }

    private void GetStats()
    {
        shipTag = lib.shipVariables.shipTag;
        environmentTag = lib.shipVariables.environmentTag;
        blazingRamDamage = lib.shipVariables.blazingRamDamage;
    }

    //private void FixedUpdate()
    //{

    //}

    private void OnTriggerEnter(Collider collider)
    {
        GameObject collidedObject = collider.gameObject;
        if (collidedObject.tag == shipTag)
        {
            collidedObject.GetComponentInParent<Core_ShipController>().TakeDamage(blazingRamDamage);
        }
    }

}

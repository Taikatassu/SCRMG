using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_Projectile : MonoBehaviour {

    //TODO: Add fading effect when projectile runs out of lifetime
    //          before hitting anything

    #region References & variables
    //References
    Core_Toolbox toolbox;
    Core_GlobalVariableLibrary lib;
    Core_EventManager em;
    Core_ShipController myShipController;
    Rigidbody rb;
    //Variables coming from within the script 
    public enum EProjectileType { DEFAULT, BULLET }
    EProjectileType projectileType = EProjectileType.DEFAULT;
    bool isPaused = false;
    float projectileSpeed = -1;
    int projectileLifetimeFrames = -1;
    int projectileLifetimeTimer = -1;
    //Variables coming from GlobalVariableLibrary
    float bulletSpeed = -1;
    float bulletRange = -1;
    #endregion

    #region Initialization
    #region Awake & GetStats
    private void Awake()
    {
        toolbox = FindObjectOfType<Core_Toolbox>();
        em = toolbox.GetComponent<Core_EventManager>();
        lib = toolbox.GetComponent<Core_GlobalVariableLibrary>();
        rb = GetComponent<Rigidbody>();
        GetStats();
    }

    private void GetStats()
    {
        bulletSpeed = lib.shipVariables.bulletSpeed;
        bulletRange = lib.shipVariables.bulletRange;
    }
    #endregion

    #region OnEnable & OnDisable
    void OnEnable()
    {
        em.OnPauseOn += OnPauseOn;
        em.OnPauseOff += OnPauseOff;
    }

    void OnDisable()
    {
        em.OnPauseOn -= OnPauseOn;
        em.OnPauseOff -= OnPauseOff;
        projectileType = EProjectileType.DEFAULT;
    }
    #endregion

    #region Subscribers
    private void OnPauseOn()
    {
        isPaused = true;
    }

    private void OnPauseOff()
    {
        isPaused = false;
    }
    #endregion

    #region Getters & setters
    public void SetProjectileType(EProjectileType newProjectileType)
    {
        projectileType = newProjectileType;
        switch (projectileType)
        {
            case EProjectileType.DEFAULT:
                Debug.LogError("Projetile should never be type: DEFAULT after spawning!!");
                break;
            case EProjectileType.BULLET:
                projectileSpeed = bulletSpeed;
                projectileLifetimeFrames = Mathf.RoundToInt((bulletRange / projectileSpeed) / Time.fixedDeltaTime);
                projectileLifetimeTimer = projectileLifetimeFrames;
                break;
        }
    }

    public void SetShipController(Core_ShipController newShipController)
    {
        myShipController = newShipController;
    }

    public void SetProjectileColor(Color newColor)
    {
        //TODO: Change this if projectile hierarchy changes!
        transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_TintColor", newColor);
    }
    #endregion
    #endregion

    #region Update & FixedUpdate
    private void Update()
    {
        if (!isPaused)
        {
            rb.MovePosition(transform.forward * projectileSpeed * Time.deltaTime + rb.position);
        }
    }

    private void FixedUpdate()
    {
        if (!isPaused)
        {
            projectileLifetimeTimer--;
            if (projectileLifetimeTimer <= 0)
            {
                projectileLifetimeTimer = 0;
                Debug.Log("Projectile lifetime ended");
                if (myShipController != null)
                    myShipController.OnProjectileLifetimeEnded(this);
            }
        }
    }
    #endregion

    #region Collision detection
    private void OnTriggerEnter(Collider collider)
    {
        if (myShipController != null)
            myShipController.OnProjectileTriggerEnter(this, collider.gameObject);
    }
    #endregion

}

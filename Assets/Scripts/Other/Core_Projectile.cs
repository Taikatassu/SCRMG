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
    BoxCollider projectileCollider;
    GameObject projectileVisuals;
    //Variables coming from within the script 
    public enum EProjectileType { DEFAULT, BULLET, RUBBERBULLET }
    EProjectileType projectileType = EProjectileType.DEFAULT;
    bool isPaused = false;
    float projectileSpeed = -1;
    int projectileLifetimeFrames = -1;
    int projectileLifetimeTimer = -1;
    int projectileRicochetCounter = -1;
    int projectileRicochetNumber = -1;
    //Variables coming from GlobalVariableLibrary
    float bulletSpeed = -1;
    float bulletRange = -1;
    float rubberBulletSpeed = -1;
    float rubberBulletRange = -1;
    int rubberBulletRicochetNumber = -1;
    string environmentTag = "Environment";
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
        environmentTag = lib.shipVariables.environmentTag;
        bulletSpeed = lib.shipVariables.bulletSpeed;
        bulletRange = lib.shipVariables.bulletRange;
        rubberBulletSpeed = lib.shipVariables.rubberBulletSpeed;
        rubberBulletRange = lib.shipVariables.rubberBulletRange;
        rubberBulletRicochetNumber = lib.shipVariables.rubberBulletRicochetNumber;
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
                //TODO: This calculation is wrong, bulletRange 50 should cover the whole arena, currently 200 is minimum requirement?
                projectileLifetimeFrames = Mathf.RoundToInt((bulletRange / projectileSpeed) / Time.fixedDeltaTime);
                projectileLifetimeTimer = projectileLifetimeFrames;
                projectileVisuals = Instantiate(Resources.Load("Projectiles/Visuals/BulletVisuals", typeof(GameObject)), 
                    transform) as GameObject;
                projectileCollider = GetComponent<BoxCollider>();
                projectileCollider.size = new Vector3(0.4f, 0.4f, 0.8f);
                break;
            case EProjectileType.RUBBERBULLET:
                projectileSpeed = rubberBulletSpeed;
                //TODO: This calculation is wrong, bulletRange 50 should cover the whole arena, currently 200 is minimum requirement?
                projectileLifetimeFrames = Mathf.RoundToInt((rubberBulletRange / projectileSpeed) / Time.fixedDeltaTime);
                projectileLifetimeTimer = projectileLifetimeFrames;
                projectileRicochetCounter = 0;
                projectileRicochetNumber = rubberBulletRicochetNumber;
                projectileVisuals = Instantiate(Resources.Load("Projectiles/Visuals/RubberBulletVisuals", typeof(GameObject)),
                    transform) as GameObject;
                projectileCollider = GetComponent<BoxCollider>();
                projectileCollider.size = new Vector3(0.8f, 0.8f, 0.8f);
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
        projectileVisuals.GetComponent<Renderer>().material.SetColor("_TintColor", newColor);
        if (projectileType == EProjectileType.RUBBERBULLET)
        {
            projectileVisuals.GetComponent<TrailRenderer>().material.SetColor("_TintColor", newColor);  //material.SetColor("_TintColor", newColor);
        }
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
        if (projectileType == EProjectileType.BULLET)
        {
            if (myShipController != null)
                myShipController.OnProjectileTriggerEnter(this, collider.gameObject);
        }
        else if (projectileType == EProjectileType.RUBBERBULLET)
        {
            string collidedObjectTag = collider.gameObject.tag;
            if (projectileRicochetCounter < projectileRicochetNumber && collidedObjectTag == environmentTag)
            {
                Vector3 originalRotation = transform.eulerAngles;
                transform.eulerAngles = new Vector3(originalRotation.x, originalRotation.y + 180, originalRotation.z);
            }
            else
            {
                if (myShipController != null)
                    myShipController.OnProjectileTriggerEnter(this, collider.gameObject);
            }
        }
    }
    #endregion

}

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
    Color projectileColor;
    bool isPaused = false;
    bool ricohetOnCooldown = false;
    float projectileSpeed = -1;
    float ricochetCooldown = 0.1f;
    int ricochetCooldownTimer = -1;
    int projectileLifetimeFrames = -1;
    int projectileLifetimeTimer = -1;
    int projectileRicochetCounter = -1;
    int projectileRicochetNumber = -1;
    //Variables coming from GlobalVariableLibrary
    string shipTag = "Ship";
    string environmentTag = "Environment";
    float bulletDamage = -1;
    float bulletSpeed = -1;
    float bulletRange = -1;
    float rubberBulletSpeed = -1;
    float rubberBulletRange = -1;
    int rubberBulletRicochetNumber = -1;
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
        shipTag = lib.shipVariables.shipTag;
        environmentTag = lib.shipVariables.environmentTag;
        bulletDamage = lib.shipVariables.bulletDamage;
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
        em.OnGameRestart += OnGameRestart;
        em.OnNewSceneLoading += OnNewSceneLoading;
    }

    void OnDisable()
    {
        em.OnPauseOn -= OnPauseOn;
        em.OnPauseOff -= OnPauseOff;
        em.OnGameRestart -= OnGameRestart;
        em.OnNewSceneLoading -= OnNewSceneLoading;
        projectileType = EProjectileType.DEFAULT;
    }
    #endregion

    #region Subscribers
    private void OnPauseOn()
    {
        isPaused = true;
        rb.velocity = Vector3.zero;
    }

    private void OnPauseOff()
    {
        isPaused = false;
    }

    private void OnGameRestart()
    {
        Destroy(gameObject);
    }

    private void OnNewSceneLoading(int sceneIndex)
    {
        Destroy(gameObject);
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

    public EProjectileType GetProjectileType()
    {
        return projectileType;
    }

    public void SetShipController(Core_ShipController newShipController)
    {
        myShipController = newShipController;
    }

    public void SetProjectileColor(Color newColor)
    {
        //TODO: Change this if projectile hierarchy changes!
        projectileColor = newColor;
        projectileVisuals.GetComponent<Renderer>().material.SetColor("_TintColor", projectileColor);
        if (projectileType == EProjectileType.RUBBERBULLET)
        {
            projectileVisuals.GetComponent<TrailRenderer>().material.SetColor("_TintColor", projectileColor);
        }
    }
    #endregion
    #endregion

    #region Update & FixedUpdate
    private void Update()
    {
        if (!isPaused)
        {
            rb.velocity = Vector3.zero;
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
                if (myShipController != null)
                    OnProjectileLifetimeEnded();
            }

            if (ricohetOnCooldown)
            {
                ricochetCooldownTimer--;
                if (ricochetCooldownTimer <= 0)
                {
                    ricochetCooldownTimer = 0;
                    ricohetOnCooldown = false;
                }
            }
        }
    }
    #endregion

    #region OnProjectileLifetimeEnded
    private void OnProjectileLifetimeEnded()
    {
        Destroy(gameObject);
    }
    #endregion

    #region Collision detection
    private void OnCollisionEnter(Collision collision)
    {

        GameObject collidedObject = collision.gameObject;
        string collidedObjectTag = collidedObject.tag;

        if (collidedObjectTag == shipTag)
        {
            //Spawn hit effect
            GameObject bulletHitEffect = Instantiate(Resources.Load("Effects/BulletHitEffect"),
                transform.position, Quaternion.identity) as GameObject;
            bulletHitEffect.GetComponentInChildren<Renderer>().material.SetColor("_TintColor", projectileColor);

            //Damage enemy ship
            collidedObject.GetComponentInParent<Core_ShipController>().TakeDamage(bulletDamage);
            //Destroy projectile
            Destroy(gameObject);
        }
        else if (collidedObjectTag == environmentTag)
        {
            if (projectileType == EProjectileType.RUBBERBULLET)
            {
                if (projectileRicochetCounter < projectileRicochetNumber && collidedObjectTag == environmentTag)
                {
                    if (!ricohetOnCooldown)
                    {
                        projectileRicochetCounter++;
                        Vector3 originalRotation = transform.eulerAngles;
                        transform.eulerAngles = new Vector3(originalRotation.x, originalRotation.y + 180, originalRotation.z);
                        ricochetCooldownTimer = Mathf.RoundToInt(ricochetCooldown / Time.fixedDeltaTime);
                        ricohetOnCooldown = true;
                    }
                }
            }
            else
            {
                //Spawn hit effect
                GameObject bulletHitEffect = Instantiate(Resources.Load("Effects/BulletHitEffect"),
                    transform.position, Quaternion.identity) as GameObject;
                bulletHitEffect.GetComponentInChildren<Renderer>().material.SetColor("_TintColor", projectileColor);

                Destroy(gameObject);
            }

        }      
    }

    private void OnTriggerEnter(Collider collider)
    {
        //if (projectileType == EProjectileType.BULLET)
        //{
        //    //if (myShipController != null)
        //    //    myShipController.OnProjectileTriggerEnter(this, collider.gameObject);
        //}
        //else if (projectileType == EProjectileType.RUBBERBULLET)
        //{
        //    string collidedObjectTag = collider.gameObject.tag;
        //    if (projectileRicochetCounter < projectileRicochetNumber && collidedObjectTag == environmentTag)
        //    {
        //        if (!ricohetOnCooldown)
        //        {
        //            Vector3 originalRotation = transform.eulerAngles;
        //            transform.eulerAngles = new Vector3(originalRotation.x, originalRotation.y + 180, originalRotation.z);
        //            ricochetCooldownTimer = Mathf.RoundToInt(ricochetCooldown / Time.fixedDeltaTime);
        //            ricohetOnCooldown = true;
        //        }
        //    }
        //    else
        //    {
        //        if (myShipController != null)
        //            myShipController.OnProjectileTriggerEnter(this, collider.gameObject);
        //    }
        //}
    }
    #endregion

}

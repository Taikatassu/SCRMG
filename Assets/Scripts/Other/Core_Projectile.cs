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
    //Variables coming from within the script 
    public enum EProjectileType { DEFAULT, BULLET, RUBBERBULLET, BLAZINGRAM, BEAMCANNON }
    EProjectileType projectileType = EProjectileType.DEFAULT;
    Color projectileColor;
    bool isPaused = false;
    bool ricohetOnCooldown = false;
    bool isPersistingProjectile = false;
    float projectileDamage = -1;
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
    int bulletRicochetNumber = -1;
    float rubberBulletDamage = -1;
    float rubberBulletSpeed = -1;
    float rubberBulletRange = -1;
    int rubberBulletRicochetNumber = -1;
    float blazingRamDamage = -1;
    float blazingRamSpeed = -1;
    float blazingRamRange = -1;
    int blazingRamRicochetNumber = -1;
    float beamCannonDamage = -1;
    float beamCannonSpeed = -1;
    float beamCannonRange = -1;
    int beamCannonRicochetNumber = -1;
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

        bulletDamage = lib.projectileVariables.bulletDamage;
        bulletSpeed = lib.projectileVariables.bulletSpeed;
        bulletRange = lib.projectileVariables.bulletRange;
        bulletRicochetNumber = lib.projectileVariables.bulletRicochetNumber;

        rubberBulletDamage = lib.projectileVariables.rubberBulletDamage;
        rubberBulletSpeed = lib.projectileVariables.rubberBulletSpeed;
        rubberBulletRange = lib.projectileVariables.rubberBulletRange;
        rubberBulletRicochetNumber = lib.projectileVariables.rubberBulletRicochetNumber;

        blazingRamDamage = lib.projectileVariables.blazingRamDamage;
        blazingRamSpeed = lib.projectileVariables.blazingRamSpeed;
        blazingRamRange = lib.projectileVariables.blazingRamRange;
        blazingRamRicochetNumber = lib.projectileVariables.blazingRamRicochetNumber;
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
        if (projectileType == EProjectileType.BLAZINGRAM)
        {
            Debug.Log("BlazingRam destroyed");
        }
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
    public void SetProjectileType(int newProjectileType)
    {
        projectileColor = myShipController.GetShipColor();
        switch (newProjectileType)
        {
            case 0:
                projectileType = EProjectileType.BULLET;
                projectileDamage = bulletDamage;
                projectileSpeed = bulletSpeed;
                projectileLifetimeFrames = Mathf.RoundToInt((bulletRange / projectileSpeed) / Time.fixedDeltaTime);
                projectileLifetimeTimer = projectileLifetimeFrames;
                projectileRicochetCounter = 0;
                projectileRicochetNumber = bulletRicochetNumber;

                Instantiate(Resources.Load("Projectiles/Visuals/BulletVisuals", typeof(GameObject)), transform);
                SetProjectileVisualsColor();

                projectileCollider = GetComponent<BoxCollider>();
                projectileCollider.size = new Vector3(0.4f, 0.4f, 0.8f);
                projectileCollider.center = new Vector3(0, 0, 0);
                isPersistingProjectile = false;
                break;
            case 1:
                projectileType = EProjectileType.RUBBERBULLET;
                projectileDamage = rubberBulletDamage;
                projectileSpeed = rubberBulletSpeed;
                projectileLifetimeFrames = Mathf.RoundToInt((rubberBulletRange / projectileSpeed) / Time.fixedDeltaTime);
                projectileLifetimeTimer = projectileLifetimeFrames;
                projectileRicochetCounter = 0;
                projectileRicochetNumber = rubberBulletRicochetNumber;

                Instantiate(Resources.Load("Projectiles/Visuals/RubberBulletVisuals", typeof(GameObject)), transform);
                SetProjectileVisualsColor();
                SetProjectileTrailColor();

                projectileCollider = GetComponent<BoxCollider>();
                projectileCollider.size = new Vector3(0.8f, 0.8f, 0.8f);
                projectileCollider.center = new Vector3(0, 0, 0);
                isPersistingProjectile = false;
                break;
            case 2:
                projectileType = EProjectileType.BLAZINGRAM;
                projectileDamage = blazingRamDamage * Time.fixedDeltaTime;
                Debug.Log("BlazingRam created, damage: " + projectileDamage);
                projectileSpeed = blazingRamSpeed;
                projectileLifetimeFrames = Mathf.RoundToInt((blazingRamRange / projectileSpeed) / Time.fixedDeltaTime);
                projectileLifetimeTimer = projectileLifetimeFrames;
                projectileRicochetCounter = 0;
                projectileRicochetNumber = blazingRamRicochetNumber;

                Instantiate(Resources.Load("Projectiles/Visuals/BlazingRamVisuals", typeof(GameObject)), transform);
                SetProjectileTrailColor();

                projectileCollider = GetComponent<BoxCollider>();
                projectileCollider.size = new Vector3(2f, 1f, 1.5f);
                projectileCollider.center = new Vector3(0, 0, 0.7f);
                isPersistingProjectile = true;
                break;
            case 3:
                projectileType = EProjectileType.BEAMCANNON;
                projectileDamage = beamCannonDamage * Time.fixedDeltaTime;
                Debug.Log("BeamCannon created, damage: " + projectileDamage);
                projectileSpeed = beamCannonSpeed;
                projectileLifetimeFrames = Mathf.RoundToInt((beamCannonRange / projectileSpeed) / Time.fixedDeltaTime);
                projectileLifetimeTimer = projectileLifetimeFrames;
                projectileRicochetCounter = 0;
                projectileRicochetNumber = beamCannonRicochetNumber;

                Instantiate(Resources.Load("Projectiles/Visuals/BeamCannonVisuals", typeof(GameObject)), transform);
                SetProjectileTrailColor();

                projectileCollider = GetComponent<BoxCollider>();
                projectileCollider.size = new Vector3(1f, 1f, 15f);
                projectileCollider.center = new Vector3(0, 0, 8.4f);
                isPersistingProjectile = true;
                break;
        }

        if (isPersistingProjectile)
        {
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Collider>().isTrigger = true;
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

    private void SetProjectileVisualsColor()
    {
        foreach(Transform child in transform)
        {
            if (child.GetComponent<Renderer>())
            {
                child.GetComponent<Renderer>().material.SetColor("_TintColor", projectileColor);
            }
        }
    }

    private void SetProjectileTrailColor()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<TrailRenderer>())
            {
                child.GetComponent<TrailRenderer>().material.SetColor("_TintColor", projectileColor);
            }
        }
    }

    private void SetProjectileParticleEffectsColor()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<ParticleSystem>())
            {
                ParticleSystem.MainModule psMain = child.GetComponentInChildren<ParticleSystem>().main;
                psMain.startColor = new Color(projectileColor.r, projectileColor.g, projectileColor.b, psMain.startColor.color.a);
            }
        }
    }
    #endregion
    #endregion

    #region Update & FixedUpdate
    private void Update()
    {
        if (!isPersistingProjectile)
        {
            if (!isPaused)
            {
                rb.velocity = Vector3.zero;
                rb.MovePosition(transform.forward * projectileSpeed * Time.deltaTime + rb.position);
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isPersistingProjectile)
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
        else
        {
            //Manage tick rate
            Collider projectileCollider = transform.GetComponent<Collider>();
            if (projectileCollider.enabled == true)
            {
                projectileCollider.enabled = false;
            }
            else
            {
                projectileCollider.enabled = true;
            }

        }
            
    }
    #endregion

    #region Projectile destruction
    public void OnPersistingProjectileDestruction()
    {
        Debug.Log("OnPersistingProjectileDestruction");
        Destroy(gameObject);
    }

    private void OnProjectileLifetimeEnded()
    {
        Destroy(gameObject);
    }

    private void DestroyOnHit()
    {
        Destroy(gameObject);
        GameObject bulletHitEffect = Instantiate(Resources.Load("Effects/BulletHitEffect"),
            transform.position, Quaternion.identity) as GameObject;
        bulletHitEffect.GetComponentInChildren<Renderer>().material.SetColor("_TintColor", projectileColor);
    }
    #endregion

    #region Collision detection
    private void OnCollisionEnter(Collision collision)
    {
        GameObject collidedObject = collision.gameObject;
        string collidedObjectTag = collidedObject.tag;

        if (collidedObjectTag == shipTag)
        {
            collidedObject.GetComponentInParent<Core_ShipController>().TakeDamage(projectileDamage);
            if (!isPersistingProjectile)
            {
                DestroyOnHit();
            }
            else
            {
                Debug.Log("Persisting projectile collision with ship");
            }
        }
        else if (collidedObjectTag == environmentTag)
        {
            if (projectileRicochetCounter < projectileRicochetNumber && !ricohetOnCooldown)
            {
                projectileRicochetCounter++;
                Vector3 originalRotation = transform.eulerAngles;
                //TODO: Calculat proper reflection angle
                transform.eulerAngles = new Vector3(originalRotation.x, originalRotation.y + 180, originalRotation.z);
                ricochetCooldownTimer = Mathf.RoundToInt(ricochetCooldown / Time.fixedDeltaTime);
                ricohetOnCooldown = true;
            }
            else
            {
                if (!isPersistingProjectile)
                {
                    DestroyOnHit();
                }
            }
        }      
    }

    private void OnTriggerEnter(Collider collider)
    {
        GameObject collidedObject = collider.gameObject;
        string collidedObjectTag = collidedObject.tag;

        if (collidedObjectTag == shipTag)
        {
            collidedObject.GetComponentInParent<Core_ShipController>().TakeDamage(projectileDamage);
            if (!isPersistingProjectile)
            {
                DestroyOnHit();
            }
            else
            {
                Debug.Log("Persisting projectile collision with ship, damage: " + projectileDamage);
            }
        }
        else if (collidedObjectTag == environmentTag)
        {
            if (projectileRicochetCounter < projectileRicochetNumber && !ricohetOnCooldown)
            {
                projectileRicochetCounter++;
                Vector3 originalRotation = transform.eulerAngles;
                //TODO: Calculat proper reflection angle
                transform.eulerAngles = new Vector3(originalRotation.x, originalRotation.y + 180, originalRotation.z);
                ricochetCooldownTimer = Mathf.RoundToInt(ricochetCooldown / Time.fixedDeltaTime);
                ricohetOnCooldown = true;
            }
            else
            {
                if (!isPersistingProjectile)
                {
                    DestroyOnHit();
                }
            }
        }
    }
    #endregion

}

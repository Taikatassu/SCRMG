using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_Projectile : MonoBehaviour {

    /*TODO: 
     * - Add fading effect when projectile runs out of lifetime
     *      before hitting anything?
     *      
     * - Make string variables for projectile visual's names (so the names don't have to be hard-coded)
     * 
     * - Rename projectiles / powerUps as "projectileOne" / "powerUpOne" (or similar)
     *      and create variables for projectile / powerUp names (same reason as above)
    */

    #region References & variables
    //References
    Core_Toolbox toolbox;
    Core_GlobalVariableLibrary lib;
    Core_EventManager em;
    Core_ShipController myShipController;
    Rigidbody rb;
    BoxCollider projectileCollider;
    //Variables coming from within the script 
    public enum EProjectileType { DEFAULT, BULLET, RUBBERBULLET, BLAZINGRAM, BEAMCANNON, BOMBS }
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
    float bombsDamage = -1;
    float bombsSpeed = -1;
    float bombsRange = -1;
    int bombsRicochetNumber = -1;
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

        beamCannonDamage = lib.projectileVariables.beamCannonDamage;
        beamCannonSpeed = lib.projectileVariables.beamCannonSpeed;
        beamCannonRange = lib.projectileVariables.beamCannonRange;
        beamCannonRicochetNumber = lib.projectileVariables.beamCannonRicochetNumber;

        bombsDamage = lib.projectileVariables.bombsDamage;
        bombsSpeed = lib.projectileVariables.bombsSpeed;
        bombsRange = lib.projectileVariables.bombsRange;
        bombsRicochetNumber = lib.projectileVariables.bombsRicochetNumber;
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
    public void SetProjectileType(int newProjectileType)
    {
        projectileColor = myShipController.GetShipColor();
        switch (newProjectileType)
        {
            //TODO: Get collider sizes and positions, and persistingStates from GVL instead of hardcoding!
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
                SetProjectileParticleEffectsColor();

                projectileCollider = GetComponent<BoxCollider>();
                projectileCollider.size = new Vector3(0.8f, 0.8f, 0.8f);
                projectileCollider.center = new Vector3(0, 0, 0);
                isPersistingProjectile = false;
                break;

            case 2:
                projectileType = EProjectileType.BLAZINGRAM;
                projectileDamage = blazingRamDamage * Time.fixedDeltaTime;
                projectileSpeed = blazingRamSpeed;
                projectileLifetimeFrames = Mathf.RoundToInt((blazingRamRange / projectileSpeed) / Time.fixedDeltaTime);
                projectileLifetimeTimer = projectileLifetimeFrames;
                projectileRicochetCounter = 0;
                projectileRicochetNumber = blazingRamRicochetNumber;

                Instantiate(Resources.Load("Projectiles/Visuals/BlazingRamVisuals", typeof(GameObject)), transform);
                SetProjectileVisualsColor();
                SetProjectileTrailColor();
                SetProjectileParticleEffectsColor();

                projectileCollider = GetComponent<BoxCollider>();
                projectileCollider.size = new Vector3(3.5f, 1f, 2f);
                projectileCollider.center = new Vector3(0, 0, 1.3f);
                isPersistingProjectile = true;
                break;

            case 3:
                projectileType = EProjectileType.BEAMCANNON;
                projectileDamage = beamCannonDamage * Time.fixedDeltaTime;
                projectileSpeed = beamCannonSpeed;
                projectileLifetimeFrames = Mathf.RoundToInt((beamCannonRange / projectileSpeed) / Time.fixedDeltaTime);
                projectileLifetimeTimer = projectileLifetimeFrames;
                projectileRicochetCounter = 0;
                projectileRicochetNumber = beamCannonRicochetNumber;

                Instantiate(Resources.Load("Projectiles/Visuals/BeamCannonVisuals", typeof(GameObject)), transform);
                SetProjectileVisualsColor();
                SetProjectileTrailColor();
                SetProjectileParticleEffectsColor();

                projectileCollider = GetComponent<BoxCollider>();
                projectileCollider.size = new Vector3(1f, 1f, 15f);
                projectileCollider.center = new Vector3(0, 0, 8.4f);
                isPersistingProjectile = true;
                break;
            
            /*
            case 4:
                projectileType = EProjectileType.BOMBS;
                projectileDamage = bombsDamage * Time.fixedDeltaTime;
                projectileSpeed = bombsSpeed;
                projectileLifetimeFrames = Mathf.RoundToInt((bombsRange / projectileSpeed) / Time.fixedDeltaTime);
                projectileLifetimeTimer = projectileLifetimeFrames;
                projectileRicochetCounter = 0;
                projectileRicochetNumber = bombsRicochetNumber;

                Instantiate(Resources.Load("Projectiles/Visuals/BombsVisuals", typeof(GameObject)), transform);
                //TODO: Properly set up bomb color and collider size / location
                //SetProjectileTrailColor();

                //projectileCollider = GetComponent<BoxCollider>();
                //projectileCollider.size = new Vector3(1f, 1f, 15f);
                //projectileCollider.center = new Vector3(0, 0, 8.4f);
                isPersistingProjectile = false;
                break;
            */
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
            foreach (Transform grandChild in child)
            {
                if (grandChild.GetComponent<Renderer>())
                {
                    grandChild.GetComponent<Renderer>().material.SetColor("_TintColor", projectileColor);
                }
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
            foreach (Transform grandChild in child)
            {
                if (grandChild.GetComponent<TrailRenderer>())
                {
                    grandChild.GetComponent<TrailRenderer>().material.SetColor("_TintColor", projectileColor);
                }
            }
        }
    }

    private void SetProjectileParticleEffectsColor()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<ParticleSystem>())
            {
                ParticleSystem ps = child.GetComponentInChildren<ParticleSystem>();
                ps.Stop();
                ParticleSystem.MainModule psMain = ps.main;
                psMain.startColor = new Color(projectileColor.r, projectileColor.g, projectileColor.b, psMain.startColor.color.a);
                ps.Play();
            }

            foreach (Transform grandChild in child)
            {
                if (grandChild.GetComponent<ParticleSystem>())
                {
                    ParticleSystem ps = grandChild.GetComponentInChildren<ParticleSystem>();
                    ps.Stop();
                    ParticleSystem.MainModule psMain = ps.main;
                    psMain.startColor = new Color(projectileColor.r, projectileColor.g, projectileColor.b, psMain.startColor.color.a);
                    ps.Play();
                }
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
                GameObject bulletHitEffect = Instantiate(Resources.Load("Effects/BulletHitEffect"),
                    collidedObject.transform.position, Quaternion.identity) as GameObject;
                bulletHitEffect.GetComponentInChildren<Renderer>().material.SetColor("_TintColor", projectileColor);
            }
        }
        else if (collidedObjectTag == environmentTag)
        {
            if (projectileRicochetCounter < projectileRicochetNumber && !ricohetOnCooldown)
            {
                projectileRicochetCounter++;
                Vector3 originalRotation = transform.eulerAngles;

                //TODO: Finish implementing proper projectile reflection
                //Remember to add proper reflection calculations to OnTriggerEnter as well

                //RaycastHit hit;
                //Vector3 rayDirection = transform.forward;
                //Vector3 startPoint = transform.position;
                //if (Physics.Raycast(startPoint, rayDirection, out hit))
                //{
                //    //float newYAngle = Vector3.Angle(hit.normal, transform.forward) + 90;
                //    Vector3 reflection = Vector3.Reflect(transform.forward, hit.normal);
                //    Debug.Log("reflection: " + reflection);
                //    float newYAngle = Vector3.Angle(Vector3.zero, reflection);
                //    Debug.Log("newYAngle: " + newYAngle);

                //    transform.eulerAngles = new Vector3(originalRotation.x, newYAngle, originalRotation.z);
                //}

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
                GameObject bulletHitEffect = Instantiate(Resources.Load("Effects/BulletHitEffect"),
                    collidedObject.transform.position, Quaternion.identity) as GameObject;
                bulletHitEffect.GetComponentInChildren<Renderer>().material.SetColor("_TintColor", projectileColor);
            }
        }
        else if (collidedObjectTag == environmentTag)
        {
            if (projectileRicochetCounter < projectileRicochetNumber && !ricohetOnCooldown)
            {
                projectileRicochetCounter++;
                Vector3 originalRotation = transform.eulerAngles;
                transform.eulerAngles = new Vector3(originalRotation.x, originalRotation.y + 180, originalRotation.z);

                //TODO: Make this identical to OnCollisionEnter


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

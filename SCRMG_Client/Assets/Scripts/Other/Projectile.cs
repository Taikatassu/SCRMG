using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    /*TODO: 
     * - Add fading effect when projectile runs out of lifetime
     *      before hitting anything?
     *      
     * - Make string variables for projectile visual's names (so the names don't have to be hard-coded)
     * 
     * - Rename projectiles / powerUps as "projectileOne" / "powerUpOne" (or similar)
     *      and create variables for projectile / powerUp names (same reason as above)
     *      
     * - Broadcast projectile destruction!
    */

    #region References & variables
    //References
    Toolbox toolbox;
    GlobalVariableLibrary lib;
    EventManager em;
    Rigidbody rb;
    BoxCollider projectileCollider;
    //Variables coming from within the script 
    public enum EProjectileType { DEFAULT, BULLET, RUBBERBULLET, BLAZINGRAM, BEAMCANNON, BOMBS }
    EProjectileType projectileType = EProjectileType.DEFAULT;
    Color projectileColor;
    bool isPaused = false;
    bool ricohetOnCooldown = false;
    bool isPersistingProjectile = false;
    bool isInitialized = false;
    float projectileDamage = -1;
    float projectileSpeed = -1;
    float projectileRicochetCooldown = -1;
    float projectileTickInterval = -1;
    int ricochetCooldownTimer = -1;
    int projectileTickRateCounter = -1;
    int projectileLifetimeTimer = -1;
    int projectileRicochetCounter = -1;
    int projectileRicochetNumber = -1;
    int projectileIndex = -1;
    int ownerIndex = -1;
    //Variables coming from GlobalVariableLibrary
    string shipTag = "Ship";
    string environmentTag = "Environment";
    #endregion

    #region Initialization
    #region Awake & GetStats
    private void Awake()
    {
        toolbox = FindObjectOfType<Toolbox>();
        em = toolbox.GetComponent<EventManager>();
        lib = toolbox.GetComponent<GlobalVariableLibrary>();
        rb = GetComponent<Rigidbody>();
        GetStats();
    }

    private void GetStats()
    {
        shipTag = lib.shipVariables.shipTag;
        environmentTag = lib.shipVariables.environmentTag;
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
    #endregion

    #region Setters & getters
    #region Setters
    public void InitializeProjectile(int newOwnerIndex, int newProjectileIndex, int newProjectileType, Color newProjectileColor)
    {
        ownerIndex = newOwnerIndex;
        projectileIndex = newProjectileIndex;
        projectileColor = newProjectileColor;
        SetProjectileType(newProjectileType);
        isInitialized = true;
        em.BroadcastProjectileSpawned(ownerIndex, projectileIndex, transform.position, transform.eulerAngles);
    }

    private void SetProjectileType(int newProjectileType)
    {
        switch (newProjectileType)
        {
            //TODO: Get collider sizes and positions, and persistingStates from GVL instead of hardcoding!
            #region Bullet
            case 0:
                projectileType = EProjectileType.BULLET;
                projectileTickInterval = 1 / lib.projectileVariables.bulletTickRate;
                projectileTickRateCounter = Mathf.RoundToInt(projectileTickInterval / Time.fixedDeltaTime);
                projectileDamage = lib.projectileVariables.bulletDamage * projectileTickInterval;
                projectileSpeed = lib.projectileVariables.bulletSpeed;
                projectileLifetimeTimer = Mathf.RoundToInt((lib.projectileVariables.bulletRange / projectileSpeed) / Time.fixedDeltaTime);
                projectileRicochetCounter = 0;
                projectileRicochetCooldown = lib.projectileVariables.bulletRicochetCooldown;
                projectileRicochetNumber = lib.projectileVariables.bulletRicochetNumber;

                Instantiate(Resources.Load("Projectiles/Visuals/BulletVisuals", typeof(GameObject)), transform);
                SetProjectileVisualsColor();
                SetProjectileTrailColor();
                SetProjectileParticleEffectsColor();

                projectileCollider = GetComponent<BoxCollider>();
                projectileCollider.size = new Vector3(0.4f, 0.4f, 0.8f);
                projectileCollider.center = new Vector3(0, 0, 0);
                isPersistingProjectile = false;
                break;
            #endregion

            #region RubberBullet
            case 1:
                projectileType = EProjectileType.RUBBERBULLET;
                projectileTickInterval = 1 / lib.projectileVariables.rubberBulletTickRate;
                projectileTickRateCounter = Mathf.RoundToInt(projectileTickInterval / Time.fixedDeltaTime);
                projectileDamage = lib.projectileVariables.rubberBulletDamage * projectileTickInterval;
                projectileSpeed = lib.projectileVariables.rubberBulletSpeed;
                projectileLifetimeTimer = Mathf.RoundToInt((lib.projectileVariables.rubberBulletRange / projectileSpeed) / Time.fixedDeltaTime);
                projectileRicochetCounter = 0;
                projectileRicochetCooldown = lib.projectileVariables.rubberBulletRicochetCooldown;
                projectileRicochetNumber = lib.projectileVariables.rubberBulletRicochetNumber;

                Instantiate(Resources.Load("Projectiles/Visuals/RubberBulletVisuals", typeof(GameObject)), transform);
                SetProjectileVisualsColor();
                SetProjectileTrailColor();
                SetProjectileParticleEffectsColor();

                projectileCollider = GetComponent<BoxCollider>();
                projectileCollider.size = new Vector3(0.8f, 0.8f, 0.8f);
                projectileCollider.center = new Vector3(0, 0, 0);
                isPersistingProjectile = false;
                break;
            #endregion

            #region BlazingRam
            case 2:
                projectileType = EProjectileType.BLAZINGRAM;
                projectileTickInterval = 1 / lib.projectileVariables.blazingRamTickRate;
                projectileTickRateCounter = Mathf.RoundToInt(projectileTickInterval / Time.fixedDeltaTime);
                projectileDamage = lib.projectileVariables.blazingRamDamage * projectileTickInterval;
                projectileSpeed = lib.projectileVariables.blazingRamSpeed;
                projectileLifetimeTimer = Mathf.RoundToInt((lib.projectileVariables.blazingRamRange / projectileSpeed) / Time.fixedDeltaTime);
                projectileRicochetCounter = 0;
                projectileRicochetCooldown = lib.projectileVariables.blazingRamRicochetCooldown;
                projectileRicochetNumber = lib.projectileVariables.blazingRamRicochetNumber;

                Instantiate(Resources.Load("Projectiles/Visuals/BlazingRamVisuals", typeof(GameObject)), transform);
                SetProjectileVisualsColor();
                SetProjectileTrailColor();
                SetProjectileParticleEffectsColor();

                projectileCollider = GetComponent<BoxCollider>();
                projectileCollider.size = new Vector3(3.5f, 1f, 2f);
                projectileCollider.center = new Vector3(0, 0, 1.3f);
                isPersistingProjectile = true;
                break;
            #endregion

            #region BeamCannon
            case 3:
                projectileType = EProjectileType.BEAMCANNON;
                projectileTickInterval = 1 / lib.projectileVariables.beamCannonTickRate;
                projectileTickRateCounter = Mathf.RoundToInt(projectileTickInterval / Time.fixedDeltaTime);
                projectileDamage = lib.projectileVariables.beamCannonDamage * projectileTickInterval;
                projectileSpeed = lib.projectileVariables.beamCannonSpeed;
                projectileLifetimeTimer = Mathf.RoundToInt((lib.projectileVariables.beamCannonRange / projectileSpeed) / Time.fixedDeltaTime);
                projectileRicochetCounter = 0;
                projectileRicochetCooldown = lib.projectileVariables.beamCannonRicochetCooldown;
                projectileRicochetNumber = lib.projectileVariables.beamCannonRicochetNumber;

                Instantiate(Resources.Load("Projectiles/Visuals/BeamCannonVisuals", typeof(GameObject)), transform);
                SetProjectileVisualsColor();
                SetProjectileTrailColor();
                SetProjectileParticleEffectsColor();

                projectileCollider = GetComponent<BoxCollider>();
                projectileCollider.size = new Vector3(1f, 1f, 15f);
                projectileCollider.center = new Vector3(0, 0, 8.4f);
                isPersistingProjectile = true;
                break;
            #endregion

            #region Bombs
            case 4:
                projectileType = EProjectileType.BOMBS;
                projectileTickInterval = 1 / lib.projectileVariables.bombsTickRate;
                projectileTickRateCounter = Mathf.RoundToInt(projectileTickInterval / Time.fixedDeltaTime);
                projectileDamage = lib.projectileVariables.bombsDamage * Time.fixedDeltaTime * projectileTickInterval;
                projectileSpeed = lib.projectileVariables.bombsSpeed;
                projectileLifetimeTimer = Mathf.RoundToInt((lib.projectileVariables.bombsRange / projectileSpeed) / Time.fixedDeltaTime);
                projectileRicochetCounter = 0;
                projectileRicochetCooldown = lib.projectileVariables.bombsRicochetCooldown;
                projectileRicochetNumber = lib.projectileVariables.bombsRicochetNumber;

                Instantiate(Resources.Load("Projectiles/Visuals/BombsVisuals", typeof(GameObject)), transform);
                SetProjectileTrailColor();
                SetProjectileTrailColor();
                SetProjectileParticleEffectsColor();

                //TODO: Properly set up bomb color and collider size / location
                //projectileCollider = GetComponent<BoxCollider>();
                //projectileCollider.size = new Vector3(1f, 1f, 15f);
                //projectileCollider.center = new Vector3(0, 0, 8.4f);
                isPersistingProjectile = false;
                break;
                #endregion
        }

        if (isPersistingProjectile)
        {
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Collider>().isTrigger = true;
        }
    }

    private void SetProjectileVisualsColor()
    {
        foreach (Transform child in transform)
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

    #region Getters
    public int GetOwnerIndex()
    {
        return ownerIndex;
    }

    public int GetProjectileIndex()
    {
        return projectileIndex;
    }
    #endregion
    #endregion

    #region FixedUpdate
    private void FixedUpdate()
    {
        if (!isPaused) 
        {
            if (isInitialized)
            {
                if (!isPersistingProjectile)
                {
                    #region Non-persisting projectile management
                    #region Movement
                    rb.velocity = Vector3.zero;
                    rb.MovePosition(transform.forward * projectileSpeed * Time.fixedDeltaTime + rb.position);
                    #endregion

                    #region Outside area bounds detection
                    //TODO: Implement a proper way to detect if projectile is outside of arena bounds, and returning it back to arena
                    Vector3 currentPosition = transform.position;
                    if (currentPosition.x > 25)
                    {
                        Vector3 newPosition = currentPosition;
                        newPosition.x = 24;
                        transform.position = newPosition;
                        //Vector3 currentRotation = transform.localEulerAngles;
                        //transform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y + 180, currentRotation.z);
                        transform.rotation = Quaternion.LookRotation(Ricochet());
                    }
                    else if (currentPosition.x < -25)
                    {
                        Vector3 newPosition = currentPosition;
                        newPosition.x = -24;
                        transform.position = newPosition;
                        //Vector3 currentRotation = transform.localEulerAngles;
                        //transform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y + 180, currentRotation.z);
                        transform.rotation = Quaternion.LookRotation(Ricochet());
                    }
                    else if (currentPosition.z > 25)
                    {
                        Vector3 newPosition = currentPosition;
                        newPosition.z = 24;
                        transform.position = newPosition;
                        //Vector3 currentRotation = transform.localEulerAngles;
                        //transform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y + 180, currentRotation.z);
                        transform.rotation = Quaternion.LookRotation(Ricochet());
                    }
                    else if (currentPosition.z < -25)
                    {
                        Vector3 newPosition = currentPosition;
                        newPosition.z = -24;
                        transform.position = newPosition;
                        //Vector3 currentRotation = transform.localEulerAngles;
                        //transform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y + 180, currentRotation.z);
                        transform.rotation = Quaternion.LookRotation(Ricochet());
                    }
                    #endregion

                    #region Lifetime timer
                    projectileLifetimeTimer--;
                    if (projectileLifetimeTimer <= 0)
                    {
                        projectileLifetimeTimer = 0;
                        OnProjectileLifetimeEnded();
                    }
                    #endregion

                    #region Ricochet cooldown
                    if (ricohetOnCooldown)
                    {
                        ricochetCooldownTimer--;
                        if (ricochetCooldownTimer <= 0)
                        {
                            ricochetCooldownTimer = 0;
                            ricohetOnCooldown = false;
                        }
                    }
                    #endregion
                    #endregion
                }
                else
                {
                    #region Persisting projectile management
                    #region Tick rate counter
                    if (projectileCollider.enabled == true)
                    {
                        projectileCollider.enabled = false;
                    }

                    projectileTickRateCounter--;
                    if (projectileTickRateCounter <= 0)
                    {
                        projectileTickRateCounter = Mathf.RoundToInt(projectileTickInterval
                            / Time.fixedDeltaTime);

                        projectileCollider.enabled = true;
                    }
                    #endregion
                    #endregion
                }
            }
        }   
    }
    #endregion

    #region Projectile destruction
    public void OnPersistingProjectileDestruction()
    {
        DestroyThisProjectile();
    }

    private void OnProjectileLifetimeEnded()
    {
        DestroyThisProjectile();
    }

    private void DestroyOnHit()
    {
        DestroyThisProjectile();
        GameObject bulletHitEffect = Instantiate(Resources.Load("Effects/BulletHitEffect"),
            transform.position, Quaternion.identity) as GameObject;
        bulletHitEffect.GetComponentInChildren<Renderer>().material.SetColor("_TintColor", projectileColor);
    }

    private void DestroyThisProjectile()
    {
        em.BroadcastProjectileDestroyed(ownerIndex, projectileIndex);
        Destroy(gameObject);
    }
    #endregion

    #region Ricocheting
    private Vector3 Ricochet()
    {
        RaycastHit hit;
        Vector3 originalDirection = transform.forward;
        Vector3 startPoint = transform.position;
        if (Physics.Raycast(startPoint, originalDirection, out hit))
        {
            Vector3 newDirection = Vector3.Reflect(originalDirection, hit.normal);
            return newDirection;
        }
        else
        {
            return originalDirection;
        }
    }
    #endregion

    #region Collision detection
    private void OnCollisionEnter(Collision collision)
    {
        if (isInitialized)
        {
            GameObject collidedObject = collision.gameObject;
            string collidedObjectTag = collidedObject.tag;

            #region Collision with ships
            if (collidedObjectTag == shipTag)
            {
                collidedObject.GetComponentInParent<ShipController>().TakeDamage(projectileDamage);
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
            #endregion

            #region Collision with environment
            else if (collidedObjectTag == environmentTag)
            {
                if (!isPersistingProjectile && projectileRicochetCounter < projectileRicochetNumber && !ricohetOnCooldown)
                {
                    projectileRicochetCounter++;

                    transform.rotation = Quaternion.LookRotation(Ricochet());

                    ricochetCooldownTimer = Mathf.RoundToInt(projectileRicochetCooldown / Time.fixedDeltaTime);
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
            #endregion
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (isInitialized)
        {
            GameObject collidedObject = collider.gameObject;
            string collidedObjectTag = collidedObject.tag;

            #region Collision with ships
            if (collidedObjectTag == shipTag)
            {
                collidedObject.GetComponentInParent<ShipController>().TakeDamage(projectileDamage);
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
            #endregion

            #region Collision with environment
            else if (collidedObjectTag == environmentTag)
            {
                if (!isPersistingProjectile && projectileRicochetCounter < projectileRicochetNumber && !ricohetOnCooldown)
                {
                    projectileRicochetCounter++;

                    transform.rotation = Quaternion.LookRotation(Ricochet());

                    ricochetCooldownTimer = Mathf.RoundToInt(projectileRicochetCooldown / Time.fixedDeltaTime);
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
            #endregion
        }
    }
    #endregion

}

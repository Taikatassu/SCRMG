using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_ShipController : MonoBehaviour {

    /* TODO:
     * Implement AIPlayerController & NetworkPlayerController
     *      -When game is started, ONE player controller is assigned to ONE ship 
     *      -Other ships are controlled by AI or through network by other players
     *      
    */

    #region References & variables
    //References
    protected Core_Toolbox toolbox;
    protected Core_GlobalVariableLibrary lib;
    protected Core_EventManager em;
    protected Rigidbody rb;
    protected Vector3 movementDirection;
    protected Vector3 lookTargetPosition;
    Transform shipHull;
    Transform shipTurret;
    Transform turretOutputMarker;
    GameObject healthBar;
    Color myShipColor;
    Core_ShipColorablePartTag[] shipColorableParts;
    List<Core_Projectile> projectileList = new List<Core_Projectile>();

    //Variables coming from within the script
    protected int index = -1; //Set by gameManager when instantiating ships
    float currentHealth = -1; //Set to full by calling Resurrect() when instantiated
    float healthBarTargetValue = -1;
    float healthBarStartValue = -1;
    float healthBarLerpStartTime = -1;
    int shootCooldownFrames = -1;
    int shootCooldownFrameTimer = -1;
    public int currentGameModeIndex = -1;
    int gameModeSingleplayerIndex = -1;
    int gameModeNetworkMultiplayerIndex = -1;
    int gameModeLocalMultiplayerIndex = -1;
    bool isMovable = false;
    bool isVulnerable = false;
    bool canShoot = false;
    bool isDead = false;
    bool shootOnCooldown = false;
    bool updatingHealthBar = false;
    bool isPaused = false;
    protected bool rotatingTurret = false;

    //Values coming from GlobalVariableLibrary
    string shipTag = "Ship";
    string environmentTag = "Environment";
    protected float maxHealth = -1;
    float movementSpeed = -1;
    float shipTurretRotationSpeed = -1;
    float shipHullRotationSpeed = -1;
    float bulletLaunchForce = -1;
    float shootCooldownTime = -1;
    float shootDamage = -1;
    float healthBarMinValue = -1;
    float healthBarMaxValue = -1;
    float healthBarLerpDuration = -1;
    protected int buildPlatform = -1; //0 = PC, 1 = Android
    #endregion

    #region Initialization
    #region Awake
    protected virtual void Awake()
    {
        toolbox = FindObjectOfType<Core_Toolbox>();
        em = toolbox.GetComponent<Core_EventManager>();
        lib = toolbox.GetComponentInChildren<Core_GlobalVariableLibrary>();
        rb = GetComponent<Rigidbody>();
        shipColorableParts = GetComponentsInChildren<Core_ShipColorablePartTag>();
        shipHull = GetComponentInChildren<Core_ShipHullTag>().transform;
        shipTurret = GetComponentInChildren<Core_ShipTurretTag>().transform;
        turretOutputMarker = GetComponentInChildren<Core_TurretOutputMarkerTag>().
            transform;
        healthBar = GetComponentInChildren<Core_ShipHealthBarTag>().gameObject;
    }
    #endregion

    #region GetStats
    protected virtual void GetStats()
    {
        buildPlatform = lib.gameSettingVariables.buildPlatform;
        shipTag = lib.shipVariables.shipTag;
        environmentTag = lib.shipVariables.environmentTag;
        movementSpeed = lib.shipVariables.movementSpeed;
        maxHealth = lib.shipVariables.maxHealth;
        shipTurretRotationSpeed = lib.shipVariables.shipTurretRotationSpeed;
        shipHullRotationSpeed = lib.shipVariables.shipHullRotationSpeed;
        bulletLaunchForce = lib.shipVariables.bulletLaunchForce;
        shootCooldownTime = lib.shipVariables.shootCooldownTime;
        shootCooldownFrames = Mathf.RoundToInt(shootCooldownTime / Time.fixedDeltaTime);
        shootDamage = lib.shipVariables.shootDamage;
        healthBarMinValue = lib.shipVariables.healthBarMinValue;
        healthBarMaxValue = lib.shipVariables.healthBarMaxValue;
        healthBarLerpDuration = lib.shipVariables.healthBarLerpDuration;
        gameModeSingleplayerIndex = lib.gameSettingVariables.gameModeSingleplayerIndex;
        gameModeNetworkMultiplayerIndex = lib.gameSettingVariables.gameModeNetworkMultiplayerIndex;
        gameModeLocalMultiplayerIndex = lib.gameSettingVariables.gameModeLocalMultiplayerIndex;
    }
    #endregion

    #region OnEnable & OnDisable
    protected virtual void OnEnable()
    {
        em.OnGameRestart += OnGameRestart;
        em.OnMatchStartTimerValue += OnMatchStartTimerValue;
        em.OnGameEnd += OnGameEnd;
        em.OnPauseOn += OnPauseOn;
        em.OnPauseOff += OnPauseOff;
    }

    protected virtual void OnDisable()
    {
        DestroyAllProjectiles();
        em.OnGameRestart -= OnGameRestart;
        em.OnMatchStartTimerValue -= OnMatchStartTimerValue;
        em.OnGameEnd -= OnGameEnd;
        em.OnPauseOn -= OnPauseOn;
        em.OnPauseOff -= OnPauseOff;
    }
    #endregion

    #region Subscribers
    private void OnMatchStartTimerValue(int currentTimerValue)
    {
        if (currentTimerValue == 1)
        {
            AddHealth(maxHealth * 2);
        }
        else if (currentTimerValue == 0)
        {
            SetIsMoveable(true);
            SetIsVulnerable(true);
            SetCanShoot(true);
        }
    }

    private void OnGameRestart()
    {
        //TODO: Change if implementing a pool for ships instead of instantiating them
        //Destroy(gameObject); //Currently done by GameManager
    }

    private void OnGameEnd(int winnerIndex)
    {
        SetIsVulnerable(false);
        SetIsMoveable(false);
    }

    public void SetGameMode(int newGameModeIndex)
    {
        currentGameModeIndex = newGameModeIndex;
    }

    private void OnPauseOn()
    {
        if (currentGameModeIndex == gameModeSingleplayerIndex)
        {
            isPaused = true;
        }
        else if (currentGameModeIndex == gameModeNetworkMultiplayerIndex)
        {
            // TODO: Decide how to manage pausing in NetMP gameMode
        }
        else if (currentGameModeIndex == gameModeLocalMultiplayerIndex)
        {
            // TODO: Decide how to manage pausing in LocMP gameMode
        }
    }

    private void OnPauseOff()
    {
        if (currentGameModeIndex == gameModeSingleplayerIndex)
        {
            isPaused = false;
        }
        else if (currentGameModeIndex == gameModeNetworkMultiplayerIndex)
        {
            // TODO: Decide how to manage pausing in NetMP gameMode
        }
        else if (currentGameModeIndex == gameModeLocalMultiplayerIndex)
        {
            // TODO: Decide how to manage pausing in LocMP gameMode
        }
    }
    #endregion
    #endregion

    #region Update & FixedUpdate
    protected virtual void Update()
    {
        //ManageProjectileList();

        if (updatingHealthBar)
        {
            float timeSinceStarted = Time.time - healthBarLerpStartTime;
            float currentPercentage = timeSinceStarted / healthBarLerpDuration;
            float currentValue = Mathf.Lerp(healthBarStartValue, healthBarTargetValue, 
                currentPercentage);
            healthBar.GetComponent<Renderer>().material.SetFloat("_Cutoff", currentValue);

            if (currentPercentage >= 1.0f)
            {
                updatingHealthBar = false;
            }
        }
    }

    protected virtual void FixedUpdate()
    {

        #region Movement
        //TODO: Add lerp to movement?
        if (!isPaused)
        {
            if (isMovable && movementDirection != Vector3.zero)
            {
                rb.MovePosition(transform.position + movementDirection * movementSpeed * Time.fixedDeltaTime);
                if (movementDirection == Vector3.zero)
                {
                    rb.velocity = Vector3.zero;
                }

                //Hull rotation
                Quaternion newHullRotation = Quaternion.LookRotation(movementDirection);
                shipHull.rotation = Quaternion.Slerp(shipHull.rotation, newHullRotation,
                    Time.fixedDeltaTime * shipHullRotationSpeed);
            }
        }
        #endregion

        #region Turret rotation
        if (!isPaused)
        {
            if (buildPlatform == 0 || (buildPlatform == 1 && rotatingTurret))
            {
                lookTargetPosition.y = shipTurret.position.y;
                Vector3 lookDirection = lookTargetPosition - shipTurret.position;
                Quaternion newTurretRotation = Quaternion.LookRotation(lookDirection);
                shipTurret.rotation = Quaternion.Slerp(shipTurret.rotation, newTurretRotation,
                    Time.fixedDeltaTime * shipTurretRotationSpeed);
            }
        }
        #endregion

        #region Shoot cooldown
        if (!isPaused)
        {
            if (shootOnCooldown)
            {
                shootCooldownFrameTimer--;
                if (shootCooldownFrameTimer <= 0)
                {
                    shootOnCooldown = false;
                }
            }
        }
        #endregion
    }
    #endregion

    #region Shooting & projectiles
    #region Old projectile management
    //private void ManageProjectileList()
    //{
    //    if (projectileList.Count > 0)
    //    {
    //        for (int i = 0; i < projectileList.Count; i++)
    //        {
    //            Core_Projectile projectile = projectileList[i];
    //            //TODO: Handle projectile lifetime lengthening somehow with regards to pausing!
    //            if (Time.time >= (projectile.GetSpawnTime() + projectile.GetLifeTime()))
    //            {
    //                DestroyProjectile(projectile);
    //                i--;
    //            }
    //        }
    //    }
    //}
    #endregion

    private void DestroyAllProjectiles()
    {
        int count = projectileList.Count;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                DestroyProjectile(projectileList[0]);
            }
        }
    }

    private void DestroyProjectile(Core_Projectile projectile)
    {
        projectileList.Remove(projectile);
        if (projectile != null)
            Destroy(projectile.gameObject);
    }

    public void OnProjectileLifetimeEnded(Core_Projectile projectile)
    {
        DestroyProjectile(projectile);
    }

    public void OnProjectileTriggerEnter(Core_Projectile projectile, GameObject collidedObject)
    {
        //Check which object collided with
        //If enemy ship, damage enemyShip
        //Destroy projectile
        //Instantiate effect
        string collidedObjectTag = collidedObject.tag;


        if (collidedObjectTag == shipTag)
        {
            //Spawn hit effect
            GameObject bulletHitEffect = Instantiate(Resources.Load("BulletHitEffect"),
                projectile.transform.position, Quaternion.identity) as GameObject;
            bulletHitEffect.GetComponentInChildren<Renderer>().material.SetColor("_TintColor", myShipColor);

            //Damage enemy ship
            collidedObject.GetComponentInParent<Core_ShipController>().TakeDamage(shootDamage);
            //Destroy projectile
            DestroyProjectile(projectile);
        }
        else if (collidedObjectTag == environmentTag)
        {
            //Spawn hit effect
            GameObject bulletHitEffect = Instantiate(Resources.Load("BulletHitEffect"),
                projectile.transform.position, Quaternion.identity) as GameObject;
            bulletHitEffect.GetComponentInChildren<Renderer>().material.SetColor("_TintColor", myShipColor);

            DestroyProjectile(projectile);
        }

    }

    protected void Shoot()
    {
        if (!isPaused)
        {
            if (canShoot && !shootOnCooldown)
            {
                //Spawn bullet at shipTurret position & rotation
                GameObject newBullet = Instantiate(Resources.Load("Bullet", typeof(GameObject)),
                    turretOutputMarker.position, turretOutputMarker.rotation) as GameObject;
                Physics.IgnoreCollision(newBullet.GetComponent<Collider>(),
                    GetComponentInChildren<Collider>());

                Core_Projectile newBulletScript = newBullet.GetComponent<Core_Projectile>();
                newBulletScript.SetProjectileType(Core_Projectile.EProjectileType.BULLET);
                newBulletScript.SetShipController(this);
                newBulletScript.SetProjectileColor(myShipColor);

                projectileList.Add(newBulletScript);
                //Set shoot on cooldown
                shootCooldownFrameTimer = shootCooldownFrames;
                shootOnCooldown = true;
            }
        }
    }
    #endregion

    #region SetVariables
    public void GiveIndex(int newIndex)
    {
        if (index == -1)
        {
            index = newIndex;
        }
    }

    protected void SetLookTargetPosition(Vector3 newLookTargetPosition)
    {
        lookTargetPosition = newLookTargetPosition;
    }

    protected void SetIsMoveable(bool state)
    {
        isMovable = state;
    }

    protected void SetIsVulnerable(bool state)
    {
        isVulnerable = state;
    }

    protected void SetCanShoot(bool state)
    {
        canShoot = state;
    }

    public void SetShipColor(Color newColor)
    {
        myShipColor = newColor;

        //Set shipColorableParts color
        for (int i = 0; i < shipColorableParts.Length; i++)
        {
            shipColorableParts[i].GetComponent<Renderer>().material.SetColor("_TintColor", myShipColor);
        }
        //Set circularHealthBarColor
        healthBar.GetComponent<Renderer>().material.SetColor("_EmissionColor", myShipColor);
    }
    #endregion

    #region GetVariables
    public int GetIndex()
    {
        return index;
    }

    public Color GetShipColor()
    {
        return myShipColor;
    }
    #endregion

    #region Health adjustments
    public void TakeDamage(float amount)
    {
        if (!isDead && isVulnerable)
        {
            //Debug.Log("I'm taking damage.");
            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
            //Update UI
            UpdateHealthBar();
        }
    }

    public void AddHealth(float amount)
    {
        if (!isDead)
        {
            currentHealth += amount;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
            //Update UI
            UpdateHealthBar();
        }
    }
    #endregion

    #region Die, Resurrect
    private void Die()
    {
        isDead = true;
        isVulnerable = false;
        isMovable = false;
        canShoot = false;
        //Broadcast ship death
        //Start spectator mode if player
        em.BroadcastShipDead(index);
        
        GameObject shipDeathEffect = Instantiate(Resources.Load("ShipDeathEffect"),
            transform.position, Quaternion.identity) as GameObject;
        shipDeathEffect.GetComponentInChildren<Renderer>().material.SetColor("_TintColor", myShipColor);

        Destroy(gameObject);
    }

    //protected void Resurrect()
    //{
    //    // TODO: Currently Die-method destroys the object, so resurrect is unneccessary
    //    //      Remove if still obsolete in the future (currently only used for setting isDead 
    //    //      to true when initializing ships)
    //    Debug.Log("Resurrecting");
    //    //Reset all stats
    //    isDead = false;
    //    AddHealth(maxHealth);
    //}
    #endregion

    #region Worldspace UI
    private void UpdateHealthBar()
    {
        healthBarTargetValue = Mathf.Clamp((1 - (currentHealth / maxHealth)), 
            healthBarMinValue, healthBarMaxValue);
        healthBarStartValue = healthBar.GetComponent<Renderer>().material.GetFloat("_Cutoff");
        healthBarLerpStartTime = Time.time;
        updatingHealthBar = true;
    }
    #endregion

    #region Collision detection
    //private void OnCollisionEnter()
    //{
    //    Debug.Log("OnCollisionEnter");
    //}
    #endregion
}

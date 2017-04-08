﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour {


    #region References & variables
    //References
    protected Toolbox toolbox;
    protected GlobalVariableLibrary lib;
    protected EventManager em;
    protected Rigidbody rb;
    protected Vector3 movementDirection;
    protected Vector3 lookTargetPosition;
    protected Transform shipHull;
    Transform shipTurret;
    Transform turretOutputMarker;
    GameObject healthBar;
    Color myShipColor;
    ShipColorablePartTag[] shipColorableParts;
    Projectile newProjectileScript;

    //Variables coming from within the script
    protected int index = -1;
    float currentHealth = -1;
    float healthBarTargetValue = -1;
    float healthBarStartValue = -1;
    float healthBarLerpStartTime = -1;
    float defaultMovementSpeed = -1;
    float speedModifier = -1;
    float damageTakenModifier = -1;
    float shootCooldownModifier = -1;
    public int currentGameModeIndex = -1;
    int shootCooldownFrameTimer = -1;
    int gameModeSingleplayerIndex = -1;
    int gameModeNetworkMultiplayerIndex = -1;
    int gameModeLocalMultiplayerIndex = -1;
    int powerUpType = -1;
    bool isPersistingProjectile = false;
    bool persistingProjectileOnline = false;
    bool isMovable = false;
    bool isVulnerable = false;
    bool canShoot = false;
    bool isDead = false;
    bool shootOnCooldown = false;
    bool updatingHealthBar = false;
    bool isPaused = false;
    bool isPoweredUp = false;
    protected bool rotatingTurret = false;
    //Values coming from GlobalVariableLibrary
    protected float maxHealth = -1;
    float movementSpeed = -1;
    float shipTurretRotationSpeed = -1;
    float shipHullRotationSpeed = -1;
    float shootCooldownDuration = -1;
    float healthBarMinValue = -1;
    float healthBarMaxValue = -1;
    float healthBarLerpDuration = -1;
    protected int buildPlatform = -1; //0 = PC, 1 = Android
    int projectileType = -1;
    int powerUpTimer = -1;
    //TODO: Remove if deemed permanently obsolete
    //int rubberBulletsIndex = -1;
    //int blazingRamIndex = -1;
    //int beamCannonIndex = -1;
    //int bombsIndex = -1;
    #endregion

    #region Initialization
    #region Awake
    protected virtual void Awake()
    {
        toolbox = FindObjectOfType<Toolbox>();
        em = toolbox.GetComponent<EventManager>();
        lib = toolbox.GetComponentInChildren<GlobalVariableLibrary>();
        rb = GetComponent<Rigidbody>();
        shipColorableParts = GetComponentsInChildren<ShipColorablePartTag>();
        shipHull = GetComponentInChildren<ShipHullTag>().transform;
        shipTurret = GetComponentInChildren<ShipTurretTag>().transform;
        turretOutputMarker = GetComponentInChildren<TurretOutputMarkerTag>().
            transform;
        //healthBar = GetComponentInChildren<ShipHealthBarTag>().gameObject;
    }
    #endregion

    #region GetStats
    protected virtual void GetStats()
    {
        //buildPlatform = lib.gameSettingVariables.buildPlatform;
        //defaultMovementSpeed = lib.shipVariables.movementSpeed;
        //movementSpeed = defaultMovementSpeed;
        //maxHealth = lib.shipVariables.maxHealth;
        //shipTurretRotationSpeed = lib.shipVariables.shipTurretRotationSpeed;
        //shipHullRotationSpeed = lib.shipVariables.shipHullRotationSpeed;
        //shootCooldownDuration = lib.shipVariables.shootCooldownDuration;
        //healthBarMinValue = lib.shipVariables.healthBarMinValue;
        //healthBarMaxValue = lib.shipVariables.healthBarMaxValue;
        //healthBarLerpDuration = lib.shipVariables.healthBarLerpDuration;
        //gameModeSingleplayerIndex = lib.gameSettingVariables.gameModeSingleplayerIndex;
        //gameModeNetworkMultiplayerIndex = lib.gameSettingVariables.gameModeNetworkMultiplayerIndex;
        //gameModeLocalMultiplayerIndex = lib.gameSettingVariables.gameModeLocalMultiplayerIndex;
        //TODO: Remove if deemed permanently obsolete
        //rubberBulletsIndex = -lib.powerUpVariables.rubberBulletsIndex;
        //blazingRamIndex = lib.powerUpVariables.blazingRamIndex;
        //beamCannonIndex = lib.powerUpVariables.beamCannonIndex;
        //bombsIndex = lib.powerUpVariables.bombsIndex;
    }
    #endregion

    #region OnEnable & OnDisable
    protected virtual void OnEnable()
    {
        //em.OnGameRestart += OnGameRestart;
        //em.OnMatchStartTimerValueChange += OnMatchStartTimerValueChange;
        //em.OnMatchStarted += OnMatchStarted;
        //em.OnMatchEnded += OnMatchEnded;
        //em.OnPauseOn += OnPauseOn;
        //em.OnPauseOff += OnPauseOff;

        powerUpType = 0;
        powerUpTimer = 0;
        projectileType = 0;
        isPersistingProjectile = false;
        speedModifier = 1;
        damageTakenModifier = 1;
        shootCooldownModifier = 1;
        canShoot = false;
        isMovable = false;
        isVulnerable = false;
        isPoweredUp = false;
    }

    protected virtual void OnDisable()
    {
        //DestroyAllProjectiles();
        //em.OnGameRestart -= OnGameRestart;
        //em.OnMatchStartTimerValueChange -= OnMatchStartTimerValueChange;
        //em.OnMatchStarted -= OnMatchStarted;
        //em.OnMatchEnded -= OnMatchEnded;
        //em.OnPauseOn -= OnPauseOn;
        //em.OnPauseOff -= OnPauseOff;
    }
    #endregion

    #region Subscribers
    private void OnMatchStartTimerValueChange(int currentTimerValue)
    {
        if (currentTimerValue == 1)
        {
            AddHealth(maxHealth * 2);
        }
    }

    private void OnGameRestart()
    {
        //TODO: Change if implementing a pool for ships instead of instantiating them
        //Destroy(gameObject); //Currently done by GameManager
    }

    private void OnMatchStarted()
    {
        powerUpType = 0;
        SetIsMoveable(true);
        SetIsVulnerable(true);
        SetCanShoot(true);
    }

    private void OnMatchEnded(int winnerIndex)
    {
        powerUpType = 0;
        SetIsVulnerable(false);
        SetIsMoveable(false);
        SetCanShoot(false);
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
        #region Circular health bar updating
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
        #endregion
    }

    protected virtual void FixedUpdate()
    {
        #region Movement
        //TODO: Add lerp to movement?
        if (!isPaused)
        {
            if (isMovable && movementDirection != Vector3.zero)
            {
                float movementDirectionMagnitude = movementDirection.magnitude;
                if (movementDirectionMagnitude > 1)
                {
                    movementDirection = movementDirection / movementDirectionMagnitude;
                }
                rb.MovePosition(transform.position + movementDirection * (movementSpeed * speedModifier) * Time.fixedDeltaTime);
                if (movementDirection == Vector3.zero)
                {
                    rb.velocity = Vector3.zero;
                }

                //Hull rotation
                Quaternion newHullRotation = Quaternion.LookRotation(movementDirection);
                shipHull.rotation = Quaternion.Slerp(shipHull.rotation, newHullRotation,
                    Time.fixedDeltaTime * shipHullRotationSpeed);
            }

            //TODO: Implement a proper way to detect if ship is outside of arena bounds, and returning it back to arena
            Vector3 currentPosition = transform.position;
            if (currentPosition.x > 25)
            {
                Vector3 newPosition = currentPosition;
                newPosition.x = 25;
                transform.position = newPosition;
            }
            else if (currentPosition.x < -25)
            {
                Vector3 newPosition = currentPosition;
                newPosition.x = -25;
                transform.position = newPosition;

            }
            else if (currentPosition.z > 25)
            {
                Vector3 newPosition = currentPosition;
                newPosition.z = 25;
                transform.position = newPosition;
            }
            else if (currentPosition.z < -25)
            {
                Vector3 newPosition = currentPosition;
                newPosition.z = -25;
                transform.position = newPosition;
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

        #region PowerUp timer
        if (!isPaused)
        {
            if (isPoweredUp)
            {
                powerUpTimer--;
                if (powerUpTimer <= 0)
                {
                    EndPowerUp();
                }
            }
        }
        #endregion
    }
    #endregion

    #region PowerUp management
    private void EndPowerUp()
    {
        //em.BroadcastPowerUpEnded(index, powerUpType);
        EndPersistingProjectile();
        powerUpType = 0;
        powerUpTimer = 0;
        projectileType = 0;
        isPersistingProjectile = false;
        speedModifier = 1;
        damageTakenModifier = 1;
        shootCooldownModifier = 1;
        canShoot = true;
        isMovable = true;
        isVulnerable = true;
        isPoweredUp = false;
    }
    #endregion

    #region Shooting & projectiles
    protected void Shoot()
    {
        if (!isPaused)
        {
            if (canShoot && !shootOnCooldown)
            {
                //Debug.Log("Shooting, projectileType: " + projectileType);
                if (!isPersistingProjectile || (isPersistingProjectile && !persistingProjectileOnline))
                {
                    GameObject newProjectile;

                    if (isPersistingProjectile)
                    {
                        newProjectile = Instantiate(Resources.Load("Projectiles/Projectile", typeof(GameObject)),
                                shipTurret.transform) as GameObject;

                        persistingProjectileOnline = true;
                    }
                    else
                    {
                        newProjectile = Instantiate(Resources.Load("Projectiles/Projectile", typeof(GameObject)),
                                turretOutputMarker.position, turretOutputMarker.rotation) as GameObject;
                    }

                    Physics.IgnoreCollision(newProjectile.GetComponent<Collider>(),
                        GetComponentInChildren<Collider>());

                    newProjectileScript = newProjectile.GetComponent<Projectile>();
                    newProjectileScript.SetShipController(this);
                    newProjectileScript.SetProjectileType(projectileType);

                    //Set shoot on cooldown
                    shootCooldownFrameTimer = Mathf.RoundToInt((shootCooldownDuration * shootCooldownModifier) / Time.fixedDeltaTime);
                    shootOnCooldown = true;

                }
            }
        }
    }

    protected void EndPersistingProjectile()
    {
        if (isPersistingProjectile)
        {
            if (newProjectileScript != null)
            {
                newProjectileScript.OnPersistingProjectileDestruction();
            }
            persistingProjectileOnline = false;
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

    public void SetPowerUpType(int newPowerUpType, float newDuration,
        int newProjectileType, bool newIsPersistingProjectileState, float newSpeedModifier,
        float newDamageTakenModifier, float newShootCooldownModifier, bool newCanShootState,
        bool newIsMoveableState, bool newIsVulnerableState)
    {
        if (isPoweredUp)
        {
            EndPowerUp();
        }

        powerUpType = newPowerUpType;
        powerUpTimer = Mathf.RoundToInt(newDuration / Time.fixedDeltaTime);
        projectileType = newProjectileType;
        isPersistingProjectile = newIsPersistingProjectileState;
        speedModifier = newSpeedModifier;
        damageTakenModifier = newDamageTakenModifier;
        shootCooldownModifier = newShootCooldownModifier;
        canShoot = newCanShootState;
        isMovable = newIsMoveableState;
        isVulnerable = newIsVulnerableState;
        isPoweredUp = true;
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
            currentHealth -= amount * damageTakenModifier;
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
        //em.BroadcastShipDead(index);

        GameObject shipDeathEffect = Instantiate(Resources.Load("Effects/ShipDeathEffect"),
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

    #region [Currently obsolete] Collision detection
    //TODO: Remove if deemed permanently obsolete
    //private void OnCollisionEnter()
    //{
    //    Debug.Log("OnCollisionEnter");
    //}
    #endregion
}

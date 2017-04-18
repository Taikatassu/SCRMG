using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{


    #region References & variables
    //References
    protected Toolbox toolbox;
    protected GlobalVariableLibrary lib;
    protected ShipInfoManager shipInfoManager;
    protected EventManager em;
    protected Rigidbody rb;
    protected Vector3 movementDirection;
    protected Vector3 lookTargetPosition;
    protected Transform shipHull;
    protected Transform shipTurret;
    Transform turretOutputMarker;
    GameObject healthBar;
    Color myShipColor;
    ShipColorablePartTag[] shipColorableParts;
    Projectile newProjectileScript;

    //Variables coming from within the script
    protected List<int> currentProjectileIndices = new List<int>();
    protected int index = -1;
    protected int myShipInfoElement = -1;
    float currentHealth = -1;
    float healthBarTargetValue = -1;
    float healthBarStartValue = -1;
    float healthBarLerpStartTime = -1;
    float defaultMovementSpeed = -1;
    float speedModifier = -1;
    float damageTakenModifier = -1;
    float shootCooldownModifier = -1;
    int shootCooldownFrameTimer = -1;
    bool isMovable = false;
    bool isVulnerable = false;
    bool canShoot = false;
    bool isDead = false;
    bool shootOnCooldown = false;
    bool updatingHealthBar = false;
    protected bool rotatingTurret = false;
    protected bool isControllerByServer = false;
    //Values coming from GlobalVariableLibrary
    protected float maxHealth = -1;
    float movementSpeed = -1;
    float shipTurretRotationSpeed = -1;
    float shipHullRotationSpeed = -1;
    float shootCooldownDuration = -1;
    float healthBarMinValue = -1;
    float healthBarMaxValue = -1;
    float healthBarLerpDuration = -1;
    #endregion

    #region Initialization
    #region Awake
    protected virtual void Awake()
    {
        toolbox = FindObjectOfType<Toolbox>();
        em = toolbox.GetComponent<EventManager>();
        lib = toolbox.GetComponentInChildren<GlobalVariableLibrary>();
        shipInfoManager = toolbox.GetComponent<ShipInfoManager>();
        rb = GetComponent<Rigidbody>();
        shipColorableParts = GetComponentsInChildren<ShipColorablePartTag>();
        shipHull = GetComponentInChildren<ShipHullTag>().transform;
        shipTurret = GetComponentInChildren<ShipTurretTag>().transform;
        turretOutputMarker = GetComponentInChildren<TurretOutputMarkerTag>().
            transform;
        healthBar = GetComponentInChildren<ShipHealthBarTag>().gameObject;
    }
    #endregion

    #region GetStats
    protected virtual void GetStats()
    {
        defaultMovementSpeed = lib.serverVariables.movementSpeed;
        movementSpeed = defaultMovementSpeed;
        maxHealth = lib.serverVariables.maxHealth;
        shipTurretRotationSpeed = lib.serverVariables.shipTurretRotationSpeed;
        shipHullRotationSpeed = lib.serverVariables.shipHullRotationSpeed;
        shootCooldownDuration = lib.serverVariables.shootCooldownDuration;
        healthBarMinValue = lib.serverVariables.healthBarMinValue;
        healthBarMaxValue = lib.serverVariables.healthBarMaxValue;
        healthBarLerpDuration = lib.serverVariables.healthBarLerpDuration;
    }
    #endregion

    #region OnEnable & OnDisable
    protected virtual void OnEnable()
    {
        //TODO: Add projectile destruction when quitting game or restarting
        //em.OnGameRestart += OnGameRestart;
        em.OnMatchStartTimerValueChange += OnMatchStartTimerValueChange;
        em.OnMatchStarted += OnMatchStarted;
        em.OnMatchEnded += OnMatchEnded;
        em.OnProjectileDestroyed += OnProjectileDestroyed;
        em.OnProjectileHitShipByClient += OnProjectileHitShipByClient;

        speedModifier = 1;
        damageTakenModifier = 1;
        shootCooldownModifier = 1;
        canShoot = false;
        isMovable = false;
        isVulnerable = false;
    }

    protected virtual void OnDisable()
    {
        //em.OnGameRestart -= OnGameRestart;
        em.OnMatchStartTimerValueChange -= OnMatchStartTimerValueChange;
        em.OnMatchStarted -= OnMatchStarted;
        em.OnMatchEnded -= OnMatchEnded;
        em.OnProjectileDestroyed -= OnProjectileDestroyed;
        em.OnProjectileHitShipByClient -= OnProjectileHitShipByClient;
    }
    #endregion

    #region Subscribers
    private void OnProjectileHitShipByClient(int projectileOwnerIndex, int projectileIndex, int hitShipIndex, float projectileDamage)
    {
        if (hitShipIndex == index && projectileOwnerIndex != index)
        {
            TakeDamage(projectileDamage, projectileOwnerIndex);
        }
    }

    private void OnProjectileDestroyed(int projectileOwnerIndex, int projectileIndex, Vector3 location)
    {
        if (projectileOwnerIndex == index)
        {
            RemoveProjectileIndexFromList(projectileIndex);
        }
    }

    private void OnMatchStartTimerValueChange(int currentTimerValue)
    {
        if (currentTimerValue == 1)
        {
            AddHealth(maxHealth * 2);
        }
    }

    private void OnMatchStarted()
    {
        SetIsMoveable(true);
        SetIsVulnerable(true);
        SetCanShoot(true);
    }

    private void OnMatchEnded(int winnerIndex)
    {
        SetIsVulnerable(false);
        SetIsMoveable(false);
        SetCanShoot(false);
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
        if (isControllerByServer)
        {
            #region Movement
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

                Quaternion newHullRotation = Quaternion.LookRotation(movementDirection);
                shipHull.rotation = Quaternion.Slerp(shipHull.rotation, newHullRotation,
                    Time.fixedDeltaTime * shipHullRotationSpeed);
            }

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
            #endregion

            #region Turret rotation
            lookTargetPosition.y = shipTurret.position.y;
            Vector3 lookDirection = lookTargetPosition - shipTurret.position;
            Quaternion newTurretRotation = Quaternion.LookRotation(lookDirection);
            shipTurret.rotation = Quaternion.Slerp(shipTurret.rotation, newTurretRotation,
                Time.fixedDeltaTime * shipTurretRotationSpeed);
            #endregion

            #region Shoot cooldown
            if (shootOnCooldown)
            {
                shootCooldownFrameTimer--;
                if (shootCooldownFrameTimer <= 0)
                {
                    shootOnCooldown = false;
                }
            }
            #endregion

            #region Updating ShipInfo
            if (myShipInfoElement == -1)
            {
                myShipInfoElement = shipInfoManager.GetMyShipInfoElement(index);
            }

            if (myShipInfoElement != -1)
            {
                if (shipInfoManager.shipInfoList[myShipInfoElement].isDead)
                {
                    Die(shipInfoManager.shipInfoList[myShipInfoElement].killerIndex);
                }

                shipInfoManager.shipInfoList[myShipInfoElement].shipPosition = transform.position;
                shipInfoManager.shipInfoList[myShipInfoElement].hullRotation = shipHull.eulerAngles;
                shipInfoManager.shipInfoList[myShipInfoElement].turretRotation = shipTurret.eulerAngles;
            }
            #endregion
        }
        else
        {
            #region Movement
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
            #endregion

            #region Turret rotation
            lookTargetPosition.y = shipTurret.position.y;
            Vector3 lookDirection = lookTargetPosition - shipTurret.position;
            Quaternion newTurretRotation = Quaternion.LookRotation(lookDirection);
            shipTurret.rotation = Quaternion.Slerp(shipTurret.rotation, newTurretRotation,
                Time.fixedDeltaTime * shipTurretRotationSpeed);
            #endregion
        }
    }
    #endregion

    #region Shooting & projectiles
    protected void Shoot()
    {
        if (canShoot && !shootOnCooldown)
        {
            GameObject newProjectile = Instantiate(Resources.Load("Projectiles/Projectile", typeof(GameObject)),
                            turretOutputMarker.position, turretOutputMarker.rotation) as GameObject;

            Physics.IgnoreCollision(newProjectile.GetComponent<Collider>(),
                GetComponentInChildren<Collider>());

            newProjectileScript = newProjectile.GetComponent<Projectile>();
            newProjectileScript.InitializeProjectile(index, GetNewProjectileIndex(), 0, myShipColor, true);

            shootCooldownFrameTimer = Mathf.RoundToInt((shootCooldownDuration * shootCooldownModifier) / Time.fixedDeltaTime);
            shootOnCooldown = true;
        }
    }

    private void RemoveProjectileIndexFromList(int destroyedProjectileIndex)
    {
        if (currentProjectileIndices.Contains(destroyedProjectileIndex))
        {
            currentProjectileIndices.Remove(destroyedProjectileIndex);
            Debug.Log("DestroyedProjectileIndex found and removed from list. destroyedProjectileIndex: " + destroyedProjectileIndex);
        }
        else
        {
            Debug.LogError("DestroyedProjectileIndex NOT found in list. destroyedProjectileIndex: " + destroyedProjectileIndex);
        }
    }

    private int GetNewProjectileIndex()
    {
        int availableIndex = -1;
        if (currentProjectileIndices.Count == 0)
        {
            availableIndex = 1;
            Debug.Log("Projectile index list empty, creating new projectile index: " + availableIndex);
            currentProjectileIndices.Add(availableIndex);
            return availableIndex;
        }
        else
        {
            for (int i = 0; i < currentProjectileIndices.Count + 1; i++)
            {
                if (!currentProjectileIndices.Contains(i))
                {
                    availableIndex = i;
                    Debug.Log("Available projectile index found: " + availableIndex);
                    currentProjectileIndices.Add(availableIndex);
                    return availableIndex;
                }
            }
            availableIndex = currentProjectileIndices.Count + 1;
            Debug.Log("Creating new projectile index: " + availableIndex);
            currentProjectileIndices.Add(availableIndex);
            return availableIndex;
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
    public void TakeDamage(float amount, int damageDealerIndex)
    {
        Debug.Log("TakeDamage, amount: " + amount);
        if (!isDead && isVulnerable)
        {
            //Debug.Log("I'm taking damage.");
            currentHealth -= amount * damageTakenModifier;
            Debug.Log("TakeDamage, currentHealth: " + currentHealth);
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die(damageDealerIndex);
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
    private void Die(int killerIndex)
    {
        Debug.Log("ShipController: Die");
        isDead = true;
        isVulnerable = false;
        isMovable = false;
        canShoot = false;
        //Broadcast ship death
        //Start spectator mode if player
        em.BroadcastShipDead(index, killerIndex);

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
}

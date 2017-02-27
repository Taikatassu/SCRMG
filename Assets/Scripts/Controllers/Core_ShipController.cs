using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_ShipController : MonoBehaviour {

    /* TODO:
     * Implement AIPlayerController & NetworkPlayerController
     *      -When game is started, ONE player controller is assigned to ONE ship 
     *      -Other ships are controlled by AI or through network by other players
     *      
     * Spectator mode
    */

    protected Core_Toolbox toolbox;
    protected Core_GlobalVariableLibrary lib;
    protected Core_EventManager em;
    protected Rigidbody rb;
    Vector3 movementDirection;
    Core_ShipColorablePartTag[] shipColorableParts;
    Vector3 lookTargetPosition;
    Transform shipHull;
    Transform shipTurret;
    Transform turretOutputMarker;
    List<Core_Projectile> projectileList = new List<Core_Projectile>();
    protected int index = 0;
    float currentHealth = 0;
    bool isMovable = false;
    bool isVulnerable = false;
    bool canShoot = false;
    bool isDead = false;
    bool shootOnCooldown = false;
    string shipTag = "Ship";
    string environmentTag = "Environment";
    GameObject healthBar;
    float healthBarMinValue = 0.01f;
    float healthBarMaxValue = 1;

    //Values coming from GlobalVariableLibrary
    float movementSpeed = 0;
    float maxHealth = 0;
    float shipTurretRotationSpeed = 0;
    float shipHullRotationSpeed = 0;
    float bulletLaunchForce = 0;
    float shootCooldownTime = 0;
    float shootDamage = 0;

    protected virtual void Awake()
    {
        toolbox = FindObjectOfType<Core_Toolbox>();
        lib = toolbox.GetComponentInChildren<Core_GlobalVariableLibrary>();
        em = toolbox.GetComponent<Core_EventManager>();
        rb = GetComponent<Rigidbody>();
        shipColorableParts = GetComponentsInChildren<Core_ShipColorablePartTag>();
        shipHull = GetComponentInChildren<Core_ShipHullTag>().transform;
        shipTurret = GetComponentInChildren<Core_ShipTurretTag>().transform;
        turretOutputMarker = GetComponentInChildren<Core_TurretOutputMarkerTag>().
            transform;
        healthBar = GetComponentInChildren<Core_ShipHealthBarTag>().gameObject;
    }

    protected virtual void GetStats()
    {
        Debug.Log("Getting stats");
        movementSpeed = lib.shipVariables.movementSpeed;
        maxHealth = lib.shipVariables.maxHealth;
        shipTurretRotationSpeed = lib.shipVariables.shipTurretRotationSpeed;
        shipHullRotationSpeed = lib.shipVariables.shipHullRotationSpeed;
        bulletLaunchForce = lib.shipVariables.bulletLaunchForce;
        shootCooldownTime = lib.shipVariables.shootCooldownTime;
        shootDamage = lib.shipVariables.shootDamage;
        Debug.Log("maxHealth: " + maxHealth);
    }

    protected virtual void Update()
    {
        ManageProjectileList();
    }

    protected virtual void FixedUpdate()
    {
        #region Movement
        //TODO: Add lerp to movement?
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
        #endregion

        #region Turret rotation
        lookTargetPosition.y = shipTurret.position.y;
        Vector3 lookDirection = lookTargetPosition - shipTurret.position;
        Quaternion newTurretRotation = Quaternion.LookRotation(lookDirection);
        shipTurret.rotation = Quaternion.Slerp(shipTurret.rotation, newTurretRotation,
            Time.fixedDeltaTime * shipTurretRotationSpeed);
        #endregion
    }

    #region Shooting & projectiles
    private void ManageProjectileList()
    {
        if (projectileList.Count > 0)
        {
            for (int i = 0; i < projectileList.Count; i++)
            {
                Core_Projectile projectile = projectileList[i];
                if (Time.time >= (projectile.GetSpawnTime() + projectile.GetLifeTime()))
                {
                    DestroyProjectile(projectile);
                    i--;
                }
            }
        }
    }

    private void DestroyProjectile(Core_Projectile projectile)
    {
        projectileList.Remove(projectile);
        Destroy(projectile.gameObject);
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
            //Damage enemy ship
            collidedObject.GetComponentInParent<Core_ShipController>().TakeDamage(shootDamage);
            //Destroy projectile
            DestroyProjectile(projectile);
        }
        else if (collidedObjectTag == environmentTag)
        {
            DestroyProjectile(projectile);
        }

    }

    protected void Shoot()
    {
        if (canShoot && !shootOnCooldown)
        {
            //Spawn bullet at shipTurret position & rotation
            GameObject newBullet = Instantiate(Resources.Load("Bullet", typeof(GameObject)),
                turretOutputMarker.position, turretOutputMarker.rotation) as GameObject;
            Physics.IgnoreCollision(newBullet.GetComponent<Collider>(), 
                GetComponentInChildren<Collider>());
            newBullet.GetComponent<Rigidbody>().AddForce(newBullet.transform.forward *
                bulletLaunchForce, ForceMode.Impulse);
            Core_Projectile newBulletScript = newBullet.GetComponent<Core_Projectile>();
            newBulletScript.SetProjectileType(Core_Projectile.EProjectileType.BULLET);
            newBulletScript.SetShipController(this);
            projectileList.Add(newBulletScript);

            StartCoroutine(ShootCooldown(shootCooldownTime));
        }
    }

    IEnumerator ShootCooldown(float time)
    {
        shootOnCooldown = true;
        yield return new WaitForSeconds(time);
        shootOnCooldown = false;
    }
    #endregion

    #region Index
    public void GiveIndex(int newIndex)
    {
        if (index == 0)
        {
            index = newIndex;
        }
    }

    //public int GetIndex()
    //{
    //    return index;
    //}
    #endregion

    #region SetVariables
    protected void SetMovementDirection(Vector3 newMovementDirection)
    {
        movementDirection = newMovementDirection;
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
        for (int i = 0; i < shipColorableParts.Length; i++)
        {
            shipColorableParts[i].GetComponent<Renderer>().material.color = newColor;
        }
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
            Debug.Log("I'm taking damage. Current hp: " + currentHealth);
        }
    }

    public void AddHealth(float amount)
    {
        if (!isDead)
        {
            currentHealth += amount;
            if (currentHealth > maxHealth)
            {
                currentHealth = 0;
            }
            //Update UI
            UpdateHealthBar();
        }
    }
    #endregion

    #region Die & Resurrect
    private void Die()
    {
        Debug.Log("I'm dead.");
        isDead = true;
        isVulnerable = false;
        isMovable = false;
        canShoot = false;
        //Broadcast ship death
        /* TODO: Remove this and implement a way to disable ship mesh and leave the ship
         *      as a moveable object in game to allow spectating after death.
         */
        gameObject.SetActive(false); 
        //Start spectator mode if player
    }

    protected void Resurrect()
    {
        Debug.Log("Resurrecting");
        //Reset all stats
        isDead = false;
        AddHealth(maxHealth);
    }
    #endregion

    #region Worldspace UI

    private void UpdateHealthBar()
    {
        float healthBarFillAmount = 1 - (currentHealth / maxHealth);
        Debug.Log("Updating healthbar. Fill amount: " + healthBarFillAmount);
        healthBar.GetComponent<Renderer>().material.SetFloat("_Cutoff",
            Mathf.Clamp(healthBarFillAmount, healthBarMinValue, healthBarMaxValue));
    }
    #endregion

    #region Collision detection
    private void OnCollisionEnter()
    {
        Debug.Log("OnCollisionEnter");
    }
    #endregion
}

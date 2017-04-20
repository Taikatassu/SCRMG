using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpController : MonoBehaviour {

    #region References & variables
    Toolbox toolbox;
    EventManager em;
    GlobalVariableLibrary lib;
    ParticleSystem powerUpPlatformEffect;
    ParticleSystem powerUpPickupEffect;
    GameObject powerUpObject;
    Collider powerUpPlatformCollider;

    //Values coming from within the script
    List<int> availablePowerUps = new List<int>();
    public int powerUpPlatformIndex = -1;
    int powerUpCooldownTimer = -1;
    int powerUpType = -1;
    int powerUpProjectileType = -1;
    int powerUpPlatformColliderTickRateCounter = -1;
    float powerUpDuration = -1;
    float powerUpShipSpeedModifier = -1;
    float powerUpShipDamageTakenModifier = -1;
    float powerUpShootCooldownModifier = -1;
    float powerUpPlatformColliderTickInterval = -1;
    bool powerUpIsPersistingProjectile = false;
    bool powerUpCanShootState = false;
    bool powerUpIsMovableState = false;
    bool powerUpIsVulnerableState = false;
    bool powerUpOnline = false;
    bool isPaused = false;
    bool matchStarted = false;
    //Values coming from GlobalVariableLibrary
    float powerUpCooldown = -1;
    float powerUpPlatformColliderTickRate = -1;
    int rubberBulletsIndex = -1;
    int blazingRamIndex = -1;
    int beamCannonIndex = -1;
    int bombsIndex = -1;
    bool rubberBulletsAvailable = false;
    bool blazingRamAvailable = false;
    bool beamCannonAvailable = false;
    bool bombsAvailable = false;
    #endregion

    #region Awake & GetStats
    private void Awake ()
    {
        toolbox = FindObjectOfType<Toolbox>();
        em = toolbox.GetComponent<EventManager>();
        lib = toolbox.GetComponent<GlobalVariableLibrary>();
        powerUpPlatformEffect = transform.GetChild(1).GetComponent<ParticleSystem>();
        powerUpPickupEffect = transform.GetChild(2).GetComponent<ParticleSystem>();
        powerUpObject = GetComponentInChildren<PowerUpAnimator>().gameObject;
        powerUpPlatformCollider = transform.GetComponent<Collider>();

        GetStats();
    }

    private void GetStats()
    {
        powerUpCooldown = lib.powerUpVariables.powerUpCooldown;
        powerUpPlatformColliderTickRate = lib.powerUpVariables.powerUpPlatformColliderTickRate;
        powerUpPlatformColliderTickInterval = 1 / powerUpPlatformColliderTickRate;

        #region PowerUp indices & availability
        rubberBulletsIndex = lib.powerUpVariables.rubberBulletsIndex;
        blazingRamIndex = lib.powerUpVariables.blazingRamIndex;
        beamCannonIndex = lib.powerUpVariables.beamCannonIndex;
        bombsIndex = lib.powerUpVariables.bombsIndex;

        rubberBulletsAvailable = lib.powerUpVariables.rubberBulletsAvailable;
        blazingRamAvailable = lib.powerUpVariables.blazingRamAvailable;
        beamCannonAvailable = lib.powerUpVariables.beamCannonAvailable;
        bombsAvailable = lib.powerUpVariables.bombsAvailable;

        if (rubberBulletsAvailable)
        {
            availablePowerUps.Add(rubberBulletsIndex);
        }
        if (blazingRamAvailable)
        {
            availablePowerUps.Add(blazingRamIndex);
        }
        if (beamCannonAvailable)
        {
            availablePowerUps.Add(beamCannonIndex);
        }
        if (bombsAvailable)
        {
            availablePowerUps.Add(bombsIndex);
        }
        #endregion       
    }
    #endregion

    #region OnEnable & OnDisable
    private void OnEnable()
    {
        em.OnPauseOn += OnPauseOn;
        em.OnPauseOff += OnPauseOff;
        em.OnMatchStarted += OnMatchStarted;
        em.OnMatchEnded += OnMatchEnded;
        em.OnGameRestart += OnGameRestart;
        em.OnNewSceneLoading += OnNewSceneLoading;
        SetPowerUpState(false);
    }

    private void OnDisable()
    {
        em.OnPauseOn -= OnPauseOn;
        em.OnPauseOff -= OnPauseOff;
        em.OnMatchStarted -= OnMatchStarted;
        em.OnMatchEnded -= OnMatchEnded;
        em.OnGameRestart -= OnGameRestart;
        em.OnNewSceneLoading -= OnNewSceneLoading;
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

    private void OnMatchStarted()
    {
        SetPowerUpState(true);
        matchStarted = true;
    }

    private void OnMatchEnded(int winnerIndex, float matchLength)
    {
        SetPowerUpState(false);
        matchStarted = false;
    }

    private void OnGameRestart()
    {
        matchStarted = false;
    }

    private void OnNewSceneLoading(int sceneIndex)
    {
        matchStarted = false;
    }
    #endregion

    #region FixedUpdate
    void FixedUpdate()
    {
        if (!isPaused)
        {
            if (matchStarted && !powerUpOnline)
            {
                powerUpCooldownTimer--;
                if (powerUpCooldownTimer <= 0)
                {
                    powerUpCooldownTimer = 0;
                    SetPowerUpState(true);
                }
            }

            //Manage tick rate
            if (powerUpOnline)
            {
                if (powerUpPlatformCollider.enabled == true)
                {
                    powerUpPlatformCollider.enabled = false;
                }

                powerUpPlatformColliderTickRateCounter--;
                if (powerUpPlatformColliderTickRateCounter <= 0)
                {
                    powerUpPlatformColliderTickRateCounter =
                        Mathf.RoundToInt(powerUpPlatformColliderTickInterval / Time.fixedDeltaTime);

                    powerUpPlatformCollider.enabled = true;
                }
            }
        }
    }
    #endregion

    #region Setters
    private void SetPowerUpState(bool state)
    {
        powerUpOnline = state;
        powerUpPlatformColliderTickRateCounter = 
            Mathf.RoundToInt(powerUpPlatformColliderTickInterval / Time.fixedDeltaTime);
        powerUpObject.SetActive(state);

        if (state)
        {
            powerUpType = availablePowerUps[Random.Range(0, availablePowerUps.Count)];

            if (powerUpType == rubberBulletsIndex)
            {
                powerUpDuration = lib.powerUpVariables.rubberBulletsDuration;
                powerUpProjectileType = lib.powerUpVariables.rubberBulletsProjectileType;
                powerUpIsPersistingProjectile = lib.powerUpVariables.rubberBulletsIsPersistingProjectileState;
                powerUpShipSpeedModifier = lib.powerUpVariables.rubberBulletsShipSpeedModifier;
                powerUpShipDamageTakenModifier = lib.powerUpVariables.rubberBulletsShipDamageTakenModifier;
                powerUpShootCooldownModifier = lib.powerUpVariables.rubberBulletsShootCooldownModifier;
                powerUpCanShootState = lib.powerUpVariables.rubberBulletsCanShootState;
                powerUpIsMovableState = lib.powerUpVariables.rubberBulletsIsMovableState;
                powerUpIsVulnerableState = lib.powerUpVariables.rubberBulletsIsVulnerableState;
            }
            else if (powerUpType == blazingRamIndex)
            {
                powerUpDuration = lib.powerUpVariables.blazingRamDuration;
                powerUpProjectileType = lib.powerUpVariables.blazingRamProjectileType;
                powerUpIsPersistingProjectile = lib.powerUpVariables.blazingRamIsPersistingProjectileState;
                powerUpShipSpeedModifier = lib.powerUpVariables.blazingRamShipSpeedModifier;
                powerUpShipDamageTakenModifier = lib.powerUpVariables.blazingRamShipDamageTakenModifier;
                powerUpShootCooldownModifier = lib.powerUpVariables.blazingRamShootCooldownModifier;
                powerUpCanShootState = lib.powerUpVariables.blazingRamCanShootState;
                powerUpIsMovableState = lib.powerUpVariables.blazingRamIsMovableState;
                powerUpIsVulnerableState = lib.powerUpVariables.blazingRamIsVulnerableState;
            }
            else if (powerUpType == beamCannonIndex)
            {
                powerUpDuration = lib.powerUpVariables.beamCannonDuration;
                powerUpProjectileType = lib.powerUpVariables.beamCannonProjectileType;
                powerUpIsPersistingProjectile = lib.powerUpVariables.beamCannonIsPersistingProjectileState;
                powerUpShipSpeedModifier = lib.powerUpVariables.beamCannonShipSpeedModifier;
                powerUpShipDamageTakenModifier = lib.powerUpVariables.beamCannonShipDamageTakenModifier;
                powerUpShootCooldownModifier = lib.powerUpVariables.beamCannonShootCooldownModifier;
                powerUpCanShootState = lib.powerUpVariables.beamCannonCanShootState;
                powerUpIsMovableState = lib.powerUpVariables.beamCannonIsMovableState;
                powerUpIsVulnerableState = lib.powerUpVariables.beamCannonIsVulnerableState;
            }
            else if (powerUpType == bombsIndex)
            {
                powerUpDuration = lib.powerUpVariables.bombsDuration;
                powerUpProjectileType = lib.powerUpVariables.bombsProjectileType;
                powerUpIsPersistingProjectile = lib.powerUpVariables.bombsIsPersistingProjectileState;
                powerUpShipSpeedModifier = lib.powerUpVariables.bombsShipSpeedModifier;
                powerUpShipDamageTakenModifier = lib.powerUpVariables.bombsShipDamageTakenModifier;
                powerUpShootCooldownModifier = lib.powerUpVariables.bombsShootCooldownModifier;
                powerUpCanShootState = lib.powerUpVariables.bombsCanShootState;
                powerUpIsMovableState = lib.powerUpVariables.bombsIsMovableState;
                powerUpIsVulnerableState = lib.powerUpVariables.bombsIsVulnerableState;
            }
            else
            {
                Debug.LogError("PowerUpController: Invalid powerUp type!");
            }

            em.BroadcastPowerUpOnline(powerUpPlatformIndex, powerUpType);
            powerUpPlatformEffect.Play();
        }
        else
        {
            powerUpPlatformEffect.Stop();
            powerUpPlatformCollider.enabled = false;
        }
    }

    public void SetPowerUpPlatformIndex(int newIndex)
    {
        powerUpPlatformIndex = newIndex;
    }
    #endregion

    #region Collision detection
    private void OnTriggerEnter(Collider collider)
    {
        if (powerUpOnline)
        {
            if (collider.gameObject.CompareTag("Ship"))
            {
                ShipController collidingShipController = collider.transform.GetComponentInParent<ShipController>();
                //Tell shipController which powerUp was received

                collidingShipController.SetPowerUpType(powerUpType, powerUpDuration, powerUpProjectileType, powerUpIsPersistingProjectile, powerUpShipSpeedModifier,
                    powerUpShipDamageTakenModifier, powerUpShootCooldownModifier, powerUpCanShootState, powerUpIsMovableState,
                    powerUpIsVulnerableState);

                int collidingShipIndex = collidingShipController.GetIndex();
                em.BroadcastPowerUpPickedUp(collidingShipIndex, powerUpPlatformIndex, powerUpType);

                powerUpPickupEffect.Play();
                powerUpCooldownTimer = Mathf.RoundToInt(powerUpCooldown / Time.fixedDeltaTime);
                SetPowerUpState(false);
            }
        }
    }
    #endregion
}

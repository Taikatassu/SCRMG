using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_PowerUpController : MonoBehaviour {

    #region References & variables
    Core_Toolbox toolbox;
    Core_EventManager em;
    Core_GlobalVariableLibrary lib;
    ParticleSystem powerUpPlatformEffect;
    ParticleSystem powerUpPickupEffect;
    GameObject powerUpObject;

    //Values coming from within the script
    List<int> availablePowerUps = new List<int>();
    int powerUpPlatformIndex = -1;
    int powerUpCooldownTimer = -1;
    int powerUpType = -1;
    int powerUpProjectileType = -1;
    float powerUpDuration = -1;
    float powerUpShipSpeedModifier = -1;
    float powerUpShipDamageTakenModifier = -1;
    float powerUpShootCooldownModifier = -1;
    bool powerUpIsPersistingProjectile = false;
    bool powerUpCanShootState = false;
    bool powerUpIsMovableState = false;
    bool powerUpIsVulnerableState = false;
    bool powerUpOnline = false;

    //Values coming from GlobalVariableLibrary
    float powerUpCooldown = -1;

    #region PowerUp indices & availability
    int rubberBulletsIndex = -1;
    int blazingRamIndex = -1;
    int beamCannonIndex = -1;
    int bombsIndex = -1;
    bool rubberBulletsAvailable = false;
    bool blazingRamAvailable = false;
    bool beamCannonAvailable = false;
    bool bombsAvailable = false;
    #endregion

    #region RubberBullets variables
    float rubberBulletsDuration = -1;
    int rubberBulletsProjectileType = -1;
    bool rubberBulletsIsPersistingProjectileState = false;
    float rubberBulletsShipSpeedModifier = -1;
    float rubberBulletsShipDamageTakenModifier = -1;
    float rubberBulletsShootCooldownModifier = -1;
    bool rubberBulletsCanShootState = false;
    bool rubberBulletsIsMovableState = false;
    bool rubberBulletsIsVulnerableState = false;
    #endregion

    #region BlazingRam variables
    float blazingRamDuration = -1;
    int blazingRamProjectileType = -1;
    bool blazingRamIsPersistingProjectileState = false;
    float blazingRamShipSpeedModifier = -1;
    float blazingRamShipDamageTakenModifier = -1;
    float blazingRamShootCooldownModifier = -1;
    bool blazingRamCanShootState = false;
    bool blazingRamIsMovableState = false;
    bool blazingRamIsVulnerableState = false;
    #endregion

    #region BeamCannon variables
    float beamCannonDuration = -1;
    int beamCannonProjectileType = -1;
    bool beamCannonIsPersistingProjectileState = false;
    float beamCannonShipSpeedModifier = -1;
    float beamCannonShipDamageTakenModifier = -1;
    float beamCannonShootCooldownModifier = -1;
    bool beamCannonCanShootState = false;
    bool beamCannonIsMovableState = false;
    bool beamCannonIsVulnerableState = false;
    #endregion

    #region Bombs variables
    float bombsDuration = -1;
    int bombsProjectileType = -1;
    bool bombsIsPersistingProjectileState = false;
    float bombsShipSpeedModifier = -1;
    float bombsShipDamageTakenModifier = -1;
    float bombsShootCooldownModifier = -1;
    bool bombsCanShootState = false;
    bool bombsIsMovableState = false;
    bool bombsIsVulnerableState = false;
    #endregion
    #endregion

    #region Awake & GetStats
    private void Awake ()
    {
        toolbox = FindObjectOfType<Core_Toolbox>();
        em = toolbox.GetComponent<Core_EventManager>();
        lib = toolbox.GetComponent<Core_GlobalVariableLibrary>();
        powerUpPlatformEffect = transform.GetChild(1).GetComponent<ParticleSystem>();
        powerUpPickupEffect = transform.GetChild(2).GetComponent<ParticleSystem>();
        powerUpObject = GetComponentInChildren<Core_PowerUpAnimator>().gameObject;

        GetStats();
    }

    private void GetStats()
    {
        powerUpCooldown = lib.powerUpVariables.powerUpCooldown;

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

        #region RubberBullets variables
        rubberBulletsDuration = lib.powerUpVariables.rubberBulletsDuration;
        rubberBulletsProjectileType = lib.powerUpVariables.rubberBulletsProjectileType;
        rubberBulletsIsPersistingProjectileState = lib.powerUpVariables.rubberBulletsIsPersistingProjectileState;
        rubberBulletsShipSpeedModifier = lib.powerUpVariables.rubberBulletsShipSpeedModifier;
        rubberBulletsShipDamageTakenModifier = lib.powerUpVariables.rubberBulletsShipDamageTakenModifier;
        rubberBulletsShootCooldownModifier = lib.powerUpVariables.rubberBulletsShootCooldownModifier;
        rubberBulletsCanShootState = lib.powerUpVariables.rubberBulletsCanShootState;
        rubberBulletsIsMovableState = lib.powerUpVariables.rubberBulletsIsMovableState;
        rubberBulletsIsVulnerableState = lib.powerUpVariables.rubberBulletsIsVulnerableState;
        #endregion

        #region BlazingRam variables
        blazingRamDuration = lib.powerUpVariables.blazingRamDuration;
        blazingRamProjectileType = lib.powerUpVariables.blazingRamProjectileType;
        blazingRamIsPersistingProjectileState = lib.powerUpVariables.blazingRamIsPersistingProjectileState;
        blazingRamShipSpeedModifier = lib.powerUpVariables.blazingRamShipSpeedModifier;
        blazingRamShipDamageTakenModifier = lib.powerUpVariables.blazingRamShipDamageTakenModifier;
        blazingRamShootCooldownModifier = lib.powerUpVariables.blazingRamShootCooldownModifier;
        blazingRamCanShootState = lib.powerUpVariables.blazingRamCanShootState;
        blazingRamIsMovableState = lib.powerUpVariables.blazingRamIsMovableState;
        blazingRamIsVulnerableState = lib.powerUpVariables.blazingRamIsVulnerableState;
        #endregion

        #region BeamCannon variables
        beamCannonDuration = lib.powerUpVariables.beamCannonDuration;
        beamCannonProjectileType = lib.powerUpVariables.beamCannonProjectileType;
        beamCannonIsPersistingProjectileState = lib.powerUpVariables.beamCannonIsPersistingProjectileState;
        beamCannonShipSpeedModifier = lib.powerUpVariables.beamCannonShipSpeedModifier;
        beamCannonShipDamageTakenModifier = lib.powerUpVariables.beamCannonShipDamageTakenModifier;
        beamCannonShootCooldownModifier = lib.powerUpVariables.beamCannonShootCooldownModifier;
        beamCannonCanShootState = lib.powerUpVariables.beamCannonCanShootState;
        beamCannonIsMovableState = lib.powerUpVariables.beamCannonIsMovableState;
        beamCannonIsVulnerableState = lib.powerUpVariables.beamCannonIsVulnerableState;
        #endregion

        #region Bombs variables
        bombsDuration = lib.powerUpVariables.bombsDuration;
        bombsProjectileType = lib.powerUpVariables.bombsProjectileType;
        bombsIsPersistingProjectileState = lib.powerUpVariables.bombsIsPersistingProjectileState;
        bombsShipSpeedModifier = lib.powerUpVariables.bombsShipSpeedModifier;
        bombsShipDamageTakenModifier = lib.powerUpVariables.bombsShipDamageTakenModifier;
        bombsShootCooldownModifier = lib.powerUpVariables.bombsShootCooldownModifier;
        bombsCanShootState = lib.powerUpVariables.bombsCanShootState;
        bombsIsMovableState = lib.powerUpVariables.bombsIsMovableState;
        bombsIsVulnerableState = lib.powerUpVariables.bombsIsVulnerableState;
        #endregion
    }
    #endregion

    #region OnEnable & OnDisable
    private void OnEnable()
    {
        em.OnMatchStarted += OnMatchStarted;
        em.OnMatchEnded += OnMatchEnded;
        SetPowerUpState(false);
    }

    private void OnDisable()
    {
        em.OnMatchStarted -= OnMatchStarted;
        em.OnMatchEnded -= OnMatchEnded;
    }
    #endregion

    #region Subscribers
    private void OnMatchStarted()
    {
        SetPowerUpState(true);
    }
    private void OnMatchEnded(int winnerIndex)
    {
        SetPowerUpState(false);
    }
    #endregion

    void FixedUpdate ()
    {
		if (!powerUpOnline)
        {
            powerUpCooldownTimer--;
            if (powerUpCooldownTimer <= 0)
            {
                powerUpCooldownTimer = 0;
                SetPowerUpState(true);
            }
        }
	}

    private void SetPowerUpState(bool state)
    {
        powerUpOnline = state;
        powerUpObject.SetActive(state);

        if (state)
        {
            powerUpType = availablePowerUps[Random.Range(0, availablePowerUps.Count)];

            if (powerUpType == rubberBulletsIndex)
            {
                powerUpDuration = rubberBulletsDuration;
                powerUpProjectileType = rubberBulletsProjectileType;
                powerUpIsPersistingProjectile = rubberBulletsIsPersistingProjectileState;
                powerUpShipSpeedModifier = rubberBulletsShipSpeedModifier;
                powerUpShipDamageTakenModifier = rubberBulletsShipDamageTakenModifier;
                powerUpShootCooldownModifier = rubberBulletsShootCooldownModifier;
                powerUpCanShootState = rubberBulletsCanShootState;
                powerUpIsMovableState = rubberBulletsIsMovableState;
                powerUpIsVulnerableState = rubberBulletsIsVulnerableState;
            }
            else if (powerUpType == blazingRamIndex)
            {
                powerUpDuration = blazingRamDuration;
                powerUpProjectileType = blazingRamProjectileType;
                powerUpIsPersistingProjectile = blazingRamIsPersistingProjectileState;
                powerUpShipSpeedModifier = blazingRamShipSpeedModifier;
                powerUpShipDamageTakenModifier = blazingRamShipDamageTakenModifier;
                powerUpShootCooldownModifier = blazingRamShootCooldownModifier;
                powerUpCanShootState = blazingRamCanShootState;
                powerUpIsMovableState = blazingRamIsMovableState;
                powerUpIsVulnerableState = blazingRamIsVulnerableState;
            }
            else if (powerUpType == beamCannonIndex)
            {
                powerUpDuration = beamCannonDuration;
                powerUpProjectileType = beamCannonProjectileType;
                powerUpIsPersistingProjectile = beamCannonIsPersistingProjectileState;
                powerUpShipSpeedModifier = beamCannonShipSpeedModifier;
                powerUpShipDamageTakenModifier = beamCannonShipDamageTakenModifier;
                powerUpShootCooldownModifier = beamCannonShootCooldownModifier;
                powerUpCanShootState = beamCannonCanShootState;
                powerUpIsMovableState = beamCannonIsMovableState;
                powerUpIsVulnerableState = beamCannonIsVulnerableState;
            }
            else if (powerUpType == bombsIndex)
            {
                powerUpDuration = bombsDuration;
                powerUpProjectileType = bombsProjectileType;
                powerUpIsPersistingProjectile = bombsIsPersistingProjectileState;
                powerUpShipSpeedModifier = bombsShipSpeedModifier;
                powerUpShipDamageTakenModifier = bombsShipDamageTakenModifier;
                powerUpShootCooldownModifier = bombsShootCooldownModifier;
                powerUpCanShootState = bombsCanShootState;
                powerUpIsMovableState = bombsIsMovableState;
                powerUpIsVulnerableState = bombsIsVulnerableState;
            }

            em.BroadcastPowerUpOnline(powerUpPlatformIndex, powerUpType);
            powerUpPlatformEffect.Play();
        }
        else
        {
            powerUpPlatformEffect.Stop();
        }
    }

    public void SetPowerUpPlatformIndex(int newIndex)
    {
        powerUpPlatformIndex = newIndex;
        Debug.Log("My powerUpPlatformIndex: " + powerUpPlatformIndex);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (powerUpOnline)
        {
            if (collider.gameObject.CompareTag("Ship"))
            {
                Core_ShipController collidingShipController = collider.transform.GetComponentInParent<Core_ShipController>();
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
}

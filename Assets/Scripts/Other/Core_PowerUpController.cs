using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_PowerUpController : MonoBehaviour {

    #region References & variables
    Core_Toolbox toolbox;
    Core_EventManager em;
    Core_GlobalVariableLibrary lib;
    ParticleSystem powerUpBaseEffect;
    ParticleSystem powerUpPickupEffect;
    GameObject powerUpObject;
    //Values coming from within the script
    List<int> availablePowerUps = new List<int>();
    int powerUpCooldownTimer = -1;
    float powerUpDuration = -1;
    bool powerUpOnline = false;
    //Values coming from GlobalVariableLibrary
    int powerUpBaseIndex = -1;
    int rubberBulletsIndex = -1;
    int blazingRamIndex = -1;
    int beamCannonIndex = -1;
    int bombsIndex = -1;
    int powerUpType = -1;
    float powerUpCooldown = -1;
    float rubberBulletsDuration = -1;
    float blazingRamDuration = -1;
    float beamCannonDuration = -1;
    float bombsDuration = -1;
    bool rubberBulletsAvailable = false;
    bool blazingRamAvailable = false;
    bool beamCannonAvailable = false;
    bool bombsAvailable = false;
    #endregion

    #region Awake & GetStats
    private void Awake ()
    {
        toolbox = FindObjectOfType<Core_Toolbox>();
        em = toolbox.GetComponent<Core_EventManager>();
        lib = toolbox.GetComponent<Core_GlobalVariableLibrary>();
        powerUpBaseEffect = transform.GetChild(1).GetComponent<ParticleSystem>();
        powerUpPickupEffect = transform.GetChild(2).GetComponent<ParticleSystem>();
        powerUpObject = GetComponentInChildren<Core_PowerUpAnimator>().gameObject;

        GetStats();
    }

    private void GetStats()
    {
        powerUpCooldown = lib.shipVariables.powerUpCooldown;
        rubberBulletsIndex = lib.shipVariables.rubberBulletsIndex;
        blazingRamIndex = lib.shipVariables.blazingRamIndex;
        beamCannonIndex = lib.shipVariables.beamCannonIndex;
        bombsIndex = lib.shipVariables.bombsIndex;
        rubberBulletsAvailable = lib.shipVariables.rubberBulletsAvailable;
        blazingRamAvailable = lib.shipVariables.blazingRamAvailable;
        beamCannonAvailable = lib.shipVariables.beamCannonAvailable;
        bombsAvailable = lib.shipVariables.bombsAvailable;
        rubberBulletsDuration = lib.shipVariables.rubberBulletsDuration;
        blazingRamDuration = lib.shipVariables.blazingRamDuration;
        beamCannonDuration = lib.shipVariables.beamCannonDuration;
        bombsDuration = lib.shipVariables.bombsDuration;

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

            if (powerUpType == 1)
            {
                powerUpDuration = rubberBulletsDuration;
            }
            else if (powerUpType == 2)
            {
                powerUpDuration = blazingRamDuration;
            }
            else if (powerUpType == 3)
            {
                powerUpDuration = beamCannonDuration;
            }
            else if (powerUpType == 4)
            {
                powerUpDuration = bombsDuration;
            }


            em.BroadcastPowerUpOnline(powerUpBaseIndex, powerUpType);
            powerUpBaseEffect.Play();
        }
        else
        {
            powerUpBaseEffect.Stop();
        }
    }

    public void SetPowerUpBaseIndex(int newIndex)
    {
        powerUpBaseIndex = newIndex;
        Debug.Log("My powerUpBaseIndex: " + powerUpBaseIndex);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (powerUpOnline)
        {
            if (collider.gameObject.CompareTag("Ship"))
            {
                Core_ShipController collidingShipController = collider.transform.GetComponentInParent<Core_ShipController>();
                //Tell shipController which powerUp was received
                collidingShipController.SetPowerUpType(powerUpType, powerUpDuration);
                int collidingShipIndex = collidingShipController.GetIndex();
                em.BroadcastPowerUpPickedUp(collidingShipIndex, powerUpBaseIndex, powerUpType);

                powerUpPickupEffect.Play();
                powerUpCooldownTimer = Mathf.RoundToInt(powerUpCooldown / Time.fixedDeltaTime);
                SetPowerUpState(false);
            }
        }
    }
}

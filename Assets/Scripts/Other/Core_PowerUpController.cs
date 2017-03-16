using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_PowerUpController : MonoBehaviour {

    ParticleSystem powerUpBaseEffect;
    ParticleSystem powerUpPickupEffect;
    GameObject powerUpObject;

    float powerUpCooldown = 5;
    int powerUpCooldownTimer = 0;
    int powerUpType = -1;
    int powerUpBaseIndex = -1;
    bool powerUpOnline = false;

	void Awake () {
        powerUpBaseEffect = transform.GetChild(1).GetComponent<ParticleSystem>();
        powerUpPickupEffect = transform.GetChild(2).GetComponent<ParticleSystem>();
        powerUpObject = GetComponentInChildren<Core_PowerUpAnimator>().gameObject;
    }

    void OnEnable()
    {
        SetPowerUpState(true);
    }

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

            //if (!powerUpPickupEffect.isPlaying)
            //{
            //    powerUpPickupEffect.Stop();
            //}
        }
	}

    private void SetPowerUpState(bool state)
    {
        powerUpOnline = state;
        powerUpObject.SetActive(state);

        if (state)
        {
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
                Debug.Log("Ship entered trigger, powerUp type: " + powerUpType);
                Core_ShipController collidingShipController = collider.transform.GetComponentInParent<Core_ShipController>();
                //Tell shipController which powerUp was received

                powerUpPickupEffect.Play();
                powerUpCooldownTimer = Mathf.RoundToInt(powerUpCooldown / Time.fixedDeltaTime);
                SetPowerUpState(false);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    public PlayerData(int newPlayerShipIndex, int newMatchID)
    {
        playerShipIndex = newPlayerShipIndex;
        matchID = newMatchID;
        lifetime = -1;

        projectileTypeZeroSpawns = 0;
        projectileTypeOneSpawns = 0;
        projectileTypeTwopawns = 0;
        projectileTypeThreeSpawns = 0;

        projectileTypeZeroHits = 0;
        projectileTypeOneHits = 0;
        projectileTypeTwoHits = 0;
        projectileTypeThreeHits = 0;

        timesPickedUpPowerUpOne = 0;
        timesPickedUpPowerUpTwo = 0;
        timesPickedUpPowerUpThree = 0;
    }

    public int playerShipIndex = -1;
    public int matchID = -1;
    public float lifetime = -1;
    public bool victory = false;

    public List<ProjectileInfo> spawnedProjectiles = new List<ProjectileInfo>();

    public int projectileTypeZeroSpawns = -1;
    public int projectileTypeOneSpawns = -1;
    public int projectileTypeTwopawns = -1;
    public int projectileTypeThreeSpawns = -1;

    public int projectileTypeZeroHits = -1;
    public int projectileTypeOneHits = -1;
    public int projectileTypeTwoHits = -1;
    public int projectileTypeThreeHits = -1;

    public int timesPickedUpPowerUpOne = -1;
    public int timesPickedUpPowerUpTwo = -1;
    public int timesPickedUpPowerUpThree = -1;

}

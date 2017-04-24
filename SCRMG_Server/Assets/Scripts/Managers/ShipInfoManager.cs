using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipInfoManager : MonoBehaviour {

    public List<ShipInfo> shipInfoList = new List<ShipInfo>();

    //void FixedUpdate()
    //{
    //    foreach (ShipInfo shipInfo in shipInfoList)
    //    {
    //        if(shipInfo.shipIndex == 1)
    //        {
    //            Debug.Log("ShipInfoManager shipInfo with index 1: shipInfo.shipPosition: " + shipInfo.shipPosition);
    //        }
    //    }
    //}

    public int GetMyShipInfoElement(int shipIndex)
    {
        for(int i = 0; i < shipInfoList.Count; i++)
        {
            if (shipInfoList[i].shipIndex == shipIndex)
            {
                return i;
            }
        }
        return -1;
    }

    public void ClearShipInfoList()
    {
        shipInfoList.Clear();
    }
}

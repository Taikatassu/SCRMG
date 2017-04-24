using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipInfoManager : MonoBehaviour {

    public List<ShipInfo> shipInfoList = new List<ShipInfo>();

    public int shipInfoListCount = -1;
    private void FixedUpdate()
    {
        shipInfoListCount = shipInfoList.Count;
    }

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
        shipInfoList = new List<ShipInfo>();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_Toolbox : MonoBehaviour
{
    static Core_Toolbox instance;

    void Awake()
    {
        #region Singletonization
        if (instance == null)
        {
            instance = this;
        }

        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        #endregion
    }
}

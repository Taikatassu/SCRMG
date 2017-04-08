using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVariableLibrary : MonoBehaviour {

    public static GlobalVariableLibrary instance;

    private void Awake()
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

    public ServerVariables serverVariables;

    public GlobalVariableLibrary()
    {
        serverVariables = new ServerVariables();
    }

    [System.Serializable]
    public class ServerVariables
    {

    }
}

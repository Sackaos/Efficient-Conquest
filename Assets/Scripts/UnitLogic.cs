using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitLogic : MonoBehaviour
{
    public static UnitLogic LogicSingleton;
    private void Awake()
    {
        if (!LogicSingleton)
        {
            LogicSingleton = this;
            return;
        }
        else
        {
            Debug.LogError("BAUBAU! UNIT LOGIC not SINGLETON");
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

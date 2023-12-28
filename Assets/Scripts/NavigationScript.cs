using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavigationScript : MonoBehaviour
{
    private UnitSelector unitSelector;
    private NavMeshAgent agent;
    void Start()
    {
        foreach (var key in unitSelector.selectedTable.Keys)
        {
            agent = GetComponent<NavMeshAgent>();

        }
    }
    void Update()
    {
       
    }
}

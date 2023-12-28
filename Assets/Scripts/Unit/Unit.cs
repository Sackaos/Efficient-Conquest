using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    //[RequireComponent()]
    UnitStats stats;
    public int OwnerID = 0;
    UnitSelector USArmy = UnitSelector.SelectorSingleton;
    NavMeshAgent agent;
    
    void Start()
    {
        if (!stats.IsBuilding)agent=GetComponent<NavMeshAgent>();
    }
    public void TakeDamage(int health)
    {

    }
    public void Move(Vector3 position)
    {
        
            agent.destination = position;
    }
    public void Destroy()
    {

    }
    
}

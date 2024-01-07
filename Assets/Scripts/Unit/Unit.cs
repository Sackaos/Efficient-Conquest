using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    //RequireComponent(UnitStats)]
    public UnitStats stats;
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
    /// <summary>
    /// this is le Summary
    /// </summary>
    /// <param name="position">the desired position to move to.</param>
    /// <returns>returns true if move succeeded.</returns>
    public bool Move(Vector3 position)
    {

        if (agent) { agent.destination = position; return true; }
        else return false;
    }
    public void Destroy()
    {

    }
    
}

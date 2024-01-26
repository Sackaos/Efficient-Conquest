//using SerializableCallback;
using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    //RequireComponent(UnitStats)]
    public UnitStats stats;
    public ulong OwnerID = 0;
    //UnitSelector USArmy = UnitSelector.SelectorSingleton;
    NavMeshAgent agent;
    
    //OwnerID = id;
    void Start()
    {
        if (stats && !stats.IsBuilding) agent = GetComponent<NavMeshAgent>();
    }
    public void TakeDamage(int health)
    {

    }
    /// <summary>
    /// this is le Summary
    /// </summary>
    /// <param name="position">the desired position to move to.</param>
    /// <returns>returns true if move succeeded.</returns>
    public void Destroy()
    {

    }
    public void MoveTo(Vector3 position)
    {
        agent.destination = position;
    }
    public bool CanMove(Vector3 position)
    {
        if (!CanDoAction(Actions.Move) || !agent || OwnerID!=UnitSelector.ID) return false;
        return true;
    }

    public void Follow(Transform transfrom)
    {
        //move to follow to move;
    }
    public void Attack(Transform enemy)
    {
        if (CanAttack(enemy.GetComponent<Unit>()) && agent)
        {
            agent.destination = enemy.position;
            //doAttackStuff
        }
    }
    public bool CanAttack(Unit unit)
    {
        //if(!isAlly&&CanAttackAirIfAir)
        if (OwnerID != unit.OwnerID)
            return true;
        return false;
    }
    public bool CanDoAction(Actions actionToCheck)
    {
        bool flag = false;
        foreach (ActionSet set in stats.Actions)
        {
            if (set.action == actionToCheck) { flag = true; break; }
        }
        return flag;

    }
    public bool DoAction(Actions actionToPerform)
    {
        bool flag = CanDoAction(actionToPerform);
        if (flag)
            switch (actionToPerform)
            {
                case (Actions.Move):
                    CanMove(new Vector3(5, 0, 8));
                    break;
                case (Actions.SpawnFunkyStuff):
                    //spawn Funky stuff
                    break;
                default:
                    Debug.LogError("WTF");
                    break;
            }

        return true;
    }
    [Serializable]
    public enum Actions
    {
        Empty,
        Move,
        Attack,
        Build,
        SpawnFunkyStuff
    }
}

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
    public Transform TargetUnit;
    public Unit.Actions CurrentAction;

    //OwnerID = id;
    void Start()
    {
        if(stats && !stats.IsBuilding)
            agent = GetComponent<NavMeshAgent>();
    }
    private void FixedUpdate()
    {
        /*
          if (following&&UnitSelector.SelectorSingleton.SuperSelected.TargetUnit != null && (UnitSelector.SelectorSingleton.SuperSelected.CurrentAction == Unit.Actions.Move))
        {
            GameObject[] selectedGameObjects = UnitSelector.SelectorSingleton.selectedTable.Values.ToArray<GameObject>();
            NetworkObjectReference[] SelectedReferenceArr = new NetworkObjectReference[selectedGameObjects.Length];
            for (int i = 0; i < SelectedReferenceArr.Length; i++)
            {
                SelectedReferenceArr[i] = selectedGameObjects[i];
            }
            MouveServerRpc(transform.position, SelectedReferenceArr);
        }*/
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
        CurrentAction = Unit.Actions.Move;
    }
    public bool CanMove()
    {
        if(!CanDoAction(Actions.Move) || !agent || OwnerID != GameCoordinator.OwnerId)
            return false;
        return true;
    }

    public void Follow(Transform transfrom)
    {
        TargetUnit = transfrom;
        CurrentAction = Unit.Actions.Move;

        //move to follow to move;
    }
    public void Attack(Transform enemy)
    {
        if(CanAttack(enemy.GetComponent<Unit>()) && agent) {
            agent.destination = enemy.position;
            //doAttackStuff
        }
    }
    public bool CanAttack(Unit target)
    {
        //if(!isAlly&&InRange&&CanAttackAirIfAir)
        if(OwnerID != target.OwnerID && Vector2.Distance(new Vector2(transform.position.x, transform.position.y), new Vector2(target.transform.position.x, target.transform.position.y);
        return true;
        return false;
    }
    public bool CanDoAction(Actions actionToCheck)
    {
        bool flag = false;
        foreach(ActionSet set in stats.Actions) {
            if(set.action == actionToCheck) { flag = true; break; }
        }
        return flag;

    }
    public bool DoAction(Actions actionToPerform)
    {
        bool flag = CanDoAction(actionToPerform);
        if(flag)
            switch(actionToPerform) {
                case (Actions.Move):
                    CanMove();
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
        Mine,
        Attack,
        Build,
        SpawnFunkyStuff,
        Idle
    }
}

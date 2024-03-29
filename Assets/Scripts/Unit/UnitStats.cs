using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Scriptable object/Inventory item")]
public class UnitStats : ScriptableObject
{
    public string Name;
    [TextArea(4, 4)]
    public string Description;
    public bool IsBuilding;
    public float MoveSpeed;
    public int Health;
    public int Shield;
    public int Damage;
    public float AttackSpeed;
    public float AttackRange;

    public int Resource1Cost;
    public int Resource2Cost;
    public Technologies[] TechnologyRequirements;
    public ActionSet[] Actions;

    public GameObject prefab;
    public Sprite sprite;
    //sounds

}
public enum Technologies
{
    house,
    barracks,
    Garage,
    AeroPort,
    LaserWeapons,
    TankUnlock
}



[Serializable]
public struct ActionSet
{
    public Unit.Actions action;
    public int id;
  
}
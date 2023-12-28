using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Scriptable object/Inventory item")]
public class UnitStats : ScriptableObject
{
    public Sprite Icon;
    public string Name;
    [TextArea(4, 4)]
    public string Description;
    public int Health;
    public int Shield;
    public int Damage;
    public float AttackSpeed;
    public float MoveSpeed;
    public bool IsBuilding;
     
    


}

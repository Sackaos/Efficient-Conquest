using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

public class GameCoordinator : NetworkBehaviour
{
    public static GameCoordinator GameCoordinatorSingleton { get; private set; }


    private void Awake()
    {

        if (!GameCoordinatorSingleton)
        {
            GameCoordinatorSingleton = this;
        }
        else
        {
            Debug.LogError("BAUBAU! GAMECOORDINATOR not SINGLETON");
            Destroy(this);
        }
    }
 


    [ServerRpc(RequireOwnership = false)]
    public void MouveServerRpc(Vector3 shmovement, NetworkObjectReference[] unitRefs, ServerRpcParams rpc = default)
    {
        if (!IsOwner) return;
        ulong clientId = rpc.Receive.SenderClientId;
        foreach (var unitref in unitRefs)
        {
            unitref.TryGet(out NetworkObject networkObject);
            var unit = networkObject.GetComponent<Unit>();
            if (unit.OwnerID == clientId)
            {
                unit.MoveTo(shmovement);
                //OwnerClientId;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AttackServerRpc(NetworkObjectReference enemyRef, NetworkObjectReference[] unitRefs, ServerRpcParams rpc = default)
    {
        if (!IsOwner) return;
        ulong clientId = rpc.Receive.SenderClientId;
        foreach (var unitref in unitRefs)
        {
            unitref.TryGet(out NetworkObject networkUnit);
            var unit = networkUnit.GetComponent<Unit>();
            if (unit.OwnerID == clientId)
            {
                enemyRef.TryGet(out NetworkObject networkEnemy);
                unit.Follow(networkEnemy.transform);
                //OwnerClientId;
            }
        }
    }

    public GameObject booboabas;
    public void SpawnUnit(UnitStats unit, Transform parentTransform)
    {
        //    Instantiate(unit.Graphics, parent.position, parent.rotation
        GameObject spawned = Instantiate(unit.Graphics, parentTransform.position, parentTransform.rotation);
        spawned.GetComponent<NetworkObject>().Spawn();
        spawned.GetComponent<Unit>().OwnerID = UnitSelector.ID;
        //Instantiate(unit.Graphics, Vector3.zero,Quaternion.identity);

    }

}
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnitLogic : MonoBehaviour
{
    public static UnitLogic LogicSingleton;
    private void Awake()
    {
        if (!LogicSingleton&&false)
        {
            LogicSingleton = this;
            return;
        }
        else
        {
            Debug.LogError("BAUBAU! UNIT LOGIC not SINGLETON");
            //Destroy(this);
        }
    }

    public void MouveServerRpc(Vector3 shmovement, GameObject[] units, ServerRpcParams rpc = default)
    {

        


        ulong clientId = rpc.Receive.SenderClientId;
        foreach (GameObject unitGO in units)
        {
            var unit = unitGO.GetComponent<Unit>();
            unit.OwnerID = clientId;
            if (unit.OwnerID == clientId)
            {
                unit.MoveTo(shmovement);
            }
            else Debug.LogError("yowtf bro ur cheating UnitSelector/MoveServerRpc");
        }
        /*if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            NetworkClient client = NetworkManager.Singleton.ConnectedClients[clientId];
            client.PlayerObject.GetComponent<CharacterController>().Move(new Vector3(0, 1f, 0));
            client.PlayerObject.transform.position = new Vector3(0, 1, 0);
        }*/

    }
}

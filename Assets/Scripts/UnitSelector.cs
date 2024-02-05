using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Unity.Netcode;
//using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.InputSystem;
using UnityEngine.Timeline;

public class UnitSelector : MonoBehaviour
{
    //NEED TO IMPLEMENT GROUPS(1-9 gets units)
    /**
    * the halo game object  must be located at this hard coded child index
    */

    public static UnitSelector SelectorSingleton;

    public Dictionary<int, GameObject> selectedTable { get; private set; } = new Dictionary<int, GameObject>();
    public Dictionary<int, GameObject>[] controlGroups { get; private set; } = new Dictionary<int, GameObject>[10];

    public Unit SuperSelected { get; private set; }
    public Vector3 startMousePos = Vector3.zero;
    public LayerMask unitLayer;
    public LayerMask groundLayer;
    private static int HALO_CHILD_INDEX = 0;
    public bool dragSelect = false;


    private void Awake()
    {
        if(!SelectorSingleton) {
            SelectorSingleton = this;
            return;
        } else {
            Debug.LogError("BAUBAU! UNIT SELECTOR not SINGLETON");
            Destroy(this);

        }
        unitLayer = LayerMask.GetMask("Unit");
        groundLayer = LayerMask.GetMask("Ground");
    }

    /* public override void OnNetworkSpawn()
    {
        if (!IsOwner) Destroy(this);
    } */

    //##########      SELECTION     ########
    public void addSelected(GameObject gameObject)
    {
        selectionVisualizer(gameObject);
        int id = gameObject.GetInstanceID();
        if(!selectedTable.ContainsKey(id)) {
            selectedTable.Add(id, gameObject);
        } else {
            deselect(selectedTable, gameObject);
        }

        if(selectedTable.Count > 0) {
            SuperSelected = selectedTable.Values.ToArray<GameObject>()[0].GetComponent<Unit>();
            UnitHud.UnitHudSingleton.DisplayUnit(SuperSelected);
        }
    }
    public void deselect(Dictionary<int, GameObject> table,GameObject gameObject)
    {
        undoVisualization(gameObject);
        selectedTable.Remove(gameObject.GetInstanceID());
        if(selectedTable.Count == 0) { UnitHud.UnitHudSingleton.DisplayUnit(null); }

    }
    public void deselectAllFromTable(Dictionary<int,GameObject> table)
    {
        foreach(GameObject go in table.Values.ToList<GameObject>()) {
            deselect(table,go);
        }
    }

    public void AddSelectionToControlGroup(Dictionary<int, GameObject> controlGroup)
    {
        foreach(GameObject go in selectedTable.Values.ToList<GameObject>()) {
            controlGroup.Add(go.GetInstanceID(),go);
        }
    }
    internal NetworkObjectReference[] newFunc()
    {
        GameObject[] selectedGameObjects = selectedTable.Values.ToArray<GameObject>();
        NetworkObjectReference[] SelectedReferenceArr = new NetworkObjectReference[selectedGameObjects.Length];
        for(int i = 0; i < SelectedReferenceArr.Length; i++) {
            SelectedReferenceArr[i] = selectedGameObjects[i];
        }
        return SelectedReferenceArr;
    }

    private void selectionVisualizer(GameObject gameObject)
    {
        gameObject.transform.GetChild(HALO_CHILD_INDEX).gameObject.SetActive(true);
        //gameObject.transform.GetChild(HALO_CHILD_INDEX).gameObject.transform.position.y=GetComponent<Renderer>().bounds.size
        gameObject.GetComponent<Renderer>().material.color = Color.red;

    }
    private void undoVisualization(GameObject gameObject)
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        gameObject.GetComponent<Renderer>().material.color = Color.white;

    }
    public void selectUnitUnderMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(startMousePos);
        if(Physics.Raycast(ray, out RaycastHit hit, 5000f, unitLayer)) {
            addSelected(hit.transform.gameObject);
        }
    }
    public void boxSelect(Vector3 startMousePos, Vector3 endMousePos)
    {
        Vector3 startBoxPos = Vector3.zero, endBoxPos = Vector3.zero;
        if(Physics.Raycast(
            Camera.main.ScreenPointToRay(startMousePos),
            out RaycastHit hitter, 5000, groundLayer)) {
            startBoxPos = hitter.point;
        }
        if(Physics.Raycast(
            Camera.main.ScreenPointToRay(endMousePos)
            , out hitter, 5000, groundLayer)) {
            endBoxPos = hitter.point;
            Vector3 center = (startBoxPos + endBoxPos) / 2;
            Vector3 extentsosia = (endBoxPos - startBoxPos) / 2;
            Vector3 extents = new Vector3(Mathf.Abs(extentsosia.x), Mathf.Abs(extentsosia.y), Mathf.Abs(extentsosia.z));
            extents.y = 20;
            Collider[] colliders = Physics.OverlapBox(center, extents, Quaternion.Euler(Camera.main.transform.forward), unitLayer);

            foreach(var collider in colliders) {
                //if(Own(unit))
                addSelected(collider.gameObject);
            }


        }
    }


    //##########      MARKERS     ########
    [SerializeField] GameObject markerPrefab;
    List<GameObject> markers = new List<GameObject>();
    public void SpawnMarker(Vector3 point)
    {
        markers.Add(Instantiate(markerPrefab, point, Quaternion.identity));
        Invoke("DestroyLatestMarker", 0.75f);
    }
    public void DestroyLatestMarker()
    {
        GameObject marker = markers.First();
        markers.Remove(marker);
        Destroy(marker);
    }

    //##########      GUI     ########
    private void OnGUI()
    {
        DrawMarquee();

    }
    private void DrawMarquee()
    {
        if(!dragSelect)
            return;

        var rect = SelectorGUI.GetScreenRect(startMousePos, Input.mousePosition);
        SelectorGUI.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.15f));
        SelectorGUI.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));


    }


    /*[ServerRpc(RequireOwnership = false)]
    public void MoveServerRpc(Vector3 shmovement, GameObject[] units, ServerRpcParams rpc = default)
    {

        // Debug.Log("id sent: "+id+" owner: "+ OwnerClientId);
         GameObject[] a = GameObject.FindGameObjectsWithTag("Player");
         foreach (GameObject GO in a)
         {
             if (GO.GetInstanceID() == id) {
                 
                 //GO.active = false;
                 
             }
             if (GO.GetComponent<NetworkBehaviour>().OwnerClientId == OwnerClientId)
             
             if (GO.GetComponent<NetworkBehaviour>().OwnerClientId == rpc.Receive.SenderClientId) { }
        

        ulong clientId = rpc.Receive.SenderClientId;
        
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            NetworkClient client = NetworkManager.Singleton.ConnectedClients[clientId];
            client.PlayerObject.GetComponent<CharacterController>().Move(new Vector3(0, 1f, 0));
            client.PlayerObject.transform.position = new Vector3(0, 1, 0);
        }

    }*/

    /*[ServerRpc(RequireOwnership = false)]
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
    }*/

}

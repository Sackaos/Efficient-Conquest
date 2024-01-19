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
    
    //public static UnitSelector SelectorSingleton;
    private void Awake()
    {
        /*if (!SelectorSingleton)
        {
            SelectorSingleton = this;
            return;
        }
        else
        {
            Debug.LogError("BAUBAU! UNIT SELECTOR not SINGLETON");
            Destroy(this);
        }*/
    }
    /*public override void OnNetworkSpawn()
    {
        if (!IsOwner) Destroy(this);
    }*/
    public Dictionary<int, GameObject> selectedTable { get; private set; } = new Dictionary<int, GameObject>();
    public void addSelected(GameObject gameObject)
    {
        selectionVisualizer(gameObject);
        int id = gameObject.GetInstanceID();
        if (!selectedTable.ContainsKey(id))
        {
            selectedTable.Add(id, gameObject);
        }
        else
        {
            deselect(gameObject);
        }

    }
    public void deselect(GameObject gameObject)
    {
        undoVisualization(gameObject);
        selectedTable.Remove(gameObject.GetInstanceID());

    }
    public void deselectAll()
    {
        foreach (GameObject go in selectedTable.Values.ToList<GameObject>())
        {
            deselect(go);
        }
    }

    private void selectionVisualizer(GameObject gameObject)
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(true);

        //gameObject.transform.GetChild(0).gameObject.transform.position.y=GetComponent<Renderer>().bounds.size


        gameObject.GetComponent<Renderer>().material.color = Color.red;

    }
    private void undoVisualization(GameObject gameObject)
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        gameObject.GetComponent<Renderer>().material.color = Color.white;

    }

    RaycastHit hit;
    bool dragSelect = false;

    public LayerMask unitLayer;
    [SerializeField] public LayerMask groundLayer;

    Vector3 startMousePos = Vector3.zero, endMousePos = Vector3.zero;
    private void Update()
    {

        LeftClick();
        RightClick();


    }

    public GameObject goog;
    private void RightClick()
    {
        //Debug.Log(selectedTable.Count);
        if (Input.GetMouseButtonDown(1) && selectedTable.Count > 0)
        {
            var cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(cameraRay.origin, cameraRay.direction, out RaycastHit rayHit, 5000, groundLayer))
            {
                foreach (GameObject go in selectedTable.Values.ToList<GameObject>())
                {
                    if (!go.GetComponent<Unit>().CanMove(rayHit.point))
                    {
                        deselect(go);
                    }
                }
                goog.GetComponent<UnitLogic>().MouveServerRpc(rayHit.point, selectedTable.Values.ToArray<GameObject>());
                //MoveServerRpc(rayHit.point, selectedTable.Values.ToArray<GameObject>());
            }
            spawnMarker(rayHit.point);
        }
    }

    private void WhatTypeIsValues(Dictionary<int, GameObject>.ValueCollection values)
    {
        throw new NotImplementedException();
    }

    private void LeftClick()
    {
        //Left click
        if (Input.GetMouseButtonDown(0))
        {
            startMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButton(0))
        {
            if ((startMousePos - Input.mousePosition).magnitude > 40)
            {
                dragSelect = true;
            }
            else dragSelect = false;
        }
        if (Input.GetMouseButtonUp(0))
        {

            endMousePos = Input.mousePosition;
            if (!dragSelect)//single select
            {
                Ray ray = Camera.main.ScreenPointToRay(startMousePos);
                if (Physics.Raycast(ray, out hit, 5000f, unitLayer))
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        addSelected(hit.transform.gameObject);
                    }
                    else
                    {
                        deselectAll();
                        addSelected(hit.transform.gameObject);

                    }
                }
                //if no units
                else if (!Input.GetKey(KeyCode.LeftShift))
                {
                    deselectAll();
                }
            }
            else//box select
            {
                if (!Input.GetKey(KeyCode.LeftShift)) deselectAll();
                boxSelect(startMousePos, endMousePos);
            }
            dragSelect = false;

        }

    }
    [SerializeField] GameObject markerPrefab;
    List<GameObject> markers = new List<GameObject>();
    void spawnMarker(Vector3 point)
    {
        markers.Add(Instantiate(markerPrefab, point, Quaternion.identity));
        Invoke("DestroyLatestMarker", 0.75f);
    }
    void DestroyLatestMarker()
    {
        GameObject marker = markers.First();
        markers.Remove(marker);
        Destroy(marker);
    }

    void boxSelect(Vector3 startMousePos, Vector3 endMousePos)
    {
        /*Debug.Log("###");
        Collider[] colliders = Physics.OverlapBox(startPos, UtilsClass.GetMouseWorldPosition());
        foreach (var collider in colliders)
        {
            Debug.Log(collider);
        }
        startPos = Mouse.current.position.ReadValue();
        startPos = UtilsClass.GetMouseWorldPosition();
        bool pressed = value.isPressed;
        Debug.Log(value.Get<float>()+"##"+pressed+"####"+startPos);*/

        Vector3 startBoxPos = Vector3.zero, endBoxPos = Vector3.zero;
        if (Physics.Raycast(
            Camera.main.ScreenPointToRay(startMousePos),
            out RaycastHit hitter, 5000, groundLayer))
        {
            startBoxPos = hitter.point;
        }
        if (Physics.Raycast(
            Camera.main.ScreenPointToRay(endMousePos)
            , out RaycastHit hitter2, 5000, groundLayer))
        {
            endBoxPos = hitter2.point;
            Vector3 center = (startBoxPos + endBoxPos) / 2;
            Vector3 extentsosia = (endBoxPos - startBoxPos) / 2;
            Vector3 extents = new Vector3(Mathf.Abs(extentsosia.x), Mathf.Abs(extentsosia.y), Mathf.Abs(extentsosia.z));
            extents.y = 20;
            Collider[] colliders = Physics.OverlapBox(center, extents, Quaternion.Euler(Camera.main.transform.forward), unitLayer);

            foreach (var collider in colliders)
            {
                //if(Own(unit))
                addSelected(collider.gameObject);
                //Debug.Log(collider.name);
            }

        }
    }



    [ServerRpc(RequireOwnership = false)]
    public void MoveServerRpc(Vector3 shmovement, GameObject[] units, ServerRpcParams rpc = default)
    {

        // Debug.Log("id sent: "+id+" owner: "+ OwnerClientId);
        /* GameObject[] a = GameObject.FindGameObjectsWithTag("Player");
         foreach (GameObject GO in a)
         {
             if (GO.GetInstanceID() == id) {
                 GO.GetComponent<CharacterController>().Move(new Vector3(10,10,10));
                 //GO.active = false;
                 Debug.Log("getInstaceID gud");
                 break;
             }
             if (GO.GetComponent<NetworkBehaviour>().OwnerClientId == OwnerClientId)
             {
                 GO.GetComponent<CharacterController>().Move(new Vector3(10, 10, 10));
                 //GO.active = false;
                 Debug.Log("ownderCLientId gud");
                 break;
             }
             if (GO.GetComponent<NetworkBehaviour>().OwnerClientId == rpc.Receive.SenderClientId) { }*/
        

        ulong clientId = rpc.Receive.SenderClientId;
        foreach (GameObject unitGO in units)
        {
            var unit =unitGO.GetComponent<Unit>();
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



    private void OnGUI()
    {
        if (dragSelect)
        {
            var rect = SelectorGUI.GetScreenRect(startMousePos, Input.mousePosition);
            SelectorGUI.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.15f));
            SelectorGUI.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));

        }

    }
}

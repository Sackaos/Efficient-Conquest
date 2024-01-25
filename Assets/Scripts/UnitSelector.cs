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
    /**
    * the halo game object  must be located at this hard coded child index
    */
    private static int HALO_CHILD_INDEX = 0; 
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
        gameObject.transform.GetChild(HALO_CHILD_INDEX).gameObject.SetActive(true);
        gameObject.GetComponent<Renderer>().material.color = Color.red;
    }

    private void undoVisualization(GameObject gameObject)
    {
        gameObject.transform.GetChild(HALO_CHILD_INDEX).gameObject.SetActive(false);
        gameObject.GetComponent<Renderer>().material.color = Color.white;
    }

    bool dragSelect = false;

    public LayerMask unitLayer;
    [SerializeField] public LayerMask groundLayer;

    Vector3 startMousePos = Vector3.zero;
    private void Update()
    {
        HandleSelection();
        HandleMovement();
    }

    public GameObject goog;
    private void HandleMovement()
    {
        const int RIGHT_MOUSE_BUTTON = 1;
        if (Input.GetMouseButtonDown(RIGHT_MOUSE_BUTTON) && selectedTable.Count > 0)
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

    private void HandleSelection()
    {
        const int LEFT_MOUSE_BUTTON = 0;
        if (Input.GetMouseButtonDown(LEFT_MOUSE_BUTTON))
        {
            startMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButton(LEFT_MOUSE_BUTTON))
        {
            int distanceFromStartPos = (startMousePos - Input.mousePosition).magnitude;
            int dragDistanceThreshold = 40;
            dragSelect = distanceFromStartPos > dragDistanceThreshold;
        }
        if (Input.GetMouseButtonUp(LEFT_MOUSE_BUTTON))
        {
            // When pressing on Shfit we are ADDING to the selection
            // else we are REPLACING the selection
            const bool isReplacingSelection = !Input.GetKey(KeyCode.LeftShift);
            if(isReplacingSelection) deselectAll();
            
            if (dragSelect) {
                boxSelect(startMousePos, Input.mousePosition);
            } else {
                selectUnitUnderMouse();
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

    void selectUnitUnderMouse() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 5000f, unitLayer)) {
            addSelected(hit.transform.gameObject);
        } 
    }

    void boxSelect(Vector3 startMousePos, Vector3 endMousePos)
    {
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

            foreach (var collider in colliders) {
                addSelected(collider.gameObject);
            }

        }
    }

    // =====================
    // =======MAGIC ========
    // =====================
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

    private void DrawMarquee() 
    {
        if (!dragSelect) return;

        var rect = SelectorGUI.GetScreenRect(startMousePos, Input.mousePosition);
        SelectorGUI.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.15f));
        SelectorGUI.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
    }

    private void OnGUI()
    {
        DrawMarquee();
    }
}

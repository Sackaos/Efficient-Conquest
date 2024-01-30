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

    public static UnitSelector SelectorSingleton;
    public static ulong ID;//NEED TO FIND A WAY TO UNSELECT UNITS NOT MINE
    public Dictionary<int, GameObject> selectedTable { get; private set; } = new Dictionary<int, GameObject>();
    public Unit SuperSelected { get; private set; }
    RaycastHit hit;
    bool dragSelect = false;

    public LayerMask unitLayer;
    public LayerMask groundLayer;

    Vector3 startMousePos = Vector3.zero, endMousePos = Vector3.zero;
    //##########      INPUT     ########



    //##########      MARKERS     ########
    [SerializeField] GameObject markerPrefab;
    List<GameObject> markers = new List<GameObject>();
    private void Awake()
    {
        if (!SelectorSingleton)
        {
            SelectorSingleton = this;
            return;
        }
        else
        {
            Debug.LogError("BAUBAU! UNIT SELECTOR not SINGLETON");
            Destroy(this);

        }
        unitLayer = LayerMask.GetMask("Unit");
        groundLayer = LayerMask.GetMask("Ground");
    }
    private void Start()
    {
        ID = new ServerRpcParams().Receive.SenderClientId;
        Debug.Log(ID);
    }
    /*
     * public override void OnNetworkSpawn()
    {
        if (!IsOwner) Destroy(this);
    }
    */
    //##########      SELECTION     ########

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

        if (selectedTable.Count > 0)
        {
            SuperSelected = selectedTable.Values.ToArray<GameObject>()[0].GetComponent<Unit>();
            UnitHud.UnitHudSingleton.DisplayUnit(SuperSelected);
        }


    }
    public void deselect(GameObject gameObject)
    {
        undoVisualization(gameObject);
        selectedTable.Remove(gameObject.GetInstanceID());
        if (selectedTable.Count == 0) { UnitHud.UnitHudSingleton.DisplayUnit(null); }

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
        Debug.Log(gameObject.name);
        gameObject.transform.GetChild(0).gameObject.SetActive(true);

        //gameObject.transform.GetChild(0).gameObject.transform.position.y=GetComponent<Renderer>().bounds.size


        gameObject.GetComponent<Renderer>().material.color = Color.red;

    }
    private void undoVisualization(GameObject gameObject)
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        gameObject.GetComponent<Renderer>().material.color = Color.white;

    }
    //##########      SELECTION     ########



    //##########      INPUT     ########


    private void Update()
    {

        LeftClick();
        RightClick();


    }


    private void RightClick()
    {
        //Debug.Log(selectedTable.Count);
        if (Input.GetMouseButtonDown(1) && selectedTable.Count > 0)
        {
            var cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(cameraRay.origin, cameraRay.direction, out RaycastHit rayHit, 5000, groundLayer + unitLayer))
            {
                foreach (GameObject go in selectedTable.Values.ToList<GameObject>())
                {
                    if (!go.GetComponent<Unit>().CanMove())
                    {
                        deselect(go);
                    }
                }
                GameObject[] selectedGameObjects = selectedTable.Values.ToArray<GameObject>();
                NetworkObjectReference[] SelectedReferenceArr = new NetworkObjectReference[selectedGameObjects.Length];
                for (int i = 0; i < SelectedReferenceArr.Length; i++)
                {
                    SelectedReferenceArr[i] = selectedGameObjects[i];
                }
                //MouveServerRpc(rayHit.point, SelectedReferenceArr);
                //MouveServerRpc(rayHit.point, selectedTable.Values.ToArray<GameObject>());

                int catLyrMask = (1 << rayHit.collider.gameObject.layer);
                if (catLyrMask == groundLayer.value) GameCoordinator.GameCoordinatorSingleton.MouveServerRpc(rayHit.point, SelectedReferenceArr);
                else if (catLyrMask == unitLayer.value) {GameCoordinator.GameCoordinatorSingleton.AttackServerRpc(rayHit.collider.gameObject, SelectedReferenceArr); }
                else { Debug.LogError(catLyrMask + "golayer<-->ground" + groundLayer.value + " wtf, Yo"); }
            }
            spawnMarker(rayHit.point);
        }
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
    //##########      MARKERS     ########





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

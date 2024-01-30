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


    public static ulong OwnerId;//NEED TO FIND A WAY TO UNSELECT UNITS NOT MINE
    UnitSelector unitSelector;
    private void Awake()
    {

        if(!GameCoordinatorSingleton) {
            GameCoordinatorSingleton = this;
        } else {
            Debug.LogError("BAUBAU! GAMECOORDINATOR not SINGLETON");
            Destroy(this);
        }

    }

    public override void OnNetworkSpawn()
    {

        /*if(!IsOwner) {
            Debug.Log("pepeW gamecoordinator spawned wrong netwrok");
            Destroy(this);
            return;

        } else {
            GetOwnIDServerRpc();
        }*/
        ChangeOwnerServerRpc(this.gameObject);
        OwnerId = NetworkManager.Singleton.LocalClientId;
        //GetComponent<NetworkObject>().ChangeOwnership(OwnerId);

    }
    private void Start()
    {
        unitSelector = UnitSelector.SelectorSingleton;

    }

    private void Update()
    {
        if(true) {
            HandleSelection();
            HandleMovement();
        }


    }

    private void HandleSelection()
    {

        const int LEFT_MOUSE_BUTTON = 0;
        if(Input.GetMouseButtonDown(LEFT_MOUSE_BUTTON)) {
            unitSelector.startMousePos = Input.mousePosition;
        }
        if(Input.GetMouseButton(LEFT_MOUSE_BUTTON)) {
            float distanceFromStartPos = (unitSelector.startMousePos - Input.mousePosition).magnitude;
            float dragDistanceThreshold = 40;
            unitSelector.dragSelect = distanceFromStartPos > dragDistanceThreshold;
        }
        if(Input.GetMouseButtonUp(LEFT_MOUSE_BUTTON)) {
            // When pressing on Shfit we are ADDING to the selection
            // else we are REPLACING the selection
            bool isReplacingSelection = !Input.GetKey(KeyCode.LeftShift);
            if(isReplacingSelection)
                unitSelector.deselectAll();

            if(unitSelector.dragSelect) {
                unitSelector.boxSelect(unitSelector.startMousePos, Input.mousePosition);

            } else {
                unitSelector.selectUnitUnderMouse();
            }
            unitSelector.dragSelect = false;
        }

    }

    private void HandleMovement()
    {
        const int RIGHT_MOUSE_BUTTON = 1;
        if(Input.GetMouseButtonDown(RIGHT_MOUSE_BUTTON) && unitSelector.selectedTable.Count > 0) {

            var cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(cameraRay.origin, cameraRay.direction, out RaycastHit rayHit, 5000, unitSelector.groundLayer + unitSelector.unitLayer)) {
                foreach(GameObject go in unitSelector.selectedTable.Values.ToList<GameObject>()) {
                    if(!go.GetComponent<Unit>().CanMove()) {

                        unitSelector.deselect(go);
                    }
                }

                NetworkObjectReference[] SelectedReferenceArr = unitSelector.newFunc();
                //MouveServerRpc(rayHit.point, SelectedReferenceArr);
                //MouveServerRpc(rayHit.point, selectedTable.Values.ToArray<GameObject>());

                int catLyrMask = (1 << rayHit.collider.gameObject.layer);
                if(catLyrMask == unitSelector.groundLayer.value) GameCoordinator.GameCoordinatorSingleton.MouveServerRpc(rayHit.point, SelectedReferenceArr);
                else if(catLyrMask == unitSelector.unitLayer.value) {
                    GameCoordinator.GameCoordinatorSingleton.AttackServerRpc(rayHit.collider.gameObject, SelectedReferenceArr);
                } else {
                    Debug.LogError(catLyrMask + "golayer<-->ground" + unitSelector.groundLayer.value + " wtf, Yo");
                }
            }
            unitSelector.SpawnMarker(rayHit.point);
        }
    }



    [ServerRpc(RequireOwnership = false)]
    public void MouveServerRpc(Vector3 shmovement, NetworkObjectReference[] unitRefs, ServerRpcParams rpc = default)
    {
        //if(!IsOwner)
        //    return;
        ulong clientId = rpc.Receive.SenderClientId;
        foreach(var unitref in unitRefs) {
            unitref.TryGet(out NetworkObject networkObject);
            var unit = networkObject.GetComponent<Unit>();
            if(unit.OwnerID == clientId) {
                unit.MoveTo(shmovement);
                
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AttackServerRpc(NetworkObjectReference enemyRef, NetworkObjectReference[] unitRefs, ServerRpcParams rpc = default)
    {
        if(!IsOwner)
            return;
        ulong clientId = rpc.Receive.SenderClientId;
        foreach(var unitref in unitRefs) {
            unitref.TryGet(out NetworkObject networkUnit);
            var unit = networkUnit.GetComponent<Unit>();
            if(unit.OwnerID == clientId) {
                enemyRef.TryGet(out NetworkObject networkEnemy);
                unit.Follow(networkEnemy.transform);
                //OwnerClientId;
            }
        }
    }

    public void SpawnUnit(UnitStats unit, Transform parentTransform)
    {
        //    Instantiate(unit.Graphics, parent.position, parent.rotation
        GameObject spawned = Instantiate(unit.prefab, parentTransform.position, parentTransform.rotation);
        spawned.GetComponent<NetworkObject>().Spawn();
        spawned.GetComponent<Unit>().OwnerID = OwnerId;
        //Instantiate(unit.Graphics, Vector3.zero,Quaternion.identity);

    }
    [ServerRpc(RequireOwnership = false)]
    public void ChangeOwnerServerRpc(NetworkObjectReference objectref, ServerRpcParams rpc = default)
    {
        ulong clientId = rpc.Receive.SenderClientId;
        objectref.TryGet(out NetworkObject networkObject);
        networkObject.GetComponent<NetworkObject>().ChangeOwnership(clientId);
    }
    public enum states
    {
        selection,
        PositionSelect,
    }
}
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
    public Dictionary<int, GameObject> UnitsCurrentlyInAction { get; private set; } = new Dictionary<int, GameObject>();


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
        Unit unit;
        foreach(GameObject unitGo in UnitsCurrentlyInAction.Values.ToArray<GameObject>()) {
            unit = unitGo.GetComponent<Unit>();
            switch(unit.CurrentAction) {
                case Unit.Actions.Empty:
                    UnitsCurrentlyInAction.Remove(unitGo.GetInstanceID());
                    break;
                case Unit.Actions.Move:
                    if(!unit.TargetUnit) { UnitsCurrentlyInAction.Remove(unitGo.GetInstanceID()); break; }//MAYBE CHANGE TO !=null
                    unit.MoveTo(unit.TargetUnit.position);
                    break;
                case Unit.Actions.Mine:
                //if(far)Go To Place 
                //else-mine
                case Unit.Actions.Attack:
                    //if(farrer than range)Go To Place 
                    //else-attack
                    break;
                case Unit.Actions.Build:
                    //if(far)Go To Place 
                    //else-build
                    break;
                case Unit.Actions.SpawnFunkyStuff:
                    break;
                case Unit.Actions.Idle:
                    UnitsCurrentlyInAction.Remove(unitGo.GetInstanceID());
                    break;
            }
        }

    }

    private void HandleSelection()
    {
        //MOUSE
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
                unitSelector.deselectAllFromTable(unitSelector.selectedTable);

            if(unitSelector.dragSelect) {
                unitSelector.boxSelect(unitSelector.startMousePos, Input.mousePosition);

            } else {
                unitSelector.selectUnitUnderMouse();
            }
            unitSelector.dragSelect = false;
        }
        //CONTROL GROUPS
        if(Input.GetKeyUp(KeyCode.Alpha1) && Input.GetKeyUp(KeyCode.LeftControl)) {//add to group
            bool isReplacingSelection = !Input.GetKey(KeyCode.LeftShift);
            if(isReplacingSelection) unitSelector.deselectAllFromTable(unitSelector.controlGroups[0]);
            unitSelector.AddSelectionToControlGroup(unitSelector.controlGroups[0]);
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

                        unitSelector.deselect(unitSelector.selectedTable, go);
                    }
                }

                NetworkObjectReference[] SelectedReferenceArr = unitSelector.newFunc();
                //MouveServerRpc(rayHit.point, SelectedReferenceArr);
                //MouveServerRpc(rayHit.point, selectedTable.Values.ToArray<GameObject>());

                int catLyrMask = (1 << rayHit.collider.gameObject.layer);
                bool hitGround = catLyrMask == unitSelector.groundLayer.value;

                bool hitUnit = (catLyrMask == unitSelector.unitLayer.value);
                bool hitEnemyUnit = rayHit.collider.gameObject.GetComponent<Unit>().OwnerID != OwnerClientId;
                if(hitGround) GameCoordinator.GameCoordinatorSingleton.MouveServerRpc(rayHit.point, SelectedReferenceArr);

                else if(hitUnit && hitEnemyUnit) {
                    GameCoordinator.GameCoordinatorSingleton.AttackServerRpc(rayHit.collider.gameObject, SelectedReferenceArr);
                    
                } else if(hitUnit) {
                    GameCoordinator.GameCoordinatorSingleton.FollowServerRpc(rayHit.collider.gameObject, SelectedReferenceArr);
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
        //if(!IsOwner) Debug.Log("mouveserverp[c");
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
        //if(!IsOwner)
        //return;
        ulong clientId = rpc.Receive.SenderClientId;
        foreach(var unitref in unitRefs) {
            unitref.TryGet(out NetworkObject networkUnit);
            var unit = networkUnit.GetComponent<Unit>();
            if(unit.OwnerID == clientId) {
                enemyRef.TryGet(out NetworkObject networkEnemy);
                unit.Attack(networkEnemy.transform);
                //OwnerClientId;
            }
        }
    }
    public void FollowServerRpc(NetworkObjectReference enemyRef, NetworkObjectReference[] unitRefs, ServerRpcParams rpc = default)
    {
        //if(!IsOwner)
        //return;
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
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using CodeMonkey.Utils;
public class RTSController : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    [SerializeField] Camera MainCamera;
    [SerializeField] LayerMask GroundLayer;
    [SerializeField] LayerMask UnitsLayer;
    Vector3 startPos;

    private void OnEnable()
    {
        playerInput.SwitchCurrentActionMap("Player");
        Debug.Log(playerInput.currentActionMap);

    }
    private void Awake()
    {
        //playerInput = new PlayerInput();

    }
    void Start()
    {

    }

    /*public void OnMouse(InputAction.CallbackContext context)
    {
        Vector2 i = Mouse.current.position.ReadValue();
        Debug.Log("Fire!");
    }*/
    void OnMouse(InputValue value)
    {
        //Debug.Log(mainCamera.ScreenToWorldPoint(value.Get<Vector2>()));
        //Debug.Log(UtilsClass.GetMouseWorldPosition());
        MyMouseClick();
    }
    private void MyMouseClick()
    {
        //Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        //if (Physics.Raycast(Camera.main.transform.position, mousePos,out RaycastHit hit))
        //{
        // Call methods here
        //    Debug.Log("Raycast Hit -> " + hit.transform.name);
        //}


        if (Physics.Raycast(
            Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()),
            out RaycastHit hitter))
        {
            //Debug.LogFormat("You passed over [{0}]", hitter.collider.gameObject.name);
        }





    }
    void OnMouseClick(InputValue value)
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

        if (Physics.Raycast(
            Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()),
            out RaycastHit hitter))
        {
            //if() hit floor
            startPos = hitter.point;
        }
        startPos = new Vector3(-50, -50, -50);

        if (Physics.Raycast(
            MainCamera.ScreenPointToRay(Mouse.current.position.ReadValue())
            ,out RaycastHit hitter2))
        {
            Vector3 endPos = hitter2.point;
            Vector3 center = (startPos + endPos) / 2;
            Vector3 extents = (endPos-startPos)/2;

            extents.y = 10;
            Collider[] colliders = Physics.OverlapBox(center, extents,Quaternion.identity,UnitsLayer);
            Debug.Log("###");

            foreach (var collider in colliders)
            {
                collider.GetComponent<Renderer>().material.color=Color.red;
                Debug.Log(collider.name);
            }
            Debug.Log("###");

        }
    }
    void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
            //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode
    }


    private void onDisable()
    {
        playerInput.enabled = false;
    }

}

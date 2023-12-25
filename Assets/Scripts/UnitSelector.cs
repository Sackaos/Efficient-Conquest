using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelector : MonoBehaviour
{

    MeshCollider selectionBox;
    Mesh selectionMesh;
    Vector2[] corners;
    Vector3[] verts;

    
    Dictionary<int, GameObject> selectedTable = new Dictionary<int, GameObject>();
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
        Debug.Log(selectedTable);

        foreach (GameObject go in selectedTable.Values.ToList<GameObject>())
        {
            deselect(go);
        }
    }

    private void selectionVisualizer(GameObject gameObject)
    {
        gameObject.GetComponent<Renderer>().material.color = Color.red;

    }
    private void undoVisualization(GameObject gameObject)
    {
        gameObject.GetComponent<Renderer>().material.color = Color.white;

    }

    RaycastHit hit;
    bool dragSelect = false;

    [SerializeField] LayerMask unitLayer;
    [SerializeField] LayerMask groundLayer;

    Vector3 startMousePos = Vector3.zero, endMousePos = Vector3.zero;
    private void Update()
    {
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
        //mouseUp
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
            Debug.Log("###");

            foreach (var collider in colliders)
            {
                //if(Own(unit))
                addSelected(collider.gameObject);
                Debug.Log(collider.name);
            }
            Debug.Log("###");

        }
    }
    private void OnDrawGizmos()
    {


    }
}

using UnityEngine;

public class CameraController : MonoBehaviour
{
    //GET MAX BOUNDS BY USING GetComponent<Renderer>().bounds.size
    public float panSpeed = 20f;
    public float panBorderThickness = 10f;
    public Vector2 panLimit;

    public float scrollSpeed = 20f;
    public float minY = 5f;
    public float maxY = 40f;
    [SerializeField] private bool PanToggle = true;
    void Update()
    {
        if (Input.GetKey(KeyCode.Insert))PanToggle = !PanToggle;
            Vector3 pos = transform.position;
        if (PanToggle)
        {
            if (Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y >= Screen.height - panBorderThickness)
            {
                pos.z += panSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y <= panBorderThickness)
            {
                pos.z -= panSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= Screen.width - panBorderThickness)
            {
                pos.x += panSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x <= panBorderThickness)
            {
                pos.x -= panSpeed * Time.deltaTime;
            }
        }
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y += scroll * scrollSpeed * 100f * Time.deltaTime;

        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);

        transform.position = pos;
    }






    public void keyCameraMovement()
    {
        
    }
}

using UnityEngine;

public class CubeManager : MonoBehaviour
{
    private float xyLimit = 5;
    private Vector3 nextPosition;

    private void Start()
    {
        transform.position = Vector3.zero;
    }

    void Update()
    {
        // Change the position of the cube
        nextPosition = transform.position;

        if (Input.GetKeyDown(KeyCode.UpArrow) && transform.position.y <= xyLimit)
        {
            nextPosition += Vector3.up;
            CsvLogger.LogEvent("Cube","Moved UP");
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) && transform.position.y >= -xyLimit)
        {
            nextPosition += Vector3.down;
            CsvLogger.LogEvent("Cube","Moved DOWN");
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) && transform.position.x <= xyLimit)
        {
            nextPosition += Vector3.right;
            CsvLogger.LogEvent("Cube","Moved RIGHT");
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) && transform.position.x >= -xyLimit)
        {
            nextPosition += Vector3.left;
            CsvLogger.LogEvent("Cube","Moved LEFT");
        }

        transform.position = nextPosition;

        // Change size
        if (Input.GetKeyDown(KeyCode.Space))
        {
            transform.localScale = Vector3.one * 3f;
            CsvLogger.LogEvent("Cube","Size: BIG");
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            transform.localScale = Vector3.one * 1f;
            CsvLogger.LogEvent("Cube","Size: Normal");
        }
    }
}

using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    public float gridSize = 1.0f;  // 그리드의 간격
    public int gridCount = 20;  // 그리드의 크기
    public Color lineColor = Color.white;  // 라인 색상

    private void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        GameObject gridParent = new GameObject("Grid");

        for (int x = -gridCount; x <= gridCount; x++)
        {
            CreateLine(new Vector3(x * gridSize, 0, -gridCount * gridSize), new Vector3(x * gridSize, 0, gridCount * gridSize), gridParent.transform);
        }

        for (int z = -gridCount; z <= gridCount; z++)
        {
            CreateLine(new Vector3(-gridCount * gridSize, 0, z * gridSize), new Vector3(gridCount * gridSize, 0, z * gridSize), gridParent.transform);
        }
    }

    void CreateLine(Vector3 start, Vector3 end, Transform parent)
    {
        GameObject line = new GameObject("GridLine");
        line.transform.parent = parent;
        LineRenderer lr = line.AddComponent<LineRenderer>();
        //줄 줄이기1
        lr.startWidth = 0.01f;
        //줄 줄이기2
        lr.endWidth = 0.01f;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lineColor;
        lr.endColor = lineColor;
    }
}

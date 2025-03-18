using UnityEngine;
using System.Collections.Generic;

public class PLYImporter : MonoBehaviour
{
    public string plyFileName = "scans"; // 확장자는 제외하고 파일명만 지정

    void Start()
    {
        LoadPLYFile();
    }

    void LoadPLYFile()
    {
        // TextAsset으로 파일 불러오기
        TextAsset plyFile = Resources.Load<TextAsset>(plyFileName);

        if (plyFile == null)
        {
            Debug.LogError("파일을 찾을 수 없습니다: " + plyFileName);
            return;
        }

        List<Vector3> vertices = new List<Vector3>();
        List<Color> colors = new List<Color>();

        string[] lines = plyFile.text.Split('\n');
        bool inVertexSection = false;
        bool inHeader = true;

        foreach (string line in lines)
        {
            if (inHeader)
            {
                if (line.StartsWith("end_header"))
                {
                    inHeader = false;
                    inVertexSection = true;
                }
                continue;
            }

            if (inVertexSection)
            {
                string[] parts = line.Split(' ');

                // Vertex positions
                float x = float.Parse(parts[0]);
                float y = float.Parse(parts[1]);
                float z = float.Parse(parts[2]);
                vertices.Add(new Vector3(x, y, z));

                // Vertex colors (if available in PLY file)
                if (parts.Length >= 6)
                {
                    byte r = byte.Parse(parts[3]);
                    byte g = byte.Parse(parts[4]);
                    byte b = byte.Parse(parts[5]);
                    colors.Add(new Color32(r, g, b, 255));
                }
            }
        }

        // 포인트 클라우드 생성
        CreatePointCloud(vertices, colors);
    }

    void CreatePointCloud(List<Vector3> vertices, List<Color> colors)
    {
        GameObject pointCloud = new GameObject("PointCloud");
        Mesh mesh = new Mesh();

        mesh.SetVertices(vertices);

        if (colors.Count > 0)
        {
            mesh.SetColors(colors);
        }

        int[] indices = new int[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            indices[i] = i;
        }

        mesh.SetIndices(indices, MeshTopology.Points, 0);

        MeshFilter meshFilter = pointCloud.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = pointCloud.AddComponent<MeshRenderer>();

        meshFilter.mesh = mesh;
        meshRenderer.material = new Material(Shader.Find("Unlit/Color"));
    }
}

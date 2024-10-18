using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ProceduralTerrain : MonoBehaviour
{
    [Range(2, 256)] [SerializeField] private int _resolution = 10; 
    [SerializeField] private float _heightMultiplier = 2f; 
    
    public float _randomness = 1f; 
    public bool _smooth = false; 
    private Mesh _mesh; 
    private Vector3[] _vertices; 
    
    private void OnValidate()
    {
        GenerateTerrain();
    }
    
    private void GenerateTerrain()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
        
        _vertices = new Vector3[_resolution * _resolution];
        int[] triangles = new int[(_resolution - 1) * (_resolution - 1) * 6];

        float stepSize = 1f / (_resolution - 1);
        
        for (int y = 0; y < _resolution; y++)
        {
            for (int x = 0; x < _resolution; x++)
            {
                int index = y * _resolution + x;
                float height = Mathf.PerlinNoise(x * stepSize * _randomness, y * stepSize * _randomness) * _heightMultiplier;
                _vertices[index] = new Vector3(x * stepSize, height, y * stepSize);
            }
        }
        
        int triIndex = 0;
        for (int y = 0; y < _resolution - 1; y++)
        {
            for (int x = 0; x < _resolution - 1; x++)
            {
                int i = y * _resolution + x;
                
                triangles[triIndex + 0] = i;
                triangles[triIndex + 1] = i + _resolution;
                triangles[triIndex + 2] = i + 1;
                
                triangles[triIndex + 3] = i + 1;
                triangles[triIndex + 4] = i + _resolution;
                triangles[triIndex + 5] = i + _resolution + 1;

                triIndex += 6;
            }
        }
        
        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.triangles = triangles;
        _mesh.RecalculateNormals();
        
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = _mesh; 
        
        if (_smooth)
        {
            SmoothTerrain();
        }
    }
    
    private void SmoothTerrain()
    {
        System.Func<int, int, Vector3> GetVertex = (x, y) => _vertices[y * _resolution + x];

        for (int y = 1; y < _resolution - 1; y++)
        {
            for (int x = 1; x < _resolution - 1; x++)
            {
                Vector3 avgHeight = (
                    GetVertex(x - 1, y) + GetVertex(x + 1, y) +
                    GetVertex(x, y - 1) + GetVertex(x, y + 1) +
                    GetVertex(x - 1, y - 1) + GetVertex(x + 1, y + 1) +
                    GetVertex(x - 1, y + 1) + GetVertex(x + 1, y - 1)
                ) / 8f;

                _vertices[y * _resolution + x].y = avgHeight.y;
            }
        }

        _mesh.vertices = _vertices;
        _mesh.RecalculateNormals();
    }
}

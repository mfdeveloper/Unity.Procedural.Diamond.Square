using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DiamondSquareTerrain : MonoBehaviour
{
    public Canvas canvasFields;

    //TODO: Limite this for 254, because Unity just create
    // a only one mesh with 65.000 vertices
    [Tooltip("Number of faces of the terrain")]
    public int divisions = 128;
    [Tooltip("All the size of terrain (e.g A matrix of 10 x 10)")]
    public float size = 30;
    [Tooltip("The max height of the terrain")]
    public float maxHeight = 5;

    /// <summary>
    /// Vertices points to render triangles
    /// in a <b>Mesh</b>
    /// </summary>
    protected Vector3[] vertices;

    /// <summary>
    /// Total number of vertices, defined by
    /// formula: 2ⁿ + 1
    /// </summary>
    protected int verticesCount;

    // ----- Unity Mesh components -----
    private MeshFilter meshFilter;
    private Mesh mesh;
    private InputField[] uiFields;

    void Awake()
    {
        if (canvasFields != null)
        {
            uiFields = canvasFields.GetComponentsInChildren<InputField>();
        }

        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }

    // Start is called before the first frame update
    void Start()
    {
        PopulateFields();
        Create();
    }

    /// <summary>
    /// Create and draw a procedural terrain.
    /// 
    /// This is a separated method because can be executed from
    /// others emitters (An C++ OpenGL renderer, or when click in
    /// a UI button, for example...)
    /// </summary>
    public void Create()
    {

        UpdateParams();

        /**
         * Calculate total of vertices.
         * Using the formula: 2ⁿ + 1 for X and Y in the terrain
         * expressed by one Matrix
         * 
         * So, this formula can be expressed like: (n + 1) * (n + 1)
         * where => (n + 1) for X and (n + 1) for Y. 
         * 
         * So, this calculation can be (divisions + 1) * (divisions + 1)
         * or (n + 1)²
         */
        verticesCount = (int)Mathf.Pow(divisions + 1, 2);

        vertices = new Vector3[verticesCount];

        //Uvs need be equal total of vertices (points)
        Vector2[] uvs = new Vector2[verticesCount];

        /**
         * divisions * divisions because all the faces are:
         * 
         *    divisions in Y multiplied by divisions in X (because a plane is a matrix)
         *  
         *   Multiply by 6, because each square is equal 2 triangles
         *   and each triangle have 3 vertices. So, 2 * 3 = 6
         *   
         */
        int[] triangles = new int[divisions * divisions * 6];

        float halfSize = size * 0.5f;
        float divisionSize = size / divisions;

        int trianglesOffset = 0;

        for (int i = 0; i <= divisions; i++)
        {
            for (int j = 0; j <= divisions; j++)
            {
                /**
                 * One-dimensional array to represent two-dimensional array (matrix)
                 */
                vertices[i * (divisions + 1) + j] = new Vector3(-halfSize + j * divisionSize, 0.0f, halfSize - i * divisionSize);
                uvs[i * (divisions + 1) + j] = new Vector2((float)i / divisions, (float)j / divisions);

                if (i < divisions && j < divisions)
                {
                    int topLeft = i * (divisions + 1) + j;
                    int bottomLeft = (i + 1) * (divisions + 1) + j;

                    triangles[trianglesOffset] = topLeft;
                    triangles[trianglesOffset + 1] = topLeft + 1;
                    triangles[trianglesOffset + 2] = bottomLeft + 1;

                    triangles[trianglesOffset + 3] = topLeft;
                    triangles[trianglesOffset + 4] = bottomLeft + 1;
                    triangles[trianglesOffset + 5] = bottomLeft;

                    trianglesOffset += 6;
                }
            }
        }

        DiamondSquare();

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    private void DiamondSquare()
    {
        vertices[0].y = Random.Range(-maxHeight, maxHeight);
        vertices[divisions].y = Random.Range(-maxHeight, maxHeight);
        vertices[vertices.Length - 1].y = Random.Range(-maxHeight, maxHeight);
        vertices[vertices.Length - 1 - divisions].y = Random.Range(-maxHeight, maxHeight);

        int iterations = (int)Mathf.Log(divisions, 2);
        int numSquares = 1;
        int squareSize = divisions;

        for (int i = 0; i < iterations; i++)
        {
            int row = 0;
            for (int j = 0; j < numSquares; j++)
            {
                int col = 0;
                for (int k = 0; k < numSquares; k++)
                {
                    DiamondStep(row, col, squareSize, maxHeight);
                    col += squareSize;
                }
                row += squareSize;
            }
            numSquares *= 2;
            squareSize /= 2;
            maxHeight *= 0.5f;
        }
    }

    private void DiamondStep(int row, int col, int size, float offset)
    {
        int halfSize = (int)(size * 0.5f);
        int topLeft = row * (divisions + 1) + col;
        int bottomLeft = (row + size) * (divisions + 1) + col;

        int mid = (int)(row + halfSize) * (divisions + 1) + (int)(col + halfSize);
        vertices[mid].y = (vertices[topLeft].y + vertices[topLeft + size].y + vertices[bottomLeft].y + vertices[bottomLeft + size].y) * 0.25f + Random.Range(-offset, offset);

        SquareStep(size, offset, halfSize, topLeft, bottomLeft, mid);
    }

    private void SquareStep(int size, float offset, int halfSize, int topLeft, int bottomLeft, int mid)
    {
        vertices[topLeft + halfSize].y = (vertices[topLeft].y + vertices[topLeft + size].y + vertices[mid].y) / 3 + Random.Range(-offset, offset);
        vertices[mid - halfSize].y = (vertices[topLeft].y + vertices[bottomLeft].y + vertices[mid].y) / 3 + Random.Range(-offset, offset);
        vertices[mid + halfSize].y = (vertices[topLeft + size].y + vertices[bottomLeft + size].y + vertices[mid].y) / 3 + Random.Range(-offset, offset);
        vertices[bottomLeft + halfSize].y = (vertices[bottomLeft].y + vertices[bottomLeft + size].y + vertices[mid].y) / 3 + Random.Range(-offset, offset);
    }

    // ------- UI Fields params -------
    private void UpdateParams()
    {
        if (uiFields != null && uiFields.Length > 0)
        {
            foreach (var field in uiFields)
            {
                if (field.text.Length > 0 && field.name.ToLower().StartsWith("divisions"))
                {
                    var value = int.Parse(field.text);
                    if (divisions != value)
                    {
                        divisions = value;
                    }
                }

                if (field.text.Length > 0 && field.name.ToLower().StartsWith("size"))
                {
                    var value = float.Parse(field.text);

                    if (size != value)
                    {
                        size = value;
                    }
                }

                if (field.text.Length > 0 && field.name.ToLower().StartsWith("maxheight"))
                {
                    var value = float.Parse(field.text);

                    if (maxHeight != value)
                    {
                        maxHeight = value;
                    }
                }
            }
        }
    }

    private void PopulateFields()
    {
        if (uiFields != null && uiFields.Length > 0)
        {
            foreach (var field in uiFields)
            {
                if (field.name.ToLower().StartsWith("divisions"))
                {
                    field.text = divisions.ToString();
                }

                if (field.name.ToLower().StartsWith("size"))
                {
                    field.text = size.ToString();
                }

                if (field.name.ToLower().StartsWith("maxheight"))
                {
                    field.text = maxHeight.ToString();
                }
            }
        }
    }
}

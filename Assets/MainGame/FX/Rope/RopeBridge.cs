using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))] // Ensure components exist
public class RopeBridge : MonoBehaviour
{
    public Transform StartPoint;
    public Transform EndPoint;
    private Material RopeMaterial; // Assign your rope material in the Inspector
    public int segmentLength = 100; // Current number of segments

    private MeshFilter meshFilter;
    private Mesh mesh;

    private List<RopeSegment> ropeSegments = new List<RopeSegment>();
    private float initialRopeSegLen = 0.1f; // Store the desired segment length
    public float RopeWidth = 0.15f; // Use this instead of lineWidth
    private float segmentAdjustSpeed = 5f; // How quickly segments are added/removed (segments per second)
    public int RadialSegments = 6; // How many sides the cylinder profile has (minimum 3)
    public float TextureTileLength = 1.0f; // World distance for one texture repetition (tile)

    // --- Wind Up Variables ---
    public float WindUpSpeed = 0.5f; // Units per second the texture scrolls
    public KeyCode WindUpKey = KeyCode.W;
    public KeyCode WindDownKey = KeyCode.S;
    private float currentTextureOffset = 0f;
    private Material ropeMaterialInstance; // Instance of the material to modify offset


    // Mesh data arrays
    private Vector3[] vertices;
    private Vector2[] uvs;
    private int[] triangles;

    void Start()
    {
        // Get Mesh components
        meshFilter = GetComponent<MeshFilter>();
        

        // --- Create Material Instance ---
        // This prevents changing the original Material asset
        // If you WANT to change the asset, use meshRenderer.sharedMaterial instead
        
        mesh = new Mesh();
        mesh.name = "Rope Mesh";
        meshFilter.mesh = mesh;

        ropeSegments.Clear();
        for (int i = 0; i < segmentLength; i++)
        {
            float t = i / (float)(segmentLength - 1);
            Vector3 pos = Vector3.Lerp(StartPoint.position, EndPoint.position, t);
            this.ropeSegments.Add(new RopeSegment(pos));
        }
        segmentLength = ropeSegments.Count;

        BuildMesh();
    }
    public void SetRopeMaterial(Material material)
    {
        RopeMaterial = material;
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>(); // Get renderer
        if (meshRenderer.material != null)
        {
            ropeMaterialInstance = meshRenderer.material;
        }
        else if (RopeMaterial != null) // Fallback if not assigned in renderer but in script
        {
            ropeMaterialInstance = Instantiate(RopeMaterial);
            meshRenderer.material = ropeMaterialInstance;
        }
        else
        {
            Debug.LogError("RopeBridge needs a material assigned either in the MeshRenderer or the RopeMaterial field.", this);
            // Optionally disable the script or handle error
        }
    }
    void Update()
    {
        HandleWindUpInput(); // Check for key presses and update offset
        SmoothlyAdjustSegments();
        UpdateMesh();
    }

    private void FixedUpdate()
    {
        if (ropeSegments.Count >= 2)
        {
            this.Simulate();
        }
    }

    private void HandleWindUpInput()
    {
        if (ropeMaterialInstance == null) return; // Don't try if material is missing

        float scrollAmount = 0f;
        if (Input.GetKey(WindUpKey))
        {
            scrollAmount = WindUpSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(WindDownKey))
        {
            scrollAmount = -WindUpSpeed * Time.deltaTime;
        }

        if (scrollAmount != 0f)
        {
            currentTextureOffset += scrollAmount;

            // Apply the offset to the material instance
            Vector2 newOffset = new Vector2(currentTextureOffset, 0);

            // Set offset for all relevant maps (ensure names match shader properties)
            ropeMaterialInstance.SetTextureOffset("_MainTex", newOffset);
            if (ropeMaterialInstance.HasProperty("_BumpMap")) // Check if property exists
            {
                ropeMaterialInstance.SetTextureOffset("_BumpMap", newOffset);
            }
            if (ropeMaterialInstance.HasProperty("_OcclusionMap")) // Check if property exists
            {
                ropeMaterialInstance.SetTextureOffset("_OcclusionMap", newOffset);
            }
        }
    }

    private void SmoothlyAdjustSegments()
    {
        int previousSegmentCount = segmentLength;

        // Use Vector3 distance
        float distance = Vector3.Distance(StartPoint.position, EndPoint.position);
        float desiredLength = distance * 1.05f;
        int idealSegmentCount = Mathf.Max(2, Mathf.RoundToInt(desiredLength / initialRopeSegLen));
        int targetSegmentCountThisFrame = Mathf.RoundToInt(Mathf.MoveTowards(segmentLength, idealSegmentCount, Time.deltaTime * segmentAdjustSpeed));

        // Add segments
        while (ropeSegments.Count < targetSegmentCountThisFrame && ropeSegments.Count > 0)
        {
            RopeSegment lastSegment = ropeSegments[ropeSegments.Count - 1];
            Vector3 directionToEnd = (EndPoint.position - lastSegment.posNow).normalized;
            Vector3 newPos = lastSegment.posNow + directionToEnd * initialRopeSegLen * 0.5f;
            ropeSegments.Add(new RopeSegment(newPos));
        }

        // Remove segments
        while (ropeSegments.Count > targetSegmentCountThisFrame && ropeSegments.Count > 2)
        {
            ropeSegments.RemoveAt(ropeSegments.Count - 1);
        }

        segmentLength = ropeSegments.Count;

        if (segmentLength != previousSegmentCount && segmentLength >= 2)
        {
            BuildMesh();
        }
    }

    private void BuildMesh()
    {
        if (segmentLength < 2 || RadialSegments < 3) return;

        int vertexCount = segmentLength * RadialSegments;
        int triangleCount = (segmentLength - 1) * RadialSegments * 6;

        vertices = new Vector3[vertexCount];
        uvs = new Vector2[vertexCount];
        triangles = new int[triangleCount];

        int currentTriangleIndex = 0;
        for (int i = 0; i < segmentLength - 1; i++)
        {
            for (int j = 0; j < RadialSegments; j++)
            {
                int currentRingBaseIndex = i * RadialSegments;
                int nextRingBaseIndex = (i + 1) * RadialSegments;

                int v0 = currentRingBaseIndex + j;
                int v1 = nextRingBaseIndex + j;
                int v2 = nextRingBaseIndex + (j + 1) % RadialSegments;
                int v3 = currentRingBaseIndex + (j + 1) % RadialSegments;

                triangles[currentTriangleIndex++] = v0;
                triangles[currentTriangleIndex++] = v1;
                triangles[currentTriangleIndex++] = v2;

                triangles[currentTriangleIndex++] = v0;
                triangles[currentTriangleIndex++] = v2;
                triangles[currentTriangleIndex++] = v3;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
    }

    private void UpdateMesh()
    {
        if (ropeSegments.Count < 2 || RadialSegments < 3 || vertices == null || vertices.Length != ropeSegments.Count * RadialSegments)
        {
            if (ropeSegments.Count >= 2) BuildMesh();
            if (vertices == null) return;
        }

        float accumulatedLength = 0f;
        Vector3 lastUpVector = Vector3.up;

        for (int i = 0; i < ropeSegments.Count; i++)
        {
            Vector3 segmentPos = ropeSegments[i].posNow;

            Vector3 segmentDir = Vector3.zero;
            if (i < ropeSegments.Count - 1)
            {
                segmentDir = (ropeSegments[i + 1].posNow - segmentPos).normalized;
            }
            else
            {
                segmentDir = (segmentPos - ropeSegments[i - 1].posNow).normalized;
            }

            if (i > 0)
            {
                accumulatedLength += Vector3.Distance(ropeSegments[i].posNow, ropeSegments[i - 1].posNow);
            }
            float uCoord = accumulatedLength / TextureTileLength;

            Quaternion segmentRotation;
            if (segmentDir != Vector3.zero && Vector3.Cross(segmentDir, lastUpVector).sqrMagnitude > 0.001f)
            {
                segmentRotation = Quaternion.LookRotation(segmentDir, lastUpVector);
                lastUpVector = segmentRotation * Vector3.up;
            }
            else
            {
                if (i > 0)
                {
                    Vector3 prevSegmentPos = ropeSegments[i - 1].posNow;
                    Vector3 prevSegmentDir = (segmentPos - prevSegmentPos).normalized;
                    if (prevSegmentDir != Vector3.zero && Vector3.Cross(prevSegmentDir, lastUpVector).sqrMagnitude > 0.001f)
                    {
                        segmentRotation = Quaternion.LookRotation(prevSegmentDir, lastUpVector);
                    }
                    else
                    {
                        segmentRotation = Quaternion.identity;
                    }
                }
                else
                {
                    segmentRotation = Quaternion.identity;
                }
            }

            Vector3 localRight = segmentRotation * Vector3.right;
            Vector3 localUp = segmentRotation * Vector3.up;

            for (int j = 0; j < RadialSegments; j++)
            {
                float angle = (float)j / RadialSegments * 360f * Mathf.Deg2Rad;
                float radius = RopeWidth * 0.5f;

                Vector3 offset = (localRight * Mathf.Cos(angle) + localUp * Mathf.Sin(angle)) * radius;
                int vertexIndex = i * RadialSegments + j;
                vertices[vertexIndex] = segmentPos + offset;

                float vCoord = (float)j / RadialSegments;
                uvs[vertexIndex] = new Vector2(uCoord, vCoord);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        //Vector3[] normals = mesh.normals;
        //for (int i = 0; i < normals.Length; i++)
        //{
        //    normals[i] = -normals[i];
        //}
        //mesh.normals = normals;
    }

    private void Simulate()
    {
        Vector3 forceGravity = new Vector3(0f, -9.81f, 0f);

        for (int i = 1; i < this.ropeSegments.Count; i++)
        {
            RopeSegment currentSegment = this.ropeSegments[i];
            Vector3 velocity = currentSegment.posNow - currentSegment.posOld;
            currentSegment.posOld = currentSegment.posNow;
            currentSegment.posNow += velocity;
            currentSegment.posNow += forceGravity * Time.fixedDeltaTime * Time.fixedDeltaTime;
            this.ropeSegments[i] = currentSegment;
        }

        int constraintIterations = 50;
        for (int i = 0; i < constraintIterations; i++)
        {
            this.ApplyConstraint();
        }
    }

    private void ApplyConstraint()
    {
        if (ropeSegments.Count < 2) return;

        RopeSegment firstSegment = this.ropeSegments[0];
        firstSegment.posNow = this.StartPoint.position;
        this.ropeSegments[0] = firstSegment;

        RopeSegment endSegment = this.ropeSegments[this.ropeSegments.Count - 1];
        endSegment.posNow = this.EndPoint.position;
        this.ropeSegments[this.ropeSegments.Count - 1] = endSegment;

        float distance = Vector3.Distance(StartPoint.position, EndPoint.position);
        float currentRequiredSegLen = (ropeSegments.Count > 1) ? distance / (ropeSegments.Count - 1) : 0;
        float constraintSegLen = Mathf.Min(initialRopeSegLen, currentRequiredSegLen);

        for (int i = 0; i < this.ropeSegments.Count - 1; i++)
        {
            RopeSegment firstSeg = this.ropeSegments[i];
            RopeSegment secondSeg = this.ropeSegments[i + 1];

            float dist = (firstSeg.posNow - secondSeg.posNow).magnitude;
            float error = dist - constraintSegLen;
            Vector3 changeDir = Vector3.zero;
            if (dist > 0.001f)
            {
                changeDir = (firstSeg.posNow - secondSeg.posNow).normalized;
            }
            Vector3 changeAmount = changeDir * error;

            if (i != 0)
            {
                firstSeg.posNow -= changeAmount * 0.5f;
                this.ropeSegments[i] = firstSeg;
                if (i + 1 < this.ropeSegments.Count - 1)
                {
                    secondSeg.posNow += changeAmount * 0.5f;
                    this.ropeSegments[i + 1] = secondSeg;
                }
            }
            else
            {
                if (i + 1 < this.ropeSegments.Count - 1)
                {
                    secondSeg.posNow += changeAmount;
                    this.ropeSegments[i + 1] = secondSeg;
                }
            }
        }
    }

    public struct RopeSegment
    {
        public Vector3 posNow;
        public Vector3 posOld;

        public RopeSegment(Vector3 pos)
        {
            this.posNow = pos;
            this.posOld = pos;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Portal : MonoBehaviour
{
    [field: SerializeField] public Portal OtherPortal { get; private set; } 
    [field: SerializeField] public Color PortalColour { get; private set; }
    public Renderer Renderer { get; private set; }
    public bool IsPlaced { get; private set; }
    
    // Cache shader properties.
    private static readonly int OutlineColour = Shader.PropertyToID("_OutlineColour");
    
    [SerializeField] private Renderer _outlineRenderer;
    [SerializeField] private LayerMask _placementMask;
    [SerializeField] private Transform _testTransform;
    
    private List<PortalableObject> _portalObjects = new();
    private Collider _wallCollider;
    private BoxCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        Renderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        _outlineRenderer.material.SetColor(OutlineColour, PortalColour);
        
        gameObject.SetActive(false);
    }

    private void Update()
    {
        Renderer.enabled = OtherPortal.IsPlaced;

        foreach (PortalableObject portableObject in _portalObjects)
        {
            Vector3 objPos = transform.InverseTransformPoint(portableObject.transform.position);

            if (objPos.z > 0.0f)
            {
                portableObject.Warp();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var obj = other.GetComponent<PortalableObject>();
        
        if (obj == null) return;
        
        _portalObjects.Add(obj);
        obj.SetIsInPortal(this, OtherPortal, _wallCollider);
    }

    private void OnTriggerExit(Collider other)
    {
        var obj = other.GetComponent<PortalableObject>();

        if (!_portalObjects.Contains(obj)) return;
        
        _portalObjects.Remove(obj);
        obj.ExitPortal(_wallCollider);
    }

    public bool PlacePortal(Collider wallCollider, Vector3 pos, Quaternion rot)
    {
        _testTransform.position = pos;
        _testTransform.rotation = rot;
        
        _testTransform.position -= _testTransform.forward * 0.001f;

        FixOverhangs();
        FixIntersects();

        if (!CheckOverlap()) return false;
        
        _wallCollider = wallCollider;
        transform.position = _testTransform.position;
        transform.rotation = _testTransform.rotation;

        gameObject.SetActive(true);
        IsPlaced = true;
        return true;
    }

    // Ensure the portal cannot extend past the edge of a surface.
    private void FixOverhangs()
    {
        List<Vector3> testPoints = new List<Vector3>
        {
            new(-0.44f, 0.0f, 0.1f),
            new(0.44f, 0.0f, 0.1f),
            new(0.0f, -0.44f, 0.1f),
            new(0.0f, 0.44f, 0.1f)
        };

        List<Vector3> testDirs = new List<Vector3>
        {
             Vector3.right,
            -Vector3.right,
             Vector3.up,
            -Vector3.up
        };

        for(int i = 0; i < 4; ++i)
        {
            RaycastHit hit;
            Vector3 raycastPos = _testTransform.TransformPoint(testPoints[i]);
            Vector3 raycastDir = _testTransform.TransformDirection(testDirs[i]);

            if(Physics.CheckSphere(raycastPos, 0.02f, _placementMask))
            {
                break;
            }

            if(Physics.Raycast(raycastPos, raycastDir, out hit, 0.44f, _placementMask))
            {
                Vector3 offset = hit.point - raycastPos;
                
                _testTransform.Translate(offset, Space.World);
            }
        }
    }

    // Ensure the portal cannot intersect a section of wall.
    private void FixIntersects()
    {
        List<Vector3> testDirections = new List<Vector3>
        {
             Vector3.right,
            -Vector3.right,
             Vector3.up,
            -Vector3.up
        };

        List<float> testDistances = new List<float> { 0.44f, 0.44f, 0.44f, 0.44f };

        for (int i = 0; i < 4; ++i)
        {
            RaycastHit hit;
            Vector3 raycastPos = _testTransform.TransformPoint(0.0f, 0.0f, -0.1f);
            Vector3 raycastDir = _testTransform.TransformDirection(testDirections[i]);

            if (Physics.Raycast(raycastPos, raycastDir, out hit, testDistances[i], _placementMask))
            {
                Vector3 offset = (hit.point - raycastPos);
                Vector3 newOffset = -raycastDir * (testDistances[i] - offset.magnitude);
                
                _testTransform.Translate(newOffset, Space.World);
            }
        }
    }

    // Once positioning has taken place, ensure the portal isn't intersecting anything.
    private bool CheckOverlap()
    {
        Vector3 checkExtents = new Vector3(0.36f, 0.36f, 0.05f);
        Vector3 testTransformPos = _testTransform.position;
        Vector3[] checkPositions = {
            testTransformPos + _testTransform.TransformVector(new Vector3( 0.0f,  0.0f, -0.1f)),
            testTransformPos + _testTransform.TransformVector(new Vector3(-0.4f, -0.4f, -0.1f)),
            testTransformPos + _testTransform.TransformVector(new Vector3(-0.4f,  0.4f, -0.1f)),
            testTransformPos + _testTransform.TransformVector(new Vector3( 0.4f, -0.4f, -0.1f)),
            testTransformPos + _testTransform.TransformVector(new Vector3( 0.4f,  0.4f, -0.1f)),
            _testTransform.TransformVector(new Vector3(0.0f, 0.0f, 0.2f))
        };

        // Ensure the portal does not intersect walls.
        Collider[] intersections = { };
        Physics.OverlapBoxNonAlloc(checkPositions[0], checkExtents, intersections, _testTransform.rotation,
            _placementMask);

        switch (intersections.Length)
        {
            case > 1:
            // We are allowed to intersect the old portal position.
            case 1 when intersections[0] != _collider:
                return false;
        }

        // Ensure the portal corners overlap a surface.
        bool isOverlapping = true;

        for(int i = 1; i < checkPositions.Length - 1; ++i)
        {
            isOverlapping &= Physics.Linecast(checkPositions[i], checkPositions[i] + checkPositions[^1], 
                _placementMask);
        }
        return isOverlapping;
    }

    public void RemovePortal()
    {
        gameObject.SetActive(false);
        IsPlaced = false;
    }
}

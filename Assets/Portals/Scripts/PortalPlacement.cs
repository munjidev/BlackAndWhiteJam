using UnityEngine;

public class PortalPlacement : MonoBehaviour
{
    [SerializeField] private PortalPair _portals;
    [SerializeField] private LayerMask _layerMask;

    private PlayerMovement _playerMovement;
    private Camera _camera;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _camera = Camera.main;
    }

    private void Start()
    {
        Vector3 trPos = transform.position;
        
        // Attempt to place second portal
        FirePortal(1, trPos, new Vector3(1.5f, -12f, 1.5f) - trPos, Mathf.Infinity);
    }

    private void Update()
    {
        Vector3 trPos = transform.position;
        Vector3 trPosCamera = _camera.transform.position;

        // Perform raycast towards mouse point.
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, _layerMask);
        
        Debug.DrawRay(trPos, hitInfo.point - trPos, Color.red);
        Debug.DrawRay(trPosCamera, hitInfo.point - trPosCamera, Color.blue);
        
        if (Input.GetButtonDown("Fire1"))
        {
            FirePortal(0, trPos, hitInfo.point - trPos, Mathf.Infinity);
        }
    }

    private void FirePortal(int portalID, Vector3 pos, Vector3 dir, float distance)
    {
        // Obtain mouse position to differentiate between desired hit point and actual hit point.
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        
        // Perform raycast ignoring layer mask.
        Physics.Raycast(ray, out RaycastHit mouseHit, distance, _layerMask);

        RaycastHit hit;
        
        // Perform raycast from player position to desired point.
        if (portalID == 0)
        {
            Physics.Raycast(pos, dir, out RaycastHit hitInfo, distance, _layerMask);
            hit = hitInfo;
        }
        else
        {
            // Ignore other layer masks except bottom mask
            Physics.Raycast(pos, dir, out RaycastHit hitInfo, distance, 
                1 << LayerMask.NameToLayer("Bottom"));
            hit = hitInfo;
        }
        
        /* Return if:
            * Hit surface is null
            * Surface is not horizontal
            * Raycast is hitting a different point than mouse point (only when firing portal 0)
        */
        switch (portalID)
        { 
            case 0 when (ReferenceEquals(null, hit.collider) || hit.normal != new Vector3(0.0f, 1.0f, 0.0f) 
                                                             || hit.point != mouseHit.point):
                return;
            
            case 1 when (ReferenceEquals(null, hit.collider) || hit.normal != new Vector3(0.0f, 1.0f, 0.0f)):
                return;
        }
        
        Quaternion cameraRotation = _playerMovement.TargetRotation;
        Vector3 portalRight = cameraRotation * Vector3.right;

        if (Mathf.Abs(portalRight.x) >= Mathf.Abs(portalRight.z))
        {
            portalRight = (portalRight.x >= 0) ? Vector3.right : -Vector3.right;
        }
        else
        {
            portalRight = (portalRight.z >= 0) ? Vector3.forward : -Vector3.forward;
        }

        // Set portal facing opposite to the hit surface normal and rotate based on camera look direction.
        Vector3 portalForward = -hit.normal;
        Vector3 portalUp = -Vector3.Cross(portalRight, portalForward);
        Quaternion portalRotation = Quaternion.LookRotation(portalForward, portalUp);

        // Attempt to place the portal.
        bool wasPlaced = _portals.Portals[portalID].PlacePortal(hit.collider, hit.point, portalRotation);

        if (wasPlaced)
        {
            Debug.Log("Portal " + portalID + " placed!");
        }
        else
        {
            Debug.Log("Portal " + portalID + " not placed!");
        }
    }
}
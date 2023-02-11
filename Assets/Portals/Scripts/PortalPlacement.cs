using System;
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
        // Have camera fire portal 1 at -7, 10, -7
        FirePortal(1, new Vector3(-7, 10, -7), Vector3.down, Mathf.Infinity);
    }

    private void Update()
    {
        Vector3 trPos = transform.position;
        Vector3 trPosCamera = _camera.transform.position;

        // Perform raycast towards desired point.
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, _layerMask);
        
        // Raycast visualization from player and character perspectives.
        Debug.DrawRay(trPos, hitInfo.point - trPos, Color.red);
        Debug.DrawRay(trPosCamera, hitInfo.point - trPosCamera, Color.blue);
        
        if(Input.GetButtonDown("Fire1"))
        {
            FirePortal(0, trPos, hitInfo.point - trPos, Mathf.Infinity);
        }
    }

    private void FirePortal(int portalID, Vector3 pos, Vector3 dir, float distance)
    {
        Physics.Raycast(pos, dir, out var hit, distance, _layerMask);
        Collider hitCollider = hit.collider;
        
        // Return if surface is not valid, a portal, or a ground surface.
        if (hitCollider == null || hitCollider.CompareTag("Portal") || !hitCollider.CompareTag("Ground")) return;
        
        // Else orient the portal according to camera look direction and surface direction.
        Quaternion cameraRotation = _playerMovement.TargetRotation;
        Vector3 portalRight = cameraRotation * Vector3.right;
            
        if(Mathf.Abs(portalRight.x) >= Mathf.Abs(portalRight.z))
        {
            portalRight = (portalRight.x >= 0) ? Vector3.right : -Vector3.right;
        }
        else
        {
            portalRight = (portalRight.z >= 0) ? Vector3.forward : -Vector3.forward;
        }

        Vector3 portalForward = -hit.normal;
        Vector3 portalUp = -Vector3.Cross(portalRight, portalForward);

        Quaternion portalRotation = Quaternion.LookRotation(portalForward, portalUp);
            
        // Attempt to place the portal.
        bool wasPlaced = _portals.Portals[portalID].PlacePortal(hit.collider, hit.point, portalRotation);

        if(wasPlaced)
        {
            // Do something
        }
    }
}

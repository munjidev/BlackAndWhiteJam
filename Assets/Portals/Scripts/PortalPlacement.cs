using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalPlacement : MonoBehaviour
{
    [SerializeField] private PortalPair portals;

    [SerializeField] private LayerMask layerMask;

    private Camera _camera;
    
    private PlayerMovement _playerMovement;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _camera = Camera.main;
    }

    // private void Start()
    // {   
    //     // Automatically place second portal at -7, -9.5, -7
    //     FirePortal(1, transform.position, new Vector3(-7f, -9.5f, -7f), Mathf.Infinity);
    // }

    private void Update()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, layerMask);
        Debug.DrawRay(transform.position, hitInfo.point - transform.position, Color.red);
        
        if(Input.GetButtonDown("Fire1"))
        {
            FirePortal(0, transform.position, hitInfo.point - transform.position, Mathf.Infinity);
        }
        // else if (Input.GetButtonDown("Fire2"))
        // {
        //     FirePortal(1, transform.position, hitInfo.point - transform.position, Mathf.Infinity);
        // }
    }

    private void FirePortal(int portalID, Vector3 pos, Vector3 dir, float distance)
    {
        Physics.Raycast(pos, dir, out var hit, distance, layerMask);

        if(hit.collider != null || hit.collider.CompareTag("Portal"))
        {

            // Orient the portal according to camera look direction and surface direction.
            var cameraRotation = _playerMovement.TargetRotation;
            var portalRight = cameraRotation * Vector3.right;
            
            if(Mathf.Abs(portalRight.x) >= Mathf.Abs(portalRight.z))
            {
                portalRight = (portalRight.x >= 0) ? Vector3.right : -Vector3.right;
            }
            else
            {
                portalRight = (portalRight.z >= 0) ? Vector3.forward : -Vector3.forward;
            }

            var portalForward = -hit.normal;
            var portalUp = -Vector3.Cross(portalRight, portalForward);

            var portalRotation = Quaternion.LookRotation(portalForward, portalUp);
            
            // Attempt to place the portal.
            bool wasPlaced = portals.Portals[portalID].PlacePortal(hit.collider, hit.point, portalRotation);

            if(wasPlaced)
            {
                // Do something
            }
        }
    }
}

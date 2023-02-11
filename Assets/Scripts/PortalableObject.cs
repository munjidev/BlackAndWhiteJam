using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PortalableObject : MonoBehaviour
{
    protected Collider _collider;
    
    private static readonly Quaternion halfTurn = Quaternion.Euler(0.0f, 180.0f, 0.0f);
    
    private Portal _inPortal;
    private Portal _outPortal;
    private Rigidbody _rigidbody;

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    public void SetIsInPortal(Portal inPortal, Portal outPortal, Collider wallCollider)
    {
        _inPortal = inPortal;
        _outPortal = outPortal;

        Physics.IgnoreCollision(_collider, wallCollider);
    }

    public void ExitPortal(Collider wallCollider)
    {
        Physics.IgnoreCollision(_collider, wallCollider, false);
    }

    public virtual void Warp()
    {
        Transform inTransform = _inPortal.transform;
        Transform outTransform = _outPortal.transform;
        
        // Teleport object to the portal's center.
        if (_inPortal.name == "Portal 2")
        {
            transform.position = inTransform.position + new Vector3(0f, 0.1f, 0f);
        }

        // Update position of object.
        Vector3 relativePos = inTransform.InverseTransformPoint(transform.position);
        relativePos = halfTurn * relativePos;
        transform.position = outTransform.TransformPoint(relativePos);

        // Update rotation of object.
        Quaternion relativeRot = Quaternion.Inverse(inTransform.rotation) * transform.rotation;
        
        // If the object is upside down, rotate it 180 degrees around the x and z axes.
        if (relativeRot.eulerAngles is { x: 180f, z: 180f })
        {
            relativeRot = halfTurn * relativeRot;
        }
        transform.rotation = outTransform.rotation * relativeRot;
        
        // Update velocity of rigidbody.
        Vector3 relativeVel = inTransform.InverseTransformDirection(_rigidbody.velocity);
        relativeVel = halfTurn * relativeVel;
        _rigidbody.velocity = outTransform.TransformDirection(relativeVel);

        // Swap portal references.
        (_inPortal, _outPortal) = (_outPortal, _inPortal);
    }
}
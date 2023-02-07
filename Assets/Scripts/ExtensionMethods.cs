using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    // Set standard coordinate system with skewed y axis
    private static Matrix4x4 _isometricMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    /// <summary>
    ///  Skews input by multiplying isometric coordinate system with player input.
    /// </summary>
    /// <param name="vector">Player input vector.</param>
    /// <returns>Returns skewed input. </returns>
    public static Vector3 ToIsometric(this Vector3 vector) => _isometricMatrix.MultiplyPoint3x4(vector);
}

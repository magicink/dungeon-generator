using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connector : MonoBehaviour
{
    [SerializeField] private Vector2 size = Vector2.one * 4.0f;
    private Vector2 _halfSize;

    private void Start()
    {
    }

    private void OnDrawGizmos()
    {
        _halfSize = size * 0.5f;
        Gizmos.color = Color.cyan;
        var offset = transform.position + Vector3.up * _halfSize.y;
        var normal = offset + Vector3.forward;
        Gizmos.DrawSphere(normal, 0.10f);
        Gizmos.DrawLine(offset, normal);
        var t = transform;
        var top = t.up * size.y;
        var halfWidth = t.right * _halfSize.x;
        var position = t.position;
        var topRight = position + top + halfWidth;
        var topLeft = position + top - halfWidth;
        var bottomRight = position + halfWidth;
        var bottomLeft = position - halfWidth;
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(topLeft, offset);
        Gizmos.DrawLine(topRight, offset);
        Gizmos.DrawLine(bottomRight, offset);
        Gizmos.DrawLine(bottomLeft, offset);
    }
}

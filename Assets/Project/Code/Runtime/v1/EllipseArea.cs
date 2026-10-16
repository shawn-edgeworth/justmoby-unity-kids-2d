
using UnityEngine;

[ExecuteAlways]
public class EllipseArea : MonoBehaviour
{
    [Min(0f)]
    public float radiusX = 100f;

    [Min(0f)]
    public float radiusY = 50f;

    public Vector2 centerOffset;

    public Color gizmoColor = Color.green;

    public bool ContainsPoint(Vector3 position)
    {
        if (radiusX <= 0f || radiusY <= 0f)
            return false;

        Vector3 localPoint = transform.InverseTransformPoint(position);

        float x = (localPoint.x - centerOffset.x) / radiusX;
        float y = (localPoint.y - centerOffset.y) / radiusY;

        return x * x + y * y <= 1f;
    }
}

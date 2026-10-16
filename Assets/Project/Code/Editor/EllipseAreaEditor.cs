
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EllipseArea))]
public class EllipseAreaEditor : Editor
{
    private const int Segments = 96;
    private const float LineThickness = 8f;
    private const float HandleSize = 0.10f;

    private void OnSceneGUI()
    {
        var area = (EllipseArea)target;
        Transform t = area.transform;

        Vector3 center = t.TransformPoint(
            new Vector3(area.centerOffset.x, area.centerOffset.y, 0f));

        Vector3 axisX = t.right.normalized;
        Vector3 axisY = t.up.normalized;

        Handles.color = area.gizmoColor;

        DrawEllipse(area, t);

        float handleSize =
            HandleUtility.GetHandleSize(center) * HandleSize;

        // Правая ручка
        DrawHandle(area, center, axisX, handleSize, Handle.Right);

        // Левая ручка
        DrawHandle(area, center, -axisX, handleSize, Handle.Left);

        // Верхняя ручка
        DrawHandle(area, center, axisY, handleSize, Handle.Top);

        // Нижняя ручка
        DrawHandle(area, center, -axisY, handleSize, Handle.Bottom);
    }

    private static void DrawEllipse(EllipseArea area, Transform t)
    {
        Vector3[] points = new Vector3[Segments + 1];

        Vector3 localCenter = new Vector3(
            area.centerOffset.x,
            area.centerOffset.y,
            0f);

        for (int i = 0; i <= Segments; i++)
        {
            float angle = i * Mathf.PI * 2f / Segments;

            Vector3 localPoint = localCenter + new Vector3(
                Mathf.Cos(angle) * area.radiusX,
                Mathf.Sin(angle) * area.radiusY,
                0f);

            points[i] = t.TransformPoint(localPoint);
        }

        Handles.color = area.gizmoColor;
        Handles.DrawAAPolyLine(LineThickness, points);
    }

    private enum Handle
    {
        Right,
        Left,
        Top,
        Bottom
    }

    private static void DrawHandle(
        EllipseArea area,
        Vector3 center,
        Vector3 direction,
        float size,
        Handle handle)
    {
        Transform t = area.transform;

        float radius = handle is Handle.Right or Handle.Left
            ? area.radiusX
            : area.radiusY;

        Vector3 start = center + direction * radius *
            (handle is Handle.Right or Handle.Left
                ? t.TransformVector(Vector3.right).magnitude
                : t.TransformVector(Vector3.up).magnitude);

        EditorGUI.BeginChangeCheck();

        Vector3 moved = Handles.Slider(
            start,
            direction,
            size,
            Handles.DotHandleCap,
            0f);

        if (!EditorGUI.EndChangeCheck())
            return;

        Undo.RecordObject(area, "Resize Ellipse");

        Vector3 local = t.InverseTransformPoint(moved);

        float x = area.centerOffset.x;
        float y = area.centerOffset.y;

        switch (handle)
        {
            case Handle.Right:
                {
                    float fixedEdge = x - area.radiusX;
                    float newEdge = Mathf.Max(fixedEdge, local.x);
                    area.radiusX = (newEdge - fixedEdge) * 0.5f;
                    area.centerOffset.x = (newEdge + fixedEdge) * 0.5f;
                    break;
                }

            case Handle.Left:
                {
                    float fixedEdge = x + area.radiusX;
                    float newEdge = Mathf.Min(fixedEdge, local.x);
                    area.radiusX = (fixedEdge - newEdge) * 0.5f;
                    area.centerOffset.x = (newEdge + fixedEdge) * 0.5f;
                    break;
                }

            case Handle.Top:
                {
                    float fixedEdge = y - area.radiusY;
                    float newEdge = Mathf.Max(fixedEdge, local.y);
                    area.radiusY = (newEdge - fixedEdge) * 0.5f;
                    area.centerOffset.y = (newEdge + fixedEdge) * 0.5f;
                    break;
                }

            case Handle.Bottom:
                {
                    float fixedEdge = y + area.radiusY;
                    float newEdge = Mathf.Min(fixedEdge, local.y);
                    area.radiusY = (fixedEdge - newEdge) * 0.5f;
                    area.centerOffset.y = (newEdge + fixedEdge) * 0.5f;
                    break;
                }
        }

        EditorUtility.SetDirty(area);
    }
}

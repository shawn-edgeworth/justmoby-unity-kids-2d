
using Assets.Project.Code.Runtime.Presentation;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HoleView))]
public class HoleViewEditor : Editor
{
    private const int Segments = 96;
    private const float LineThickness = 8f;
    private const float HandleSize = 0.10f;

    private void OnSceneGUI()
    {
        var view = (HoleView)target;
        Transform t = view.transform;

        Vector3 center = t.TransformPoint(
            new Vector3(view.CenterOffset.x, view.CenterOffset.y, 0f));

        Vector3 axisX = t.right.normalized;
        Vector3 axisY = t.up.normalized;

        Handles.color = view.GizmoColor;

        DrawEllipse(view, t);

        float handleSize =
            HandleUtility.GetHandleSize(center) * HandleSize;

        // Правая ручка
        DrawHandle(view, center, axisX, handleSize, Handle.Right);

        // Левая ручка
        DrawHandle(view, center, -axisX, handleSize, Handle.Left);

        // Верхняя ручка
        DrawHandle(view, center, axisY, handleSize, Handle.Top);

        // Нижняя ручка
        DrawHandle(view, center, -axisY, handleSize, Handle.Bottom);
    }

    private static void DrawEllipse(HoleView area, Transform t)
    {
        Vector3[] points = new Vector3[Segments + 1];

        Vector3 localCenter = new Vector3(
            area.CenterOffset.x,
            area.CenterOffset.y,
            0f);

        for (int i = 0; i <= Segments; i++)
        {
            float angle = i * Mathf.PI * 2f / Segments;

            Vector3 localPoint = localCenter + new Vector3(
                Mathf.Cos(angle) * area.RadiusX,
                Mathf.Sin(angle) * area.RadiusY,
                0f);

            points[i] = t.TransformPoint(localPoint);
        }

        Handles.color = area.GizmoColor;
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
        HoleView view,
        Vector3 center,
        Vector3 direction,
        float size,
        Handle handle)
    {
        Transform t = view.transform;

        float radius = handle is Handle.Right or Handle.Left
            ? view.RadiusX
            : view.RadiusY;

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

        Undo.RecordObject(view, "Resize Ellipse");

        Vector3 local = t.InverseTransformPoint(moved);

        Vector2 centerOffset = view.CenterOffset;
        float radiusX = view.RadiusX;
        float radiusY = view.RadiusY;

        switch (handle)
        {
            case Handle.Right:
                {
                    float fixedEdge = centerOffset.x - view.RadiusX;
                    float newEdge = Mathf.Max(fixedEdge, local.x);
                    radiusX = (newEdge - fixedEdge) * 0.5f;
                    centerOffset.x = (newEdge + fixedEdge) * 0.5f;
                    break;
                }

            case Handle.Left:
                {
                    float fixedEdge = centerOffset.x + view.RadiusX;
                    float newEdge = Mathf.Min(fixedEdge, local.x);
                    radiusX = (fixedEdge - newEdge) * 0.5f;
                    centerOffset.x = (newEdge + fixedEdge) * 0.5f;
                    break;
                }

            case Handle.Top:
                {
                    float fixedEdge = centerOffset.y - view.RadiusY;
                    float newEdge = Mathf.Max(fixedEdge, local.y);
                    radiusY = (newEdge - fixedEdge) * 0.5f;
                    centerOffset.y = (newEdge + fixedEdge) * 0.5f;
                    break;
                }

            case Handle.Bottom:
                {
                    float fixedEdge = centerOffset.y + view.RadiusY;
                    float newEdge = Mathf.Min(fixedEdge, local.y);
                    radiusY = (fixedEdge - newEdge) * 0.5f;
                    centerOffset.y = (newEdge + fixedEdge) * 0.5f;
                    break;
                }
        }

        view.SetData(radiusX, radiusY, centerOffset);
        EditorUtility.SetDirty(view);
    }
}

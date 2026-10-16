using System.Collections.Generic;
using Assets.Project.Code.Runtime.v1;
using UnityEngine;

public class CubeStackView : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;

    private List<DynamicCubeView> cubes = new();
    private List<float> cubeOffsets = new();

    private float Bottom => rectTransform.position.y - rectTransform.rect.height * 0.5f;
    private float Top => rectTransform.position.y + rectTransform.rect.height * 0.5f;

    public bool TryAddCube(DynamicCubeView cube)
    {
        if (!CanAddCube(new Vector2(cube.Width, cube.Height)))
        {
            return false;
        }

        cube.SetInStack(true);
        cube.OnDragStarted += RemoveCube;

        cubes.Add(cube);
        cubeOffsets.Add(GetRandomOffsetForCube(cube.Width));
        UpdateCubesPositions();

        return true;
    }

    private void RemoveCube(DynamicCubeView view)
    {
        view.OnDragStarted -= RemoveCube;
        int index = cubes.IndexOf(view);
        if (index >= 0)
        {
            cubes.RemoveAt(index);
            cubeOffsets.RemoveAt(index);
            UpdateCubesPositions();
        }
    }

    public bool IsInside(Vector2 position)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, position);
    }

    public bool CanAddCube(Vector2 size)
    {
        if (cubes.Count == 0)
        {
            return Bottom + size.y <= Top;
        }

        var lastCube = cubes[^1];
        return lastCube.Position.y + lastCube.Height + size.y <= Top;
    }

    private void UpdateCubesPositions()
    {
        var currentY = Bottom;
        for (int i = 0; i < cubes.Count; i++)
        {
            var cube = cubes[i];
            var yPosition = currentY;
            cube.UpdatePosition(new Vector2(rectTransform.position.x + cubeOffsets[i], yPosition));
            currentY += cube.Height;
        }
    }

    private float GetRandomOffsetForCube(float width)
    {
        return Random.Range(-width * 0.5f, width * 0.5f);
    }
}
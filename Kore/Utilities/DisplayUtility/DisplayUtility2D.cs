using UnityEngine;

public static class DisplayUtility2D
{
    public static float GetHeight(Camera camera)
    {
        return camera.orthographicSize * 2;
    }

    public static float GetWidth(Camera camera)
    {
        return (camera.orthographicSize * 2) * camera.aspect;
    }

    public static float GetExtremeLeft(Camera camera)
    {
        float halfHeight = camera.orthographicSize;
        float halfWidth = halfHeight * camera.aspect;
        Vector3 cameraPosition = camera.gameObject.transform.position;
        return cameraPosition.x - halfWidth;
    }
    public static float GetExtremeRight(Camera camera)
    {
        float halfHeight = camera.orthographicSize;
        float halfWidth = halfHeight * camera.aspect;
        Vector3 cameraPosition = camera.gameObject.transform.position;
        return cameraPosition.x + halfWidth;
    }
}

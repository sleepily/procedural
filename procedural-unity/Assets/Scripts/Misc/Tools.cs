using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tools
{
    public static Vector3 ClampVector3(Vector3 input, Vector3 min, Vector3 max)
    {
        Vector3 output = new Vector3();

        output.x = Mathf.Clamp(input.x, min.x, max.x);
        output.y = Mathf.Clamp(input.y, min.y, max.y);
        output.z = Mathf.Clamp(input.z, min.z, max.z);

        return output;
    }

    public static Vector3 ClampVector3(Vector3 input, float min, float max)
    {
        Vector3 output = new Vector3();

        output.x = Mathf.Clamp(input.x, min, max);
        output.y = Mathf.Clamp(input.y, min, max);
        output.z = Mathf.Clamp(input.z, min, max);

        return output;
    }

    public static Vector3Int ClampVector3Int(Vector3Int input, int min, int max)
    {
        Vector3Int output = new Vector3Int();

        output.x = Mathf.Clamp(input.x, min, max);
        output.y = Mathf.Clamp(input.y, min, max);
        output.z = Mathf.Clamp(input.z, min, max);

        return output;
    }

    public static Vector3Int RandomVector3Int(int min, int max)
    {
        Vector3Int output = new Vector3Int();

        output.x = Random.Range(min, max);
        output.y = Random.Range(min, max);
        output.z = Random.Range(min, max);

        return output;
    }

    public static Vector3Int RandomVector3Int(Vector3Int min, Vector3Int max)
    {
        Vector3Int output = new Vector3Int();

        output.x = Random.Range(min.x, max.x);
        output.y = Random.Range(min.y, max.y);
        output.z = Random.Range(min.z, max.z);

        return output;
    }
}

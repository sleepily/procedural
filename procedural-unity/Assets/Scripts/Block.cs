using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public enum BlockType
    {
        None,
        Dirt,
        Grass,
        Sand,
        Stone,
        Water,
        Wood,
        Leaves
    }

    public BlockType type;
    public GameObject cube;
    public MeshRenderer meshRenderer;
    public Vector3 position;
    public bool skipRender = true; // skip rendering the block by default

    void Start()
    {
        meshRenderer = cube.GetComponent<MeshRenderer>();
    }

    public void SetPosition(Vector3 position)
    {
        this.position = position;
        this.cube.transform.position = this.position;
    }

    public void SetMaterial(Material material)
    {
        this.meshRenderer.material = material;
    }
}

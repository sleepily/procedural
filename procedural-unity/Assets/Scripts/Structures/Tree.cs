using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree
{
    Vector2Int trunkMin = new Vector2Int(1, 3);
    Vector2Int trunkMax = new Vector2Int(2, 5);

    Vector2Int topMin = new Vector2Int(3, 3);
    Vector2Int topMax = new Vector2Int(5, 5);

    public Vector3Int horizontalTrunkOffset = new Vector3Int(2, 0, 2);
    public Block.BlockType[,,] treeBlocks;
    public Vector3Int position;

    public Tree()
    {
        Calculate();
    }

    public void SetPosition(Vector3Int position)
    {
        this.position = position;
    }

    void Calculate()
    {
        // randomizing the tree components //@TODO: implement this
        /*
        Vector2Int trunk = new Vector2Int(Random.Range(trunkMin.x, trunkMax.x), Random.Range(trunkMin.y, trunkMax.y));
        Vector2Int top = new Vector2Int(Random.Range(topMin.x, topMax.x), Random.Range(topMin.y, topMax.y));
        tree = new Block.BlockType[top.x, trunk.y + top.y, top.x];
        */

        // initialize tree array
        treeBlocks = new Block.BlockType[5, 5, 5];

        // create tree trunk
        for (int trunkHeight = 0; trunkHeight < 4; trunkHeight++)
            treeBlocks[horizontalTrunkOffset.x, trunkHeight, horizontalTrunkOffset.z] = Block.BlockType.Wood;

        // create tree leaves
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 2; y++)
                for (int z = 0; z < 5; z++)
                {
                    // don't make the outer top ring leaves
                    if (y == 1)
                        if (x == 0 || z == 0 || x == 4 || z == 4)
                        {
                            treeBlocks[x, y + 3, z] = Block.BlockType.None;
                            continue;
                        }

                    // don't make the topmost trunk block leaves
                    if (x == 2 && z == 2)
                        if (y == 0)
                            continue;

                    treeBlocks[x, y + 3, z] = Block.BlockType.Leaves;
                }
    }

    public Block.BlockType[,,] Spawn()
    {
        return treeBlocks;
    }
}

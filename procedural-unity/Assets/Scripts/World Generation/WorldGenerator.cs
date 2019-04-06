using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{    
    [Header("Perlin Noise")]
    public float seed = 0f;
    public float perlinStrength = 50f;
    public float perlinDivisor = 20f;

    [Header("World")]
    public Vector3Int chunkSize = Vector3Int.one * 10;
    Block[,,] chunk;

    public int stoneOffset = 12;
    public int waterHeight = 13;
    public Material[] materials = new Material[6];

    [Header("Structures")]
    public int treeSeed = 56728323;
    public float treeRatePerGrass = .06f;
    public int minTreeSpacing = 4;

    [Header("Rendering")]
    public Vector3 cameraOffset = new Vector3(-20, 0, -20);
    public bool renderOuterCubes = true;

    [Header("Animation")]
    public bool animated = true;
    float timer;
    public float animationTimeInterval = 2.0f;
    public float animationSeedInterval = 0.01f;

    Tree[] trees;
    
    void Start()
    {
        timer = Time.time;

        Regenerate();
    }
    
    void Update()
    {
        if (animationTimeInterval < 1.8f)
            if (Time.time > timer + animationTimeInterval)
            {
                Regenerate();

                timer += animationTimeInterval;
            }
    }

    public void Regenerate()
    {
        DeleteCubes();
        UpdateChunk();

        CalculateCameraPosition();

        CalculateBlockTypes();
        GenerateTrees();
        RenderCubes();
    }

    void DeleteCubes()
    {
        if (chunk != null)
            foreach (Block block in chunk)
                Destroy(block.cube);
    }

    void UpdateChunk()
    {
        if (animated)
            if (animationTimeInterval < 1.8f) // animation interval slider from 2 to .1, ignore when too slow
                seed += animationSeedInterval;
        
        chunk = new Block[chunkSize.x, chunkSize.y, chunkSize.z];

        for (int x = 0; x < chunkSize.x; x++)
            for (int y = 0; y < chunkSize.y; y++)
                for (int z = 0; z < chunkSize.z; z++)
                    chunk[x, y, z] = new Block();
    }

    /*
     * Automatic camera position calculation to always make the generated world centered
     */
    void CalculateCameraPosition()
    {
        Camera.main.transform.position =
            new Vector3(cameraOffset.x + chunkSize.x / 2, cameraOffset.y, cameraOffset.z + chunkSize.z / 2)
            + Vector3.up * (int)((this.perlinStrength / 2) + 10);
    }

    /*
     * Calculate all block types inside of this chunk
     */
    void CalculateBlockTypes()
    {
        for (int x = 0; x < chunkSize.x; x++)
            for (int z = 0; z < chunkSize.z; z++)
            {
                // get perlin noise function value
                int y = Algorithms.CalculatePerlinNoiseInt(x, z, seed, perlinDivisor, perlinStrength);

                // clamp between 0 and world height so nothing goes out of bounds
                y = Mathf.Clamp(y, 0, chunkSize.y - 1);

                // set perlin noise output to grass blocks
                chunk[x, y, z].type = Block.BlockType.Grass;
                
                // set everything underground to be dirt, then stone after a certain depth
                for (int belowGrass = 0; belowGrass < y; belowGrass++)
                {
                    chunk[x, belowGrass, z].type = Block.BlockType.Dirt;

                    if (belowGrass < y - stoneOffset)
                        chunk[x, belowGrass, z].type = Block.BlockType.Stone;
                }

                // set all empty blocks between grass and water height to be water
                for (int waterY = y + 1; waterY < waterHeight; waterY++)
                    if (chunk[x, waterY, z].type == Block.BlockType.None)
                    {
                        chunk[x, waterY, z].type = Block.BlockType.Water;

                        // set blocks below water to be sand
                        chunk[x, y, z].type = Block.BlockType.Sand;
                    }
            }

        // replace blocks around water with sand
        for (int x = 0; x < chunkSize.x; x++)
            for (int y = 0; y < chunkSize.y; y++)
                for (int z = 0; z < chunkSize.z; z++)
                {
                    if (chunk[x, y, z].type == Block.BlockType.Water)
                        for (int xOffset = -1; xOffset <= 1; xOffset++)
                            for (int yOffset = -1; yOffset <= 0; yOffset++)
                                for (int zOffset = -1; zOffset <= 1; zOffset++)
                                {
                                    // clamp the position to not leave the chunk
                                    Vector3Int clamped = new Vector3Int(x + xOffset, y + yOffset, z + zOffset);
                                    clamped = Tools.ClampVector3Int(clamped, 0, chunkSize.x - 1);
                                    
                                    // replace non-water blocks with sand
                                    if (chunk[clamped.x, clamped.y, clamped.z].type != Block.BlockType.Water)
                                        chunk[clamped.x, clamped.y, clamped.z].type = Block.BlockType.Sand;
                                }
                }
    }

    void GenerateTrees()
    {
        // get tree positions from algorithm
        Vector3Int[] treePositions = GetTreePositions();

        // create array as big as amount of tree positions present
        trees = new Tree[treePositions.Length];

        // spawn two trees at non random positions
        for (int treeIndex = 0; treeIndex < trees.Length; treeIndex++)
        {
            trees[treeIndex] = new Tree();
            trees[treeIndex].SetPosition(treePositions[treeIndex]);
        }

        // spawn the trees
        foreach (Tree tree in trees)
        {
            // calculate actual world offset
            Vector3Int worldOffset = tree.position - tree.horizontalTrunkOffset;

            for (int treeBlockX = 0; treeBlockX < 5; treeBlockX++)
                for (int treeBlockY = 0; treeBlockY < 5; treeBlockY++)
                    for (int treeBlockZ = 0; treeBlockZ < 5; treeBlockZ++)
                    {
                        // calculate actual world coordinates
                        int worldTreeBlockX = tree.position.x - tree.horizontalTrunkOffset.x + treeBlockX;
                        int worldTreeBlockY = tree.position.y - tree.horizontalTrunkOffset.y + treeBlockY;
                        int worldTreeBlockZ = tree.position.z - tree.horizontalTrunkOffset.z + treeBlockZ;

                        // prevent replacing world blocks with air
                        if (tree.treeBlocks[treeBlockX, treeBlockY, treeBlockZ] == Block.BlockType.None)
                            continue;

                        // don't place a block if it's out of bounds
                        if
                        (
                            worldTreeBlockX >= chunkSize.x ||
                            worldTreeBlockY >= chunkSize.y ||
                            worldTreeBlockZ >= chunkSize.z ||
                            worldTreeBlockX < 0 ||
                            worldTreeBlockY < 0 ||
                            worldTreeBlockZ < 0
                        )
                            continue;

                        // replace world block with tree block
                        chunk[worldTreeBlockX, worldTreeBlockY, worldTreeBlockZ].type = tree.treeBlocks[treeBlockX, treeBlockY, treeBlockZ];
                    }
        }
    }

    Vector3Int[] GetTreePositions()
    {
        int grassBlockCounter = 0;

        foreach (Block block in chunk)
        {
            if (block.type == Block.BlockType.Grass)
                grassBlockCounter++;
        }

        // experiment 1: amount of trees relative to amount of grass blocks
        int treeAmount = (int)(grassBlockCounter * treeRatePerGrass);


        List<Vector3Int> treePositions = new List<Vector3Int>();

        int tries = 1;
        // completely randomize tree positions
        for (int treeIndex = 0; treeIndex < treeAmount; treeIndex++)
        {
            // generate a random position based on the tree seed, world seed and chunk size
            int treeSeedX = (int)(seed + treeSeed * (treeIndex + 2 * tries)) % chunkSize.x;
            int treeSeedZ = (int)(seed + treeSeed * (treeIndex + 2 * (tries * 1.2f)) / 3) % chunkSize.z;

            int y = 1 + Algorithms.CalculatePerlinNoiseInt(treeSeedX, treeSeedZ, seed, perlinDivisor, perlinStrength);

            // create vector for storing the tree's position
            Vector3Int newTreePosition = new Vector3Int(treeSeedX, y, treeSeedZ);

            // log the new tree's positions and tree index
            // Debug.Log(string.Format("Tree {0} at {1}", treeIndex, newTreePosition.ToString()));

            // check if the tree should be respawned
            // 1. if the tree is in water
            bool respawn = (chunk[treeSeedX, y, treeSeedZ].type == Block.BlockType.Water);

            // 2. if the tree is inside another tree
            foreach (Vector3Int otherTreePosition in treePositions)
            {
                if (newTreePosition.x >= otherTreePosition.x - minTreeSpacing && newTreePosition.x <= otherTreePosition.x + minTreeSpacing &&
                    newTreePosition.z >= otherTreePosition.z - minTreeSpacing && newTreePosition.z <= otherTreePosition.z + minTreeSpacing)
                {
                    respawn = true;
                    break;
                }
            }

            if (respawn)
            {
                // increase tries variable to change new tree's position
                tries++;

                // retry tree at this array index
                treeIndex--;
                continue;
            }
            
            // set the new y value
            treePositions.Add(newTreePosition);

            // reset tries for next tree
            tries = 1;
        }
        
        return treePositions.ToArray();
    }

    /*
     * Instantiate cubes with respective materials for all block data inside of the chunks array
     */
    void RenderCubes()
    {
        for (int x = (int)chunkSize.x - 1; x >= 0; x--)
            for (int y = (int)chunkSize.y - 1; y >= 0; y--)
                for (int z = (int)chunkSize.z - 1; z >= 0; z--)
                {
                    // if air, skip block
                    if (chunk[x, y, z].type == Block.BlockType.None)
                    {
                        chunk[x, y, z].skipRender = true;
                        continue;
                    }

                    // if there is air around this block, render it
                    for (int xOffset = -1; xOffset <= 1; xOffset++)
                        for (int yOffset = -1; yOffset <= 1; yOffset++)
                            for (int zOffset = -1; zOffset <= 1; zOffset++)
                            {
                                // clamp the position to not leave the chunk
                                int clampedX = (int)Mathf.Clamp(x + xOffset, 0, chunkSize.x - 1);
                                int clampedY = (int)Mathf.Clamp(y + yOffset, 0, chunkSize.y - 1);
                                int clampedZ = (int)Mathf.Clamp(z + zOffset, 0, chunkSize.z - 1);

                                // don't check for the block itself
                                if (clampedX == 0 && clampedY == 0 && clampedZ == 0)
                                    continue;

                                // set the skip flag false if there is air around the block
                                if
                                (
                                    chunk[clampedX, clampedY, clampedZ].type == Block.BlockType.None ||
                                    chunk[clampedX, clampedY, clampedZ].type == Block.BlockType.Water
                                )
                                {
                                    chunk[x, y, z].skipRender = false;
                                    break;
                                }
                            }

                    // render all water
                    if (chunk[x, y, z].type == Block.BlockType.Water)
                        chunk[x, y, z].skipRender = false;

                    // if the cube is at the edge of the world, render it
                    if (renderOuterCubes)
                    {
                        if
                        (
                            x == 0 || y == 0 || z == 0 ||
                            x == chunkSize.x - 1 || y == chunkSize.y - 1 || z == chunkSize.z - 1
                        )
                            chunk[x, y, z].skipRender = false;
                    }

                    // if the block still has to be skipped, go to next block
                    if (chunk[x, y, z].skipRender)
                        continue;

                    // finally render the cube
                    chunk[x, y, z].cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

                    chunk[x, y, z].cube.transform.SetParent(this.gameObject.transform);

                    chunk[x, y, z].cube.transform.position =
                        Vector3.right   * x +
                        Vector3.up      * y +
                        Vector3.forward * z;

                    chunk[x, y, z].cube.GetComponent<MeshRenderer>().material = materials[(int)chunk[x, y, z].type];
                }
    }

    /*
     * Set methods for the UI sliders
     */
    public void SetSeed(float value)
    {
        this.seed = value;

        Regenerate();
    }

    public void SetSize(float value)
    {
        DeleteCubes();

        this.chunkSize = new Vector3Int((int)value, (int)this.perlinStrength, (int)value);

        Regenerate();
    }

    public void SetDivisor(float value)
    {
        // don't divide by zero
        if (value > -.01f && value < .01f)
            value = .01f;

        this.perlinDivisor = value;

        Regenerate();
    }

    public void SetStrength(float value)
    {
        this.perlinStrength = (int)value;
        this.chunkSize.y = (int)value;

        Regenerate();
    }

    public void SetAnimationSpeed(float value)
    {
        this.animationTimeInterval = value;
    }
}

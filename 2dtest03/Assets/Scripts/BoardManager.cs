using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
    [Serializable]
    public class Count
    {
        public int maximum;
        public int minimum;

        public Count(int min, int max)
        {
            maximum = max;
            minimum = min;
        }
    }

    private int columns = 16;
    private int rows = 9;
    public Count wallCount = new Count(5,9); 
    public Count foodCount = new Count(1,5);
    public GameObject exit;
    public GameObject[] floorTiles;
    public GameObject[] wallTiles;
    public GameObject[] foodTiles;
    public GameObject[] enemyTiles;
    public GameObject[] outerWallTiles;

    private Transform boardHolder;
    private List<Vector3> gridPositions = new List<Vector3>();

    //MapGenerator start from here

    public int width;
    public int height;

    public string seed;
    public bool useRandomSeed;

    [Range(0, 100)]
    public int randomFillPercent;

    int[,] map;

    //-----------------------------------

    void InitialiseList()//获取可用tile的列表
    {
        gridPositions.Clear();

        //for (int x = 1; x < columns - 1; x++)
        //{
        //    for(int y = 1; y < rows - 1; y++)
        //    {
        //        gridPositions.Add(new Vector3(x,y,0f));
        //    }
        //}
        if (map != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if(map[x, y] == 0)
                    {
                        gridPositions.Add(new Vector3(x, y, 0f));
                    }
                }
            }
        }
    }

    //放置地板以及墙
    void BoardSetup()
    {
        boardHolder = new GameObject("Board").transform;
        //for (int x = -1; x < columns + 1; x++)
        //{
        //    for (int y = -1; y < rows + 1; y++)
        //    {
        //        GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
        //        if (x == -1 || x == columns || y == -1 || y == rows)
        //        {
        //            toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
        //        }
        //        GameObject instance = Instantiate(toInstantiate,new Vector3(x,y,0f),Quaternion.identity) as GameObject;

        //        instance.transform.SetParent(boardHolder);
        //    }
        //}
        if (map != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    GameObject toInstantiate;
                    GameObject instance;
                    if (map[x, y] == 0)
                    {
                        toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
                        instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

                        instance.transform.SetParent(boardHolder);
                    }

                    if (map[x, y] == 1 )
                    {
                        if ((x > 0&& map[x-1, y] == 0)|| (x < width-1 && map[x + 1, y] == 0) || (y > 0 && map[x, y - 1] == 0) || (y < height - 1 && map[x, y + 1] == 0)|| (x > 0 && y>0&& map[x - 1, y-1] == 0) || (x >0 && y<height-1&&map[x - 1, y+1] == 0) || (x<width -1 &&y > 0 && map[x+1, y - 1] == 0) || (x<width-1&&y < height - 1 && map[x+1, y + 1] == 0))
                        {
                            {
                                toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                                instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
                                instance.transform.SetParent(boardHolder);
                            }

                        }
                    }
                }
            }
        }
    }

    //返回一个随机的可用空间并将其从list删除
    Vector3 RandomPosition()
    {
        int randomIndex = Random.Range(0,gridPositions.Count);
        Vector3 randomPosition = gridPositions[randomIndex];
        gridPositions.RemoveAt(randomIndex);
        return randomPosition;
    }

    //将指定类型和个数范围的Tile随机放置于可用空间中
    void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)//
    {
        int objectCount = Random.Range(minimum, maximum + 1);
        for(int i = 0; i < objectCount; i++)
        {
            Vector3 randomPosition = RandomPosition();
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
            Instantiate(tileChoice, randomPosition, Quaternion.identity);
        }
    }

    void LayoutExitPlayer()
    {
        Vector3 exitPosition = gridPositions[gridPositions.Count - 1];
        gridPositions.RemoveAt(gridPositions.Count - 1);
        Vector3 playerPosition = gridPositions[0];
        gridPositions.RemoveAt(0);

        Instantiate(exit, exitPosition, Quaternion.identity);
        GameObject.Find("Player").transform.position = playerPosition;

    }

    public void SetupScene(int level)
    {
        GenerateMap();//----------------------------------
        BoardSetup();
        InitialiseList();
        LayoutExitPlayer();//放置玩家和出口的位置
        LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum);
        LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);
        int enemyCount = (int)Mathf.Log(level, 2f);
        LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);
        //Instantiate(exit, new Vector3(columns - 1, rows - 1, 0f), Quaternion.identity);
    }
    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();

        for (int i = 0; i < 5; i++)
        {
            SmoothMap();
        }

        //ProcessMap0();
        //ProcessMap1();
    }

    //Type为1(黑)的区域的集合中，对于Size小于阈值的区域，将这些区域的所有tile的Type置为0
    void ProcessMap0()
    {
        List<List<Coord>> roomRegions = GetRegions(1);

        int roomThresholdSize = 50;
        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }
    }

    //同上，只是白黑的处理逆转
    void ProcessMap1()
    {
        List<List<Coord>> roomRegions = GetRegions(0);

        int roomThresholdSize = 30;
        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
        }
    }

    //返回了指定Type的所有连通区域的集合
    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)//遍历List的简单方法
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    //返回了一个包括着startX,startY这个点的连通区域的所有tiles
    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (x == tile.tileX || y == tile.tileY))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }

    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());
        Debug.Log(seed);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                    map[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    map[x, y] = 0;

            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInMapRange(neighbourX, neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }
}

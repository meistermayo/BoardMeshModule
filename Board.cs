namespace BoardMeshModule
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class Board
    {
        public const float TILE_SIZE = 1.0f;

        const int width = 64;
        const int height = 64;

        // 2d arrays don't serialize 
        [SerializeField]
        protected Tile[] tiles = new Tile[width*height];

        public Tile[] GetTiles() => tiles;

        public void ResizeBoard(Vector2Int newSize)
        {
            Tile[] newTiles = new Tile[newSize.x * newSize.y];

            for (int x=0; x < tiles.GetLength(0); x++)
            {
                for (int y=0; y < tiles.GetLength(1); y++)
                {
                    if (x < newSize.x && y < newSize.y)
                    {
                        int i = x + y * newSize.x;
                        newTiles[i] = tiles[i];
                    }
                }
            }

            tiles = newTiles;
        }

        public bool CoordIsValid(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;
        public bool CoordIsWalkable(int x, int y) => CoordIsValid(x,y) && GetTile(x,y).tileSet != null;

        public void SetTileSet(int x, int y, BoardTileSet boardTileSet) => tiles[CoordToIndex(x, y)].tileSet = boardTileSet;


        int CoordToIndex(int x, int y) => x + y * width;
        public Tile GetTile(int x, int y) => tiles[CoordToIndex(x,y)];

        public int GetWidth() => width;//tiles.GetLength(0);
        public int GetHeight() => height;//tiles.GetLength(1);
        public Vector2Int GetSize() => new Vector2Int(GetWidth(), GetHeight());
        public void ClearBoard()
        {
            for(int i=0; i<tiles.Length; i++)
            {
                tiles[i].tileSet = null;
            }
        }

        // PosToCoord Floors what it gets
        // It assumes it is getting a Vector3 within a square, and just floors it to the square.
        public static Vector2Int PosToCoord(Vector3 pos)
        {
            pos /= TILE_SIZE;
            
            Vector2Int coord = new Vector2Int();
            coord.x = Mathf.FloorToInt(pos.x);
            coord.y = Mathf.FloorToInt(pos.z);

            return coord;
        }

        // DirToCoord Rounds what it gets
        // It assumes it is getting a transform axis that may be close to 1 or 0, and just rounds it to the nearest.
        public static Vector2Int DirToCoord(Vector3 dir)
        {
            Vector2Int coord = new Vector2Int();
            coord.x = Mathf.RoundToInt(dir.x);
            coord.y = Mathf.RoundToInt(dir.z);

            return coord;
        }

        public static Vector3 CoordToPos(int x, int y)
        {
            Vector3 pos = new Vector3(x, 0.0f, y);
            return pos * TILE_SIZE;
        }

        public static Vector3 WorldToBoard(Vector3 world)
        {
            Vector2Int coord = PosToCoord(world);
            world = CoordToPos(coord.x, coord.y);
            world += new Vector3(0.5f, 0, 0.5f) * Board.TILE_SIZE;

            return world;
        }
    }
}

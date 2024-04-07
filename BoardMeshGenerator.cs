namespace BoardMeshModule
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;

    public static class BoardMeshGenerator
    {
        class MeshData
        {
            public Dictionary<BoardTileSet, SubMeshData> subMeshDatas = new Dictionary<BoardTileSet, SubMeshData>();

            public void AddMesh(int x, int y, Board board, Mesh prefab, Quaternion rotation, float xScale = 1.0f)
            {
                BoardTileSet tileSet = board.GetTile(x, y).tileSet;

                SubMeshData subMeshData = GetOrCreateSubmeshData(tileSet);
                subMeshData.AddMesh(x, y, prefab, rotation, xScale);
            }

            public void ApplyToMeshRenderer(MeshRenderer meshRenderer)
            {
                Material[] materials = new Material[subMeshDatas.Count];
                int index = 0;

                foreach (var pair in subMeshDatas)
                {
                    Material material = pair.Key.material;
                    materials[index] = material;
                    index++;
                }
                meshRenderer.sharedMaterials = materials;
            }

            public void ApplyToMeshFilter(MeshFilter meshFilter)
            {
                SubMeshData totalMeshData = new SubMeshData();

                meshFilter.sharedMesh = new Mesh();
                meshFilter.sharedMesh.subMeshCount = subMeshDatas.Count;

                int index = 0;
                foreach (var pair in subMeshDatas)
                {
                    SubMeshData subMeshData = pair.Value;

                    // Accumulate the data
                    totalMeshData.vertList.AddRange(subMeshData.vertList);
                    totalMeshData.uvList.AddRange(subMeshData.uvList);
                    totalMeshData.triList.AddRange(subMeshData.triList);
                    totalMeshData.normalList.AddRange(subMeshData.normalList);

                    index++;
                }

                // ApplyToMeshFilter
                meshFilter.sharedMesh.SetVertices(totalMeshData.vertList);

                // setting submesh indices
                index = 0;
                foreach (var pair in subMeshDatas)
                {
                    SubMeshData subMeshData = pair.Value;
                    meshFilter.sharedMesh.SetTriangles(subMeshData.triList, index);

                    index++;
                }

                meshFilter.sharedMesh.SetUVs(0, totalMeshData.uvList);
                meshFilter.sharedMesh.SetNormals(totalMeshData.normalList);

                // Apply SubMeshDescriptors
                index = 0;
                int vertCount = 0;
                int triCount = 0;
                foreach (var pair in subMeshDatas)
                {
                    SubMeshData subMeshData = pair.Value;

                    // Start building the descriptor
                    SubMeshDescriptor subMeshDescriptor = new SubMeshDescriptor();
                    subMeshDescriptor.baseVertex = vertCount;

                    subMeshDescriptor.firstVertex = vertCount;
                    subMeshDescriptor.vertexCount = subMeshData.vertList.Count;

                    subMeshDescriptor.indexStart = triCount;
                    subMeshDescriptor.indexCount = subMeshData.triList.Count;

                    // Add the data
                    vertCount += subMeshData.vertList.Count;
                    triCount += subMeshData.triList.Count;

                    meshFilter.sharedMesh.SetSubMesh(index, subMeshDescriptor, MeshUpdateFlags.DontRecalculateBounds);

                    index++;
                }
            }

            public SubMeshData GetOrCreateSubmeshData(BoardTileSet tileSet)
            {
                if (!subMeshDatas.ContainsKey(tileSet))
                {
                    subMeshDatas.Add(tileSet, new SubMeshData());
                }

                tileSet.GetSubMeshes(out subMeshDatas[tileSet].boardTileSetData);

                return subMeshDatas[tileSet];
            }
        }

        class SubMeshData
        {
            public List<Vector3> vertList = new List<Vector3>();
            public List<int> triList = new List<int>();
            public List<Vector2> uvList = new List<Vector2>();
            public List<Vector3> normalList = new List<Vector3>();
            public BoardTileSetData boardTileSetData;

            public void AddMesh(int x, int y, Mesh prefab, Quaternion rotation, float xScale = 1.0f)
            {
                SubMeshData prefabData = new SubMeshData();
                prefab.GetVertices(prefabData.vertList);
                prefab.GetUVs(0, prefabData.uvList);
                prefab.GetNormals(prefabData.normalList);

                Vector3 offset = Board.CoordToPos(x, y) + new Vector3(0.5f, 0.0f, 0.5f);
                for (int i = 0; i < prefabData.vertList.Count; i++)
                {
                    Vector3 vert = prefabData.vertList[i];
                    vert.x *= xScale;
                    prefabData.vertList[i] = rotation * vert;
                    prefabData.vertList[i] += offset;
                }

                int[] tris = prefab.GetTriangles(0);

                // fix triangle facing
                if (xScale < 0)
                {
                    int count = 0;
                    for (int i = 0; i < tris.Length - 2; i++)
                    {
                        if (count == 0)
                        {
                            int tmp = tris[i + 1];
                            tris[i + 1] = tris[i + 2];
                            tris[i + 2] = tmp;
                        }
                        count = (count + 1) % 3;
                    }
                }

                for (int i = 0; i < tris.Length; i++)
                {
                    int index = tris[i];
                    triList.Add(vertList.Count + index);
                }
                foreach (var vertex in prefabData.vertList)
                {
                    vertList.Add(vertex);
                }
                foreach (var uv in prefabData.uvList)
                {
                    uvList.Add(uv);
                }
                foreach (var normal in prefabData.normalList)
                {
                    normalList.Add(normal);
                }
            }
        }

        static void HandleWallTile(int x, int y, int numTurns, Board board, MeshData meshData, SubMeshData subMeshData)
        {
            Vector3 wallSide = Vector3.forward;
            Vector3 right = Vector3.right;

            Quaternion rotation = Quaternion.Euler(Vector3.up * 90.0f * numTurns);

            wallSide = rotation * wallSide;
            right = rotation * right;

            int xx = x + Mathf.RoundToInt(wallSide.x);
            int yy = y + Mathf.RoundToInt(wallSide.z);

            if (!board.CoordIsWalkable(xx, yy))
            {
                int lx = xx + Mathf.RoundToInt(right.x);
                int ly = yy + Mathf.RoundToInt(right.z);

                int rx = xx - Mathf.RoundToInt(right.x);
                int ry = yy - Mathf.RoundToInt(right.z);

                bool leftIsCorner = board.CoordIsWalkable(lx, ly);
                bool rightIsCorner = board.CoordIsWalkable(rx, ry);

                lx = x + Mathf.RoundToInt(right.x);
                ly = y + Mathf.RoundToInt(right.z);

                rx = x - Mathf.RoundToInt(right.x);
                ry = y - Mathf.RoundToInt(right.z);

                bool leftIsInset = subMeshData.boardTileSetData.halfInsetObject != null && !board.CoordIsWalkable(lx, ly);
                bool rightIsInset = subMeshData.boardTileSetData.halfInsetObject != null && !board.CoordIsWalkable(rx, ry);

                Mesh leftMesh;
                if (leftIsInset)
                {
                    leftMesh = subMeshData.boardTileSetData.halfInsetObject.sharedMesh;
                }
                else if (leftIsCorner)
                {
                    leftMesh = subMeshData.boardTileSetData.halfCornerObject.sharedMesh;
                }
                else
                {
                    leftMesh = subMeshData.boardTileSetData.halfWallObject.sharedMesh;
                }

                Mesh rightMesh;
                if (rightIsInset)
                {
                    rightMesh = subMeshData.boardTileSetData.halfInsetObject.sharedMesh;
                }
                else if (rightIsCorner)
                {
                    rightMesh = subMeshData.boardTileSetData.halfCornerObject.sharedMesh;
                }
                else
                {
                    rightMesh = subMeshData.boardTileSetData.halfWallObject.sharedMesh;
                }

                meshData.AddMesh(x, y, board, leftMesh, rotation, 1.0f);
                meshData.AddMesh(x, y, board, rightMesh, rotation, -1.0f);
            }
        }

        static void HandleGate(int x, int y, int numTurns, Board board, MeshData meshData, SubMeshData subMeshData)
        {
            BoardTileSet tileSet = board.GetTile(x, y).tileSet;

            Vector3 wallSide = Vector3.forward;
            Vector3 right = Vector3.right;

            Quaternion rotation = Quaternion.Euler(Vector3.up * 90.0f * numTurns);

            wallSide = rotation * wallSide;
            right = rotation * right;

            int xx = x + Mathf.RoundToInt(wallSide.x);
            int yy = y + Mathf.RoundToInt(wallSide.z);

            BoardTileSet nextTileSet = board.GetTile(xx, yy).tileSet;

            if (tileSet != nextTileSet && board.CoordIsWalkable(xx, yy))
            {
                if (tileSet.gatePriority != 0)
                {
                    if (tileSet.gatePriority > nextTileSet.gatePriority)
                    {
                        int lx = xx + Mathf.RoundToInt(right.x);
                        int ly = yy + Mathf.RoundToInt(right.z);

                        int rx = xx - Mathf.RoundToInt(right.x);
                        int ry = yy - Mathf.RoundToInt(right.z);

                        if (!board.CoordIsWalkable(lx, ly) && !board.CoordIsWalkable(rx, ry))
                        {
                            meshData.AddMesh(x, y, board, subMeshData.boardTileSetData.gateObject.sharedMesh, rotation, -1.0f);
                        }
                    }
                }
            }
        }

        static void HandleFloorTile(int x, int y, Board board, MeshData meshData)
        {
            Tile tile = board.GetTile(x, y);
            BoardTileSet tileSet = tile.tileSet; // we need to cache all of these!

            SubMeshData subMeshData = meshData.GetOrCreateSubmeshData(tileSet);

            if (tileSet == null)
            {
                throw new System.Exception($"Tile at {x}, {y} had no tileset. This should be checked before calling this function.");
            }
            else
            {
                meshData.AddMesh(x, y, board, subMeshData.boardTileSetData.floorObject.sharedMesh, Quaternion.identity);
                meshData.AddMesh(x, y, board, subMeshData.boardTileSetData.ceilingObject.sharedMesh, Quaternion.identity);

                HandleWallTile(x, y, 0, board, meshData, subMeshData);
                HandleWallTile(x, y, 1, board, meshData, subMeshData);
                HandleWallTile(x, y, 2, board, meshData, subMeshData);
                HandleWallTile(x, y, 3, board, meshData, subMeshData);

                if (subMeshData.boardTileSetData.gateObject != null)
                {
                    HandleGate(x, y, 0, board, meshData, subMeshData);
                    HandleGate(x, y, 1, board, meshData, subMeshData);
                    HandleGate(x, y, 2, board, meshData, subMeshData);
                    HandleGate(x, y, 3, board, meshData, subMeshData);
                }
            }
        }

        public static void Generate(Board board, MeshFilter meshFilter)
        {
            MeshData meshData = new MeshData();

            for (int x = 0; x < board.GetWidth(); x++)
            {
                for (int y = 0; y < board.GetHeight(); y++)
                {
                    Tile tile = board.GetTile(x, y);
                    if (tile.tileSet != null)
                    {
                        HandleFloorTile(x, y, board, meshData);
                    }
                }
            }

            meshData.ApplyToMeshRenderer(meshFilter.GetComponent<MeshRenderer>());
            meshData.ApplyToMeshFilter(meshFilter);
        }
    }
}
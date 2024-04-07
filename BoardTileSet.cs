namespace BoardMeshModule
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public struct BoardTileSetData
    {
        public MeshFilter floorObject;
        public MeshFilter ceilingObject;
        public MeshFilter halfWallObject;
        public MeshFilter halfCornerObject;
        public MeshFilter halfInsetObject;
        public MeshFilter gateObject;
    }

    [CreateAssetMenu(fileName = "BoardTileSet", menuName = "DungeonCrawler/Board/BoardTileSet", order = 0)]
    public class BoardTileSet : ScriptableObject
    {
    #if UNITY_EDITOR
        public Color color = Color.white;
#endif
        public const int NO_GATE = 0;

        [Header("Optional child subnames: \"corner\", \"inset\", \"gate\".")]
        [Header("Required child subnames: \"floor\", \"ceiling\", \"wall\".")]

        public GameObject modelPrefab;
        public Material material;

        // BoardTileSets with the same non-zero priority will use both gates.
        [Header("Set to 0 for \"No Gate\".")]
        public int gatePriority = NO_GATE;

        public void GetSubMeshes(out BoardTileSetData outData)
        {
            MeshFilter[] meshFilters = modelPrefab.GetComponentsInChildren<MeshFilter>();
            outData.floorObject = SeekMeshFilter(meshFilters, "floor");
            outData.ceilingObject = SeekMeshFilter(meshFilters, "ceiling");
            outData.halfWallObject = SeekMeshFilter(meshFilters, "wall");
            outData.halfCornerObject = SeekMeshFilter(meshFilters, "corner", true);
            outData.halfInsetObject = SeekMeshFilter(meshFilters, "inset", true);
            outData.gateObject = SeekMeshFilter(meshFilters, "gate", true);
        }

        MeshFilter SeekMeshFilter(MeshFilter[] meshFilters, string meshFilterName, bool optional = false)
        {
            foreach(var meshFilter in meshFilters)
            {
                if (meshFilter.gameObject.name.ToLower().Contains(meshFilterName))
                {
                    return meshFilter;
                }
            }

            if (!optional)
            {
                throw new System.Exception($"BoardTileSet {meshFilterName} Error -- could not find a meshFilter with name {meshFilterName} on {modelPrefab.name}");
            }
            return null;
        }
    }
}

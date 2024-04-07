namespace BoardMeshModule
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Helper struct for dynamically passing out child data
    /// </summary>
    public struct BoardTileSetData
    {
        public MeshFilter floorObject;
        public MeshFilter ceilingObject;
        public MeshFilter halfWallObject;
        public MeshFilter halfCornerObject;
        public MeshFilter halfInsetObject;
        public MeshFilter gateObject;
    }

    /// <summary>
    /// BoardTileSet asset.
    /// 
    /// Hook this up to 3d assets to use on the board mesh.
    /// </summary>
    [CreateAssetMenu(fileName = "BoardTileSet", menuName = "DungeonCrawler/Board/BoardTileSet", order = 0)]
    public class BoardTileSet : ScriptableObject
    {
    #if UNITY_EDITOR
        public Color color = Color.white;
#endif
        /// <summary>
        /// Gate priority value which doubles as having no gate
        /// </summary>
        public const int NO_GATE = 0;

        /// <summary>
        /// Each model consists of a transform with children each containing one of the following substrings:
        /// floor, ceiling, wall. These are substrings and are not case sensitive. e.g.: "floor", "FLoOr", "TheFloor", "Floor2"
        /// 
        /// Optionally: corner, inset, and gate.
        /// </summary>
        [Header("Optional child subnames: \"corner\", \"inset\", \"gate\".")]
        [Header("Required child subnames: \"floor\", \"ceiling\", \"wall\".")]
        public GameObject modelPrefab;
        public Material material;

        /// <summary>
        /// BoardTileSets with NO_GATE are assumed to not use a gate.
        /// BoardTileSets with the same non-zero priority will use both gates.
        /// </summary>
        [Header("Set to 0 for \"No Gate\".")]
        public int gatePriority = NO_GATE;

        /// <summary>
        /// Gets all MeshFilters in its children and iterates over them, searching
        /// for substring names.
        /// 
        /// Will error if a non-optional child is not found.
        /// </summary>
        /// <param name="outData"></param>
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

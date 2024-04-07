namespace BoardMeshModule
{
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using DungeonCrawler.AI;

    /// <summary>
    /// Component for generating the mesh.
    /// 
    /// Editor script dynamically generates the mesh, but it does not save.
    /// The mesh needs to be generated at runtime.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class BoardMeshComponent : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField]
        protected Board board;

        MeshFilter boardMeshFilter;
        MeshRenderer boardMeshRenderer;

        void Awake()
        {
            boardMeshFilter = GetComponent<MeshFilter>();
            boardMeshRenderer = GetComponent<MeshRenderer>();
        }

        /// <summary>
        /// Call this to generate the board mesh at runtime
        /// </summary>
        public void Init()
        {
            BoardMeshGenerator.Generate(board, boardMeshFilter, boardMeshRenderer);
        }

        public Board GetBoard() => board;

        /// <summary>
        /// A board is fully capable of having its data
        /// designed in the editor.
        /// 
        /// However, if you are doing proc gen or have some other
        /// reason to set the board, it is not disallowed.
        /// </summary>
        /// <param name="board"></param>
        public void SetBoard(Board board) => this.board = board;
    }
}

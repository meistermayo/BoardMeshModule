namespace BoardMeshModule
{
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using DungeonCrawler.AI;

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class BoardMeshComponent : MonoBehaviour
    {
        [HideInInspector]
        [SerializeField]
        protected Board board;

        MeshFilter boardMeshFilter;

        void Awake()
        {
            boardMeshFilter = GetComponent<MeshFilter>();
        }

        public void Init()
        {
            BoardMeshGenerator.Generate(board, boardMeshFilter);
        }

        public Board GetBoard() => board;

        public void SetBoard(Board board) => this.board = board;
    }
}

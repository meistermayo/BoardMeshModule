namespace BoardMeshModule
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System.Reflection;
    using System;
    using System.Linq;

    [CustomEditor(typeof(BoardMeshComponent))]
    public class BoardMeshComponentEditor : Editor {
        BoardMeshComponent boardBehaviour;
        MeshFilter boardMeshFilter;
        MeshRenderer boardMeshRenderer;

        Vector2Int? size = Vector2Int.zero;
        Vector2Int lastCoord = Vector2Int.zero;

        Camera mainCamera;

        string[] tileSetNames;
        Dictionary<string, BoardTileSet> tileSets = new Dictionary<string, BoardTileSet>();

        static int currentTileSetIndex = 0;

        bool mouseInPanel = false;

        void OnEnable()
        {
            mainCamera = Camera.main;

            Tools.hidden = true;
            SceneView.duringSceneGui += this.OnSceneGUI;
            Init();
        }
    
        void OnDisable()
        {
            Tools.hidden = false;
            SceneView.duringSceneGui -= this.OnSceneGUI;
            EditorGUIUtility.hotControl = 0;
            boardBehaviour = null;
            boardMeshFilter = null;
        }

        void Init()
        {
            InitBoardView();
            tileSets.Clear();
            string[] guids = AssetDatabase.FindAssets("t:BoardTileSet");
            tileSetNames = new string[guids.Length];

            for (int i=0; i<guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                BoardTileSet boardTileSet = (BoardTileSet)AssetDatabase.LoadAssetAtPath(path, typeof(BoardTileSet));

                tileSetNames[i] = boardTileSet.name;
                tileSets.Add(boardTileSet.name, boardTileSet);
            }

            if (currentTileSetIndex >= tileSetNames.Length || currentTileSetIndex < 0)
            {
                currentTileSetIndex = 0;
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            // UpdateAnimAlpha();

            DrawBackground();
            DrawBoard();

            Vector3 mouseBoardPos = ProjectMousePositionOntoBoard(Event.current.mousePosition);
            if (!mouseInPanel)
            {
                DrawTile(Board.WorldToBoard(mouseBoardPos), tileSets[tileSetNames[currentTileSetIndex]].color * 0.25f);
            }

            DrawBoardSettingsPanel(sceneView.camera.pixelRect);

            ProcessEvents(Event.current);

            Vector2Int coord = Board.PosToCoord(mouseBoardPos);
            if (coord != lastCoord) GUI.changed = true;
            lastCoord = coord;

            if (GUI.changed)
            {
                Repaint();
            }
        }

        void DrawBackground()
        {
            Board board = boardBehaviour.GetBoard();
            Handles.color = Color.green;
            Vector3 boardSize = new Vector3(board.GetWidth(), 0, board.GetHeight());
            Handles.DrawWireCube(boardSize * 0.5f, boardSize);
            Handles.DrawLine(Vector3.zero, Vector3.right * board.GetWidth());
            Handles.DrawLine(Vector3.zero, Vector3.forward * board.GetHeight());
            Handles.DrawLine(Vector3.zero, Vector3.right * board.GetWidth());
            Handles.DrawLine(Vector3.zero, Vector3.forward * board.GetHeight());
        }

        void DrawTile(int x, int y)
        {
            BoardTileSet tileSet = boardBehaviour.GetBoard().GetTile(x, y).tileSet;
            Vector3 center = new Vector3(x*Board.TILE_SIZE, 0, y*Board.TILE_SIZE) + new Vector3(Board.TILE_SIZE * 0.5f, 0, Board.TILE_SIZE * 0.5f); 
            DrawTile(center, tileSet.color);
        }

        void DrawTile(Vector3 center, Color color)
        {
            Vector3[] verts = new Vector3[4];
            verts[0] = center + new Vector3(-Board.TILE_SIZE * 0.5f, 0, -Board.TILE_SIZE * 0.5f) * 0.4f;
            verts[1] = center + new Vector3(-Board.TILE_SIZE * 0.5f, 0,  Board.TILE_SIZE * 0.5f) * 0.4f;
            verts[2] = center + new Vector3( Board.TILE_SIZE * 0.5f, 0,  Board.TILE_SIZE * 0.5f) * 0.4f;
            verts[3] = center + new Vector3( Board.TILE_SIZE * 0.5f, 0, -Board.TILE_SIZE * 0.5f) * 0.4f;

            Handles.color = Color.white;
            Handles.DrawSolidRectangleWithOutline(verts, color, Color.black);
        }

        void DrawBoard()
        {
            Board board = boardBehaviour.GetBoard();
            for (int x = 0; x < board.GetWidth(); x++)
            {
                for (int y = 0; y < board.GetHeight(); y++)
                {
                    Vector2 viewportPoint = mainCamera.WorldToViewportPoint(Board.CoordToPos(x, y));
                    if (viewportPoint.x > 0 && viewportPoint.y > 0 && viewportPoint.x < 1 && viewportPoint.y < 1)
                    {
                        if (board.GetTile(x,y).tileSet != null)
                        {
                            DrawTile(x,y);
                        }
                    }
                }
            }
        }
        
        void InitBoardView()
        {
            boardBehaviour = target as BoardMeshComponent;
            if (boardBehaviour != null)
            {
                boardMeshFilter = boardBehaviour.GetComponent<MeshFilter>();
                boardMeshRenderer = boardBehaviour.GetComponent<MeshRenderer>();
                BoardMeshGenerator.Generate(boardBehaviour.GetBoard(), boardMeshFilter, boardMeshRenderer);
            }
            else
            {
                throw new System.Exception("Target was not a BoardBehaviour!");
            }
        }

        void DrawBoardSettingsPanel(Rect position)
        {
            // Removing this means I can't use box styling on the next area i make for some reason...
            GUILayout.BeginArea(new Rect(16, 16, 256, 128));// -> this breaks , new GUIStyle(GUI.skin.box));
            GUILayout.EndArea();

            Rect rect = new Rect(16, position.height - position.height / 3, 256, position.height / 3 - 16);
            mouseInPanel = rect.Contains(Event.current.mousePosition);

            GUILayout.BeginArea(rect, new GUIStyle(GUI.skin.box));

            GUILayout.Label("Board Settings");
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            currentTileSetIndex = EditorGUILayout.Popup(currentTileSetIndex, tileSetNames);

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Generate Mesh"))
            {
                Undo.RecordObject(boardMeshFilter, "Changed board mesh");
                BoardMeshGenerator.Generate(boardBehaviour.GetBoard(), boardMeshFilter, boardMeshRenderer);
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("Clear Board", GUILayout.Width(96)))
            {
                Undo.RecordObject(boardBehaviour, "Cleared board");
                boardBehaviour.GetBoard().ClearBoard();
                BoardMeshGenerator.Generate(boardBehaviour.GetBoard(), boardMeshFilter, boardMeshRenderer);
                EditorUtility.SetDirty(boardBehaviour);
            }
            GUILayout.EndArea();
        }

        void ProcessEvents(Event e)
        {
            if (mouseInPanel) return;
            if (e.alt || e.control) return;

            switch (e.type)
            {
                case EventType.MouseDown:
                    switch (e.button)
                    {
                        case 0:
                            CheckMouseTile(e.mousePosition, !e.shift);
                            break;
                        case 1:
                           // DropDownMenu(e.mousePosition);
                            break;
                        case 2:
                            break;
                    }
                    break;
                
                case EventType.MouseUp:
                    switch (e.button)
                    {
                        case 0:
                            break;
                        case 1:
                            break;
                        case 2:
                            break;
                    }
                    break;
                
                case EventType.MouseDrag:
                    switch (e.button)
                    {
                        case 0:
                            CheckMouseTile(e.mousePosition, !e.shift);
                            break;
                        case 1:
                            break;
                        case 2:
                            break;
                    }
                    break;
            }
        }

        Vector3 ProjectMousePositionOntoBoard(Vector2 pos)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(pos);

            Vector3 vectY = new Vector3(0, -ray.origin.y, 0);

            return ray.origin + (ray.direction * (vectY.y/ray.direction.y));
        }

        void CheckMouseTile(Vector2 pos, bool enable)
        {
            Vector3 clickPos = ProjectMousePositionOntoBoard(pos);

            Vector2Int coord = Board.PosToCoord(clickPos);
            Board board = boardBehaviour.GetBoard();

            if (board.CoordIsValid(coord.x, coord.y))
            {
                Undo.RecordObject(boardBehaviour, "Changed board tile");
                board.SetTileSet(coord.x, coord.y, enable ? tileSets[tileSetNames[currentTileSetIndex]] : null);
                EditorUtility.SetDirty(boardBehaviour);
            }
        }
    }

}

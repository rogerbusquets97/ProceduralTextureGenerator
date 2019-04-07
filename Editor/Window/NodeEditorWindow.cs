using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{

    public class NodeEditorWindow : EditorWindow
    {
        //List Nodes
        private List<NodeBase> nodes;
        NodeBase selectedNode;
        //List connections
        private List<NodeConnection> connections;
        private ConnectionPoint selectedInPoint;
        private ConnectionPoint selectedOutPoint;
        //Grid
        private Vector2 offset;
        private Vector2 drag;
        //GUI
        private GUIStyle inPointStyle;
        private GUIStyle outPointStyle;
        // Preview Win
        private Rect previewRect;
        Camera previewCam;
        GameObject camObj;
        GameObject previewObj;

        float minFOV = 15f;
        float maxFOV = 150f;
        float sensitivity = 10f;
        float rotSpeed = 20f;


        [MenuItem("Tools/Procedural Texture Generator")]
        private static void LaunchEditor()
        {
            NodeEditorWindow win = (NodeEditorWindow)EditorWindow.GetWindow(typeof(NodeEditorWindow), false, "Procedural Texture Generator");
            win.autoRepaintOnSceneChange = true;
        }
        private void OnEnable()
        {
            inPointStyle = new GUIStyle();
            inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
            inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
            inPointStyle.border = new RectOffset(4, 4, 12, 12);

            outPointStyle = new GUIStyle();
            outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
            outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
            outPointStyle.border = new RectOffset(4, 4, 12, 12);

            nodes = new List<NodeBase>();

            previewRect = new Rect(position.width - 300, position.height / 2, 300, position.height);

            camObj = new GameObject();
            camObj.SetActive(false);
            camObj.transform.SetPositionAndRotation(new Vector3(0, 1000, 0), Quaternion.identity);
            previewCam = camObj.AddComponent(typeof(Camera)) as Camera;
            previewCam.backgroundColor = Color.black;
            previewCam.clearFlags = CameraClearFlags.SolidColor;
            camObj.hideFlags = HideFlags.HideAndDontSave;

            previewObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            previewObj.SetActive(false);
            previewObj.transform.position = new Vector3(camObj.transform.position.x, camObj.transform.position.y, camObj.transform.position.z + 15);
            previewObj.hideFlags = HideFlags.HideAndDontSave;

            MeshRenderer renderer = previewObj.GetComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("HDRenderPipeline/Lit"));

            var folder = System.IO.Directory.CreateDirectory("./Tmp");
            AssetDatabase.Refresh();
        }

        private void OnDisable()
        {
            DestroyImmediate(camObj);
            DestroyImmediate(previewObj);
            if (System.IO.Directory.Exists("./Tmp"))
            {
                FileUtil.DeleteFileOrDirectory("./Tmp");
            }
            AssetDatabase.Refresh();

            for(int i = 0; i<nodes.Count; i++)
            {
                OnClickRemoveNode(nodes[i]);
            }

          
        }

        private void OnGUI()
        {
            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);
            ProcessNodeEvents(Event.current);
            DrawConnections();
            DrawConnectionLine(Event.current);

            DrawEditor();

            ProcessEvents(Event.current);
            if (GUI.changed)
                Repaint();
        }

        void DrawEditor()
        {
            BeginWindows();

            DrawPreviewWindow();
            GUI.BringWindowToFront(0);
            DrawNodes();
            EndWindows();
        }

        public void SetSelectedNode(NodeBase n)
        {
            selectedNode = n;
            Selection.activeObject = n;
        }

        private void DrawNodes()
        {
            Event e = Event.current;

            if(nodes!= null)
            {
                for(int i = 0; i<nodes.Count; ++i)
                {
                    nodes[i].DrawInOutPoints();
                }
            }

            if(nodes!= null)
            {
                for(int i = 0; i< nodes.Count; ++i)
                {
                    nodes[i].Update();
                    nodes[i].Draw();
                }
            }
        }

        private void DrawConnections()
        {
            if(connections!= null)
            {
                for(int i = 0; i< connections.Count; ++i)
                {
                    connections[i].Draw();
                }
            }
        }

        private void ProcessNodeEvents(Event e)
        {
            if(nodes!= null)
            {
                for(int i = nodes.Count -1; i>=0; --i)
                {
                    nodes[i].ProcessEvents(e);
                }
            }
        }

        private void DrawConnectionLine(Event e)
        {
            if (selectedInPoint != null && selectedOutPoint == null)
            {
                Handles.DrawBezier(selectedInPoint.rect.center,
                    e.mousePosition,
                    selectedInPoint.rect.center + Vector2.left * 50f,
                    e.mousePosition - Vector2.left * 50f,
                    Color.white,
                    null,
                    2f);
                GUI.changed = true;
            }

            if (selectedOutPoint != null && selectedInPoint == null)
            {
                Handles.DrawBezier(selectedOutPoint.rect.center,
                   e.mousePosition,
                   selectedOutPoint.rect.center - Vector2.left * 50f,
                   e.mousePosition + Vector2.left * 50f,
                   Color.white,
                   null,
                   2f);

                GUI.changed = true;
            }

            Repaint();
        }

        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            offset += drag * 0.5f;
            Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);

            }

            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        void DrawPreviewWindow()
        {
            previewRect.height = 300;
            previewRect.x = position.width - 300;
            previewRect.y = position.height - 300;
            previewRect = GUI.Window(0, previewRect, DrawPreview, "Preview");
        }
        void DrawPreview(int unusedWindowId)
        {
            previewObj.SetActive(true);

            previewRect.x = 0;
            previewRect.y = 0 + 17;
            previewRect.height -= 15;
            previewCam.pixelRect = previewRect;


            Handles.DrawCamera(previewCam.pixelRect, previewCam);

            previewObj.SetActive(false);
        }

        void ProcessEvents(Event e)
        {
            drag = Vector2.zero;
            ProcessPreviewEvents(e);

            switch(e.type)
            {
                case EventType.MouseDown:
                    if(e.button == 0)
                    {
                        ClearConnectionSelection();
                    }
                    if(e.button == 1)
                    {
                        ProcessContextMenu(e.mousePosition);
                        Debug.Log("Process");
                    }
                    break;
                case EventType.MouseDrag:
                    if(e.button == 0)
                    {
                        if(!previewRect.Contains(e.mousePosition))
                        {
                            OnDrag(e.delta);
                        }
                    }
                    break;
            }
        }

        private void OnDrag(Vector2 delta)
        {
            drag = delta;
            if(nodes!= null)
            {
                for(int i = 0; i< nodes.Count; ++i)
                {
                    nodes[i].Drag(delta);
                }
            }
            GUI.changed = true;
        }

        private void ProcessContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Generators/Fractal Noise"), false, () => OnClickAddNode(mousePosition,NodeType.Fractal));
            genericMenu.AddItem(new GUIContent("Generators/Cellular Noise"), false, () => OnClickAddNode(mousePosition, NodeType.Cellular));
            genericMenu.AddItem(new GUIContent("Operators/Blend"),false, ()=> OnClickAddNode(mousePosition,NodeType.Blend));
            genericMenu.AddItem(new GUIContent("Filters/Levels"), false, () => OnClickAddNode(mousePosition, NodeType.Levels));
            genericMenu.AddItem(new GUIContent("Filters/Normal"), false, () => OnClickAddNode(mousePosition, NodeType.Normal));
            genericMenu.AddItem(new GUIContent("Filters/One Minus"), false, () => OnClickAddNode(mousePosition, NodeType.OneMinus));
            genericMenu.AddItem(new GUIContent("Generators/Flat Color"), false, () => OnClickAddNode(mousePosition, NodeType.Color));
            genericMenu.AddItem(new GUIContent("Filters/Mix"), false, () => OnClickAddNode(mousePosition, NodeType.Mix));



            genericMenu.ShowAsContext();
        }

        private void OnClickAddNode(Vector2 mousePosition, NodeType type)
        {
            switch(type)
            {
                case NodeType.Fractal:
                    FractalNode fractal = FractalNode.CreateInstance<FractalNode>();
                    fractal.Init(mousePosition, 100, 100, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, this);
                    nodes.Add(fractal);
                    Selection.activeObject = fractal;
                    break;
                case NodeType.Cellular:
                    CellularNode cellular  = CellularNode.CreateInstance<CellularNode>();
                    cellular.Init(mousePosition, 100, 100, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, this);
                    nodes.Add(cellular);
                    Selection.activeObject = cellular;
                    break;
                case NodeType.Blend:
                    BlendNode blend = BlendNode.CreateInstance<BlendNode>();
                    blend.Init(mousePosition, 100, 100, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, this);
                    nodes.Add(blend);
                    Selection.activeObject = blend;
                    break;
                case NodeType.Levels:
                    LevelsNode levels = LevelsNode.CreateInstance<LevelsNode>();
                    levels.Init(mousePosition, 100, 100, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, this);
                    nodes.Add(levels);
                    Selection.activeObject = levels;
                    break;
                case NodeType.Normal:
                    NormalNode normal = NormalNode.CreateInstance<NormalNode>();
                    normal.Init(mousePosition, 100, 100, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, this);
                    nodes.Add(normal);
                    Selection.activeObject = normal;
                    break;
                case NodeType.OneMinus:
                    MinusOneNode n = MinusOneNode.CreateInstance<MinusOneNode>();
                    n.Init(mousePosition, 100, 100, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, this);
                    nodes.Add(n);
                    Selection.activeObject = n;
                    break;
                case NodeType.Color:
                    FlatColor color = FlatColor.CreateInstance<FlatColor>();
                    color.Init(mousePosition, 100, 100, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, this);
                    nodes.Add(color);
                    Selection.activeObject = color;
                    break;
                case NodeType.Mix:
                    MixNode mix = MixNode.CreateInstance<MixNode>();
                    mix.Init(mousePosition, 100, 100, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, this);
                    nodes.Add(mix);
                    Selection.activeObject = mix;
                    break;
            }
        }

        public NodeBase GetSelectedNode()
        {
            return selectedNode;
        }

        private void OnClickRemoveNode(NodeBase n)
        {
            if(connections!= null)
            {
                List<NodeConnection> connectionToRemove = new List<NodeConnection>();
                for(int i = 0; i< connections.Count; ++i)
                {
                    if(n.inPoints!= null)
                    {
                        for(int j = 0; j<n.inPoints.Count; ++j)
                        {
                            if(connections[i].inPoint == n.inPoints[j])
                            {
                                connectionToRemove.Add(connections[i]);
                            }
                        }
                    }

                    if(n.outPoints!= null)
                    {
                        for(int j = 0; j<n.outPoints.Count; ++j)
                        {
                            if(connections[i].outPoint == n.outPoints[j])
                            {
                                connectionToRemove.Add(connections[i]);
                            }
                        }
                    }
                }

                for(int i = 0; i<connectionToRemove.Count; ++i)
                {
                    connections.Remove(connectionToRemove[i]);
                }

                connectionToRemove = null;
            }

            if(selectedNode == n)
            {
                selectedNode = null;
            }

            nodes.Remove(n);
            DestroyImmediate(n);
        }

        private void OnClickOutPoint(ConnectionPoint outPoint)
        {
            selectedOutPoint = outPoint;
            if (selectedInPoint != null)
            {
                if (selectedOutPoint.node != selectedInPoint.node)
                {
                    CreateConnection();
                    ClearConnectionSelection();
                }
                else
                {
                    ClearConnectionSelection();
                }
            }
        }
        private void OnClickInPoint(ConnectionPoint inPoint)
        {
            selectedInPoint = inPoint;

            if (selectedOutPoint != null)
            {
                if (selectedOutPoint.node != selectedInPoint.node)
                {
                    CreateConnection();
                    ClearConnectionSelection();
                }
                else
                {
                    ClearConnectionSelection();

                }
            }
        }
        private void OnClickRemoveConnection(NodeConnection connection)
        {

            connection.inPoint.connections.Clear();
            connection.outPoint.connections.Clear();

            connections.Remove(connection);
        }

        private void CreateConnection()
        {
            if (connections == null)
            {
                connections = new List<NodeConnection>();
            }

            NodeConnection con = new NodeConnection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection);

            if (selectedInPoint != null && selectedOutPoint != null)
            {
                selectedInPoint.connections.Add(con);
                selectedOutPoint.connections.Add(con);

                selectedOutPoint.node.StartComputeThread(true);
            }

            connections.Add(con);
        }

        private void ClearConnectionSelection()
        {
            selectedOutPoint = null;
            selectedInPoint = null;
        }

        public GameObject GetPreviewObj()
        {
            return previewObj;
        }

        void ProcessPreviewEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDrag:
                    if (e.button == 0)
                    {

                        if (previewRect.Contains(e.mousePosition))
                        {
                            float rotX = e.delta.x * rotSpeed * Mathf.Deg2Rad;
                            float rotY = e.delta.y * rotSpeed * Mathf.Deg2Rad;

                            previewObj.transform.Rotate(Vector3.up, -rotX, Space.World);
                            previewObj.transform.Rotate(Vector3.right, -rotY, Space.World);

                            Repaint();
                        }
                    }
                    break;
                case EventType.ScrollWheel:
                    if (previewRect.Contains(e.mousePosition))
                    {
                        float fov = previewCam.fieldOfView;
                        fov += e.delta.y;
                        fov = Mathf.Clamp(fov, minFOV, maxFOV);
                        previewCam.fieldOfView = fov;
                        Repaint();
                    }
                    break;
            }
        }
    }
}

   

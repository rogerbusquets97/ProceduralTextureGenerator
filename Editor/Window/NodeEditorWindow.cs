using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PTG
{

    public class NodeEditorWindow : EditorWindow
    {
        //List Nodes
        NodeBase selectedNode;
        //List connections
        //Grid
        private Vector2 offset;
        private Vector2 drag;
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


        }
        private void OnGUI()
        {
            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);

            DrawEditor();

            ProcessEvents(Event.current);
        }

        void DrawEditor()
        {
            BeginWindows();

            DrawPreviewWindow();
            GUI.BringWindowToFront(0);
            EndWindows();
        }

        public void SetSelectedNode(NodeBase n)
        {
            selectedNode = n;
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
            ProcessPreviewEvents(e);
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
                        else
                        {
                            //OnDrag(e.delta);
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

   

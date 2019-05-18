using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Threading;

namespace PTG
{
    public enum NodeType { None = 0, Fractal, Cellular, Blend,Levels, Normal, OneMinus, Color, Mix,MaskMap, Checker, Generator, Warp, Blur, Occlusion, Tile}

    public class NodeBase : ScriptableObject
    {
        public Vector2Int ressolution;
        public Vector2Int lastRessolution;
        public Rect rect;
        public string title;
        public bool isDragged;
        public bool isSelected;

        protected NodeEditorWindow editor;

        public List<ConnectionPoint> inPoints;
        public List<ConnectionPoint> outPoints;

        public Action<NodeBase> OnRemoveNode;

        public RenderTexture texture;
        public ComputeShader shader;
        public int kernel;

        virtual public object GetValue(int x, int y) { return 0; }
       
        public void InitTexture()
        {
            ressolution = new Vector2Int(1024, 1024);
            lastRessolution = ressolution;
            texture = new RenderTexture(ressolution.x, ressolution.y, 24);
        }

        public void ChangeRessolution(int res, bool selfCompute = false)
        {
            DestroyImmediate(texture);
            ressolution = new Vector2Int(res, res);
            texture = new RenderTexture(ressolution.x, ressolution.y, 24);
            texture.enableRandomWrite = true;
            texture.Create();
            Compute(selfCompute);
        }
        public RenderTexture GetTexture()
        {
            return texture;
        }
        public void Drag(Vector2 delta)
        {
            rect.position += delta;
        }

        virtual public void Compute(bool selfcompute = false)
        {
            
        }
        public virtual void Draw()
        {
            GUI.Box(rect, title);
        }

        public virtual void DrawInspector()
        {
            GUILayout.Space(10);
            ressolution = EditorGUILayout.Vector2IntField("Ressolution", ressolution);
            if(lastRessolution!=ressolution)
            {
                lastRessolution = ressolution;
                ChangeRessolution(ressolution.x, true);
            }
            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Preview Texture");
            GUI.DrawTexture(new Rect(GUILayoutUtility.GetLastRect().x, GUILayoutUtility.GetRect(256, 256).y, 256, 256), texture, ScaleMode.ScaleToFit, false);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            if (GUILayout.Button("Save as PNG"))
            {
                if (texture != null)
                {
                    var path = EditorUtility.SaveFilePanel("Save Texture as PNG", Application.dataPath, texture.name + ".png", "png");
                    if (path.Length != 0)
                    {
                        Texture2D output = new Texture2D(ressolution.x, ressolution.y, TextureFormat.ARGB32, false);
                        RenderTexture.active = texture;
                        output.ReadPixels(new Rect(0, 0, ressolution.x, ressolution.y), 0, 0);
                        output.Apply();
                        RenderTexture.active = null;

                        System.IO.File.WriteAllBytes(path, output.EncodeToPNG());
                        AssetDatabase.Refresh();
                    }
                }
            }
            GUILayout.EndVertical();
        }

        public virtual void DrawInOutPoints()
        {
            if (inPoints != null)
            {
                for (int i = 0; i < inPoints.Count; i++)
                {
                    inPoints[i].Draw(i + 1);
                }
            }
            if (outPoints != null)
            {
                for (int i = 0; i < outPoints.Count; i++)
                {
                    outPoints[i].Draw(i + 1);
                }
            }

        }

        public bool ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {

                        if (this.rect.Contains(e.mousePosition))
                        {
                            isDragged = true;
                            GUI.changed = true;
                            isSelected = true;

                            editor.SetSelectedNode(this);

                        }
                        else
                        {
                            GUI.changed = true;
                            isSelected = false;

                        }
                    }

                    if (e.button == 1 && isSelected && rect.Contains(e.mousePosition))
                    {
                        ProcessContextMenu();
                        e.Use();
                    }
                    break;
                case EventType.MouseUp:
                    isDragged = false;
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0 && isDragged)
                    {
                        Drag(e.delta);
                        e.Use();
                        return true;
                    }
                    break;

                    
            }
            return false;
        }
        protected void ProcessContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Remove Node"), false, OnClickRemoveNode);
            genericMenu.ShowAsContext();
        }

        protected void OnClickRemoveNode()
        {
            if(OnRemoveNode!= null)
            {
                OnRemoveNode(this);
                DestroyImmediate(texture);
            }
        }
    }

}
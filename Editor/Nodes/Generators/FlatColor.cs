using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    public class FlatColor : NodeBase
    {
        Color color = Color.black;
        Color lastColor;
        Color[] pixels;

        ConnectionPoint outPoint; 

        public FlatColor()
        {
            title = "Flat Color";
            door = new object();
            color = Color.black;
            lastColor = color;
        }

        public void OnEnable()
        {
            ressolution = new Vector2Int(256, 256);
            texture = new Texture2D(ressolution.x, ressolution.y, TextureFormat.ARGB32, false);
            pixels = texture.GetPixels();
        }

        public void Init(Vector2 position, float width, float height, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<NodeBase> OnClickRemoveNode, NodeEditorWindow editor)
        {
            rect = new Rect(position.x, position.y, width, height);
            this.editor = editor;
            outPoint = new ConnectionPoint(this, ConnectionType.Out, outPointStyle, OnClickOutPoint);
            if (outPoints == null)
            {
                outPoints = new List<ConnectionPoint>();
            }

            outPoints.Add(outPoint);
            OnRemoveNode = OnClickRemoveNode;

            StartComputeThread(true);
        }

        public override void StartComputeThread(bool selfCompute)
        {
            base.StartComputeThread(selfCompute);
        }

        public override void Draw()
        {
            base.Draw();
            base.Draw();
            GUILayout.BeginArea(rect);
            if (texture != null)
            {
                GUI.DrawTexture(new Rect((rect.width / 4) - 15, (rect.height / 4) - 8, rect.width - 20, rect.height - 20), texture);
            }
            GUILayout.EndArea();

            if(lastColor!= color)
            {
                lastColor = color;
                StartComputeThread(true);
            }
        }

        public override void DrawInspector()
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Color");
            color = EditorGUILayout.ColorField(color);
            GUILayout.EndVertical();

            base.DrawInspector();
        }

        private void FillColor(Color color)
        {
            if(pixels!= null)
            {
                for(int x = 0; x<ressolution.x; x++)
                {
                    for(int y = 0; y< ressolution.y; y++)
                    {
                        pixels[Filter.GetIndex(x, y, ressolution.x, ressolution.y)] = color;
                    }
                }
            }
        }

        public override void Compute(bool selfcompute = false)
        {
            if(pixels!= null)
            {
                lock(door)
                {
                    FillColor(color);
                }
            }

            Action MainThreadAction = () =>
            {
                if (selfcompute)
                {
                    texture.SetPixels(pixels);
                    texture.wrapMode = TextureWrapMode.Clamp;
                    texture.Apply();
                    editor.Repaint();
                }

                if (outPoint.connections != null)
                {
                    for (int i = 0; i < outPoint.connections.Count; i++)
                    {
                        outPoint.connections[i].inPoint.node.StartComputeThread(true);
                    }
                }
            };

            QueueMainThreadFunction(MainThreadAction);
        }
    }

}
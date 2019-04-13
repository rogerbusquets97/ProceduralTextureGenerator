using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    public class TileGenerator : NodeBase
    {
        ConnectionPoint outPoint;
        Color[] outPixels;

        public TileGenerator()
        {
            title = "Tile Generator";
        }

        private void OnEnable()
        {
            InitTexture();
            //outPixels = texture.GetPixels();
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

            Compute(true);
        }

        public override object GetValue(int x, int y)
        {
            return 0;
        }

        public override void Draw()
        {
            base.Draw();

            base.Draw();
            GUILayout.BeginArea(rect);
            //Texture
            if (texture != null)
            {
                GUI.DrawTexture(new Rect((rect.width / 4) - 15, (rect.height / 4) - 8, rect.width - 20, rect.height - 20), texture);
            }

            GUILayout.EndArea();
        }

        public override void DrawInspector()
        {
            base.DrawInspector();
        }

        public override void Compute(bool selfcompute = false)
        {
            if (outPixels != null)
            {
                
            }

            Action MainThreadAction = () =>
            {
                if (selfcompute)
                {
                    //texture.SetPixels(outPixels);
                    texture.wrapMode = TextureWrapMode.Clamp;
                    //texture.Apply();
                    editor.Repaint();
                }

                if (outPoint.connections != null)
                {
                    for (int i = 0; i < outPoint.connections.Count; i++)
                    {
                        outPoint.connections[i].inPoint.node.Compute(true);
                    }
                }
            };
        }
    }
}

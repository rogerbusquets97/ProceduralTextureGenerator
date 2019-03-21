using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    public class FractalNode : NodeBase
    {
        ConnectionPoint outPoint;
        Texture2D texture;
        Color[] noisePixels;
        Noise.FractalSettings settings;
        Noise.FractalType type;

        public FractalNode(Vector2 position, float width, float height, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<NodeBase> OnClickRemoveNode, NodeEditorWindow editor)
        {
            title = "Fractal Noise";
            rect = new Rect(position.x, position.y, width, height);
            this.editor = editor;
            outPoint = new ConnectionPoint(this, ConnectionType.Out, outPointStyle, OnClickOutPoint);

            door = new object();
            ressolution = new Vector2Int(256, 256);
            texture = new Texture2D(ressolution.x, ressolution.y, TextureFormat.ARGB32, false);
            ressolution = new Vector2Int(texture.width, texture.height);
            noisePixels = texture.GetPixels();

            if(outPoints == null)
            {
                outPoints = new List<ConnectionPoint>();
            }

            outPoints.Add(outPoint);

            OnRemoveNode = OnClickRemoveNode;

            settings.octaves = 7;
            settings.offsetx = 0;
            settings.offsety = 0;
            settings.persistance = 3;
            settings.Xscale = 0.1f;
            settings.Yscale = 0.1f;
            type = Noise.FractalType.Brownian;

            StartComputeThread(true);
        }

        public override float GetValue(int x, int y)
        {
            return Noise.fBM(x, y, settings);
        }

        public override void Draw()
        {
            base.Draw();
            GUILayout.BeginArea(rect);
            //Sliders
            //Texture
            GUILayout.EndArea();
        }

        public override void Compute(bool selfcompute = false)
        {
            if(noisePixels!= null && selfcompute)
            {
                lock(door)
                {
                    noisePixels = Noise.Fractal(ressolution, settings, type);
                }
            }

            Action MainThreadAction = () =>
            {
                if (selfcompute)
                {
                    texture.SetPixels(noisePixels);
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

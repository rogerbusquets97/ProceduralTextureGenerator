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
        Color[] noisePixels;
        Noise.FractalSettings settings;
        
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

            if (outPoints == null)
            {
                outPoints = new List<ConnectionPoint>();
            }

            outPoints.Add(outPoint);

            OnRemoveNode = OnClickRemoveNode;

            settings = new Noise.FractalSettings(1337, 8, 2f, 0.008f, 0.5f, FastNoise.FractalType.FBM, FastNoise.NoiseType.SimplexFractal);


            StartComputeThread(true);
        }

        public override float GetValue(int x, int y)
        {
            return 0;
        }
        public override void Draw()
        {
            base.Draw();
            GUILayout.BeginArea(rect);
            //Sliders
            //Texture
            if(texture!=null)
            {
                GUI.DrawTexture(new Rect((rect.width / 4) - 15, (rect.height / 4) - 8, rect.width - 20, rect.height - 20), texture);
            }
            GUILayout.EndArea();
        }

        public override void Compute(bool selfcompute = false)
        {
            if(noisePixels!= null && selfcompute)
            {
                lock(door)
                {
                    noisePixels = Noise.SimplexFractal(ressolution, settings);
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

                    System.IO.File.WriteAllBytes("Assets/Diffuse.png", texture.EncodeToPNG());
                    AssetDatabase.Refresh();
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

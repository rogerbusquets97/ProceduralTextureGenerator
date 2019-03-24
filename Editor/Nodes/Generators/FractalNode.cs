using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    enum FractalNoiseType { Cubic, Simplex, Perlin, Value};
    public class FractalNode : NodeBase
    {
        ConnectionPoint outPoint;
        Color[] noisePixels;
        FractalNoiseType noiseType;
        FractalNoiseType lastNoiseType;
        Noise.FractalSettings settings;
        Noise.FractalSettings lastSettings;
        FastNoise noise;

        public FractalNode()
        {
            title = "Fractal Noise";
            door = new object();

            noiseType = FractalNoiseType.Simplex;
            lastNoiseType = noiseType;
            settings = new Noise.FractalSettings(1337, 5, 2f, 0.01f, 0.5f, FastNoise.FractalType.FBM, FastNoise.NoiseType.SimplexFractal);
            lastSettings = settings;
            noise = new FastNoise();
        }

        private void OnEnable()
        {
            ressolution = new Vector2Int(256, 256);
            texture = new Texture2D(ressolution.x, ressolution.y, TextureFormat.ARGB32, false);
            noisePixels = texture.GetPixels();
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

        public override float GetValue(int x, int y)
        {
            return Noise.GetSingleFractal(x, y, ressolution, settings, noise);
        }
        public override void Draw()
        {
            base.Draw();
            GUILayout.BeginArea(rect);
            //Texture
            if(texture!=null)
             {
                 GUI.DrawTexture(new Rect((rect.width / 4) - 15, (rect.height / 4) - 8, rect.width - 20, rect.height - 20), texture);
             }

            GUILayout.EndArea();

            if(lastSettings.octaves!= settings.octaves || lastSettings.persistance!= settings.persistance|| lastSettings.seed!= settings.seed|| lastSettings.frequency!= settings.frequency||lastSettings.lacunarity!= settings.lacunarity||lastSettings.fractalType!= settings.fractalType)
            {
                lastSettings = settings;
                StartComputeThread(true);
            }

            if(noiseType!= lastNoiseType)
            {
                lastNoiseType = noiseType;
                switch(noiseType)
                {
                    case FractalNoiseType.Cubic:
                        settings.noiseType = FastNoise.NoiseType.CubicFractal;
                        lastSettings = settings;
                        break;
                    case FractalNoiseType.Perlin:
                        settings.noiseType = FastNoise.NoiseType.PerlinFractal;
                        lastSettings = settings;
                        break;
                    case FractalNoiseType.Simplex:
                        settings.noiseType = FastNoise.NoiseType.SimplexFractal;
                        lastSettings = settings;
                        break;
                    case FractalNoiseType.Value:
                        settings.noiseType = FastNoise.NoiseType.ValueFractal;
                        lastSettings = settings;
                        break;
                }

                StartComputeThread(true);
            }
        }

        public override void DrawInspector()
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Octaves");
            settings.octaves = EditorGUILayout.IntField(settings.octaves);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Frequency");
            settings.frequency = EditorGUILayout.FloatField(settings.frequency);
            GUILayout.EndVertical();


            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Seed");
            GUILayout.BeginHorizontal();
            settings.seed = EditorGUILayout.IntField(settings.seed);
            if(GUILayout.Button("Randomize"))
            {
                settings.seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Lacunarity");
            settings.lacunarity = EditorGUILayout.FloatField(settings.lacunarity);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Persistance");
            settings.persistance = EditorGUILayout.FloatField(settings.persistance);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Fractal Type");
            settings.fractalType = (FastNoise.FractalType)EditorGUILayout.EnumPopup(settings.fractalType);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Fractal Noise Type");
            noiseType = (FractalNoiseType)EditorGUILayout.EnumPopup(noiseType);
            GUILayout.EndVertical();






        }
      
        public override void Compute(bool selfcompute = false)
        {
            if(noisePixels!= null && selfcompute)
            {
                lock(door)
                {
                    noisePixels = Noise.Fractal(ressolution, settings,noise);
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

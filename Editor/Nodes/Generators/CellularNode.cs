using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    public class CellularNode : NodeBase
    {
        ConnectionPoint outPoint;
        Color[] noisePixels;
        Noise.CellularSettings settings;
        Noise.CellularSettings lastSettings;
        FastNoise noise;

        public CellularNode()
        {
            title = "Cellular Node";
            door = new object();
            settings = new Noise.CellularSettings(1337, FastNoise.NoiseType.Cellular, FastNoise.CellularDistanceFunction.Euclidean, FastNoise.CellularReturnType.CellValue, 0.01f, new Vector2Int(0, 1), 0.45f);
            lastSettings = settings;
            noise = new FastNoise();
        }

        private void OnEnable()
        {
            ressolution = new Vector2Int(256, 256);
            texture = new Texture2D(ressolution.x, ressolution.y, TextureFormat.ARGB32,false);
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

        public override object GetValue(int x, int y)
        {
            return Noise.GetSingleCellular(x, y, ressolution, settings, noise);
        }

        public override void Draw()
        {
            base.Draw();
            GUILayout.BeginArea(rect);
            //Texture
            if (texture != null)
            {
                GUI.DrawTexture(new Rect((rect.width / 4) - 15, (rect.height / 4) - 8, rect.width - 20, rect.height - 20), texture);
            }

            GUILayout.EndArea();

            if(lastSettings.CellularDistanceFunc!= settings.CellularDistanceFunc || lastSettings.cellularIndices!= settings.cellularIndices|| lastSettings.seed!= settings.seed||lastSettings.frequency!= settings.frequency||lastSettings.jitter!= settings.jitter||lastSettings.noiseType!= settings.noiseType || lastSettings.returnType!=settings.returnType || lastSettings.lookup!= settings.lookup)
            {
                lastSettings = settings;
                StartComputeThread(true);
            }

        }

        public override void DrawInspector()
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Cellular Distance Function");
            settings.CellularDistanceFunc = (FastNoise.CellularDistanceFunction)EditorGUILayout.EnumPopup(settings.CellularDistanceFunc);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Frequency");
            settings.frequency = EditorGUILayout.FloatField(settings.frequency);
            GUILayout.EndVertical();


            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Seed");
            GUILayout.BeginHorizontal();
            settings.seed = EditorGUILayout.IntField(settings.seed);
            if (GUILayout.Button("Randomize"))
            {
                settings.seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Cellular Return Type");
            settings.returnType = (FastNoise.CellularReturnType)EditorGUILayout.EnumPopup(settings.returnType);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            settings.cellularIndices = EditorGUILayout.Vector2IntField("Cellular Indices", settings.cellularIndices);
            if(settings.cellularIndices.x > settings.cellularIndices.y || settings.cellularIndices.x > 4 || settings.cellularIndices.y > 4)
            {
                EditorGUILayout.HelpBox("Indices can NOT be greater than 0. X index MUST be smaller than Y index", MessageType.Warning);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Jitter");
            settings.jitter = EditorGUILayout.FloatField(settings.jitter);
            GUILayout.EndVertical();

            
        }

        public override void Compute(bool selfcompute = false)
        {
            if(noisePixels!= null && selfcompute)
            {
                lock(door)
                {
                    noisePixels = Noise.Cellular(ressolution, settings, noise);
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

                if(outPoint.connections!= null)
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
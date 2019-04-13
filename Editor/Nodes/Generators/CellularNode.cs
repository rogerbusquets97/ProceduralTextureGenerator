using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    public enum CellularType {F1, F2, DistanceSub, DistanceMul};
    public class CellularNode : NodeBase
    {
        ConnectionPoint outPoint;
        CellularType type;
        CellularType lastType;
        int octaves;
        int lastOctaves;
        float frequency;
        float lastFrequency;

        bool seamless;
        bool lastSeamless;

        float XScale;
        float YScale;
        float lastXScale;
        float lastYScale; 

        public CellularNode()
        {
            title = "Cellular Node";
            type = CellularType.F1;
            lastType = type;
            octaves = 1;
            lastOctaves = octaves;
            frequency = 0.01f;
            lastFrequency = frequency;
            seamless = false;
            lastSeamless = seamless;
            XScale = 10;
            YScale = 10;
            lastXScale = XScale;
            lastYScale = YScale;
        }

        private void OnEnable()
        {
            InitTexture();
            shader = (ComputeShader)Resources.Load("Noises");
            kernel = shader.FindKernel("F1DistanceVoronoi");
            texture.enableRandomWrite = true;
            texture.Create();
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
            GUILayout.BeginArea(rect);
            //Texture
            if (texture != null)
            {
                GUI.DrawTexture(new Rect((rect.width / 4) - 15, (rect.height / 4) - 8, rect.width - 20, rect.height - 20), texture);
            }

            GUILayout.EndArea();

            if (lastOctaves != octaves || lastFrequency != frequency || lastYScale!= YScale || lastXScale!= XScale)
            {
                lastFrequency = frequency;
                lastOctaves = octaves;
                Compute(true);
            }

            if (lastType != type || lastSeamless!= seamless)
            {
                lastType = type;
                lastSeamless = seamless;

                switch (type)
                {
                    case CellularType.F1:
                        if (!seamless)
                        {
                            kernel = shader.FindKernel("F1DistanceVoronoi");
                        }
                        else
                        {
                            kernel = shader.FindKernel("F1DistanceSeamlessVoronoi");
                        }
                        break;
                    case CellularType.F2:
                        if(!seamless)
                        {
                            kernel = shader.FindKernel("F2DistanceVoronoi");
                        }
                        else
                        {
                            kernel = shader.FindKernel("F2DistanceSeamlessVoronoi");
                        }
                        break;
                    case CellularType.DistanceSub:
                        if (!seamless)
                        {
                            kernel = shader.FindKernel("FMinusDistanceVoronoi");
                        }
                        else
                        {
                            kernel = shader.FindKernel("FMinusDistanceSeamlessVoronoi");
                        }
                        break;
                    case CellularType.DistanceMul:
                        break;
                }
            }

            Compute(true);
        }

        public override void DrawInspector()
        {
            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Octaves");
            octaves = EditorGUILayout.IntField(octaves);
            GUILayout.EndVertical();

            if (!seamless)
            {
                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Frequency");
                frequency = EditorGUILayout.FloatField(frequency);
                GUILayout.EndVertical();
            }

            else
            {
                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("XScale");
                XScale = EditorGUILayout.FloatField(XScale);
                GUILayout.EndVertical();

                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("YScale");
                YScale = EditorGUILayout.FloatField(YScale);
                GUILayout.EndVertical();
            }

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Cellular Type");
            type = (CellularType)EditorGUILayout.EnumPopup(type);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            seamless = EditorGUILayout.Toggle("Seamless", seamless);
            GUILayout.EndVertical();

            base.DrawInspector();
            
        }

        public override void Compute(bool selfcompute = false)
        {
           if(selfcompute)
           {
                if(shader!= null)
                {
                    shader.SetTexture(kernel, "Result", texture);
                    shader.SetFloat("ressolution", (float)ressolution.x);
                    shader.SetInt("octaves", octaves);
                    shader.SetFloat("frequency", frequency);
                    shader.SetFloat("YScale", YScale);
                    shader.SetFloat("XScale", XScale);
                    shader.Dispatch(kernel, ressolution.x / 8, ressolution.y / 8, 1);
                }
           }

            if (outPoint.connections != null)
            {
                for (int i = 0; i < outPoint.connections.Count; i++)
                {
                    outPoint.connections[i].inPoint.node.Compute(true);
                }
            }
        }
    }
}
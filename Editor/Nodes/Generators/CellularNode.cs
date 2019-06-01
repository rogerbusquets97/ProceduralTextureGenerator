using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    public enum CellularType {F1, F2, DistanceSub, DistanceMul, OneMinusF};
    public class CellularNode : NodeBase
    {
        ConnectionPoint outPoint;
        CellularType type;
        CellularType lastType;

        float XScale;
        float YScale;
        float lastXScale;
        float lastYScale;
        float jitter;
        float lastJitter;
        float seed;
        float lastSeed;

        public CellularNode()
        {
            title = "Cellular Node";
            type = CellularType.F1;
            lastType = type;
            XScale = 10;
            YScale = 10;
            lastXScale = XScale;
            lastYScale = YScale;
            jitter = 1f;
            lastJitter = jitter;
            seed = 1000;
            lastSeed = seed;
        }

        private void OnEnable()
        {
            InitTexture();
            shader = (ComputeShader)Resources.Load("Worley");
            kernel = shader.FindKernel("F1DistanceSeamlessVoronoi");
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
                GUI.DrawTexture(new Rect(0 + rect.width / 8, 0 + rect.height / 5, rect.width - rect.width / 4, rect.height - rect.height / 4), texture);
            }

            GUILayout.EndArea();

            if (lastYScale!= YScale || lastXScale!= XScale || lastJitter != jitter || lastSeed != seed)
            {
                lastYScale = YScale;
                lastXScale = XScale;
                lastJitter = jitter;
                lastSeed = seed;
                Compute(true);
            }

            if (lastType != type)
            {
                lastType = type;

                switch (type)
                {
                    case CellularType.F1:
                        kernel = shader.FindKernel("F1DistanceSeamlessVoronoi");
                        break;
                    case CellularType.F2:
                        kernel = shader.FindKernel("F2DistanceSeamlessVoronoi");
                        break;
                    case CellularType.DistanceSub:
                        kernel = shader.FindKernel("FMinusDistanceSeamlessVoronoi");
                        break;
                    case CellularType.DistanceMul:
                        kernel = shader.FindKernel("FMultDistanceSeamlessVoronoi");
                        break;
                    case CellularType.OneMinusF:
                        kernel = shader.FindKernel("OneMinusDistanceSeamlessVoronoi");
                        break;
                }

                Compute(true);
            }

            
        }

        public override void DrawInspector()
        {
            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("XScale");
            XScale = EditorGUILayout.FloatField(XScale);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("YScale");
            YScale = EditorGUILayout.FloatField(YScale);
            GUILayout.EndVertical();
           
            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Cellular Type");
            type = (CellularType)EditorGUILayout.EnumPopup(type);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Jitter");
            jitter = EditorGUILayout.Slider(jitter,0f,1f);
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal("Box");
            EditorGUILayout.LabelField("Seed");
            seed = EditorGUILayout.FloatField(seed);
            if (GUILayout.Button("Random"))
            {
                seed = UnityEngine.Random.Range(0f, 1000f);
            }
            GUILayout.EndHorizontal();

            base.DrawInspector();
            
        }

        public override void Compute(bool selfcompute = false)
        {
           if(selfcompute)
           {
                if(shader!= null)
                {
                    if (ressolution.x / 8 > 0)
                    {
                        shader.SetTexture(kernel, "Result", texture);
                        shader.SetFloat("ressolution", (float)ressolution.x);
                        shader.SetFloat("YScale", YScale);
                        shader.SetFloat("XScale", XScale);
                        shader.SetFloat("Jitter", jitter);
                        shader.SetFloat("seed", seed);
                        shader.Dispatch(kernel, ressolution.x / 8, ressolution.y / 8, 1);
                    }
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    public enum FractalType { FractalBrownianMotion, Billow, Ridged };
    public class FractalNode : NodeBase
    {
        ConnectionPoint outPoint;

        FractalType type;
        FractalType lastType;
        float frequency;
        float lastFrequency;
        int octaves;
        int lastOctaves;


        public FractalNode()
        {
            title = "Fractal Noise";
            type = FractalType.FractalBrownianMotion;
            lastType = type;
            frequency = 0.001f;
            lastFrequency = frequency;
            octaves = 20;
            lastOctaves = octaves;
        }

        private void OnEnable()
        {
            InitTexture();
            shader = (ComputeShader)Resources.Load("Noises");
            kernel = shader.FindKernel("FractalBrownianMotion");
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
             if(texture!=null)
             {
                 GUI.DrawTexture(new Rect((rect.width / 4) - 15, (rect.height / 4) - 8, rect.width - 20, rect.height - 20), texture);
             }

            GUILayout.EndArea();

            if(lastOctaves!= octaves || lastFrequency!= frequency )
            {
                lastFrequency = frequency;
                lastOctaves = octaves;
                Compute(true);
            }

            if(lastType!= type)
            {
                lastType = type;

                switch(type)
                {
                    case FractalType.FractalBrownianMotion:
                        kernel = shader.FindKernel("FractalBrownianMotion");
                        break;
                    case FractalType.Billow:
                        kernel = shader.FindKernel("FractalBillow");
                        break;
                    case FractalType.Ridged:
                        kernel = shader.FindKernel("FractalRidged");
                        break;
                }

                Compute(true);
            }
              

        }

        public override void DrawInspector()
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Octaves");
            octaves = EditorGUILayout.IntField(octaves);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Frequency");
            frequency = EditorGUILayout.FloatField(frequency);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Fractal Type");
            type = (FractalType)EditorGUILayout.EnumPopup(type);
            GUILayout.EndVertical();

            base.DrawInspector();
        }
      
        public override void Compute(bool selfcompute = false)
        {
            if(selfcompute)
            {
                if (shader != null)
                {
                    shader.SetTexture(kernel, "Result", texture);
                    shader.SetFloat("ressolution", (float)ressolution.x);
                    shader.SetInt("octaves", octaves);
                    shader.SetFloat("frequency", frequency);
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

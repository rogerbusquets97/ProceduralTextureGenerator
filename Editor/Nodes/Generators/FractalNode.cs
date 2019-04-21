using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    public enum FractalType { FractalBrownianMotion = 0, Billow, Ridged };
    public class FractalNode : NodeBase
    {
        ConnectionPoint outPoint;

        public FractalType type;
        FractalType lastType;

        public int     scale_x;
        public int     scale_y;
        public float   scale_value;
        public int     start_band;
        public int     end_band;
        public float   persistance;

        int     last_scale_x;
        int     last_scale_y;
        float   last_scale_value;
        int     last_start_band;
        int     last_end_band;
        float   last_persistance;


        public FractalNode()
        {
            title = "Fractal Noise";
            type = FractalType.FractalBrownianMotion;
            lastType = type;

            scale_x = 4;
            scale_y = 4;
            scale_value = 0.7f;
            start_band = 1;
            end_band = 8;
            persistance = 0.5f;

            last_scale_x = scale_x;
            last_scale_y = scale_y;
            last_scale_value = scale_value;
            last_start_band = start_band;
            last_end_band = end_band;
            last_persistance = persistance;
        }

        private void OnEnable()
        {
            InitTexture();
            shader = (ComputeShader)Resources.Load("FractalNoises");
            kernel = shader.FindKernel("Liquid");
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

           
            if(last_end_band != end_band || last_scale_value!= scale_value || last_scale_x != scale_x || last_scale_y!= scale_y || last_start_band!= start_band || last_persistance!= persistance || lastType!= type)
            {
                last_scale_x = scale_x;
                last_scale_y = scale_y;
                last_scale_value = scale_value;
                last_start_band = start_band;
                last_end_band = end_band;
                last_persistance = persistance;
                lastType = type;
                Compute(true);
            }
        }

        public override void DrawInspector()
        {

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Scale X");
            scale_x = EditorGUILayout.IntField(scale_x);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Scale Y");
            scale_y = EditorGUILayout.IntField(scale_y);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Start Band");
            start_band = EditorGUILayout.IntField(start_band);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("End Band");
            end_band = EditorGUILayout.IntField(end_band);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Scale Value");
            scale_value = EditorGUILayout.Slider(scale_value,0f,1f);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Persistance");
            persistance = EditorGUILayout.Slider(persistance,0f,1f);
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
                    shader.SetInt("scale_x", scale_x);
                    shader.SetInt("scale_y", scale_y);
                    shader.SetInt("start_band", start_band);
                    shader.SetInt("end_band", end_band);
                    shader.SetFloat("scale_value", scale_value);
                    shader.SetFloat("persistance", persistance);
                    shader.SetInt("type", (int)type);
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

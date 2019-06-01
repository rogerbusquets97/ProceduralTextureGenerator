using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{

    public class Parabola : NodeBase
    {
        ConnectionPoint outPoint;
        float count;
        float lastCount;
        float width;
        float lastWidth;

        public Parabola()
        {
            title = "Parabola";
            count = 1f;
            lastCount = count;
            width = 1f;
            lastWidth = width;
        }

        private void OnEnable()
        {
            InitTexture();
            shader = (ComputeShader)Resources.Load("Parabola");
            kernel = shader.FindKernel("Parabola");
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

        public override void Draw()
        {
            base.Draw();
            GUILayout.BeginArea(rect);
            if (texture != null)
            {
                GUI.DrawTexture(new Rect(0 + rect.width / 8, 0 + rect.height / 5, rect.width - rect.width / 4, rect.height - rect.height / 4), texture);
            }
            GUILayout.EndArea();

            if(lastWidth != width || lastCount != count)
            {
                lastCount = count;
                lastWidth = width;
                Compute(true);
            }
        }

        public override void DrawInspector()
        {
            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Count");
            count = EditorGUILayout.FloatField(count);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Width");
            width = EditorGUILayout.FloatField(width);
            GUILayout.EndVertical();

            base.DrawInspector();
        }

        public override void Compute(bool selfcompute = false)
        {
            if(selfcompute)
            {
                if(shader!= null)
                {
                    if(ressolution.x/8 > 0)
                    {
                        shader.SetTexture(kernel, "Result", texture);
                        shader.SetFloat("ressolution", (float)ressolution.x);
                        shader.SetFloat("count", count);
                        shader.SetFloat("width", width);
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
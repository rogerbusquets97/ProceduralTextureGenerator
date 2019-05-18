using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    public class FlatColor : NodeBase
    {
        Color color;
        Color lastColor;
        ConnectionPoint outPoint; 

        public FlatColor()
        {
            title = "Flat Color";
            color = Color.black;
            lastColor = color;
        }

        public void OnEnable()
        {
            InitTexture();
            shader = (ComputeShader)Resources.Load("Color");
            kernel = shader.FindKernel("FlatColor");
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
                GUI.DrawTexture(new Rect((rect.width / 4) - 15, (rect.height / 4) - 8, rect.width - 20, rect.height - 20), texture);
            }
            GUILayout.EndArea();

            if(lastColor!= color)
            {
                lastColor = color;
                Compute(true);
            }
        }

        public override void DrawInspector()
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Color");
            color = EditorGUILayout.ColorField(color);
            GUILayout.EndVertical();

            base.DrawInspector();
        }

        public override void Compute(bool selfcompute = false)
        {
            if (selfcompute)
            {
                if (color != null)
                {
                    if (ressolution.x / 8 > 0)
                    {
                        shader.SetTexture(kernel, "Result", texture);
                        shader.SetFloats("MainColor", color.r, color.g, color.b, color.a);
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
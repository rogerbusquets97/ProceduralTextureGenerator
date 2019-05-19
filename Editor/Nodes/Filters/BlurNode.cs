using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
namespace PTG
{

    public class BlurNode : NodeBase
    {
        RenderTexture source;
        ConnectionPoint inPoint;
        ConnectionPoint outPoint;

        Vector2 direction;
        Vector2 lastDirection;
        float strength;
        float lastStrength;

        public BlurNode()
        {
            title = "Blur";
            direction = new Vector2(1, 1);
            lastDirection = direction;
            strength = 2f;
            lastStrength = strength;
        }

        public void OnEnable()
        {
            InitTexture();
            shader = (ComputeShader)Resources.Load("Blur");
            kernel = shader.FindKernel("Blur");
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

            inPoint = new ConnectionPoint(this, ConnectionType.In, inPointStyle, OnClickInPoint);

            if (inPoints == null)
            {
                inPoints = new List<ConnectionPoint>();
            }

            inPoints.Add(inPoint);

            OnRemoveNode = OnClickRemoveNode;
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

            if(lastDirection!= direction || lastStrength!= strength)
            {
                lastDirection = direction;
                lastStrength = strength;
                Compute(true);
            }
        }

        public override void DrawInspector()
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical("Box");
            direction = EditorGUILayout.Vector2Field("Direction", direction);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            strength = EditorGUILayout.FloatField("Strength", strength);
            GUILayout.EndVertical();

            base.DrawInspector();
        }
        public override object GetValue(int x, int y)
        {
            return 0;
        }


        public override void Compute(bool selfcompute = false)
        {
            if (selfcompute)
            {
                NodeBase n = null;

                if (inPoint.connections.Count != 0)
                {
                    n = inPoint.connections[0].outPoint.node;

                    source = n.GetTexture();

                    if(n.ressolution != this.ressolution)
                    {
                        ChangeRessolution(n.ressolution.x);
                    }

                    if (shader != null)
                    {
                        if (ressolution.x / 8 > 0)
                        {
                            shader.SetTexture(kernel, "Result", texture);
                            shader.SetTexture(kernel, "source", source);
                            shader.SetFloat("ressolution", (float)ressolution.x);
                            shader.SetFloats("direction", direction.x, direction.y);
                            shader.SetFloat("radius", strength);
                            shader.Dispatch(kernel, ressolution.x / 8, ressolution.y / 8, 1);
                        }
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

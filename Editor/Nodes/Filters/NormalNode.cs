using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    public class NormalNode : NodeBase
    {
        RenderTexture source;

        ConnectionPoint inPoint;
        ConnectionPoint outPoint;

        float strength;
        float lastStrength;


        public NormalNode()
        {
            title = "Normal";
            strength = 4f;
            lastStrength = strength;
        }

        public void OnEnable()
        {
            InitTexture();
            shader = (ComputeShader)Resources.Load("Normal");
            kernel = shader.FindKernel("Normal");
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

        /*public override void StartComputeThread(bool selfCompute)
        {
            NodeBase n = null;
            if(inPoint.connections.Count!=0)
            {
                n = inPoint.connections[0].outPoint.node;
            }
            if(n!=null)
            {
                if(n.GetTexture()!= null)
                {
                    //source = n.GetTexture().GetPixels();
                    base.StartComputeThread(selfCompute);
                }
            }
        }*/

        public override void Draw()
        {
            base.Draw();
            GUILayout.BeginArea(rect);
            if (texture != null)
            {
                GUI.DrawTexture(new Rect(0 + rect.width / 8, 0 + rect.height / 5, rect.width - rect.width / 4, rect.height - rect.height / 4), texture);
            }
            GUILayout.EndArea();

            if(lastStrength!= strength)
            {
                lastStrength = strength;
                Compute(true);
            }
        }

        public override void DrawInspector()
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical("Box");
            GUILayout.Label("Strength");
            strength = EditorGUILayout.Slider(strength,0f,100f);
            GUILayout.EndVertical();
            base.DrawInspector();
        }

        public override object GetValue(int x, int y)
        {
            return 0;
        }

        public override void Compute(bool selfcompute = false)
        {
            NodeBase n = null;
            if (inPoint.connections.Count != 0)
            {
                n = inPoint.connections[0].outPoint.node;
            }

            if (n != null)
            {
                if (n.GetTexture() != null)
                {
                    source = n.GetTexture();
                }
            }
            if (selfcompute)
            {
                if (source != null && texture != null)
                {
                    if(n.ressolution != ressolution)
                    {
                        ChangeRessolution(n.ressolution.x);
                    }
                    if (ressolution.x / 8 > 0)
                    {
                        if (shader != null)
                        {
                            shader.SetTexture(kernel, "Result", texture);
                            shader.SetTexture(kernel, "source", source);
                            shader.SetFloat("strength", strength);
                            shader.SetFloat("ressolution", (float)ressolution.x);
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

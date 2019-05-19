using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    public class WarpNode : NodeBase
    {
        RenderTexture source;
        RenderTexture warper;

        ConnectionPoint inSource;
        ConnectionPoint inWarper;
        ConnectionPoint outPoint;

        float strength;
        float lastStrength;

        public WarpNode()
        {
            title = "Warp";
            strength = 0.01f;
            lastStrength = strength;
        }

        public void OnEnable()
        {
            InitTexture();
            shader = (ComputeShader)Resources.Load("Warp");
            kernel = shader.FindKernel("Warp");
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

            inSource = new ConnectionPoint(this, ConnectionType.In, inPointStyle, OnClickInPoint);
            inWarper = new ConnectionPoint(this, ConnectionType.In, inPointStyle, OnClickInPoint);

            if (inPoints == null)
            {
                inPoints = new List<ConnectionPoint>();
            }

            inPoints.Add(inSource);
            inPoints.Add(inWarper);

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

            if (lastStrength!= strength)
            {
                lastStrength = strength;
                Compute(true);
            }
        }

        public override void DrawInspector()
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical("Box");
            EditorGUILayout.HelpBox("All inputs MUST have the same ressolution", MessageType.Info);
            GUILayout.Label("Strength");
            strength = EditorGUILayout.Slider(strength,0f,1f);
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
                NodeBase s = null;
                NodeBase w = null;

                if (inSource.connections.Count != 0 && inWarper.connections.Count != 0)
                {
                    s = inSource.connections[0].outPoint.node;
                    w = inWarper.connections[0].outPoint.node;

                    source = s.GetTexture();
                    warper = w.GetTexture();

                    if (s.ressolution == w.ressolution)
                    {
                        if(s.ressolution!=ressolution)
                        {
                            ChangeRessolution(s.ressolution.x);
                        }
                        if (ressolution.x / 8 > 0)
                        {
                            if (shader != null)
                            {
                                shader.SetTexture(kernel, "Result", texture);
                                shader.SetTexture(kernel, "source", source);
                                shader.SetTexture(kernel, "warper", warper);
                                shader.SetFloat("ressolution", (float)ressolution.x);
                                shader.SetFloat("strength", strength);
                                shader.Dispatch(kernel, ressolution.x / 8, ressolution.y / 8, 1);
                            }
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

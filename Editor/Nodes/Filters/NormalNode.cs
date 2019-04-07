﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    public class NormalNode : NodeBase
    {
        Color[] outPixels;
        Color[] source;

        ConnectionPoint inPoint;
        ConnectionPoint outPoint;

        float strength;
        float lastStrength;


        public NormalNode()
        {
            title = "Normal";
            door = new object();
            strength = 4f;
            lastStrength = strength;
        }

        public void OnEnable()
        {
            ressolution = new Vector2Int(256, 256);
            texture = new Texture2D(ressolution.x, ressolution.y);
            outPixels = texture.GetPixels();
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

        public override void StartComputeThread(bool selfCompute)
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
                    source = n.GetTexture().GetPixels();
                    base.StartComputeThread(selfCompute);
                }
            }
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

            if(lastStrength!= strength)
            {
                lastStrength = strength;
                StartComputeThread(true);
            }
        }

        public override void DrawInspector()
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical("Box");
            GUILayout.Label("Strength");
            strength = EditorGUILayout.FloatField(strength);
            GUILayout.EndVertical();
            base.DrawInspector();
        }

        public override object GetValue(int x, int y)
        {
            return 0;
        }

        public override void Compute(bool selfcompute = false)
        {
            if(source!= null)
            {
                lock(door)
                {
                    outPixels = Filter.Normal(ressolution, source, strength);
                }
            }

            Action MainThreadAction = () =>
            {
                if (selfcompute)
                {
                    texture.SetPixels(outPixels);
                    texture.wrapMode = TextureWrapMode.Clamp;
                    texture.Apply();
                    editor.Repaint();
                }

                if (outPoint.connections != null)
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

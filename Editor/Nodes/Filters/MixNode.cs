﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    public class MixNode : NodeBase
    {
        Color[] outPixels;
        Color[] source;

        Color color1;
        Color color2;

        Color lastColor1;
        Color lastColor2;

        ConnectionPoint inPoint;
        ConnectionPoint outPoint;

        public MixNode()
        {
            title = "Mix";
            color1 = Color.white;
            color2 = Color.black;

            lastColor1 = color1;
            lastColor2 = color2;
        }

        public void OnEnable()
        {
            InitTexture();
            //outPixels = texture.GetPixels();
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
            if(inPoint.connections.Count!= 0)
            {
                n = inPoint.connections[0].outPoint.node;
            }

            if(n!= null)
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
                GUI.DrawTexture(new Rect((rect.width / 4) - 15, (rect.height / 4) - 8, rect.width - 20, rect.height - 20), texture);
            }
            GUILayout.EndArea();

            if(lastColor1 != color1 || lastColor2 != color2)
            {
                lastColor1 = color1;
                lastColor2 = color2;
                Compute(true);
            }
        }

        public override void DrawInspector()
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical("Box");
            GUILayout.Label("Color 1");
            color1 = EditorGUILayout.ColorField(color1);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            GUILayout.Label("Color 2");
            color2 = EditorGUILayout.ColorField(color2);
            GUILayout.EndVertical();

            base.DrawInspector();
        }

        public override object GetValue(int x, int y)
        {
            return Filter.GetSingleMixValue(ressolution, x, y, source, color1, color2);
        }

        public override void Compute(bool selfcompute = false)
        {
           if(source!= null)
           {
           }

            Action MainThreadAction = () =>
            {
                if (selfcompute)
                {
                    //texture.SetPixels(outPixels);
                    texture.wrapMode = TextureWrapMode.Clamp;
                    //texture.Apply();
                    editor.Repaint();
                }

                if (outPoint.connections != null)
                {
                    for (int i = 0; i < outPoint.connections.Count; i++)
                    {
                        outPoint.connections[i].inPoint.node.Compute(true);
                    }
                }
            };
        }
    }
}
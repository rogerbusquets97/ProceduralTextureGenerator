using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{

    public class Polygon : NodeBase
    {
        ConnectionPoint outPoint;
        int Vertices;
        int lastVertices;

        float falloff;
        float lastFalloff;

        float size;
        float lastSize;

        bool rounded;
        bool lastRounded;

        public Polygon()
        {
            title = "Polygon";
            Vertices = 3;
            lastVertices = Vertices;
            falloff = 0.07f;
            lastFalloff = falloff;
            size = 0.5f;
            lastSize = size;
            rounded = false;
            lastRounded = rounded;
        }

        private void OnEnable()
        {
            InitTexture();
            shader = (ComputeShader)Resources.Load("Polygon");
            kernel = shader.FindKernel("Polygon");
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
            //Texture
            if (texture != null)
            {
                GUI.DrawTexture(new Rect(0 + rect.width / 8, 0 + rect.height / 5, rect.width - rect.width / 4, rect.height - rect.height / 4), texture);
            }

            GUILayout.EndArea();

            if(lastVertices != Vertices || lastFalloff != falloff || lastSize != size || lastRounded != rounded)
            {
                lastVertices = Vertices;
                lastFalloff = falloff;
                lastSize = size;
                lastRounded = rounded;
                Compute(true);
            }
        }

        public override void DrawInspector()
        {

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Vertices Count");
            Vertices = EditorGUILayout.IntField(Vertices);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Falloff");
            falloff = EditorGUILayout.Slider(falloff, 0f, 1f);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Size");
            size = EditorGUILayout.Slider(size, 0f, 1f);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Rounded");
            rounded = EditorGUILayout.Toggle(rounded);
            GUILayout.EndVertical();


            base.DrawInspector();
        }

        public override void Compute(bool selfcompute = false)
        {
            if(selfcompute)
            {
                if(shader != null)
                {
                    if(ressolution.x/8 > 0)
                    {
                        shader.SetTexture(kernel, "Result", texture);
                        shader.SetInt("vertices", Vertices);
                        shader.SetFloat("ressolution", ressolution.x);
                        shader.SetFloat("falloff", falloff);
                        shader.SetFloat("size", size);
                        shader.SetBool("rounded", rounded);
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

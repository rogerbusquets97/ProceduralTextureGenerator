using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
namespace PTG
{ 
    public class MakeItTile : NodeBase
    {
        RenderTexture source;
        ConnectionPoint inPoint;
        ConnectionPoint outPoint;
        Vector2 tilling;
        Vector2 lastTilling;

        float offsetX;
        float lastOffsetX;

        float offsetY;
        float lastOffsetY;

        float scale;
        float lastScale;

        float angle;
        float lastAngle;

        float globalAngle;
        float lastGlobalAngle;

        public MakeItTile()
        {
            title = "Transform";
            tilling = new Vector2(1f, 1f);
            lastTilling = tilling;

            offsetX = 0.0f;
            lastOffsetX = offsetX;

            offsetY = 0.0f;
            lastOffsetY = offsetY;

            scale = 1f;
            lastScale = scale;

            angle = 0f;
            lastAngle = angle;

            globalAngle = 0f;
            lastGlobalAngle = globalAngle;

        }

        public void OnEnable()
        {
            InitTexture();
            shader = (ComputeShader)Resources.Load("MakeItTile");
            kernel = shader.FindKernel("Tile");
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

            if (lastTilling != tilling || lastOffsetX!= offsetX || lastOffsetY!=offsetY || lastScale!= scale
                || lastAngle!=angle || lastGlobalAngle!= globalAngle)
            {
                lastTilling = tilling;
                lastOffsetX = offsetX;
                lastOffsetY = offsetY;
                lastScale = scale;
                lastAngle = angle;
                lastGlobalAngle = globalAngle;
                Compute(true);
            }
        }

        public override void DrawInspector()
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical("Box");
            tilling = EditorGUILayout.Vector2Field("Tilling",tilling);
            GUILayout.EndVertical();
            GUILayout.BeginVertical("Box");
            GUILayout.Label("Horizontal Offset");
            offsetX = EditorGUILayout.Slider(offsetX, 0f, 1f);
            GUILayout.Label("Vertical Offset");
            offsetY = EditorGUILayout.Slider(offsetY, 0f, 1f);
            GUILayout.EndVertical();
            /*GUILayout.BeginVertical("Box");
            GUILayout.Label("Scale");
            scale = EditorGUILayout.Slider(scale, 0f, 1f);
            GUILayout.EndVertical();*/
            GUILayout.BeginVertical("Box");
            GUILayout.Label("Global Rotation angle");
            globalAngle = EditorGUILayout.Slider(globalAngle, 0f, 1f);
           /* GUILayout.Label("Tile Rotation angle");
            angle = EditorGUILayout.Slider(angle, 0f, 1f);
            GUILayout.EndVertical();*/

            base.DrawInspector();
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
                    if (n.ressolution != this.ressolution)
                    {
                        ChangeRessolution(n.ressolution.x);
                    }
                }
            }
            if (selfcompute)
            {
                if (source != null && texture != null)
                {
                    if (shader != null)
                    {
                        if (ressolution.x / 8 > 0)
                        {
                            shader.SetTexture(kernel, "Result", texture);
                            shader.SetTexture(kernel, "source", source);
                            shader.SetFloat("ressolution", ressolution.x);
                            shader.SetFloats("tilling", tilling.x,tilling.y);
                            shader.SetFloat("offsetX", offsetX);
                            shader.SetFloat("offsetY", offsetY);
                            shader.SetFloat("scale", scale);
                            shader.SetFloat("angle", angle);
                            shader.SetFloat("globalangle", globalAngle);

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
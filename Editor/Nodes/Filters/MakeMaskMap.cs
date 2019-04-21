using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    public class MakeMaskMap : NodeBase
    {
        RenderTexture R;
        RenderTexture G;
        RenderTexture B;
        RenderTexture A;

        ConnectionPoint inRed;
        ConnectionPoint inGreen;
        ConnectionPoint inBlue;
        ConnectionPoint inAlpha;

        ConnectionPoint outPoint;

        public MakeMaskMap()
        {
            title = "Mask Map";
        }

        private void OnEnable()
        {
            InitTexture();
            shader = (ComputeShader)Resources.Load("MaskMap");
            kernel = shader.FindKernel("MaskMap");
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

            inRed = new ConnectionPoint(this, ConnectionType.In, inPointStyle, OnClickInPoint);
            inGreen = new ConnectionPoint(this, ConnectionType.In, inPointStyle, OnClickInPoint);
            inBlue = new ConnectionPoint(this, ConnectionType.In, inPointStyle, OnClickInPoint);
            inAlpha = new ConnectionPoint(this, ConnectionType.In, inPointStyle, OnClickInPoint);
            if (inPoints == null)
            {
                inPoints = new List<ConnectionPoint>();
            }

            inPoints.Add(inRed);
            inPoints.Add(inGreen);
            inPoints.Add(inBlue);
            inPoints.Add(inAlpha);

            OnRemoveNode = OnClickRemoveNode;
        }

        public override void Draw()
        {
            base.Draw();
            GUI.Label(new Rect(inRed.rect.x + 15, inRed.rect.y, 400, 40), "Metallic");
            GUI.Label(new Rect(inGreen.rect.x + 15, inGreen.rect.y, 400, 40), "AO");
            GUI.Label(new Rect(inBlue.rect.x + 15, inBlue.rect.y, 400, 40), "Detail Map");
            GUI.Label(new Rect(inAlpha.rect.x + 15, inAlpha.rect.y, 400, 40), "Smoothness");

        }

        public override void DrawInspector()
        {
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
                if (inRed.connections.Count != 0)
                {
                    n = inRed.connections[0].outPoint.node;
                    R = n.GetTexture();
                }
                if (inGreen.connections.Count != 0)
                {
                    n = inGreen.connections[0].outPoint.node;
                    G = n.GetTexture();
                }
                if (inBlue.connections.Count != 0)
                {
                    n = inBlue.connections[0].outPoint.node;
                    B = n.GetTexture();
                }
                if (inAlpha.connections.Count != 0)
                {
                    n = inAlpha.connections[0].outPoint.node;
                    A = n.GetTexture();
                }

                if (R != null && G != null && B != null && A != null && texture!= null)
                {
                    shader.SetTexture(kernel, "Result", texture);
                    shader.SetTexture(kernel, "R", R);
                    shader.SetTexture(kernel, "G", G);
                    shader.SetTexture(kernel, "B", B);
                    shader.SetTexture(kernel, "A", A);
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

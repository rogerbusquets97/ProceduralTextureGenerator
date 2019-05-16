using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    public class BlendNode : NodeBase
    {
        Filter.BlendMode mode;
        Filter.BlendMode lastMode;

        RenderTexture A;
        RenderTexture B;
        RenderTexture Mask; 

        ConnectionPoint Apoint;
        ConnectionPoint Bpoint;
        ConnectionPoint maskPoint;

        ConnectionPoint outPoint;

        public BlendNode()
        {
            title = "Blend";
            mode = Filter.BlendMode.Multiply;
            lastMode = mode;
        }

        public void OnEnable()
        {
            InitTexture();
            shader = (ComputeShader)Resources.Load("Blends");
            kernel = shader.FindKernel("BlendMultiply");
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

            Apoint = new ConnectionPoint(this, ConnectionType.In, inPointStyle, OnClickInPoint);
            Bpoint = new ConnectionPoint(this, ConnectionType.In, inPointStyle, OnClickInPoint);
            maskPoint = new ConnectionPoint(this, ConnectionType.In, inPointStyle, OnClickInPoint);

            if(inPoints == null)
            {
                inPoints = new List<ConnectionPoint>();
            }

            inPoints.Add(Apoint);
            inPoints.Add(Bpoint);
            inPoints.Add(maskPoint);

            OnRemoveNode = OnClickRemoveNode;
        }

        /*public override void StartComputeThread(bool selfCompute)
        {
            NodeBase n = null;
            NodeBase n2 = null;
            
            if(Apoint.connections.Count!= 0 && Bpoint.connections.Count!= 0)
            {
                //InPoints only has 1 connection
                n = Apoint.connections[0].outPoint.node;
                n2 = Bpoint.connections[0].outPoint.node;
            }
            if (n!= null && n2!= null)
            {
                if (n.GetTexture() != null && n2.GetTexture() != null)
                {
                    if (mode == Filter.BlendMode.Mask)
                    {
                        if (maskPoint.connections.Count!= 0)
                        {
                            //mask = maskPoint.connections[0].outPoint.node.GetTexture().GetPixels();
                        }
                    }

                    //Apixels = n.GetTexture().GetPixels();
                    //Bpixels = n2.GetTexture().GetPixels();

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

            if(lastMode != mode)
            {
                lastMode = mode;
                Compute(true);
            }
        }

        public override void DrawInspector()
        {
            GUILayout.Space(10);

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Blend Mode");
            mode = (Filter.BlendMode)EditorGUILayout.EnumPopup(mode);
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
            NodeBase n2 = null;

            if (Apoint.connections.Count != 0 && Bpoint.connections.Count != 0)
            {
                //InPoints only has 1 connection
                n = Apoint.connections[0].outPoint.node;
                n2 = Bpoint.connections[0].outPoint.node;

                A = n.GetTexture();
                B = n2.GetTexture();
            }

            if(maskPoint.connections.Count != 0)
            {
                NodeBase mask = maskPoint.connections[0].outPoint.node;
                Mask = mask.GetTexture();
            }

            if (selfcompute)
            {
                if (A != null && B != null && texture!= null)
                {
                    switch (mode)
                    {
                        case Filter.BlendMode.Multiply:
                            if (shader != null)
                            {
                                kernel = shader.FindKernel("BlendMultiply");
                                shader.SetTexture(kernel, "Result", texture);
                                shader.SetTexture(kernel, "A", A);
                                shader.SetTexture(kernel, "B", B);
                                shader.Dispatch(kernel, ressolution.x / 8, ressolution.y / 8, 1);
                            }
                            break;
                        case Filter.BlendMode.Addition:
                            kernel = shader.FindKernel("BlendAddition");
                            shader.SetTexture(kernel, "Result", texture);
                            shader.SetTexture(kernel, "A", A);
                            shader.SetTexture(kernel, "B", B);
                            shader.Dispatch(kernel, ressolution.x / 8, ressolution.y / 8, 1);
                            break;
                        case Filter.BlendMode.Substraction:
                            kernel = shader.FindKernel("BlendSubstraction");
                            shader.SetTexture(kernel, "Result", texture);
                            shader.SetTexture(kernel, "A", A);
                            shader.SetTexture(kernel, "B", B);
                            shader.Dispatch(kernel, ressolution.x / 8, ressolution.y / 8, 1);
                            break;
                        case Filter.BlendMode.Mask:
                            if (Mask != null)
                            {
                                kernel = shader.FindKernel("BlendMask");
                                shader.SetTexture(kernel, "Result", texture);
                                shader.SetTexture(kernel, "A", A);
                                shader.SetTexture(kernel, "B", B);
                                shader.SetTexture(kernel, "Mask", Mask);
                                shader.Dispatch(kernel, ressolution.x / 8, ressolution.y / 8, 1);
                            }
                            break;
                        case Filter.BlendMode.Screen:
                            kernel = shader.FindKernel("BlendScreen");
                            shader.SetTexture(kernel, "Result", texture);
                            shader.SetTexture(kernel, "A", A);
                            shader.SetTexture(kernel, "B", B);
                            shader.Dispatch(kernel, ressolution.x / 8, ressolution.y / 8, 1);
                            break;
                        case Filter.BlendMode.Overlay:
                            kernel = shader.FindKernel("BlendOverlay");
                            shader.SetTexture(kernel, "Result", texture);
                            shader.SetTexture(kernel, "A", A);
                            shader.SetTexture(kernel, "B", B);
                            shader.Dispatch(kernel, ressolution.x / 8, ressolution.y / 8, 1);
                            break;
                        case Filter.BlendMode.Max:
                            kernel = shader.FindKernel("BlendMax");
                            shader.SetTexture(kernel, "Result", texture);
                            shader.SetTexture(kernel, "A", A);
                            shader.SetTexture(kernel, "B", B);
                            shader.Dispatch(kernel, ressolution.x / 8, ressolution.y / 8, 1);
                            break;
                        case Filter.BlendMode.Min:
                            kernel = shader.FindKernel("BlendMin");
                            shader.SetTexture(kernel, "Result", texture);
                            shader.SetTexture(kernel, "A", A);
                            shader.SetTexture(kernel, "B", B);
                            shader.Dispatch(kernel, ressolution.x / 8, ressolution.y / 8, 1);
                            break;
                        case Filter.BlendMode.Divide:
                            kernel = shader.FindKernel("BlendDivide");
                            shader.SetTexture(kernel, "Result", texture);
                            shader.SetTexture(kernel, "A", A);
                            shader.SetTexture(kernel, "B", B);
                            shader.Dispatch(kernel, ressolution.x / 8, ressolution.y / 8, 1);
                            break;
                        case Filter.BlendMode.AddSub:
                            kernel = shader.FindKernel("BlendAddSub");
                            shader.SetTexture(kernel, "Result", texture);
                            shader.SetTexture(kernel, "A", A);
                            shader.SetTexture(kernel, "B", B);
                            shader.Dispatch(kernel, ressolution.x / 8, ressolution.y / 8, 1);
                            break;
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

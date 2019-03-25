using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    public class BlendNode : NodeBase
    {
        Color[] outPixels;
        Filter.BlendMode mode;
        Filter.BlendMode lastMode;
        Color[] mask = null;

        Color[] Apixels;
        Color[] Bpixels;

        ConnectionPoint Apoint;
        ConnectionPoint Bpoint;
        ConnectionPoint maskPoint;

        ConnectionPoint outPoint;

        public BlendNode()
        {
            title = "Blend";
            door = new object();
            mode = Filter.BlendMode.Multiply;
            lastMode = mode;
        }

        public void OnEnable()
        {
            ressolution = new Vector2Int(256, 256);
            texture = new Texture2D(ressolution.x, ressolution.y, TextureFormat.ARGB32, false);
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

        public override void StartComputeThread(bool selfCompute)
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
                            mask = maskPoint.connections[0].outPoint.node.GetTexture().GetPixels();
                        }
                    }

                    Apixels = n.GetTexture().GetPixels();
                    Bpixels = n2.GetTexture().GetPixels();

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
            else
            {
                Debug.Log("texture null");
            }

            GUILayout.EndArea();

            if(lastMode != mode)
            {
                lastMode = mode;
                StartComputeThread(true);
            }
        }

        public override void DrawInspector()
        {
            GUILayout.Space(10);

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Blend Mode");
            mode = (Filter.BlendMode)EditorGUILayout.EnumPopup(mode);
            GUILayout.EndVertical();
        }
        public override object GetValue(int x, int y)
        {
            if (Apixels != null && Bpixels != null)
                return Filter.GetSingleBlendValue(ressolution, x, y, Apixels, Bpixels, mode, mask);
            else
                return Color.black;
        }
        public override void Compute(bool selfcompute = false)
        {
            if(Apixels!= null && Bpixels!= null)
            {
                lock (door)
                {
                    outPixels = Filter.Blend(ressolution,  Apixels,  Bpixels, mode, mask);
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

                    System.IO.File.WriteAllBytes("Assets/Diffuse.png", texture.EncodeToPNG());
                    AssetDatabase.Refresh();
                }

                if (outPoint.connections != null)
                {
                    for(int i = 0; i< outPoint.connections.Count; i++)
                    {
                        outPoint.connections[i].inPoint.node.StartComputeThread(true);
                    }
                }
            };

            QueueMainThreadFunction(MainThreadAction);
        }
    }
}

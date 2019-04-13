using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    public class MakeMaskMap : NodeBase
    {
        Color[] outPixels;

        Color[] rChannel;
        Color[] gChannel;
        Color[] bChannel;
        Color[] aChannel;

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

       /* public override void StartComputeThread(bool selfCompute)
        {
            NodeBase n = null;
            if(inRed.connections.Count!= 0)
            {
                n = inRed.connections[0].outPoint.node;
                //rChannel = n.GetTexture().GetPixels();
            }
            if (inGreen.connections.Count != 0)
            {
                n = inGreen.connections[0].outPoint.node;
                //gChannel = n.GetTexture().GetPixels();
            }
            if (inBlue.connections.Count != 0)
            {
                n = inBlue.connections[0].outPoint.node;
                //bChannel = n.GetTexture().GetPixels();
            }
            if (inAlpha.connections.Count != 0)
            {
                n = inAlpha.connections[0].outPoint.node;
                //aChannel = n.GetTexture().GetPixels();
            }

            base.StartComputeThread(selfCompute);
        }
        */
        public override void Draw()
        {
            base.Draw();
            GUILayout.BeginArea(rect);
            if (texture != null)
            {
                GUI.DrawTexture(new Rect((rect.width / 4) - 15, (rect.height / 4) - 8, rect.width - 20, rect.width - 20), texture);
            }
            GUILayout.EndArea();
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
            if(rChannel!= null && bChannel!= null && gChannel!= null && aChannel!= null)
            {
                for(int i = 0; i<ressolution.x; i++)
                {
                    for(int j = 0; j<ressolution.y;j++)
                    {
                        float rValue = rChannel[Filter.GetIndex(i, j, ressolution.x, ressolution.y)].grayscale;
                        float gValue = gChannel[Filter.GetIndex(i, j, ressolution.x, ressolution.y)].grayscale;
                        float bValue = bChannel[Filter.GetIndex(i, j, ressolution.x, ressolution.y)].grayscale;
                        float aValue = aChannel[Filter.GetIndex(i, j, ressolution.x, ressolution.y)].grayscale;
                        outPixels[Filter.GetIndex(i, j, ressolution.x, ressolution.y)] = new Color(rValue, gValue, bValue, aValue);
                    }
                }
                
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
namespace PTG
{
    public class LevelsNode : NodeBase
    {
        Color[] outPixels;
        Color[] source;

        ConnectionPoint inPoint;
        ConnectionPoint outPoint;

        Filter.LevelsData data;
        Filter.LevelsData lastData;

        public LevelsNode()
        {
            title = "Levels Node";
            door = new object();
            data = new Filter.LevelsData(new Vector2(0, 1), new Vector2(0, 1));
            lastData = data;
        }

        public void OnEnable()
        {
            InitTexture();
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
            if(inPoint.connections.Count!= 0)
            {
                n = inPoint.connections[0].outPoint.node;
            }

            if(n!= null)
            {
                if(n.GetTexture() != null)
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

            if (lastData.inputLevels != data.inputLevels || lastData.outputLevels!=data.outputLevels)
            {
                lastData = data;
                StartComputeThread(true);
            }
        }

        public override void DrawInspector()
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical("Box");
            EditorGUILayout.MinMaxSlider("Input Levels", ref data.inputLevels.x, ref data.inputLevels.y, 0, 1);
            EditorGUILayout.MinMaxSlider("Output Levels", ref data.outputLevels.x, ref data.outputLevels.y, 0, 1);
            GUILayout.EndVertical();

            

            base.DrawInspector();
        }

        public override object GetValue(int x, int y)
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
                    source = n.GetTexture().GetPixels();
                    return Filter.GetSingleLevelsValue(ressolution, x, y, source, data);
                }
            }

            return 0;
        }

        public override void Compute(bool selfcompute = false)
        {
            if(source!= null)
            {
                lock(door)
                {
                    outPixels = Filter.Levels(ressolution, source, data);
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

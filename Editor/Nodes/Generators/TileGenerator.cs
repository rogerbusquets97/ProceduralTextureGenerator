using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    public class TileGenerator : NodeBase
    {
        ConnectionPoint outPoint;

        float size;
        float brickWidth;
        float brickHeight;
        float brickOffset;
        float brickGradient;
        float gradientAngle;
        float gradientStrength;
        float mortarWidth;
        float mortarHeight;
        float brickRotation;

        float lastSize;
        float lastBrickWidth;
        float lastbBrickHeight;
        float lastOffset;
        float lastBrickGradient;
        float lastGradientAngle;
        float lastGradientStrength;
        float lastMortarHeight;
        float lastMortarWidth;
        float lastBrickRotation;

        public TileGenerator()
        {
            title = "Tile Generator";

            size = 5f;
            brickWidth = 1f;
            brickHeight = 1f;
            brickOffset = 0.5f;
            brickGradient = 1f;
            gradientAngle = 0f;
            gradientStrength = 1f;
            mortarHeight = 0.9f;
            mortarWidth = 0.9f;
            brickRotation = 0f;

            lastSize = size;
            lastBrickWidth = brickWidth;
            lastbBrickHeight = brickHeight;
            lastOffset = brickOffset;
            lastBrickGradient = brickGradient;
            lastGradientAngle = gradientAngle;
            lastGradientStrength = gradientStrength;
            lastMortarHeight = mortarHeight;
            lastMortarWidth = mortarWidth;
            lastBrickRotation = brickRotation;


        }

        private void OnEnable()
        {
            InitTexture();
            shader = (ComputeShader)Resources.Load("TileGenerator");
            kernel = shader.FindKernel("Bricks");
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

        public override object GetValue(int x, int y)
        {
            return 0;
        }

        public override void Draw()
        {
            base.Draw();

            base.Draw();
            GUILayout.BeginArea(rect);
            //Texture
            if (texture != null)
            {
                GUI.DrawTexture(new Rect((rect.width / 4) - 15, (rect.height / 4) - 8, rect.width - 20, rect.height - 20), texture);
            }

            if(lastSize != size || lastBrickWidth !=  brickWidth || lastbBrickHeight != brickHeight || lastOffset!= brickOffset || lastBrickGradient!= brickGradient
                || lastGradientAngle!= gradientAngle || lastGradientAngle!= gradientStrength || lastMortarWidth != mortarWidth || lastMortarHeight!= mortarHeight
                || lastBrickRotation!= brickRotation)
            {
                lastSize = size;
                lastBrickWidth = brickWidth;
                lastbBrickHeight = brickHeight;
                lastOffset = brickOffset;
                lastBrickGradient = brickGradient;
                lastGradientAngle = gradientAngle;
                lastGradientStrength = gradientStrength;
                lastMortarHeight = mortarHeight;
                lastMortarWidth = mortarWidth;
                lastBrickRotation = brickRotation;

                Compute(true);
            }
            GUILayout.EndArea();
        }

        public override void DrawInspector()
        {
            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Size");
            size = EditorGUILayout.FloatField(size);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Brick Width");
            brickWidth = EditorGUILayout.Slider(brickWidth,0.1f,5f);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Brick Height");
            brickHeight = EditorGUILayout.Slider(brickHeight, 0.1f, 5f);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Mortar Width");
            mortarWidth = EditorGUILayout.Slider(mortarWidth, 0f, 1f);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Mortar Height");
            mortarHeight = EditorGUILayout.Slider(mortarHeight, 0f, 1f);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Brick Offset");
            brickOffset = EditorGUILayout.Slider(brickOffset, 0f, 0.9f);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Brick Rotation");
            brickRotation = EditorGUILayout.Slider(brickRotation, 0f, 2f);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Brick Gradient");
            brickGradient = EditorGUILayout.Slider(brickGradient, 0f, 1f);
            EditorGUILayout.LabelField("Gradient Angle");
            gradientAngle = EditorGUILayout.Slider(gradientAngle, 0f, 2f);
            EditorGUILayout.LabelField("Gradient Strength");
            gradientStrength = EditorGUILayout.Slider(gradientStrength, 0f, 5f);
            GUILayout.EndVertical();


            base.DrawInspector();
        }

        public override void Compute(bool selfcompute = false)
        {
            if(selfcompute)
            {
                if(shader!= null)
                {
                    if (ressolution.x / 8 > 0)
                    {
                        shader.SetTexture(kernel, "Result", texture);
                        shader.SetFloat("ressolution", (float)ressolution.x);
                        shader.SetFloat("size", size);
                        shader.SetFloat("brickWidth", brickWidth);
                        shader.SetFloat("brickHeight", brickHeight);
                        shader.SetFloat("brickOffset", brickOffset);
                        shader.SetFloat("brickGradient", brickGradient);
                        shader.SetFloat("gradientAngle", gradientAngle);
                        shader.SetFloat("gradientStrength", gradientStrength);
                        shader.SetFloat("mortarHeight", mortarHeight);
                        shader.SetFloat("mortarWidth", mortarWidth);
                        shader.SetFloat("brickRotation", brickRotation);
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

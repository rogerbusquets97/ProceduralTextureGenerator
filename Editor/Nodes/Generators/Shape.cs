using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PTG
{
    enum ShapeType { Square, Circle, Pyramid};
    public class Shape : NodeBase
    {
        ConnectionPoint outPoint;
        ShapeType shapeType;
        ShapeType lastShapeType;

        float radius;
        float lastRadius;
        float circleGradient;
        float lastCircleGradient;
        float squareWidth;
        float squareHeight;
        float lastSquareWidth;
        float lastSquareHeight;
        float smoothedges;
        float lastSmoothEdges;
        float squareGradient;
        float lastSquareGradient;
        float squareGradientStrength;
        float lastSquareGradientStrength;
        float squareGradientAngle;
        float lastSquareGradientAngle;
        
        public Shape()
        {
            title = "Shape";
            shapeType = ShapeType.Square;
            lastShapeType = shapeType;

            radius = 0.2f;
            lastRadius = radius;

            circleGradient = 0.1f;
            lastCircleGradient = circleGradient;

            squareWidth = 0.9f;
            lastSquareWidth = squareWidth;
            squareHeight = 0.9f;
            lastSquareHeight = squareHeight;
            smoothedges = 0.01f;
            lastSmoothEdges = smoothedges;
            squareGradient = 1f;
            lastSquareGradient = squareGradient;
            squareGradientStrength = 1f;
            lastSquareGradientStrength = squareGradientStrength;
            squareGradientAngle = 0f;
            lastSquareGradientAngle = squareGradientAngle;
        }

        private void OnEnable()
        {
            InitTexture();
            shader = (ComputeShader)Resources.Load("Shape");
            kernel = shader.FindKernel("Shape");
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

            if(lastShapeType != shapeType || lastRadius != radius || lastCircleGradient != circleGradient || lastSquareWidth != squareWidth || lastSquareHeight != squareHeight || lastSquareGradient != squareGradient || lastSquareGradientAngle != squareGradientAngle || lastSquareGradientStrength!= squareGradientStrength || lastSmoothEdges!=smoothedges)
            {
                lastShapeType = shapeType;
                lastRadius = radius;
                lastCircleGradient = circleGradient;
                lastSquareWidth = squareWidth;
                lastSquareHeight = squareHeight;
                lastSquareGradient = squareGradient;
                lastSquareGradientAngle = squareGradientAngle;
                lastSquareGradientStrength = squareGradientStrength;
                lastSmoothEdges = smoothedges;
                Compute(true);
            }
        }

        public override void DrawInspector()
        {
            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Shape Type");
            shapeType = (ShapeType)EditorGUILayout.EnumPopup(shapeType);
            GUILayout.EndVertical();

            if (shapeType == ShapeType.Circle)
            {
                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Radius");
                radius = EditorGUILayout.Slider(radius, 0f, 1f);
                GUILayout.EndVertical();

                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Circle Gradient");
                circleGradient = EditorGUILayout.Slider(circleGradient, 0f, 1f);
                GUILayout.EndVertical();
            }
            else if(shapeType == ShapeType.Square)
            {
                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Width");
                squareWidth = EditorGUILayout.Slider(squareWidth, 0f, 1f);
                GUILayout.EndVertical();

                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Height");
                squareHeight = EditorGUILayout.Slider(squareHeight, 0f, 1f);
                GUILayout.EndVertical();

                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Smooth Edges");
                smoothedges = EditorGUILayout.Slider(smoothedges, 0f, 1f);
                GUILayout.EndVertical();

                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Square Gradient");
                squareGradient = EditorGUILayout.Slider(squareGradient, 0f, 1f);
                EditorGUILayout.LabelField("Gradient Angle");
                squareGradientAngle = EditorGUILayout.Slider(squareGradientAngle, 0f, 2f);
                EditorGUILayout.LabelField("Gradient Strength");
                squareGradientStrength = EditorGUILayout.Slider(squareGradientStrength, 0f, 5f);
                GUILayout.EndVertical();
            }

            base.DrawInspector();
        }

        public override void Compute(bool selfcompute = false)
        {
            if(selfcompute)
            {
                if(shader != null)
                {
                    if(ressolution.x / 8 > 0)
                    {
                        shader.SetTexture(kernel, "Result", texture);
                        shader.SetFloat("ressolution", ressolution.x);
                        shader.SetInt("type", (int)shapeType);
                        shader.SetFloat("radius", radius);
                        shader.SetFloat("circleGradient", circleGradient);
                        shader.SetFloat("squareHeight", squareHeight);
                        shader.SetFloat("squareWidth", squareWidth);
                        shader.SetFloat("smoothEdges", smoothedges);
                        shader.SetFloat("squareGradient", squareGradient);
                        shader.SetFloat("squareGradientStrength", squareGradientStrength);
                        shader.SetFloat("squareGradientAngle", squareGradientAngle);
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

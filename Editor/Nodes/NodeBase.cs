using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Threading;

namespace PTG
{
    public enum NodeType { None = 0, Fractal}

    public class NodeBase
    {
        public Vector2Int ressolution;
        public Rect rect;
        public string title;
        public bool isDragged;
        public bool isSelected;

        protected object door;
        protected NodeEditorWindow editor;

        public List<ConnectionPoint> inPoints;
        public List<ConnectionPoint> outPoints;

        public List<Action> MainThreadActions = new List<Action>();
        public Action<NodeBase> OnRemoveNode;

        Texture2D texture;

        virtual public float GetValue(int x, int y) { return 0; }

        public Texture2D GetTexture()
        {
            return texture;
        }
        public void Drag(Vector2 delta)
        {
            rect.position += delta;
        }

        virtual public void StartComputeThread(bool selfCompute)
        {
            Thread nodeThread = new Thread(new ThreadStart(() => Compute(selfCompute)));
            nodeThread.Start();
        }
        virtual public void Compute(bool selfcompute = false)
        {

        }
        public virtual void Draw()
        {
            GUI.Box(rect, title);
        }

        public virtual void DrawInOutPoints()
        {
            if (inPoints != null)
            {
                for (int i = 0; i < inPoints.Count; i++)
                {
                    inPoints[i].Draw(i + 1);
                }
            }
            if (outPoints != null)
            {
                for (int i = 0; i < outPoints.Count; i++)
                {
                    outPoints[i].Draw(i + 1);
                }
            }

        }

        public virtual void Update()
        {
            while (MainThreadActions.Count > 0)
            {
                // Grab the first/oldest function in the list
                Action func = MainThreadActions[0];
                MainThreadActions.RemoveAt(0);

                // Now run it
                func();
            }
        }

        public void QueueMainThreadFunction(Action someFunction)
        {
            // We need to make sure that someFunction is running from the
            // main thread

            //someFunction(); // This isn't okay, if we're in a child thread

            MainThreadActions.Add(someFunction);
        }

        public bool ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {

                        if (this.rect.Contains(e.mousePosition))
                        {
                            isDragged = true;
                            GUI.changed = true;
                            isSelected = true;

                            editor.SetSelectedNode(this);

                        }
                        else
                        {
                            GUI.changed = true;
                            isSelected = false;

                        }
                    }

                    if (e.button == 1 && isSelected && rect.Contains(e.mousePosition))
                    {
                        ProcessContextMenu();
                        e.Use();
                    }
                    break;
                case EventType.MouseUp:
                    isDragged = false;
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0 && isDragged)
                    {
                        Drag(e.delta);
                        e.Use();
                        return true;
                    }
                    break;
            }
            return false;
        }
        protected void ProcessContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Remove Node"), false, OnClickRemoveNode);
            genericMenu.ShowAsContext();
        }

        protected void OnClickRemoveNode()
        {
            if(OnRemoveNode!= null)
            {
                OnRemoveNode(this);
            }
        }
    }

}
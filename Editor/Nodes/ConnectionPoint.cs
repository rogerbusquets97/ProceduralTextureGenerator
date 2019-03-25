using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PTG
{
    public enum ConnectionType { In, Out }
    public class ConnectionPoint
    {
        public Rect rect;
        public ConnectionType type;
        public GUIStyle style;
        public NodeBase node = null;

        public List<NodeConnection> connections = null;

        public Action<ConnectionPoint> OnClickConnectionPoint;

        public bool enabled = true;

        public ConnectionPoint(NodeBase node, ConnectionType type, GUIStyle style, Action<ConnectionPoint>OnClickConnectionPoint, bool enabled = false)
        {
            this.node = node;
            this.type = type;
            this.style = style;
            this.OnClickConnectionPoint = OnClickConnectionPoint;
            rect = new Rect(0, 0, 10f, 20f);
            connections = new List<NodeConnection>();
        }

        public void Draw(int id)
        {
            if(enabled)
            {
                rect.y = node.rect.y + (25 * id);

                switch (type)
                {
                    case ConnectionType.In:
                        rect.x = node.rect.x - rect.width + 1f;
                        break;
                    case ConnectionType.Out:
                        rect.x = node.rect.x + node.rect.width - 1;
                        break;
                }

                if (GUI.Button(rect, "", style))
                {
                    if (OnClickConnectionPoint != null)
                    {
                        OnClickConnectionPoint(this);
                    }
                }
            }
        }

        

    }
}

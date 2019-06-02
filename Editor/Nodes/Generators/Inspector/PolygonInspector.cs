using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace PTG
{
    [CustomEditor(typeof(Polygon))]
    public class PolygonInspector : Editor
    {
        Polygon n = null;

        public override void OnInspectorGUI()
        {
            n = (Polygon)target;
            if (n != null)
            {
                n.DrawInspector();
            }
        }
    }
}

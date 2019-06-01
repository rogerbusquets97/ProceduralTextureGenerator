using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace PTG
{
    [CustomEditor(typeof(Shape))]
    public class ShapeInspector : Editor
    {
        Shape n = null;

        public override void OnInspectorGUI()
        {
            n = (Shape)target;
            if (n != null)
            {
                n.DrawInspector();
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PTG
{
    [CustomEditor(typeof(Parabola))]
    public class ParabolaInspector : Editor
    {
        Parabola n = null;

        public override void OnInspectorGUI()
        {
            n = (Parabola)target;
            if (n != null)
            {
                n.DrawInspector();
            }
        }
    }
}
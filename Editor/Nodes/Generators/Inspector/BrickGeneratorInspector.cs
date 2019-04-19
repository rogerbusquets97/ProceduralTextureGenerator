using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PTG
{
    [CustomEditor(typeof(TileGenerator))]
    public class BrickGeneratorInspector : Editor
    {
        TileGenerator n = null;

        public override void OnInspectorGUI()
        {
            n = (TileGenerator)target;
            if (n != null)
            {
                n.DrawInspector();
            }
        }
    }
}

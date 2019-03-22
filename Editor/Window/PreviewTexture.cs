using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PreviewTexture : EditorWindow
{
    public Texture2D texture;

    private void OnEnable()
    {
        texture = new Texture2D(256,256);
    }
    private void OnGUI()
    {
        if (texture != null)
        {
            GUI.DrawTexture(new Rect(0, 0, position.width, position.height), texture);
        }
    }

}

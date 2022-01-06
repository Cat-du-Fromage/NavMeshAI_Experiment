using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Unity.Mathematics.math;

namespace KaizerWaldCode
{
    public static class RectangleSelectionUtils
    {
        private static Texture2D whiteTexture;
        private static readonly float screenHeight = Screen.height;
        private static Texture2D WhiteTexture => whiteTexture == null ? GetWhiteTexture() : whiteTexture;
        private static Texture2D GetWhiteTexture()
        {
            whiteTexture = new Texture2D(1, 1);
            whiteTexture.SetPixel(0, 0, Color.white);
            whiteTexture.Apply();
            return whiteTexture;
        }
    
        public static void DrawScreenRect(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, WhiteTexture);
            GUI.color = Color.white;
        }
    
        public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
        {
            // Top
            DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
            // Left
            DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
            // Right
            DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
            // Bottom
            DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
        }
    
        public static Rect GetScreenRect(Vector2 startPoint, Vector2 endPoint)
        {
            // Careful, 0,0 is at BOTTOM-LEFT (not top left as usual..)
            // Move origin from bottom left to top left
            startPoint.y = screenHeight - startPoint.y;
            endPoint.y = screenHeight - endPoint.y;
            // Calculate corners
            Vector2 topLeft = Vector2.Min(startPoint, endPoint);
            Vector2 bottomRight = Vector2.Max(startPoint, endPoint);
            // Create Rect
            return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }

        //generate a mesh from the 4 bottom points
        public static Mesh GenerateSelectionMesh(Vector3[] corners, Vector3[] vecs)
        {
            Mesh selectionMesh = new Mesh();
            selectionMesh.name = "init";
            Vector3[] meshVertices = new Vector3[8];
            
            for (int i = 0; i < meshVertices.Length >> 1; i++) // (n >> 1) = (n / 2)
            {
                meshVertices[i] = corners[i];
                meshVertices[i + 4] = vecs[i];
            }
            
            selectionMesh.vertices = meshVertices;
            selectionMesh.triangles = Utils.KWmesh.CubeVertices;
            return selectionMesh;
        }
    }
}

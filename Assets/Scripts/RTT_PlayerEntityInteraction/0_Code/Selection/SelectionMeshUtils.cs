using UnityEngine;
using KWUtils;

using static KWUtils.KWRect;
using static KWUtils.KWmesh;
using static Unity.Mathematics.math;

namespace KaizerWaldCode.PlayerEntityInteractions.RTTSelection
{
    public static class SelectionMeshUtils
    {
        //INITIALIZATION
        public static MeshCollider InitializeCollider(this GameObject go, in Mesh selectionMesh)
        {
            MeshCollider selectionCollider = go.AddComponent<MeshCollider>();
            selectionCollider.convex = true;
            selectionCollider.isTrigger = true;
            selectionCollider.enabled = false;
            selectionCollider.sharedMesh = selectionMesh;
            return selectionCollider;
        }
        
        public static Mesh InitializeMesh(in Vector3[] vertices) =>  new Mesh {vertices = vertices, triangles = CubeVertices };
        

        /// <summary>
        /// Define the array "uiCorner" Depending on the position of the startMouseClick according to the current mouse position
        /// </summary>
        /// <param name="uiCorners">array of vertices we want to fill</param>
        /// <param name="startMouseClick">Mouse Position when the first click happes</param>
        /// <param name="endMouseClick">Current mouse position</param>
        public static void GetBoxSelectionVertices(this Vector2[] uiCorners, in Vector2 startMouseClick, in Vector2 endMouseClick)
        {
            if (startMouseClick.IsLeft(endMouseClick)) //startMouseClick is Left
            {
                if (startMouseClick.IsAbove(endMouseClick)) //LeftTop
                {
                    uiCorners[0] = startMouseClick;
                    uiCorners[1] = new Vector2(endMouseClick.x, startMouseClick.y);//top right
                    uiCorners[2] = new Vector2(startMouseClick.x, endMouseClick.y);//bottom left
                    uiCorners[3] = endMouseClick;
                }
                else //LeftBot
                {
                    uiCorners[0] = new Vector2(startMouseClick.x, endMouseClick.y);//top left
                    uiCorners[1] = endMouseClick;
                    uiCorners[2] = startMouseClick;
                    uiCorners[3] = new Vector2(endMouseClick.x, startMouseClick.y);//bottom right
                }
            }
            else //startMouseClick is Right
            {
                if (startMouseClick.IsAbove(endMouseClick)) //RightTop
                { 
                    uiCorners[0] = new Vector2(endMouseClick.x, startMouseClick.y);//top left
                    uiCorners[1] = startMouseClick;
                    uiCorners[2] = endMouseClick;
                    uiCorners[3] = new Vector2(startMouseClick.x, endMouseClick.y);//bottom right
                }
                else //RightBot
                {
                    uiCorners[0] = endMouseClick;
                    uiCorners[1] = new Vector2(startMouseClick.x, endMouseClick.y);//top right
                    uiCorners[2] = new Vector2(endMouseClick.x, startMouseClick.y);//bottom left
                    uiCorners[3] = startMouseClick;
                }
            }
        }
    }
}
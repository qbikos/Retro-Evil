using UnityEngine;

namespace HorrorEngine
{
    public class DebugUtils : MonoBehaviour
    {

        public static void DrawCross(Vector3 position, Vector3 right, Vector3 up, Vector3 forward, float Size, Color c, float duration)
        {
            Debug.DrawLine(position - right * Size * 0.5f, position + right * Size * 0.5f, c, duration);
            Debug.DrawLine(position - forward * Size * 0.5f, position + forward * Size * 0.5f, c, duration);
            Debug.DrawLine(position - up * Size * 0.5f, position + up * Size * 0.5f, c, duration);
        }

        public static void DrawBox(Vector3 pos, Quaternion rot, Vector3 scale, Color c, float duration)
        {
            // create matrix
            Matrix4x4 m = new Matrix4x4();
            m.SetTRS(pos, rot, scale);

            var point1 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0.5f));
            var point2 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, 0.5f));
            var point3 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, -0.5f));
            var point4 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, -0.5f));

            var point5 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, 0.5f));
            var point6 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, 0.5f));
            var point7 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, -0.5f));
            var point8 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, -0.5f));

            Debug.DrawLine(point1, point2, c, duration);
            Debug.DrawLine(point2, point3, c, duration);
            Debug.DrawLine(point3, point4, c, duration);
            Debug.DrawLine(point4, point1, c, duration);

            Debug.DrawLine(point5, point6, c, duration);
            Debug.DrawLine(point6, point7, c, duration);
            Debug.DrawLine(point7, point8, c, duration);
            Debug.DrawLine(point8, point5, c, duration);

            Debug.DrawLine(point1, point5, c, duration);
            Debug.DrawLine(point2, point6, c, duration);
            Debug.DrawLine(point3, point7, c, duration);
            Debug.DrawLine(point4, point8, c, duration);
        }
    }
}
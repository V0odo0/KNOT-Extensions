using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Knot.Extensions
{
    public static class KnotExtensions
    {
        private static StringBuilder _stringBuilder = new StringBuilder();
        private static System.Random _systemRandom = new System.Random();


        #region Arrays
        public static void DestroyGameObjects<T>(this IEnumerable<T> gameObjects) where T : Behaviour
        {
            if (gameObjects == null)
                return;

            foreach (var o in gameObjects)
            {
                if (o == null)
                    continue;

                Object.Destroy(o.gameObject);
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            if (list == null)
                return;

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _systemRandom.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
        
        public static void ShuffleWithSeed<T>(this IList<T> list, int seed)
        {
            if (list == null)
                return;

            System.Random rng = new System.Random(seed);

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        public static void Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            (list[indexA], list[indexB]) = (list[indexB], list[indexA]);
        }
        #endregion

        #region Various
        public static string ToStringAuto(this object obj)
        {
            _stringBuilder.Clear();
            foreach (PropertyInfo property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance))
            {
                _stringBuilder.Append(property.Name);
                _stringBuilder.Append(": ");
                if (!(property.GetIndexParameters().Length > 0))
                    _stringBuilder.Append(property.GetValue(obj, null) ?? "null");
                _stringBuilder.Append(Environment.NewLine);
            }

            foreach (var field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                _stringBuilder.Append(field.Name);
                _stringBuilder.Append(": ");
                _stringBuilder.Append(field.GetValue(obj) ?? "null");
                _stringBuilder.Append(Environment.NewLine);
            }

            return _stringBuilder.ToString();
        }

        public static bool Equals(this Resolution r1, Resolution r2)
        {
            return r1.width == r2.width && r1.height == r2.height && r1.refreshRate == r2.refreshRate;
        }

        public static AnimationClip GetClipByIndex(this Animation animation, int index)
        {
            if (animation == null)
                return null;

            int i = 0;
            foreach (AnimationState animationState in animation)
            {
                if (i == index)
                    return animationState.clip;
                i++;
            }
            return null;
        }
        #endregion

        #region String
        public static bool IsValidUrl(this string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
        
        public static string[] SplitAt(this string source, params int[] index)
        {
            index = index.Distinct().OrderBy(x => x).ToArray();
            string[] output = new string[index.Length + 1];
            int pos = 0;

            for (int i = 0; i < index.Length; pos = index[i++])
                output[i] = source.Substring(pos, index[i] - pos);

            output[index.Length] = source.Substring(pos);
            return output;
        }

        public static (string left, string right) SplitAt(this string text, int index) =>
            (text.Substring(0, index), text.Substring(index + 1));
        
        public static bool ContainsIgnoreCase(this string source, string toCheck)
        {
            return source.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
        }
        #endregion

        #region Vector
        public static Vector2Int Clamp(this Vector2Int value, int min, int max)
        {
            return new Vector2Int(Mathf.Clamp(value.x, min, max), Mathf.Clamp(value.y, min, max));
        }
        
        public static Vector2 Clamp(this Vector2 value, float min, float max)
        {
            return new Vector2(Mathf.Clamp(value.x, min, max), Mathf.Clamp(value.y, min, max));
        }

        public static bool IsInRange(this Vector2Int range, int value)
        {
            return value >= range.x && value <= range.y;
        }

        public static bool IsInRange(this Vector2 range, int value)
        {
            return value >= range.x && value <= range.y;
        }

        public static float Min(this Vector2 v) => Mathf.Min(v.x, v.y);

        public static float Max(this Vector2 v) => Mathf.Max(v.x, v.y);

        public static int Min(this Vector2Int v) => Mathf.Min(v.x, v.y);
        
        public static int Max(this Vector2Int v) => Mathf.Max(v.x, v.y);

        public static float GetRandom(this Vector2 v)
        {
            return UnityEngine.Random.Range(v.x, v.y);
        }

        public static Vector3 GetClosestPointOnLine(this Vector3 point, Vector3 from, Vector3 to)
        {
            Vector3 heading = (to - from);
            float magnitudeMax = heading.magnitude;
            heading.Normalize();

            Vector3 lhs = point - from;
            float dotP = Vector3.Dot(lhs, heading);
            dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
            return from + heading * dotP;
        }

        #endregion

        #region Texture
        public static void Resize(this RenderTexture rt, int w, int h)
        {
            if (rt == null)
                return;

            if (rt.width == w && rt.height == h)
                return;
            
            rt.Release();
            rt.width = Mathf.Clamp(w, 1, SystemInfo.maxTextureSize);
            rt.height = Mathf.Clamp(h, 1, SystemInfo.maxTextureSize);
            rt.Create();
        }

        public static void Clear(this RenderTexture rt, Color color, bool clearDepth = true)
        {
            if (rt == null)
                return;

            var prevActiveRt = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(clearDepth, true, color);
            RenderTexture.active = prevActiveRt;
        }

        public static Texture2D ToTexture2D(this RenderTexture rt, bool mipMaps = false)
        {
            if (rt == null)
                return null;

            var lastActiveRt = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, mipMaps);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            RenderTexture.active = lastActiveRt;

            return tex;
        }
        
        public static string EncodeToPNGBase64(this Texture2D tex)
        {
            if (tex == null)
                return string.Empty;

            return Convert.ToBase64String(tex.EncodeToPNG());
        }
        #endregion

        #region Color
        public static string ToHtmlString(this Color color) => $"#{ColorUtility.ToHtmlStringRGBA(color)}";

        public static Color R(this Color c, float r)
        {
            c.r = r;
            return c;
        }

        public static Color G(this Color c, float g)
        {
            c.g = g;
            return c;
        }

        public static Color B(this Color c, float b)
        {
            c.g = b;
            return c;
        }

        public static Color A(this Color c, float a)
        {
            c.a = a;
            return c;
        }
        #endregion

        #region Numerics
        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
        }

        public static float RemapClamped(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            float t = (value - fromMin) / (fromMax - fromMin);
            if (t > 1f)
                return toMax;
            if (t < 0f)
                return toMin;
            return toMin + (toMax - toMin) * t;
        }

        public static string ToDigit(this float value)
        {
            return value.ToString("N1").Replace(",", ".");
        }

        public static string ToDigit2(this float value)
        {
            return value.ToString("N2").Replace(",", ".");
        }
        #endregion

        #region Transform
        public static void CopyPositionTo(this RectTransform copyFrom, RectTransform copyTo)
        {
            if (copyFrom == null || copyTo == null)
                return;

            copyTo.anchorMin = copyFrom.anchorMin;
            copyTo.anchorMax = copyFrom.anchorMax;
            copyTo.anchoredPosition = copyFrom.anchoredPosition;
            copyTo.sizeDelta = copyFrom.sizeDelta;
        }

        public static void ClampPosition(this RectTransform target, RectTransform parent)
        {
            if (target == null || parent == null)
                return;

            var targetPos = target.anchoredPosition;
            var targetSize = target.sizeDelta;

            if (targetPos.y > parent.rect.yMax - targetSize.y)
                targetPos.y = parent.rect.yMax - targetSize.y;
            if (targetPos.y < parent.rect.yMin + targetSize.y)
                targetPos.y = parent.rect.yMin + targetSize.y;

            if (targetPos.x > parent.rect.xMax - targetSize.x)
                targetPos.x = parent.rect.xMax - targetSize.x;
            if (targetPos.x < parent.rect.xMin + targetSize.x)
                targetPos.x = parent.rect.xMin + targetSize.x;

            target.anchoredPosition = targetPos;
        }
        #endregion
    }
}
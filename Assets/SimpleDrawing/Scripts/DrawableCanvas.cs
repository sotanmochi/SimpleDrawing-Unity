﻿using UnityEngine;

namespace SimpleDrawing
{
	[RequireComponent(typeof(Renderer))]
	[DisallowMultipleComponent]
    public class DrawableCanvas : MonoBehaviour
    {
        public bool ResetCanvasOnPlay = true;
        public Color ResetColor = new Color(1, 1, 1, 1);

        Vector2 defaultTexCoord = Vector2.zero;

        Texture2D drawableTexture;
        Color32[] currentPixels;
        Color32[] resetColorPixels;

        private void Start()
        {
            InitializeDrawCanvas();
        }

        private void InitializeDrawCanvas()
        {
            Material material = GetComponent<Renderer>().material;
            Texture2D mainTexture = (Texture2D) material.mainTexture;
            
            drawableTexture = new Texture2D(mainTexture.width, mainTexture.height);
            material.mainTexture = drawableTexture;
            currentPixels = drawableTexture.GetPixels32();

            resetColorPixels = new Color32[currentPixels.Length];
            for (int i = 0; i < resetColorPixels.Length; i++)
            {
                resetColorPixels[i] = ResetColor;
            }

            if (ResetCanvasOnPlay)
            {
                ResetCanvas();
            }
        }

        public void Draw(Vector2 currentTexCoord, Vector2 previousTexCoord, int thickness, Color color)
        {
            currentPixels = drawableTexture.GetPixels32();

            Vector2 currentPixelPos = currentTexCoord;
            currentPixelPos.x *= drawableTexture.width;
            currentPixelPos.y *= drawableTexture.height;

            Vector2 previousPixelPos = previousTexCoord;
            previousPixelPos.x *= drawableTexture.width;
            previousPixelPos.y *= drawableTexture.height;

            if (previousTexCoord == defaultTexCoord)
            {
                // If this is the first time we've ever dragged on this image, 
                // simply color the pixels at our mouse position.
                MarkPixelsToColor(currentPixelPos, thickness, color);
            }
            else
            {
                // Color in a line from where we were on the last update call.
                ColorBetween(previousPixelPos, currentPixelPos, thickness, color);
            }

            // Apply pixel update
            drawableTexture.SetPixels32(currentPixels);
            drawableTexture.Apply();
        }

        public void Erase(Vector2 currentTexCoord, Vector2 previousTexCoord, int thickness)
        {
            Draw(currentTexCoord, previousTexCoord, thickness, ResetColor);
        }

        public void MarkPixelsToColor(Vector2 pixelPos, int thickness, Color color)
        {
            int width = drawableTexture.width;
            int height = drawableTexture.height;

            int pixelPosX = (int)pixelPos.x;
            int pixelPosY = (int)pixelPos.y;

            for (int y = pixelPosY - thickness; y <= pixelPosY + thickness; y++)
            {
                for (int x = pixelPosX - thickness; x <= pixelPosX + thickness; x++)
                {
                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        currentPixels[x + y * width] = color;
                    }
                }
            }
        }

        public void ColorBetween(Vector2 startPixelPos, Vector2 endPixelPos, int thickness, Color color)
        {
            int width = drawableTexture.width;
            int height = drawableTexture.height;

            Vector2 a = new Vector2((int)startPixelPos.x, (int)startPixelPos.y);
            Vector2 b = new Vector2((int)endPixelPos.x, (int)endPixelPos.y);

            int dx = (b.x - a.x) > 0 ? 1 : -1;
            int dy = (b.y - a.y) > 0 ? 1 : -1;
            // *********************
            //  CPU implementation
            // *********************
            for (int py = 0; py < height; py++)
            // for (int py = (int)a.y; py != (int)b.y; py += dy)
            {
                for (int px = 0; px < width; px++)
                // for (int px = (int)a.x; px != (int)b.x; px += dx)
                {
                    // *********************************************************
                    //  Solve a system of linear equations using Cramer's rule
                    //  The system of linear equations:
                    //    px = s*ax + t*bx
                    //    py = s*ay + t*by
                    //    Unknowns: s,t
                    // *********************************************************
                    float determinant = a.x*b.y - b.x*a.y;
                    if (!Mathf.Approximately(determinant, 0.0f))
                    {
                        float s = (px*b.y - b.x*py) / determinant;
                        float t = (a.x*py - px*a.y) / determinant;

                        // Whether a point (px,py) is on the line segment AB.
                        if (GreaterThanEqualApproximately(s, 0.0f) &&
                            GreaterThanEqualApproximately(t, 0.0f) &&
                            Mathf.Approximately((s + t), 1.0f))
                        {
                            float d = Mathf.Sqrt((b.y - a.y)*(b.y - a.y) + (b.x - a.x)*(b.x - a.x));
                            if (d > 0.0f)
                            {
                                d = 1.0f/d * Mathf.Abs((b.y - a.y)*px - (b.x - a.x)*py + (b.x*a.y - b.y*a.x));
                            }

                            // if (d < 0.5f * Mathf.Sqrt(2.0f) )
                            if (d < 1.0f )
                            {
                                MarkPixelsToColor(new Vector2(px, py), thickness, color);
                            }
                        }
                    }
                }
            }

            // ***********************************************************************
            // // Get the distance from start to end
            // float distance = Vector2.Distance(startPixelPos, endPixelPos);
            // Vector2 direction = (startPixelPos - endPixelPos).normalized;

            // // Calculate how many times we should interpolate 
            // // between start point and end point based on 
            // // the amount of time that has passed since the last update.
            // float lerpSteps = 1 / distance;
            // Vector2 currentPosition = startPixelPos;
            // for (float lerp = 0; lerp <= 1; lerp += lerpSteps)
            // {
            //     currentPosition = Vector2.Lerp(startPixelPos, endPixelPos, lerp);
            //     MarkPixelsToColor(currentPosition, thickness, color);
            // }
            // ***********************************************************************
        }

        public void ResetCanvas()
        {
            drawableTexture.SetPixels32(resetColorPixels);
            drawableTexture.Apply();
        }

        private bool GreaterThanEqualApproximately(float a, float b)
        {
            return (a > b) || Mathf.Approximately(a, b);
        }
    }
}
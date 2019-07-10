using UnityEngine;

namespace SimpleDrawing
{
	[RequireComponent(typeof(Renderer))]
	[DisallowMultipleComponent]
    public class DrawableCanvas : MonoBehaviour
    {
        public bool ResetCanvasOnPlay = true;
        public Color ResetColor = new Color(1, 1, 1, 1);

        private Material drawLineMaterial = null;
        private Material singleColorMaterial = null;
        private RenderTexture drawableRenderTexture;

        #region ShaderPropertyID
		private int mainTexturePropertyID;
		private int lineColorPropertyID;
		private int thicknessPropertyID;
		private int startPositionUVPropertyID;
		private int endPositionUVPropertyID;
		private int singleColorPropertyID;
        #endregion

        private void Awake()
        {
            drawLineMaterial = new Material(Resources.Load<Material>("SimpleDrawing.DrawLine"));
            singleColorMaterial = new Material(Resources.Load<Material>("SimpleDrawing.SingleColor"));
            InitializePropertyID();
        }

        private void Start()
        {
            InitializeDrawCanvas();
        }

        private void InitializePropertyID()
        {
            // DrawLine.shader
			mainTexturePropertyID = Shader.PropertyToID("_MainTex");
			lineColorPropertyID = Shader.PropertyToID("_LineColor");
			thicknessPropertyID = Shader.PropertyToID("_Thickness");
			startPositionUVPropertyID = Shader.PropertyToID("_StartPositionUV");
			endPositionUVPropertyID = Shader.PropertyToID("_EndPositionUV");

            // SingleColor.shader
            singleColorPropertyID = Shader.PropertyToID("_SingleColor");
        }

        private void InitializeDrawCanvas()
        {
            Material material = GetComponent<Renderer>().material;
            Texture2D mainTexture = (Texture2D) material.mainTexture;
            
            drawableRenderTexture = new RenderTexture(mainTexture.width, mainTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			drawableRenderTexture.filterMode = mainTexture.filterMode;
			Graphics.Blit(mainTexture, drawableRenderTexture);
            material.mainTexture = drawableRenderTexture;

            if (ResetCanvasOnPlay)
            {
                ResetCanvas();
            }
        }

        public void Draw(Vector2 currentTexCoord, Vector2 previousTexCoord, int thickness, Color color)
        {
            drawLineMaterial.SetInt(thicknessPropertyID, thickness);
            drawLineMaterial.SetVector(lineColorPropertyID, color);
            drawLineMaterial.SetVector(startPositionUVPropertyID, previousTexCoord);
            drawLineMaterial.SetVector(endPositionUVPropertyID, currentTexCoord);

			var mainPaintTextureBuffer = RenderTexture.GetTemporary(drawableRenderTexture.width, drawableRenderTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(drawableRenderTexture, mainPaintTextureBuffer, drawLineMaterial);
            Graphics.Blit(mainPaintTextureBuffer, drawableRenderTexture);
            RenderTexture.ReleaseTemporary(mainPaintTextureBuffer);
        }

        public void Erase(Vector2 currentTexCoord, Vector2 previousTexCoord, int thickness)
        {
            Draw(currentTexCoord, previousTexCoord, thickness, ResetColor);
        }

        public void ResetCanvas()
        {
            singleColorMaterial.SetVector(singleColorPropertyID, ResetColor);

			var mainPaintTextureBuffer = RenderTexture.GetTemporary(drawableRenderTexture.width, drawableRenderTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(drawableRenderTexture, mainPaintTextureBuffer, singleColorMaterial);
            Graphics.Blit(mainPaintTextureBuffer, drawableRenderTexture);
            RenderTexture.ReleaseTemporary(mainPaintTextureBuffer);
        }
    }
}
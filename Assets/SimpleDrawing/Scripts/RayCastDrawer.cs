using UnityEngine;

namespace SimpleDrawing
{
    [RequireComponent(typeof(MeshRenderer))]
    public class RayCastDrawer : MonoBehaviour
    {   
        private enum RayDirectionType
        {
            TransformForward,
            TransformBackward,
            TransformRight,
            TransformLeft,
            TransformUp,
            TransformDown,
        }

        public bool RayCastEnabled = true;
        public bool VariableThickness = true;
        public bool Erase = false;

        public float RayDistance = 2.0f;
        public float RayOffset = 0.0f;
        public float NibLength = 0.5f;

        public Color PenColor = Color.black;
        public int PenWidth = 5;

        [SerializeField]
        RayDirectionType directionType = RayDirectionType.TransformDown;

        Vector2 defaultTexCoord = Vector2.zero;

        Vector2 currentTexCoord;
        Vector2 previousTexCoord;

        int _PreviousPenWidth;

		void Start()
		{
			GetComponent<MeshRenderer>().material.color = PenColor;
		}

        void Update()
        {
            if (RayCastEnabled)
            {
                Vector3 rayDir = GetCurrentDirection();
                Vector3 rayOrigin = this.transform.position + rayDir * RayOffset;
                var ray = new Ray(rayOrigin, rayDir);

                RaycastHit hitInfo;
				if(Physics.Raycast(ray, out hitInfo, RayDistance))
                {
                    if(hitInfo.collider != null && hitInfo.collider is MeshCollider)
                    {
                        var drawObject = hitInfo.transform.GetComponent<DrawableCanvas>();
                        if (drawObject != null)
                        {
                            previousTexCoord = currentTexCoord;
                            currentTexCoord = hitInfo.textureCoord;
                            if (Erase)
                            {
                                drawObject.Erase(currentTexCoord, previousTexCoord, PenWidth);
                            }
                            else
                            {
                                float dist = Vector3.Distance(hitInfo.point, rayOrigin);
                                if (VariableThickness)
                                {
                                    float currentPenWidth = (RayDistance - dist) / NibLength * PenWidth;
                                    currentPenWidth = Mathf.Clamp(currentPenWidth, 0.0f, PenWidth);
                                    drawObject.Draw(currentTexCoord, (int)currentPenWidth, previousTexCoord, _PreviousPenWidth, PenColor);
                                    _PreviousPenWidth = (int)currentPenWidth;
                                }
                                else
                                {
                                    drawObject.Draw(currentTexCoord, (int)PenWidth, previousTexCoord, (int)PenWidth, PenColor);
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("If you want to draw using a RaycastHit, need set MeshCollider for object.");
                    }
                }
                else
                {
                    currentTexCoord = defaultTexCoord;
                }
            }
            else if (!RayCastEnabled)
            {
                currentTexCoord = defaultTexCoord;
            }
        }

        private Vector3 GetCurrentDirection()
        {
            Vector3 direction = Vector3.zero;
            switch(directionType)
            {
                case RayDirectionType.TransformForward:
                    direction =  this.transform.forward;
                    break;
                case RayDirectionType.TransformBackward:
                    direction = -this.transform.forward;
                    break;
                case RayDirectionType.TransformRight:
                    direction =  this.transform.right;
                    break;
                case RayDirectionType.TransformLeft:
                    direction = -this.transform.right;
                    break;
                case RayDirectionType.TransformUp:
                    direction =  this.transform.up;
                    break;
                case RayDirectionType.TransformDown:
                    direction = -this.transform.up;
                    break;
            }
            return direction;
        }
    }
}
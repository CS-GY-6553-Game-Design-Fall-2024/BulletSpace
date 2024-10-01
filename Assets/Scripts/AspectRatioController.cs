using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AspectRatioController : MonoBehaviour
{
    public static AspectRatioController current;
    [SerializeField] private Camera m_camera;
    [SerializeField] private float m_screenWidth = 16f;
    [SerializeField] private float m_screenHeight = 9f;

    [SerializeField] private float m_worldWidth;
    public float worldWidth => m_worldWidth;
    [SerializeField] private float m_worldHeight;
    public float worldHeight => m_worldHeight;

    private void Awake() {
        current = this;
        SetAspectRatio();
    }

    public void SetAspectRatio() {
        // set the desired aspect ratio (the values in this example are
        // hard-coded for 16:9, but you could make them into public
        // variables instead so you can set them at design time)
        float targetAspect = m_screenWidth / m_screenHeight;
    
        // determine the game window's current aspect ratio
        float windowAspect = (float)Screen.width / (float)Screen.height;

        // current viewport height should be scaled by this amount
        float scaleHeight = windowAspect / targetAspect;
    
        // Get a reference to ther camera's rect
        Rect rect = m_camera.rect;

        // if scaled height is less than current height, add letterbox
        
        if (scaleHeight < 1.0f) {  
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else // add pillarbox
        {
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        // Set the camera's rect to the new one
        m_camera.rect = rect;

        // Get the resulting screen widht and height in world coordinates
        float aspect = (float)Screen.width / Screen.height;
        m_worldHeight = m_camera.orthographicSize * 2;
        m_worldWidth = worldHeight * aspect * rect.width;
        Debug.Log($"World Dimensions, According to Screen: {m_worldWidth}:{m_worldHeight}");
    }
}

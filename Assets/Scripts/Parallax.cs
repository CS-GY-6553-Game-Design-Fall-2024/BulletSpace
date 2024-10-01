using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    Material mat;
    Vector2 textureOffset;

    // Control speed and direction of parallax
    [Range(0f, 0.5f)] public float speed = 0.05f;
    public Vector2 scrollDirection = Vector2.up; // default horizontal scrolling
    public float layerMultiplier = 1f; // adjust this per object for different scrolling speeds

    // Start is called before the first frame update
    void Start()
    {
        // Get the material from the object's renderer
        mat = GetComponent<Renderer>().material;
        textureOffset = mat.GetTextureOffset("_MainTex");
    }

    // Update is called once per frame
    void Update()
    {
        // Increment the texture offset
        textureOffset += scrollDirection * speed * layerMultiplier * Time.deltaTime;

        // Ensure the offset wraps around (loops) using Mathf.Repeat
        textureOffset.x = Mathf.Repeat(textureOffset.x, 1f); // For horizontal scrolling
        textureOffset.y = Mathf.Repeat(textureOffset.y, 1f); // For vertical scrolling, if needed

        // Apply the updated texture offset to create scrolling effect
        mat.SetTextureOffset("_MainTex", textureOffset);
    }
}

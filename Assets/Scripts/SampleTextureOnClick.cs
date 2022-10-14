using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class SampleTextureOnClick : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer Renderer;
    [SerializeField]
    private float SampleGrowthTime = 4;
    [SerializeField] 
    private float RepeatDelay = 0.1f;

    private Color[] OriginalColors;
    private Texture2D OriginalTexture;
    private Texture2D RenderTexture;

    private float InitialClickTime;
    private float LastSampleTime;

    private void Awake()
    {
        OriginalTexture = Renderer.sprite.texture;
        RenderTexture = new Texture2D(
            OriginalTexture.width, 
            OriginalTexture.height, 
            OriginalTexture.format, 
            false,
            true
        );
        OriginalColors = OriginalTexture.GetPixels();

        // Prevent overriding default asset colors                
        RenderTexture.SetPixels(OriginalColors);
        RenderTexture.Apply();

        Sprite sprite = Sprite.Create(
            RenderTexture, 
            new Rect(0, 0, RenderTexture.width, RenderTexture.height), 
            Vector2.one / 2f
        );
        Renderer.sprite = sprite;
    }

    private void Update()
    {
        if (Application.isFocused && Mouse.current.leftButton.isPressed)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                InitialClickTime = Time.time;
            }

            if (LastSampleTime + RepeatDelay < Time.time)
            {
                SampleTexture(Time.time - InitialClickTime);
                LastSampleTime = Time.time;
            }
        }
        if (Application.isFocused && Mouse.current.rightButton.wasReleasedThisFrame)
        {
            Renderer.sprite.texture.SetPixels(OriginalColors);
            Renderer.sprite.texture.Apply();
        }
    }

    private void SampleTexture(float HoldTime)
    {
        Vector2 halfSize = new Vector2(
            RenderTexture.width / 2f, 
            RenderTexture.height / 2f
        );

        int halfSquareRadius = Mathf.CeilToInt(
            Mathf.Lerp(0.01f, halfSize.x, Mathf.Clamp01(HoldTime / SampleGrowthTime))
        );

        int minX = Mathf.FloorToInt(halfSize.x) - halfSquareRadius;
        int minY = Mathf.FloorToInt(halfSize.y) - halfSquareRadius;

        Color[] sampleColors = OriginalTexture.GetPixels(
            minX,
            minY,
            halfSquareRadius * 2,
            halfSquareRadius * 2
        );

        float[] colorsAsGrey = System.Array.ConvertAll(
            sampleColors, 
            (color) => color.grayscale
        );
        float totalGreyValue = colorsAsGrey.Sum();

        float grey = Random.Range(0, totalGreyValue);
        int i = 0;
        for (; i < colorsAsGrey.Length; i++)
        {
            grey -= colorsAsGrey[i];
            if (grey <= 0)
            {
                break;
            }
        }

        int x = minX + i % (halfSquareRadius * 2);
        int y = minY + i / (halfSquareRadius * 2);

        Debug.Log($"Time: {HoldTime}, i: {i}: ({x}, {y}). " +
            $"Sampled {sampleColors.Length} Pixels starting at ({minX}, {minY}) " +
            $"halfWidth: {halfSize} and halfSquareRadius: {halfSquareRadius}. " +
            $"Total Grey: {totalGreyValue}"
        );

        // Do something based on the sampled data. For today we'll just show
        // the sample area and sampled pixel for this frame.
        RenderTexture.SetPixels(OriginalColors);
        SetRectPixels(RenderTexture, minX, minY, halfSquareRadius * 2);
        RenderTexture.SetPixel(x, y, Color.green);

        RenderTexture.Apply();
    }


    private void SetRectPixels(Texture2D RenderTexture, int MinX, int MinY, int RectSize)
    {
        for (int x = 0; x < RectSize; x++)
        {
            for (int y = 0; y < RectSize; y++)
            {
                if (x == 0 || x == RectSize - 1 || y == 0 || y == RectSize - 1)
                {
                    RenderTexture.SetPixel(MinX + x, MinY + y, Color.red);
                }
            }
        }
    }
}

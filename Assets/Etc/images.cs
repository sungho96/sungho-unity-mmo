using UnityEngine;
using RosMessageTypes.Sensor; // Use the appropriate namespace for Image message
using Unity.Robotics.ROSTCPConnector;
using System;

public class ImageSubscriber : MonoBehaviour
{
    public ROSConnection ros;
    public Renderer targetRenderer; // Assign a Renderer component to display the image

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<ImageMsg>("/detectImage", ImageCallback);
    }

    void ImageCallback(ImageMsg image)
    {
        int width = (int)image.width;
        int height = (int)image.height;
        byte[] imageData = image.data;

        // Create a new Texture2D (assuming the image format is RGB8)
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Assign pixel data directly, flipping horizontally
        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width + x) * 3;
                int reverseIndex = (y * width + (width - 1 - x)) * 3; // Reverse index for horizontal flip

                float r = imageData[reverseIndex] / 255.0f;
                float g = imageData[reverseIndex + 1] / 255.0f;
                float b = imageData[reverseIndex + 2] / 255.0f;
                pixels[y * width + x] = new Color(r, g, b);
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();

        // Assign the texture to the target renderer to display it
        targetRenderer.material.mainTexture = texture;
    }
}


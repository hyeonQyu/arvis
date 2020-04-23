using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;

public static class Coder
{
    // WebCamTexture -> byte[]
    public static byte[] Encode(WebCamTexture cam)
    {
        Color32[] colors = cam.GetPixels32();
        int length = colors.Length;
        byte[] pixels = new byte[length * 3];

        for(int i = 0; i < length; i++)
        {
            pixels[3 * i + 0] = colors[i].b;
            pixels[3 * i + 1] = colors[i].g;
            pixels[3 * i + 2] = colors[i].r;
        }

        return pixels;
    }

    public static Mat Decode(byte[] pixels, int width, int height)
    {
        Mat img = new Mat(height, width, MatType.CV_8UC3);

        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                var pt = img.At<Vec3b>(y, x);

                int row = width * (height - y - 1);
                pt.Item0 = pixels[row + 3 * x];
                pt.Item1 = pixels[row + 3 * x + 1];
                pt.Item2 = pixels[row + 3 * x + 2];
            }
        }

        return img;
    }
}

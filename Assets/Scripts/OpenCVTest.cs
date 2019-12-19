using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using System;

[StructLayout(LayoutKind.Sequential)]
    struct Test
    {
        [MarshalAs(UnmanagedType.I4)]
        public int a;
        [MarshalAs(UnmanagedType.I4)]
        public int b;
        [MarshalAs(UnmanagedType.I4)]
        public int c;
    }

public class OpenCVTest:MonoBehaviour
{
    

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct Information
    {
        public string Name;
        public int Number;
    }

    struct Point
    {
        public int x;
        public int y;
    }

    public Button btnFlipImage;
    public Image imageBlack;
    public Text txt;

    [DllImport("opencv410")]
    private static extern void FlipImage(ref Color32[] rawImage, int width, int height);
    [DllImport("opencv410")]
    private static extern void Marshalling(out IntPtr tests, [MarshalAs(UnmanagedType.I4)] out int count);
    //[DllImport("opencv410")]
    //public static extern int GetInformations(out IntPtr informations, out int informationCount);
    [DllImport("opencv410")]
    public static extern void GetPoints([MarshalAs(UnmanagedType.I4)] out int x, [MarshalAs(UnmanagedType.I4)] out int y, [MarshalAs(UnmanagedType.I4)] int i);
    [DllImport("opencv410")]
    public static extern void GetHandPoints();


    // Start is called before the first frame update
    void Start()
    {
        btnFlipImage.onClick.AddListener(CallGetHandPoints);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CallGetHandPoints()
    {
        GetHandPoints();
    }

    private void CallFlipImage()
    {
        var blackImage = imageBlack.sprite.texture.GetPixels32();
        FlipImage(ref blackImage, 800, 800);
        imageBlack.sprite.texture.SetPixels32(blackImage);
        imageBlack.sprite.texture.Apply();
    }

    private void CallGetPoints()
    {
        Point[] points = new Point[21];

        for(int i = 0; i < 21; i++)
        {
            int x, y;
            GetPoints(out x, out y, i);
            points[i].x = x;
            points[i].y = y;
        }

        for(int i = 0; i < 21; i++)
        {
            string log = i + ")  x: " + points[i].x + "   y: " + points[i].y;
            Debug.Log(log);
        }
    }

    private void CallMarshalling()
    {
        //float[] test = new float[3];
        //test[0] = 0.5f;
        //test[1] = 1.5f;
        //test[2] = 2.5f;
        int count = 0;
        IntPtr tests = Marshal.AllocHGlobal(36);
        tests = IntPtr.Zero;
        Marshalling(out tests, out count);
        //Debug.Log(tests);
        List<Test> testList = new List<Test>();
        int size = Marshal.SizeOf(typeof(Test));

        for(int i = 0; i < count; i++)
        {
            Test test = (Test)Marshal.PtrToStructure(tests, typeof(Test));
            //Test test = Marshal.PtrToStructure<Test>(tests);
            testList.Add(test);
            tests = (IntPtr)(tests.ToInt32() + size);

            string log = "test.a: " + test.a + " test.b: " + test.b + " test.c: " + test.c;
            Debug.Log(i);
        }

        //string log = "test[0]: " + test.a + " test[1]: " + test.b + " test[2]: " + test.c;
        //Debug.Log(log);
    }

    //private void CallGetInformation()
    //{
    //    IntPtr informations = IntPtr.Zero;
    //    int informationCount = 0;

    //    int result = GetInformations(out informations, out informationCount);

    //    List<Information> informationList = new List<Information>();
    //    int structSize = Marshal.SizeOf(typeof(Information));

    //    for(int i = 0; i < informationCount; i++)
    //    {
    //        Information info = (Information)Marshal.PtrToStructure(informations, typeof(Information));
    //        informationList.Add(info);
    //        informations = new IntPtr(informations.ToInt32() + structSize);
    //    }
    //}
}
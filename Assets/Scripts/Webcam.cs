using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;
using OpenCvSharp.Demo;
using System;

public class Webcam:WebCamera
{
    protected override void Awake()
    {
        base.Awake();
        this.forceFrontalCamera = true;
    }

    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
    {
        //Mat imgFrame = OpenCvSharp.Unity.TextureToMat(input, TextureParameters);
        //output = OpenCvSharp.Unity.MatToTexture(imgFrame, output);


        int range_count = 0;

        Scalar skin = new Scalar(95, 127, 166);
        Scalar table = new Scalar(176, 211, 238);


        Mat rgbColor = new Mat(1, 1, MatType.CV_8UC3, skin);
        Mat rgbColor2 = new Mat(1, 1, MatType.CV_8UC3, table);
        Mat hsvColor = new Mat();

        Cv2.CvtColor(rgbColor, hsvColor, ColorConversionCodes.BGR2HSV);

        int hue = (int)hsvColor.At<Vec3b>(0, 0)[0];
        int saturation = (int)hsvColor.At<Vec3b>(0, 0)[1];
        int value = (int)hsvColor.At<Vec3b>(0, 0)[2];



        // 기존의 색상값(H) 에 따른 hue 범위 정하기 코드.

        int lowHue = hue - 7;
        int high_hue = hue + 7;

        int low_hue1 = 0, low_hue2 = 0;
        int high_hue1 = 0, high_hue2 = 0;


        if(lowHue < 10)
        {
            range_count = 2;

            high_hue1 = 180;
            low_hue1 = lowHue + 180;
            high_hue2 = high_hue;
            low_hue2 = 0;
        }
        else if(high_hue > 170)
        {
            range_count = 2;

            high_hue1 = lowHue;
            low_hue1 = 180;
            high_hue2 = high_hue - 180;
            low_hue2 = 0;
        }
        else
        {
            range_count = 1;

            low_hue1 = lowHue;
            high_hue1 = high_hue;
        }



        for(;;)
        {
            // wait for a new frame from camera and store it into 'frame'
            Mat imgFrame, imgHsv;
            imgFrame = OpenCvSharp.Unity.TextureToMat(input, TextureParameters);


            // check if we succeeded
            if(imgFrame.Empty())
            {
                Debug.Log("ERROR! blank frame grabbed");
                break;
            }


            //HSV로 변환
            imgHsv = new Mat(imgFrame.Size(), MatType.CV_8UC3);
            Cv2.CvtColor(imgFrame, imgHsv, ColorConversionCodes.BGR2HSV);


            //지정한 HSV 범위를 이용하여 영상을 이진화
            Mat imgMask1, imgMask2;
            imgMask1 = new Mat();
            imgMask2 = new Mat();
            Cv2.InRange(imgHsv, new Scalar(low_hue1, 50, 80), new Scalar(high_hue1, 255, 255), imgMask1);
            if(range_count == 2)
            {
                Cv2.InRange(imgHsv, new Scalar(low_hue2, 50, 80), new Scalar(high_hue2, 255, 255), imgMask2);
                imgMask1 |= imgMask2;
            }


            //morphological opening 작은 점들을 제거
            Cv2.Erode(imgMask1, imgMask1, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)));
            Cv2.Dilate(imgMask1, imgMask1, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)));


            //morphological closing 영역의 구멍 메우기
            Cv2.Dilate(imgMask1, imgMask1, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)));
            Cv2.Erode(imgMask1, imgMask1, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)));

            // 원본영상 & 마스크이미지 -> 피부색 영역 검출
            Mat img_skin = new Mat();
            Cv2.BitwiseAnd(imgFrame, imgFrame, img_skin, imgMask1);

            // 피부색 추출 -> GrayScale
            Mat img_gray = new Mat();
            Cv2.CvtColor(img_skin, img_gray, ColorConversionCodes.BGR2GRAY);
            Cv2.BitwiseNot(img_gray, img_gray);  // 색상반전

            // GrayScale -> Canny
            Mat img_canny = new Mat();
            Cv2.Canny(img_gray, img_canny, 100, 200);
            //bitwise_not(img_canny, img_canny);  // 색상반전


            // 특정 윤곽선 검출하기
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(img_canny, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            Point[][] hull = new Point[contours.Length][];
            Mat img_hand = Mat.Zeros(img_gray.Size(), MatType.CV_8UC3);
            Cv2.BitwiseNot(img_hand, img_hand);

            // Version_1
            int largest_area = 0;
            int largest_contour_index = 0;
            double a;

            for(int i = 0; i < contours.Length; i++) // iterate through each contour.
            {
                a = Cv2.ContourArea(contours[i], false);  //  Find the area of contour
                if(a > largest_area)
                {
                    largest_area = (int)a;
                    largest_contour_index = i;                //Store the index of largest contour
                }

            }
            if(largest_area > 5000)
            {
                Cv2.DrawContours(img_hand, contours, largest_contour_index, new Scalar(0, 255, 0));
                hull[largest_contour_index] = Cv2.ConvexHull(contours[largest_contour_index]);
                //Cv2.DrawContours(img_hand, hull, largest_contour_index, new Scalar(0, 0, 255));
            }




            //drawContours(img_hand, contours, -1, Scalar(0,0,0));

            // Version_3
            /*
            int largest_area = 0;
            int main_area = 0;
            int largest_contour_index = 0;

            /// Find the convex hull,contours and defects for each contour
            vector<vector<Point> >hull(contours.size());
            vector<vector<int> >inthull(contours.size());
            vector<vector<Vec4i> >defects(contours.size());
            for (int i = 0; i < contours.size(); i++)
            {
                convexHull(Mat(contours[i]), hull[i], false);
                convexHull(Mat(contours[i]), inthull[i], false);
                if (inthull[i].size()>3)
                    convexityDefects(contours[i], inthull[i], defects[i]);
            }
            //find  hulland contour and defects end here
            //this will find largest contour
            for (int i = 0; i< contours.size(); i++) // iterate through each contour.
            {
                double a = contourArea(contours[i], false);  //  Find the area of contour
                if (a>main_area)
                {
                    main_area = a;
                    largest_contour_index = i;                //Store the index of largest contour
                }

            }

            //search for largest contour has end

            if (contours.size() > 0)
            {
                drawContours(img_hand, contours, largest_contour_index, CV_RGB(0, 255, 0), 2, 8, hierarchy);
                //if want to show all contours use below one
                //drawContours(img_hand,contours,-1, CV_RGB(0, 255, 0), 2, 8, hierarchy);
                //if (showhull)
                drawContours(img_hand, hull, largest_contour_index, CV_RGB(0, 0, 255), 2, 8, hierarchy);
                //if want to show all hull, use below one
                //drawContours(img_hand,hull,-1, CV_RGB(0, 255, 0), 2, 8, hierarchy);
                //if (showcondefects)
                condefects(defects[largest_contour_index], contours[largest_contour_index],img_hand);
                 //
            }
    */

            //Version_2
            /*
             vector<Point2f> approx;
             for (int i = 0; i < contours.size(); i++) {
                 approxPolyDP(Mat(contours[i]), approx, arcLength(Mat(contours[i]), true)*0.01, true);

                 if (fabs(contourArea(Mat(approx))) > 0)
                     drawContours(img_hand, contours, i, Scalar(0,0,0));
              }
             */





            ////라벨링
            //Mat img_labels, stats, centroids;
            //int numOfLables = connectedComponentsWithStats(img_mask1, img_labels,
            //    stats, centroids, 8, CV_32S);

            ////영역박스 그리기
            //int max = -1, idx = 0;
            //for(int j = 1; j < numOfLables; j++)
            //{
            //    int area = stats.at<int>(j, CC_STAT_AREA);
            //    if(max < area)
            //    {
            //        max = area;
            //        idx = j;
            //    }
            //}


            //int left = stats.at<int>(idx, CC_STAT_LEFT);
            //int top = stats.at<int>(idx, CC_STAT_TOP);
            //int width = stats.at<int>(idx, CC_STAT_WIDTH);
            //int height = stats.at<int>(idx, CC_STAT_HEIGHT);


            //rectangle(img_hand, Point(left, top), Point(left + width, top + height),
            //    Scalar(0, 0, 255), 1);

            //imshow("원본 영상", img_frame);
            //imshow("hsv",img_hsv);
            //imshow("이진화 영상", img_mask1);
            //imshow("피부색 추출 영상", img_skin);
            //imshow("Gray", img_gray);
            //imshow("Canny", img_canny);
            //Cv2.ImShow("Hand", img_hand);
            output = OpenCvSharp.Unity.MatToTexture(imgMask1, output);

            if(Cv2.WaitKey(5) >= 0)
                break;
        }

        return true;
    }
}

//public class Webcam:MonoBehaviour
//{
//    private RawImage _background;
//    private WebCamTexture _webCamTexture;
//    private Mat _frame;

//    // Start is called before the first frame update
//    void Start()
//    {
//        _background = this.GetComponent<RawImage>();
//        _webCamTexture = new WebCamTexture(Screen.width, Screen.height);


//        //_frame = OpenCvSharp.Unity.TextureToMat(_webCamTexture, OpenCvSharp.Demo.WebCamera.);


//        _background.texture = _webCamTexture;
//        _background.material.mainTexture = _webCamTexture;
//        _webCamTexture.Play();
//    }
//}

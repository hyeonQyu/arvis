# Arvis

##### Arvis는 OpenCVSharp를 사용하여 손을 인식하여 유니티에서 AR 물체를 제어하는 프로젝트이다.




## 손 인식 및 트래킹

##### 본 프로젝트를 진행하기 위한 환경을 구성하기 위해서는 Unity 엔진이 필요하다. Unity에서 사용하는 언어는 C#이지만, OpenCV는 C++이나 파이썬으로 라이브러리화 되어있다. 그래서 Unity Asset Store에서 무료로 제공하는 OpenCV Sharp를 사용한다.

##### 본 프로젝트에서는 색상 모델을 이용하여 손을 추출해 내는 방식을 사용한다.


### 색상 모델

##### 디바이스의 카메라로부터 얻은 프레임 이미지로부터 피부색을 검출하여 HSV로 변환한다. HSV 모델은 Hue(색조), Saturation(채도), Value(명도)의 3가지 성분으로 색을 표현한다. 색상을 통한 영상인식에서 가장 큰 문제점은 사물이 밝은 곳에 있을 때와 어두운 곳에 있을 때 들어오는 영상이 달라진다는 것이다. 이를 해결하기 위해 크게 Gray 모델을 통해 색을 사용하지 않고 밝기 정보만을 사용하는 방법과 밝기 정보 없이 순수 색상정보만을 사용하는 방법이 있다. 이 프로젝트에서는 손을 인식을 해야 하기 때문에 피부색을 검출해야 한다. 피부색은 사람마다 밝은 사람도 있고 어두운 사람도 있다. 하지만 피부는 대부분 붉은색 계열을 띄기 때문에 색상 정보에 좀 더 초점을 두되 이로 인한 과적합을 막기 위해 밝기 정보도 적절히 사용한다. 이미지 프레임을 블러처리를 수행한 후 Cv2.CvtColor함수를 통해 HSV로 변환한다.

```c#
Mat imgHsv = new Mat(imgBlur.Size(), MatType.CV_8UC3);
Cv2.CvtColor(imgBlur, imgHsv, ColorConversionCodes.BGR2HSV);
```

![원본 프레임 이미지](C:/Users/hgKim/Documents/College/3Grade/2ndSemester/CapstoneDesign1/screenshots/0. 원본 프레임 이미지_가림.jpg)

![HSV 이미지] (C:/Users/hgKim/Documents/College/3Grade/2ndSemester/CapstoneDesign1/screenshots/5-1. HSV 이미지_가림.jpg)

### 영상 이진화

##### HSV 범위를 이용하여 영상을 이진화 한 후 mask 이미지를 만든다. 이진화 관련 함수는 아래와 같다.

```c#
Mat imgMask1, imgMask2;
imgMask1 = new Mat();
imgMask2 = new Mat();
Cv2.InRange(imgHsv, new Scalar(_lowHue1, 50, 50), new Scalar(_highHue1, 255, 255), imgMask1);
if(_rangeCount == 2)
{
Cv2.InRange(imgHsv, new Scalar(_lowHue2, 50, 50), new Scalar(_highHue2, 255, 255), imgMask2);
imgMask1 |= imgMask2;
}

//morphological opening 작은 점들을 제거
Cv2.Erode(imgMask1, imgMask1, 						Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)));
Cv2.Dilate(imgMask1, imgMask1, 						Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)));

//morphological closing 영역의 구멍 메우기
Cv2.Dilate(imgMask1, imgMask1, 						Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)));
Cv2.Erode(imgMask1, imgMask1, 						Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)));
```

##### Cv2.CvtColor 함수에서 기본 BGR의 컬러를 HSV로 바꿔준다. 이후 Cv2.InRange 함수에서 정해진 범위 안에 들어가 있는 영역은 0으로 만들어주고, 나머지는 1로 만들어 흑백사진으로 만들어 처리를 한다. 이후 이미지 형태를 전환하기 위해 Erosion과 Dilation을 진행한다.  Erosion은 이미지를 침식시키는데, Foreground가 되는 이미지의 경계 부분을 침식시켜 Background 이미지로 전환하는 작업이다. Dilation은 이미지를 팽창시키고, Erosion과 반대의 역할을 진행한다. Opening과 Closing은 Erosion과 Dilation을 활용하여 노이즈를 없애는 방법이다. Opening에서 Erosion 수행 후 Dilation을 적용하면 작은 노이즈를 없애고, Closing에서 Dilation 수행 후 Erosion을 적용하면 전반적인 이미지가 깨끗해진다.

![피부색을 검출한 Mask 이미지] (C:/Users/hgKim/Documents/College/3Grade/2ndSemester/CapstoneDesign1/screenshots/5-2. 피부색 검출한 마스크 이미지.jpg)


### 손의 윤곽선 및 꼭짓점 검출

##### 원본 이미지와 mask 이미지를 사용하여 피부색 영역을 검출한다. 피부색을 추출한 후 GrayScale을 수행, 색상을 반전시킨다. 이후 Cv2.Canny 함수를 통해 이미지의 Edge 부분(가장자리)을 검출하도록 한다. 매개변수로 100과 200이 들어가는데, 이는 100 이하에 포함된 Edge는 Edge에서 제외하고, 200 이상에 포함된 Edge는 Edge로 간주하는 임계값이다.


### 얼굴 제거

##### 현 과정까지는 얼굴을 포함한 여러 노이즈로 인해 꼭짓점들이 제대로 검출되지 않을 때가 있다. 그래서 우선 배경을 제거하도록 하였다. 움직이지 않는 픽셀은 배경으로 인식하고, 움직이는 픽셀은 손으로 인식하도록 코딩을 하였다. 하지만 배경이 깔끔하게 지워지지도 않고 연산량 또한 많아 속도가 느린 현상이 발생하였다.

##### 얼굴도 피부색으로 함께 검출되기 때문에 순수하게 손만 검출해내기에 노이즈가 생긴다. 따라서 얼굴을 인식하지 않도록 작업이 필요하다. 우선 들어온 이미지를 GrayScale 후 히스토그램 균일화를 진행한다. 이미지의 히스토그램이 특정 영역에 집중되어 있으면 contrast가 낮아지기 때문에 좋은 이미지라고 할 수 없다. 그래서 전체 영역에 골고루 분포가 될 수 있게 하는 함수가 Cv2.EqualizeHist이다. 얼굴 인식이 가능한 xml을 활용하여 들어온 이미지에서 얼굴을 찾아 검은 사각형으로 얼굴을 제거한다.

##### 얼굴을 제거한 후 다시 꼭짓점을 검출한다. 그러나 손가락 끝마다 예상했던 꼭짓점 수보다 많은 꼭짓점이 생성되어 Unity 상에서 가상 손 모델을 만들기 위한 좌표를 추출하기에 어려움이 있다. 꼭짓점 수를 줄여야 한다.


### 꼭짓점 줄이기
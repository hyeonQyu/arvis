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

![원본 프레임 이미지](https://user-images.githubusercontent.com/44297538/71531403-e4623880-2931-11ea-9ec0-511250cb6436.jpg)

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
Cv2.Dilate(imgMask1, imgMask1, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)));

//morphological closing 영역의 구멍 메우기
Cv2.Dilate(imgMask1, imgMask1, 						Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)));
Cv2.Erode(imgMask1, imgMask1, Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5)));
```

##### Cv2.CvtColor 함수에서 기본 BGR의 컬러를 HSV로 바꿔준다. 이후 Cv2.InRange 함수에서 정해진 범위 안에 들어가 있는 영역은 0으로 만들어주고, 나머지는 1로 만들어 흑백사진으로 만들어 처리를 한다. 이후 이미지 형태를 전환하기 위해 Erosion과 Dilation을 진행한다.  Erosion은 이미지를 침식시키는데, Foreground가 되는 이미지의 경계 부분을 침식시켜 Background 이미지로 전환하는 작업이다. Dilation은 이미지를 팽창시키고, Erosion과 반대의 역할을 진행한다. Opening과 Closing은 Erosion과 Dilation을 활용하여 노이즈를 없애는 방법이다. Opening에서 Erosion 수행 후 Dilation을 적용하면 작은 노이즈를 없애고, Closing에서 Dilation 수행 후 Erosion을 적용하면 전반적인 이미지가 깨끗해진다.

![피부색을 검출한 Mask 이미지] (https://user-images.githubusercontent.com/44297538/71531408-e6c49280-2931-11ea-9cc1-4a782a7d01c3.jpg)


### 손의 윤곽선 및 꼭짓점 검출

##### 원본 이미지와 mask 이미지를 사용하여 피부색 영역을 검출한다. 피부색을 추출한 후 GrayScale을 수행, 색상을 반전시킨다. 이후 Cv2.Canny 함수를 통해 이미지의 Edge 부분(가장자리)을 검출하도록 한다. 매개변수로 100과 200이 들어가는데, 이는 100 이하에 포함된 Edge는 Edge에서 제외하고, 200 이상에 포함된 Edge는 Edge로 간주하는 임계값이다.


### 얼굴 제거

##### 얼굴도 피부색으로 함께 검출되기 때문에 순수하게 손만 검출해내기에 노이즈가 생긴다. 따라서 얼굴을 인식하지 않도록 작업이 필요하다. 우선 들어온 이미지를 GrayScale 후 히스토그램 균일화를 진행한다. 이미지의 히스토그램이 특정 영역에 집중되어 있으면 contrast가 낮아지기 때문에 좋은 이미지라고 할 수 없다. 그래서 전체 영역에 골고루 분포가 될 수 있게 하는 함수가 Cv2.EqualizeHist이다. 얼굴 인식이 가능한 xml을 활용하여 들어온 이미지에서 얼굴을 찾아 검은 사각형으로 얼굴을 제거한다.

![얼굴을 인식하여 제거한 마스크 이미지](https://user-images.githubusercontent.com/44297538/71531419-edeba080-2931-11ea-9c90-d4b36b164e52.jpg)

##### 얼굴을 제거한 후 다시 꼭짓점을 검출한다. 그러나 손가락 끝마다 예상했던 꼭짓점 수보다 많은 꼭짓점이 생성되어 Unity 상에서 가상 손 모델을 만들기 위한 좌표를 추출하기에 어려움이 있다. 꼭짓점 수를 줄여야 한다.

![꼭짓점 검출](https://user-images.githubusercontent.com/44297538/71531430-f512ae80-2931-11ea-8c91-53b1ecfad422.jpg)


### 꼭짓점 수 최소화

##### 현재 손가락 끝과 엄지손가락 부분에 많은 점들이 존재한다. 우리가 추출해야할 점은 손가락마다 한 개이므로 이점들을 하나의 점으로 통합해야 한다. 통합은 각 점들이 일정 거리(d)만큼 인접해 있다면 같은 점으로 판단해 진행한다.
![](https://user-images.githubusercontent.com/44297538/71531652-09a37680-2933-11ea-8d58-d76e71c325a0.JPG)

##### 일정 거리 d를 상수처럼 정의한다면 손의 크기가 달라짐에 따라, 예를 들어 손이 많이 멀어졌을 때 대부분의 점들이 통합이 되어버리는 문제가 발생하게 된다. 손의 크기에 유연하게 대처하기 위해 손바닥영역을 인식하고 반지름의 크기를 구하여 손의 크기에 따라 d의 값이 변하도록 한다. 
d = radius(손바닥의 반지름) / 2 * 0.8
통계를 통해 도출한 식이다. 손이 가까워지면 d의 값이 커지고 멀어지면 d의 값이 작아짐에 따라 제대로 통합이 된다.

![꼭짓점 수 최소화](https://user-images.githubusercontent.com/44297538/71531432-f7750880-2931-11ea-8abb-7aca4a8bc1a0.jpg)


### 음영 제거

##### 손가락 5개의 좌표만 추출하기 위해 손 영역에서 중앙을 검출하도록 한다. 손가락의 끝 점을 구할 때 손의 중앙으로부터 가장 먼 5개의 점을 추출하여, 손가락의 좌표를 구하는 방식이다. 손 중앙을 검출할 때 6.1.2에서 만든 Mask 이미지의 흰색 부분에서 가장 넓은 부분을 손바닥으로 인식, 이 영역에서 중앙을 찾는다. 이 때 기본 OpenCV 라이브러리 함수 DistanceTransform를 통해 손바닥 중심으로부터 손바닥 가장자리까지의 가장 짧은 거리, 즉, 거리 변환 행렬에서 가장 큰 값을 반환한다. 이 값은 손바닥을 둘러싸는 원의 반지름이 된다. 그러나 이진화 된 Mask 이미지를 사용하기 때문에 조명에 매우 민감하다. 예를 들어 손바닥에 음영이 생기거나 손가락을 접는 경우 손의 중앙을 제대로 찾지 못하는 경우가 발생한다. 이 문제를 해결하기 위해 contour를 통해 추출한 손의 윤곽 내부를 하얗게 칠해 다시 중앙을 찾는 과정을 적용하여 음영에 영향을 받지 않도록 하여 손바닥을 검출한다. 이렇게 검출한 손바닥 이미지를 다시 DistanceTransform 함수를 통해 반지름과 중앙을 검출한다.

![손의 음영 제거 전](https://user-images.githubusercontent.com/44297538/71531433-f93ecc00-2931-11ea-9add-507a4bd6f647.jpg)

![손의 음영 제거 후](https://user-images.githubusercontent.com/44297538/71531438-fb088f80-2931-11ea-8c4a-635cfb05324a.jpg)


### 손가락 끝점 추출

##### 손의 중앙 지점과 최소 개수로 뽑아낸 꼭짓점과의 거리를 각각 비교하여 손의 중앙과 가장 먼 꼭짓점 5개를 찾아 손가락의 끝점으로 인식을 한다.

![손가락 끝점](https://user-images.githubusercontent.com/44297538/71531440-fcd25300-2931-11ea-9595-d37519117825.jpg)


### 부정확한 인식 보정

##### 몇몇 프레임에 대해서는 인식이 정확하게 이루어지지 않아 손가락 끝 점을 나타내는 좌표들이 전혀 이상한 곳을 가리키면서 손가락 끝을 나타내는 구 오브젝트가 멋대로 이동하는 현상이 나타난다. 이러한 현상이 발생하는 이유는 손의 중앙점을 구하기 위해 Contour를 통해 추출한 손의 윤곽선 내부를 칠하는 과정에서 손의 윤곽선이 완전히 닫히지 않는 경우 손의 내부가 아니라 배경을 칠하게 되면서 손의 중앙이 완전히 이상한 곳을 가리켰기 때문이다. 손의 중앙이 틀어짐으로 인해 손 끝의 위치도 틀어지게 된 것이다. 이를 해결하기 위해 인식이 제대로 되지 않는 프레임은 예외로 처리해준다.

##### 인식이 제대로 되지 않는 경우는 크게 두 가지가 있다. 첫 번째로 반지름이 너무 크거나 작은 경우이다. 화면에 손이 없거나 손이 아닌 다른 부분을 인식하게 되면 반지름이 너무 커지거나 작아진다. 두 번째로는 손의 중앙이 음영을 제거하지 않은 Mask 이미지에서 찾은 중앙과 많이 차이가 날 경우이다. 이 경우는 위에서 설명했듯이 손의 윤곽선 내부가 아닌 외부를 칠하게 된 상황으로 인식이 제대로 되지 않은 것이다. 이러한 경우에 대해서는 손 끝점을 찾고 그 끝점을 가상 손을 구성하는 오브젝트로 맵핑 해주는 과정을 생략하게 되어 이전에 인식이 잘 된 프레임의 손 끝점을 유지하게 된다.

## 가상 손 모델 생성 및 AR 환경 제어

##### 최종적으로 추출해 낸 꼭짓점의 이미지 행렬 내의 카메라 이미지 좌표를 Unity 가상 세계의 좌표계로 변환하여 해당 좌표에 맞게 가상 손 모델을 생성하였다. cvt3 변수는 3차원 벡터로 변환된 좌표를 저장하는 변수이다. 매개 변수로 들어오는 _point는 카메라 이미지 좌표를 저장하고 있다. 카메라 이미지 좌표계와 Unity 가상 세계 이미지 좌표계가 서로 다르다. 카메라 이미지 좌표계의 (0,0)은 화면의 정중앙을 나타내지만, Unity 가상 세계 좌표계의 (0,0)은 화면의 왼·아래쪽으로 되어있다. 그래서 카메라 이미지의 x,y 좌표를 Unity 가상 세계 좌표에 맞게 값을 지정해주고, 기기마다 해상도가 다르기 때문에 기기에 맞게 해상도를 맞출 수 있도록 설정하였다. 이번 프로젝트에서는 x, y 좌표만 움직이도록 하고, z 좌표는 AR 오브젝트의 z좌표로 고정하였다. _cvtList는 총 6개의 3차원 벡터를 저장하는데, 0번째는 손의 중앙 좌표를 저장하고, 1~5는 각 손가락의 좌표를 저장한다.

```c#
private Vector3 Point2Vector3(Point _point)
{
	Vector3 cvt3 = new Vector3(0, 0, _object.transform.position.z);
	cvt3.x = (_point.X - gameObject.GetComponent<RectTransform>().sizeDelta.x / 2) * gameObject.GetComponent<Transform>().transform.lossyScale.x;
	cvt3.y = (gameObject.GetComponent<RectTransform>().sizeDelta.y / 2 - _point.Y) * 	gameObject.GetComponent<Transform>().transform.lossyScale.y;
	return cvt3;
}

private void InputPoint(List<Point> pointList)
{
	_cvt3List.Add(Point2Vector3(_center));
	for(int i = 0; i < pointList.Count; i++)
	{
		_cvt3List.Add(Point2Vector3(pointList[i]));
	}
}

![좌표계 변환 결과1](https://user-images.githubusercontent.com/44297538/71532117-3eb0c880-2935-11ea-8a72-973ffa6d5b8c.jpg)

![좌표계 변환 결과2](https://user-images.githubusercontent.com/44297538/71532122-407a8c00-2935-11ea-9ffc-fffb8cf06ea1.jpg)
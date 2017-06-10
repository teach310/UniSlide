using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UniSlide
{
    /// <summary>
    /// RenderTextureを更新
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(USCameraController))]
    public class USSlideRenderer : MonoBehaviour
    {
#if UNITY_EDITOR
        USCameraController cameraController;
        USCameraController CameraController
        {
            get
            {
                if (cameraController == null)
                {
                    cameraController = this.GetComponent<USCameraController>();
                }
                return cameraController;
            }
        }


        public bool isUpdateSlideImage = true;

        void Awake()
        {
            ScriptableSingleton<USEventBus>.instance.OnUpdateAsObservable()
                                           .Subscribe(_ => OnUSUpdate())
                                           .AddTo(this);
        }

        int renderSlideIndex = 0; // レンダリングするスライド
        int updateCount = 0;
        const int renderSlideInterval = 1; // 何フレームごとにレンダリングするか

        void OnUSUpdate()
        {
            if (!isUpdateSlideImage)
                return;

            // TODO シーンが更新されていたら

            if (updateCount >= renderSlideInterval)
            {
                UpdateSlideData(CameraController.SlideCamera);
                updateCount = 0;
            }
            updateCount++;
        }

        /// <summary>
        /// 対象スライドの決定と更新
        /// </summary>
        void UpdateSlideData(Camera cam)
        {
            var slideObject = CameraController.slideObject;
            if (slideObject == null)
                return;

            RenderSlide(cam, slideObject.slideList[renderSlideIndex]);
            renderSlideIndex++;
            if (renderSlideIndex >= slideObject.slideList.Count)
            {
                renderSlideIndex = 0;
            }
        }

        /// <summary>
        /// スライドを更新する
        /// </summary>
        void RenderSlide(Camera cam, UniSlideData slideData)
        {
            var camTran = cam.transform;
            var tempPos = cam.transform.position;
            var tempRot = cam.transform.rotation;
            var tempTargetTexture = cam.targetTexture;
            SetSlideDataToCamera(cam, slideData);
            cam.Render();
            // リセット
            camTran.SetPositionAndRotation(tempPos, tempRot);
            cam.targetTexture = tempTargetTexture;
        }

        // カメラにSlideの情報をセットする
        void SetSlideDataToCamera(Camera cam, UniSlideData slideData)
        {
            cam.transform.SetPositionAndRotation(slideData.camPos, slideData.camRot);
            cam.targetTexture = slideData.rt;
        }
#endif
    }
}

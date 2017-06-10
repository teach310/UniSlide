using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UniRx;

namespace UniSlide
{
    /// <summary>
    /// Slide表示WindowPresenter
    /// </summary>
    public class UniSlideListWindow : EditorWindowBase
    {
        private string _soPath = string.Empty;

        public static int selectedIndex = -1;


        [SerializeField]
        UniSlideListWindowView view;
        //		[SerializeField]
        //		UniSlideListWindowUseCase useCase;

        public UniSlideObject uniSlideObject;

        [MenuItem("Tools/UniSlideListWindow")]
        static void Open()
        {
            GetWindow<UniSlideListWindow>("ListView");
        }

        void OnSelectionChange()
        {
            UniSlideObject tempObject = null;


            // GameObject選択時
            if (Selection.activeGameObject != null)
            {
                var uscc = Selection.activeGameObject.GetComponent<USCameraController>();
                // CameraControllerからUniSlideObjectを取得
                if (uscc != null)
                    tempObject = uscc.slideObject;

            }
            else
            {
                // 直接UniSlideObjectを選択
                tempObject = Selection.activeObject as UniSlideObject;
            }
            if (tempObject != null) SelectDatabaseObject(tempObject);
        }

        // 選択したデータの情報取得
        void SelectDatabaseObject(UniSlideObject slide)
        {
            if (slide == this.uniSlideObject)
                return;

            view.ResetList(slide);
            if (slide != null)
            {
                this.uniSlideObject = slide;
                // ScriptableObject の　Pathを取得
                _soPath = AssetDatabase.GetAssetPath(uniSlideObject);
            }
            Repaint();
        }

        protected override void Init()
        {
            view = new UniSlideListWindowView();
            //			useCase = new UniSlideListWindowUseCase ();
            SetListener();
        }

        void SetListener()
        {
            view.OnAddItemAsObservable().Subscribe(slideList =>
            {
                var element = CreateCamera();

                // 要素を追加
                slideList.list.Add(element);


                // 最後の要素を選択状態にする
                slideList.index = slideList.count - 1;
                // 増やした場合のみこちらで変更
                USCameraController.Instance.events.UpdateMap(USCameraController.Instance.slideObject);
                Repaint();
            });

            view.OnRemoveItemAsObservable().Subscribe(slideList =>
            {
                USCameraController.Instance.slideObject.DeleteRT(slideList.index);
                AssetDatabase.ImportAsset(_soPath);
                ReorderableList.defaultBehaviours.DoRemoveButton(slideList);
                Repaint();
            });

            view.OnSelectItemAsObservable().Subscribe(slideList =>
            {
                selectedIndex = slideList.index;
            });
        }

        void OnInspectorUpdate()
        {
            // UniSlideで使用する共通のUpdate
            ScriptableSingleton<USEventBus>.instance.DoUpdate();
            Repaint();
        }

        void OnGUI()
        {
            InitializeIfNeeded();

            if (uniSlideObject == null)
            {
                EditorGUILayout.LabelField("slidedata is null");
                return;
            }

            view.SetSOIfNull(uniSlideObject);

			selectedIndex = view._slideList.index;

            view.OnGUI();

        }

        public UniSlideData CreateCamera()
        {
            var srcTran = SceneCamera.transform;
            var data = new UniSlideData(srcTran.position, srcTran.rotation);
            //親に child オブジェクトを追加
            AssetDatabase.AddObjectToAsset(data.rt, _soPath);

            //インポート処理を走らせて最新の状態にする
            AssetDatabase.ImportAsset(_soPath);
            return data;
        }

        public Camera SceneCamera
        {
            get { return SceneView.lastActiveSceneView.camera; }
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace UniSlide
{
	public class UniSlideListWindow : EditorWindow
	{

		private bool isInited = false;
		private bool isOpenSlide = false;
		[SerializeField]
		private ReorderableList _slideList = null;
		[SerializeField]
		private SerializedObject so = null;
		private string _soPath = string.Empty;

		public static int selectedIndex = -1;
		private Vector2 _scrollPos = Vector2.zero;


		[MenuItem ("Tools/UniSlideListWindow")]
		static void Open ()
		{
			GetWindow<UniSlideListWindow> ();
		}

		// 初期化
		void InitializeIfNeeded ()
		{
			if (isInited == true) {
				return;
			}

			titleContent = new GUIContent ("ListView");
			isInited = true;


		}

		// 値の反映
		void OnInspectorUpdate(){
			if (!USCameraController.HasInstance()) {
				return;
			}
			USCameraController.Instance.UpdateSlideData ();
			Repaint ();
		}

		// 選択したデータの情報取得
		void SelectDatabaseObject (UniSlideObject uniSlideObject)
		{
			if (uniSlideObject == null) {
				_slideList = null;
				so = null;
			} else {
				// ScriptableObject の　Pathを取得
				_soPath = AssetDatabase.GetAssetPath (uniSlideObject);

				_slideList = new ReorderableList(uniSlideObject.slideList, typeof(UniSlideData), true, true, true, true);
				_slideList.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, uniSlideObject.name); };
				_slideList.elementHeight = UniSlideData.SlideImgHeight + 4;
				_slideList.drawElementCallback = DrawProperty;
				_slideList.onAddCallback += OnAddItem;
				_slideList.onRemoveCallback += OnRemoveItem;
				_slideList.onSelectCallback += OnSelectItem;


				so = new SerializedObject (uniSlideObject);

			}
			Repaint ();
		}

		void DrawProperty(Rect rect, int index, bool isActive, bool isFocused)
		{
			var slideListSO = so.FindProperty ("slideList");
			var element = slideListSO.GetArrayElementAtIndex (index);
			rect.height -= 4;
			rect.y += 2;
			//EditorGUI.PropertyField (rect, element);


			// 各プロパティーの SerializeProperty を求める
			var iconProperty = element.FindPropertyRelative ("rt");
			var tex = (Texture)iconProperty.objectReferenceValue;

			var iconRect = new Rect (rect) {
				width = UniSlideData.SlideImgWidth,
			};
			if (tex == null) {
				return;
			}
			GUI.DrawTexture (iconRect, tex);


		}




		void OnGUI ()
		{
			InitializeIfNeeded ();
			if (!USCameraController.HasInstance()) {
				isOpenSlide = false;
				EditorGUILayout.LabelField ("USCameraController is not in the scene");
				EditorGUILayout.LabelField ("Or Play Once");
				return;
			}

			if (USCameraController.Instance.slideData == null) {
				isOpenSlide = false;
				EditorGUILayout.LabelField ("slidedata is null");
				return;
			}

			if (so == null)
				isOpenSlide = false;

			if (!isOpenSlide) {
				SelectDatabaseObject (USCameraController.Instance.slideData);
				isOpenSlide = true;
			}



			if (so == null)
				return;



			so.Update ();

			using (var scrollView = new EditorGUILayout.ScrollViewScope (_scrollPos)) {
				_scrollPos = scrollView.scrollPosition;
				// リスト表示
				_slideList.DoLayoutList ();
				selectedIndex = _slideList.index;
			}

			so.ApplyModifiedProperties ();
		}

		void OnAddItem (ReorderableList slideList)
		{
			var element = CreateCamera();

			// 要素を追加
			slideList.list.Add(element);


			// 最後の要素を選択状態にする
			slideList.index = slideList.count - 1;
			// 増やした場合のみこちらで変更
			USCameraController.Instance.events.UpdateMap (USCameraController.Instance.slideData);
			Repaint ();

		}


		void OnRemoveItem(ReorderableList slideList){

			USCameraController.Instance.slideData.DeleteRT(slideList.index);
			AssetDatabase.ImportAsset (_soPath);
			ReorderableList.defaultBehaviours.DoRemoveButton (slideList);
			Repaint ();
		}

		void OnSelectItem(ReorderableList slideList){
			selectedIndex = slideList.index;
		}



		public UniSlideData CreateCamera(){
			
			var srcTran = SceneCamera.transform;
			var data = new UniSlideData(srcTran.position, srcTran.rotation);
			//親に child オブジェクトを追加
			AssetDatabase.AddObjectToAsset (data.rt, _soPath);

			//インポート処理を走らせて最新の状態にする
			AssetDatabase.ImportAsset (_soPath);
			return data;
		}

		public Camera SceneCamera{
			get{ return SceneView.lastActiveSceneView.camera; }
		}

	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UniRx;

namespace UniSlide
{
	public class UniSlideListWindow : EditorWindowBase
	{

		private bool isOpenSlide = false;
		private string _soPath = string.Empty;

		public static int selectedIndex = -1;
		private Vector2 _scrollPos = Vector2.zero;

		[SerializeField]
		UniSlideListWindowView view;

		[MenuItem ("Tools/UniSlideListWindow")]
		static void Open ()
		{
			GetWindow<UniSlideListWindow> ("ListView");
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
			view.ResetList (uniSlideObject);
			if (uniSlideObject != null) {
				// ScriptableObject の　Pathを取得
				_soPath = AssetDatabase.GetAssetPath (uniSlideObject);
			}
			Repaint ();
		}

		protected override void Init ()
		{
			view = new UniSlideListWindowView ();
			SetListener ();
		}

		void SetListener(){
			view.OnAddItemAsObservable ().Subscribe (slideList => {


				var element = CreateCamera ();

				// 要素を追加
				slideList.list.Add (element);


				// 最後の要素を選択状態にする
				slideList.index = slideList.count - 1;
				// 増やした場合のみこちらで変更
				USCameraController.Instance.events.UpdateMap (USCameraController.Instance.slideData);
				Repaint ();
			});

			view.OnRemoveItemAsObservable ().Subscribe (slideList => {
				USCameraController.Instance.slideData.DeleteRT(slideList.index);
				AssetDatabase.ImportAsset (_soPath);
				ReorderableList.defaultBehaviours.DoRemoveButton (slideList);
				Repaint ();
			});

			view.OnSelectItemAsObservable ().Subscribe (slideList=>{
				selectedIndex = slideList.index;
			});
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

			if (view.so == null)
				isOpenSlide = false;

			if (!isOpenSlide) {
				SelectDatabaseObject (USCameraController.Instance.slideData);
				isOpenSlide = true;
			}
				
			if (view.so == null)
				return;

			view.so.Update ();

			using (var scrollView = new EditorGUILayout.ScrollViewScope (_scrollPos)) {
				_scrollPos = scrollView.scrollPosition;
				view.OnGUI ();
				selectedIndex = view._slideList.index;
			}

			view.so.ApplyModifiedProperties ();
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEditorInternal;
using UnityEditor;

namespace UniSlide
{
    [System.Serializable]
    public class UniSlideListWindowView : IGUI
    {
        Subject<ReorderableList> onAddItemSubject = new Subject<ReorderableList>();
        public IObservable<ReorderableList> OnAddItemAsObservable()
        {
            return onAddItemSubject;
        }

        Subject<ReorderableList> onRemoveItemSubject = new Subject<ReorderableList>();
        public IObservable<ReorderableList> OnRemoveItemAsObservable()
        {
            return onRemoveItemSubject;
        }

        Subject<ReorderableList> onSelectItemSubject = new Subject<ReorderableList>();
        public IObservable<ReorderableList> OnSelectItemAsObservable()
        {
            return onSelectItemSubject;
        }


        public ReorderableList _slideList = null;

        private Vector2 _scrollPos = Vector2.zero;

        public SerializedObject so = null;
        public void SetSOIfNull(UniSlideObject uniSlideObject) {
            if (so == null)
                ResetList(uniSlideObject);
        }


		public UniSlideListWindowView ()
		{
		}


		public void ResetList (UniSlideObject uniSlideObject)
		{
			if (uniSlideObject == null) {
				_slideList = null;
				so = null;
			} else {
				_slideList = new ReorderableList (uniSlideObject.slideList, typeof(UniSlideData), true, true, true, true);
				_slideList.drawHeaderCallback = (Rect rect) => {
					EditorGUI.LabelField (rect, uniSlideObject.name);
				};
				_slideList.elementHeight = UniSlideData.SlideImgHeight + 4;
				_slideList.drawElementCallback = DrawProperty;
				_slideList.onAddCallback += OnAddItem;
				_slideList.onRemoveCallback += OnRemoveItem;
				_slideList.onSelectCallback += OnSelectItem;

				so = new SerializedObject (uniSlideObject);
			}
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

		void OnAddItem (ReorderableList slideList)
		{
			onAddItemSubject.OnNext (slideList);
		}

		void OnRemoveItem(ReorderableList slideList){
			onRemoveItemSubject.OnNext (slideList);
		}

		void OnSelectItem(ReorderableList slideList){
			onSelectItemSubject.OnNext (slideList);
		}

		public void OnGUI ()
		{
            if (so == null)
            {
                EditorGUILayout.LabelField("so is null");
                return;
            }
            

            so.Update();
            using (var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPos))
            {
                _scrollPos = scrollView.scrollPosition;
                // リスト表示
                _slideList.DoLayoutList();
            }
            so.ApplyModifiedProperties();
		}
	}
}

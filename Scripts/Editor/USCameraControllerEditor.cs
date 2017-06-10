using UnityEngine;
using System.Collections;
using UnityEditor;

namespace UniSlide
{
	[CustomEditor (typeof(USCameraController))]
	public class USCameraControllerEditor : Editor
	{
		SerializedProperty _slideEventMap;

		void OnEnable(){
			_slideEventMap = serializedObject.FindProperty ("events").FindPropertyRelative ("_slideEventMap");
		}

		public override void OnInspectorGUI ()
		{
			DrawDefaultInspector ();
			serializedObject.Update ();
            if (UniSlideListWindow.selectedIndex != -1)
            {

                // UniSlideListWindow, OnAddItemで対処
                var uscc = (USCameraController)target;
                uscc.events.UpdateMap(uscc.slideObject);

                if (_slideEventMap.arraySize - 1 < UniSlideListWindow.selectedIndex)
                    return;

                //EditorGUILayout.Space ();
                EditorGUILayout.LabelField("Slide" + UniSlideListWindow.selectedIndex.ToString());





                var element = _slideEventMap.GetArrayElementAtIndex(UniSlideListWindow.selectedIndex);

                if (GUILayout.Button("SetSceneCameraPos"))
                {
                    var data = uscc.slideObject.slideList[UniSlideListWindow.selectedIndex];
                    uscc.transform.position = data.camPos;
                    uscc.transform.rotation = data.camRot;
                }

                if (GUILayout.Button(("Update"))) {
                    var targetData = uscc.slideObject.slideList[UniSlideListWindow.selectedIndex];
                    targetData.camPos = uscc.transform.position;
                    targetData.camRot = uscc.transform.rotation;
                }
        
				var onEnterEvent = element.FindPropertyRelative ("onEnter");

				EditorGUILayout.PropertyField (onEnterEvent);
			}
			serializedObject.ApplyModifiedProperties ();

		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System;

namespace UniSlide
{
	[CreateAssetMenu (menuName = "UniSlide")]
	public class UniSlideObject : ScriptableObject
	{
		public List<UniSlideData> slideList = new List<UniSlideData> ();

		public void DeleteRT(int index){
			if (slideList.Count <= index ) {
				return;
			}
			if (slideList [index].rt != null) {
				Object.DestroyImmediate (slideList [index].rt, true);
			}
			slideList [index].rt = null;
		}
	}
}
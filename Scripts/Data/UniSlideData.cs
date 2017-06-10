using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace UniSlide
{
	[Serializable]
	public class UniSlideData
    {
		public static readonly int SlideImgHeight = 80;
		public static readonly int SlideImgWidth = 120;

		public int id;
		public Vector3 camPos;
		public Quaternion camRot;
		public RenderTexture rt;

		public UniSlideData(){}

		public UniSlideData(Vector3 pos, Quaternion rot){
			camPos = pos;
			camRot = rot;
			this.rt = new RenderTexture (SlideImgWidth,SlideImgHeight, 24, RenderTextureFormat.ARGB4444);
			id = (int)TimeUtil.GetUnixTime (System.DateTime.Now);
			this.rt.name = "RT" + id.ToString();
		}
	}

}

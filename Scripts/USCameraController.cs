using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using System.Linq;

namespace UniSlide
{
	[System.Serializable]
	public class UniSlideEventDictionary{
		[System.Serializable]
		public class SlideEvent
		{
			public string id;
			public UnityEvent onEnter;
			public SlideEvent(UniSlideData data){
				this.id = data.rt.name;
				onEnter = new UnityEvent();
			}
		}

		[SerializeField]
		private List<SlideEvent> _slideEventMap;

		public UnityEvent GetEvent(int index){
			return _slideEventMap [index].onEnter;
		}

		public UniSlideEventDictionary(){
			_slideEventMap = new List<SlideEvent> ();
		}

		// Mapを更新
		public void UpdateMap(UniSlideObject obj){
			_slideEventMap = obj.slideList.Select (x => SelectEvent (x)).ToList();
		}

		public SlideEvent SelectEvent(UniSlideData data){

			if (_slideEventMap.Any (x=>x.id == data.rt.name)) {
				return _slideEventMap.First(x=>x.id == data.rt.name);
			}

			return new SlideEvent (data);
		}
	}

	[ExecuteInEditMode]
	public class USCameraController : SingletonMonoBehaviour<USCameraController>
	{
		int currentSlideIndex = 0;
		public UniSlideObject slideData;
		public float duration = 1f;

		private Tween _currentTween;
		[HideInInspector]
		public UniSlideEventDictionary events = new UniSlideEventDictionary();

		public bool isUpdateSlideImage = true;

		public void UpdateSlideData ()
		{
			if (slideData == null)
				return;

			var camera = this.GetComponent<Camera> ();
			var tempPos = this.transform.position;
			var tempRot = this.transform.rotation;
			for (int i = 0; i < slideData.slideList.Count; i++) {
				RenderSlide (camera, slideData.slideList [i]);
			}
			this.transform.position = tempPos;
			this.transform.rotation = tempRot;
			camera.targetTexture = null;
		}

		void RenderSlide (Camera cam, UniSlideData data)
		{
			if (!isUpdateSlideImage) {
				return;
			}
			cam.transform.position = data.camPos;
			cam.transform.rotation = data.camRot;
			cam.targetTexture = data.rt;
			cam.Render ();
		}

		void Start ()
		{
			if (!Application.isPlaying)
				return;
			//Screen.SetResolution (1280, 720, true);
			var data = slideData.slideList [0];
			this.transform.position = data.camPos;
			this.transform.rotation = data.camRot;
		}

		void Update (){


			if (!Application.isPlaying){
				CheckInstance ();
				return;
			}
			if (_currentTween != null) {
				if (_currentTween.IsPlaying ())
					return;
			}
			
			if (Input.GetKeyDown (KeyCode.RightArrow)) {
				if (currentSlideIndex < slideData.slideList.Count - 1) {
					_currentTween = PushSlide ();
				}
			}
			if (Input.GetKeyDown (KeyCode.LeftArrow)) {
				if (currentSlideIndex != 0) {
					_currentTween = BackSlide ();
				}
			}
		}

		public Tween PushSlide ()
		{
			var index = currentSlideIndex + 1;
			var tween = TweenSlide (index);
			tween.OnComplete (() => {
				currentSlideIndex++;

				events.GetEvent(index).Invoke();
			});
			return tween;

		}

		public Tween BackSlide ()
		{
			var index = currentSlideIndex - 1;
			var tween = TweenSlide (index);
			tween.OnComplete (() => currentSlideIndex--);
			return tween;
		}

		public Tween TweenSlide(int index){
			var data = slideData.slideList [index];
			var seq = DOTween.Sequence ();
			seq.Join (this.transform.DOMove (data.camPos, duration))
				.Join (this.transform.DORotate (data.camRot.eulerAngles, duration));
			return seq;
		}

	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using System.Linq;
using UniRx;

namespace UniSlide
{
	[System.Serializable]
	public class UniSlideEventDictionary
	{
		[System.Serializable]
		public class SlideEvent
		{
			public string id;
			public UnityEvent onEnter;

			public SlideEvent (UniSlideData data)
			{
				this.id = data.rt.name;
				onEnter = new UnityEvent ();
			}
		}

		[SerializeField]
		private List<SlideEvent> _slideEventMap;

		public UnityEvent GetEvent (int index)
		{
			return _slideEventMap [index].onEnter;
		}

		public UniSlideEventDictionary ()
		{
			_slideEventMap = new List<SlideEvent> ();
		}

		// Mapを更新
		public void UpdateMap (UniSlideObject obj)
		{
			_slideEventMap = obj.slideList.Select (x => SelectEvent (x)).ToList ();
		}

		public SlideEvent SelectEvent (UniSlideData data)
		{

			if (_slideEventMap.Any (x => x.id == data.rt.name)) {
				return _slideEventMap.First (x => x.id == data.rt.name);
			}

			return new SlideEvent (data);
		}
	}


	[RequireComponent (typeof(Camera))]
	public class USCameraController : SingletonMonoBehaviour<USCameraController>
	{
		int currentSlideIndex = 0;
		public UniSlideObject slideObject;
		public float duration = 1f;

		private Tween _currentTween;
		[HideInInspector]
		public UniSlideEventDictionary events = new UniSlideEventDictionary ();

		private Camera slideCamera;
		public Camera SlideCamera{
			get { 
				if (slideCamera == null) {
					slideCamera = this.GetComponent<Camera> ();
				}
				return slideCamera;
			}
		}


		void Start ()
		{
			//Screen.SetResolution (1280, 720, true);
			var data = slideObject.slideList [0];
			this.transform.position = data.camPos;
			this.transform.rotation = data.camRot;
		}

		void Update ()
		{
			if (_currentTween != null) {
				if (_currentTween.IsPlaying ())
					return;
			}
			
			if (Input.GetKeyDown (KeyCode.RightArrow)) {
				if (currentSlideIndex < slideObject.slideList.Count - 1) {
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

				events.GetEvent (index).Invoke ();
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

		public Tween TweenSlide (int index)
		{
			var data = slideObject.slideList [index];
			var seq = DOTween.Sequence ();
			seq.Join (this.transform.DOMove (data.camPos, duration))
				.Join (this.transform.DORotate (data.camRot.eulerAngles, duration));
			return seq;
		}
	}
}

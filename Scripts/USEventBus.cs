using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEditor;

namespace UniRx
{
	public class USEventBus : ScriptableSingleton<USEventBus>
	{
		[SerializeField] Subject<Unit> onUpdateSubject = new Subject<Unit>();
		public IObservable<Unit> OnUpdateAsObservable(){
			return onUpdateSubject;
		}

		public void DoUpdate(){
			onUpdateSubject.OnNext (Unit.Default);
		}
	}
}

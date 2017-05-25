using UnityEditor;

public abstract class EditorWindowBase : EditorWindow {
	private bool isInitialized = false;
	protected virtual bool IsInitialized(){
		return isInitialized;
	}

	protected virtual void Init(){}

	// 初期化
	protected virtual void InitializeIfNeeded(){
		if (!IsInitialized ()) {
			Init ();
			isInitialized = true;
		}
	}
}

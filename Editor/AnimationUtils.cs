//----------------------------------------------------------------------------------------------
// Created by Leonid [Zanleo] Voitko (12/27/2015)
//----------------------------------------------------------------------------------------------

using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using Object = UnityEngine.Object;

public enum TangentMode
{
	Editable = 0,
	Smooth = 1,
	Linear = 2,
	Stepped = Linear | Smooth,
}

public static class AnimationUtils
{
	[MenuItem("GameObject/Animation/Animation => animator", false, 11)]
	private static void MoveAnimationToAnimator()
	{
		var selected = Selection.activeObject as GameObject;
		var animation = selected.GetComponent<Animation>();
		if (animation == null)
		{
			Debug.LogError("Null animation on this object");
			return;
		}

		var animator = selected.AddComponent<Animator>();
		var fakeController = AnimatorController.CreateAnimatorControllerAtPath("Assets/FakeAnimatorController.controller");
		animator.runtimeAnimatorController = fakeController;

		foreach (AnimationState animState in animation)
		{
			var animClip = animState.clip;
			animClip.legacy = false;
			EditorUtility.SetDirty(animClip);
			fakeController.AddMotion(animClip);
		}

		EditorUtility.SetDirty(selected);
		EditorUtility.SetDirty(fakeController);
		Object.DestroyImmediate(animation, false);
	}

	[MenuItem("GameObject/Animation/Animator => animation", false, 11)]
	private static void MoveAnimatorToAnimation()
	{
		var selected = Selection.activeObject as GameObject;
		var animator = selected.GetComponent<Animator>();
		if (animator == null)
		{
			Debug.LogError("Null Animator on this object");
			return;
		}

		var animation = selected.AddComponent<Animation>();
		var animController = animator.runtimeAnimatorController as AnimatorController;

		foreach (var animClip in animController.animationClips)
		{
			animClip.legacy = true;
			EditorUtility.SetDirty(animClip);
			animation.AddClip(animClip, animClip.name);
			if (animation.clip == null)
			{
				animation.clip = animClip;
			}
		}

		EditorUtility.SetDirty(selected);
		Object.DestroyImmediate(animator, false);
		var path = AssetDatabase.GetAssetPath(animController);
		AssetDatabase.DeleteAsset(path);
		AssetDatabase.Refresh();
	}

	[MenuItem("Assets/Animation/Create mirrored")]
	private static void CreateMirroredAnimation()
	{
		var selectedAnim = Selection.activeObject as AnimationClip;
		string path = AssetDatabase.GetAssetPath(selectedAnim);
		path = path.Substring(0, path.Length - ".anim".Length);
		path += "Mirrored";
		var newClip = new AnimationClip
						  {
							  name = path.Substring(path.LastIndexOf("/") + 1),
							  frameRate = selectedAnim.frameRate,
							  legacy = selectedAnim.legacy,
							  localBounds = selectedAnim.localBounds,
							  wrapMode = selectedAnim.wrapMode
						  };
		Debug.Log(path + ": " + selectedAnim.length);

		var curvesBindings = AnimationUtility.GetCurveBindings(selectedAnim);
		for (int i = curvesBindings.Length - 1; i >= 0; i--)
		{
			var curveBinding = curvesBindings[i];
			var animCurve = AnimationUtility.GetEditorCurve(selectedAnim, curveBinding);
			var keys = animCurve.keys;
			for (int j = 0; j < keys.Length; j++)
			{
				var keyframe = keys[j];
				keyframe.time = selectedAnim.length - keyframe.time;
				keyframe.inTangent = keyframe.outTangent;
				keyframe.outTangent = keyframe.inTangent;
				keyframe.tangentMode = keyframe.tangentMode;
				keys[j] = keyframe;
			}

			var newCurve = new AnimationCurve(keys)
								{
									preWrapMode = animCurve.preWrapMode,
									postWrapMode = animCurve.postWrapMode
								};
			newCurve.UpdateAllLinearTangents();
			AnimationUtility.SetEditorCurve(newClip, curveBinding, newCurve);
		}

		ReplaceOrCreate(path + ".anim", newClip);
		UnityEngine.Object.DestroyImmediate(newClip);
	}

	[MenuItem("Assets/Animation/Cut all scales")]
	private static void CutAllScales()
	{
		var selectedAnim = Selection.activeObject as AnimationClip;
		string path = AssetDatabase.GetAssetPath(selectedAnim);
		var newClip = new AnimationClip
						  {
							  name = path.Substring(path.LastIndexOf("/") + 1),
							  frameRate = selectedAnim.frameRate,
							  legacy = selectedAnim.legacy,
							  localBounds = selectedAnim.localBounds,
							  wrapMode = selectedAnim.wrapMode
						  };
		Debug.Log(path + ": " + selectedAnim.length);

		var curvesBindings = AnimationUtility.GetCurveBindings(selectedAnim);
		for (int i = 0; i < curvesBindings.Length; i++)
		{
			var curveBinding = curvesBindings[i];
			if (curveBinding.propertyName.Contains("Scale"))
			{
				Debug.Log("Cut: " + curveBinding.path + "; " + curveBinding.propertyName + " of " + curveBinding.type);
				continue;
			}

			var animCurve = AnimationUtility.GetEditorCurve(selectedAnim, curveBinding);
			var keys = animCurve.keys;
			var newCurve = new AnimationCurve(keys)
								{
									preWrapMode = animCurve.preWrapMode,
									postWrapMode = animCurve.postWrapMode
								};
			newCurve.UpdateAllLinearTangents();
			AnimationUtility.SetEditorCurve(newClip, curveBinding, newCurve);
		}

		var animEvents = AnimationUtility.GetAnimationEvents(selectedAnim);
		AnimationUtility.SetAnimationEvents(newClip, animEvents);

		ReplaceOrCreate(path, newClip);
		UnityEngine.Object.DestroyImmediate(newClip);
	}

	[MenuItem("Assets/Animation/Cut all '_end' curves")]
	private static void CutAllEndCurves()
	{
		var selectedAnim = Selection.activeObject as AnimationClip;
		string path = AssetDatabase.GetAssetPath(selectedAnim);
		var newClip = new AnimationClip
						  {
							  name = path.Substring(path.LastIndexOf("/") + 1),
							  frameRate = selectedAnim.frameRate,
							  legacy = selectedAnim.legacy,
							  localBounds = selectedAnim.localBounds,
							  wrapMode = selectedAnim.wrapMode
						  };
		Debug.Log(path + ": " + selectedAnim.length);

		var curvesBindings = AnimationUtility.GetCurveBindings(selectedAnim);
		for (int i = 0; i < curvesBindings.Length; i++)
		{
			var curveBinding = curvesBindings[i];
			if (curveBinding.path.EndsWith("_end"))
			{
				Debug.Log("Cut: " + curveBinding.path + "; " + curveBinding.propertyName + " of " + curveBinding.type);
				continue;
			}

			var animCurve = AnimationUtility.GetEditorCurve(selectedAnim, curveBinding);
			var keys = animCurve.keys;
			var newCurve = new AnimationCurve(keys)
								{
									preWrapMode = animCurve.preWrapMode,
									postWrapMode = animCurve.postWrapMode
								};
			newCurve.UpdateAllLinearTangents();
			AnimationUtility.SetEditorCurve(newClip, curveBinding, newCurve);
		}

		var animEvents = AnimationUtility.GetAnimationEvents(selectedAnim);
		AnimationUtility.SetAnimationEvents(newClip, animEvents);

		ReplaceOrCreate(path, newClip);
		UnityEngine.Object.DestroyImmediate(newClip);
	}

	[MenuItem("Assets/Animation/Cut all '_end' curves", true)]
	[MenuItem("Assets/Animation/Cut all scales", true)]
	[MenuItem("Assets/Animation/Create mirrored", true)]
	private static bool IsAnimationClip()
	{
		if (Selection.activeObject == null)
		{
			return false;
		}

		return Selection.activeObject is AnimationClip;
	}

	private static void ReplaceOrCreate(string path, AnimationClip animClip)
	{
		AnimationClip outputAnimClip = AssetDatabase.LoadMainAssetAtPath(path) as AnimationClip;
		if (outputAnimClip == null)
		{
			outputAnimClip = new AnimationClip();
			AssetDatabase.CreateAsset(outputAnimClip, path);
		}
		EditorUtility.CopySerialized(animClip, outputAnimClip);
		AssetDatabase.SaveAssets();
	}

	public static void UpdateAllLinearTangents(this AnimationCurve curve)
	{
		for (int i = 0; i < curve.keys.Length; i++)
		{
			UpdateTangentsFromMode(curve, i);
		}
	}

	public static void UpdateTangentsFromMode(AnimationCurve curve, int index)
	{
		if (index < 0 || index >= curve.length)
			return;

		Keyframe key = curve[index];
		if (GetKeyTangentMode(key, 0) == TangentMode.Linear && index >= 1)
		{
			key.inTangent = CalculateLinearTangent(curve, index, index - 1);
			curve.MoveKey(index, key);
		}
		if (GetKeyTangentMode(key, 1) == TangentMode.Linear && index + 1 < curve.length)
		{
			key.outTangent = CalculateLinearTangent(curve, index, index + 1);
			curve.MoveKey(index, key);
		}
		if (GetKeyTangentMode(key, 0) != TangentMode.Smooth && GetKeyTangentMode(key, 1) != TangentMode.Smooth)
			return;

		curve.SmoothTangents(index, 0.0f);
	}

	private static float CalculateLinearTangent(AnimationCurve curve, int index, int toIndex)
	{
		return (float) (((double) curve[index].value - (double) curve[toIndex].value) /
							 ((double) curve[index].time - (double) curve[toIndex].time));
	}

	public static void SetKeyTangentMode(object keyframe, int leftRight, TangentMode mode)
	{
		Type t = typeof(Keyframe);
		FieldInfo field = t.GetField("m_TangentMode", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		int tangentMode = (int) field.GetValue(keyframe);

		if (leftRight == 0)
		{
			tangentMode &= -7;
			tangentMode |= (int) mode << 1;
		}
		else
		{
			tangentMode &= -25;
			tangentMode |= (int) mode << 3;
		}

		field.SetValue(keyframe, tangentMode);
		if (GetKeyTangentMode(tangentMode, leftRight) == mode)
			return;

		Debug.LogError("bug");
	}

	public static TangentMode GetKeyTangentMode(Keyframe keyframe, int leftRight)
	{
		Type t = typeof(UnityEngine.Keyframe);
		FieldInfo field = t.GetField("m_TangentMode", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		return GetKeyTangentMode((int) field.GetValue(keyframe), leftRight);
	}

	public static TangentMode GetKeyTangentMode(int tangentMode, int leftRight)
	{
		if (leftRight == 0)
			return (TangentMode) ((tangentMode & 6) >> 1);
		else
			return (TangentMode) ((tangentMode & 24) >> 3);
	}
}
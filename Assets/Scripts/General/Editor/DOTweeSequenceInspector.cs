using DG.DOTweenEditor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(DOTweenSequence))]
public class DOTweeSequenceInspector : Editor
{
    SerializedProperty mSequence;
    ReorderableList mSequenceList;

    GUIContent mPlayBtnContent;
    GUIContent mRewindBtnContent;
    GUIContent mResetBtnContent;
    GUILayoutOption mBtnHeight;

    void OnEnable()
    {
        mPlayBtnContent = EditorGUIUtility.TrIconContent("d_PlayButton@2x", "播放");
        mRewindBtnContent = EditorGUIUtility.TrIconContent("d_preAudioAutoPlayOff@2x", "倒放");
        mResetBtnContent = EditorGUIUtility.TrIconContent("d_preAudioLoopOff@2x", "重置");
        mBtnHeight = GUILayout.Height(35);
        mSequence = serializedObject.FindProperty("mSequence");
        mSequenceList = new ReorderableList(serializedObject, mSequence);
        mSequenceList.drawElementCallback = OnDrawSequenceItem;
        mSequenceList.elementHeightCallback = index =>
        {
            var item = mSequence.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(item);
        };
        mSequenceList.drawHeaderCallback = OnDrawSequenceHeader;
    }

    public override void OnInspectorGUI()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(mPlayBtnContent, mBtnHeight))
                {
                    if (DOTweenEditorPreview.isPreviewing)
                    {
                        DOTweenEditorPreview.Stop(true);
                        ((DOTweenSequence)target).DOKill();
                    }
                    DOTweenEditorPreview.PrepareTweenForPreview(((DOTweenSequence)target).DOPlay());
                    DOTweenEditorPreview.Start();
                }
                // if (GUILayout.Button(mRewindBtnContent, mBtnHeight))
                // {
                //     if (DOTweenEditorPreview.isPreviewing)
                //     {
                //         DOTweenEditorPreview.Stop(true);
                //         ((DOTweenSequence)target).DOKill();
                //     }
                //     DOTweenEditorPreview.PrepareTweenForPreview(((DOTweenSequence)target).DORewind());
                //     DOTweenEditorPreview.Start();
                // }
                if (GUILayout.Button(mResetBtnContent, mBtnHeight))
                {
                    DOTweenEditorPreview.Stop(true);
                    ((DOTweenSequence)target).DOKill();
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        serializedObject.Update();
        mSequenceList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();
    }

    static void OnDrawSequenceHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, "Animation Sequences");
    }

    void OnDrawSequenceItem(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty element = mSequence.GetArrayElementAtIndex(index);
        EditorGUI.PropertyField(rect, element, true);
    }
}
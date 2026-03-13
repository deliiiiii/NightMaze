using DG.Tweening;
using General;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomPropertyDrawer(typeof(DOTweenSequence.SequenceAnimation))]
public class SequenceTweenMoveDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var onPlay = property.FindPropertyRelative("OnPlay");
        var onUpdate = property.FindPropertyRelative("OnUpdate");
        var onComplete = property.FindPropertyRelative("OnComplete");
        return EditorGUIUtility.singleLineHeight * 11 + (property.isExpanded ? EditorGUI.GetPropertyHeight(onPlay) + EditorGUI.GetPropertyHeight(onUpdate) + EditorGUI.GetPropertyHeight(onComplete) : 0);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.indentLevel++;
        var target = property.FindPropertyRelative("Target");
        var addType = property.FindPropertyRelative("AddType");
        var tweenType = property.FindPropertyRelative("AnimationType");
        var toValue = property.FindPropertyRelative("ToValue");
        var useToTarget = property.FindPropertyRelative("UseToTarget");
        var toTarget = property.FindPropertyRelative("ToTarget");
        var useFromValue = property.FindPropertyRelative("UseFromValue");
        var fromValue = property.FindPropertyRelative("FromValue");
        var duration = property.FindPropertyRelative("DurationOrSpeed");
        var speedBased = property.FindPropertyRelative("SpeedBased");
        var delay = property.FindPropertyRelative("Delay");
        var customEase = property.FindPropertyRelative("CustomEase");
        var ease = property.FindPropertyRelative("Ease");
        var easeCurve = property.FindPropertyRelative("EaseCurve");
        var loops = property.FindPropertyRelative("Loops");
        var loopType = property.FindPropertyRelative("LoopType");
        var updateType = property.FindPropertyRelative("UpdateType");
        var snapping = property.FindPropertyRelative("Snapping");
        var onPlay = property.FindPropertyRelative("OnPlay");
        var onUpdate = property.FindPropertyRelative("OnUpdate");
        var onComplete = property.FindPropertyRelative("OnComplete");

        var lastRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(lastRect, addType);

        EditorGUI.BeginChangeCheck();
        lastRect.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(lastRect, target);
        lastRect.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(lastRect, tweenType);

        if (EditorGUI.EndChangeCheck())
        {
            var fixedComType = GetFixedComponentType(target.objectReferenceValue as Component, (DOTweenSequence.DOTweenType)tweenType.enumValueIndex);
            if (fixedComType != null)
            {
                target.objectReferenceValue = fixedComType;
            }
        }

        if (target.objectReferenceValue != null && null == GetFixedComponentType(target.objectReferenceValue as Component, (DOTweenSequence.DOTweenType)tweenType.enumValueIndex))
        {
            lastRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.HelpBox(lastRect,
                $"{(target.objectReferenceValue == null ? "Target" : target.objectReferenceValue.GetType().Name)}不支持{tweenType.enumDisplayNames[tweenType.enumValueIndex]}", MessageType.Error);
        }
        const float itemWidth = 110;
        const float setBtnWidth = 30;
        //Delay, Snapping
        lastRect.y += EditorGUIUtility.singleLineHeight;
        var horizontalRect = lastRect;
        horizontalRect.width -= setBtnWidth + itemWidth;
        EditorGUI.PropertyField(horizontalRect, delay);
        horizontalRect.x += setBtnWidth + horizontalRect.width;
        horizontalRect.width = itemWidth;
        snapping.boolValue = EditorGUI.ToggleLeft(horizontalRect, "Snapping", snapping.boolValue);

        //From Value
        lastRect.y += EditorGUIUtility.singleLineHeight;
        horizontalRect = lastRect;
        horizontalRect.width -= setBtnWidth + itemWidth;



        //ToTarget
        lastRect.y += EditorGUIUtility.singleLineHeight;
        var toRect = lastRect;
        toRect.width -= setBtnWidth + itemWidth;

        //To Value
        var dotweenTp = (DOTweenSequence.DOTweenType)tweenType.enumValueIndex;
        switch (dotweenTp)
        {
            case DOTweenSequence.DOTweenType.DOMoveX:
            case DOTweenSequence.DOTweenType.DOMoveY:
            case DOTweenSequence.DOTweenType.DOMoveZ:
            case DOTweenSequence.DOTweenType.DOLocalMoveX:
            case DOTweenSequence.DOTweenType.DOLocalMoveY:
            case DOTweenSequence.DOTweenType.DOLocalMoveZ:
            case DOTweenSequence.DOTweenType.DOAnchorPosX:
            case DOTweenSequence.DOTweenType.DOAnchorPosY:
            case DOTweenSequence.DOTweenType.DOAnchorPosZ:
            case DOTweenSequence.DOTweenType.DOFade:
            case DOTweenSequence.DOTweenType.DOCanvasGroupFade:
            case DOTweenSequence.DOTweenType.DOFillAmount:
            case DOTweenSequence.DOTweenType.DOValue:
            case DOTweenSequence.DOTweenType.DOScaleX:
            case DOTweenSequence.DOTweenType.DOScaleY:
            case DOTweenSequence.DOTweenType.DOScaleZ:
            {
                EditorGUI.BeginDisabledGroup(!useFromValue.boolValue);
                var value = fromValue.vector4Value;
                value.x = EditorGUI.FloatField(horizontalRect, "From", value.x);
                fromValue.vector4Value = value;
                EditorGUI.EndDisabledGroup();

                if (!useToTarget.boolValue)
                {
                    value = toValue.vector4Value;
                    value.x = EditorGUI.FloatField(toRect, "To", value.x);
                    toValue.vector4Value = value;
                }
            }
                break;
            case DOTweenSequence.DOTweenType.DOAnchorPos:
            case DOTweenSequence.DOTweenType.DOFlexibleSize:
            case DOTweenSequence.DOTweenType.DOMinSize:
            case DOTweenSequence.DOTweenType.DOPreferredSize:
            case DOTweenSequence.DOTweenType.DOSizeDelta:
            {
                EditorGUI.BeginDisabledGroup(!useFromValue.boolValue);
                fromValue.vector4Value = EditorGUI.Vector2Field(horizontalRect, "From", fromValue.vector4Value);
                EditorGUI.EndDisabledGroup();
                if (!useToTarget.boolValue)
                    toValue.vector4Value = EditorGUI.Vector2Field(toRect, "To", toValue.vector4Value);
            }
                break;
            case DOTweenSequence.DOTweenType.DOMove:
            case DOTweenSequence.DOTweenType.DOLocalMove:
            case DOTweenSequence.DOTweenType.DOAnchorPos3D:
            case DOTweenSequence.DOTweenType.DOScale:
            case DOTweenSequence.DOTweenType.DORotate:
            case DOTweenSequence.DOTweenType.DOLocalRotate:
            {
                EditorGUI.BeginDisabledGroup(!useFromValue.boolValue);
                fromValue.vector4Value = EditorGUI.Vector3Field(horizontalRect, "From", fromValue.vector4Value);
                EditorGUI.EndDisabledGroup();
                if (!useToTarget.boolValue)
                    toValue.vector4Value = EditorGUI.Vector3Field(toRect, "To", toValue.vector4Value);
            }
                break;
            case DOTweenSequence.DOTweenType.DOColor:
            {
                EditorGUI.BeginDisabledGroup(!useFromValue.boolValue);
                fromValue.vector4Value = EditorGUI.ColorField(horizontalRect, "From", fromValue.vector4Value);
                EditorGUI.EndDisabledGroup();
                if (!useToTarget.boolValue)
                    toValue.vector4Value = EditorGUI.ColorField(toRect, "To", toValue.vector4Value);
            }
                break;
        }
        if (useToTarget.boolValue)
        {
            toTarget.objectReferenceValue = EditorGUI.ObjectField(toRect, "To", toTarget.objectReferenceValue, target.objectReferenceValue != null ? target.objectReferenceValue.GetType() : typeof(Component), true);

            if (toTarget.objectReferenceValue == null)
            {
                lastRect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.HelpBox(lastRect, "To target cannot be null.", MessageType.Error);
            }
        }
        horizontalRect.x += horizontalRect.width;
        horizontalRect.width = setBtnWidth;
        if (useFromValue.boolValue && GUI.Button(horizontalRect, "Set"))
        {
            SetValueFromTarget(dotweenTp, target, fromValue);
        }
        horizontalRect.x += setBtnWidth;
        horizontalRect.width = itemWidth;
        useFromValue.boolValue = EditorGUI.ToggleLeft(horizontalRect, "Enable", useFromValue.boolValue);

        toRect.x += toRect.width;
        toRect.width = setBtnWidth;
        if (!useToTarget.boolValue && GUI.Button(toRect, "Set"))
        {
            SetValueFromTarget(dotweenTp, target, toValue);
        }
        toRect.x += setBtnWidth;
        toRect.width = itemWidth;
        useToTarget.boolValue = EditorGUI.ToggleLeft(toRect, "ToTarget", useToTarget.boolValue);

        //Duration
        lastRect.y += EditorGUIUtility.singleLineHeight;
        horizontalRect = lastRect;
        horizontalRect.width -= setBtnWidth + itemWidth;
        EditorGUI.PropertyField(horizontalRect, duration);
        horizontalRect.x += setBtnWidth + horizontalRect.width;
        horizontalRect.width = itemWidth;
        speedBased.boolValue = EditorGUI.ToggleLeft(horizontalRect, "Use Speed", speedBased.boolValue);

        //Ease
        lastRect.y += EditorGUIUtility.singleLineHeight;
        horizontalRect = lastRect;
        horizontalRect.width -= setBtnWidth + itemWidth;
        EditorGUI.PropertyField(horizontalRect, customEase.boolValue ? easeCurve : ease);
        horizontalRect.x += setBtnWidth + horizontalRect.width;
        horizontalRect.width = itemWidth;
        customEase.boolValue = EditorGUI.ToggleLeft(horizontalRect, "Use Curve", customEase.boolValue);

        //Loops
        lastRect.y += EditorGUIUtility.singleLineHeight;
        horizontalRect = lastRect;
        horizontalRect.width -= setBtnWidth + itemWidth;
        EditorGUI.PropertyField(horizontalRect, loops);
        horizontalRect.x += setBtnWidth + horizontalRect.width;
        horizontalRect.width = itemWidth;
        EditorGUI.BeginDisabledGroup(loops.intValue == 1);
        loopType.enumValueIndex = (int)(LoopType)EditorGUI.EnumPopup(horizontalRect, (LoopType)loopType.enumValueIndex);
        EditorGUI.EndDisabledGroup();
        //UpdateType
        lastRect.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(lastRect, updateType);

        //Events
        lastRect.y += EditorGUIUtility.singleLineHeight;
        property.isExpanded = EditorGUI.Foldout(lastRect, property.isExpanded, "Animation Events");
        if (property.isExpanded)
        {
            //OnPlay
            lastRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(lastRect, onPlay);

            //OnUpdate
            lastRect.y += EditorGUI.GetPropertyHeight(onPlay);
            EditorGUI.PropertyField(lastRect, onUpdate);

            //OnComplete
            lastRect.y += EditorGUI.GetPropertyHeight(onUpdate);
            EditorGUI.PropertyField(lastRect, onComplete);
        }

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }

    static void SetValueFromTarget(DOTweenSequence.DOTweenType tweenType, SerializedProperty target, SerializedProperty value)
    {
        if (target.objectReferenceValue == null) return;
        var targetCom = target.objectReferenceValue;
        switch (tweenType)
        {
            case DOTweenSequence.DOTweenType.DOMove:
            {
                value.vector4Value = ((Transform)targetCom).position;
                break;
            }
            case DOTweenSequence.DOTweenType.DOMoveX:
            {
                var tmpValue = value.vector4Value;
                tmpValue.x = ((Transform)targetCom).position.x;
                value.vector4Value = tmpValue;
                break;
            }
            case DOTweenSequence.DOTweenType.DOMoveY:
            {
                var tmpValue = value.vector4Value;
                tmpValue.x = ((Transform)targetCom).position.y;
                value.vector4Value = tmpValue;
                break;
            }
            case DOTweenSequence.DOTweenType.DOMoveZ:
            {
                var tmpValue = value.vector4Value;
                tmpValue.x = ((Transform)targetCom).position.z;
                value.vector4Value = tmpValue;
                break;
            }
            case DOTweenSequence.DOTweenType.DOLocalMove:
            {
                value.vector4Value = ((Transform)targetCom).localPosition;
                break;
            }
            case DOTweenSequence.DOTweenType.DOLocalMoveX:
            {
                var tmpValue = value.vector4Value;
                tmpValue.x = ((Transform)targetCom).localPosition.x;
                value.vector4Value = tmpValue;
                break;
            }
            case DOTweenSequence.DOTweenType.DOLocalMoveY:
            {
                var tmpValue = value.vector4Value;
                tmpValue.x = ((Transform)targetCom).localPosition.y;
                value.vector4Value = tmpValue;
                break;
            }
            case DOTweenSequence.DOTweenType.DOLocalMoveZ:
            {
                var tmpValue = value.vector4Value;
                tmpValue.x = ((Transform)targetCom).localPosition.z;
                value.vector4Value = tmpValue;
                break;
            }
            case DOTweenSequence.DOTweenType.DOAnchorPos:
            {
                value.vector4Value = ((RectTransform)targetCom).anchoredPosition;
                break;
            }
            case DOTweenSequence.DOTweenType.DOAnchorPosX:
            {
                var tmpValue = value.vector4Value;
                tmpValue.x = ((RectTransform)targetCom).anchoredPosition.x;
                value.vector4Value = tmpValue;
                break;
            }
            case DOTweenSequence.DOTweenType.DOAnchorPosY:
            {
                var tmpValue = value.vector4Value;
                tmpValue.x = ((RectTransform)targetCom).anchoredPosition.y;
                value.vector4Value = tmpValue;
                break;
            }
            case DOTweenSequence.DOTweenType.DOAnchorPosZ:
            {
                var tmpValue = value.vector4Value;
                tmpValue.x = ((RectTransform)targetCom).anchoredPosition3D.z;
                value.vector4Value = tmpValue;
                break;
            }
            case DOTweenSequence.DOTweenType.DOAnchorPos3D:
            {
                value.vector4Value = ((RectTransform)targetCom).anchoredPosition3D;
                break;
            }
            case DOTweenSequence.DOTweenType.DOColor:
            {
                value.vector4Value = ((Graphic)targetCom).color;
                break;
            }
            case DOTweenSequence.DOTweenType.DOFade:
            {
                var tmpValue = value.vector4Value;
                tmpValue.x = ((Graphic)targetCom).color.a;
                value.vector4Value = tmpValue;
                break;
            }
            case DOTweenSequence.DOTweenType.DOCanvasGroupFade:
            {
                var tmpValue = value.vector4Value;
                tmpValue.x = ((CanvasGroup)targetCom).alpha;
                value.vector4Value = tmpValue;
                break;
            }
            case DOTweenSequence.DOTweenType.DOValue:
            {
                var tmpValue = value.vector4Value;
                tmpValue.x = ((Slider)targetCom).value;
                value.vector4Value = tmpValue;
                break;
            }
            case DOTweenSequence.DOTweenType.DOSizeDelta:
            {
                value.vector4Value = ((RectTransform)targetCom).sizeDelta;
                break;
            }
            case DOTweenSequence.DOTweenType.DOFillAmount:
            {
                var tmpValue = value.vector4Value;
                tmpValue.x = ((Image)targetCom).fillAmount;
                value.vector4Value = tmpValue;
                break;
            }
            case DOTweenSequence.DOTweenType.DOFlexibleSize:
            {
                value.vector4Value = (targetCom as LayoutElement).GetFlexibleSize();
                break;
            }
            case DOTweenSequence.DOTweenType.DOMinSize:
            {
                value.vector4Value = (targetCom as LayoutElement).GetMinSize();
                break;
            }
            case DOTweenSequence.DOTweenType.DOPreferredSize:
            {
                value.vector4Value = (targetCom as LayoutElement).GetPreferredSize();
                break;
            }
            case DOTweenSequence.DOTweenType.DOScale:
            {
                value.vector4Value = ((Transform)targetCom).localScale;
                break;
            }
            case DOTweenSequence.DOTweenType.DOScaleX:
            {
                var tmpValue = value.vector4Value;
                tmpValue.x = ((Transform)targetCom).localScale.x;
                value.vector4Value = tmpValue;
                break;
            }
            case DOTweenSequence.DOTweenType.DOScaleY:
            {
                var tmpValue = value.vector4Value;
                tmpValue.x = ((Transform)targetCom).localScale.y;
                value.vector4Value = tmpValue;
                break;
            }
            case DOTweenSequence.DOTweenType.DOScaleZ:
            {
                var tmpValue = value.vector4Value;
                tmpValue.x = ((Transform)targetCom).localScale.z;
                value.vector4Value = tmpValue;
                break;
            }
            case DOTweenSequence.DOTweenType.DORotate:
            {
                value.vector4Value = ((Transform)targetCom).eulerAngles;
                break;
            }
            case DOTweenSequence.DOTweenType.DOLocalRotate:
            {
                value.vector4Value = ((Transform)targetCom).localEulerAngles;
                break;
            }
        }
    }

    static Component GetFixedComponentType(Component com, DOTweenSequence.DOTweenType tweenType)
    {
        if (com == null) return null;
        switch (tweenType)
        {
            case DOTweenSequence.DOTweenType.DOMove:
            case DOTweenSequence.DOTweenType.DOMoveX:
            case DOTweenSequence.DOTweenType.DOMoveY:
            case DOTweenSequence.DOTweenType.DOMoveZ:
            case DOTweenSequence.DOTweenType.DOLocalMove:
            case DOTweenSequence.DOTweenType.DOLocalMoveX:
            case DOTweenSequence.DOTweenType.DOLocalMoveY:
            case DOTweenSequence.DOTweenType.DOLocalMoveZ:
            case DOTweenSequence.DOTweenType.DOScale:
            case DOTweenSequence.DOTweenType.DOScaleX:
            case DOTweenSequence.DOTweenType.DOScaleY:
            case DOTweenSequence.DOTweenType.DOScaleZ:
                return com.gameObject.GetComponent<Transform>();
            case DOTweenSequence.DOTweenType.DOAnchorPos:
            case DOTweenSequence.DOTweenType.DOAnchorPosX:
            case DOTweenSequence.DOTweenType.DOAnchorPosY:
            case DOTweenSequence.DOTweenType.DOAnchorPosZ:
            case DOTweenSequence.DOTweenType.DOAnchorPos3D:
            case DOTweenSequence.DOTweenType.DOSizeDelta:
                return com.gameObject.GetComponent<RectTransform>();
            case DOTweenSequence.DOTweenType.DOColor:
            case DOTweenSequence.DOTweenType.DOFade:
                return com.gameObject.GetComponent<Graphic>();
            case DOTweenSequence.DOTweenType.DOCanvasGroupFade:
                return com.gameObject.GetComponent<CanvasGroup>();
            case DOTweenSequence.DOTweenType.DOFillAmount:
                return com.gameObject.GetComponent<Image>();
            case DOTweenSequence.DOTweenType.DOFlexibleSize:
            case DOTweenSequence.DOTweenType.DOMinSize:
            case DOTweenSequence.DOTweenType.DOPreferredSize:
                return com.gameObject.GetComponent<LayoutElement>();
            case DOTweenSequence.DOTweenType.DOValue:
                return com.gameObject.GetComponent<Slider>();

        }
        return null;
    }
}
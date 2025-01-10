using UnityEditor;
using UnityEngine;

namespace Editor
{
#if UNITY_EDITOR
    public static class TransformCopyShortcuts
    {
        private static Vector3 copiedPosition;
        private static Quaternion copiedRotation;
        private static Vector3 copiedScale;
        private static Vector2 copiedAnchorMin;
        private static Vector2 copiedAnchorMax;
        private static Vector2 copiedPivot;
        private static Vector2 copiedAnchoredPosition;
        private static Vector2 copiedSizeDelta;

        [MenuItem("CONTEXT/Transform/Copy Component")]
        private static void CopyTransform(MenuCommand command)
        {
            var transform = (Transform)command.context;
            copiedPosition = transform.position;
            copiedRotation = transform.rotation;
            copiedScale = transform.localScale;
            
            if (transform is RectTransform rectTransform)
            {
                copiedAnchorMin = rectTransform.anchorMin;
                copiedAnchorMax = rectTransform.anchorMax;
                copiedPivot = rectTransform.pivot;
                copiedAnchoredPosition = rectTransform.anchoredPosition;
                copiedSizeDelta = rectTransform.sizeDelta;
            }
        }

        [MenuItem("CONTEXT/Transform/Paste Component")]
        private static void PasteTransform(MenuCommand command)
        {
            var transform = (Transform)command.context;

            Undo.RecordObject(transform, "Paste Transform");
            transform.position = copiedPosition;
            transform.rotation = copiedRotation;
            transform.localScale = copiedScale;
            
            if (transform is RectTransform rectTransform)
            {
                Undo.RecordObject(rectTransform, "Paste RectTransform Properties");
                rectTransform.anchorMin = copiedAnchorMin;
                rectTransform.anchorMax = copiedAnchorMax;
                rectTransform.pivot = copiedPivot;
                rectTransform.anchoredPosition = copiedAnchoredPosition;
                rectTransform.sizeDelta = copiedSizeDelta;
            }
        }
    }
#endif
}
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

        [MenuItem("CONTEXT/Transform/Copy Component")]
        private static void CopyTransform(MenuCommand command)
        {
            var transform = (Transform)command.context;
            copiedPosition = transform.position;
            copiedRotation = transform.rotation;
            copiedScale = transform.localScale;
        }

        [MenuItem("CONTEXT/Transform/Paste Component")]
        private static void PasteTransform(MenuCommand command)
        {
            var transform = (Transform)command.context;

            Undo.RecordObject(transform, "Paste Transform");
            transform.position = copiedPosition;
            transform.rotation = copiedRotation;
            transform.localScale = copiedScale;
        }
    }
#endif
}
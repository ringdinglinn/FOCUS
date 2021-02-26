using UnityEditor;
using UnityEngine;

namespace GreasePencil
{

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.DisabledGroupScope(true))
                base.OnGUI(position, property, label);
        }
    }

    /*
    [CustomPropertyDrawer(typeof(RenderQueueAttribute))]
    public class RenderQueueDrawer : PropertyDrawer
    {
        public static GUIContent SPACE = new GUIContent(" ");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel))
            {
                RenderQueueField(property, position);
            }
        }

        const float kQueuePopupWidth = 100f;
        const float kCustomQueuePopupWidth = kQueuePopupWidth + 15f;

        public static void RenderQueueField(SerializedProperty property, Rect r)
        {
            var mixedValue = property.hasMultipleDifferentValues;
            EditorGUI.showMixedValue = mixedValue;

            int curRawQueue = property.intValue;
            int curDisplayQueue = property.intValue; // this gets final queue value used for rendering, taking shader's queue into account

            // Figure out if we're using one of common queues, or a custom one
            GUIContent[] queueNames = null;
            int[] queueValues = null;
            float labelWidth;
            // If we use queue value that is not available, lets switch to the custom one
            bool useCustomQueue = System.Array.IndexOf(RenderQueueStyles.queueValues, curRawQueue) < 0;
            if (useCustomQueue)
            {
                // It is a big chance that we already have this custom queue value available
                bool updateNewCustomQueueValue = System.Array.IndexOf(RenderQueueStyles.customQueueNames, curRawQueue) < 0;
                if (updateNewCustomQueueValue)
                {
                    int targetQueueIndex = CalculateClosestQueueIndexToValue(curRawQueue);
                    string targetQueueName = RenderQueueStyles.queueNames[targetQueueIndex].text;
                    int targetQueueValueOverflow = curRawQueue - RenderQueueStyles.queueValues[targetQueueIndex];

                    string newQueueName = string.Format(
                        targetQueueValueOverflow > 0 ? "{0}+{1}" : "{0}{1}",
                        targetQueueName,
                        targetQueueValueOverflow);
                    RenderQueueStyles.customQueueNames[RenderQueueStyles.kCustomQueueIndex].text = newQueueName;
                    RenderQueueStyles.customQueueValues[RenderQueueStyles.kCustomQueueIndex] = curRawQueue;
                }

                queueNames = RenderQueueStyles.customQueueNames;
                queueValues = RenderQueueStyles.customQueueValues;
                labelWidth = kCustomQueuePopupWidth;
            }
            else
            {
                queueNames = RenderQueueStyles.queueNames;
                queueValues = RenderQueueStyles.queueValues;
                labelWidth = kQueuePopupWidth;
            }

            // We want the custom queue number field to line up with thumbnails & other value fields
            // (on the right side), and common queues popup to be on the left of that.
            Rect popupRect = r;
            popupRect.width -= 100f;

            Rect numberRect = r;
            numberRect.xMin = numberRect.xMax - 100;

            // Queues popup
            EditorGUIUtility.labelWidth = popupRect.width - 120f;
            int newPopupValue = EditorGUI.IntPopup(popupRect, RenderQueueStyles.queueLabel, curRawQueue, queueNames, queueValues);

            // Custom queue field
            //EditorGUIUtility.fieldWidth = 140f;
            EditorGUIUtility.labelWidth = 10f;
            int newDisplayQueue = EditorGUI.IntField(numberRect, SPACE, curDisplayQueue);

            // If popup or custom field changed, set the new queue
            if (curRawQueue != newPopupValue || curDisplayQueue != newDisplayQueue)
            {
                // Take the value from the number field,
                int newQueue = newDisplayQueue;
                // But if it's the popup that was changed
                if (newPopupValue != curRawQueue)
                    newQueue = newPopupValue;
                newQueue = Mathf.Clamp(newQueue, -1, 5000); // clamp to valid queue ranges, change the material queues
                property.intValue = newQueue;
            }

            EditorGUIUtility.labelWidth = -1;
            EditorGUIUtility.fieldWidth = -1;
            EditorGUI.showMixedValue = false;
        }

        private static int CalculateClosestQueueIndexToValue(int requestedValue)
        {
            int bestCloseByDiff = int.MaxValue;
            int result = 1;
            for (int i = 1; i < RenderQueueStyles.queueValues.Length; i++)
            {
                int queueValue = RenderQueueStyles.queueValues[i];
                int closeByDiff = Mathf.Abs(queueValue - requestedValue);
                if (closeByDiff < bestCloseByDiff)
                {
                    result = i;
                    bestCloseByDiff = closeByDiff;
                }
            }
            return result;
        }

        private static class RenderQueueStyles
        {
            public const int kNewShaderQueueValue = -1;
            public const int kCustomQueueIndex = 4;
            public static readonly GUIContent queueLabel = Styles.Content("Render Queue");
            public static readonly GUIContent[] queueNames =
            {
                Styles.Content("From Shader"),
                Styles.Content("Geometry", "Queue 2000"),
                Styles.Content("AlphaTest", "Queue 2450"),
                Styles.Content("Transparent", "Queue 3000"),
            };
            public static readonly int[] queueValues =
            {
                kNewShaderQueueValue,
                (int)UnityEngine.Rendering.RenderQueue.Geometry,
                (int)UnityEngine.Rendering.RenderQueue.AlphaTest,
                (int)UnityEngine.Rendering.RenderQueue.Transparent,
            };
            public static GUIContent[] customQueueNames =
            {
                queueNames[0],
                queueNames[1],
                queueNames[2],
                queueNames[3],
                RenderQueueStyles.Content(string.Empty), // This name will be overriden during runtime
            };
            public static int[] customQueueValues =
            {
                queueValues[0],
                queueValues[1],
                queueValues[2],
                queueValues[3],
                0, // This value will be overriden during runtime
            };
        }
    }
    */
}
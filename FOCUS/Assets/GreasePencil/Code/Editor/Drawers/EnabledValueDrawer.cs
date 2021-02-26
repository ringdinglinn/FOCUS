/*
MIT License

Copyright (c) 2018 Keith Boshoff (Wahooney)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnabledValue), true)]
public class EnabledValueDrawer : PropertyDrawer
{
    public static GUIContent SPACE = new GUIContent(" ");

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        using (new EditorGUI.PropertyScope(position, label, property))
        {
            var enabledRect = new Rect(position);
            enabledRect.width = EditorGUIUtility.labelWidth + EditorGUIUtility.singleLineHeight;

            var valueRect = new Rect(position);
            valueRect.xMin = enabledRect.xMax;

            EditorGUI.PropertyField(enabledRect, property.FindPropertyRelative("enabled"), label);
            using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel))
            {
                if (property.FindPropertyRelative("enabled").boolValue)
                {
                    EditorGUIUtility.labelWidth = 10f;
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("value"), SPACE);
                    EditorGUIUtility.labelWidth = -1f;
                }
            }
        }
    }
}

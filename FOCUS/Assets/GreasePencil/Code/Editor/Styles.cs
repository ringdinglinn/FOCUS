using UnityEngine;
using UnityEditor;

namespace GreasePencil
{
    public static class Styles
    {
        static GUIStyle _largeLabelStyle;
        public static GUIStyle LargeLabelStyle
        {
            get
            {
                if (_largeLabelStyle == null)
                {
                    _largeLabelStyle = new GUIStyle("Box")
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal = new GUIStyleState() { textColor = Color.white },
                        fontSize = 32
                    };
                }

                return _largeLabelStyle;
            }
        }

        static GUIStyle _labelStyle;
        public static GUIStyle LabelStyle
        {
            get
            {
                if (_labelStyle == null)
                {
                    _labelStyle = new GUIStyle("Box")
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal = new GUIStyleState() { textColor = Color.white },
                        fontSize = 20
                    };
                }

                return _labelStyle;
            }
        }

        static GUIStyle _infoStyle;
        public static GUIStyle InfoStyle
        {
            get
            {
                if (_infoStyle == null)
                {
                    _infoStyle = new GUIStyle("Box")
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal = new GUIStyleState() { textColor = Color.white },
                        richText = true,
                        fontSize = 12
                    };
                }

                return _infoStyle;
            }
        }

        static GUIStyle _footerStyle;
        public static GUIStyle FooterStyle
        {
            get
            {
                if (_footerStyle == null)
                {
                    _footerStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleRight,
                        richText = true,
                        fontStyle = FontStyle.Italic,

                        //normal = new GUIStyleState() { textColor = Color.white },
                        //fontSize = 12
                    };
                }

                return _footerStyle;
            }
        }

        static GUIStyle _add;
        public static GUIStyle Add
        {
            get
            {
                if (_add == null)
                {
                    _add = new GUIStyle("OL Plus");
                }

                return _add;
            }
        }

        static GUIStyle _remove;
        public static GUIStyle Remove
        {
            get
            {
                if (_remove == null)
                {
                    _remove = new GUIStyle("OL Plus");
                }

                return _remove;
            }
        }

        public static GUIContent Content(string title)
        {
            return new GUIContent(title);
        }
    }
}
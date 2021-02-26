using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GreasePencil
{
    [System.Serializable]
    public class LayerModifier
    {
        [SerializeField]
        bool _enabled = true;

        [System.Serializable]
        public class SerializedModifier : ISerializationCallbackReceiver
        {
            [SerializeField]
            string _modifierType;

            [SerializeField]
            string _modifierName;

            [SerializeField]
            string _modifierJson;

            [SerializeField]
            GreasePencilModifier _modifier;
            public GreasePencilModifier modifier { get { return _modifier; } internal set { _modifier = value; } }

            public void Validate()
            {
                if (_modifier == null)
                {
                    _modifier = (GreasePencilModifier)ScriptableObject.CreateInstance(_modifierType);
                    _modifier.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave | HideFlags.DontUnloadUnusedAsset;
                    _modifier.name = _modifierName;

                    JsonUtility.FromJsonOverwrite(_modifierJson, _modifier);
                }

                _modifier.onValidate -= OnValidate;
                _modifier.onValidate += OnValidate;
            }

            public void OnAfterDeserialize()
            {
                if (_modifier != null)
                {
                    _modifier.onValidate -= OnValidate;
                    _modifier.onValidate += OnValidate;
                }

#if UNITY_EDITOR
                if (_modifier == null)
                    UnityEditor.EditorApplication.delayCall += Validate;
#endif
            }

            void OnValidate()
            {
                if (_modifier != null)
                {
                    var json = JsonUtility.ToJson(_modifier);
                    if (json != _modifierJson)
                    {
                        _modifierJson = json;
                        GreasePencilModifier.SignalUpdate();
                    }
                }
            }

            public void OnBeforeSerialize()
            {
                if (_modifier != null)
                {
                    _modifierType = _modifier.GetType().Name;
                    if (string.IsNullOrEmpty(_modifierName))
                        _modifierName = _modifier.GetType().Name;
                    _modifierJson = JsonUtility.ToJson(_modifier);
                }
            }
        }

        [SerializeField]
        SerializedModifier _modifier;
        public GreasePencilModifier modifier { get { return _modifier.modifier; } }

        public void ModifyStroke(Layer layer, List<Stroke> modifiedStrokes)
        {
            if ((_modifier != null) && _enabled)
            {
                _modifier.Validate();
                modifier.ModifyStroke(layer, modifiedStrokes);
                modifiedStrokes.RemoveAll(m => m.IsEmpty);
            }
        }
    }

    public interface IInlineModifier { }

    public abstract class GreasePencilModifier : ScriptableObject
    {
        public static event UnityAction onModifierUpdated;
        public static void SignalUpdate()
        {
            if (onModifierUpdated != null)
                onModifierUpdated();
        }

        static List<GreasePencilModifier> _allModifiers = new List<GreasePencilModifier>();

        public abstract void ModifyStroke(Layer layer, List<Stroke> modifiedStrokes);

        public event UnityAction onValidate;

#if  UNITY_EDITOR
        void OnValidate()
        {
            if (!_allModifiers.Contains(this))
                _allModifiers.Add(this);

            GreasePencilCanvas.UpdateWithModifier(this);

            if (onValidate != null)
                onValidate();
        }
#endif
    }
}
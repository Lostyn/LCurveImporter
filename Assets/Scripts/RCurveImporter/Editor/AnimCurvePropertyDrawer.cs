using UnityEditor;
using UnityEngine;

namespace fr.lostyn.RCurve
{
    [CustomPropertyDrawer( typeof( AnimationCurve ), false )]
    public class AnimCurvePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
            property.serializedObject.Update();

            using( new EditorGUILayout.HorizontalScope() ) {
                using( var sck = new EditorGUI.ChangeCheckScope() ) {
                    EditorGUILayout.LabelField( ObjectNames.NicifyVariableName( property.name ), GUILayout.Width( EditorGUIUtility.labelWidth ) );
                    var newValue = EditorGUILayout.CurveField( property.animationCurveValue );

                    if( EditorGUILayout.DropdownButton( new GUIContent( "" ), FocusType.Passive ) ) {
                        RCurvePickerWindow.OpenWidow( property, result => {
                            property.animationCurveValue = new AnimationCurve( result.curve.keys );
                            property.animationCurveValue.preWrapMode = result.curve.preWrapMode;
                            property.animationCurveValue.postWrapMode = result.curve.postWrapMode;

                            property.serializedObject.ApplyModifiedProperties();
                            EditorUtility.SetDirty( property.serializedObject.targetObject );
                        } );
                    }

                   if( sck.changed ) {
                        property.animationCurveValue = newValue;
                   }
                }
            }

            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
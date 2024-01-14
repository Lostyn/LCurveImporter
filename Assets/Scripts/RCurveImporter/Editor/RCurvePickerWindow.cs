using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace fr.lostyn.RCurve
{
    public delegate void RCurveSelectHandler( RCurve result );
    public class RCurvePickerWindow : EditorWindow
    {
        SerializedProperty _prop;
        RCurveSelectHandler _handler;
        string _filter;
        List<RCurve> _candidates;
        Vector2 scrollPos;

        public static RCurvePickerWindow OpenWidow( SerializedProperty curveProp, RCurveSelectHandler handler ) {
            RCurvePickerWindow windowInstance = ScriptableObject.CreateInstance<RCurvePickerWindow>();
            windowInstance.Init( curveProp, handler );
            windowInstance.titleContent = new GUIContent( "RCurve Picker" );
            windowInstance.Show();

            return windowInstance;
        }

        void Init( SerializedProperty curveProp, RCurveSelectHandler handler ) {
            _prop = curveProp;
            _handler = handler;
            _filter = PlayerPrefs.GetString( "RcurvePicker_filter", "" ); ;
            _candidates = new List<RCurve>();

            string[] guids = AssetDatabase.FindAssets( $"t:{typeof( RCurve )}" );
            for( int i = 0; i < guids.Length; i++ ) {
                string assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );
                _candidates.Add( AssetDatabase.LoadAssetAtPath<RCurve>( assetPath ) );
            }
        }

        private void OnGUI() {
            if(_prop == null) {
                Close();
                return;
            }

            
            OnGUIProperty();
            OnGUIPropertyOptions();
            GUILayout.Space( EditorGUIUtility.singleLineHeight );
            OnGUISearch();
        }

        private void OnGUIProperty() {
            _prop.serializedObject.Update();
            EditorGUILayout.LabelField( ObjectNames.NicifyVariableName( _prop.name ) );
            _prop.animationCurveValue = EditorGUILayout.CurveField( _prop.animationCurveValue, GUILayout.Height(50f) ) ;
            _prop.serializedObject.ApplyModifiedProperties();
        }

        private void OnGUIPropertyOptions() {
            using( new EditorGUILayout.HorizontalScope() ) {
                GUILayout.FlexibleSpace();
                if( GUILayout.Button( "Export Curve")) {
                    string savePath = EditorUtility.SaveFilePanelInProject("Export curve", "AnimationCurve", "rcurve", "Export");
                    if( !string.IsNullOrEmpty( savePath ) ) {
                        ExportCurve( _prop, savePath );
                    }
                }
            }
        }

        private void ExportCurve( SerializedProperty prop, string savePath ) {
            AnimationCurve curve = prop.animationCurveValue;
            string txtData = string.Join("\n", curve.keys.Select( frame => $"{frame.time} {frame.value}" ) );
            File.WriteAllText( savePath, txtData );
            AssetDatabase.ImportAsset( savePath );
            Close();
        }

        private void OnGUISearch() {

            RCurve result = null;

            string cacheFilter = _filter;
            using( new EditorGUILayout.HorizontalScope() ) {
                EditorGUILayout.LabelField( "Filtrer", GUILayout.Width( 50 ) );
                _filter = GUILayout.TextField( _filter );
            }
            if( _filter != cacheFilter )
                PlayerPrefs.SetString( "RcurvePicker_filter", _filter );

            List<RCurve> filteredCandidates = _candidates.FindAll( obj => obj.name.Contains( _filter ) );

            scrollPos = EditorGUILayout.BeginScrollView( scrollPos, GUILayout.Width( Screen.width ), GUILayout.Height( Screen.height - 150 ) );
            for( int i = 0; i < filteredCandidates.Count; i++ ) {
                using( new EditorGUILayout.HorizontalScope() ) {
                    if( GUILayout.Button( filteredCandidates[i].name, GUILayout.Width( EditorGUIUtility.labelWidth ) ) ) {
                        result = filteredCandidates[i];
                    }

                    GUI.enabled = false;
                    EditorGUILayout.CurveField( filteredCandidates[i].curve );
                    GUI.enabled = true;
                }
            }
            EditorGUILayout.EndScrollView();

            if( result != null && _handler != null ) {
                _handler( result );
                _handler = null;
                Close();
            }
        }
    }
}

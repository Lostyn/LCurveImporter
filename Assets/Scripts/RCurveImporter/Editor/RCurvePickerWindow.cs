using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace fr.lostyn.RCurve
{
    public delegate void RCurveSelectHandler( RCurve result );
    public class RCurvePickerWindow : EditorWindow
    {
        RCurveSelectHandler _handler;
        string _filter;
        List<RCurve> _candidates;
        Vector2 scrollPos;

        public static RCurvePickerWindow OpenWidow( RCurveSelectHandler handler ) {
            RCurvePickerWindow windowInstance = ScriptableObject.CreateInstance<RCurvePickerWindow>();
            windowInstance.Init( handler );
            windowInstance.titleContent = new GUIContent( "RCurve Picker" );
            windowInstance.Show();

            return windowInstance;
        }

        void Init( RCurveSelectHandler handler ) {
            _handler = handler;
            _filter = PlayerPrefs.GetString( "RcurvePicker_filter", "" ); ;
            _candidates = new List<RCurve> ();

            string[] guids = AssetDatabase.FindAssets( $"t:{typeof( RCurve )}" );
            for( int i = 0; i < guids.Length; i++ ) {
                string assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );
                _candidates.Add( AssetDatabase.LoadAssetAtPath<RCurve>( assetPath ) );
            }
        }

        private void OnGUI() {
            RCurve result = null;

            string cacheFilter = _filter;
            using( new EditorGUILayout.HorizontalScope() ) {
                EditorGUILayout.LabelField( "Filtrer", GUILayout.Width(50) );
                _filter = GUILayout.TextField( _filter );
            }
            if( _filter != cacheFilter )
                PlayerPrefs.SetString( "RcurvePicker_filter", _filter );

            List<RCurve> filteredCandidates = _candidates.FindAll( obj => obj.name.Contains( _filter ) );

            scrollPos = EditorGUILayout.BeginScrollView( scrollPos, GUILayout.Width( Screen.width ), GUILayout.Height( Screen.height ) );
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

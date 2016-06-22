using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEditor;

namespace Ulfred
{
    /// <summary>
    /// .
    /// </summary>
    public class UlfredEditorWindow : EditorWindow
    {
        private string search = "";

        private bool isFirstUpdate = true;

        private List<string> findAssetPaths = new List<string>();

        private GUISkin skin;

        [MenuItem("Window/Ulfred &u")]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<UlfredEditorWindow>(true, "Ulfred", true);
            window.skin = AssetDatabase.LoadAssetAtPath<GUISkin>( "Assets/Editor/Ulfred/UlfredGUISkin.guiskin" );
            window.isFirstUpdate = true;
            window.minSize = new Vector2( 765.0f, 847.0f );
            window.position = new Rect( (Screen.currentResolution.width - 765.0f) / 2, (Screen.currentResolution.height - 847.0f) / 2, 765.0f, 847.0f);
            Debug.Log( Screen.width );
        }

        void Update()
        {
            this.Repaint();
            //Debug.Log( this.position.ToString() );
        }

        void OnGUI()
        {
            this.DrawSearchTextField();
            int imax = this.findAssetPaths.Count > 10 ? 10 : this.findAssetPaths.Count;
            var iconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize( Vector2.one * this.skin.GetStyle( "fileLabel" ).fontSize );

            for( int i = 0; i < imax; i++ )
            {
                var path = AssetDatabase.GUIDToAssetPath( this.findAssetPaths[i] );
                var fileName = path.Substring( path.LastIndexOf("/") + 1 );
                var obj = AssetDatabase.LoadAssetAtPath( path, typeof( Object ) );
                GUILayout.BeginVertical( this.skin.GetStyle( "elementBackground" ) );
                EditorGUILayout.LabelField( GetGUIContent( obj ), this.skin.GetStyle( "fileLabel" ) );
                EditorGUILayout.LabelField( path, this.skin.GetStyle( "pathLabel" ) );
                GUILayout.EndVertical();
            }
            EditorGUIUtility.SetIconSize( iconSize );
            if( Event.current.keyCode == KeyCode.Return )
            {
                this.Close();
            }
        }

        private void DrawSearchTextField()
        {
            GUI.SetNextControlName( "SearchTextField" );
            EditorGUI.BeginChangeCheck();
            this.search = EditorGUILayout.TextField( this.search, this.skin.textField );
            if( EditorGUI.EndChangeCheck() )
            {
                this.findAssetPaths = new List<string>( AssetDatabase.FindAssets( this.search ) );
                Debug.Log("fileNumber = " + this.findAssetPaths.Count );
            }

            if( this.isFirstUpdate )
            {
                this.isFirstUpdate = false;
                EditorGUI.FocusTextInControl( "SearchTextField" );
            }
        }

        private GUIContent GetGUIContent( UnityEngine.Object obj )
        {
            if( obj == null )
            {
                return new GUIContent();
            }

            var content = new GUIContent( EditorGUIUtility.ObjectContent( obj, obj.GetType() ) );
            content.tooltip = AssetDatabase.GetAssetPath( obj );

            return content;
        }
    }
}

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

		private Vector2 scrollPosition = Vector2.zero;

        private int index = 0;

		public static Data CurrentData
		{
			get
			{
				if(data == null)
				{
					LoadData();
				}

				return data;
			}
		}
		private static Data data = null;

		private const string FileDirectory = "Ulfred";

		private const string FileName = "/data.dat";

        private const string SearchTextFieldControlName = "SearchTextField";

        [MenuItem("Window/Ulfred &u")]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<UlfredEditorWindow>(true, "Ulfred", true);
            window.skin = AssetDatabase.LoadAssetAtPath<GUISkin>( "Assets/Editor/Ulfred/UlfredGUISkin.guiskin" );
            window.isFirstUpdate = true;
			window.minSize = CurrentData.size;
			window.position = new Rect( (Screen.currentResolution.width - window.minSize.x) / 2, (Screen.currentResolution.height - window.minSize.y) / 2, window.minSize.x, window.minSize.y);
			Debug.Log( Screen.currentResolution.height );
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

			this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            for( int i = 0; i < imax; i++ )
            {
                var path = AssetDatabase.GUIDToAssetPath( this.findAssetPaths[i] );
                var obj = AssetDatabase.LoadAssetAtPath( path, typeof( Object ) );
				EditorGUILayout.BeginVertical( this.skin.GetStyle( "elementBackground" ) );
				EditorGUILayout.LabelField( GetGUIContent( obj ), this.skin.GetStyle( "fileLabel" ) );
                EditorGUILayout.LabelField( path, this.skin.GetStyle( "pathLabel" ) );
				EditorGUILayout.EndVertical();
            }
			EditorGUILayout.EndScrollView();

            EditorGUIUtility.SetIconSize( iconSize );
//            if( Event.current.keyCode == KeyCode.Return )
//            {
//                this.Close();
//            }
        }

        private void DrawSearchTextField()
        {
            GUI.SetNextControlName( SearchTextFieldControlName );
            EditorGUI.BeginChangeCheck();
			this.search = EditorGUILayout.TextField( this.search, this.skin.GetStyle("searchTextField") );
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

            if( GUI.GetNameOfFocusedControl() == SearchTextFieldControlName )
            {
                if( Event.current.keyCode == KeyCode.DownArrow )
                {
                    this.index++;
                    Debug.Log("index = " + this.index);
                }
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
			
		private static void LoadData()
		{
			if( data != null )
			{
				return;
			}

			var loadObject = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget( FileDirectory + FileName );
			if( loadObject.Length > 0 )
			{
				data = loadObject[0] as Data;
			}
			else
			{
				data = ScriptableObject.CreateInstance<Data>();
			}
		}

        private GUIStyle GetStyle( bool isActive, string activeName, string inactiveName )
        {
            return this.skin.GetStyle( isActive ? activeName : inactiveName );
        }
    }
}

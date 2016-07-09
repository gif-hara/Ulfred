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
				if( data == null )
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

		[MenuItem( "Window/Ulfred &u" )]
		public static void ShowWindow()
		{
			var window = EditorWindow.GetWindow<UlfredEditorWindow>( true, "Ulfred", true );
			window.skin = AssetDatabase.LoadAssetAtPath<GUISkin>( "Assets/Editor/Ulfred/UlfredGUISkin.guiskin" );
			window.isFirstUpdate = true;
			window.minSize = CurrentData.size;
			window.position = new Rect( ( Screen.currentResolution.width - window.minSize.x ) / 2, ( Screen.currentResolution.height - window.minSize.y ) / 2, window.minSize.x, window.minSize.y );
			Debug.Log( Screen.currentResolution.height );
		}

		void Update()
		{
			this.Repaint();
		}

		void OnGUI()
		{
			this.InputEvent();
			this.DrawSearchTextField();
			this.DrawSearchResult();
		}

		private void InputEvent()
		{
			if( this.GetKeyDown( KeyCode.DownArrow ) )
			{
				this.index++;
				var max = this.findAssetPaths.Count - 1;
				max = max > CurrentData.searchCount - 1 ? CurrentData.searchCount - 1 : max;
				this.index = this.index > max ? max : this.index;
			}
			if( this.GetKeyDown( KeyCode.UpArrow ) )
			{
				this.index--;
				this.index = this.index < 0 ? 0 : this.index;
			}
			if( this.GetKeyDown( KeyCode.Return ) )
			{
				if( this.findAssetPaths.Count > 0 )
				{
					Selection.activeObject = AssetDatabase.LoadAssetAtPath( AssetDatabase.GUIDToAssetPath( this.findAssetPaths[this.index] ), typeof( Object ) ) as Object;
					this.Close();
				}

			}
		}

		private void DrawSearchTextField()
		{

			GUI.SetNextControlName( SearchTextFieldControlName );
			EditorGUI.BeginChangeCheck();
			this.search = EditorGUILayout.TextField( this.search, this.skin.GetStyle( "searchTextField" ) );
			if( EditorGUI.EndChangeCheck() )
			{
				this.findAssetPaths = new List<string>( AssetDatabase.FindAssets( this.search ) );
			}

			if( this.isFirstUpdate )
			{
				this.isFirstUpdate = false;
				EditorGUI.FocusTextInControl( "SearchTextField" );
			}
		}

		private void DrawSearchResult()
		{
			int imax = this.findAssetPaths.Count > CurrentData.searchCount ? CurrentData.searchCount : this.findAssetPaths.Count;
			var iconSize = EditorGUIUtility.GetIconSize();
			EditorGUIUtility.SetIconSize( Vector2.zero );

			this.scrollPosition = EditorGUILayout.BeginScrollView( this.scrollPosition );
			for( int i = 0; i < imax; i++ )
			{
				var isActive = i == this.index;
				var path = AssetDatabase.GUIDToAssetPath( this.findAssetPaths[i] );
				var obj = AssetDatabase.LoadAssetAtPath( path, typeof( Object ) );
				EditorGUILayout.BeginVertical( this.GetStyle( isActive, "elementBackgroundActive", "elementBackgroundDeactive" ) );
				EditorGUILayout.LabelField( GetGUIContent( obj ), this.GetStyle( isActive, "fileLabelActive", "fileLabelDeactive" ) );
				EditorGUILayout.LabelField( path, this.GetStyle( isActive, "pathLabelActive", "pathLabelDeactive" ) );
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndScrollView();
			EditorGUIUtility.SetIconSize( iconSize );
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

		private bool GetKeyDown( KeyCode keyCode )
		{
			if( Event.current.type != EventType.KeyDown )
			{
				return false;
			}

			return Event.current.keyCode == keyCode;
		}
	}
}

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

		private List<string> findAssetGuids = new List<string>();

		private GUISkin skin;

		private Vector2 scrollPosition = Vector2.zero;

		private int index = 0;

		private Rect selectedRect;

		private const float FindAssetGUIHeight = 82.0f;

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
				var max = this.findAssetGuids.Count - 1;
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
				if( this.findAssetGuids.Count > 0 )
				{
					this.Close();
					var selectObject = AssetDatabase.LoadAssetAtPath( AssetDatabase.GUIDToAssetPath( this.findAssetGuids[this.index] ), typeof( Object ) ) as Object;
					if( Event.current.command )
					{
						AssetDatabase.OpenAsset( selectObject );
					}
					else
					{
						Selection.activeObject = selectObject;
					}
				}
			}
			if( this.GetKeyDown( KeyCode.LeftArrow ) )
			{
				Debug.Log( "selectedRect = " + this.selectedRect );
			}
		}

		private void DrawSearchTextField()
		{
			GUI.SetNextControlName( SearchTextFieldControlName );
			EditorGUI.BeginChangeCheck();
			this.search = EditorGUI.TextField( this.SearchTextFieldRect, this.search, this.SearchTextFieldStyle );
			if( EditorGUI.EndChangeCheck() )
			{
				SearchAssets();
			}

			if( this.isFirstUpdate )
			{
				this.isFirstUpdate = false;
				EditorGUI.FocusTextInControl( "SearchTextField" );
			}
		}

		private void DrawSearchResult()
		{
			int imax = this.findAssetGuids.Count > CurrentData.searchCount ? CurrentData.searchCount : this.findAssetGuids.Count;
			var iconSize = EditorGUIUtility.GetIconSize();
			EditorGUIUtility.SetIconSize( this.IconSize );

			this.scrollPosition = GUI.BeginScrollView( this.FindAssetViewRect, this.scrollPosition, this.FindAssetTableRect );
			for( int i = 0; i < imax; i++ )
			{
				var isActive = i == this.index;
				var path = AssetDatabase.GUIDToAssetPath( this.findAssetGuids[i] );
				var obj = AssetDatabase.LoadAssetAtPath( path, typeof( Object ) );

				var rect = this.GetFindAssetRect( i );
				GUI.Box( rect, "", this.GetStyle( isActive, "elementBackgroundActive", "elementBackgroundDeactive" ) );

				var guiStyle = this.GetStyle( isActive, "fileLabelActive", "fileLabelDeactive" );
				rect = new Rect( rect.x, rect.y + 8, rect.width, guiStyle.CalcHeight( GUIContent.none, rect.width ) );
				GUI.Label( rect, GetGUIContent( obj ), guiStyle );

				guiStyle = this.GetStyle( isActive, "pathLabelActive", "pathLabelDeactive" );
				rect = new Rect( rect.x, rect.y + rect.height + 8, rect.width, guiStyle.CalcHeight( GUIContent.none, rect.width ) );
				GUI.Label( rect, path, guiStyle );
			}
			GUI.EndScrollView();
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

		private void SearchAssets()
		{
			if( string.IsNullOrEmpty( this.search ) )
			{
				this.findAssetGuids = new List<string>();
				return;
			}

			this.findAssetGuids = new List<string>( AssetDatabase.FindAssets( this.search ) );
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

		private GUIStyle SearchTextFieldStyle
		{
			get
			{
				return this.skin.GetStyle( "searchTextField" );
			}
		}

		private bool GetKeyDown( KeyCode keyCode )
		{
			if( Event.current.type != EventType.KeyDown )
			{
				return false;
			}

			return Event.current.keyCode == keyCode;
		}

		private Vector2 IconSize
		{
			get
			{
				return Vector2.one * this.skin.GetStyle( "fileLabelActive" ).fontSize;
			}
		}

		private Rect SearchTextFieldRect
		{
			get
			{
				var style = this.SearchTextFieldStyle;
				return new Rect( Vector2.zero, new Vector2( this.position.width, style.CalcHeight( GUIContent.none, this.position.width ) ) );
			}
		}

		private Rect FindAssetViewRect
		{
			get
			{
				var searchTextFieldRect = this.SearchTextFieldRect;
				return new Rect( 0.0f, searchTextFieldRect.height, this.position.width, this.position.height - searchTextFieldRect.height );
			}
		}

		private Rect FindAssetTableRect
		{
			get
			{
				var findAssetViewRect = this.FindAssetViewRect;
				var heightCount = this.findAssetGuids.Count;
				heightCount = heightCount > CurrentData.searchCount ? CurrentData.searchCount : heightCount;
				return new Rect( 0.0f, findAssetViewRect.y, 0.0f, heightCount * FindAssetGUIHeight );
			}
		}

		private Rect GetFindAssetRect( int index )
		{
			var searchTextFieldRect = this.SearchTextFieldRect;
			return new Rect(
				searchTextFieldRect.x,
				searchTextFieldRect.height + ( index * FindAssetGUIHeight ),
				searchTextFieldRect.width,
				80
			);
		}

		private Rect AddRect( Rect rect, int addY )
		{
			return new Rect( rect.x, rect.y + addY, rect.width, rect.height );
		}

		private Rect NextRect( Rect rect, int offsetY )
		{
			return new Rect( rect.x, rect.y + rect.height + offsetY, rect.width, rect.height );
		}
	}
}

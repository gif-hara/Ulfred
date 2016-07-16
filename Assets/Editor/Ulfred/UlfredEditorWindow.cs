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

		private int listIndex = 0;

		private int selectIndex = 0;

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
			window.minSize = window.WindowSize;
			window.maxSize = window.WindowSize;
			window.position = new Rect( ( Screen.currentResolution.width - window.minSize.x ) / 2, ( Screen.currentResolution.height - window.minSize.y ) / 2, window.minSize.x, window.minSize.y );
		}

		void Update()
		{
			//this.Repaint();
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
				if( this.selectIndex >= CurrentData.searchCount - 1 )
				{
					this.listIndex++;
					var max = this.ListIndexMax;
					this.listIndex = this.listIndex > max ? max : this.listIndex;
				}
				else
				{
					this.selectIndex++;
					var max = CurrentData.searchCount - 1;
					max = max > this.findAssetGuids.Count - 1 ? this.findAssetGuids.Count - 1 : max;
					this.selectIndex = this.selectIndex > max ? max : this.selectIndex;
				}

				Debug.Log( "this.listIndex = " + this.listIndex + " this.findAssetGuids.Count = " + this.findAssetGuids.Count );
			}
			if( this.GetKeyDown( KeyCode.UpArrow ) )
			{
				if( this.selectIndex <= 0 )
				{
					this.listIndex--;
				}
				else
				{
					this.selectIndex--;
				}
					
				this.listIndex = this.listIndex < 0 ? 0 : this.listIndex;
			}
			if( this.GetKeyDown( KeyCode.Return ) )
			{
				if( this.findAssetGuids.Count > 0 )
				{
					this.Close();
					var selectObject = AssetDatabase.LoadAssetAtPath( AssetDatabase.GUIDToAssetPath( this.findAssetGuids[this.listIndex] ), typeof( Object ) ) as Object;
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
			if( Event.current.character > 0 || Event.current.keyCode == KeyCode.Backspace || Event.current.keyCode == KeyCode.Delete )
			{
				this.selectIndex = 0;
				this.listIndex = 0;
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
				EditorGUI.FocusTextInControl( SearchTextFieldControlName );
			}
		}

		private void DrawSearchResult()
		{
			int imax = this.findAssetGuids.Count > CurrentData.searchCount ? CurrentData.searchCount : this.findAssetGuids.Count;
			var iconSize = EditorGUIUtility.GetIconSize();
			EditorGUIUtility.SetIconSize( this.IconSize );

			for( int i = 0; i < imax; i++ )
			{
				var isActive = i == this.selectIndex;
				var path = AssetDatabase.GUIDToAssetPath( this.findAssetGuids[this.listIndex + i] );
				var obj = AssetDatabase.LoadAssetAtPath( path, typeof( Object ) );

				var rect = this.GetFindAssetRect( i, isActive );
				GUI.Box( rect, GUIContent.none, this.ElementBackgroundStyle( isActive ) );

				var guiStyle = this.FileLabelStyle( isActive );
				rect = new Rect( rect.x, rect.y + CurrentData.fileLabelMargin, rect.width, guiStyle.CalcHeight( GUIContent.none, rect.width ) );
				GUI.Label( rect, GetGUIContent( obj ), guiStyle );

				guiStyle = this.PathLabelStyle( isActive );
				rect = new Rect( rect.x, rect.y + rect.height + CurrentData.pathLabelMargin, rect.width, guiStyle.CalcHeight( GUIContent.none, rect.width ) );
				GUI.Label( rect, path, guiStyle );
			}

			var findAssetTableRect = this.FindAssetTableRect;
			var scrollY = findAssetTableRect.height * ((float)this.listIndex / this.findAssetGuids.Count);
			GUI.BeginScrollView( this.FindAssetViewRect, new Vector2(0.0f, scrollY), findAssetTableRect );
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
				return new Rect( 0.0f, findAssetViewRect.y, 0.0f, heightCount * FindAssetGUIHeight );
			}
		}

		private Rect GetFindAssetRect( int index, bool isActive )
		{
			var searchTextFieldRect = this.SearchTextFieldRect;
			var height = 
				this.FileLabelStyle( isActive ).CalcHeight( GUIContent.none, searchTextFieldRect.width )
				+ this.PathLabelStyle( isActive ).CalcHeight( GUIContent.none, SearchTextFieldRect.width )
				+ CurrentData.elementMargin;
				
			return new Rect(
				searchTextFieldRect.x,
				searchTextFieldRect.height + ( index * height ),
				searchTextFieldRect.width,
				height
			);
		}

		private GUIStyle SearchTextFieldStyle
		{
			get
			{
				return this.skin.GetStyle( "searchTextField" );
			}
		}

		private GUIStyle ElementBackgroundStyle( bool isActive )
		{
			return this.GetStyle( isActive, "elementBackgroundActive", "elementBackgroundDeactive" );
		}

		private GUIStyle FileLabelStyle( bool isActive )
		{
			return this.GetStyle( isActive, "fileLabelActive", "fileLabelDeactive" );
		}

		private GUIStyle PathLabelStyle( bool isActive )
		{
			return this.GetStyle( isActive, "pathLabelActive", "pathLabelDeactive" );
		}

		private GUIStyle GetStyle( bool isActive, string activeName, string inactiveName )
		{
			return this.skin.GetStyle( isActive ? activeName : inactiveName );
		}

		private Vector2 WindowSize
		{
			get
			{
				var result = new Vector2();
				var searchTextFieldRect = this.SearchTextFieldRect;
				result.x = Screen.currentResolution.width / 2;
				result.y += searchTextFieldRect.height;
				var elementHeight = this.FileLabelStyle( true ).CalcHeight( GUIContent.none, searchTextFieldRect.width )
				                    + this.PathLabelStyle( true ).CalcHeight( GUIContent.none, SearchTextFieldRect.width )
				                    + CurrentData.elementMargin;
				result.y += elementHeight * 5;

				return result;
			}
		}

		private int ListIndexMax
		{
			get
			{
				var result = ( this.findAssetGuids.Count - 1 ) - CurrentData.searchCount;
				result = result < 0 ? 0 : result;

				return result;
			}
		}
	}
}

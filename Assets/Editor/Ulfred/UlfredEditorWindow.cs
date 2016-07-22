//#define CheckPerformance_Search
#define UpdateRepaint
//#define OnLostFocusClose

using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Linq;
using CallbackFunction = UnityEditor.EditorApplication.CallbackFunction;

namespace Ulfred
{
	public class UlfredEditorWindow : EditorWindow
	{
		private string search = "";

		private List<AssetAccessCount> findAssetAccessCounts = new List<AssetAccessCount>();

		private List<CommandAccessCount> findCommandAccessCounts = new List<CommandAccessCount>();

		public GUISkin Skin
		{
			get
			{
				if( this.skin == null )
				{
					this.skin = AssetDatabase.LoadAssetAtPath<GUISkin>( "Assets/Editor/Ulfred/UlfredGUISkin.guiskin" );
				}

				return this.skin;
			}
		}

		private GUISkin skin;

		private int listIndex = 0;

		private int selectIndex = 0;

		private GUIStyle logoStyle = null;

		private int searchTextFieldKeyboardControl = -1;

		private int hiddenSearchTextFieldKeyboardControl = -1;

		private static Dictionary<string, Command> commands;

		private const string SearchTextFieldControlName = "SearchTextField";

		private const string HiddenSearchTextFieldControlName = "HiddenSearchTextField";

		[MenuItem( "Window/Ulfred" )]
		public static void ShowWindow()
		{
			var window = EditorWindow.GetWindow<UlfredEditorWindow>( true, "Ulfred", true );
			window.minSize = window.WindowSize;
			window.maxSize = window.WindowSize;
			window.position = new Rect( ( Screen.currentResolution.width - window.minSize.x ) / 2, ( Screen.currentResolution.height - window.minSize.y ) / 2, window.minSize.x, window.minSize.y );
			window.InitializeLogoStyle();
		}

#if UpdateRepaint
		void Update()
		{
			this.Repaint();
		}
#endif

		void OnLostFocus()
		{
#if OnLostFocusClose
			this.Close();
#endif
		}

		void OnGUI()
		{
			this.InputEvent();
			this.DrawOptionButton();
			this.DrawUlfredLogo();
			this.DrawSearchTextField();
			if( this.IsCommandMode )
			{
				this.DrawSearchResultCommands();
			}
			else
			{
				this.DrawSearchResultAssets();
			}
			this.DrawOptionImage();
		}

		[InitializeOnLoadMethod()]
		private static void InitializeOnLoad()
		{
			commands = 
				typeof( UlfredCommand ).GetMethods()
					.Where( m => m.IsStatic && System.Attribute.GetCustomAttribute( m, typeof( UlfredCommandMethodAttribute ) ) != null )
					.ToDictionary( m => m.Name.ToLower(), m => new Command( m ) );

			EditorApplicationUtility.globalEventHandler -= ResidentUpdate;
			EditorApplicationUtility.globalEventHandler += ResidentUpdate;
		}

		private static void ResidentUpdate()
		{
			if( Event.current.Equals( Event.KeyboardEvent( Data.Instance.shortCutKeyCode ) ) )
			{
				ShowWindow();
			}
		}

		private void InitializeLogoStyle()
		{
			this.logoStyle = new GUIStyle( EditorStyles.boldLabel );
			this.logoStyle.alignment = TextAnchor.LowerRight;
			this.logoStyle.normal.textColor = new Color( 0.5f, 0.5f, 0.5f, 0.5f );
			this.logoStyle.fontSize = 128;
		}

		private void InputEvent()
		{
			var currentEvent = Event.current;
			if( this.GetKeyDown( KeyCode.DownArrow, currentEvent ) )
			{
				GUIUtility.keyboardControl = this.hiddenSearchTextFieldKeyboardControl;
				if( this.selectIndex >= Data.Instance.searchCount - 1 )
				{
					this.listIndex++;
					var max = this.ListIndexMax;
					this.listIndex = this.listIndex > max ? max : this.listIndex;
				}
				else
				{
					this.selectIndex++;
					var max = Data.Instance.searchCount - 1;
					max = max > this.SelectIndexMax ? this.SelectIndexMax : max;
					this.selectIndex = this.selectIndex > max ? max : this.selectIndex;
				}
			}
			if( this.GetKeyDown( KeyCode.UpArrow, currentEvent ) )
			{
				GUIUtility.keyboardControl = this.hiddenSearchTextFieldKeyboardControl;
				if( this.selectIndex <= 0 )
				{
					this.listIndex--;
					this.listIndex = this.listIndex < 0 ? 0 : this.listIndex;
				}
				else
				{
					this.selectIndex--;
					this.selectIndex = this.selectIndex < 0 ? 0 : this.selectIndex;
				}
			}
			if( this.GetKeyDown( KeyCode.Return, currentEvent ) )
			{
				if( this.IsCommandMode )
				{
					if( this.findCommandAccessCounts.Count > 0 )
					{
						this.Close();
						this.findCommandAccessCounts[this.listIndex + this.selectIndex].command.methodInfo.Invoke( null, null );
					}
				}
				if( this.findAssetAccessCounts.Count > 0 )
				{
					this.Close();
					var guid = this.findAssetAccessCounts[this.listIndex + this.selectIndex].guid;
					var selectObject = AssetDatabase.LoadAssetAtPath( AssetDatabase.GUIDToAssetPath( guid ), typeof( Object ) ) as Object;
					Data.Instance.AddAccessCount( guid );
					Data.Save();
					if( Event.current.command || Event.current.control )
					{
						AssetDatabase.OpenAsset( selectObject );
					}
					else
					{
						Selection.activeObject = selectObject;
					}
				}
			}
			if( this.GetKeyDown( KeyCode.Escape, currentEvent ) )
			{
				this.Close();
			}
			if( currentEvent.character > 0 || currentEvent.keyCode == KeyCode.Backspace || currentEvent.keyCode == KeyCode.Delete )
			{
				//GUIUtility.keyboardControl = this.searchTextFieldKeyboardControl;
				this.selectIndex = 0;
				this.listIndex = 0;
			}
		}

		private void DrawUlfredLogo()
		{
			EditorGUI.LabelField( new Rect( 0, 0, this.position.width, this.position.height ), "Ulfred", this.logoStyle );
		}

		private void DrawSearchTextField()
		{
			GUI.SetNextControlName( SearchTextFieldControlName );
			EditorGUI.BeginChangeCheck();
			var _search = EditorGUI.TextField( this.SearchTextFieldRect, this.search, this.SearchTextFieldStyle );
			if( EditorGUI.EndChangeCheck() )
			{
				if( _search.IndexOf( " " ) == 0 && !this.IsCommandMode )
				{
					this.search = ">";
				}
				else
				{
					this.search = _search;
				}

				if( this.IsCommandMode )
				{
					this.SearchCommands();
				}
				else
				{
					this.SearchAssets();
				}
			}
			EditorGUI.LabelField( this.SearchTextFieldRect, this.search, this.Skin.GetStyle( "searchLabel" ) );

			GUI.SetNextControlName( HiddenSearchTextFieldControlName );
			EditorGUI.TextField( new Rect( -100, -100, 0, 0 ), "" );
			if( this.hiddenSearchTextFieldKeyboardControl == -1 )
			{
				EditorGUI.FocusTextInControl( HiddenSearchTextFieldControlName );
				this.hiddenSearchTextFieldKeyboardControl = GUIUtility.keyboardControl;
			}

			if( this.searchTextFieldKeyboardControl == -1 )
			{
				EditorGUI.FocusTextInControl( SearchTextFieldControlName );
				this.searchTextFieldKeyboardControl = GUIUtility.keyboardControl;
			}
		}

		private void DrawSearchResultAssets()
		{
			int imax = this.findAssetAccessCounts.Count > Data.Instance.searchCount ? Data.Instance.searchCount : this.findAssetAccessCounts.Count;
			var iconSize = EditorGUIUtility.GetIconSize();
			EditorGUIUtility.SetIconSize( this.IconSize );

			for( int i = 0; i < imax; i++ )
			{
				var path = AssetDatabase.GUIDToAssetPath( this.findAssetAccessCounts[this.listIndex + i].guid );
				var obj = AssetDatabase.LoadAssetAtPath( path, typeof( Object ) );
				this.DrawElement( i, i == this.selectIndex, this.GetGUIContent( obj ), new GUIContent( path ) );
			}

			this.DrawScrollbar();
			EditorGUIUtility.SetIconSize( iconSize );
		}

		private void DrawSearchResultCommands()
		{
			int imax = this.findCommandAccessCounts.Count > Data.Instance.searchCount ? Data.Instance.searchCount : this.findCommandAccessCounts.Count;
			for( int i = 0; i < imax; i++ )
			{
				var commandAccessCount = this.findCommandAccessCounts[this.listIndex + i];
				this.DrawElement( i, i == this.selectIndex, new GUIContent( commandAccessCount.command.methodInfo.Name, this.Skin.GetStyle( "option" ).normal.background ), new GUIContent( commandAccessCount.command.attribute.description ) );
			}
			this.DrawScrollbar();
		}

		private void DrawOptionButton()
		{
			if( GUI.Button( this.OptionButtonRect, "", this.Skin.GetStyle( "option" ) ) )
			{
				UlfredOptionWindow.ShowWindow();
				this.Close();
			}
		}

		private void DrawOptionImage()
		{
			// 検索フォームの手前にボタンを描画するとボタンが反応しないためダミーでテクスチャーを表示させる.
			GUI.DrawTexture( this.OptionButtonRect, this.Skin.GetStyle( "option" ).normal.background );
		}

		private void DrawElement( int index, bool isActive, GUIContent fileLabel, GUIContent pathLabel )
		{
			var rect = this.GetFindAssetRect( index, isActive );
			GUI.Box( rect, GUIContent.none, this.ElementBackgroundStyle( isActive ) );

			var guiStyle = this.FileLabelStyle( isActive );
			rect = new Rect( rect.x, rect.y + Data.Instance.fileLabelMargin, rect.width, guiStyle.CalcHeight( GUIContent.none, rect.width ) );
			GUI.Label( rect, fileLabel, guiStyle );

			guiStyle = this.PathLabelStyle( isActive );
			rect = new Rect( rect.x, rect.y + rect.height + Data.Instance.pathLabelMargin, rect.width, guiStyle.CalcHeight( GUIContent.none, rect.width ) );
			GUI.Label( rect, pathLabel, guiStyle );
		}

		private void DrawScrollbar()
		{
			var findAssetTableRect = this.FindAssetTableRect;
			var scrollY = findAssetTableRect.height * ( (float)this.listIndex / ( this.findAssetAccessCounts.Count - 1 ) );
			GUI.BeginScrollView( this.FindAssetViewRect, new Vector2( 0.0f, scrollY ), findAssetTableRect );
			GUI.EndScrollView();
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
				this.findAssetAccessCounts = new List<AssetAccessCount>();
				return;
			}

#if CheckPerformance_Search
			var s = new System.Diagnostics.Stopwatch();
			s.Start();
#endif
			var guids = AssetDatabase.FindAssets( this.search );
			var notAccessGuids = new List<string>();
			this.findAssetAccessCounts = new List<AssetAccessCount>();
			for( var i = 0; i < guids.Length; i++ )
			{
				var accessCount = Data.Instance.assetAccessCounts.Find( a => a.guid == guids[i] );
				if( accessCount != null )
				{
					this.findAssetAccessCounts.Add( accessCount );
				}
				else
				{
					notAccessGuids.Add( guids[i] );
				}
			}
			this.findAssetAccessCounts.Sort( ( a, b ) => b.count - a.count );
			for( var i = 0; i < notAccessGuids.Count; i++ )
			{
				this.findAssetAccessCounts.Add( new AssetAccessCount( notAccessGuids[i], 0 ) );
			}

#if CheckPerformance_Search
			s.Stop();
			Debug.Log( "Search " + s.ElapsedMilliseconds + "ms" );
#endif
		}

		private void SearchCommands()
		{
#if CheckPerformance_Search
			var s = new System.Diagnostics.Stopwatch();
			s.Start();
#endif
			var fixedSearch = this.search.Substring( this.search.IndexOf( ">" ) + 1 );
			this.findCommandAccessCounts = commands.Where( c => c.Key.IndexOf( fixedSearch ) >= 0 ).Select( c => new CommandAccessCount( c.Value, 0 ) ).ToList();
#if CheckPerformance_Search
			s.Stop();
			Debug.Log( "Search " + s.ElapsedMilliseconds + "ms" );
#endif
		}


		private bool GetKeyDown( KeyCode keyCode, Event currentEvent )
		{
			if( currentEvent.type != EventType.KeyDown )
			{
				return false;
			}

			return currentEvent.keyCode == keyCode;
		}

		private bool IsCommandMode
		{
			get
			{
				return this.search.IndexOf( ">" ) == 0;
			}
		}

		private Vector2 IconSize
		{
			get
			{
				return Vector2.one * this.Skin.GetStyle( "fileLabelActive" ).fontSize;
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
				var heightCount = this.findAssetAccessCounts.Count - 1;
				return new Rect( 0.0f, findAssetViewRect.y, 0.0f, heightCount * GetFindAssetHeight( true ) );
			}
		}

		private Rect GetFindAssetRect( int index, bool isActive )
		{
			var searchTextFieldRect = this.SearchTextFieldRect;
			var height = this.GetFindAssetHeight( isActive );
				
			return new Rect(
				searchTextFieldRect.x,
				searchTextFieldRect.height + ( index * height ),
				searchTextFieldRect.width,
				height
			);
		}

		private float GetFindAssetHeight( bool isActive )
		{
			var searchTextFieldRect = this.SearchTextFieldRect;
			return this.FileLabelStyle( isActive ).CalcHeight( GUIContent.none, searchTextFieldRect.width )
			+ this.PathLabelStyle( isActive ).CalcHeight( GUIContent.none, searchTextFieldRect.width )
			+ Data.Instance.elementMargin;
		}

		private Rect OptionButtonRect
		{
			get
			{
				return new Rect( this.position.width - 19, 4, 16, 16 );
			}
		}

		private GUIStyle SearchTextFieldStyle
		{
			get
			{
				return this.Skin.GetStyle( "searchTextField" );
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
			return this.Skin.GetStyle( isActive ? activeName : inactiveName );
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
				                    + Data.Instance.elementMargin;
				result.y += elementHeight * Data.Instance.searchCount;

				return result;
			}
		}

		private int ListIndexMax
		{
			get
			{
				
				var result = this.IsCommandMode
					? ( this.findCommandAccessCounts.Count - 1 ) - Data.Instance.searchCount
					: ( this.findAssetAccessCounts.Count - 1 ) - Data.Instance.searchCount;
				result = result < 0 ? 0 : result;

				return result;
			}
		}

		private int SelectIndexMax
		{
			get
			{
				var result = this.IsCommandMode
					? ( this.findCommandAccessCounts.Count - 1 )
					: ( this.findAssetAccessCounts.Count - 1 );
				result = result < 0 ? 0 : result;

				return result;
			}
		}
	}

	[InitializeOnLoad]
	class EditorApplicationUtility
	{
		static BindingFlags flags =
			BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic;

		static FieldInfo info = typeof( EditorApplication )
			.GetField( "globalEventHandler", flags );

		public static CallbackFunction globalEventHandler
		{
			get
			{
				return  (CallbackFunction)info.GetValue( null );
			}
			set
			{
				CallbackFunction functions = (CallbackFunction)info.GetValue( null );
				functions += value;
				info.SetValue( null, (object)functions );
			}
		}
	}
}

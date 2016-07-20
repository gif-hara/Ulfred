using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

namespace IconWindow
{
	/// <summary>
	/// .
	/// </summary>
	public class IconsWindow : EditorWindow
	{
		private List<UnityEngine.Object> _objects;

		private Vector2 scrollPosition = new Vector2();

		private const string FileDirectory = "IconWindow";

		private const string FileName = "/data.dat";

		[MenuItem( "Window/IconWindow" )] 
		public static void ShowWindow()
		{
			EditorWindow.GetWindow<IconsWindow>();
		}

		void OnGUI()
		{
			if( _objects == null )
			{
				_objects = new List<UnityEngine.Object>( Resources.FindObjectsOfTypeAll( typeof( Texture2D ) ) );
				_objects.Sort( ( pA, pB ) => System.String.Compare( pA.name, pB.name, System.StringComparison.OrdinalIgnoreCase ) );
				Save();
			}
			this.scrollPosition = EditorGUILayout.BeginScrollView( this.scrollPosition );
			foreach( var o in _objects )
			{
				EditorGUILayout.LabelField( new GUIContent( o.name, o as Texture2D ) );
			}
			EditorGUILayout.EndScrollView();
		}

		private void Save()
		{
			Directory.CreateDirectory( FileDirectory );
			File.Delete( FileDirectory + FileName );
			UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget( new UnityEngine.Object[]{ new Data( _objects ) }, FileDirectory + FileName, true );
		}

	}
}

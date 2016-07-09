using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEditor;

namespace IconWindow
{
	/// <summary>
	/// .
	/// </summary>
	public class IconsWindow : EditorWindow
	{
		private List<UnityEngine.Object> _objects;

		private Vector2 scrollPosition = new Vector2();

		[MenuItem( "Window/IconWindow" )] 
		public static void ShowWindow()
		{
			var w = EditorWindow.GetWindow<IconsWindow>();
		}

		void OnGUI()
		{
			if( _objects == null )
			{
				_objects = new List<UnityEngine.Object>( Resources.FindObjectsOfTypeAll( typeof( Texture2D ) ) );
				_objects.Sort( ( pA, pB ) => System.String.Compare( pA.name, pB.name, System.StringComparison.OrdinalIgnoreCase ) );
				//EditorUtility.
			}
			this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
			foreach(var o in _objects)
			{
				EditorGUILayout.ObjectField(o, typeof(Texture2D));
			}
			EditorGUILayout.EndScrollView();
		}

	}
}

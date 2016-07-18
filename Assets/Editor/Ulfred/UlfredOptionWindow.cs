using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEditor;

namespace Ulfred
{
	public class UlfredOptionWindow : EditorWindow
	{
		public static void ShowWindow()
		{
			var window = EditorWindow.GetWindow<UlfredOptionWindow>( true, "Ulfred Option", true );
			window.minSize = new Vector2( 640, 480 );
			window.maxSize = window.minSize;
			window.position = new Rect( ( Screen.currentResolution.width - window.minSize.x ) / 2, ( Screen.currentResolution.height - window.minSize.y ) / 2, 640, 480 );
		}

		void OnGUI()
		{
		}
			
	}
}
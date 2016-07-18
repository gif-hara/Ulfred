using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEditor;

namespace Ulfred
{
	public class UlfredOptionWindow : EditorWindow
	{
		private Data editData;

		private bool editShortCutKey = false;

		[MenuItem( "Window/Ulfred Option" )]
		public static void ShowWindow()
		{
			var window = EditorWindow.GetWindow<UlfredOptionWindow>( true, "Ulfred Option", true );
			window.minSize = new Vector2( 320, 240 );
			window.maxSize = window.minSize;
			window.position = new Rect( ( Screen.currentResolution.width - window.minSize.x ) / 2, ( Screen.currentResolution.height - window.minSize.y ) / 2, window.minSize.x, window.minSize.y );
			window.editData = Data.Instance.Clone();
		}

		void OnGUI()
		{
			this.InputEvent();
			this.DrawSearchCount();
			this.DrawShortCutKey();
			this.DrawSystemButton();
		}

		private void InputEvent()
		{
			if( this.editShortCutKey && Event.current.type == EventType.KeyDown && Event.current.character > 0 )
			{
				var result = "";
				string s = new string( new char[]{ Event.current.character } );
				result += s.ToLower();
				this.editData.shortCutKeyCode = result;
				this.editShortCutKey = false;
				this.Repaint();
			}
		}

		private void DrawSearchCount()
		{
			this.editData.searchCount = EditorGUILayout.IntSlider( "SearchListNumber", this.editData.searchCount, 1, 5 );
		}

		private void DrawShortCutKey()
		{
			EditorGUILayout.PrefixLabel("Shortcut Key");
			GUILayout.BeginHorizontal();
			if( GUILayout.Button( "Bind ShortCut Key", GUILayout.Width( this.position.width / 2 ) ) )
			{
				this.editShortCutKey = true;
			}
			if( this.editShortCutKey )
			{
				GUILayout.Label( "Press any key" );
			}
			else
			{
				GUILayout.Label( "Key = " + this.editData.shortCutKeyCode );
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.Width( this.position.width / 2 ));
			this.DrawShortCutKeyModifierToggle("&", "alt");
			this.DrawShortCutKeyModifierToggle("%", "ctrl");
			this.DrawShortCutKeyModifierToggle("#", "shift");
			GUILayout.EndHorizontal();
		}

		private void DrawShortCutKeyModifierToggle(string modifierCode, string label)
		{
			var isModifier = this.editData.shortCutKeyCode.IndexOf( modifierCode ) >= 0;
			var newModifier = GUILayout.Toggle( isModifier, label );
			if( isModifier != newModifier )
			{
				if( newModifier )
				{
					this.editData.shortCutKeyCode = modifierCode + this.editData.shortCutKeyCode;
				}
				else
				{
					var splitString = this.editData.shortCutKeyCode.Split( new string[] { modifierCode }, System.StringSplitOptions.RemoveEmptyEntries );
					var fixedCode = "";
					System.Array.ForEach(splitString, s => fixedCode += s);
					this.editData.shortCutKeyCode = fixedCode;
				}
			}
		}

		private void DrawSystemButton()
		{
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			if( GUILayout.Button( "Save" ) )
			{
				Data.Instance.Copy( this.editData );
				Data.Save();
				this.Close();
			}
			if( GUILayout.Button( "Cancel" ) )
			{
				this.Close();
			}
			GUILayout.EndHorizontal();
		}
	}
}
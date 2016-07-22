using UnityEngine;
using UnityEditor;

namespace Ulfred
{
	public class UlfredCommand
	{
		[UlfredCommandMethodAttribute("ほげ的な関数を実行します。")]
		public static void Hoge()
		{
		}

		[UlfredCommandMethodAttribute("ふが")]
		public static void Fuga()
		{
			Debug.Log( "UlfredCommand Fuga!" );
		}

		[UlfredCommandMethodAttribute("")]
		public static void FocusProjector()
		{
			
		}
	}
}
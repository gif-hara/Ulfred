using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

namespace Ulfred
{
	public class UlfredCommand
	{
		[UlfredCommandMethod("ほげ的な関数を実行します。")]
		public static void Hoge()
		{
			Debug.Log( "UlfredCommand Hoge!" );
		}

		[UlfredCommandMethod("ふが")]
		public static void Fuga()
		{
			Debug.Log( "UlfredCommand Fuga!" );
		}
	}
}
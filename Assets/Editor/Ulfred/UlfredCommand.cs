using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

namespace Ulfred
{
	public class UlfredCommand
	{
		public static void Hoge()
		{
			Debug.Log( "UlfredCommand Hoge!" );
		}

		public void Fuga()
		{
			Debug.Log( "UlfredCommand Fuga!" );
		}
	}
}
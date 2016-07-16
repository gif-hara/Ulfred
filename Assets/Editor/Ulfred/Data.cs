using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

namespace Ulfred
{
	/// <summary>
	/// .
	/// </summary>
	[System.Serializable]
	public class Data : ScriptableObject
	{
		public Vector2 size = new Vector2( Screen.currentResolution.width / 2, Screen.currentResolution.height / 2 );

		public int searchCount = 5;

		public int fileLabelMargin = 16;

		public int pathLabelMargin = 8;

		public int elementMargin = 40;
	}
}
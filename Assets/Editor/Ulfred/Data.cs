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
		public int searchCount = 5;

		public int fileLabelMargin = 16;

		public int pathLabelMargin = 8;

		public int elementMargin = 40;

		public List<AccessCount> accessCounts = new List<AccessCount>();

		public void AddAccessCount( string guid )
		{
			var accessCount = this.accessCounts.Find( a => a.guid == guid );
			if( accessCount == null )
			{
				accessCount = new AccessCount( guid, 0 );
				this.accessCounts.Add( accessCount );
			}
			accessCount.count++;
		}
	}

	[System.Serializable]
	public class AccessCount
	{
		public string guid;

		public int count;

		public AccessCount( string guid, int count )
		{
			this.guid = guid;
			this.count = count;
		}
	}
}
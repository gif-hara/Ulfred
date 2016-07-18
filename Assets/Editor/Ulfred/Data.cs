using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.IO;

namespace Ulfred
{
	/// <summary>
	/// .
	/// </summary>
	[System.Serializable]
	public class Data : ScriptableObject
	{
		public static Data Instance
		{
			get
			{
				if( instance == null )
				{
					LoadData();
				}

				return instance;
			}
		}
		private static Data instance = null;

		public int searchCount = 5;

		public string shortCutKeyCode = "&u";

		public int fileLabelMargin = 16;

		public int pathLabelMargin = 8;

		public int elementMargin = 40;

		public List<AccessCount> accessCounts = new List<AccessCount>();

		private const string FileDirectory = "Ulfred";

		private const string FileName = "/data.dat";

		public void Copy(Data other)
		{
			this.searchCount = other.searchCount;
			this.shortCutKeyCode = other.shortCutKeyCode;
		}

		public Data Clone()
		{
			var result = ScriptableObject.CreateInstance<Data>();
			result.Copy(this);

			return result;
		}

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

		private static void LoadData()
		{
			if( instance != null )
			{
				return;
			}

			var loadObject = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget( FileDirectory + FileName );
			if( loadObject.Length > 0 )
			{
				instance = loadObject[0] as Data;
			}
			else
			{
				instance = ScriptableObject.CreateInstance<Data>();
				Save();
			}
		}

		public static void Save()
		{
			Directory.CreateDirectory( FileDirectory );
			File.Delete( FileDirectory + FileName );
			UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget( new UnityEngine.Object[]{ Instance }, FileDirectory + FileName, true );
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
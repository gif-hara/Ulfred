using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;

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

		public bool isAlt = true;

		public bool isControl = false;

		public bool isShift = false;

		public int fileLabelMargin = 16;

		public int pathLabelMargin = 8;

		public int elementMargin = 40;

		public List<AssetAccessCount> assetAccessCounts = new List<AssetAccessCount>();

		public List<CommandAccessCount> commandAccessCounts = new List<CommandAccessCount>();

		private const string FileDirectory = "Ulfred";

		private const string FileName = "/data.dat";

		public void Copy( Data other )
		{
			this.searchCount = other.searchCount;
			this.shortCutKeyCode = other.shortCutKeyCode;
			this.isAlt = other.isAlt;
			this.isControl = other.isControl;
			this.isShift = other.isShift;
		}

		public Data Clone()
		{
			var result = ScriptableObject.CreateInstance<Data>();
			result.Copy( this );

			return result;
		}

		public void AddAccessCount( string guid )
		{
			var accessCount = this.assetAccessCounts.Find( a => a.guid == guid );
			if( accessCount == null )
			{
				accessCount = new AssetAccessCount( guid, 0 );
				this.assetAccessCounts.Add( accessCount );
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
	public class AssetAccessCount
	{
		public string guid;

		public int count;

		public AssetAccessCount( string guid, int count )
		{
			this.guid = guid;
			this.count = count;
		}
	}

	[System.Serializable]
	public class CommandAccessCount
	{
		public Command command;

		public int count;

		public CommandAccessCount( Command command, int count )
		{
			this.command = command;
			this.count = count;
		}
	}

	[System.Serializable]
	public class Command
	{
		public MethodInfo methodInfo;

		public UlfredCommandMethodAttribute attribute;

		public Command( MethodInfo methodInfo )
		{
			this.methodInfo = methodInfo;
			this.attribute = Attribute.GetCustomAttribute( this.methodInfo, typeof( UlfredCommandMethodAttribute ) ) as UlfredCommandMethodAttribute;
		}
	}
}
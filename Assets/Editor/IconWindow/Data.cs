using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

namespace IconWindow
{
	/// <summary>
	/// .
	/// </summary>
	[System.Serializable]
	public class Data : ScriptableObject
	{
		public List<Element> elements = new List<Element>();

		public Data( List<UnityEngine.Object> objects )
		{
			objects.ForEach( o => elements.Add( new Element( o ) ) );
		}
	}

	[System.Serializable]
	public class Element
	{
		public string name;

		public UnityEngine.Object obj;

		public Element( UnityEngine.Object obj )
		{
			this.name = obj.name;
			this.obj = obj;
		}
	}
}
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;

namespace Ulfred
{
	[AttributeUsage(AttributeTargets.Method)][System.Serializable]
	public class UlfredCommandMethodAttribute : Attribute
	{
		public string description;

		public UlfredCommandMethodAttribute(string description)
		{
			this.description = description;
		}
	}
}
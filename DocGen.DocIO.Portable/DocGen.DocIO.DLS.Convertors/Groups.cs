using System.Collections.Generic;

namespace DocGen.DocIO.DLS.Convertors;

internal class Groups
{
	private List<Groups> childElements;

	internal List<Groups> ChildElements
	{
		get
		{
			if (childElements == null)
			{
				childElements = new List<Groups>();
			}
			return childElements;
		}
		set
		{
			childElements = value;
		}
	}
}

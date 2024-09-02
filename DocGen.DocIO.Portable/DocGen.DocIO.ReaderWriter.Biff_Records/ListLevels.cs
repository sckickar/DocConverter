using System.Collections.Generic;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

internal class ListLevels : List<object>
{
	internal new ListLevel this[int index]
	{
		get
		{
			return base[index] as ListLevel;
		}
		set
		{
			base[index] = value;
		}
	}
}

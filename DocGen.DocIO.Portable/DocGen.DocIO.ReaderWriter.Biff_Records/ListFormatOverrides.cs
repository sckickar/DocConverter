using System.Collections.Generic;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

internal class ListFormatOverrides : List<ListFormatOverride>
{
	internal new ListFormatOverride this[int index]
	{
		get
		{
			return base[index];
		}
		set
		{
			base[index] = value;
		}
	}
}

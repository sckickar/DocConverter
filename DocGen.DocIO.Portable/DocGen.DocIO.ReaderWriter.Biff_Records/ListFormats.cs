using System;
using System.Collections.Generic;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

internal class ListFormats : List<ListData>
{
	internal new ListData this[int index]
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

	internal ListData GetListFromId(int id)
	{
		for (int i = 0; i < base.Count; i++)
		{
			if (this[i].ListID == id)
			{
				return this[i];
			}
		}
		return null;
	}

	internal ListData FindListData(int listId)
	{
		return GetListFromId(listId) ?? throw new ArgumentException("List data with the specified id could not be found.");
	}
}

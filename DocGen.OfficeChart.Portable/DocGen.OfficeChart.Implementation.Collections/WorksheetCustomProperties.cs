using System;
using System.Collections;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class WorksheetCustomProperties : TypedSortedListEx<string, ICustomProperty>, IWorksheetCustomProperties
{
	public ICustomProperty this[int index] => GetByIndex(index);

	public new ICustomProperty this[string strName] => GetByName(strName);

	public WorksheetCustomProperties()
	{
	}

	public WorksheetCustomProperties(IList m_arrRecords, int iCustomPropertyPos)
	{
		if (m_arrRecords == null)
		{
			throw new ArgumentNullException("m_arrRecords");
		}
		int count = m_arrRecords.Count;
		if (iCustomPropertyPos < 0 || iCustomPropertyPos >= count)
		{
			throw new ArgumentOutOfRangeException("iCustomPropertyPos");
		}
		while (iCustomPropertyPos < count && m_arrRecords[iCustomPropertyPos] is CustomPropertyRecord property)
		{
			Add(property);
			iCustomPropertyPos++;
		}
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			(GetByIndex(i) as WorksheetCustomProperty).Serialize(records);
		}
	}

	public ICustomProperty Add(string strName)
	{
		WorksheetCustomProperty property = new WorksheetCustomProperty(strName);
		return Add(property);
	}

	public ICustomProperty Add(ICustomProperty property)
	{
		Add(property.Name, property);
		return property;
	}

	[CLSCompliant(false)]
	public void Add(CustomPropertyRecord property)
	{
		if (property == null)
		{
			throw new ArgumentNullException("property");
		}
		WorksheetCustomProperty property2 = new WorksheetCustomProperty(property);
		Add(property2);
	}
}

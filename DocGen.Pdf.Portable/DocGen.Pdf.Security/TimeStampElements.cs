using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal class TimeStampElements
{
	private Dictionary<DerObjectID, List<TimeStampElement>> m_elements;

	internal TimeStampElement this[DerObjectID id]
	{
		get
		{
			if (m_elements.ContainsKey(id))
			{
				object obj = m_elements[id];
				if (obj is IList)
				{
					return (TimeStampElement)((IList)obj)[0];
				}
				return (TimeStampElement)obj;
			}
			return null;
		}
	}

	internal TimeStampElements(Asn1Set values)
	{
		m_elements = new Dictionary<DerObjectID, List<TimeStampElement>>(values.Count);
		for (int i = 0; i != values.Count; i++)
		{
			TimeStampElement timeStampElement = TimeStampElement.GetTimeStampElement(values[i]);
			DerObjectID type = timeStampElement.Type;
			if (m_elements.ContainsKey(type))
			{
				object obj = m_elements[type];
				List<TimeStampElement> list;
				if (obj is TimeStampElement)
				{
					list = new List<TimeStampElement>
					{
						obj as TimeStampElement,
						timeStampElement
					};
				}
				else
				{
					list = (List<TimeStampElement>)obj;
					list.Add(timeStampElement);
				}
				m_elements[type] = list;
			}
			else
			{
				List<TimeStampElement> value = new List<TimeStampElement>(1) { timeStampElement };
				m_elements[type] = value;
			}
		}
	}
}

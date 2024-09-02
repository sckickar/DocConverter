using System;

namespace DocGen.OfficeChart.Implementation.Collections.Grouping;

internal class RichTextStringGroup : CommonObject, IRichTextString, IParentApplication, IOptimizedUpdate
{
	private RangeGroup m_rangeGroup;

	private string m_rtfText;

	public IRichTextString this[int index] => m_rangeGroup[index].RichText;

	public int Count => m_rangeGroup.Count;

	public string Text
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return null;
			}
			string text = this[0].Text;
			for (int i = 1; i < count; i++)
			{
				if (text != this[i].Text)
				{
					return null;
				}
			}
			return text;
		}
		set
		{
			int i = 0;
			for (int count = Count; i < count; i++)
			{
				this[i].Text = value;
			}
		}
	}

	public string RtfText
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return null;
			}
			m_rtfText = this[0].RtfText;
			for (int i = 1; i < count; i++)
			{
				if (m_rtfText != this[i].RtfText)
				{
					return null;
				}
			}
			return m_rtfText;
		}
		set
		{
			m_rtfText = value;
		}
	}

	public bool IsFormatted
	{
		get
		{
			int count = Count;
			if (count == 0)
			{
				return false;
			}
			bool isFormatted = this[0].IsFormatted;
			for (int i = 1; i < count; i++)
			{
				if (isFormatted != this[i].IsFormatted)
				{
					return false;
				}
			}
			return isFormatted;
		}
	}

	public RichTextStringGroup(IApplication application, object parent)
		: base(application, parent)
	{
		FindParents();
	}

	private void FindParents()
	{
		m_rangeGroup = FindParent(typeof(RangeGroup)) as RangeGroup;
		if (m_rangeGroup == null)
		{
			throw new ArgumentOutOfRangeException("parent", "Can't find parent range group.");
		}
	}

	public IOfficeFont GetFont(int iPosition)
	{
		throw new NotImplementedException();
	}

	public void SetFont(int iStartPos, int iEndPos, IOfficeFont font)
	{
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			this[i].SetFont(iStartPos, iEndPos, font);
		}
	}

	public void ClearFormatting()
	{
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			this[i].ClearFormatting();
		}
	}

	public void Append(string text, IOfficeFont font)
	{
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			this[i].Append(text, font);
		}
	}

	public void Clear()
	{
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			this[i].Clear();
		}
	}

	public void BeginUpdate()
	{
	}

	public void EndUpdate()
	{
	}
}

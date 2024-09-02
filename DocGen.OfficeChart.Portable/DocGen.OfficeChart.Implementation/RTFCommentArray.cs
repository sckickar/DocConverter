using System;

namespace DocGen.OfficeChart.Implementation;

internal class RTFCommentArray : CommonObject, IRichTextString, IParentApplication, IOptimizedUpdate
{
	private IRange m_range;

	private string m_rtfText;

	public string Text
	{
		get
		{
			_ = m_range.Cells;
			return null;
		}
		set
		{
			_ = m_range.Cells;
		}
	}

	public string RtfText
	{
		get
		{
			_ = m_range.Cells;
			m_rtfText = null;
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
			_ = m_range.Cells;
			return false;
		}
	}

	public RTFCommentArray(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
	}

	public IOfficeFont GetFont(int iPosition)
	{
		_ = m_range.Cells;
		return null;
	}

	public void SetFont(int iStartPos, int iEndPos, IOfficeFont font)
	{
		_ = m_range.Cells;
	}

	public void ClearFormatting()
	{
		_ = m_range.Cells;
	}

	public void Append(string text, IOfficeFont font)
	{
		_ = m_range.Cells;
	}

	public void Clear()
	{
		_ = m_range.Cells;
	}

	private void SetParents()
	{
		m_range = FindParent(typeof(IRange)) as IRange;
		if (m_range == null)
		{
			throw new ArgumentNullException("Can't find parent range");
		}
	}

	public void BeginUpdate()
	{
	}

	public void EndUpdate()
	{
	}
}

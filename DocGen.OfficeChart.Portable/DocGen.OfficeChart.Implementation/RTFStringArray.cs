using System;

namespace DocGen.OfficeChart.Implementation;

internal class RTFStringArray : IRTFWrapper, IDisposable, IRichTextString, IParentApplication, IOptimizedUpdate
{
	private IRange m_range;

	private string m_rtfText;

	public string Text
	{
		get
		{
			IRange[] cells = m_range.Cells;
			int num = cells.Length;
			if (num == 0)
			{
				return null;
			}
			string text = cells[0].Text;
			if (text != null)
			{
				for (int i = 1; i < num; i++)
				{
					if (text != cells[i].Text)
					{
						text = null;
						break;
					}
				}
			}
			return text;
		}
		set
		{
			IRange[] cells = m_range.Cells;
			int i = 0;
			for (int num = cells.Length; i < num; i++)
			{
				cells[i].RichText.Text = value;
			}
		}
	}

	public string RtfText
	{
		get
		{
			IRange[] cells = m_range.Cells;
			int num = cells.Length;
			if (num == 0)
			{
				return null;
			}
			if (!cells[0].HasRichText)
			{
				return null;
			}
			m_rtfText = cells[0].RichText.RtfText;
			for (int i = 1; i < num; i++)
			{
				if (!cells[i].HasRichText)
				{
					return null;
				}
				if (m_rtfText != cells[i].RichText.RtfText)
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
			IRange[] cells = m_range.Cells;
			int num = cells.Length;
			if (num == 0)
			{
				return false;
			}
			for (int i = 0; i < num; i++)
			{
				if (!cells[i].HasRichText)
				{
					return false;
				}
				if (!cells[i].RichText.IsFormatted)
				{
					return false;
				}
			}
			return true;
		}
	}

	public IApplication Application => (m_range as RangeImpl).Application;

	public object Parent => m_range;

	private RTFStringArray()
	{
	}

	public RTFStringArray(IRange range)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		m_range = range;
	}

	public IOfficeFont GetFont(int iPosition)
	{
		IRange[] cells = m_range.Cells;
		int num = cells.Length;
		if (num == 0)
		{
			return null;
		}
		if (!cells[0].HasRichText)
		{
			return null;
		}
		IOfficeFont font = cells[0].RichText.GetFont(iPosition);
		for (int i = 1; i < num; i++)
		{
			if (!cells[i].HasRichText)
			{
				return null;
			}
			if (font != cells[i].RichText.GetFont(iPosition))
			{
				return null;
			}
		}
		return font;
	}

	public void SetFont(int iStartPos, int iEndPos, IOfficeFont font)
	{
		IRange[] cells = m_range.Cells;
		int i = 0;
		for (int num = cells.Length; i < num; i++)
		{
			cells[i].RichText.SetFont(iStartPos, iEndPos, font);
		}
	}

	public void ClearFormatting()
	{
		IRange[] cells = m_range.Cells;
		int i = 0;
		for (int num = cells.Length; i < num; i++)
		{
			if (cells[i].HasRichText)
			{
				cells[i].RichText.ClearFormatting();
			}
		}
	}

	public void Append(string text, IOfficeFont font)
	{
		IRange[] cells = m_range.Cells;
		int i = 0;
		for (int num = cells.Length; i < num; i++)
		{
			if (cells[i].HasRichText)
			{
				cells[i].RichText.Append(text, font);
			}
		}
	}

	public void BeginUpdate()
	{
	}

	public void EndUpdate()
	{
	}

	public void Dispose()
	{
	}

	public void Clear()
	{
		IRange[] cells = m_range.Cells;
		int i = 0;
		for (int num = cells.Length; i < num; i++)
		{
			if (cells[i].HasRichText)
			{
				((RangeRichTextString)cells[i].RichText).Clear();
			}
		}
	}
}

using System.Collections.Generic;

namespace DocGen.OfficeChart.Implementation;

internal class LineInfoImpl
{
	private List<TextInfoImpl> _textInfoCollection;

	private float m_height;

	internal List<TextInfoImpl> TextInfoCollection
	{
		get
		{
			return _textInfoCollection;
		}
		set
		{
			_textInfoCollection = value;
		}
	}

	internal string Text
	{
		get
		{
			string text = string.Empty;
			foreach (TextInfoImpl item in _textInfoCollection)
			{
				text += item.Text;
			}
			return text;
		}
	}

	internal float Width
	{
		get
		{
			float num = 0f;
			foreach (TextInfoImpl item in _textInfoCollection)
			{
				num += item.Width;
			}
			return num;
		}
	}

	internal float Height
	{
		get
		{
			if (_textInfoCollection.Count > 0)
			{
				m_height = 0f;
				foreach (TextInfoImpl item in _textInfoCollection)
				{
					if (m_height < item.Height)
					{
						m_height = item.Height;
					}
				}
			}
			return m_height;
		}
		set
		{
			m_height = value;
		}
	}

	internal LineInfoImpl()
	{
		_textInfoCollection = new List<TextInfoImpl>();
	}

	internal void Dispose()
	{
		foreach (TextInfoImpl item in _textInfoCollection)
		{
			item.Dispose();
		}
		_textInfoCollection.Clear();
		_textInfoCollection = null;
	}
}

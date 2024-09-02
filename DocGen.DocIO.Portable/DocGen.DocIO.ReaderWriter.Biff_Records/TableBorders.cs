using System;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

[CLSCompliant(false)]
internal class TableBorders
{
	private BorderCode[] m_brcArr = new BorderCode[6];

	internal BorderCode this[int index]
	{
		get
		{
			return m_brcArr[index];
		}
		set
		{
			m_brcArr[index] = value;
		}
	}

	internal BorderCode LeftBorder
	{
		get
		{
			return m_brcArr[1];
		}
		set
		{
			m_brcArr[1] = value;
		}
	}

	internal BorderCode RightBorder
	{
		get
		{
			return m_brcArr[3];
		}
		set
		{
			m_brcArr[3] = value;
		}
	}

	internal BorderCode TopBorder
	{
		get
		{
			return m_brcArr[0];
		}
		set
		{
			m_brcArr[0] = value;
		}
	}

	internal BorderCode BottomBorder
	{
		get
		{
			return m_brcArr[2];
		}
		set
		{
			m_brcArr[2] = value;
		}
	}

	internal BorderCode HorizontalBorder
	{
		get
		{
			return m_brcArr[4];
		}
		set
		{
			m_brcArr[4] = value;
		}
	}

	internal BorderCode VerticalBorder
	{
		get
		{
			return m_brcArr[5];
		}
		set
		{
			m_brcArr[5] = value;
		}
	}

	internal TableBorders()
	{
		Init();
	}

	private void Init()
	{
		for (int i = 0; i < 6; i++)
		{
			m_brcArr[i] = new BorderCode();
		}
	}
}

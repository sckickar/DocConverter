namespace DocGen.PdfViewer.Base;

internal class GraphicObjectData
{
	internal float m_mitterLength;

	private string m_currentFont;

	private float m_fontSize;

	private float m_textLeading;

	private float m_characterSpacing;

	private float m_wordSpacing;

	internal Matrix Ctm;

	internal Matrix textLineMatrix;

	internal Matrix textMatrix;

	internal Matrix documentMatrix;

	internal Matrix textMatrixUpdate;

	internal Matrix drawing2dMatrixCTM;

	internal float HorizontalScaling = 100f;

	internal int Rise;

	internal Matrix transformMatrixTM;

	internal float m_strokingOpacity = 1f;

	internal float m_nonStrokingOpacity = 1f;

	internal string CurrentFont
	{
		get
		{
			return m_currentFont;
		}
		set
		{
			m_currentFont = value;
		}
	}

	internal float FontSize
	{
		get
		{
			return m_fontSize;
		}
		set
		{
			m_fontSize = value;
		}
	}

	internal float TextLeading
	{
		get
		{
			return m_textLeading;
		}
		set
		{
			m_textLeading = value;
		}
	}

	internal float CharacterSpacing
	{
		get
		{
			return m_characterSpacing;
		}
		set
		{
			m_characterSpacing = value;
		}
	}

	internal float WordSpacing
	{
		get
		{
			return m_wordSpacing;
		}
		set
		{
			m_wordSpacing = value;
		}
	}

	internal GraphicObjectData()
	{
	}
}

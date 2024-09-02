using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Parsing;

namespace DocGen.PdfViewer.Base;

internal class GraphicObjectDataHelperNet
{
	internal float m_mitterLength;

	private Color m_strokingColor;

	private Color m_nonStokingColor;

	private Font m_font;

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

	internal DocGen.Drawing.Matrix drawing2dMatrixCTM;

	internal float HorizontalScaling = 100f;

	internal int Rise;

	internal Matrix transformMatrixTM;

	private PdfBrush m_strokingBrush;

	private PdfBrush m_nonStrokingBrush;

	private Colorspace m_strokingColorspace;

	private Colorspace m_nonStrokingColorspace;

	internal float m_strokingOpacity = 1f;

	internal float m_nonStrokingOpacity = 1f;

	internal Colorspace StrokingColorspace
	{
		get
		{
			return m_strokingColorspace;
		}
		set
		{
			m_strokingColorspace = value;
		}
	}

	internal Colorspace NonStrokingColorspace
	{
		get
		{
			return m_nonStrokingColorspace;
		}
		set
		{
			m_nonStrokingColorspace = value;
		}
	}

	internal PdfBrush StrokingBrush
	{
		get
		{
			return m_strokingBrush;
		}
		set
		{
			m_strokingBrush = value;
		}
	}

	internal PdfBrush NonStrokingBrush
	{
		get
		{
			return m_nonStrokingBrush;
		}
		set
		{
			m_nonStrokingBrush = value;
		}
	}

	internal Color StrokingColor
	{
		get
		{
			return m_strokingColor;
		}
		set
		{
			m_strokingColor = value;
		}
	}

	internal Color NonStrokingColor
	{
		get
		{
			return m_nonStokingColor;
		}
		set
		{
			m_nonStokingColor = value;
		}
	}

	internal Font Font
	{
		get
		{
			return m_font;
		}
		set
		{
			m_font = value;
		}
	}

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

	internal GraphicObjectDataHelperNet()
	{
	}
}

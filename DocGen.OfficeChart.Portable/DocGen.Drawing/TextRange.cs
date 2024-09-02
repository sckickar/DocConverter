using DocGen.OfficeChart;
using DocGen.OfficeChart.Implementation;

namespace DocGen.Drawing;

internal class TextRange : ITextRange
{
	private string m_text;

	private TextFrame textFrame;

	private RichTextString m_strText;

	private PreservationLogger preservationLogger;

	public string Text
	{
		get
		{
			return RichText.Text;
		}
		set
		{
			RichText.Text = value;
		}
	}

	public IRichTextString RichText
	{
		get
		{
			if (m_strText == null)
			{
				InitializeVariables();
			}
			return m_strText;
		}
	}

	internal TextRange(TextFrame textFrame, PreservationLogger preservationLogger)
	{
		this.textFrame = textFrame;
		this.preservationLogger = preservationLogger;
	}

	internal TextRange Clone(object parent)
	{
		TextRange textRange = (TextRange)MemberwiseClone();
		textFrame = (TextFrame)parent;
		if (m_strText != null)
		{
			textRange.m_strText = (RichTextString)m_strText.Clone(textRange.m_strText.Parent);
		}
		return textRange;
	}

	protected virtual void InitializeVariables()
	{
		IWorkbook workbook = textFrame.GetWorkbook();
		m_strText = new RichTextString((workbook as WorkbookImpl).Application, workbook, isReadOnly: false, bCreateText: true, preservationLogger);
	}
}

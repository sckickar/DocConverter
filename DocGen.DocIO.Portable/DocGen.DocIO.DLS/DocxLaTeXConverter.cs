using DocGen.Office;

namespace DocGen.DocIO.DLS;

internal class DocxLaTeXConverter : DocumentLaTeXConverter
{
	private WordDocument m_doc;

	internal DocxLaTeXConverter(WordDocument document)
	{
		m_doc = document;
	}

	internal override void CreateMathRunElement(IOfficeMathRunElement officeMathRunElement, string text)
	{
		officeMathRunElement.Item = new WTextRange(m_doc);
		(officeMathRunElement.Item as WTextRange).Text = text;
	}

	internal override void AppendTextInMathRun(IOfficeMathRunElement officeMathRunElement, string text)
	{
		if (officeMathRunElement.Item is WTextRange)
		{
			(officeMathRunElement.Item as WTextRange).Text += text;
		}
	}

	internal override string GetText(IOfficeMathRunElement officeMathRunElement)
	{
		if (officeMathRunElement.Item is WTextRange)
		{
			return (officeMathRunElement.Item as WTextRange).Text;
		}
		return null;
	}

	internal void Close()
	{
	}
}

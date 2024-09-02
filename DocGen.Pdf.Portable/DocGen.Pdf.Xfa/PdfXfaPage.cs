using DocGen.Drawing;

namespace DocGen.Pdf.Xfa;

public class PdfXfaPage
{
	internal int pageId;

	internal bool isAdded;

	internal bool isBreaked;

	internal PdfXfaPageSettings pageSettings = new PdfXfaPageSettings();

	internal void Save(XfaWriter writer)
	{
		writer.Write.WriteStartElement("pageArea");
		writer.Write.WriteAttributeString("name", "Page" + pageId);
		writer.Write.WriteStartElement("contentArea");
		writer.Write.WriteAttributeString("x", pageSettings.Margins.Left + "pt");
		writer.Write.WriteAttributeString("y", pageSettings.Margins.Top + "pt");
		SizeF sizeF = new SizeF(pageSettings.PageSize.Width - (pageSettings.Margins.Left + pageSettings.Margins.Right), pageSettings.PageSize.Height - (pageSettings.Margins.Top + pageSettings.Margins.Bottom));
		if (pageSettings.PageOrientation == PdfXfaPageOrientation.Portrait)
		{
			writer.Write.WriteAttributeString("w", sizeF.Width + "pt");
			writer.Write.WriteAttributeString("h", sizeF.Height + "pt");
		}
		else
		{
			writer.Write.WriteAttributeString("w", sizeF.Height + "pt");
			writer.Write.WriteAttributeString("h", sizeF.Width + "pt");
		}
		writer.Write.WriteEndElement();
		writer.Write.WriteStartElement("medium");
		writer.Write.WriteAttributeString("short", pageSettings.PageSize.Width + "pt");
		writer.Write.WriteAttributeString("long", pageSettings.PageSize.Height + "pt");
		if (pageSettings.PageOrientation != 0)
		{
			writer.Write.WriteAttributeString("orientation", pageSettings.PageOrientation.ToString().ToLower());
		}
		writer.Write.WriteEndElement();
		writer.Write.WriteEndElement();
		isAdded = true;
	}

	public SizeF GetClientSize()
	{
		if (pageSettings.PageOrientation == PdfXfaPageOrientation.Landscape)
		{
			return new SizeF(pageSettings.PageSize.Height - (pageSettings.Margins.Left + pageSettings.Margins.Right), pageSettings.PageSize.Width - (pageSettings.Margins.Top + pageSettings.Margins.Bottom));
		}
		return new SizeF(pageSettings.PageSize.Width - (pageSettings.Margins.Left + pageSettings.Margins.Right), pageSettings.PageSize.Height - (pageSettings.Margins.Top + pageSettings.Margins.Bottom));
	}
}

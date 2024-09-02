using System.IO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.DLS.Rendering;

namespace DocGen.DocIORenderer;

public static class Extensions
{
	public static void AutoFit(this WTable table, AutoFitType autoFitType)
	{
		WordDocument.RenderHelper = new RenderHelper();
		table.Document.FontSettings.EmbedDocumentFonts(table.Document);
		DocumentLayouter.DrawingContext.FontStreams = table.Document.FontSettings.FontStreams;
		table.AutoFitTable(autoFitType);
		RenderHelper.ClearTypeFaceCache(table.Document);
	}

	public static void UpdateTableOfContents(this WordDocument document)
	{
		WordDocument.RenderHelper = new RenderHelper();
		document.FontSettings.EmbedDocumentFonts(document);
		DocumentLayouter.DrawingContext.FontStreams = document.FontSettings.FontStreams;
		document.UpdateTableOfContent();
		RenderHelper.ClearTypeFaceCache(document);
	}

	public static void UpdateWordCount(this WordDocument document, bool performlayout)
	{
		WordDocument.RenderHelper = new RenderHelper();
		document.FontSettings.EmbedDocumentFonts(document);
		DocumentLayouter.DrawingContext.FontStreams = document.FontSettings.FontStreams;
		document.InternalUpdateWordCount(performlayout);
		RenderHelper.ClearTypeFaceCache(document);
	}

	public static void UpdateDocumentFields(this WordDocument document, bool performLayout)
	{
		WordDocument.RenderHelper = new RenderHelper();
		document.FontSettings.EmbedDocumentFonts(document);
		DocumentLayouter.DrawingContext.FontStreams = document.FontSettings.FontStreams;
		document.UpdateDocumentFields(performLayout);
		RenderHelper.ClearTypeFaceCache(document);
	}

	public static Stream SaveAsImage(this WChart chart)
	{
		WordDocument.RenderHelper = new RenderHelper();
		chart.Document.FontSettings.EmbedDocumentFonts(chart.Document);
		DocumentLayouter.DrawingContext.FontStreams = chart.Document.FontSettings.FontStreams;
		Stream result = chart.SaveAsImage();
		RenderHelper.ClearTypeFaceCache(chart.Document);
		return result;
	}

	public static void AutoFit(this IWTable table, AutoFitType autoFitType)
	{
		(table as WTable).AutoFit(autoFitType);
	}

	public static void UpdateTableOfContents(this IWordDocument document)
	{
		(document as WordDocument).UpdateTableOfContents();
	}

	public static void UpdateWordCount(this IWordDocument document, bool performlayout)
	{
		(document as WordDocument).UpdateWordCount(performlayout);
	}

	public static void UpdateDocumentFields(this IWordDocument document, bool performLayout)
	{
		UpdateDocumentFields(document as WordDocument, performLayout);
	}
}

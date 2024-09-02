using DocGen.DocIO.DLS.Rendering;
using DocGen.DocIO.Rendering;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

internal class LayoutFootnoteInfoImpl : FootnoteLayoutInfo
{
	public LayoutFootnoteInfoImpl(WFootnote footnote)
		: base(ChildrenLayoutDirection.Horizontal)
	{
		WParagraph ownerParagraphValue = footnote.GetOwnerParagraphValue();
		DrawingContext drawingContext = DocumentLayouter.DrawingContext;
		int num = ((footnote.FootnoteType != FootnoteType.Endnote) ? (++DocumentLayouter.m_footnoteId) : (++DocumentLayouter.m_endnoteId));
		if (GetBaseEntity(footnote) is WSection wSection)
		{
			if (footnote.FootnoteType == FootnoteType.Footnote)
			{
				if (wSection.PageSetup.RestartIndexForFootnotes == FootnoteRestartIndex.DoNotRestart)
				{
					num += wSection.PageSetup.InitialFootnoteNumber - 1;
				}
				else if (wSection.PageSetup.RestartIndexForFootnotes == FootnoteRestartIndex.RestartForEachPage)
				{
					num = DocumentLayouter.m_footnoteIDRestartEachPage++;
				}
				else if (wSection.PageSetup.RestartIndexForFootnotes == FootnoteRestartIndex.RestartForEachSection)
				{
					num = DocumentLayouter.m_footnoteIDRestartEachSection++;
				}
			}
			else if (wSection.PageSetup.RestartIndexForEndnote == EndnoteRestartIndex.DoNotRestart)
			{
				num += wSection.PageSetup.InitialEndnoteNumber - 1;
			}
			else if (wSection.PageSetup.RestartIndexForEndnote == EndnoteRestartIndex.RestartForEachSection)
			{
				num = DocumentLayouter.m_footnoteIDRestartEachSection++;
			}
		}
		if (footnote.CustomMarkerIsSymbol || footnote.CustomMarker != string.Empty)
		{
			DocumentLayouter.m_footnoteId--;
		}
		base.FootnoteID = GetFootnoteID(footnote, num);
		base.TextRange = footnote.GenerateText(base.FootnoteID);
		FormatBase baseFormat = footnote.GetCharFormat().BaseFormat;
		if (baseFormat != null)
		{
			base.TextRange.CharacterFormat.ApplyBase(baseFormat);
		}
		if (footnote.CustomMarkerIsSymbol && !footnote.MarkerCharacterFormat.HasValue(0) && footnote.SymbolFontName != string.Empty && footnote.SymbolFontName != footnote.MarkerCharacterFormat.FontName)
		{
			base.TextRange.CharacterFormat.FontName = footnote.SymbolFontName;
		}
		base.TextRange.SetOwner(ownerParagraphValue);
		base.Size = drawingContext.MeasureTextRange(base.TextRange, base.FootnoteID);
	}
}

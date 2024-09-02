using System.Collections.Generic;
using DocGen.DocIO.ReaderWriter.Biff_Records.Structures;

namespace DocGen.DocIO.DLS;

internal class CompatibilityOptions
{
	private Dictionary<CompatibilityOption, bool> m_propertiesHash;

	private IWordDocument m_document;

	internal bool this[CompatibilityOption key]
	{
		get
		{
			return GetValue(key);
		}
		set
		{
			SetValue(key, value);
		}
	}

	internal Dictionary<CompatibilityOption, bool> PropertiesHash
	{
		get
		{
			if (m_propertiesHash == null)
			{
				m_propertiesHash = new Dictionary<CompatibilityOption, bool>();
			}
			return m_propertiesHash;
		}
	}

	internal CompatibilityOptions(IWordDocument document)
	{
		m_document = document;
	}

	private void SetValue(CompatibilityOption key, bool value)
	{
		DOPDescriptor dOP = (m_document as WordDocument).DOP;
		switch (key)
		{
		case CompatibilityOption.NoTabForInd:
			dOP.Dop2000.Copts.Copts80.Copts60.NoTabForInd = value;
			return;
		case CompatibilityOption.NoSpaceRaiseLower:
			dOP.Dop2000.Copts.Copts80.Copts60.NoSpaceRaiseLower = value;
			return;
		case CompatibilityOption.SuppressSpBfAfterPgBrk:
			dOP.Dop2000.Copts.Copts80.Copts60.SuppressSpBfAfterPgBrk = value;
			return;
		case CompatibilityOption.WrapTrailSpaces:
			dOP.Dop2000.Copts.Copts80.Copts60.WrapTrailSpaces = value;
			return;
		case CompatibilityOption.MapPrintTextColor:
			dOP.Dop2000.Copts.Copts80.Copts60.MapPrintTextColor = value;
			return;
		case CompatibilityOption.NoColumnBalance:
			dOP.Dop2000.Copts.Copts80.Copts60.NoColumnBalance = value;
			return;
		case CompatibilityOption.ConvMailMergeEsc:
			dOP.Dop2000.Copts.Copts80.Copts60.ConvMailMergeEsc = value;
			return;
		case CompatibilityOption.SuppressTopSpacing:
			dOP.Dop2000.Copts.Copts80.Copts60.SuppressTopSpacing = value;
			return;
		case CompatibilityOption.OrigWordTableRules:
			dOP.Dop2000.Copts.Copts80.Copts60.OrigWordTableRules = value;
			return;
		case CompatibilityOption.ShowBreaksInFrames:
			dOP.Dop2000.Copts.Copts80.Copts60.ShowBreaksInFrames = value;
			return;
		case CompatibilityOption.SwapBordersFacingPgs:
			dOP.Dop2000.Copts.Copts80.Copts60.SwapBordersFacingPgs = value;
			return;
		case CompatibilityOption.LeaveBackslashAlone:
			dOP.Dop2000.Copts.Copts80.Copts60.LeaveBackslashAlone = value;
			return;
		case CompatibilityOption.ExpShRtn:
			dOP.Dop2000.Copts.Copts80.Copts60.ExpShRtn = value;
			return;
		case CompatibilityOption.DntULTrlSpc:
			dOP.Dop2000.Copts.Copts80.Copts60.DntULTrlSpc = value;
			return;
		case CompatibilityOption.DntBlnSbDbWid:
			dOP.Dop2000.Copts.Copts80.Copts60.DntBlnSbDbWid = value;
			return;
		case CompatibilityOption.SuppressTopSpacingMac5:
			dOP.Dop2000.Copts.Copts80.SuppressTopSpacingMac5 = value;
			return;
		case CompatibilityOption.TruncDxaExpand:
			dOP.Dop2000.Copts.Copts80.TruncDxaExpand = value;
			return;
		case CompatibilityOption.PrintBodyBeforeHdr:
			dOP.Dop2000.Copts.Copts80.PrintBodyBeforeHdr = value;
			return;
		case CompatibilityOption.NoExtLeading:
			dOP.Dop2000.Copts.Copts80.NoExtLeading = value;
			return;
		case CompatibilityOption.DontMakeSpaceForUL:
			dOP.Dop2000.Copts.Copts80.DontMakeSpaceForUL = value;
			return;
		case CompatibilityOption.MWSmallCaps:
			dOP.Dop2000.Copts.Copts80.MWSmallCaps = value;
			return;
		case CompatibilityOption.F2ptExtLeadingOnly:
			dOP.Dop2000.Copts.Copts80.F2ptExtLeadingOnly = value;
			return;
		case CompatibilityOption.TruncFontHeight:
			dOP.Dop2000.Copts.Copts80.TruncFontHeight = value;
			return;
		case CompatibilityOption.SubOnSize:
			dOP.Dop2000.Copts.Copts80.SubOnSize = value;
			return;
		case CompatibilityOption.LineWrapLikeWord6:
			dOP.Dop2000.Copts.Copts80.LineWrapLikeWord6 = value;
			return;
		case CompatibilityOption.WW6BorderRules:
			dOP.Dop2000.Copts.Copts80.WW6BorderRules = value;
			return;
		case CompatibilityOption.ExactOnTop:
			dOP.Dop2000.Copts.Copts80.ExactOnTop = value;
			return;
		case CompatibilityOption.ExtraAfter:
			dOP.Dop2000.Copts.Copts80.ExtraAfter = value;
			return;
		case CompatibilityOption.WPSpace:
			dOP.Dop2000.Copts.Copts80.WPSpace = value;
			return;
		case CompatibilityOption.WPJust:
			dOP.Dop2000.Copts.Copts80.WPJust = value;
			return;
		case CompatibilityOption.PrintMet:
			dOP.Dop2000.Copts.Copts80.PrintMet = value;
			return;
		case CompatibilityOption.SpLayoutLikeWW8:
			dOP.Dop2000.Copts.SpLayoutLikeWW8 = value;
			return;
		case CompatibilityOption.FtnLayoutLikeWW8:
			dOP.Dop2000.Copts.FtnLayoutLikeWW8 = value;
			return;
		case CompatibilityOption.DontUseHTMLParagraphAutoSpacing:
			dOP.Dop2000.Copts.DontUseHTMLParagraphAutoSpacing = value;
			return;
		case CompatibilityOption.DontAdjustLineHeightInTable:
			dOP.Dop2000.Copts.DontAdjustLineHeightInTable = value;
			return;
		case CompatibilityOption.ForgetLastTabAlign:
			dOP.Dop2000.Copts.ForgetLastTabAlign = value;
			return;
		case CompatibilityOption.UseAutospaceForFullWidthAlpha:
			dOP.Dop2000.Copts.UseAutospaceForFullWidthAlpha = value;
			return;
		case CompatibilityOption.AlignTablesRowByRow:
			dOP.Dop2000.Copts.AlignTablesRowByRow = value;
			return;
		case CompatibilityOption.LayoutRawTableWidth:
			dOP.Dop2000.Copts.LayoutRawTableWidth = value;
			return;
		case CompatibilityOption.LayoutTableRowsApart:
			dOP.Dop2000.Copts.LayoutTableRowsApart = value;
			return;
		case CompatibilityOption.UseWord97LineBreakingRules:
			dOP.Dop2000.Copts.UseWord97LineBreakingRules = value;
			return;
		case CompatibilityOption.DontBreakWrappedTables:
			dOP.Dop2000.Copts.DontBreakWrappedTables = value;
			return;
		case CompatibilityOption.DontSnapToGridInCell:
			dOP.Dop2000.Copts.DontSnapToGridInCell = value;
			return;
		case CompatibilityOption.DontAllowFieldEndSelect:
			dOP.Dop2000.Copts.DontAllowFieldEndSelect = value;
			return;
		case CompatibilityOption.ApplyBreakingRules:
			dOP.Dop2000.Copts.ApplyBreakingRules = value;
			return;
		case CompatibilityOption.DontWrapTextWithPunct:
			dOP.Dop2000.Copts.DontWrapTextWithPunct = value;
			return;
		case CompatibilityOption.DontUseAsianBreakRules:
			dOP.Dop2000.Copts.DontUseAsianBreakRules = value;
			return;
		case CompatibilityOption.UseWord2002TableStyleRules:
			dOP.Dop2000.Copts.UseWord2002TableStyleRules = value;
			return;
		case CompatibilityOption.GrowAutoFit:
			dOP.Dop2000.Copts.GrowAutoFit = value;
			return;
		case CompatibilityOption.UseNormalStyleForList:
			dOP.Dop2000.Copts.UseNormalStyleForList = value;
			return;
		case CompatibilityOption.DontUseIndentAsNumberingTabStop:
			dOP.Dop2000.Copts.DontUseIndentAsNumberingTabStop = value;
			return;
		case CompatibilityOption.FELineBreak11:
			dOP.Dop2000.Copts.FELineBreak11 = value;
			return;
		case CompatibilityOption.AllowSpaceOfSameStyleInTable:
			dOP.Dop2000.Copts.AllowSpaceOfSameStyleInTable = value;
			return;
		case CompatibilityOption.WW11IndentRules:
			dOP.Dop2000.Copts.WW11IndentRules = value;
			return;
		case CompatibilityOption.DontAutofitConstrainedTables:
			dOP.Dop2000.Copts.DontAutofitConstrainedTables = value;
			return;
		case CompatibilityOption.AutofitLikeWW11:
			dOP.Dop2000.Copts.AutofitLikeWW11 = value;
			return;
		case CompatibilityOption.UnderlineTabInNumList:
			dOP.Dop2000.Copts.UnderlineTabInNumList = value;
			return;
		case CompatibilityOption.HangulWidthLikeWW11:
			dOP.Dop2000.Copts.HangulWidthLikeWW11 = value;
			return;
		case CompatibilityOption.SplitPgBreakAndParaMark:
			dOP.Dop2000.Copts.SplitPgBreakAndParaMark = value;
			return;
		case CompatibilityOption.DontVertAlignCellWithSp:
			dOP.Dop2000.Copts.DontVertAlignCellWithSp = value;
			return;
		case CompatibilityOption.DontBreakConstrainedForcedTables:
			dOP.Dop2000.Copts.DontBreakConstrainedForcedTables = value;
			return;
		case CompatibilityOption.DontVertAlignInTxbx:
			dOP.Dop2000.Copts.DontVertAlignInTxbx = value;
			return;
		case CompatibilityOption.Word11KerningPairs:
			dOP.Dop2000.Copts.Word11KerningPairs = value;
			return;
		case CompatibilityOption.CachedColBalance:
			dOP.Dop2000.Copts.CachedColBalance = value;
			return;
		}
		if (PropertiesHash.ContainsKey(key))
		{
			PropertiesHash[key] = value;
		}
		else
		{
			PropertiesHash.Add(key, value);
		}
	}

	private bool GetValue(CompatibilityOption key)
	{
		DOPDescriptor dOP = (m_document as WordDocument).DOP;
		switch (key)
		{
		case CompatibilityOption.NoTabForInd:
			return dOP.Dop2000.Copts.Copts80.Copts60.NoTabForInd;
		case CompatibilityOption.NoSpaceRaiseLower:
			return dOP.Dop2000.Copts.Copts80.Copts60.NoSpaceRaiseLower;
		case CompatibilityOption.SuppressSpBfAfterPgBrk:
			return dOP.Dop2000.Copts.Copts80.Copts60.SuppressSpBfAfterPgBrk;
		case CompatibilityOption.WrapTrailSpaces:
			return dOP.Dop2000.Copts.Copts80.Copts60.WrapTrailSpaces;
		case CompatibilityOption.MapPrintTextColor:
			return dOP.Dop2000.Copts.Copts80.Copts60.MapPrintTextColor;
		case CompatibilityOption.NoColumnBalance:
			return dOP.Dop2000.Copts.Copts80.Copts60.NoColumnBalance;
		case CompatibilityOption.ConvMailMergeEsc:
			return dOP.Dop2000.Copts.Copts80.Copts60.ConvMailMergeEsc;
		case CompatibilityOption.SuppressTopSpacing:
			return dOP.Dop2000.Copts.Copts80.Copts60.SuppressTopSpacing;
		case CompatibilityOption.OrigWordTableRules:
			return dOP.Dop2000.Copts.Copts80.Copts60.OrigWordTableRules;
		case CompatibilityOption.ShowBreaksInFrames:
			return dOP.Dop2000.Copts.Copts80.Copts60.ShowBreaksInFrames;
		case CompatibilityOption.SwapBordersFacingPgs:
			return dOP.Dop2000.Copts.Copts80.Copts60.SwapBordersFacingPgs;
		case CompatibilityOption.LeaveBackslashAlone:
			return dOP.Dop2000.Copts.Copts80.Copts60.LeaveBackslashAlone;
		case CompatibilityOption.ExpShRtn:
			return dOP.Dop2000.Copts.Copts80.Copts60.ExpShRtn;
		case CompatibilityOption.DntULTrlSpc:
			return dOP.Dop2000.Copts.Copts80.Copts60.DntULTrlSpc;
		case CompatibilityOption.DntBlnSbDbWid:
			return dOP.Dop2000.Copts.Copts80.Copts60.DntBlnSbDbWid;
		case CompatibilityOption.SuppressTopSpacingMac5:
			return dOP.Dop2000.Copts.Copts80.SuppressTopSpacingMac5;
		case CompatibilityOption.TruncDxaExpand:
			return dOP.Dop2000.Copts.Copts80.TruncDxaExpand;
		case CompatibilityOption.PrintBodyBeforeHdr:
			return dOP.Dop2000.Copts.Copts80.PrintBodyBeforeHdr;
		case CompatibilityOption.NoExtLeading:
			return dOP.Dop2000.Copts.Copts80.NoExtLeading;
		case CompatibilityOption.DontMakeSpaceForUL:
			return dOP.Dop2000.Copts.Copts80.DontMakeSpaceForUL;
		case CompatibilityOption.MWSmallCaps:
			return dOP.Dop2000.Copts.Copts80.MWSmallCaps;
		case CompatibilityOption.F2ptExtLeadingOnly:
			return dOP.Dop2000.Copts.Copts80.F2ptExtLeadingOnly;
		case CompatibilityOption.TruncFontHeight:
			return dOP.Dop2000.Copts.Copts80.TruncFontHeight;
		case CompatibilityOption.SubOnSize:
			return dOP.Dop2000.Copts.Copts80.SubOnSize;
		case CompatibilityOption.LineWrapLikeWord6:
			return dOP.Dop2000.Copts.Copts80.LineWrapLikeWord6;
		case CompatibilityOption.WW6BorderRules:
			return dOP.Dop2000.Copts.Copts80.WW6BorderRules;
		case CompatibilityOption.ExactOnTop:
			return dOP.Dop2000.Copts.Copts80.ExactOnTop;
		case CompatibilityOption.ExtraAfter:
			return dOP.Dop2000.Copts.Copts80.ExtraAfter;
		case CompatibilityOption.WPSpace:
			return dOP.Dop2000.Copts.Copts80.WPSpace;
		case CompatibilityOption.WPJust:
			return dOP.Dop2000.Copts.Copts80.WPJust;
		case CompatibilityOption.PrintMet:
			return dOP.Dop2000.Copts.Copts80.PrintMet;
		case CompatibilityOption.SpLayoutLikeWW8:
			return dOP.Dop2000.Copts.SpLayoutLikeWW8;
		case CompatibilityOption.FtnLayoutLikeWW8:
			return dOP.Dop2000.Copts.FtnLayoutLikeWW8;
		case CompatibilityOption.DontUseHTMLParagraphAutoSpacing:
			if ((m_document as WordDocument).WordVersion <= 193 && (m_document as WordDocument).WordVersion != 0)
			{
				return true;
			}
			return dOP.Dop2000.Copts.DontUseHTMLParagraphAutoSpacing;
		case CompatibilityOption.DontAdjustLineHeightInTable:
			return dOP.Dop2000.Copts.DontAdjustLineHeightInTable;
		case CompatibilityOption.ForgetLastTabAlign:
			return dOP.Dop2000.Copts.ForgetLastTabAlign;
		case CompatibilityOption.UseAutospaceForFullWidthAlpha:
			return dOP.Dop2000.Copts.UseAutospaceForFullWidthAlpha;
		case CompatibilityOption.AlignTablesRowByRow:
			return dOP.Dop2000.Copts.AlignTablesRowByRow;
		case CompatibilityOption.LayoutRawTableWidth:
			return dOP.Dop2000.Copts.LayoutRawTableWidth;
		case CompatibilityOption.LayoutTableRowsApart:
			return dOP.Dop2000.Copts.LayoutTableRowsApart;
		case CompatibilityOption.UseWord97LineBreakingRules:
			return dOP.Dop2000.Copts.UseWord97LineBreakingRules;
		case CompatibilityOption.DontBreakWrappedTables:
			return dOP.Dop2000.Copts.DontBreakWrappedTables;
		case CompatibilityOption.DontSnapToGridInCell:
			return dOP.Dop2000.Copts.DontSnapToGridInCell;
		case CompatibilityOption.DontAllowFieldEndSelect:
			return dOP.Dop2000.Copts.DontAllowFieldEndSelect;
		case CompatibilityOption.ApplyBreakingRules:
			return dOP.Dop2000.Copts.ApplyBreakingRules;
		case CompatibilityOption.DontWrapTextWithPunct:
			return dOP.Dop2000.Copts.DontWrapTextWithPunct;
		case CompatibilityOption.DontUseAsianBreakRules:
			return dOP.Dop2000.Copts.DontUseAsianBreakRules;
		case CompatibilityOption.UseWord2002TableStyleRules:
			return dOP.Dop2000.Copts.UseWord2002TableStyleRules;
		case CompatibilityOption.GrowAutoFit:
			return dOP.Dop2000.Copts.GrowAutoFit;
		case CompatibilityOption.UseNormalStyleForList:
			return dOP.Dop2000.Copts.UseNormalStyleForList;
		case CompatibilityOption.DontUseIndentAsNumberingTabStop:
			return dOP.Dop2000.Copts.DontUseIndentAsNumberingTabStop;
		case CompatibilityOption.FELineBreak11:
			return dOP.Dop2000.Copts.FELineBreak11;
		case CompatibilityOption.AllowSpaceOfSameStyleInTable:
			return dOP.Dop2000.Copts.AllowSpaceOfSameStyleInTable;
		case CompatibilityOption.WW11IndentRules:
			return dOP.Dop2000.Copts.WW11IndentRules;
		case CompatibilityOption.DontAutofitConstrainedTables:
			return dOP.Dop2000.Copts.DontAutofitConstrainedTables;
		case CompatibilityOption.AutofitLikeWW11:
			return dOP.Dop2000.Copts.AutofitLikeWW11;
		case CompatibilityOption.UnderlineTabInNumList:
			return dOP.Dop2000.Copts.UnderlineTabInNumList;
		case CompatibilityOption.HangulWidthLikeWW11:
			return dOP.Dop2000.Copts.HangulWidthLikeWW11;
		case CompatibilityOption.SplitPgBreakAndParaMark:
			if ((m_document as WordDocument).WordVersion <= 268 && (m_document as WordDocument).WordVersion != 0)
			{
				return true;
			}
			return dOP.Dop2000.Copts.SplitPgBreakAndParaMark;
		case CompatibilityOption.DontVertAlignCellWithSp:
			return dOP.Dop2000.Copts.DontVertAlignCellWithSp;
		case CompatibilityOption.DontBreakConstrainedForcedTables:
			return dOP.Dop2000.Copts.DontBreakConstrainedForcedTables;
		case CompatibilityOption.DontVertAlignInTxbx:
			return dOP.Dop2000.Copts.DontVertAlignInTxbx;
		case CompatibilityOption.Word11KerningPairs:
			return dOP.Dop2000.Copts.Word11KerningPairs;
		case CompatibilityOption.CachedColBalance:
			return dOP.Dop2000.Copts.CachedColBalance;
		default:
			if (PropertiesHash.ContainsKey(key))
			{
				return PropertiesHash[key];
			}
			return false;
		}
	}

	internal void Close()
	{
		m_document = null;
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
			m_propertiesHash = null;
		}
	}
}

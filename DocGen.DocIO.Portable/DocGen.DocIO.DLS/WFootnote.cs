using DocGen.DocIO.DLS.Rendering;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class WFootnote : ParagraphItem, ILeafWidget, IWidget
{
	internal const int DEF_FTNSTYLE_REF_ID = 38;

	internal const int DEF_EDNSTYLE_REF_ID = 39;

	private FootnoteType m_footnoteType;

	private WTextBody m_textBody;

	private byte m_symbolCode;

	private string m_strSymbolFontName = "Symbol";

	internal string m_strCustomMarker = string.Empty;

	private short m_changesCount;

	private byte m_bFlags = 1;

	public override EntityType EntityType => EntityType.Footnote;

	public FootnoteType FootnoteType
	{
		get
		{
			return m_footnoteType;
		}
		set
		{
			m_footnoteType = value;
		}
	}

	public bool IsAutoNumbered
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			if (IsAutoNumbered != value)
			{
				if (!base.Document.IsOpening)
				{
					UpdateChangeFlag(value: true);
					UpdateAutoMarker(value);
				}
				m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
			}
		}
	}

	public WTextBody TextBody => m_textBody;

	public WCharacterFormat MarkerCharacterFormat => m_charFormat;

	public byte SymbolCode
	{
		get
		{
			return m_symbolCode;
		}
		set
		{
			if (value != m_symbolCode && !base.Document.IsOpening)
			{
				UpdateChangeFlag(value: true);
				UpdateSymbolMarker(value);
			}
			m_symbolCode = value;
		}
	}

	internal string SymbolFontName
	{
		get
		{
			return m_strSymbolFontName;
		}
		set
		{
			if (value != m_strSymbolFontName && !base.Document.IsOpening)
			{
				UpdateChangeFlag(value: true);
			}
			m_strSymbolFontName = value;
		}
	}

	public string CustomMarker
	{
		get
		{
			return GetCustomMarkerValue();
		}
		set
		{
			string customMarkerValue = GetCustomMarkerValue();
			string text = value;
			if (text.Length > 10)
			{
				text = text.Substring(0, 10);
			}
			if (text != customMarkerValue && !IsAutoNumbered && !base.Document.IsOpening && m_symbolCode == 0)
			{
				UpdateChangeFlag(value: true);
				UpdateCustomMarker(customMarkerValue, text);
			}
			ClearPreviousCustomMarker();
			m_strCustomMarker = text;
		}
	}

	internal bool CustomMarkerIsSymbol => m_symbolCode > 0;

	internal bool IsLayouted
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	public WFootnote(IWordDocument doc)
		: base((WordDocument)doc)
	{
		m_textBody = new WTextBody(base.Document, this);
		m_charFormat = new WCharacterFormat(base.Document, this);
	}

	internal WFootnote(IWordDocument doc, string marker)
		: this(doc)
	{
		m_strCustomMarker = marker;
		IsAutoNumbered = false;
	}

	internal override void AddSelf()
	{
		if (m_textBody != null)
		{
			m_textBody.AddSelf();
		}
	}

	internal override void AttachToParagraph(WParagraph owner, int itemPos)
	{
		base.AttachToParagraph(owner, itemPos);
		if (m_textBody != null)
		{
			m_textBody.AttachToDocument();
		}
	}

	protected override void CreateLayoutInfo()
	{
		IsLayouted = false;
		m_layoutInfo = new LayoutFootnoteInfoImpl(this);
		WParagraph ownerParagraphValue = GetOwnerParagraphValue();
		if (ownerParagraphValue != null)
		{
			m_layoutInfo.IsVerticalText = ownerParagraphValue.IsVerticalText();
		}
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (m_textBody != null)
		{
			m_textBody.InitLayoutInfo(entity, ref isLastTOCEntry);
			if (isLastTOCEntry)
			{
				return;
			}
		}
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}

	protected override object CloneImpl()
	{
		WFootnote wFootnote = (WFootnote)base.CloneImpl();
		wFootnote.m_textBody = (WTextBody)m_textBody.Clone();
		wFootnote.m_textBody.SetOwner(wFootnote);
		return wFootnote;
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		if (m_textBody != null)
		{
			m_textBody.CloneRelationsTo(doc, nextOwner);
		}
		base.CloneRelationsTo(doc, nextOwner);
		m_doc = doc;
	}

	internal override void OnStateChange(object sender)
	{
		if (sender is WCharacterFormat)
		{
			UpdateChangeFlag(value: true);
		}
	}

	internal override void Close()
	{
		if (m_textBody != null)
		{
			m_textBody.Close();
			m_textBody = null;
		}
		if (m_strCustomMarker != null)
		{
			m_strCustomMarker = string.Empty;
		}
		if (m_strSymbolFontName != null)
		{
			m_strSymbolFontName = string.Empty;
		}
		base.Close();
	}

	protected override void InitXDLSHolder()
	{
		base.XDLSHolder.AddElement("body", m_textBody);
		base.XDLSHolder.AddElement("marker-character-format", m_charFormat);
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("type", FootnoteType.Footnote);
		writer.WriteValue("AutoNumbered", IsAutoNumbered);
		if (m_footnoteType == FootnoteType.Endnote)
		{
			writer.WriteValue("IsEndnoteAttr", value: true);
		}
		if (m_strCustomMarker != string.Empty)
		{
			writer.WriteValue("CustomMarker", m_strCustomMarker);
		}
		if (CustomMarkerIsSymbol)
		{
			writer.WriteValue("SymbolCode", m_symbolCode);
			writer.WriteValue("SymbolFontName", m_strSymbolFontName);
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("AutoNumbered"))
		{
			IsAutoNumbered = reader.ReadBoolean("AutoNumbered");
		}
		if (reader.HasAttribute("CustomMarker"))
		{
			m_strCustomMarker = reader.ReadString("CustomMarker");
		}
		if (reader.HasAttribute("IsEndnoteAttr"))
		{
			m_footnoteType = (reader.ReadBoolean("IsEndnoteAttr") ? FootnoteType.Endnote : FootnoteType.Footnote);
		}
		if (reader.HasAttribute("SymbolCode"))
		{
			m_symbolCode = reader.ReadByte("SymbolCode");
		}
		if (reader.HasAttribute("SymbolFontName"))
		{
			m_strSymbolFontName = reader.ReadString("SymbolFontName");
		}
	}

	private void UpdateAutoMarker(bool isAuto)
	{
		if (!base.Document.IsOpening && (IsAutoNumbered || !string.IsNullOrEmpty(m_strCustomMarker)))
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			if (!isAuto)
			{
				empty2 = '\u0002'.ToString();
				empty = m_strCustomMarker;
			}
			else
			{
				empty2 = m_strCustomMarker;
				empty = '\u0002'.ToString();
			}
			UpdateFtnMarker(empty2, empty);
		}
	}

	private void UpdateCustomMarker(string curMarker, string destMarker)
	{
		if (!base.Document.IsOpening && !string.IsNullOrEmpty(curMarker))
		{
			UpdateFtnMarker(curMarker, destMarker);
		}
	}

	private void UpdateSymbolMarker(byte symbolCode)
	{
		if (!base.Document.IsOpening && m_textBody.Items.Count != 0 && !IsAutoNumbered && symbolCode > 0 && !string.IsNullOrEmpty(m_strCustomMarker))
		{
			WParagraph wParagraph = m_textBody.Items[0] as WParagraph;
			TextSelection selection = wParagraph.Find(m_strCustomMarker, caseSensitive: true, wholeWord: true);
			WTextRange symbol = GenerateSymbol(symbolCode);
			ReplaceSelection(symbol, wParagraph, selection);
			m_charFormat.FontName = m_strSymbolFontName;
			UpdateChangeFlag(value: false);
		}
	}

	private void ClearPreviousCustomMarker()
	{
		if (!(base.Owner is WParagraph))
		{
			return;
		}
		WParagraph ownerParagraph = base.OwnerParagraph;
		int num = base.OwnerParagraph.ChildEntities.IndexOf(this);
		int num2 = m_strCustomMarker.Length;
		if (num2 >= 10)
		{
			return;
		}
		for (int i = num + 1; i < ownerParagraph.Items.Count && ownerParagraph.Items[i].EntityType == EntityType.TextRange; i++)
		{
			WTextRange wTextRange = ownerParagraph.Items[i] as WTextRange;
			bool flag = CompareCharacterFormat(wTextRange.CharacterFormat);
			if (wTextRange.Text.Contains(" "))
			{
				if (flag)
				{
					int num3 = wTextRange.Text.IndexOf(' ');
					if (num2 + num3 > 10)
					{
						wTextRange.Text = wTextRange.Text.Remove(0, 10 - num2);
					}
					else
					{
						wTextRange.Text = wTextRange.Text.Remove(0, num3);
					}
				}
				break;
			}
			if (flag)
			{
				if (num2 + wTextRange.Text.Length > 10)
				{
					wTextRange.Text = wTextRange.Text.Remove(0, 10 - num2);
					break;
				}
				ownerParagraph.Items.Remove(wTextRange);
				num2 += wTextRange.Text.Length;
				if (num2 == 10)
				{
					break;
				}
				i--;
			}
		}
	}

	private string GetCustomMarkerValue()
	{
		if (!(base.Owner is WParagraph))
		{
			return m_strCustomMarker;
		}
		string strCustomMarker = m_strCustomMarker;
		if (strCustomMarker.Length == 10)
		{
			if (strCustomMarker.Contains(" "))
			{
				return strCustomMarker.Substring(0, strCustomMarker.IndexOf(" "));
			}
			return strCustomMarker;
		}
		if (strCustomMarker.Length > 10)
		{
			if (strCustomMarker.Contains(" "))
			{
				int num = strCustomMarker.IndexOf(" ");
				if (num > 10)
				{
					num = 10;
				}
				return strCustomMarker.Substring(0, num);
			}
			return strCustomMarker.Substring(0, 10);
		}
		return strCustomMarker;
	}

	internal bool CompareCharacterFormat(WCharacterFormat textRangeFormat)
	{
		if (MarkerCharacterFormat.FontName != textRangeFormat.FontName)
		{
			return false;
		}
		if (MarkerCharacterFormat.FontSize != textRangeFormat.FontSize)
		{
			return false;
		}
		if (MarkerCharacterFormat.ComplexScript != textRangeFormat.ComplexScript)
		{
			return false;
		}
		if (MarkerCharacterFormat.Bold != textRangeFormat.Bold)
		{
			return false;
		}
		if (MarkerCharacterFormat.Italic != textRangeFormat.Italic)
		{
			return false;
		}
		if (MarkerCharacterFormat.Strikeout != textRangeFormat.Strikeout)
		{
			return false;
		}
		if (MarkerCharacterFormat.AllCaps != textRangeFormat.AllCaps)
		{
			return false;
		}
		if (MarkerCharacterFormat.TextColor != textRangeFormat.TextColor)
		{
			return false;
		}
		if (MarkerCharacterFormat.DoubleStrike != textRangeFormat.DoubleStrike)
		{
			return false;
		}
		if (MarkerCharacterFormat.FontNameFarEast != textRangeFormat.FontNameFarEast)
		{
			return false;
		}
		if (MarkerCharacterFormat.TextBackgroundColor != textRangeFormat.TextBackgroundColor)
		{
			return false;
		}
		if (MarkerCharacterFormat.Emboss != textRangeFormat.Emboss)
		{
			return false;
		}
		if (MarkerCharacterFormat.Engrave != textRangeFormat.Engrave)
		{
			return false;
		}
		if (MarkerCharacterFormat.HighlightColor != textRangeFormat.HighlightColor)
		{
			return false;
		}
		if (MarkerCharacterFormat.FontNameAscii != textRangeFormat.FontNameAscii)
		{
			return false;
		}
		if (MarkerCharacterFormat.FontNameBidi != textRangeFormat.FontNameBidi)
		{
			return false;
		}
		if (MarkerCharacterFormat.OutLine != textRangeFormat.OutLine)
		{
			return false;
		}
		if (MarkerCharacterFormat.Position != textRangeFormat.Position)
		{
			return false;
		}
		if (MarkerCharacterFormat.FontSizeBidi != textRangeFormat.FontSizeBidi)
		{
			return false;
		}
		if (MarkerCharacterFormat.CharStyleName != textRangeFormat.CharStyleName)
		{
			return false;
		}
		if (MarkerCharacterFormat.Shadow != textRangeFormat.Shadow)
		{
			return false;
		}
		if (MarkerCharacterFormat.SubSuperScript != textRangeFormat.SubSuperScript)
		{
			return false;
		}
		if (MarkerCharacterFormat.SmallCaps != textRangeFormat.SmallCaps)
		{
			return false;
		}
		if (MarkerCharacterFormat.Special != textRangeFormat.Special)
		{
			return false;
		}
		if (MarkerCharacterFormat.CharacterSpacing != textRangeFormat.CharacterSpacing)
		{
			return false;
		}
		if (MarkerCharacterFormat.FieldVanish != textRangeFormat.FieldVanish)
		{
			return false;
		}
		if (MarkerCharacterFormat.FontNameNonFarEast != textRangeFormat.FontNameNonFarEast)
		{
			return false;
		}
		if (MarkerCharacterFormat.Hidden != textRangeFormat.Hidden)
		{
			return false;
		}
		if (MarkerCharacterFormat.ItalicBidi != textRangeFormat.ItalicBidi)
		{
			return false;
		}
		if (MarkerCharacterFormat.LocaleIdBidi != textRangeFormat.LocaleIdBidi)
		{
			return false;
		}
		if (MarkerCharacterFormat.NumberSpacing != textRangeFormat.NumberSpacing)
		{
			return false;
		}
		if (MarkerCharacterFormat.UnderlineStyle != textRangeFormat.UnderlineStyle)
		{
			return false;
		}
		if (MarkerCharacterFormat.BoldBidi != textRangeFormat.BoldBidi)
		{
			return false;
		}
		if (MarkerCharacterFormat.TextureStyle != textRangeFormat.TextureStyle)
		{
			return false;
		}
		if (MarkerCharacterFormat.Border != null && textRangeFormat.Border != null && !CompareBorderProperty(textRangeFormat.Border))
		{
			return false;
		}
		return true;
	}

	private bool CompareBorderProperty(Border textRangeBorder)
	{
		if (MarkerCharacterFormat.Border.BorderPosition != textRangeBorder.BorderPosition)
		{
			return false;
		}
		if (MarkerCharacterFormat.Border.BorderType != textRangeBorder.BorderType)
		{
			return false;
		}
		if (MarkerCharacterFormat.Border.Color != textRangeBorder.Color)
		{
			return false;
		}
		if (MarkerCharacterFormat.Border.LineWidth != textRangeBorder.LineWidth)
		{
			return false;
		}
		if (MarkerCharacterFormat.Border.Shadow != textRangeBorder.Shadow)
		{
			return false;
		}
		if (MarkerCharacterFormat.Border.Space != textRangeBorder.Space)
		{
			return false;
		}
		return true;
	}

	internal void UpdateFtnMarker(string curMarker, string destMarker)
	{
		if (m_textBody.Items.Count != 0)
		{
			WParagraph wParagraph = m_textBody.Items[0] as WParagraph;
			if (!string.IsNullOrEmpty(curMarker))
			{
				TextSelection selection = wParagraph.Find(curMarker, caseSensitive: true, wholeWord: false);
				ReplaceMarker(selection, destMarker);
			}
			else
			{
				AppendMarker(destMarker, wParagraph);
			}
			UpdateChangeFlag(value: false);
		}
	}

	internal void EnsureFtnMarker()
	{
		if (m_changesCount <= 0)
		{
			return;
		}
		if (!IsAutoNumbered && m_symbolCode > 0)
		{
			AppendFtnSymbol();
			UpdateChangeFlag(value: false);
			return;
		}
		string text = (IsAutoNumbered ? '\u0002'.ToString() : CustomMarker);
		if (text.TrimStart(' ') != string.Empty)
		{
			text = text.TrimStart(' ');
		}
		WParagraph wParagraph = new WParagraph(m_doc);
		if (m_textBody.Items.Count == 0)
		{
			AppendMarker(text, wParagraph);
			m_textBody.Items.Insert(0, wParagraph);
		}
		else
		{
			TextSelection textSelection = null;
			foreach (WParagraph paragraph in m_textBody.Paragraphs)
			{
				textSelection = paragraph.Find(text, caseSensitive: true, wholeWord: false);
				if (textSelection != null)
				{
					wParagraph = paragraph;
					break;
				}
			}
			if (textSelection == null)
			{
				if (m_textBody.Paragraphs.Count > 0)
				{
					wParagraph = m_textBody.Paragraphs[0];
				}
				else
				{
					m_textBody.Items.Insert(0, wParagraph);
				}
				AppendMarker(text, wParagraph);
			}
			else
			{
				ReplaceMarker(textSelection, text);
			}
		}
		UpdateChangeFlag(value: false);
	}

	private void AppendFtnSymbol()
	{
		WTextRange wTextRange = GenerateSymbol(m_symbolCode);
		m_charFormat.FontName = m_strSymbolFontName;
		if (m_textBody.Items.Count == 0)
		{
			WParagraph wParagraph = new WParagraph(m_doc);
			wParagraph.Items.Add(wTextRange);
			wParagraph.AppendText(" ");
			m_textBody.Items.Insert(0, wParagraph);
			return;
		}
		string text = (IsAutoNumbered ? '\u0002'.ToString() : CustomMarker);
		WParagraph wParagraph2 = m_textBody.Items[0] as WParagraph;
		TextSelection selection = null;
		if (text != string.Empty)
		{
			selection = wParagraph2.Find(text, caseSensitive: true, wholeWord: true);
		}
		ReplaceSelection(wTextRange, wParagraph2, selection);
	}

	private WTextRange GenerateSymbol(byte symbolCode)
	{
		WTextRange wTextRange = new WTextRange(base.Document);
		char c = (char)symbolCode;
		wTextRange.Text = c.ToString();
		wTextRange.CharacterFormat.ImportContainer(m_charFormat);
		wTextRange.CharacterFormat.FontName = m_strSymbolFontName;
		return wTextRange;
	}

	private void ReplaceSelection(WTextRange symbol, WParagraph para, TextSelection selection)
	{
		if (selection == null)
		{
			para.Items.Insert(0, symbol);
			WTextRange wTextRange = new WTextRange(para.Document);
			wTextRange.Text = " ";
			para.Items.Insert(1, wTextRange);
		}
		else
		{
			WTextRange asOneRange = selection.GetAsOneRange();
			int indexInOwnerCollection = asOneRange.GetIndexInOwnerCollection();
			para.Items.Remove(asOneRange);
			para.Items.Insert(indexInOwnerCollection, symbol);
		}
	}

	internal void EnsureFtnStyle()
	{
		if (base.Document.IsOpening)
		{
			return;
		}
		string charStyleName = m_charFormat.CharStyleName;
		Style style = null;
		if (!string.IsNullOrEmpty(charStyleName))
		{
			style = base.Document.Styles.FindByName(charStyleName) as Style;
		}
		if (style == null || (style.StyleId != 39 && style.StyleId != 38))
		{
			WCharacterStyle wCharacterStyle = null;
			if (m_footnoteType == FootnoteType.Footnote)
			{
				wCharacterStyle = (WCharacterStyle)Style.CreateBuiltinStyle(BuiltinStyle.FootnoteReference, StyleType.CharacterStyle, base.Document);
			}
			else if (m_footnoteType == FootnoteType.Endnote)
			{
				wCharacterStyle = (WCharacterStyle)Style.CreateBuiltinStyle(BuiltinStyle.EndnoteReference, StyleType.CharacterStyle, base.Document);
			}
			if (wCharacterStyle != null)
			{
				m_charFormat.CharStyleName = wCharacterStyle.Name;
				base.Document.Styles.Add(wCharacterStyle);
			}
		}
	}

	private void UpdateChangeFlag(bool value)
	{
		if (!base.Document.IsOpening)
		{
			if (value)
			{
				m_changesCount++;
			}
			else
			{
				m_changesCount--;
			}
		}
	}

	private void ReplaceMarker(TextSelection selection, string replaceText)
	{
		if (selection != null)
		{
			selection.GetAsOneRange().Text = replaceText;
		}
	}

	private void AppendMarker(string marker, WParagraph para)
	{
		WTextRange entity = GenerateText(marker);
		para.Items.Insert(0, entity);
		WTextRange wTextRange = new WTextRange(m_textBody.Document);
		wTextRange.Text = " ";
		para.Items.Insert(1, wTextRange);
	}

	internal WTextRange GenerateText(string marker)
	{
		WTextRange wTextRange = new WTextRange(m_doc);
		wTextRange.Text = marker;
		wTextRange.CharacterFormat.ImportContainer(m_charFormat);
		return wTextRange;
	}

	SizeF ILeafWidget.Measure(DrawingContext dc)
	{
		return ((IWidget)this).LayoutInfo.Size;
	}

	void IWidget.InitLayoutInfo()
	{
		if (m_layoutInfo != null && FootnoteType != FootnoteType.Endnote)
		{
			if (DocumentLayouter.m_footnoteId > 0)
			{
				_ = DocumentLayouter.DrawingContext;
				DocumentLayouter.m_footnoteId--;
			}
			if (DocumentLayouter.m_footnoteIDRestartEachPage > 1)
			{
				DocumentLayouter.m_footnoteIDRestartEachPage--;
			}
			if (DocumentLayouter.m_footnoteIDRestartEachSection > 1)
			{
				DocumentLayouter.m_footnoteIDRestartEachSection--;
			}
			m_layoutInfo = null;
		}
	}

	void IWidget.InitLayoutInfo(IWidget widget)
	{
	}
}

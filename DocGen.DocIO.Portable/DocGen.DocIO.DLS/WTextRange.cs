using System.Text;
using DocGen.DocIO.DLS.Rendering;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;
using DocGen.Office;

namespace DocGen.DocIO.DLS;

public class WTextRange : ParagraphItem, IWTextRange, IParagraphItem, IEntity, IStringWidget, ISplitLeafWidget, ILeafWidget, IWidget, ITextMeasurable
{
	internal class LayoutTabInfo : TabsLayoutInfo
	{
		public LayoutTabInfo(WTextRange textRange)
			: base(ChildrenLayoutDirection.Horizontal)
		{
			textRange.Text = string.Empty;
			WParagraph wParagraph = textRange.OwnerParagraph;
			if (textRange.Owner is InlineContentControl || textRange.Owner is XmlParagraphItem)
			{
				wParagraph = textRange.GetOwnerParagraphValue();
			}
			m_defaultTabWidth = wParagraph.GetDefaultTabWidth();
			WParagraphFormat paragraphFormat = wParagraph.ParagraphFormat;
			if (wParagraph.GetStyle() == null && !(textRange.Document.Styles.FindByName("Normal", StyleType.ParagraphStyle) is IWParagraphStyle))
			{
				_ = (WParagraphStyle)Style.CreateBuiltinStyle(BuiltinStyle.Normal, textRange.Document);
			}
			if (m_list.Count == 0)
			{
				SortParagraphTabsCollection(paragraphFormat, null, 0);
			}
		}
	}

	private int m_txtLength;

	private string m_detachedText = string.Empty;

	private string m_originalText = string.Empty;

	private float m_ascent = float.MinValue;

	private byte m_bFlags;

	private CharacterRangeType m_charRangeType;

	private FontScriptType m_fontScriptType;

	internal int m_wcEndPos = -1;

	private int m_wcTextLength = -1;

	public override EntityType EntityType => EntityType.TextRange;

	public virtual string Text
	{
		get
		{
			if (!base.ItemDetached && base.Owner is WParagraph && base.IsDetachedTextChanged)
			{
				string text = base.OwnerParagraph.Text;
				m_detachedText = text.Substring(base.StartPos, m_txtLength);
				base.IsDetachedTextChanged = false;
			}
			else if (!base.ItemDetached && base.Owner is InlineContentControl && !base.IsMappedItem && base.IsDetachedTextChanged)
			{
				WParagraph ownerParagraphValue = GetOwnerParagraphValue();
				if (ownerParagraphValue != null)
				{
					string text2 = ownerParagraphValue.Text;
					m_detachedText = text2.Substring(base.StartPos, m_txtLength);
					base.IsDetachedTextChanged = false;
				}
			}
			return m_detachedText;
		}
		set
		{
			WParagraph wParagraph = ((base.Owner is InlineContentControl && !base.IsMappedItem) ? GetOwnerParagraphValue() : base.OwnerParagraph);
			if (base.ItemDetached || wParagraph == null)
			{
				m_detachedText = value;
			}
			else if (value != Text)
			{
				wParagraph.UpdateText(this, value, isRemove: true);
				m_txtLength = value.Length;
				base.IsDetachedTextChanged = true;
			}
			if (!m_doc.IsOpening)
			{
				UpdateXMLMappedValue(wParagraph, value);
			}
			SafeText = base.Document.IsOpening;
		}
	}

	internal string OrignalText
	{
		get
		{
			return m_originalText;
		}
		set
		{
			m_originalText = value;
		}
	}

	public WCharacterFormat CharacterFormat
	{
		get
		{
			return m_charFormat;
		}
		internal set
		{
			m_charFormat = value;
		}
	}

	internal int TextLength
	{
		get
		{
			if (!base.ItemDetached)
			{
				return m_txtLength;
			}
			return m_detachedText.Length;
		}
		set
		{
			if (m_txtLength != value || (base.Owner is WParagraph && base.OwnerParagraph.IsTextReplaced) || (base.Owner is WParagraph && base.StartPos >= 0 && base.StartPos + value <= base.OwnerParagraph.Text.Length && m_detachedText != base.OwnerParagraph.Text.Substring(base.StartPos, value)))
			{
				base.IsDetachedTextChanged = true;
			}
			m_txtLength = value;
		}
	}

	internal CharacterRangeType CharacterRange
	{
		get
		{
			return m_charRangeType;
		}
		set
		{
			m_charRangeType = value;
		}
	}

	internal FontScriptType ScriptType
	{
		get
		{
			return m_fontScriptType;
		}
		set
		{
			m_fontScriptType = value;
		}
	}

	internal override int EndPos => base.EndPos + m_txtLength;

	internal bool SafeText
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsParagraphMark
	{
		get
		{
			if (base.Owner == null && CharacterFormat != null && CharacterFormat.BaseFormat != null)
			{
				return CharacterFormat.BaseFormat.OwnerBase is WParagraph;
			}
			return false;
		}
	}

	internal int WCTextLength
	{
		get
		{
			if (m_wcTextLength != -1 && base.Document.IsComparing)
			{
				return m_wcTextLength;
			}
			return TextLength;
		}
		set
		{
			m_wcTextLength = value;
		}
	}

	internal override int WCEndPos
	{
		get
		{
			if (m_wcEndPos != -1 && base.Document.IsComparing)
			{
				return m_wcEndPos;
			}
			return WCStartPos + WCTextLength;
		}
	}

	internal override int WCStartPos
	{
		get
		{
			if (m_wcStartPos != -1 && base.Document.IsComparing)
			{
				return m_wcStartPos;
			}
			return base.StartPos;
		}
		set
		{
			m_wcStartPos = value;
			StringBuilder stringBuilder = new StringBuilder(base.OwnerParagraph.InternalText);
			base.OwnerParagraph.InternalText = stringBuilder.Remove(WCStartPos, WCTextLength).ToString();
			WCTextLength = 0;
			m_wcEndPos = m_wcStartPos;
		}
	}

	private void UpdateXMLMappedValue(WParagraph ownerPara, string value)
	{
		if (base.Owner is InlineContentControl && (base.Owner as InlineContentControl).ContentControlProperties.XmlMapping.IsMapped && (base.Owner as InlineContentControl).ContentControlProperties.Type == ContentControlType.Text)
		{
			(base.Owner as InlineContentControl).ContentControlProperties.XmlMapping.UpdateMappedValue(base.Owner as InlineContentControl, m_doc, value.Trim());
		}
		else if (ownerPara != null && ownerPara.Owner != null && ownerPara.Owner.Owner is BlockContentControl && (ownerPara.Owner.Owner as BlockContentControl).ContentControlProperties.XmlMapping.IsMapped && (ownerPara.Owner.Owner as BlockContentControl).ContentControlProperties.Type == ContentControlType.Text)
		{
			(ownerPara.Owner.Owner as BlockContentControl).ContentControlProperties.XmlMapping.UpdateMappedValue(ownerPara.Owner.Owner as BlockContentControl, m_doc, value.Trim());
		}
	}

	public WTextRange(IWordDocument doc)
		: base((WordDocument)doc)
	{
		m_charFormat = new WCharacterFormat(base.Document, this);
	}

	internal override void AttachToParagraph(WParagraph paragraph, int itemPos)
	{
		m_txtLength = 0;
		base.IsDetachedTextChanged = true;
		base.AttachToParagraph(paragraph, itemPos);
		Text = m_detachedText;
	}

	internal void Attach(WParagraph paragraph, int itemPos, bool isField)
	{
		m_txtLength = 0;
		base.AttachToParagraph(paragraph, itemPos);
	}

	internal void InsertTextInParagraphText(WParagraph paragraph)
	{
		paragraph.UpdateText(this, m_detachedText, isRemove: false);
		m_txtLength = m_detachedText.Length;
	}

	internal override void Detach()
	{
		base.Detach();
		WParagraph ownerParagraphValue = GetOwnerParagraphValue();
		if (ownerParagraphValue != null)
		{
			m_detachedText = Text;
			ownerParagraphValue.UpdateText(this, string.Empty, isRemove: true);
		}
	}

	internal void Detach(bool isField)
	{
		base.Detach();
	}

	protected override object CloneImpl()
	{
		WTextRange obj = (WTextRange)base.CloneImpl();
		obj.m_detachedText = Text;
		return obj;
	}

	internal object CloneImpl(bool isField)
	{
		return (WTextRange)base.CloneImpl();
	}

	public void ApplyCharacterFormat(WCharacterFormat charFormat)
	{
		if (charFormat != null)
		{
			SetParagraphItemCharacterFormat(charFormat);
		}
	}

	internal void SplitWidgets()
	{
		SplitByTab();
		SplitByParagraphBreak();
	}

	private void SplitByTab()
	{
		string empty = string.Empty;
		if (Text != ControlChar.Tab && Text.Contains(ControlChar.Tab))
		{
			int num = Text.IndexOf(ControlChar.Tab);
			empty = Text;
			ParagraphItemCollection obj = ((base.Owner is InlineContentControl) ? (base.Owner as InlineContentControl).ParagraphItems : base.OwnerParagraph.Items);
			int num2 = obj.IndexOf(this);
			string text = empty.Substring(num + 1);
			WTextRange wTextRange = Clone() as WTextRange;
			if (num > 0)
			{
				wTextRange.Text = empty.Substring(num);
				Text = empty.Substring(0, num);
			}
			else if (text != string.Empty)
			{
				wTextRange.Text = text;
				Text = ControlChar.Tab;
			}
			obj.Insert(num2 + 1, wTextRange);
		}
	}

	internal override void Close()
	{
		if (m_detachedText != null)
		{
			m_detachedText = null;
		}
		base.Close();
	}

	private void SplitByParagraphBreak()
	{
		string empty = string.Empty;
		empty = Text.Replace(ControlChar.CrLf, ControlChar.ParagraphBreak);
		empty = empty.Replace(ControlChar.LineFeedChar, '\r');
		if (!empty.Contains(ControlChar.ParagraphBreak))
		{
			return;
		}
		int num = empty.IndexOf(ControlChar.ParagraphBreak);
		string text = empty.Substring(num + 1);
		WTextRange wTextRange = Clone() as WTextRange;
		if (base.ParaItemCharFormat.TableStyleCharacterFormat != null)
		{
			wTextRange.ParaItemCharFormat.TableStyleCharacterFormat = base.ParaItemCharFormat.TableStyleCharacterFormat;
		}
		if (num > 0)
		{
			wTextRange.Text = empty.Substring(num + 1);
			Text = empty.Substring(0, num);
		}
		else if (text != string.Empty)
		{
			wTextRange.Text = text;
			Text = string.Empty;
		}
		if (base.Owner is InlineContentControl)
		{
			ParagraphItemCollection paragraphItems = (base.Owner as InlineContentControl).ParagraphItems;
			int num2 = paragraphItems.IndexOf(this);
			base.Document.IsSkipFieldDetach = true;
			Break @break = new Break(base.Document, BreakType.LineBreak);
			@break.TextRange.Text = ControlChar.LineBreak;
			@break.TextRange.CharacterFormat.ImportContainer(wTextRange.CharacterFormat);
			@break.TextRange.CharacterFormat.CopyProperties(wTextRange.CharacterFormat);
			paragraphItems.Insert(++num2, @break);
			paragraphItems.Insert(++num2, wTextRange);
			if (empty == ControlChar.ParagraphBreak)
			{
				Text = string.Empty;
				wTextRange.Text = string.Empty;
			}
		}
		else
		{
			WParagraph wParagraph = base.OwnerParagraph.Clone() as WParagraph;
			ApplyTableStyleFormatting(base.OwnerParagraph, wParagraph);
			wParagraph.ClearItems();
			(((IWidget)base.OwnerParagraph).LayoutInfo as ParagraphLayoutInfo).IsPageBreak = false;
			wParagraph.m_layoutInfo = null;
			int indexInOwnerCollection = base.OwnerParagraph.GetIndexInOwnerCollection();
			base.OwnerParagraph.OwnerTextBody.Items.Insert(indexInOwnerCollection + 1, wParagraph);
			wParagraph.Items.Add(wTextRange);
			if (empty == ControlChar.ParagraphBreak)
			{
				Text = string.Empty;
				wTextRange.Text = string.Empty;
			}
			int num3 = base.OwnerParagraph.Items.IndexOf(this);
			base.Document.IsSkipFieldDetach = true;
			while (num3 + 1 < base.OwnerParagraph.Items.Count)
			{
				wParagraph.Items.Add(base.OwnerParagraph.Items[num3 + 1]);
			}
		}
		base.Document.IsSkipFieldDetach = false;
	}

	protected override void InitXDLSHolder()
	{
		base.XDLSHolder.AddElement("character-format", m_charFormat);
	}

	protected override void WriteXmlContent(IXDLSContentWriter writer)
	{
		base.WriteXmlContent(writer);
		writer.WriteChildStringElement("text", Text);
	}

	protected override bool ReadXmlContent(IXDLSContentReader reader)
	{
		if (reader.TagName == "text")
		{
			if (base.Owner is WParagraph)
			{
				base.StartPos = base.OwnerParagraph.Text.Length;
			}
			Text = reader.ReadChildStringContent();
			if (Text == "")
			{
				reader.InnerReader.Read();
			}
			SafeText = true;
			return true;
		}
		return false;
	}

	protected override void CreateLayoutInfo()
	{
		SplitWidgets();
		m_layoutInfo = ((Text == ControlChar.Tab) ? new LayoutTabInfo(this) : new LayoutInfo(ChildrenLayoutDirection.Horizontal));
		WCharacterFormat charFormat = CharacterFormat;
		string text = Text.Trim(' ');
		if (base.IsDeleteRevision && !base.Document.RevisionOptions.ShowDeletedText)
		{
			m_layoutInfo.IsSkip = true;
		}
		if (text == string.Empty && base.Owner is WParagraph && base.OwnerParagraph.Text == Text)
		{
			charFormat = base.OwnerParagraph.BreakCharacterFormat;
		}
		m_layoutInfo.Font = new SyncFont(DocumentLayouter.DrawingContext.GetFont(this, charFormat, Text));
		if (!(base.Owner is WParagraph))
		{
			WParagraph wParagraph = null;
			wParagraph = ((!(base.Owner is InlineContentControl) && !(base.Owner is XmlParagraphItem)) ? ((CharacterFormat.BaseFormat == null) ? (base.Owner as Break).OwnerParagraph : (CharacterFormat.BaseFormat.OwnerBase as WParagraph)) : GetOwnerParagraphValue());
			if (wParagraph != null)
			{
				m_layoutInfo.IsVerticalText = wParagraph.IsVerticalText();
			}
			if (wParagraph != null && wParagraph.SectionEndMark && wParagraph.PreviousSibling != null)
			{
				m_layoutInfo.IsSkip = true;
			}
		}
		else
		{
			m_layoutInfo.IsVerticalText = base.OwnerParagraph.IsVerticalText();
		}
		if (CharacterFormat.Hidden && base.OwnerParagraph != null)
		{
			m_layoutInfo.IsSkip = true;
		}
		if (Text == "" && !(m_layoutInfo is LayoutTabInfo) && !DocumentLayouter.DrawingContext.IsTOC(this) && !(base.PreviousSibling is Break))
		{
			m_layoutInfo.IsSkip = true;
		}
		m_layoutInfo.Size = GetTextRangeSize(null);
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}

	internal SizeF GetTextRangeSize(WTextRange clonedTextRange)
	{
		DrawingContext drawingContext = DocumentLayouter.DrawingContext;
		string text = Text;
		WParagraph ownerParagraphValue = GetOwnerParagraphValue();
		if (text == '\u0002'.ToString() && ownerParagraphValue.OwnerTextBody.Owner is WFootnote)
		{
			text = ((ownerParagraphValue.OwnerTextBody.Owner as IWidget).LayoutInfo as FootnoteLayoutInfo).FootnoteID;
		}
		SizeF sizeF = default(SizeF);
		if (Text.Equals(string.Empty) || IsSpaceWidthSetToZero(ownerParagraphValue, text))
		{
			sizeF = drawingContext.MeasureTextRange((clonedTextRange == null) ? this : clonedTextRange, " ");
			sizeF.Width = 0f;
			return sizeF;
		}
		if (base.Owner is WParagraph && GetIndexInOwnerCollection() == base.OwnerParagraph.Items.Count - 1 && !(base.Owner as WParagraph).ParagraphFormat.Bidi)
		{
			text = drawingContext.GetTrimmedText(Text);
		}
		return drawingContext.MeasureTextRange((clonedTextRange == null) ? this : clonedTextRange, (clonedTextRange == null) ? text : clonedTextRange.Text);
	}

	void IWidget.InitLayoutInfo()
	{
		if (OrignalText != string.Empty)
		{
			Text = OrignalText;
		}
		if (m_layoutInfo is LayoutTabInfo && Text == string.Empty)
		{
			Text = ControlChar.Tab;
		}
		m_layoutInfo = null;
	}

	void IWidget.InitLayoutInfo(IWidget widget)
	{
	}

	SizeF ITextMeasurable.Measure(string text)
	{
		DrawingContext drawingContext = new DrawingContext();
		if (text != Text)
		{
			if (text != null && (text == string.Empty || IsLastTextRangeWithSpace(text)))
			{
				SizeF result = drawingContext.MeasureTextRange(this, " ");
				result.Width = 0f;
				return result;
			}
			return drawingContext.MeasureTextRange(this, text);
		}
		return ((IWidget)this).LayoutInfo.Size;
	}

	SizeF ITextMeasurable.Measure(DrawingContext dc, string text)
	{
		if (text != Text)
		{
			if (text != null && (text == string.Empty || IsLastTextRangeWithSpace(text)))
			{
				SizeF result = dc.MeasureTextRange(this, " ");
				result.Width = 0f;
				return result;
			}
			return dc.MeasureTextRange(this, text);
		}
		return ((IWidget)this).LayoutInfo.Size;
	}

	double IStringWidget.GetTextAscent(DrawingContext dc, ref float exceededLineAscent)
	{
		if (m_ascent == float.MinValue)
		{
			DocGen.Drawing.Font font = GetFont();
			font = ((CharacterFormat == null || CharacterFormat.Document.FontSettings.FallbackFonts.Count <= 0) ? dc.GetAlternateFontToRender(Text, font, CharacterFormat) : WordDocument.RenderHelper.GetFallbackFont(font, Text, ScriptType, CharacterFormat.Document.FontSettings.FallbackFonts, CharacterFormat.Document.FontSettings.FontStreams));
			m_ascent = dc.GetAscent(font, ScriptType);
		}
		string fontNameToRender = CharacterFormat.GetFontNameToRender(ScriptType);
		if ((fontNameToRender == "Arial Unicode MS" || fontNameToRender == "Lucida Sans Unicode") && Text.Trim(' ') != "" && !CharacterFormat.ComplexScript)
		{
			exceededLineAscent = dc.GetExceededLineHeightForArialUnicodeMSFont(GetFont(), isAscent: true, ScriptType) - m_ascent;
			if (fontNameToRender == "Lucida Sans Unicode")
			{
				exceededLineAscent /= 2f;
			}
		}
		return m_ascent;
	}

	private DocGen.Drawing.Font GetFont()
	{
		WCharacterFormat wCharacterFormat = CharacterFormat;
		if (this is WField && ((this as WField).FieldType == FieldType.FieldPage || (this as WField).FieldType == FieldType.FieldNumPages || (this as WField).FieldType == FieldType.FieldAutoNum))
		{
			wCharacterFormat = (this as WField).GetCharacterFormatValue();
		}
		DocGen.Drawing.Font font = wCharacterFormat.GetFontToRender(ScriptType);
		if (this is WCheckBox)
		{
			font = ((IWidget)this).LayoutInfo.Font.GetFont(base.Document, ScriptType);
		}
		return base.Document.FontSettings.GetFont(wCharacterFormat.GetFontNameFromHint(ScriptType), font.Size, font.Style, ScriptType);
	}

	int IStringWidget.OffsetToIndex(DrawingContext dc, double offset, string text, float clientWidth, float clientActiveAreaWidth, bool isSplitByCharacter)
	{
		float clientWidth2 = GetClientWidth(dc, clientWidth);
		bool flag = !(m_layoutInfo is ParagraphLayoutInfo paragraphLayoutInfo) || paragraphLayoutInfo.TextWrap;
		return dc.GetSplitIndexByOffset(text, this, offset, !flag, GetOwnerParagraphValue().IsInCell, clientWidth2, clientActiveAreaWidth, isSplitByCharacter);
	}

	internal float GetClientWidth(DrawingContext dc, float clientWidth)
	{
		float result = 0f;
		if (base.Owner != null)
		{
			Entity owner = base.Owner;
			bool flag = false;
			while (!(owner is WSection) && ((!(owner is WTable) && !(owner is Shape) && !(owner is ChildShape) && !(owner is WTextBox) && (!(owner is WParagraph) || (owner as WParagraph).ParagraphFormat.FrameWidth == 0f || (owner as WParagraph).OwnerTextBody.Owner == null || (owner as WParagraph).OwnerTextBody.Owner is WTextBox || (owner as WParagraph).OwnerTextBody.Owner is Shape || (owner as WParagraph).OwnerTextBody.Owner is ChildShape || (owner as WParagraph).IsInCell)) || flag) && owner.Owner != null)
			{
				owner = owner.Owner;
				if (owner is WFootnote)
				{
					flag = true;
				}
			}
			if (owner is WSection || (owner is WParagraph && (owner as WParagraph).ParagraphFormat.IsInFrame()))
			{
				ParagraphLayoutInfo paragraphLayoutInfo = ((IWidget)GetOwnerParagraphValue()).LayoutInfo as ParagraphLayoutInfo;
				result = ((owner is WParagraph && (owner as WParagraph).ParagraphFormat.IsInFrame()) ? (owner as WParagraph).ParagraphFormat.FrameWidth : clientWidth) - (paragraphLayoutInfo.Margins.Left + paragraphLayoutInfo.Margins.Right + (paragraphLayoutInfo.IsFirstLine ? (paragraphLayoutInfo.FirstLineIndent + paragraphLayoutInfo.ListTab) : 0f));
			}
			else if (owner is WTable)
			{
				result = dc.GetCellWidth(this);
			}
			else if (owner is Shape)
			{
				result = (owner as Shape).TextLayoutingBounds.Width;
			}
			else if (owner is WTextBox)
			{
				result = (owner as WTextBox).TextLayoutingBounds.Width;
			}
			else if (owner is ChildShape)
			{
				result = (owner as ChildShape).TextLayoutingBounds.Width;
			}
		}
		return result;
	}

	ISplitLeafWidget[] ISplitLeafWidget.SplitBySize(DrawingContext dc, SizeF offset, float clientWidth, float clientActiveAreaWidth, ref bool isLastWordFit, bool isTabStopInterSectingfloattingItem, bool isSplitByCharacter, bool isFirstItemInLine, ref int countForConsecutivelimit, Layouter layouter, ref bool isHyphenated)
	{
		return SplitStringWidget.SplitBySize(dc, offset.Width, this, null, clientWidth, clientActiveAreaWidth, ref isLastWordFit, isTabStopInterSectingfloattingItem, isSplitByCharacter, isFirstItemInLine, ref countForConsecutivelimit, layouter, ref isHyphenated);
	}

	SizeF ILeafWidget.Measure(DrawingContext dc)
	{
		return ((IWidget)this).LayoutInfo.Size;
	}

	private bool IsLastTextRangeWithSpace(string text)
	{
		text = text.Trim(ControlChar.SpaceChar);
		if (text == string.Empty && !(((IWidget)this).LayoutInfo is TabsLayoutInfo))
		{
			Entity entity = base.NextSibling as Entity;
			if (entity == null && base.Owner is InlineContentControl)
			{
				entity = base.Owner.NextSibling as Entity;
			}
			while ((entity is WTextRange && (entity as WTextRange).Text.Trim(ControlChar.SpaceChar) == string.Empty && !((entity as WTextRange).m_layoutInfo is TabsLayoutInfo) && !(entity is WField)) || entity is BookmarkStart || entity is BookmarkEnd || entity is WFieldMark)
			{
				WParagraph ownerParagraphValue = GetOwnerParagraphValue();
				entity = ((ownerParagraphValue != null) ? (ownerParagraphValue.GetNextSibling(entity as IWidget) as Entity) : null);
				if (entity == null)
				{
					break;
				}
			}
			if (entity == null)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	private bool IsSpaceWidthSetToZero(WParagraph ownerPara, string text)
	{
		bool flag = IsLastTextRangeWithSpace(text);
		if (ownerPara == null || !ownerPara.ParagraphFormat.Bidi)
		{
			return flag;
		}
		if (!CharacterFormat.Bidi)
		{
			return ownerPara.Text.Trim(ControlChar.SpaceChar).Equals(string.Empty) && flag;
		}
		return flag;
	}
}

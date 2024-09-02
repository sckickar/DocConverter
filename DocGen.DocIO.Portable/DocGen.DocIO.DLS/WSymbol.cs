using System.Text;
using DocGen.DocIO.DLS.Rendering;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;
using DocGen.Office;

namespace DocGen.DocIO.DLS;

public class WSymbol : ParagraphItem, ILeafWidget, IWidget
{
	private string m_fontName = "Symbol";

	private byte m_charCode;

	private byte m_charCodeExt;

	private string m_charValue = string.Empty;

	public override EntityType EntityType => EntityType.Symbol;

	public WCharacterFormat CharacterFormat => m_charFormat;

	public string FontName
	{
		get
		{
			return m_fontName;
		}
		set
		{
			m_fontName = value;
		}
	}

	public byte CharacterCode
	{
		get
		{
			return m_charCode;
		}
		set
		{
			m_charCode = value;
		}
	}

	internal byte CharCodeExt
	{
		get
		{
			return m_charCodeExt;
		}
		set
		{
			m_charCodeExt = value;
		}
	}

	internal string CharValue => ((CharCodeExt != 240) ? "00" : "F0") + CharacterCode.ToString("X2");

	public WSymbol(IWordDocument doc)
		: base((WordDocument)doc)
	{
		m_charFormat = new WCharacterFormat(doc, this);
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo();
		m_layoutInfo.IsSkip = false;
		string text = char.ConvertFromUtf32(CharacterCode);
		WParagraph ownerParagraphValue = GetOwnerParagraphValue();
		if (ownerParagraphValue.IsInCell || (ownerParagraphValue.IsNeedToFitSymbol(ownerParagraphValue) && ((IWidget)ownerParagraphValue).LayoutInfo.IsClipped))
		{
			m_layoutInfo.IsClipped = true;
		}
		m_layoutInfo.IsVerticalText = ((IWidget)ownerParagraphValue).LayoutInfo.IsVerticalText;
		if (!CharacterFormat.HasValue(0) && FontName != string.Empty && FontName != CharacterFormat.FontName)
		{
			WCharacterFormat wCharacterFormat = new WCharacterFormat(base.Document);
			wCharacterFormat.ImportContainer(CharacterFormat);
			wCharacterFormat.CopyProperties(CharacterFormat);
			wCharacterFormat.ApplyBase(ownerParagraphValue.BreakCharacterFormat.BaseFormat);
			wCharacterFormat.FontName = FontName;
			m_layoutInfo.Font = new SyncFont(DocumentLayouter.DrawingContext.GetFont(FontScriptType.English, wCharacterFormat, text));
		}
		else
		{
			m_layoutInfo.Font = new SyncFont(DocumentLayouter.DrawingContext.GetFont(FontScriptType.English, CharacterFormat, text));
		}
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}

	protected override object CloneImpl()
	{
		return (WSymbol)base.CloneImpl();
	}

	protected override void InitXDLSHolder()
	{
		base.InitXDLSHolder();
		base.XDLSHolder.AddElement("character-format", m_charFormat);
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("type", ParagraphItemType.Symbol);
		writer.WriteValue("FontName", FontName);
		writer.WriteValue("CharCode", CharacterCode);
		writer.WriteValue("CharCodeExt", CharCodeExt);
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		if (reader.HasAttribute("FontName"))
		{
			FontName = reader.ReadString("FontName");
		}
		if (reader.HasAttribute("CharCode"))
		{
			CharacterCode = reader.ReadByte("CharCode");
		}
		if (reader.HasAttribute("CharCodeExt"))
		{
			CharCodeExt = reader.ReadByte("CharCodeExt");
		}
	}

	SizeF ILeafWidget.Measure(DrawingContext dc)
	{
		string text = char.ConvertFromUtf32(CharacterCode);
		WCharacterFormat wCharacterFormat = new WCharacterFormat(base.Document);
		if (!CharacterFormat.HasValue(0) && FontName != string.Empty && FontName != CharacterFormat.FontName)
		{
			wCharacterFormat.ImportContainer(CharacterFormat);
			wCharacterFormat.CopyProperties(CharacterFormat);
			WParagraph ownerParagraphValue = GetOwnerParagraphValue();
			wCharacterFormat.ApplyBase(ownerParagraphValue.BreakCharacterFormat.BaseFormat);
			wCharacterFormat.FontName = FontName;
			return dc.MeasureString(text, dc.GetFont(FontScriptType.English, wCharacterFormat, text), null, wCharacterFormat, isMeasureFromTabList: false, FontScriptType.English);
		}
		return dc.MeasureString(text, dc.GetFont(FontScriptType.English, CharacterFormat, text), null, CharacterFormat, isMeasureFromTabList: false, FontScriptType.English);
	}

	internal DocGen.Drawing.Font GetFont(DrawingContext dc)
	{
		if (!CharacterFormat.HasValue(0) && FontName != string.Empty && FontName != CharacterFormat.FontName)
		{
			return base.Document.FontSettings.GetFont(FontName, CharacterFormat.FontSize, CharacterFormat.GetFontToRender(FontScriptType.English).Style, FontScriptType.English);
		}
		return CharacterFormat.GetFontToRender(FontScriptType.English);
	}

	void IWidget.InitLayoutInfo()
	{
		m_layoutInfo = null;
	}

	void IWidget.InitLayoutInfo(IWidget widget)
	{
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('\u0017');
		stringBuilder.Append(GetProperties());
		stringBuilder.Append('\u0017');
		return stringBuilder;
	}

	internal StringBuilder GetProperties()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(CharacterCode + ";");
		stringBuilder.Append(CharCodeExt + ";");
		stringBuilder.Append(CharValue + ";");
		stringBuilder.Append(FontName + ";");
		return stringBuilder;
	}

	internal bool Compare(WSymbol symbol)
	{
		if (CharacterCode != symbol.CharacterCode || CharCodeExt != symbol.CharCodeExt || CharValue != symbol.CharValue || FontName != symbol.FontName)
		{
			return false;
		}
		return true;
	}
}

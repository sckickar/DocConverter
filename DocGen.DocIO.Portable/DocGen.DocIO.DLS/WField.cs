using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DocGen.DocIO.DLS.Rendering;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;
using DocGen.Office;

namespace DocGen.DocIO.DLS;

public class WField : WTextRange, IWField, IWTextRange, IParagraphItem, IEntity, ILeafWidget, IWidget
{
	internal enum Month
	{
		January = 1,
		February,
		March,
		April,
		May,
		June,
		July,
		August,
		September,
		October,
		November,
		December
	}

	private const char COMMASEPARATOR = ',';

	private const char OPENPARENTHESIS = '(';

	private const char CLOSEPARENTHESIS = ')';

	private const string PARAGRAPHMARK = "\r";

	private const string CELLMARK = "\a";

	private const string ROWMARK = "\r\a";

	private const char TableStartMark = '\u0013';

	private const char TableEndMark = '\u0015';

	private const float DefaultIntegralSymbolSize = 17.726074f;

	private string m_fieldPattern = "{0}";

	protected FieldType m_fieldType;

	protected bool m_bConvertedToText;

	private bool m_bIsLocal;

	private bool m_hasInnerPageField;

	private bool m_isNestedField;

	protected ParagraphItemType m_paraItemType;

	protected internal string m_formattingString = string.Empty;

	protected internal string m_fieldValue = string.Empty;

	protected TextFormat m_textFormat;

	private string m_localRef;

	private short m_sourceFldType;

	private short m_bFlags;

	private Range m_range;

	private string m_fieldResult = string.Empty;

	internal Stack<WField> m_nestedFields = new Stack<WField>();

	private WFieldMark m_fieldSeparator;

	private WFieldMark m_fieldEnd;

	internal string m_detachedFieldCode = string.Empty;

	internal string m_currentPageNumber;

	private WCharacterFormat m_resultFormat;

	private WField originalField;

	private Stack<Entity> entities;

	private List<WField> nestedFields;

	private Stack<Dictionary<string, object>> m_unlinkNestedFieldStack = new Stack<Dictionary<string, object>>();

	private string m_screenTip;

	private List<string> functions;

	internal const char FieldAscii = '\u0013';

	public TextFormat TextFormat
	{
		get
		{
			return m_textFormat;
		}
		set
		{
			TextFormat textFormat = m_textFormat;
			m_textFormat = value;
			if (!base.Document.IsOpening && textFormat != m_textFormat)
			{
				FieldCode = UpdateTextFormatSwitchString(m_textFormat);
			}
		}
	}

	public override EntityType EntityType => EntityType.Field;

	public string FieldPattern
	{
		get
		{
			return m_fieldPattern;
		}
		set
		{
			m_fieldPattern = value;
		}
	}

	public string FieldValue => m_fieldValue;

	public FieldType FieldType
	{
		get
		{
			return m_fieldType;
		}
		set
		{
			FieldType fieldType = m_fieldType;
			m_fieldType = value;
			if (fieldType == FieldType.FieldUnknown && (value == FieldType.FieldAutoNum || value == FieldType.FieldAutoNumLegal) && !base.Document.Fields.SortedAutoNumFields.Contains(this))
			{
				base.Document.Fields.InsertAutoNumFieldInAsc(this);
			}
			if (base.Document.IsOpening || fieldType == value)
			{
				return;
			}
			if (FieldEnd != null)
			{
				RemovePreviousFieldCode();
			}
			if (base.OwnerParagraph != null && base.NextSibling is WTextRange)
			{
				WTextRange wTextRange = base.NextSibling as WTextRange;
				wTextRange.Text = FieldTypeDefiner.GetFieldCode(m_fieldType);
				if (this is WMergeField && !string.IsNullOrEmpty((this as WMergeField).FieldName))
				{
					wTextRange.Text = wTextRange.Text + " " + (this as WMergeField).FieldName;
				}
				UpdateFieldCode(wTextRange.Text);
			}
		}
	}

	internal bool IsDirty
	{
		get
		{
			return (m_bFlags & 0x800) >> 11 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xF7FF) | (int)((value ? 1u : 0u) << 11));
		}
	}

	internal bool IsLocked
	{
		get
		{
			return (m_bFlags & 0x1000) >> 12 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xEFFF) | (int)((value ? 1u : 0u) << 12));
		}
	}

	internal bool IsLocal
	{
		get
		{
			return m_bIsLocal;
		}
		set
		{
			m_bIsLocal = value;
			SetLocalSwitchString();
		}
	}

	internal bool HasInnerPageField
	{
		get
		{
			return m_hasInnerPageField;
		}
		set
		{
			m_hasInnerPageField = value;
		}
	}

	internal bool IsNestedField
	{
		get
		{
			return m_isNestedField;
		}
		set
		{
			m_isNestedField = value;
		}
	}

	internal string FormattingString
	{
		get
		{
			return m_formattingString;
		}
		set
		{
			m_formattingString = value;
		}
	}

	internal string LocalReference
	{
		get
		{
			return m_localRef;
		}
		set
		{
			m_localRef = value;
		}
	}

	internal string ScreenTip
	{
		get
		{
			return m_screenTip;
		}
		set
		{
			m_screenTip = value;
		}
	}

	public string FieldCode
	{
		get
		{
			if (IsFormField())
			{
				return FieldTypeDefiner.GetFieldCode(FieldType);
			}
			if (!base.Document.IsInternalManipulation() && base.Owner == null)
			{
				return m_detachedFieldCode;
			}
			return UpdateNestedFieldCode(isUpdateNestedFields: true, null);
		}
		set
		{
			if (IsFormField())
			{
				return;
			}
			if (!base.Document.IsInternalManipulation() && base.Owner == null)
			{
				m_detachedFieldCode = value;
				return;
			}
			string fieldCode = FieldCode;
			string text = value;
			string text2 = text.ToUpper();
			if (text2.Contains("\\* FUSIONFORMAT"))
			{
				int startIndex = text2.IndexOf("\\* FUSIONFORMAT");
				text = text.Remove(startIndex, "\\* FUSIONFORMAT".Length);
				text = text.Insert(startIndex, "\\* MERGEFORMAT");
			}
			if (text2.Contains("NBPAGES") && m_fieldType == FieldType.FieldNumPages)
			{
				int startIndex2 = text2.IndexOf("NBPAGES");
				text = text.Remove(startIndex2, "NBPAGES".Length);
				text = text.Insert(startIndex2, "NUMPAGES");
			}
			if (!base.Document.IsOpening && fieldCode != text && base.OwnerParagraph != null)
			{
				if (FieldEnd != null)
				{
					RemovePreviousFieldCode();
				}
				m_fieldType = FieldTypeDefiner.GetFieldType(text);
				WTextRange wTextRange = new WTextRange(m_doc);
				wTextRange.ApplyCharacterFormat(base.CharacterFormat);
				wTextRange.Text = text;
				if (wTextRange.CharacterFormat.PropertiesHash.ContainsKey(106))
				{
					wTextRange.CharacterFormat.PropertiesHash.Remove(106);
				}
				base.OwnerParagraph.Items.Insert(Index + 1, wTextRange);
				UpdateFieldCode(text);
				if (!base.Document.IsMailMerge && FieldType == FieldType.FieldMergeField && FieldEnd != null && base.Document.Settings.UpdateResultOnFieldCodeChange)
				{
					(this as WMergeField).UpdateMergeFieldResult();
				}
				if (!base.Document.IsMailMerge && FieldType == FieldType.FieldSequence && FieldEnd != null && base.Document.Settings.UpdateResultOnFieldCodeChange)
				{
					(this as WSeqField).UpdateSequenceFieldResult();
				}
			}
		}
	}

	internal string InternalFieldCode
	{
		get
		{
			if (base.NextSibling is WTextRange)
			{
				return (base.NextSibling as WTextRange).Text;
			}
			return string.Empty;
		}
	}

	internal short SourceFieldType
	{
		get
		{
			return m_sourceFldType;
		}
		set
		{
			m_sourceFldType = value;
		}
	}

	internal string FieldResult
	{
		get
		{
			return m_fieldResult;
		}
		set
		{
			m_fieldResult = value;
		}
	}

	internal Range Range
	{
		get
		{
			if (m_range == null)
			{
				m_range = new Range(base.Document, this);
			}
			else if (!IsFieldRangeUpdated && !base.Document.IsOpening && !base.Document.IsCloning)
			{
				m_range.Items.Clear();
			}
			if (!IsFieldRangeUpdated && !base.Document.IsOpening && !base.Document.IsCloning)
			{
				UpdateFieldRange();
			}
			return m_range;
		}
	}

	internal WFieldMark FieldSeparator
	{
		get
		{
			return m_fieldSeparator;
		}
		set
		{
			m_fieldSeparator = value;
			if (value != null)
			{
				m_fieldSeparator.ParentField = this;
			}
		}
	}

	internal WFieldMark FieldEnd
	{
		get
		{
			return m_fieldEnd;
		}
		set
		{
			m_fieldEnd = value;
			if (value != null)
			{
				m_fieldEnd.ParentField = this;
			}
		}
	}

	internal new bool IsCloned
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFFFE) | (value ? 1 : 0));
		}
	}

	internal bool IsAdded
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFFFD) | (int)((value ? 1u : 0u) << 1));
		}
	}

	internal bool IsFieldRangeUpdated
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFFFB) | (int)((value ? 1u : 0u) << 2));
		}
	}

	internal bool IsFieldSeparator
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFFF7) | (int)((value ? 1u : 0u) << 3));
		}
	}

	internal bool IsSkip
	{
		get
		{
			return (m_bFlags & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFFEF) | (int)((value ? 1u : 0u) << 4));
		}
	}

	internal bool IsUpdated
	{
		get
		{
			return (m_bFlags & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFFDF) | (int)((value ? 1u : 0u) << 5));
		}
	}

	internal bool IsInFieldResult
	{
		get
		{
			return (m_bFlags & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFFBF) | (int)((value ? 1u : 0u) << 6));
		}
	}

	internal bool IsPgNum
	{
		get
		{
			return (m_bFlags & 0x400) >> 10 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFBFF) | (int)((value ? 1u : 0u) << 10));
		}
	}

	internal bool IsNumPagesInsideExpressionField
	{
		get
		{
			return (m_bFlags & 0x80) >> 7 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFF7F) | (int)((value ? 1u : 0u) << 7));
		}
	}

	internal bool IsNumPageUsedForEvaluation
	{
		get
		{
			return (m_bFlags & 0x100) >> 8 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFEFF) | (int)((value ? 1u : 0u) << 8));
		}
	}

	internal bool IsFieldInsideUnknownField
	{
		get
		{
			return (m_bFlags & 0x200) >> 9 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFDFF) | (int)((value ? 1u : 0u) << 9));
		}
	}

	internal WCharacterFormat ResultFormat => m_resultFormat;

	public override string Text
	{
		get
		{
			if (FieldType == FieldType.FieldAutoNum || FieldType == FieldType.FieldAutoNumLegal)
			{
				return GetAutoNumFieldValue(this);
			}
			return GetFieldResultValue();
		}
		set
		{
			if (!base.Document.IsOpening && !base.Document.IsCloning && !base.Document.IsMailMerge && !IsCloned && FieldType != FieldType.FieldAutoNum && FieldType != FieldType.FieldAutoNumLegal)
			{
				if (IsFieldWithoutSeparator)
				{
					throw new Exception("Field Result is not available for this field ");
				}
				UpdateFieldResult(value);
			}
		}
	}

	internal bool IsFieldWithoutSeparator => CheckFieldWithoutSeparator();

	internal WField OriginalField
	{
		get
		{
			return originalField;
		}
		set
		{
			originalField = value;
		}
	}

	internal List<string> Functions
	{
		get
		{
			if (functions == null)
			{
				functions = new List<string>(new string[18]
				{
					"product", "sum", "average", "mod", "abs", "int", "round", "sign", "count", "defined",
					"or", "and", "not", "max", "min", "true", "false", "if"
				});
			}
			return functions;
		}
	}

	ILayoutInfo IWidget.LayoutInfo
	{
		get
		{
			if (m_layoutInfo == null)
			{
				CreateLayoutInfo();
			}
			return m_layoutInfo;
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
		}
	}

	public WField(IWordDocument doc)
		: base(doc)
	{
		m_paraItemType = ParagraphItemType.Field;
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		m_fieldType = (FieldType)(object)reader.ReadEnum("FieldType", typeof(FieldType));
		m_bConvertedToText = reader.ReadBoolean("ConvertedToText");
		if (reader.HasAttribute("TextFormat"))
		{
			m_textFormat = (TextFormat)(object)reader.ReadEnum("TextFormat", typeof(TextFormat));
		}
		if (reader.HasAttribute("IsLocal"))
		{
			m_bIsLocal = reader.ReadBoolean("IsLocal");
		}
		if (reader.HasAttribute("FieldFormatting"))
		{
			m_formattingString = reader.ReadString("FieldFormatting");
		}
		if (reader.HasAttribute("FieldValue"))
		{
			m_fieldValue = reader.ReadString("FieldValue");
		}
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("type", m_paraItemType);
		writer.WriteValue("FieldType", FieldType);
		writer.WriteValue("TextFormat", m_textFormat);
		if (m_bIsLocal)
		{
			writer.WriteValue("IsLocal", m_bIsLocal);
		}
		if (m_formattingString != string.Empty)
		{
			writer.WriteValue("FieldFormatting", m_formattingString);
		}
		if (m_fieldValue != null && m_fieldValue != "")
		{
			writer.WriteValue("FieldValue", m_fieldValue);
		}
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new FieldLayoutInfo(ChildrenLayoutDirection.Horizontal);
		if (FieldType == FieldType.FieldNumPages || FieldType == FieldType.FieldPage || FieldType == FieldType.FieldSectionPages || FieldType == FieldType.FieldAutoNum)
		{
			m_layoutInfo.Font = new SyncFont(GetCharacterFormatValue().GetFontToRender(GetFontScriptType()));
		}
		else
		{
			m_layoutInfo.Font = new SyncFont(GetCharFormat().GetFontToRender(base.ScriptType));
		}
		if (base.CharacterFormat.Hidden)
		{
			m_layoutInfo.IsSkip = true;
		}
		if ((!DocumentLayouter.IsUpdatingTOC || FieldType != FieldType.FieldTOCEntry) && (FieldSeparator == null || FieldSeparator.Owner == null))
		{
			if (FieldType != FieldType.FieldFormCheckBox && FieldType != FieldType.FieldFormDropDown && FieldType != FieldType.FieldSymbol && FieldType != FieldType.FieldExpression && FieldType != FieldType.FieldAutoNum && FieldType != FieldType.FieldAutoNumLegal && ((FieldType != FieldType.FieldPage && FieldType != FieldType.FieldNumPages && FieldType != FieldType.FieldSectionPages) || !DocumentLayouter.IsLayoutingHeaderFooter))
			{
				m_layoutInfo.IsSkip = true;
			}
			if (FieldType == FieldType.FieldMacroButton)
			{
				SkipMacroButtonFieldCode();
			}
			else
			{
				SkipLayoutingOfFieldCode();
			}
			return;
		}
		if (FieldSeparator != null)
		{
			if (FieldType != FieldType.FieldPage && FieldType != FieldType.FieldNumPages && FieldType != FieldType.FieldSectionPages && FieldType != FieldType.FieldTOCEntry && FieldType != FieldType.FieldPageRef && FieldType != FieldType.FieldFormCheckBox && FieldType != FieldType.FieldExpression && FieldType != FieldType.FieldFormDropDown && FieldType != FieldType.FieldHyperlink && FieldType != FieldType.FieldRef && FieldType != FieldType.FieldSymbol)
			{
				m_layoutInfo.IsSkip = true;
			}
			if (base.Owner is InlineContentControl && FieldEnd != null)
			{
				SkipLayoutingFieldItems(isFieldCode: true);
			}
			else
			{
				SkipLayoutingOfFieldCode();
			}
			if (IsSkipFieldResult())
			{
				SkipLayoutingFieldItems(isFieldCode: false);
			}
		}
		if (!IsNumPagesInsideExpressionField && DocumentLayouter.IsLayoutingHeaderFooter && !IsFieldInsideUnknownField && (FieldType == FieldType.FieldIf || FieldType == FieldType.FieldCompare || FieldType == FieldType.FieldFormula) && !IsNestedField)
		{
			Update();
			SkipLayoutingOfFieldCode();
		}
	}

	internal bool IsSkipFieldResult()
	{
		if (FieldEnd != null)
		{
			if (FieldType != FieldType.FieldPage && FieldType != FieldType.FieldAutoNum && FieldType != FieldType.FieldAutoNumLegal && FieldType != FieldType.FieldSet && FieldType != FieldType.FieldSymbol && (FieldType != FieldType.FieldUnknown || !IsInvalidFieldCode()) && (FieldType != FieldType.FieldNumPages || !DocumentLayouter.IsFirstLayouting))
			{
				if (FieldType == FieldType.FieldSectionPages)
				{
					return DocumentLayouter.IsFirstLayouting;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private bool HasNestedField()
	{
		for (int i = 0; i < Range.Items.Count; i++)
		{
			if ((Range.Items[i] as Entity) is WField)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsInvalidFieldCode()
	{
		string text = UpdateNestedFieldCode(isUpdateNestedFields: false, null).Trim();
		if (string.IsNullOrEmpty(text))
		{
			return true;
		}
		if (text.Contains("(") || text.Contains(")"))
		{
			return true;
		}
		if (!HasNestedField())
		{
			string text2 = text.Split(' ')[0];
			foreach (char c in text2)
			{
				if (c != '_' && c != '"' && !char.IsLetterOrDigit(c))
				{
					return true;
				}
			}
		}
		char c2 = text[0];
		if (c2 == ControlChar.DoubleQuote && text.Length > 1)
		{
			c2 = text[1];
		}
		return !char.IsLetter(c2);
	}

	internal void SetSkipForFieldCode(IEntity entity)
	{
		while (entity != null && entity != FieldSeparator)
		{
			(entity as IWidget).LayoutInfo.IsSkip = true;
			entity = entity.NextSibling;
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

	internal void SetFieldTypeValue(FieldType fieldType)
	{
		m_fieldType = fieldType;
	}

	protected internal virtual void ParseFieldCode(string fieldCode)
	{
		if (!(InternalFieldCode == "") || FieldType != FieldType.FieldFormTextInput)
		{
			m_fieldType = FieldTypeDefiner.GetFieldType(fieldCode);
		}
		UpdateFieldCode(fieldCode);
	}

	protected internal virtual void UpdateFieldCode(string fieldCode)
	{
		switch (m_fieldType)
		{
		case FieldType.FieldHyperlink:
		{
			fieldCode = fieldCode.Trim();
			Match match2 = new Regex("HYPERLINK\\s+(\\\\l\\s+)?[\"]?([^\"]+)(\"| )").Match(fieldCode);
			if (match2.Groups[2].Value == string.Empty)
			{
				m_fieldValue = fieldCode.Replace("HYPERLINK", string.Empty);
			}
			else if (match2.Groups[2].Value == "\\l")
			{
				m_fieldValue = fieldCode.Replace("HYPERLINK", string.Empty);
				m_fieldValue = m_fieldValue.Replace("\\l", string.Empty);
			}
			else
			{
				m_fieldValue = "\"" + match2.Groups[2].Value + "\"";
			}
			int num = fieldCode.IndexOf("\\o");
			if (num != -1)
			{
				SetScreenTipAndPositionSwitch(fieldCode, num, "\\o ");
			}
			int num2 = fieldCode.IndexOf("\\t");
			if (num2 != -1)
			{
				SetScreenTipAndPositionSwitch(fieldCode, num2, "\\t ");
			}
			int num3 = fieldCode.IndexOf("\\l");
			if (match2.Groups[1].Length > 0 || num3 != -1)
			{
				m_bIsLocal = true;
				SetLocalSwitchString();
				if (fieldCode.IndexOf(m_fieldValue) < num3)
				{
					ParseLocalRef(fieldCode, num3);
				}
			}
			break;
		}
		case FieldType.FieldIncludePicture:
		{
			fieldCode = fieldCode.Trim();
			Match match2 = new Regex("INCLUDEPICTURE\\s+\"([^\"]+)\"(?<Options>.*)").Match(fieldCode);
			m_fieldValue = "\"" + match2.Groups[1].Value + "\"";
			m_formattingString = match2.Groups[2].Value;
			break;
		}
		case FieldType.FieldTOC:
		{
			fieldCode = fieldCode.Trim();
			Match match2 = new Regex("(TOC\\s+)(?<Options>.*)").Match(fieldCode);
			m_formattingString = match2.Groups["Options"].Value;
			break;
		}
		case FieldType.FieldPageRef:
		{
			fieldCode = fieldCode.Trim();
			Match match2 = new Regex("(\\w+)\\s+\"?([^:\"]+):?([^\"]*)\"?").Match(fieldCode);
			m_fieldValue = match2.Groups[2].Value;
			break;
		}
		case FieldType.FieldFormula:
			fieldCode = fieldCode.Trim();
			fieldCode = fieldCode.Replace("=", string.Empty);
			m_fieldValue = fieldCode;
			break;
		case FieldType.FieldLink:
			fieldCode = fieldCode.Trim();
			fieldCode = fieldCode.Replace("LINK ", string.Empty);
			m_fieldValue = fieldCode;
			break;
		case FieldType.FieldFillIn:
		{
			fieldCode = fieldCode.Trim();
			Match match = new Regex("(\\w+)\\s+\"?([^:\"]+)?([^\"]*)\"?").Match(fieldCode);
			int i = 2;
			for (int count = match.Groups.Count; i < count; i++)
			{
				if (match.Groups[i].Length > 0)
				{
					m_fieldValue += match.Groups[i].Value;
				}
			}
			break;
		}
		default:
			ParseField(fieldCode);
			break;
		}
	}

	private bool CheckFieldWithoutSeparator()
	{
		switch (FieldType)
		{
		case FieldType.FieldIndexEntry:
		case FieldType.FieldTOCEntry:
		case FieldType.FieldRefDoc:
		case FieldType.FieldTOAEntry:
		case FieldType.FieldPrivate:
			return true;
		default:
			return false;
		}
	}

	internal bool AddFormattingString()
	{
		if (m_fieldType != FieldType.FieldDocVariable && m_fieldType != FieldType.FieldTitle)
		{
			return m_fieldType == FieldType.FieldSubject;
		}
		return true;
	}

	internal string FindFieldCode()
	{
		return FieldTypeDefiner.GetFieldCode(FieldType) + " ";
	}

	private void SetTextFormatSwitchString()
	{
		m_formattingString = m_formattingString.Replace("\\* Upper", string.Empty);
		m_formattingString = m_formattingString.Replace("\\* Lower", string.Empty);
		m_formattingString = m_formattingString.Replace("\\* FirstCap", string.Empty);
		m_formattingString = m_formattingString.Replace("\\* Caps", string.Empty);
		switch (m_textFormat)
		{
		case TextFormat.Uppercase:
			m_formattingString += " \\* Upper ";
			break;
		case TextFormat.Lowercase:
			m_formattingString += " \\* Lower ";
			break;
		case TextFormat.FirstCapital:
			m_formattingString += " \\* FirstCap ";
			break;
		case TextFormat.Titlecase:
			m_formattingString += " \\* Caps ";
			break;
		}
	}

	private string UpdateTextFormatSwitchString(TextFormat newType)
	{
		string text = GetFieldCodeForUnknownFieldType();
		int num = -1;
		if (text.Contains("\\* Upper"))
		{
			num = text.IndexOf("\\* Upper");
			text = text.Replace("\\* Upper", string.Empty);
		}
		if (text.Contains("\\* Lower"))
		{
			num = text.IndexOf("\\* Lower");
			text = text.Replace("\\* Lower", string.Empty);
		}
		if (text.Contains("\\* FirstCap"))
		{
			num = text.IndexOf("\\* FirstCap");
			text = text.Replace("\\* FirstCap", string.Empty);
		}
		if (text.Contains("\\* Caps"))
		{
			num = text.IndexOf("\\* Caps");
			text = text.Replace("\\* Caps", string.Empty);
		}
		if (num == -1)
		{
			num = text.IndexOf("\\* MERGEFORMAT");
		}
		switch (newType)
		{
		case TextFormat.Uppercase:
			text = ((num == -1) ? (text + " \\* Upper ") : text.Insert(num, " \\* Upper "));
			break;
		case TextFormat.Lowercase:
			text = ((num == -1) ? (text + " \\* Lower ") : text.Insert(num, " \\* Lower "));
			break;
		case TextFormat.FirstCapital:
			text = ((num == -1) ? (text + " \\* FirstCap ") : text.Insert(num, " \\* FirstCap "));
			break;
		case TextFormat.Titlecase:
			text = ((num == -1) ? (text + " \\* Caps ") : text.Insert(num, " \\* Caps "));
			break;
		}
		return text;
	}

	private void SetLocalSwitchString()
	{
		m_formattingString = m_formattingString.Replace(" \\l", string.Empty);
		if (IsLocal)
		{
			m_formattingString += " \\l";
		}
	}

	private void SetScreenTipAndPositionSwitch(string fieldCode, int switchStartIndex, string formattingSwitch)
	{
		switchStartIndex += 2;
		string text = fieldCode.Substring(switchStartIndex, fieldCode.Length - switchStartIndex).Trim();
		if (StartsWithExt(text.TrimStart(), "\""))
		{
			m_formattingString += formattingSwitch;
			string[] array = text.Split(new char[1] { '"' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length != 0)
			{
				m_formattingString = m_formattingString + "\"" + array[0] + "\"";
				m_screenTip = ((formattingSwitch == "\\o ") ? array[0] : m_screenTip);
			}
		}
	}

	protected internal virtual string ConvertSwitchesToString()
	{
		SetTextFormatSwitchString();
		SetLocalSwitchString();
		return m_formattingString;
	}

	internal override void AttachToParagraph(WParagraph owner, int itemPos)
	{
		Attach(owner, itemPos, isField: true);
		if (!base.DeepDetached)
		{
			base.Document.Fields.Add(this);
			IsCloned = false;
		}
		else
		{
			IsCloned = true;
		}
	}

	internal override void Detach()
	{
		if (FieldEnd != null && FieldEnd.Owner is WParagraph && !base.Document.IsSkipFieldDetach && base.Owner is WParagraph && !base.OwnerParagraph.Document.IsClosing)
		{
			bool isFoundFieldEnd = false;
			RemoveItemsUptoFieldEnd(base.OwnerParagraph, GetIndexInOwnerCollection() + 1, ref isFoundFieldEnd);
			if (base.OwnerParagraph.OwnerTextBody != null && FieldEnd.OwnerParagraph != null && FieldEnd.OwnerParagraph.OwnerTextBody != null && base.OwnerParagraph.OwnerTextBody == FieldEnd.OwnerParagraph.OwnerTextBody && base.OwnerParagraph != FieldEnd.OwnerParagraph)
			{
				int num;
				for (num = base.OwnerParagraph.GetIndexInOwnerCollection() + 1; num < base.OwnerParagraph.OwnerTextBody.Items.Count; num++)
				{
					Entity entity = base.OwnerParagraph.OwnerTextBody.Items[num];
					if (entity == FieldEnd.OwnerParagraph)
					{
						WParagraph ownerParagraph = FieldEnd.OwnerParagraph;
						RemoveItemsUptoFieldEnd(ownerParagraph, 0, ref isFoundFieldEnd);
						while (ownerParagraph.ChildEntities.Count > 0)
						{
							base.OwnerParagraph.ChildEntities.Add(ownerParagraph.ChildEntities[0]);
						}
						base.OwnerParagraph.ParagraphFormat.CopyFormat(ownerParagraph.ParagraphFormat);
						base.OwnerParagraph.BreakCharacterFormat.CopyFormat(ownerParagraph.BreakCharacterFormat);
						ownerParagraph.RemoveSelf();
						break;
					}
					base.OwnerParagraph.OwnerTextBody.Items.Remove(entity);
					num--;
				}
			}
		}
		else if (FieldEnd != null && (base.Owner is InlineContentControl || FieldEnd.Owner is InlineContentControl))
		{
			WParagraph ownerParagraphValue = GetOwnerParagraphValue();
			WParagraph wParagraph = ((FieldEnd != null) ? FieldEnd.GetOwnerParagraphValue() : null);
			if (FieldEnd != null && wParagraph != null && !base.Document.IsSkipFieldDetach && !ownerParagraphValue.Document.IsClosing)
			{
				RemoveUptoFieldEndInInlineControl(ownerParagraphValue, wParagraph);
			}
		}
		Detach(isField: true);
		if (!base.DeepDetached)
		{
			base.Document.Fields.Remove(this);
		}
	}

	private void RemoveUptoFieldEndInInlineControl(WParagraph ownerPara, WParagraph fieldEndOwnerPara)
	{
		bool isFoundFieldEnd = false;
		if (base.Owner is WParagraph)
		{
			RemoveItemsUptoFieldEnd(base.OwnerParagraph, GetIndexInOwnerCollection() + 1, ref isFoundFieldEnd);
		}
		else if (base.Owner is InlineContentControl)
		{
			RemoveItemsUptoFieldEnd(base.Owner as InlineContentControl, GetIndexInOwnerCollection() + 1, ref isFoundFieldEnd);
			if (!isFoundFieldEnd)
			{
				Entity owner = base.Owner;
				while (!(owner is WParagraph) && owner != null && !(owner.Owner is WParagraph))
				{
					owner = owner.Owner;
				}
				RemoveItemsUptoFieldEnd(owner.Owner as WParagraph, owner.Index + 1, ref isFoundFieldEnd);
			}
		}
		if (ownerPara.OwnerTextBody == null || fieldEndOwnerPara == null || fieldEndOwnerPara.OwnerTextBody == null || ownerPara.OwnerTextBody != fieldEndOwnerPara.OwnerTextBody || ownerPara == fieldEndOwnerPara || isFoundFieldEnd)
		{
			return;
		}
		int num = ownerPara.GetIndexInOwnerCollection() + 1;
		while (num < ownerPara.OwnerTextBody.Items.Count && !isFoundFieldEnd)
		{
			Entity entity = ownerPara.OwnerTextBody.Items[num];
			if (entity == fieldEndOwnerPara)
			{
				RemoveItemsUptoFieldEnd(fieldEndOwnerPara, 0, ref isFoundFieldEnd);
				break;
			}
			base.OwnerParagraph.OwnerTextBody.Items.Remove(entity);
			num--;
			num++;
		}
	}

	private void RemoveItemsUptoFieldEnd(WParagraph ownerParagraph, int startItemIndex, ref bool isFoundFieldEnd)
	{
		int num = startItemIndex;
		while (num < ownerParagraph.ChildEntities.Count && !isFoundFieldEnd)
		{
			Entity entity = ownerParagraph.Items[num];
			if (entity is InlineContentControl && FieldEnd.Owner == entity)
			{
				RemoveItemsUptoFieldEnd(entity as InlineContentControl, 0, ref isFoundFieldEnd);
			}
			else
			{
				ownerParagraph.Items.Remove(entity);
			}
			num--;
			if (entity == FieldEnd)
			{
				isFoundFieldEnd = true;
				break;
			}
			num++;
		}
	}

	private void RemoveItemsUptoFieldEnd(InlineContentControl inlineContenControl, int startItemIndex, ref bool isFoundFieldEnd)
	{
		int num = startItemIndex;
		while (num < inlineContenControl.ParagraphItems.Count && !isFoundFieldEnd)
		{
			Entity entity = inlineContenControl.ParagraphItems[num];
			if (entity is InlineContentControl && FieldEnd.Owner == entity)
			{
				RemoveItemsUptoFieldEnd(entity as InlineContentControl, 0, ref isFoundFieldEnd);
			}
			else
			{
				inlineContenControl.ParagraphItems.Remove(entity);
			}
			num--;
			if (entity == FieldEnd)
			{
				isFoundFieldEnd = true;
				break;
			}
			num++;
		}
		if (!isFoundFieldEnd)
		{
			Entity owner = inlineContenControl.Owner;
			if (owner is InlineContentControl)
			{
				RemoveItemsUptoFieldEnd(owner as InlineContentControl, inlineContenControl.GetIndexInOwnerCollection() + 1, ref isFoundFieldEnd);
			}
		}
	}

	internal override void AttachToDocument()
	{
		if (IsCloned)
		{
			WSection wSection = GetOwnerSection(this) as WSection;
			WTextBody wTextBody = GetFieldOwnerTextBody(this) as WTextBody;
			if (wSection == null || wTextBody == null || wTextBody.Owner != wSection || wSection.Body == wTextBody)
			{
				base.Document.Fields.Add(this);
				IsCloned = false;
			}
		}
	}

	private Entity GetFieldOwnerTextBody(Entity entity)
	{
		while (entity != null && !(entity is WSection) && (entity is HeaderFooter || !(entity.Owner is WSection)))
		{
			entity = entity.Owner;
		}
		if (!(entity is WTextBody))
		{
			return null;
		}
		return entity;
	}

	protected override object CloneImpl()
	{
		WField wField = (WField)CloneImpl(isField: true);
		wField.m_range = new Range(base.Document, wField);
		wField.m_bFlags = 1;
		if (FieldType == FieldType.FieldAutoNum || FieldType == FieldType.FieldAutoNumLegal)
		{
			wField.OriginalField = this;
		}
		return wField;
	}

	internal override void Close()
	{
		if (m_range != null)
		{
			m_range.Close();
			m_range = null;
		}
		if (m_nestedFields != null)
		{
			m_nestedFields.Clear();
			m_nestedFields = null;
		}
		m_fieldSeparator = null;
		m_fieldEnd = null;
		if (m_fieldValue != null)
		{
			m_fieldValue = null;
		}
		if (m_fieldPattern != null)
		{
			m_fieldPattern = null;
		}
		if (m_formattingString != null)
		{
			m_formattingString = null;
		}
		if (m_fieldResult != null)
		{
			m_fieldResult = null;
		}
		originalField = null;
		if (functions != null)
		{
			functions.Clear();
			functions = null;
		}
		base.Close();
	}

	internal ParagraphItem GetAsSymbolOrTextRange()
	{
		string[] array = FieldCode.Split('\\');
		string text = string.Empty;
		float num = 0f;
		int result = 0;
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string text2 = array2[i].TrimStart(ControlChar.SpaceChar);
			if (StartsWithExt(text2.ToUpper(), "SYMBOL"))
			{
				string text3 = text2.ToUpper().Replace("SYMBOL", string.Empty).Trim(ControlChar.SpaceChar);
				if (text3.StartsWith("0X"))
				{
					text3 = text3.Replace("0X", string.Empty);
					int.TryParse(text3, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
				}
				else
				{
					int.TryParse(text3, out result);
				}
				string text4 = result.ToString("X");
				if (StartsWithExt(text4, "F0"))
				{
					result -= 61440;
				}
			}
			else if (StartsWithExt(text2, "f"))
			{
				text = text2.Replace("f", string.Empty).Trim(ControlChar.SpaceChar);
				text = text.Trim('"');
			}
			else if (StartsWithExt(text2, "s"))
			{
				num = (float)Convert.ToDouble(text2.Replace("s", string.Empty).Trim(ControlChar.SpaceChar));
			}
		}
		if (result <= 255)
		{
			WSymbol wSymbol = new WSymbol(base.Document);
			wSymbol.SetOwner(base.OwnerParagraph);
			wSymbol.CharacterCode = (byte)result;
			wSymbol.CharacterFormat.ImportContainer(base.CharacterFormat);
			wSymbol.CharacterFormat.CopyProperties(base.CharacterFormat);
			if (base.CharacterFormat.BaseFormat != null)
			{
				wSymbol.CharacterFormat.ApplyBase(base.CharacterFormat.BaseFormat);
			}
			if (num > 0f)
			{
				wSymbol.CharacterFormat.SetPropertyValue(3, num);
			}
			if (!string.IsNullOrEmpty(text))
			{
				wSymbol.FontName = text;
			}
			return wSymbol;
		}
		WTextRange wTextRange = new WTextRange(base.Document);
		base.OwnerParagraph.CloneWithoutItems().ChildEntities.Add(wTextRange);
		wTextRange.CharacterFormat.ImportContainer(base.CharacterFormat);
		wTextRange.CharacterFormat.CopyProperties(base.CharacterFormat);
		if (base.CharacterFormat.BaseFormat != null)
		{
			wTextRange.CharacterFormat.ApplyBase(base.CharacterFormat.BaseFormat);
		}
		if (num > 0f)
		{
			wTextRange.CharacterFormat.SetPropertyValue(3, num);
		}
		if (!string.IsNullOrEmpty(text))
		{
			wTextRange.CharacterFormat.FontName = text;
		}
		wTextRange.Text = Convert.ToString((char)result, CultureInfo.InvariantCulture);
		return wTextRange;
	}

	internal string GetFieldCodeForUnknownFieldType()
	{
		string internalFieldCode = InternalFieldCode;
		if (base.OwnerParagraph == null)
		{
			return internalFieldCode;
		}
		WFieldMark wFieldMark = ((FieldSeparator == null) ? FieldEnd : FieldSeparator);
		if (wFieldMark == null || wFieldMark.OwnerParagraph == null)
		{
			return internalFieldCode;
		}
		IsFieldRangeUpdated = false;
		if (Range.Items.Count == 0)
		{
			UpdateFieldRange();
		}
		internalFieldCode = UpdateNestedFieldCode(isUpdateNestedFields: false, null);
		IsFieldRangeUpdated = false;
		Range.Items.Clear();
		return internalFieldCode;
	}

	internal void UpdateUnknownFieldType(WCharacterFormat resultFormat)
	{
		string fieldCodeForUnknownFieldType = GetFieldCodeForUnknownFieldType();
		if (!(fieldCodeForUnknownFieldType != string.Empty))
		{
			return;
		}
		string text = fieldCodeForUnknownFieldType.Trim();
		FieldType fieldType = FieldTypeDefiner.GetFieldType(text);
		bool flag = true;
		if (fieldType == FieldType)
		{
			if (fieldCodeForUnknownFieldType != InternalFieldCode)
			{
				UpdateFieldCode(fieldCodeForUnknownFieldType);
			}
			if (resultFormat != null)
			{
				ApplyCharacterFormat(resultFormat);
			}
		}
		else if (fieldType != FieldType.FieldUnknown)
		{
			WField wField = null;
			switch (fieldType)
			{
			case FieldType.FieldMergeField:
				wField = new WMergeField(m_doc);
				break;
			case FieldType.FieldFormTextInput:
			case FieldType.FieldFormCheckBox:
			case FieldType.FieldFormDropDown:
				if (StartsWithExt(text.ToUpper(), "TEXTINPUT") || StartsWithExt(text.ToUpper(), "FORMTEXT"))
				{
					wField = new WTextFormField(m_doc);
				}
				else if (StartsWithExt(text.ToUpper(), "DDLIST") || StartsWithExt(text.ToUpper(), "FORMDROPDOWN"))
				{
					wField = new WDropDownFormField(m_doc);
				}
				else if (StartsWithExt(text.ToUpper(), "CHECKBOX") || StartsWithExt(text.ToUpper(), "FORMCHECKBOX"))
				{
					wField = new WCheckBox(m_doc);
				}
				if (wField != null)
				{
					(wField as WFormField).HasFFData = false;
				}
				break;
			case FieldType.FieldIf:
				wField = new WIfField(m_doc);
				break;
			case FieldType.FieldTOC:
				if (m_doc.TOC.Count == 0)
				{
					ParagraphItemCollection items = base.OwnerParagraph.Items;
					int index = Index;
					UpdateFieldCode(fieldCodeForUnknownFieldType);
					TableOfContent tableOfContent = new TableOfContent(m_doc, FormattingString);
					m_doc.TOC.Add(tableOfContent.TOCField, tableOfContent);
					items.Remove(this);
					items.Insert(index, tableOfContent);
					if (resultFormat != null)
					{
						tableOfContent.SetParagraphItemCharacterFormat(resultFormat);
					}
				}
				flag = false;
				break;
			default:
				m_fieldType = fieldType;
				UpdateFieldCode(fieldCodeForUnknownFieldType);
				if (resultFormat != null)
				{
					ApplyCharacterFormat(resultFormat);
				}
				flag = false;
				break;
			}
			if (flag && wField != null)
			{
				ParagraphItemCollection items2 = base.OwnerParagraph.Items;
				int index2 = Index;
				items2.RemoveFromInnerList(index2);
				items2.InsertToInnerList(index2, wField);
				wField.SetOwner(base.OwnerParagraph);
				wField.FieldSeparator = FieldSeparator;
				wField.FieldEnd = FieldEnd;
				if (wField.FieldEnd != null)
				{
					wField.RemovePreviousFieldCode();
				}
				wField.m_fieldType = fieldType;
				WTextRange wTextRange = new WTextRange(m_doc);
				wTextRange.Text = fieldCodeForUnknownFieldType;
				wTextRange.ApplyCharacterFormat(wField.CharacterFormat);
				items2.Insert(index2 + 1, wTextRange);
				wField.UpdateFieldCode(fieldCodeForUnknownFieldType);
				if (resultFormat != null)
				{
					wField.ApplyCharacterFormat(resultFormat);
				}
				else if (base.CharacterFormat != null)
				{
					wField.ApplyCharacterFormat(base.CharacterFormat);
				}
				int num = base.Document.Fields.InnerList.IndexOf(this);
				if (num != -1)
				{
					base.Document.Fields.InnerList.RemoveAt(num);
					base.Document.Fields.InnerList.Insert(num, wField);
				}
			}
		}
		else
		{
			m_fieldType = fieldType;
		}
	}

	internal WField CreateFieldByType(string fieldCode, FieldType fieldType)
	{
		WField wField = null;
		if (fieldType == FieldType.FieldUnknown)
		{
			fieldType = FieldTypeDefiner.GetFieldType(fieldCode.Trim());
		}
		switch (fieldType)
		{
		case FieldType.FieldMergeField:
			wField = new WMergeField(base.Document);
			break;
		case FieldType.FieldSequence:
			wField = new WSeqField(base.Document);
			break;
		case FieldType.FieldEmbed:
			wField = new WEmbedField(base.Document);
			break;
		case FieldType.FieldFormCheckBox:
			wField = new WCheckBox(base.Document);
			base.Document.SetTriggerElement(ref base.Document.m_supportedElementFlag_1, 9);
			break;
		case FieldType.FieldFormTextInput:
			wField = new WTextFormField(base.Document);
			base.Document.SetTriggerElement(ref base.Document.m_supportedElementFlag_2, 14);
			break;
		case FieldType.FieldFormDropDown:
			wField = new WDropDownFormField(base.Document);
			base.Document.SetTriggerElement(ref base.Document.m_supportedElementFlag_1, 15);
			break;
		case FieldType.FieldIf:
			wField = new WIfField(base.Document);
			break;
		case FieldType.FieldOCX:
			wField = new WControlField(base.Document);
			break;
		default:
			if (wField == null)
			{
				wField = new WField(base.Document);
			}
			break;
		}
		if (wField is WFormField)
		{
			(wField as WFormField).HasFFData = false;
		}
		wField.FieldType = fieldType;
		base.Document.SetTriggerElement(ref base.Document.m_supportedElementFlag_1, 18);
		return wField;
	}

	internal bool IsFormField()
	{
		FieldType fieldType = FieldType;
		if (fieldType == FieldType.FieldFormCheckBox || fieldType == FieldType.FieldFormDropDown || fieldType == FieldType.FieldFormTextInput)
		{
			return true;
		}
		return false;
	}

	internal void RemoveFieldCodeItems()
	{
		IsFieldRangeUpdated = false;
		if (Range.Items.Count == 0)
		{
			UpdateFieldRange();
		}
		WFieldMark wFieldMark = ((FieldSeparator == null) ? FieldEnd : FieldSeparator);
		for (int i = 0; i < Range.Items.Count; i++)
		{
			Entity entity = Range.Items[i] as Entity;
			if (entity == wFieldMark)
			{
				break;
			}
			if (entity is WTextRange)
			{
				entity.RemoveSelf();
			}
		}
		Range.Items.Clear();
		IsFieldRangeUpdated = false;
	}

	private string FindFieldResult()
	{
		if (FieldSeparator == null)
		{
			return string.Empty;
		}
		IsFieldRangeUpdated = false;
		if (Range.Items.Count == 0)
		{
			UpdateFieldRange();
		}
		string text = string.Empty;
		IsFieldSeparator = false;
		int num = 0;
		if (base.OwnerParagraph == FieldSeparator.OwnerParagraph)
		{
			num = Range.Items.IndexOf(FieldSeparator) + 1;
		}
		else if (FieldSeparator.OwnerParagraph != null)
		{
			WParagraph ownerParagraph = FieldSeparator.OwnerParagraph;
			for (int i = FieldSeparator.Index + 1; i < ownerParagraph.Items.Count; i++)
			{
				if (ownerParagraph.Items[i] == FieldEnd)
				{
					Range.Items.Clear();
					IsFieldRangeUpdated = false;
					return text;
				}
				text += UpdateTextForParagraphItem(ownerParagraph.Items[i], isUpdateNestedFields: false);
			}
			num = Range.Items.IndexOf(ownerParagraph) + 1;
		}
		for (int j = num; j < Range.Items.Count; j++)
		{
			if (j != Range.Items.Count - 1 || !(Range.Items[j] is WParagraph) || Range.Items[j] != FieldEnd.OwnerParagraph)
			{
				text = ((!(Range.Items[j] is ParagraphItem)) ? (text + UpdateTextForTextBodyItem(Range.Items[j] as Entity, isUpdateNestedFields: false)) : (text + UpdateTextForParagraphItem(Range.Items[j] as Entity, isUpdateNestedFields: false)));
				continue;
			}
			WParagraph ownerParagraph2 = FieldEnd.OwnerParagraph;
			int index = 0;
			for (; j < ownerParagraph2.Items.Count; j++)
			{
				if (ownerParagraph2.Items[index] == FieldEnd)
				{
					break;
				}
				text += UpdateTextForParagraphItem(ownerParagraph2.Items[index], isUpdateNestedFields: false);
			}
		}
		Range.Items.Clear();
		IsFieldRangeUpdated = false;
		return text;
	}

	internal void ReplaceAsTOCField()
	{
		if (base.Document.TOC.ContainsKey(this))
		{
			return;
		}
		if (base.Document.TOC.Count > 0)
		{
			RemoveNestedTOCFields();
		}
		base.Document.SetTriggerElement(ref base.Document.m_supportedElementFlag_2, 15);
		string fieldCodeForUnknownFieldType = GetFieldCodeForUnknownFieldType();
		UpdateFieldCode(fieldCodeForUnknownFieldType);
		WParagraph ownerParagraph = base.OwnerParagraph;
		bool isSkipFieldDetach = base.Document.IsSkipFieldDetach;
		TableOfContent tableOfContent = new TableOfContent(base.Document, FormattingString);
		tableOfContent.TOCField.FieldSeparator = FieldSeparator;
		tableOfContent.TOCField.FieldEnd = FieldEnd;
		foreach (Revision item in RevisionsInternal)
		{
			int num = item.Range.InnerList.IndexOf(this);
			if (num != -1)
			{
				item.Range.InnerList.RemoveAt(num);
				item.Range.InnerList.Insert(num, tableOfContent);
			}
			tableOfContent.RevisionsInternal.Add(item);
		}
		base.Document.TOC.Add(this, tableOfContent);
		base.Document.IsSkipFieldDetach = true;
		ownerParagraph.Items.Remove(this);
		base.Document.IsSkipFieldDetach = isSkipFieldDetach;
		ownerParagraph.Items.Insert(Index, tableOfContent);
	}

	private void RemoveNestedTOCFields()
	{
		IsFieldRangeUpdated = false;
		if (Range.Items.Count == 0)
		{
			UpdateFieldRange();
		}
		for (int i = 0; i < Range.Items.Count; i++)
		{
			if (Range.Items[i] is TableOfContent)
			{
				TableOfContent tableOfContent = Range.Items[i] as TableOfContent;
				WField wField = tableOfContent.FindKey();
				if (wField != null)
				{
					base.Document.TOC.Remove(wField);
					WParagraph ownerParagraph = tableOfContent.OwnerParagraph;
					ownerParagraph.Items.Insert(tableOfContent.Index, wField);
					ownerParagraph.Items.Remove(tableOfContent);
				}
				break;
			}
		}
		Range.Items.Clear();
		IsFieldRangeUpdated = false;
	}

	internal WField ReplaceValidField()
	{
		ParagraphItemCollection paragraphItemCollection = base.OwnerParagraph.Items;
		if (base.Owner is InlineContentControl)
		{
			paragraphItemCollection = (base.Owner as InlineContentControl).ParagraphItems;
		}
		int index = Index;
		WField wField = CreateFieldByType(string.Empty, FieldType);
		wField.FieldSeparator = FieldSeparator;
		wField.FieldEnd = FieldEnd;
		WField wField2 = paragraphItemCollection[index] as WField;
		int num = m_doc.Fields.InnerList.IndexOf(wField2);
		if (num != -1)
		{
			m_doc.Fields.InnerList.RemoveAt(num);
		}
		paragraphItemCollection.RemoveFromInnerList(index);
		paragraphItemCollection.Insert(index, wField);
		if (wField2.CharacterFormat != null)
		{
			wField.ApplyCharacterFormat(wField2.CharacterFormat);
		}
		for (int i = 0; i < wField2.RevisionsInternal.Count; i++)
		{
			wField.RevisionsInternal.Add(wField2.RevisionsInternal[i]);
			int num2 = wField2.RevisionsInternal[i].Range.InnerList.IndexOf(wField2);
			if (num2 >= 0)
			{
				wField2.RevisionsInternal[i].Range.InnerList.Remove(wField2);
			}
			wField2.RevisionsInternal[i].Range.InnerList.Insert(num2, wField);
		}
		return wField;
	}

	internal void SetUnknownFieldType()
	{
		WFieldMark wFieldMark = ((FieldSeparator != null) ? FieldSeparator : FieldEnd);
		if (FieldEnd == null || base.OwnerParagraph == null)
		{
			return;
		}
		ParagraphItemCollection paragraphItemCollection = base.OwnerParagraph.Items;
		if (base.Owner is InlineContentControl)
		{
			paragraphItemCollection = (base.Owner as InlineContentControl).ParagraphItems;
		}
		IsFieldSeparator = false;
		string text = string.Empty;
		for (int i = Index + 1; i < paragraphItemCollection.Count; i++)
		{
			ParagraphItem paragraphItem = paragraphItemCollection[i];
			if (i > Index + 1 && paragraphItem is WField && UpdateFieldType(text))
			{
				IsFieldSeparator = false;
				return;
			}
			text += UpdateTextForParagraphItem(paragraphItem, isUpdateNestedFields: false);
			if (paragraphItem == wFieldMark)
			{
				UpdateFieldType(text);
				IsFieldSeparator = false;
				return;
			}
		}
		if (base.OwnerParagraph != wFieldMark.OwnerParagraph)
		{
			text = UpdateTextBodyFieldCode(base.OwnerParagraph.OwnerTextBody, wFieldMark.OwnerParagraph, base.OwnerParagraph.Index + 1, text);
		}
		UpdateFieldType(text);
		IsFieldSeparator = false;
	}

	internal void EnusreSpaceInResultText(WMergeField mergeField, string resultText, string textBefore, string textAfter)
	{
		bool isDoubleQuote = false;
		string text = UpdateNestedFieldCode(isUpdateNestedFields: false, mergeField).TrimStart();
		text = RemoveText(text, "if", isTrim: false);
		char[] trimChars = new char[3] { ' ', '\u00a0', '"' };
		List<string> list = new List<string>(new string[6] { "<=", ">=", "<>", "=", "<", ">" });
		bool flag = false;
		if (text == string.Empty)
		{
			mergeField.TextBefore = " " + textBefore;
		}
		else
		{
			string text2 = text.TrimEnd(trimChars);
			if (text.Length == text2.Length)
			{
				List<int> operatorIndex = GetOperatorIndex(list, text2, ref isDoubleQuote);
				if (flag = operatorIndex.Count > 0)
				{
					text2 = text2.Substring(operatorIndex[0]);
					if (list.Contains(text2))
					{
						mergeField.TextBefore = " " + textBefore;
					}
				}
			}
			else
			{
				flag = GetOperatorIndex(list, text2, ref isDoubleQuote).Count > 0;
			}
		}
		if (flag)
		{
			return;
		}
		string text3 = UpdateNestedFieldCode(isUpdateNestedFields: false, null).TrimStart();
		text3 = RemoveText(text3, "if", isTrim: false);
		text3 = text3.Remove(0, text.Length).TrimStart(trimChars);
		if (text3.StartsWith(mergeField.Text))
		{
			text3 = text3.Remove(0, mergeField.Text.Length);
		}
		string text4 = text3.TrimStart(trimChars);
		if (text3.Length != text4.Length)
		{
			return;
		}
		List<int> operatorIndex2 = GetOperatorIndex(list, text4, ref isDoubleQuote);
		if (operatorIndex2.Count > 0)
		{
			text4 = text4.Remove(operatorIndex2[0]);
			if (text4 == string.Empty)
			{
				mergeField.TextAfter = textAfter + " ";
			}
		}
	}

	private string UpdateTextBodyFieldCode(WTextBody tBody, WParagraph endParagraph, int startIndex, string code)
	{
		for (int i = startIndex; i < tBody.Items.Count; i++)
		{
			Entity entity = tBody.Items[i];
			code = ((!(entity is BlockContentControl)) ? (code + UpdateTextForTextBodyItem(entity, isUpdateNestedFields: false)) : (code + UpdateTextBodyFieldCode((entity as BlockContentControl).TextBody, endParagraph, (i == startIndex) ? startIndex : 0, code)));
			if (entity == endParagraph)
			{
				return code;
			}
		}
		return code;
	}

	private bool UpdateFieldType(string fieldCode)
	{
		FieldType fieldType = FieldTypeDefiner.GetFieldType(fieldCode);
		if (fieldType != FieldType.FieldUnknown)
		{
			FieldType = fieldType;
			return true;
		}
		return false;
	}

	internal void RemovePreviousFieldCode()
	{
		if (!(base.Owner is WParagraph))
		{
			return;
		}
		WFieldMark wFieldMark = ((FieldSeparator == null) ? FieldEnd : FieldSeparator);
		if (wFieldMark == null || !(wFieldMark.Owner is WParagraph) || (base.NextSibling is WFieldMark && ((base.NextSibling as WFieldMark).Type == FieldMarkType.FieldEnd || (base.NextSibling as WFieldMark).Type == FieldMarkType.FieldSeparator)))
		{
			return;
		}
		WParagraph ownerParagraph = base.OwnerParagraph;
		WParagraph ownerParagraph2 = wFieldMark.OwnerParagraph;
		if (ownerParagraph.OwnerTextBody == null || GetSection(ownerParagraph.OwnerTextBody) == null || ownerParagraph2.OwnerTextBody == null || GetSection(ownerParagraph2.OwnerTextBody) == null)
		{
			if (ownerParagraph == ownerParagraph2)
			{
				int num = GetIndexInOwnerCollection() + 1;
				int indexInOwnerCollection = wFieldMark.GetIndexInOwnerCollection();
				for (int i = num; i < indexInOwnerCollection; i++)
				{
					if (num >= ownerParagraph.ChildEntities.Count)
					{
						break;
					}
					ownerParagraph.ChildEntities.RemoveFromInnerList(num);
				}
			}
		}
		else
		{
			int index = GetIndexInOwnerCollection() + 1;
			BookmarkStart bookmarkStart = new BookmarkStart(m_doc, "_FieldBookmark");
			BookmarkEnd bookmarkEnd = new BookmarkEnd(m_doc, "_FieldBookmark");
			ownerParagraph.Items.Insert(index, bookmarkStart);
			EnsureBookmarkStart(bookmarkStart);
			int indexInOwnerCollection2 = wFieldMark.GetIndexInOwnerCollection();
			ownerParagraph2.Items.Insert(indexInOwnerCollection2, bookmarkEnd);
			EnsureBookmarkStart(bookmarkEnd);
			BookmarksNavigator bookmarksNavigator = new BookmarksNavigator(base.Document);
			bookmarksNavigator.MoveToBookmark("_FieldBookmark");
			bookmarksNavigator.RemoveEmptyParagraph = false;
			base.Document.IsSkipFieldDetach = true;
			bookmarksNavigator.DeleteBookmarkContent(saveFormatting: false);
			base.Document.IsSkipFieldDetach = false;
			if (ownerParagraph.Items.Contains(bookmarkStart))
			{
				ownerParagraph.Items.Remove(bookmarkStart);
			}
			if (ownerParagraph2.Items.Contains(bookmarkEnd))
			{
				ownerParagraph2.Items.Remove(bookmarkEnd);
			}
		}
		IsFieldRangeUpdated = false;
	}

	private Entity GetSection(Entity entity)
	{
		while (!(entity is WSection))
		{
			entity = entity.Owner;
			if (entity == null)
			{
				break;
			}
		}
		return entity;
	}

	internal void EnsureBookmarkStart(BookmarkStart bookmarkStart)
	{
		if (m_doc.Bookmarks.FindByName(bookmarkStart.Name) == null)
		{
			m_doc.Bookmarks.AttachBookmarkStart(bookmarkStart);
		}
	}

	internal void EnsureBookmarkStart(BookmarkEnd bookmarkEnd)
	{
		Bookmark bookmark = m_doc.Bookmarks.FindByName(bookmarkEnd.Name);
		if (bookmark != null && bookmark.BookmarkEnd == null)
		{
			m_doc.Bookmarks.AttachBookmarkEnd(bookmarkEnd);
		}
	}

	internal List<WCharacterFormat> GetResultCharacterFormatting()
	{
		List<WCharacterFormat> characterFormats = new List<WCharacterFormat>();
		if ((base.Document.Settings.MaintainFormattingOnFieldUpdate || FormattingString.ToUpper().Contains("\\* MERGEFORMAT")) && FieldSeparator != null && FieldSeparator.NextSibling != null && FieldSeparator.NextSibling != FieldEnd)
		{
			int num = Range.Items.IndexOf(FieldSeparator.NextSibling);
			if (num == -1 && FieldSeparator.NextSibling.Owner != null)
			{
				num = Range.Items.IndexOf(FieldSeparator.NextSibling.Owner);
			}
			List<object> list = new List<object>();
			if (num != -1)
			{
				for (int i = num; i < Range.Items.Count; i++)
				{
					object obj = Range.Items[i];
					if (obj is Entity && (obj as Entity).Equals(FieldEnd))
					{
						break;
					}
					list.Add(obj);
				}
			}
			if (list.Count <= 1)
			{
				characterFormats.Add((FieldSeparator.NextSibling as ParagraphItem).ParaItemCharFormat);
			}
			else
			{
				UpdateRangeCharacterFormats(list, ref characterFormats, isNotMergeFormat: false);
			}
			return characterFormats;
		}
		if (InternalFieldCode.Trim() != string.Empty)
		{
			characterFormats.Add(base.CharacterFormat);
			return characterFormats;
		}
		UpdateRangeCharacterFormats(Range.Items, ref characterFormats, isNotMergeFormat: true);
		return characterFormats;
	}

	private void UpdateRangeCharacterFormats(IList items, ref List<WCharacterFormat> characterFormats, bool isNotMergeFormat)
	{
		for (int i = 0; i < items.Count; i++)
		{
			Entity entity = items[i] as Entity;
			int count = 0;
			if (entity is ParagraphItem && entity.EntityType == EntityType.TextRange)
			{
				WCharacterFormat resultCharFormatFromRange = GetResultCharFormatFromRange(entity as WTextRange, i, ref count);
				if (resultCharFormatFromRange == null)
				{
					continue;
				}
				if (isNotMergeFormat)
				{
					characterFormats.Add(resultCharFormatFromRange);
					break;
				}
				if (count >= 2 || i == 0)
				{
					while (count > 0)
					{
						characterFormats.Add(resultCharFormatFromRange);
						count--;
					}
				}
			}
			else if (entity is WParagraph)
			{
				for (int j = 0; j < (entity as WParagraph).ChildEntities.Count; j++)
				{
					Entity entity2 = (entity as WParagraph).ChildEntities[j];
					if (!(entity2 is ParagraphItem) || entity2.EntityType != EntityType.TextRange)
					{
						continue;
					}
					WCharacterFormat resultCharFormatFromRange = GetResultCharFormatFromRange(entity2 as WTextRange, i, ref count);
					if (resultCharFormatFromRange == null)
					{
						continue;
					}
					if (isNotMergeFormat)
					{
						characterFormats.Add(resultCharFormatFromRange);
						break;
					}
					if (count >= 2 || i == 0)
					{
						while (count > 0)
						{
							characterFormats.Add(resultCharFormatFromRange);
							count--;
						}
					}
				}
			}
			else if (isNotMergeFormat)
			{
				break;
			}
		}
	}

	private WCharacterFormat GetResultCharFormatFromRange(WTextRange textRange, int i, ref int count)
	{
		char[] array = new char[9] { ' ', ',', '.', '/', '-', ':', '\t', '\ufffd', '\ufffd' };
		string text = textRange.Text;
		if ((text.Trim() != string.Empty && text.Trim(array) != string.Empty) || i == 0)
		{
			count = text.Length;
			char[] array2 = array;
			foreach (char c in array2)
			{
				text = text.Replace(c.ToString(), string.Empty);
			}
			count = count - text.Length + 1;
			return textRange.GetCharFormat();
		}
		return null;
	}

	internal ParagraphItem GetIncudePictureFieldResult()
	{
		if (FieldType != FieldType.FieldIncludePicture || FieldSeparator == null || !(FieldSeparator.Owner is WParagraph))
		{
			return null;
		}
		int num = -1;
		if (Range.Items.Contains(FieldSeparator))
		{
			num = Range.Items.IndexOf(FieldSeparator);
		}
		else
		{
			if (!Range.Items.Contains(FieldSeparator.OwnerParagraph))
			{
				return null;
			}
			num = Range.Items.IndexOf(FieldSeparator.OwnerParagraph);
		}
		if (num != -1)
		{
			for (int i = num; i < Range.Items.Count; i++)
			{
				Entity entity = Range.Items[i] as Entity;
				if (entity is WPicture)
				{
					return entity as ParagraphItem;
				}
				if (!(entity is WParagraph))
				{
					continue;
				}
				foreach (ParagraphItem childEntity in (entity as WParagraph).ChildEntities)
				{
					if (childEntity is WPicture)
					{
						return childEntity;
					}
				}
			}
		}
		return null;
	}

	public void Update()
	{
		IsFieldRangeUpdated = false;
		if (!base.IsDeleteRevision)
		{
			switch (FieldType)
			{
			case FieldType.FieldDate:
			case FieldType.FieldTime:
			{
				DateTime currentDateTime = (WordDocument.DisableDateTimeUpdating ? DateTime.MaxValue : DateTime.Now);
				UpdateFieldResult(UpdateDateField(FormattingString, currentDateTime));
				break;
			}
			case FieldType.FieldDocVariable:
			{
				string oldValue = '"'.ToString();
				string text2 = m_doc.Variables[FieldValue.Replace(oldValue, string.Empty)];
				if (text2 != null)
				{
					string numberFormat = string.Empty;
					RemoveMergeFormat(FieldCode, ref numberFormat);
					string text3 = text2;
					text2 = (string.IsNullOrEmpty(numberFormat) ? UpdateTextFormat(text3) : UpdateNumberFormat(text3, numberFormat));
					UpdateFieldResult(text2);
				}
				break;
			}
			case FieldType.FieldFormula:
			{
				string text = FieldCode.Replace(" ", "").ToUpper();
				if (!text.StartsWith("=SUM(ABOVE)") && !text.StartsWith("=MAX(ABOVE)"))
				{
					UpdateFormulaField();
				}
				break;
			}
			case FieldType.FieldCompare:
				UpdateCompareField();
				break;
			case FieldType.FieldIf:
				(this as WIfField).UpdateIfField();
				break;
			case FieldType.FieldDocProperty:
				UpdateDocPropertyField();
				break;
			case FieldType.FieldSection:
				UpdateSectionField();
				break;
			case FieldType.FieldNumPages:
				UpdateNumberFormatResult(base.Document.PageCount.ToString());
				break;
			case FieldType.FieldPage:
				if (DocumentLayouter.IsLayoutingHeaderFooter)
				{
					WSection wSection = GetOwnerSection(base.OwnerParagraph) as WSection;
					m_currentPageNumber = wSection.PageSetup.GetNumberFormatValue((byte)wSection.PageSetup.PageNumberStyle, DocumentLayouter.PageNumber);
					UpdateNumberFormatResult(m_currentPageNumber);
				}
				break;
			case FieldType.FieldRef:
				UpdateRefField();
				break;
			case FieldType.FieldTitle:
				UpdateDocumentBuiltInProperties("Title");
				break;
			case FieldType.FieldSubject:
				UpdateDocumentBuiltInProperties("Subject");
				break;
			case FieldType.FieldAuthor:
				UpdateDocumentBuiltInProperties("Author");
				break;
			case FieldType.FieldComments:
				UpdateDocumentBuiltInProperties("Comments");
				break;
			case FieldType.FieldSet:
				UpdateSetFields();
				break;
			case FieldType.FieldUnknown:
				UpdateUnknownField();
				break;
			case FieldType.FieldSequence:
			{
				string fieldResultNumber = string.Empty;
				if (base.Document.SequenceFieldResults != null)
				{
					fieldResultNumber = ((!string.IsNullOrEmpty((this as WSeqField).CaptionName)) ? GetSequenceFieldResult().ToString() : "Error! No sequence specified");
					fieldResultNumber = ((this as WSeqField).HideResult ? string.Empty : fieldResultNumber);
				}
				else
				{
					base.Document.SequenceFieldResults = new Dictionary<string, int>();
					for (int i = 0; i < base.Document.Fields.Count; i++)
					{
						if (base.Document.Fields[i].FieldType == FieldType.FieldSequence)
						{
							if (!string.IsNullOrEmpty((this as WSeqField).BookmarkName) && IsBooKMarkSeqFieldUpdated(base.Document.Fields[i]))
							{
								return;
							}
							base.Document.Fields[i].Update();
							if (string.IsNullOrEmpty((this as WSeqField).BookmarkName) && base.Document.Fields[i] == this)
							{
								ClearSeqFieldInternalCollection();
								return;
							}
						}
					}
					ClearSeqFieldInternalCollection();
				}
				UpdateSequenceFieldResult(fieldResultNumber);
				break;
			}
			}
		}
		if (!IsUpdated)
		{
			IsUpdated = true;
		}
	}

	private bool IsBooKMarkSeqFieldUpdated(WField field)
	{
		Bookmark bookmark = base.Document.Bookmarks.FindByName((this as WSeqField).BookmarkName);
		if (bookmark != null)
		{
			string text = ((bookmark.BookmarkEnd.NextSibling is WSeqField) ? (bookmark.BookmarkEnd.NextSibling as Entity).GetHierarchicalIndex(string.Empty) : bookmark.BookmarkEnd.GetHierarchicalIndex(string.Empty));
			string hierarchicalIndex = field.GetHierarchicalIndex(string.Empty);
			if (text != hierarchicalIndex && CompareHierarchicalIndex(text, field.GetHierarchicalIndex(string.Empty)))
			{
				UpdateSequenceFieldResult(base.Document.SequenceFieldResults[(this as WSeqField).CaptionName].ToString());
				ClearSeqFieldInternalCollection();
				return true;
			}
			return false;
		}
		UpdateSequenceFieldResult(0.ToString());
		ClearSeqFieldInternalCollection();
		return true;
	}

	private void ClearSeqFieldInternalCollection()
	{
		base.Document.SequenceFieldResults.Clear();
		base.Document.SequenceFieldResults = null;
		if (!IsUpdated)
		{
			IsUpdated = true;
		}
	}

	internal void UpdateSequenceFieldResult(string fieldResultNumber)
	{
		if (!string.IsNullOrEmpty((this as WSeqField).CaptionName))
		{
			string numberFormat = string.Empty;
			RemoveMergeFormat(FieldCode, ref numberFormat);
			string text = fieldResultNumber;
			fieldResultNumber = (string.IsNullOrEmpty(numberFormat) ? UpdateTextFormat(text) : UpdateNumberFormat(text, numberFormat));
		}
		UpdateFieldResult(fieldResultNumber);
	}

	private int GetSequenceFieldResult()
	{
		if (!string.IsNullOrEmpty((this as WSeqField).BookmarkName))
		{
			return 0;
		}
		if (base.Document.SequenceFieldResults.ContainsKey((this as WSeqField).CaptionName))
		{
			if ((this as WSeqField).ResetNumber != -1)
			{
				return base.Document.SequenceFieldResults[(this as WSeqField).CaptionName] = (this as WSeqField).ResetNumber;
			}
			if ((this as WSeqField).ResetHeadingLevel != -1 && IsNeedToResetHeadingLevel())
			{
				return base.Document.SequenceFieldResults[(this as WSeqField).CaptionName] = 1;
			}
			if ((this as WSeqField).RepeatNearestNumber)
			{
				return base.Document.SequenceFieldResults[(this as WSeqField).CaptionName];
			}
			return ++base.Document.SequenceFieldResults[(this as WSeqField).CaptionName];
		}
		if ((this as WSeqField).RepeatNearestNumber)
		{
			return 0;
		}
		if ((this as WSeqField).ResetNumber != -1)
		{
			base.Document.SequenceFieldResults.Add((this as WSeqField).CaptionName, (this as WSeqField).ResetNumber);
			return (this as WSeqField).ResetNumber;
		}
		base.Document.SequenceFieldResults.Add((this as WSeqField).CaptionName, 1);
		return 1;
	}

	private bool IsNeedToResetHeadingLevel()
	{
		WParagraph wParagraph = GetOwnerParagraphValue();
		int headingLevel = GetHeadingLevel(wParagraph.ParaStyle as WParagraphStyle);
		if (headingLevel != -1 && headingLevel <= (this as WSeqField).ResetHeadingLevel)
		{
			return true;
		}
		WSeqField prevSeqField = GetPrevSeqField();
		if (prevSeqField == null)
		{
			return false;
		}
		WParagraph ownerParagraphValue = prevSeqField.GetOwnerParagraphValue();
		if (ownerParagraphValue == null)
		{
			return false;
		}
		for (Entity ownerShape = GetOwnerShape(ownerParagraphValue); ownerShape != null; ownerShape = GetOwnerShape(ownerParagraphValue))
		{
			ownerParagraphValue = (ownerShape as ParagraphItem).GetOwnerParagraphValue();
		}
		if (wParagraph == ownerParagraphValue)
		{
			return false;
		}
		do
		{
			wParagraph = GetPreviousParagraph(wParagraph);
			if (wParagraph == null)
			{
				return false;
			}
			_ = wParagraph.ParaStyle;
			headingLevel = GetHeadingLevel(wParagraph.ParaStyle as WParagraphStyle);
			if (headingLevel != -1 && headingLevel <= (this as WSeqField).ResetHeadingLevel)
			{
				return true;
			}
		}
		while (wParagraph != ownerParagraphValue);
		return false;
	}

	internal WParagraph GetPreviousParagraph(WParagraph paragrph)
	{
		IEntity previousSibling = paragrph.PreviousSibling;
		ParagraphItem paragraphItem = GetOwnerShape(paragrph) as ParagraphItem;
		if (paragraphItem != null)
		{
			WParagraph ownerParagraph = paragraphItem.OwnerParagraph;
			for (int num = paragraphItem.Index - 1; num >= 0; num--)
			{
				Entity entity = ownerParagraph.ChildEntities[num];
				if (entity is WSeqField && (entity as WSeqField).CaptionName == (this as WSeqField).CaptionName)
				{
					return null;
				}
			}
		}
		if (previousSibling is WParagraph)
		{
			return previousSibling as WParagraph;
		}
		if (previousSibling is BlockContentControl)
		{
			return GetPreviousParagraphIsInSDTContent(previousSibling as BlockContentControl);
		}
		if (previousSibling is WTable)
		{
			return GetPreviousParagraphIsInTable(previousSibling as WTable);
		}
		if (previousSibling == null && (paragraphItem is WTextBox || paragraphItem is Shape))
		{
			WParagraph ownerParagraph2 = paragraphItem.OwnerParagraph;
			return GetPreviousParagraph(ownerParagraph2);
		}
		if (previousSibling == null && paragrph.IsInCell)
		{
			return GetPreviousParagraphIsInCell(paragrph);
		}
		if (previousSibling == null)
		{
			return GetPreviousParagraphIsInSection(paragrph);
		}
		return null;
	}

	private WParagraph GetPreviousParagraphIsInTable(WTable table)
	{
		IEntity entity = table.LastCell.WidgetInnerCollection[table.LastCell.WidgetInnerCollection.Count - 1];
		if (entity is WParagraph)
		{
			return entity as WParagraph;
		}
		if (entity is BlockContentControl)
		{
			return GetPreviousParagraphIsInSDTContent(entity as BlockContentControl);
		}
		if (entity is WTable)
		{
			return GetPreviousParagraphIsInTable(entity as WTable);
		}
		return null;
	}

	private WParagraph GetPreviousParagraphIsInCell(WParagraph paragraph)
	{
		WTableCell ownerTableCell = paragraph.GetOwnerTableCell(paragraph.OwnerTextBody);
		WTableCell wTableCell = ownerTableCell.PreviousSibling as WTableCell;
		Entity entity = null;
		entity = ((wTableCell != null) ? wTableCell.Items.LastItem : ((!(ownerTableCell.OwnerRow.PreviousSibling is WTableRow wTableRow) || wTableRow.Cells.Count <= 0) ? (ownerTableCell.OwnerRow.OwnerTable.PreviousSibling as Entity) : wTableRow.Cells[wTableRow.Cells.Count - 1].Items.LastItem));
		if (entity is WParagraph)
		{
			return entity as WParagraph;
		}
		if (entity is BlockContentControl)
		{
			return GetPreviousParagraphIsInSDTContent(entity as BlockContentControl);
		}
		if (entity is WTable)
		{
			return GetPreviousParagraphIsInTable(entity as WTable);
		}
		if (entity == null)
		{
			return GetPreviousParagraphIsInSection(paragraph.GetOwnerTableCell(paragraph.OwnerTextBody).OwnerRow.OwnerTable);
		}
		return null;
	}

	private WParagraph GetPreviousParagraphIsInSection(IEntity textBodyItem)
	{
		IEntity entity = null;
		if (textBodyItem.Owner is WSection)
		{
			if (!((textBodyItem.Owner as WSection).PreviousSibling is WSection wSection))
			{
				return null;
			}
			entity = wSection.Body.Items.LastItem;
			if (entity is WParagraph)
			{
				return entity as WParagraph;
			}
			if (entity is BlockContentControl)
			{
				return GetPreviousParagraphIsInSDTContent(entity as BlockContentControl);
			}
			if (entity is WTable)
			{
				return GetPreviousParagraphIsInTable(entity as WTable);
			}
			return null;
		}
		return null;
	}

	private WParagraph GetPreviousParagraphIsInSDTContent(BlockContentControl sdtContent)
	{
		BodyItemCollection items = sdtContent.TextBody.Items;
		IEntity entity = items[items.Count - 1];
		if (entity is WParagraph)
		{
			return entity as WParagraph;
		}
		if (entity is BlockContentControl)
		{
			return GetPreviousParagraphIsInSDTContent(entity as BlockContentControl);
		}
		if (entity is WTable)
		{
			return GetPreviousParagraphIsInTable(entity as WTable);
		}
		return null;
	}

	private WSeqField GetPrevSeqField()
	{
		if (base.Document.Fields.Count > 1)
		{
			for (int num = base.Document.Fields.InnerList.IndexOf(this) - 1; num >= 0; num--)
			{
				if (base.Document.Fields[num] is WSeqField && (this as WSeqField).CaptionName == (base.Document.Fields[num] as WSeqField).CaptionName)
				{
					return base.Document.Fields[num] as WSeqField;
				}
			}
		}
		return null;
	}

	private int GetHeadingLevel(WParagraphStyle paragraphStyle)
	{
		if (!paragraphStyle.BuiltInStyleIdentifier.ToString().ToLower().Contains("heading"))
		{
			return -1;
		}
		string text = paragraphStyle.BuiltInStyleIdentifier.ToString();
		if (text.Contains(","))
		{
			text = text.Split(new char[1] { ',' })[0];
		}
		if (text.Contains("+"))
		{
			text = text.Split(new char[1] { '+' })[0];
		}
		char[] array = text.ToCharArray();
		string text2 = "";
		char[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			char c = array2[i];
			if (c == '_')
			{
				break;
			}
			if (int.TryParse(string.Format(CultureInfo.InvariantCulture, c.ToString()), out var result))
			{
				text2 += result.ToString(CultureInfo.InvariantCulture);
			}
		}
		return int.Parse(text2);
	}

	private int ParseIntegerValue(string value)
	{
		int result = 0;
		double result2 = 0.0;
		if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result2))
		{
			if (value.Contains("."))
			{
				int num = value.IndexOf('.');
				if (num > 0)
				{
					value = value.Substring(0, num);
				}
				else if (num == 0)
				{
					value = "0";
				}
			}
			int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
		}
		return result;
	}

	private void UpdateUnknownField()
	{
		string text = FieldCode.Trim();
		if (!(text != ""))
		{
			return;
		}
		string text2 = text.Split(' ')[0];
		if (!(text2.ToUpper() != "BIBLIOGRAPHY"))
		{
			return;
		}
		if (base.Document.Bookmarks.FindByName(text2) != null)
		{
			BookmarksNavigator bookmarksNavigator = new BookmarksNavigator(base.Document);
			bookmarksNavigator.MoveToBookmark(text2);
			TextBodyPart bookmarkContent = bookmarksNavigator.GetBookmarkContent();
			List<Entity> list = new List<Entity>();
			foreach (TextBodyItem bodyItem in bookmarkContent.BodyItems)
			{
				if (bodyItem is WParagraph)
				{
					Entity nextItem = null;
					int itemIndex = 0;
					List<Entity> clonedParagraph = GetClonedParagraph(bodyItem, null, ref nextItem, ref itemIndex, isRefFieldUpdate: false);
					if (list.Count == 0 && clonedParagraph[0] is WParagraph)
					{
						foreach (ParagraphItem item in (clonedParagraph[0] as WParagraph).Items)
						{
							list.Add(item);
						}
						continue;
					}
					foreach (Entity item2 in clonedParagraph)
					{
						list.Add(item2);
					}
				}
				else if (bodyItem is WTable)
				{
					list.Add(GetClonedTable(bodyItem, isRefFieldUpdate: false));
				}
			}
			UpdateUnKnownFieldResult(list);
		}
		else if (IsFunction(text.ToLower()))
		{
			bool isFieldCodeStartWithCurrencySymbol = false;
			string text3 = UpdateFunction(text, ref isFieldCodeStartWithCurrencySymbol);
			UpdateFieldResult(text3);
		}
		else
		{
			string text4 = "Error! Bookmark not defined.";
			UpdateFieldResult(text4);
		}
	}

	private string GetAutoNumFieldValue(WField field)
	{
		string text = string.Empty;
		if (field.FieldSeparator == null && (field.FieldType == FieldType.FieldAutoNum || field.FieldType == FieldType.FieldAutoNumLegal))
		{
			if (field.OwnerParagraph != null)
			{
				if (IsInValidTextBody(field))
				{
					text = UpdateTextFormat(base.Document.Fields.GetAutoNumFieldResult(field));
					char autoNumSeparatorChar = GetAutoNumSeparatorChar(field);
					if (autoNumSeparatorChar != 0)
					{
						text += autoNumSeparatorChar;
					}
				}
				else
				{
					text = "Main Document Only";
				}
			}
			else
			{
				text = UpdateTextFormat(base.Document.Fields.GetAutoNumFieldResult(field));
				char autoNumSeparatorChar2 = GetAutoNumSeparatorChar(field);
				if (autoNumSeparatorChar2 != 0)
				{
					text += autoNumSeparatorChar2;
				}
			}
		}
		return text;
	}

	private char GetAutoNumSeparatorChar(WField field)
	{
		char result = '.';
		_ = new char[0];
		string[] array = field.FieldCode.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == "\\s" && i + 1 < array.Length)
			{
				result = array[i + 1].ToCharArray()[0];
			}
			else if (array[i] == "\\e" && field.FieldType == FieldType.FieldAutoNumLegal)
			{
				result = '\0';
				break;
			}
		}
		return result;
	}

	private bool IsInValidTextBody(WField field)
	{
		WParagraph ownerParagraph = field.OwnerParagraph;
		Entity ownerEntity = ownerParagraph.GetOwnerEntity();
		if (ownerEntity is WTextBox || ownerEntity is Shape)
		{
			return false;
		}
		if (ownerParagraph.IsInHeaderFooter())
		{
			return false;
		}
		return true;
	}

	internal void UpdateSetFields()
	{
		string numberFormat = string.Empty;
		string[] array = RemoveMergeFormat(FieldCode, ref numberFormat).Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 3)
		{
			string text = array[1];
			string text2 = array[2];
			UpdateFieldResult(text2);
			RemoveBookMarksForSetFields(text);
			UpdateBookMarkForSetFields(text);
		}
	}

	private void UpdateBookMarkForSetFields(string bkName)
	{
		int index = FieldSeparator.GetIndexInOwnerCollection() + 1;
		WParagraph ownerParagraph = base.OwnerParagraph;
		BookmarkStart entity = new BookmarkStart(m_doc, bkName);
		BookmarkEnd entity2 = new BookmarkEnd(m_doc, bkName);
		ownerParagraph.ChildEntities.Insert(index, entity);
		int indexInOwnerCollection = FieldEnd.GetIndexInOwnerCollection();
		ownerParagraph.ChildEntities.Insert(indexInOwnerCollection, entity2);
	}

	private void RemoveBookMarksForSetFields(string bookMarkName)
	{
		Bookmark bookmark = base.Document.Bookmarks.FindByName(bookMarkName);
		if (bookmark != null)
		{
			base.Document.Bookmarks.Remove(bookmark);
		}
	}

	private void UpdateDocumentBuiltInProperties(string propertyName)
	{
		if (string.IsNullOrEmpty(FieldCode))
		{
			return;
		}
		string fieldCode = FieldCode;
		string text = fieldCode;
		text = text.TrimStart(' ');
		text = RemoveMergeFormat(text);
		bool isHavingStringFormat = true;
		text = RemoveStringFormat(text, out isHavingStringFormat);
		char[] array = new char[5] { '\n', '\v', '\r', '\t', ' ' };
		text.TrimStart(array);
		string[] array2 = text.Split(array, StringSplitOptions.RemoveEmptyEntries);
		if (array2.Length > 1 && !string.IsNullOrEmpty(array2[1]))
		{
			array2[1].TrimStart(array);
			if (StartsWithExt(array2[1], "\""))
			{
				text = fieldCode.TrimStart(' ');
				array2 = text.Split(new char[1] { '"' }, StringSplitOptions.RemoveEmptyEntries);
			}
			else if (array2[1].Contains("\""))
			{
				array2[1] = array2[1].Split(new char[1] { '"' }, StringSplitOptions.RemoveEmptyEntries)[0];
			}
			if (Regex.Matches(array2[1], Regex.Escape("\\")).Count >= 1)
			{
				int num = array2[1].IndexOf('\\');
				string input = array2[1].Substring(0, num + 1);
				array2[1] = Regex.Replace(input, "\\\\", "") + array2[1].Substring(num + 1);
			}
			switch (propertyName.ToLower())
			{
			case "author":
			case "title":
			case "subject":
			case "comments":
				array2[1] = (isHavingStringFormat ? UpdateTextFormat(array2[1]) : array2[1]);
				UpdateFieldResult(array2[1]);
				break;
			}
		}
		else
		{
			if (array2.Length != 1)
			{
				return;
			}
			switch (propertyName.ToLower())
			{
			case "author":
				if (!string.IsNullOrEmpty(base.Document.BuiltinDocumentProperties.Author))
				{
					UpdateFieldResult(base.Document.BuiltinDocumentProperties.Author);
				}
				break;
			case "title":
				if (!string.IsNullOrEmpty(base.Document.BuiltinDocumentProperties.Title))
				{
					UpdateFieldResult(base.Document.BuiltinDocumentProperties.Title);
				}
				break;
			case "subject":
				if (!string.IsNullOrEmpty(base.Document.BuiltinDocumentProperties.Subject))
				{
					UpdateFieldResult(base.Document.BuiltinDocumentProperties.Subject);
				}
				break;
			case "comments":
				if (!string.IsNullOrEmpty(base.Document.BuiltinDocumentProperties.Comments))
				{
					UpdateFieldResult(base.Document.BuiltinDocumentProperties.Comments);
				}
				break;
			}
		}
	}

	private string RemoveStringFormat(string fieldCode, out bool isHavingStringFormat)
	{
		isHavingStringFormat = true;
		string[] array = fieldCode.Split(new string[1] { "\\*" }, StringSplitOptions.RemoveEmptyEntries);
		bool flag = true;
		for (int i = 1; i < array.Length; i++)
		{
			string text = array[i].ToLower().Trim(' ');
			int count = Regex.Matches(text, Regex.Escape("\\")).Count;
			if (count <= 0)
			{
				continue;
			}
			if (count == 1)
			{
				text = Regex.Replace(text, "\\\\", "");
			}
			if (count >= 2)
			{
				continue;
			}
			switch (text)
			{
			case "lower":
			case "upper":
			case "firstcap":
			case "caps":
				continue;
			}
			if (text.Contains("\""))
			{
				char[] array2 = new char[5] { '\n', '\v', '\r', '\t', ' ' };
				array[0].TrimStart(array2);
				string[] array3 = array[0].Split(array2, StringSplitOptions.RemoveEmptyEntries);
				for (int j = 1; j < array3.Length; j++)
				{
					if (array3[j].Contains("\""))
					{
						flag = true;
						break;
					}
				}
				continue;
			}
			flag = false;
			isHavingStringFormat = false;
			break;
		}
		if (flag)
		{
			return array[0];
		}
		return string.Empty;
	}

	internal string UpdateDateField(string text, DateTime currentDateTime)
	{
		if (!IsFieldRangeUpdated && base.Document.IsOpening)
		{
			Range.Items.Clear();
			UpdateFieldRange();
		}
		bool isMeridiemDefined = false;
		if (text.Contains("\\* MERGEFORMAT"))
		{
			text = text.Remove(text.IndexOf("\\* MERGEFORMAT")).Trim();
		}
		else if (text.Contains("\\* Mergeformat"))
		{
			text = text.Remove(text.IndexOf("\\* Mergeformat")).Trim();
		}
		if (text.LastIndexOf("\\*") != -1 && text.Substring(text.LastIndexOf('*') + 1).Trim().ToUpper() == "CHARFORMAT")
		{
			text = text.Remove(text.LastIndexOf("\\*")).Trim();
		}
		bool ordinalString = false;
		if (text.ToLower().Contains("ordinal"))
		{
			text = text.Remove(text.ToLower().IndexOf("ordinal")).Trim();
			ordinalString = true;
			if (text.ToLower().EndsWith("\\*"))
			{
				text = text.Remove(text.ToLower().IndexOf("\\*")).Trim();
			}
		}
		if (text.Contains("\\@"))
		{
			text = ParseSwitches(text, text.IndexOf("\\@"));
			text = RemoveMeridiem(text, out isMeridiemDefined);
			text = UpdateDateValue(text, currentDateTime);
			text = GetOrdinalstring(ordinalString, text);
			if (isMeridiemDefined)
			{
				text = UpdateMeridiem(text, currentDateTime);
			}
		}
		else
		{
			CultureInfo cultureInfo = ((!base.CharacterFormat.HasValue(73) && !base.CharacterFormat.BaseFormat.HasValue(73)) ? CultureInfo.CurrentCulture : GetCulture((LocaleIDs)base.CharacterFormat.LocaleIdASCII));
			text = currentDateTime.ToString(cultureInfo.DateTimeFormat.ShortDatePattern);
		}
		return text;
	}

	private string GetOrdinalstring(bool ordinalString, string text)
	{
		if (ordinalString)
		{
			text = text.Trim().ToString();
			for (int i = 0; i < text.Length; i++)
			{
				if (char.IsLetter(text[i]))
				{
					ordinalString = false;
					break;
				}
			}
			if (ordinalString)
			{
				int num = 0;
				if (text.Length >= 2 && char.IsNumber(text[text.Length - 1]) && char.IsNumber(text[text.Length - 2]))
				{
					num = int.Parse(text.Substring(text.Length - 2, 2));
					text = text.Remove(text.Length - 2, 2);
					text += base.Document.GetOrdinal(num, base.CharacterFormat);
				}
				else if (text.Length >= 1 && char.IsNumber(text[text.Length - 1]))
				{
					num = int.Parse(text.Substring(text.Length - 1));
					text = text.Remove(text.Length - 1);
					text += base.Document.GetOrdinal(num, base.CharacterFormat);
				}
			}
		}
		return text;
	}

	internal bool UpdateNextIfField()
	{
		string numberFormat = string.Empty;
		string text = RemoveMergeFormat(FieldCode, ref numberFormat);
		text = RemoveText(text, "nextif");
		_ = string.Empty;
		return UpdateCondition(text, null, null) == "1";
	}

	private void UpdateSectionField()
	{
		Entity entity = base.OwnerBase as Entity;
		int num = 1;
		while (!(entity is WSection) && entity.Owner != null)
		{
			entity = entity.Owner;
		}
		if (entity != null && entity is WSection)
		{
			num += (entity as WSection).GetIndexInOwnerCollection();
		}
		UpdateNumberFormatResult(num.ToString());
	}

	private bool IsPictureSwitchIsInSecondPlace()
	{
		string internalFieldCode = InternalFieldCode;
		internalFieldCode = Regex.Replace(internalFieldCode, "\\s", "");
		if (internalFieldCode.Contains("\\#") && internalFieldCode.Contains("\\*"))
		{
			return internalFieldCode.IndexOf("\\#") > internalFieldCode.IndexOf("\\*");
		}
		return false;
	}

	internal void UpdateNumberFormatResult(string result)
	{
		UpdateNumberFormatResult(result, skipFieldResultPartUpdate: false);
	}

	internal void UpdateNumberFormatResult(string result, bool skipFieldResultPartUpdate)
	{
		string numberFormat = string.Empty;
		string empty = string.Empty;
		if (Regex.Matches(InternalFieldCode, Regex.Escape("\\#")).Count > 1)
		{
			empty = "Error! Too many picture switches defined.";
		}
		else if (IsPictureSwitchIsInSecondPlace())
		{
			empty = "Error! Picture switch must be first formatting switch.";
		}
		else
		{
			RemoveMergeFormat(FieldCode, ref numberFormat);
			empty = result;
			empty = (string.IsNullOrEmpty(numberFormat) ? UpdateTextFormat(empty) : UpdateNumberFormat(empty, numberFormat));
		}
		if (skipFieldResultPartUpdate)
		{
			FieldResult = empty;
		}
		else
		{
			UpdateFieldResult(empty);
		}
	}

	private void UpdateDocPropertyField()
	{
		string text = m_fieldValue.Trim();
		string text2 = string.Empty;
		string text3 = "Error! Unknown document property name.";
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (string key in base.Document.CustomDocumentProperties.CustomHash.Keys)
		{
			string value = base.Document.CustomDocumentProperties.CustomHash[key].Value.ToString();
			dictionary.Add(key.ToLower(), value);
		}
		if (dictionary.ContainsKey(text.ToLower()))
		{
			text2 = dictionary[text.ToLower()];
		}
		else
		{
			switch (text.ToLower())
			{
			case "author":
				if (base.Document.BuiltinDocumentProperties.Author != null)
				{
					text2 = base.Document.BuiltinDocumentProperties.Author;
				}
				break;
			case "bytes":
				text2 = base.Document.BuiltinDocumentProperties.BytesCount.ToString();
				break;
			case "category":
				if (base.Document.BuiltinDocumentProperties.Category != null)
				{
					text2 = base.Document.BuiltinDocumentProperties.Category.ToString();
				}
				break;
			case "characters":
			case "characterswithspaces":
				text2 = base.Document.BuiltinDocumentProperties.CharCount.ToString();
				break;
			case "comments":
				text2 = ((base.Document.BuiltinDocumentProperties.Comments != null) ? base.Document.BuiltinDocumentProperties.Comments : text3);
				break;
			case "company":
				if (base.Document.BuiltinDocumentProperties.Company != null)
				{
					text2 = base.Document.BuiltinDocumentProperties.Company;
				}
				break;
			case "createtime":
				_ = base.Document.BuiltinDocumentProperties.CreateDate;
				text2 = base.Document.BuiltinDocumentProperties.CreateDate.ToString("g");
				break;
			case "keywords":
				text2 = ((base.Document.BuiltinDocumentProperties.Keywords != null) ? base.Document.BuiltinDocumentProperties.Keywords : text3);
				break;
			case "lastprinted":
				_ = base.Document.BuiltinDocumentProperties.LastPrinted;
				text2 = base.Document.BuiltinDocumentProperties.LastPrinted.ToString("g");
				break;
			case "lastsavedby":
				if (base.Document.BuiltinDocumentProperties.LastAuthor != null)
				{
					text2 = base.Document.BuiltinDocumentProperties.LastAuthor;
				}
				break;
			case "lastsavedtime":
				_ = base.Document.BuiltinDocumentProperties.LastSaveDate;
				text2 = base.Document.BuiltinDocumentProperties.LastSaveDate.ToString("g");
				break;
			case "lines":
				text2 = base.Document.BuiltinDocumentProperties.LinesCount.ToString();
				break;
			case "manager":
				if (base.Document.BuiltinDocumentProperties.Manager != null)
				{
					text2 = base.Document.BuiltinDocumentProperties.Manager;
				}
				break;
			case "nameofapplication":
				if (base.Document.BuiltinDocumentProperties.ApplicationName != null)
				{
					text2 = base.Document.BuiltinDocumentProperties.ApplicationName;
				}
				break;
			case "odmadocid":
				text2 = "Error! This property is only valid for ODMA documents.";
				break;
			case "pages":
				text2 = base.Document.BuiltinDocumentProperties.PageCount.ToString();
				break;
			case "paragraphs":
				text2 = base.Document.BuiltinDocumentProperties.ParagraphCount.ToString();
				break;
			case "revisionnumber":
				if (base.Document.BuiltinDocumentProperties.RevisionNumber != null)
				{
					text2 = base.Document.BuiltinDocumentProperties.RevisionNumber.ToString();
				}
				break;
			case "security":
				text2 = base.Document.BuiltinDocumentProperties.DocSecurity.ToString();
				break;
			case "subject":
				text2 = ((base.Document.BuiltinDocumentProperties.Subject != null) ? base.Document.BuiltinDocumentProperties.Subject : text3);
				break;
			case "template":
				if (base.Document.BuiltinDocumentProperties.Template != null)
				{
					text2 = base.Document.BuiltinDocumentProperties.Template;
				}
				break;
			case "title":
				text2 = ((base.Document.BuiltinDocumentProperties.Title != null) ? base.Document.BuiltinDocumentProperties.Title : text3);
				break;
			case "totaleditingtime":
				if (base.Document.BuiltinDocumentProperties.TotalEditingTime.TotalMinutes > 0.0)
				{
					text2 = base.Document.BuiltinDocumentProperties.TotalEditingTime.TotalMinutes.ToString();
				}
				break;
			case "words":
				text2 = base.Document.BuiltinDocumentProperties.WordCount.ToString();
				break;
			default:
				text2 = text3;
				break;
			}
		}
		if (text2 != text3)
		{
			text2 = UpdateTextFormat(text2);
			string numberFormat = string.Empty;
			text = RemoveMergeFormat(FieldCode, ref numberFormat);
			if (numberFormat.Contains(" "))
			{
				numberFormat = numberFormat.Remove(numberFormat.IndexOf(" "));
			}
			text2 = UpdateNumberFormat(text2, numberFormat);
		}
		UpdateFieldResult(text2);
	}

	internal string UpdateTextFormat(string text)
	{
		string fieldCode = FieldCode;
		if (fieldCode.Contains("\\"))
		{
			return UpdateTextFormat(text, fieldCode.Substring(fieldCode.IndexOf("\\")));
		}
		return text;
	}

	internal string UpdateTextFormat(string text, string formattingString)
	{
		bool pageFieldHasFormatText = false;
		return UpdateTextFormat(text, formattingString, ref pageFieldHasFormatText);
	}

	internal string UpdateTextFormat(string text, string formattingString, ref bool pageFieldHasFormatText)
	{
		string[] array = formattingString.Trim().Split(new char[1] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
		CultureInfo cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
		if (cultureInfo.Calendar is GregorianCalendar || cultureInfo.Calendar is EastAsianLunisolarCalendar || cultureInfo.Calendar is JulianCalendar || cultureInfo.Calendar is ChineseLunisolarCalendar)
		{
			cultureInfo.DateTimeFormat.Calendar.TwoDigitYearMax = 2029;
		}
		DateTime result;
		bool flag = DateTime.TryParse(text, cultureInfo, out result);
		bool isNum = false;
		bool flag2 = false;
		bool flag3 = false;
		string text2 = string.Empty;
		string text3 = text;
		foreach (string text4 in array)
		{
			if (text4.Length <= 0)
			{
				continue;
			}
			string number = GetNumber(text3, ref isNum);
			string text5 = ClearStringFromOtherCharacters(text4).Trim().ToUpper();
			switch (text4[0])
			{
			case '*':
				switch (text5)
				{
				case "UPPER":
					text = text.ToUpper();
					break;
				case "LOWER":
					text = text.ToLower();
					break;
				case "CAPS":
					text = GetCapsstring(text);
					break;
				case "FIRSTCAP":
					if (this is WMergeField wMergeField && !string.IsNullOrEmpty(wMergeField.TextBefore))
					{
						string textBefore = wMergeField.TextBefore;
						if (char.IsLetter(textBefore[0]))
						{
							wMergeField.TextBefore = char.ToUpper(textBefore[0]) + textBefore.Substring(1);
							break;
						}
						int l;
						for (l = 0; l < textBefore.Length && !char.IsLetter(textBefore[l]); l++)
						{
						}
						if (l == textBefore.Length)
						{
							if (text != string.Empty && char.IsLetter(text[0]))
							{
								wMergeField.TextBefore = char.ToUpper(text[0]) + text.Substring(1);
							}
						}
						else
						{
							wMergeField.TextBefore = textBefore.Substring(0, l) + char.ToUpper(textBefore[l]) + textBefore.Substring(l + 1);
						}
					}
					else if (text != string.Empty)
					{
						string text9 = text[0].ToString().ToUpper();
						text = text.Remove(0, 1);
						text = text9 + text;
					}
					break;
				case "ARABICDASH":
				case "ARABIC":
					pageFieldHasFormatText = true;
					break;
				case "ROMAN":
					pageFieldHasFormatText = true;
					if (isNum && !flag)
					{
						text = base.Document.GetAsRoman(Convert.ToInt32(number));
						if (Regex.Replace(text4, "\\s+", "")[1] == 'r')
						{
							text = text.ToLower();
						}
					}
					else if (flag && !flag3)
					{
						if (flag2)
						{
							flag3 = true;
							text = GetDateValue(text2, result);
							text = base.Document.GetAsRoman(Convert.ToInt32(text));
						}
						else
						{
							text = string.Empty;
						}
					}
					break;
				case "ALPHABETIC":
					pageFieldHasFormatText = true;
					if (isNum && !flag)
					{
						text = base.Document.GetAsLetter(Convert.ToInt32(number));
						if (Regex.Replace(text4, "\\s+", "")[1] == 'a')
						{
							text = text.ToLower();
						}
					}
					else if (flag && !flag3)
					{
						if (flag2)
						{
							return "Error! Number cannot be represented in specified format.";
						}
						text = string.Empty;
					}
					break;
				case "HEX":
					if (isNum && !flag)
					{
						text = Convert.ToInt32(number).ToString("X");
					}
					else if (flag && !flag3)
					{
						if (flag2)
						{
							flag3 = true;
							text = GetDateValue(text2, result);
							text = Convert.ToInt32(text).ToString("X");
						}
						else
						{
							text = "0";
						}
					}
					break;
				case "ORDINAL":
					if (isNum && !flag)
					{
						text = GetOrdinalstring(ordinalString: true, number);
					}
					else if (flag && !flag3)
					{
						if (flag2)
						{
							flag3 = true;
							text = GetDateValue(text2, result);
							text = GetOrdinalstring(ordinalString: true, text);
						}
						else
						{
							text = "0TH";
						}
					}
					break;
				case "CARDTEXT":
					if (isNum && !flag)
					{
						text = base.Document.GetCardTextString(cardinalString: true, number.Contains(".") ? Math.Round(Convert.ToDecimal(number)).ToString() : number);
					}
					break;
				case "ORDTEXT":
					if (isNum && !flag)
					{
						text = base.Document.GetOrdTextString(ordinalString: true, number);
					}
					break;
				case "DOLLARTEXT":
					if (isNum && !flag)
					{
						text = base.Document.GetCardTextString(cardinalString: true, number.Contains(".") ? decimal.ToInt32(Convert.ToDecimal(number)).ToString() : number);
						double num = double.Parse(number);
						num -= (double)decimal.Truncate(Convert.ToDecimal(num));
						string text8 = $"{num:#.00}";
						text = text + " and " + text8.Substring(1) + "/100";
					}
					break;
				default:
					if (this != null && FieldType == FieldType.FieldFormula)
					{
						text = "Error! Unknown switch argument.";
					}
					break;
				}
				break;
			case '@':
				if (this is WMergeField && (this as WMergeField).DateFormat != string.Empty)
				{
					break;
				}
				text2 = ParseSwitches(text4, text4.IndexOf("@"));
				if (DateTime.TryParse(text, cultureInfo, out result))
				{
					flag2 = true;
					text = UpdateDateValue(text2, result);
				}
				else
				{
					if (!(text2.Replace('\u00a0', ' ') == "MMMM d, yyyy"))
					{
						break;
					}
					List<string> list = new List<string>();
					string text6 = string.Empty;
					for (int j = 0; j < text.Length; j++)
					{
						if (text[j] == '-' && text[j - 1] != '-')
						{
							list.Add(text6);
							text6 = string.Empty;
						}
						else
						{
							text6 += text[j];
						}
						if (j == text.Length - 1)
						{
							list.Add(text6);
						}
					}
					if (list.Count != 3)
					{
						break;
					}
					string text7 = string.Empty;
					for (int k = 0; k < list.Count; k++)
					{
						if (int.TryParse(list[k], out var result2))
						{
							if (k == 2 && result2 < 0)
							{
								text7 += DateTime.Now.Year;
								continue;
							}
							text7 += result2;
							text7 += "-";
							continue;
						}
						text7 = string.Empty;
						break;
					}
					if (DateTime.TryParse(text7, cultureInfo, out result))
					{
						flag2 = true;
						text = UpdateDateValue(text2, result);
					}
				}
				break;
			}
		}
		return text;
	}

	private string GetCapsstring(string text)
	{
		char[] obj = new char[2] { ' ', '-' };
		text = text.ToLower();
		char[] array = obj;
		foreach (char separator in array)
		{
			text = CapsConversion(text, separator);
		}
		return text;
	}

	private string CapsConversion(string text, char separator)
	{
		string[] array = text.Split(new char[1] { separator });
		text = string.Empty;
		for (int i = 0; i < array.Length; i++)
		{
			string text2 = array[i];
			if (text2 != string.Empty)
			{
				string text3 = text2[0].ToString().ToUpper();
				text2 = text2.Remove(0, 1);
				text = text + text3 + text2;
			}
			text += separator;
		}
		text = text.TrimEnd(new char[1] { separator });
		return text;
	}

	private string GetNumberFormat(string numberFormat)
	{
		int num = numberFormat.LastIndexOf(NumberFormatInfo.InvariantInfo.CurrencyGroupSeparator);
		int num2 = numberFormat.LastIndexOf(NumberFormatInfo.InvariantInfo.CurrencyDecimalSeparator);
		int num3 = numberFormat.IndexOf(NumberFormatInfo.InvariantInfo.CurrencyGroupSeparator);
		if (num2 == -1 && num > -1 && num3 == num)
		{
			bool flag = false;
			if (numberFormat.StartsWith("$"))
			{
				flag = true;
				numberFormat = numberFormat.TrimStart('$');
			}
			if (numberFormat.StartsWith(",0"))
			{
				numberFormat = "0" + numberFormat;
			}
			else if (numberFormat.EndsWith("0,"))
			{
				numberFormat += "0";
			}
			numberFormat = (flag ? ("$" + numberFormat) : numberFormat);
			num = numberFormat.LastIndexOf(NumberFormatInfo.InvariantInfo.CurrencyGroupSeparator);
		}
		if ((num > -1 && num2 > -1 && num < num2) || (NumberFormatInfo.InvariantInfo.CurrencyGroupSeparator == NumberFormatInfo.CurrentInfo.CurrencyGroupSeparator && NumberFormatInfo.InvariantInfo.CurrencyDecimalSeparator == NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator))
		{
			return numberFormat;
		}
		string text = numberFormat;
		if (NumberFormatInfo.CurrentInfo.CurrencyGroupSeparator == ControlChar.NonBreakingSpace)
		{
			text = numberFormat.Replace(ControlChar.Space, ControlChar.NonBreakingSpace);
		}
		else if (NumberFormatInfo.CurrentInfo.CurrencyGroupSeparator == ControlChar.Space)
		{
			text = numberFormat.Replace(ControlChar.NonBreakingSpace, ControlChar.Space);
		}
		num = text.LastIndexOf(NumberFormatInfo.CurrentInfo.CurrencyGroupSeparator);
		num2 = text.LastIndexOf(NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator);
		if (num > -1 && num2 > -1 && num < num2)
		{
			return ChangeToInvariantFormat(text);
		}
		if (num2 > -1)
		{
			return ChangeToInvariantFormat(text);
		}
		return numberFormat;
	}

	private string ChangeToInvariantFormat(string tempNumberFormat)
	{
		string text = "";
		bool flag = false;
		for (int num = tempNumberFormat.Length - 1; num >= 0; num--)
		{
			string text2 = tempNumberFormat[num].ToString();
			if (!flag && text2 == NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator)
			{
				text2 = NumberFormatInfo.InvariantInfo.CurrencyDecimalSeparator;
				flag = true;
			}
			else if (flag && text2 == NumberFormatInfo.CurrentInfo.CurrencyGroupSeparator)
			{
				text2 = NumberFormatInfo.InvariantInfo.CurrencyGroupSeparator;
			}
			text = text2 + text;
		}
		tempNumberFormat = null;
		return text;
	}

	private void UpdateRefField()
	{
		string[] array = InternalFieldCode.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 1)
		{
			UpdateFieldResult("Error! No bookmark name given.");
			return;
		}
		bool flag = false;
		ReferenceKind referenceKind = ReferenceKind.ContentText;
		bool isHiddenBookmark = false;
		BookmarkStart bookmarkOfCrossRefField = GetBookmarkOfCrossRefField(ref isHiddenBookmark, isReturnHiddenBookmark: true);
		if (bookmarkOfCrossRefField != null)
		{
			for (int i = 2; i < array.Length; i++)
			{
				switch (array[i].ToLower())
				{
				case "\\p":
					if (flag)
					{
						referenceKind = ReferenceKind.AboveBelow;
					}
					flag = true;
					break;
				case "\\r":
					referenceKind = ReferenceKind.NumberRelativeContext;
					break;
				case "\\n":
					referenceKind = ReferenceKind.NumberNoContext;
					break;
				case "\\w":
					referenceKind = ReferenceKind.NumberFullContext;
					break;
				}
			}
			if (referenceKind == ReferenceKind.NumberRelativeContext || referenceKind == ReferenceKind.NumberNoContext || referenceKind == ReferenceKind.NumberFullContext)
			{
				return;
			}
			if ((referenceKind == ReferenceKind.ContentText && !flag) || (((referenceKind == ReferenceKind.ContentText && flag) || referenceKind == ReferenceKind.AboveBelow) && !CompareOwnerTextBody(bookmarkOfCrossRefField)))
			{
				if (IsBookmarkSelfReference(bookmarkOfCrossRefField.Name))
				{
					UpdateFieldResult("Error! Not a valid bookmark self-reference.");
					return;
				}
				BookmarksNavigator bookmarksNavigator = new BookmarksNavigator(base.Document);
				bookmarksNavigator.MoveToBookmark(bookmarkOfCrossRefField.Name);
				TextBodyPart bookmarkContent = bookmarksNavigator.GetBookmarkContent();
				List<Entity> list = new List<Entity>();
				foreach (TextBodyItem bodyItem in bookmarkContent.BodyItems)
				{
					if (bodyItem is WParagraph)
					{
						Entity nextItem = null;
						int itemIndex = 0;
						List<Entity> clonedParagraph = GetClonedParagraph(bodyItem, null, ref nextItem, ref itemIndex, isRefFieldUpdate: true);
						if (list.Count == 0 && clonedParagraph[0] is WParagraph)
						{
							foreach (ParagraphItem item in (clonedParagraph[0] as WParagraph).Items)
							{
								int num = FieldEnd.Index - (((FieldSeparator == null) ? Index : FieldSeparator.Index) + 1);
								if ((clonedParagraph[0] as WParagraph).Items.Count > 0 && num == 1 && FormattingString.ToUpper().Contains("\\* MERGEFORMAT") && item is WTextRange)
								{
									WCharacterFormat characterFormatValue = GetCharacterFormatValue();
									if (characterFormatValue.HasValue(106))
									{
										characterFormatValue.PropertiesHash.Remove(106);
									}
									(item as WTextRange).CharacterFormat.ImportContainer(characterFormatValue);
									(item as WTextRange).CharacterFormat.CopyProperties(characterFormatValue);
									characterFormatValue = null;
								}
								else if ((clonedParagraph[0] as WParagraph).Items.Count > 0 && num > 1 && FormattingString.ToUpper().Contains("\\* MERGEFORMAT") && item is WTextRange)
								{
									WCharacterFormat characterFormatValue2 = GetCharacterFormatValue(item.Index);
									if (characterFormatValue2.HasValue(106))
									{
										characterFormatValue2.PropertiesHash.Remove(106);
									}
									(item as WTextRange).CharacterFormat.ImportContainer(characterFormatValue2);
									(item as WTextRange).CharacterFormat.CopyProperties(characterFormatValue2);
									characterFormatValue2 = null;
								}
								list.Add(item);
							}
							continue;
						}
						foreach (Entity item2 in clonedParagraph)
						{
							list.Add(item2);
						}
					}
					else if (bodyItem is WTable)
					{
						list.Add(GetClonedTable(bodyItem, isRefFieldUpdate: true));
					}
				}
				UpdateRefFieldResult(list);
			}
			else if (((referenceKind == ReferenceKind.ContentText && flag) || referenceKind == ReferenceKind.AboveBelow) && CompareOwnerTextBody(bookmarkOfCrossRefField))
			{
				UpdateFieldResult(GetPositionValue(bookmarkOfCrossRefField));
			}
		}
		else
		{
			UpdateFieldResult("Error! Reference source not found.");
		}
	}

	private bool IsBookmarkSelfReference(string bkmkName)
	{
		Bookmark bookmark = base.Document.Bookmarks.FindByName(bkmkName);
		BookmarkStart bookmarkStart = bookmark.BookmarkStart;
		BookmarkEnd bookmarkEnd = bookmark.BookmarkEnd;
		WParagraph ownerParagraph = bookmarkStart.OwnerParagraph;
		int startIndex = bookmarkStart.GetIndexInOwnerCollection() + 1;
		bool isFieldReached = false;
		bool isBookmarkEndReached = false;
		CheckRefBookmarkParaItems(startIndex, ownerParagraph.Items, bookmarkEnd, ref isFieldReached, ref isBookmarkEndReached);
		if (!isBookmarkEndReached && !isFieldReached)
		{
			CheckRefBkmkParaInTableAndBlockCC(ownerParagraph, bookmarkEnd, ref isFieldReached, ref isBookmarkEndReached);
		}
		if (!isBookmarkEndReached && !isFieldReached)
		{
			CheckRefBookmarkBody(ownerParagraph, bookmarkEnd, ref isFieldReached, ref isBookmarkEndReached);
		}
		if (isFieldReached)
		{
			return true;
		}
		return false;
	}

	private void CheckRefBookmarkParaItems(int startIndex, ParagraphItemCollection paragraphItems, BookmarkEnd bkmkEnd, ref bool isFieldReached, ref bool isBookmarkEndReached)
	{
		for (int i = startIndex; i < paragraphItems.Count; i++)
		{
			IEntity entity = paragraphItems[i];
			if (entity.EntityType == EntityType.Field)
			{
				if (entity as WField == this)
				{
					isFieldReached = true;
					break;
				}
				continue;
			}
			if (entity.EntityType == EntityType.BookmarkEnd && entity == bkmkEnd)
			{
				isBookmarkEndReached = true;
				break;
			}
			if (entity.EntityType == EntityType.InlineContentControl)
			{
				InlineContentControl inlineContentControl = entity as InlineContentControl;
				CheckRefBookmarkParaItems(0, inlineContentControl.ParagraphItems, bkmkEnd, ref isFieldReached, ref isBookmarkEndReached);
				if (isBookmarkEndReached | isFieldReached)
				{
					break;
				}
			}
		}
	}

	private void CheckRefBookmarkBodyItems(int startIndex, BodyItemCollection bodyItems, BookmarkEnd bkmkEnd, ref bool isFieldReached, ref bool isBookmarkEndReached)
	{
		for (int i = startIndex; i < bodyItems.Count; i++)
		{
			IEntity entity = bodyItems[i];
			if (entity.EntityType == EntityType.Paragraph)
			{
				WParagraph wParagraph = entity as WParagraph;
				CheckRefBookmarkParaItems(0, wParagraph.Items, bkmkEnd, ref isFieldReached, ref isBookmarkEndReached);
				if (isBookmarkEndReached | isFieldReached)
				{
					break;
				}
			}
			else if (entity.EntityType == EntityType.Table)
			{
				foreach (WTableRow row in (entity as WTable).Rows)
				{
					foreach (WTableCell cell in row.Cells)
					{
						CheckRefBookmarkBodyItems(0, cell.Items, bkmkEnd, ref isFieldReached, ref isBookmarkEndReached);
						if (isBookmarkEndReached | isFieldReached)
						{
							return;
						}
					}
				}
			}
			else if (entity.EntityType == EntityType.BlockContentControl)
			{
				BlockContentControl blockContentControl = entity as BlockContentControl;
				CheckRefBookmarkBodyItems(0, blockContentControl.TextBody.Items, bkmkEnd, ref isFieldReached, ref isBookmarkEndReached);
				if (isBookmarkEndReached | isFieldReached)
				{
					break;
				}
			}
		}
	}

	private void CheckRefBkmkParaInTableAndBlockCC(WParagraph paragraphStart, BookmarkEnd bkmkEnd, ref bool isFieldReached, ref bool isBookmarkEndReached)
	{
		if (paragraphStart.Owner is WTableCell)
		{
			WTableCell wTableCell = paragraphStart.Owner as WTableCell;
			WTable wTable = (WTable)wTableCell.Owner.Owner;
			WTable wTable2 = wTable;
			int nextItemIndex = paragraphStart.GetIndexInOwnerCollection() + 1;
			CheckRefBookmarkTable(wTable, wTableCell, nextItemIndex, bkmkEnd, ref isFieldReached, ref isBookmarkEndReached);
			if (isBookmarkEndReached | isFieldReached)
			{
				return;
			}
			while (wTable2.IsInCell)
			{
				WTableCell wTableCell2 = wTable2.Owner as WTableCell;
				WTable wTable3 = wTableCell2.Owner.Owner as WTable;
				wTable2 = wTable3;
				int nextItemIndex2 = wTable.GetIndexInOwnerCollection() + 1;
				CheckRefBookmarkTable(wTable3, wTableCell2, nextItemIndex2, bkmkEnd, ref isFieldReached, ref isBookmarkEndReached);
				if (isBookmarkEndReached | isFieldReached)
				{
					break;
				}
			}
		}
		else if (paragraphStart.Owner is BlockContentControl)
		{
			BlockContentControl blockContentControl = paragraphStart.Owner as BlockContentControl;
			CheckRefBookmarkBodyItems(paragraphStart.GetIndexInOwnerCollection() + 1, blockContentControl.TextBody.Items, bkmkEnd, ref isFieldReached, ref isBookmarkEndReached);
			_ = isBookmarkEndReached | isFieldReached;
		}
	}

	private void CheckRefBookmarkTable(WTable table, WTableCell tableCell, int nextItemIndex, BookmarkEnd bkmkEnd, ref bool isFieldReached, ref bool isBookmarkEndReached)
	{
		int indexInOwnerCollection = tableCell.Owner.GetIndexInOwnerCollection();
		int indexInOwnerCollection2 = tableCell.GetIndexInOwnerCollection();
		for (int i = indexInOwnerCollection; i < table.Rows.Count; i++)
		{
			WTableRow wTableRow = table.Rows[i];
			for (int j = ((wTableRow == tableCell.Owner) ? indexInOwnerCollection2 : 0); j < wTableRow.Cells.Count; j++)
			{
				WTableCell wTableCell = wTableRow.Cells[j];
				int startIndex = ((wTableCell == tableCell) ? nextItemIndex : 0);
				CheckRefBookmarkBodyItems(startIndex, wTableCell.Items, bkmkEnd, ref isFieldReached, ref isBookmarkEndReached);
				if (isBookmarkEndReached | isFieldReached)
				{
					return;
				}
			}
		}
	}

	private void CheckRefBookmarkBody(WParagraph paragraphStart, BookmarkEnd bkmkEnd, ref bool isFieldReached, ref bool isBookmarkEndReached)
	{
		if (paragraphStart.Owner is WTableCell)
		{
			WTable wTable = GetOwnerTable(paragraphStart) as WTable;
			int startIndex = wTable.GetIndexInOwnerCollection() + 1;
			if (wTable.OwnerTextBody.Owner is WSection)
			{
				CheckRefBookmarkBodyItems(startIndex, wTable.OwnerTextBody.Items, bkmkEnd, ref isFieldReached, ref isBookmarkEndReached);
				_ = isBookmarkEndReached | isFieldReached;
			}
		}
		else if (paragraphStart.Owner is BlockContentControl && (paragraphStart.Owner as WTextBody).Owner is WSection)
		{
			BlockContentControl blockContentControl = paragraphStart.Owner as BlockContentControl;
			int startIndex2 = blockContentControl.GetIndexInOwnerCollection() + 1;
			if (blockContentControl.OwnerTextBody.Owner is WSection)
			{
				CheckRefBookmarkBodyItems(startIndex2, blockContentControl.OwnerTextBody.Items, bkmkEnd, ref isFieldReached, ref isBookmarkEndReached);
				_ = isBookmarkEndReached | isFieldReached;
			}
		}
		else if (paragraphStart.Owner is WTextBody && (paragraphStart.Owner as WTextBody).Owner is WSection)
		{
			WTextBody ownerTextBody = paragraphStart.OwnerTextBody;
			int startIndex3 = paragraphStart.GetIndexInOwnerCollection() + 1;
			CheckRefBookmarkBodyItems(startIndex3, ownerTextBody.Items, bkmkEnd, ref isFieldReached, ref isBookmarkEndReached);
			_ = isBookmarkEndReached | isFieldReached;
		}
	}

	internal bool CompareOwnerTextBody(Entity bookmark)
	{
		WTextBody ownerTextBody = (bookmark as ParagraphItem).OwnerParagraph.OwnerTextBody;
		WTextBody ownerTextBody2 = base.OwnerParagraph.OwnerTextBody;
		Entity ownerEntity = (bookmark as ParagraphItem).OwnerParagraph.GetOwnerEntity();
		Entity ownerEntity2 = base.OwnerParagraph.GetOwnerEntity();
		if (((!base.OwnerParagraph.IsInCell && !(ownerTextBody2.Owner is WSection) && !(ownerTextBody2.Owner is BlockContentControl)) || ownerTextBody2 is HeaderFooter || (!(bookmark as ParagraphItem).OwnerParagraph.IsInCell && !(ownerTextBody.Owner is WSection) && !(ownerTextBody.Owner is BlockContentControl)) || ownerTextBody is HeaderFooter) && (!(ownerTextBody2 is HeaderFooter) || !(ownerTextBody is HeaderFooter)) && (!(ownerTextBody2.Owner is WFootnote) || (ownerTextBody2.Owner as WFootnote).FootnoteType != 0 || !(ownerTextBody.Owner is WFootnote) || (ownerTextBody.Owner as WFootnote).FootnoteType != 0) && (!(ownerTextBody2.Owner is WFootnote) || (ownerTextBody2.Owner as WFootnote).FootnoteType != FootnoteType.Endnote || !(ownerTextBody.Owner is WFootnote) || (ownerTextBody.Owner as WFootnote).FootnoteType != FootnoteType.Endnote) && ((!(ownerEntity2 is Shape) && !(ownerEntity2 is WTextBox)) || (!(ownerEntity is Shape) && !(ownerEntity is WTextBox))))
		{
			if (ownerTextBody2.Owner is WComment)
			{
				return ownerTextBody.Owner is WComment;
			}
			return false;
		}
		return true;
	}

	private void UpdateUnKnownFieldResult(List<Entity> fieldResult)
	{
		CheckFieldSeparator();
		RemovePreviousResult();
		WParagraph ownerParagraph = base.OwnerParagraph;
		if (FieldSeparator.OwnerParagraph == FieldEnd.OwnerParagraph)
		{
			ownerParagraph.ChildEntities.Remove(FieldEnd);
		}
		int num = FieldSeparator.GetIndexInOwnerCollection() + 1;
		int num2 = FieldSeparator.OwnerParagraph.GetIndexInOwnerCollection() + 1;
		WTextBody ownerTextBody = FieldSeparator.OwnerParagraph.OwnerTextBody;
		for (int i = 0; i < fieldResult.Count; i++)
		{
			if (fieldResult[i] is ParagraphItem)
			{
				FieldSeparator.OwnerParagraph.Items.Insert(num, fieldResult[i]);
				num++;
				if (i == fieldResult.Count - 1 && FieldSeparator.OwnerParagraph != FieldEnd.OwnerParagraph)
				{
					FieldSeparator.OwnerParagraph.Items.Insert(num, FieldEnd);
				}
				continue;
			}
			if (FieldEnd.OwnerParagraph == null)
			{
				ownerParagraph = new WParagraph(base.Document);
				ownerParagraph.ChildEntities.Add(FieldEnd);
				ownerTextBody.Items.Add(ownerParagraph);
			}
			if (i == fieldResult.Count - 1 && fieldResult[i] is WParagraph)
			{
				if (FieldSeparator.OwnerParagraph == FieldEnd.OwnerParagraph)
				{
					(ownerTextBody.Items[num2] as WParagraph).Items.Add(FieldEnd);
				}
				else
				{
					int count = (fieldResult[i] as WParagraph).ChildEntities.Count;
					for (int j = 0; j < count; j++)
					{
						FieldEnd.OwnerParagraph.ChildEntities.Insert(0, (fieldResult[i] as WParagraph).ChildEntities[(fieldResult[i] as WParagraph).ChildEntities.Count - 1]);
					}
				}
			}
			else
			{
				ownerTextBody.Items.Insert(num2, fieldResult[i]);
			}
			num2++;
		}
	}

	private void UpdateRefFieldResult(List<Entity> fieldResult)
	{
		CheckFieldSeparator();
		RemovePreviousResult();
		int num = FieldSeparator.GetIndexInOwnerCollection() + 1;
		int num2 = FieldSeparator.OwnerParagraph.GetIndexInOwnerCollection() + 1;
		WTextBody ownerTextBody = FieldSeparator.OwnerParagraph.OwnerTextBody;
		for (int i = 0; i < fieldResult.Count; i++)
		{
			if (fieldResult[i] is ParagraphItem)
			{
				FieldSeparator.OwnerParagraph.Items.Insert(num, fieldResult[i]);
				num++;
			}
			else if (i == fieldResult.Count - 1 && FieldSeparator.OwnerParagraph == FieldEnd.OwnerParagraph)
			{
				if (!(fieldResult[i] is WParagraph))
				{
					continue;
				}
				WParagraph wParagraph = fieldResult[i] as WParagraph;
				bool isHiddenBookmark = false;
				BookmarkStart bookmarkOfCrossRefField = GetBookmarkOfCrossRefField(ref isHiddenBookmark, isReturnHiddenBookmark: true);
				if (base.Document.Bookmarks.FindByName(bookmarkOfCrossRefField.Name).BookmarkEnd.IsAfterParagraphMark)
				{
					ownerTextBody.Items.Insert(num2, wParagraph);
					num2++;
					WParagraph wParagraph2 = FieldEnd.OwnerParagraph.Clone() as WParagraph;
					wParagraph2.ClearItems();
					MoveFieldEndToLastPara(wParagraph2);
					ownerTextBody.Items.Insert(num2, wParagraph2);
					continue;
				}
				WParagraph wParagraph3 = FieldEnd.OwnerParagraph.Clone() as WParagraph;
				wParagraph3.ClearItems();
				int num3;
				for (num3 = 0; num3 < wParagraph.ChildEntities.Count; num3++)
				{
					wParagraph3.ChildEntities.Add(wParagraph.ChildEntities[num3]);
					num3--;
				}
				MoveFieldEndToLastPara(wParagraph3);
				ownerTextBody.Items.Insert(num2, wParagraph3);
			}
			else
			{
				ownerTextBody.Items.Insert(num2, fieldResult[i]);
				num2++;
			}
		}
	}

	private void MoveFieldEndToLastPara(WParagraph para)
	{
		int indexInOwnerCollection = FieldEnd.GetIndexInOwnerCollection();
		WParagraph ownerParagraph = FieldEnd.OwnerParagraph;
		int num;
		for (num = indexInOwnerCollection; num < ownerParagraph.ChildEntities.Count; num++)
		{
			WFieldMark wFieldMark = null;
			if (ownerParagraph.ChildEntities[num] is WField && (ownerParagraph.ChildEntities[num] as WField).FieldEnd != null)
			{
				wFieldMark = (ownerParagraph.ChildEntities[num] as WField).FieldEnd;
				wFieldMark.SetOwner((ownerParagraph.ChildEntities[num] as WField).FieldEnd.OwnerParagraph);
				(ownerParagraph.ChildEntities[num] as WField).FieldEnd = null;
			}
			para.ChildEntities.Add(ownerParagraph.ChildEntities[num]);
			if (para.ChildEntities[para.ChildEntities.Count - 1] is WField)
			{
				WField wField = para.ChildEntities[para.ChildEntities.Count - 1] as WField;
				if (wField.FieldEnd == null || !(wField.FieldEnd.Owner is WParagraph))
				{
					wField.FieldEnd = wFieldMark;
				}
				if (wField.FieldSeparator == null || !(wField.FieldSeparator.Owner is WParagraph))
				{
					wField.FieldSeparator = null;
				}
			}
			num--;
		}
		if (ownerParagraph.ChildEntities.Count == 0)
		{
			ownerParagraph.RemoveSelf();
		}
	}

	internal string GetHierarchicalIndex(Entity entity)
	{
		string text = entity.GetIndexInOwnerCollection() + ";";
		Entity owner = entity.Owner;
		while (owner != base.Document)
		{
			text = ((owner is WParagraph) ? "WP" : ((owner is WTable) ? "WT" : ((owner is WTableRow) ? "WTR" : ((owner is WTableCell) ? "WTC" : ((owner is WSection) ? "WS" : ((owner is WTextBody) ? "WTB" : ((owner is WComment) ? "WC" : ((!(owner is WFootnote)) ? ((owner is BlockContentControl) ? "SDTB" : "") : (((owner as WFootnote).FootnoteType == FootnoteType.Footnote) ? "WFF" : "WFE"))))))))) + owner.GetIndexInOwnerCollection() + ";" + text;
			owner = owner.Owner;
			if (owner is BlockContentControl)
			{
				owner = owner.Owner;
			}
			if (owner == null || owner is WordDocument || owner is Shape || owner is WTextBox)
			{
				break;
			}
		}
		return text;
	}

	internal string GetPositionValue(BookmarkStart bkStart)
	{
		bool flag = false;
		WParagraph ownerParagraph = bkStart.OwnerParagraph;
		WTextBody entityOwnerTextBody = base.Document.GetEntityOwnerTextBody(ownerParagraph);
		WParagraph ownerParagraph2 = base.OwnerParagraph;
		WTextBody entityOwnerTextBody2 = base.Document.GetEntityOwnerTextBody(ownerParagraph2);
		if (entityOwnerTextBody2 is HeaderFooter && entityOwnerTextBody is HeaderFooter)
		{
			if (entityOwnerTextBody == entityOwnerTextBody2)
			{
				if (ownerParagraph == ownerParagraph2)
				{
					if (bkStart.GetIndexInOwnerCollection() < GetIndexInOwnerCollection())
					{
						flag = true;
					}
				}
				else if (ownerParagraph.GetIndexInOwnerCollection() < ownerParagraph2.GetIndexInOwnerCollection())
				{
					flag = true;
				}
			}
			else if ((entityOwnerTextBody as HeaderFooter).Type == HeaderFooterType.FirstPageHeader || (entityOwnerTextBody as HeaderFooter).Type == HeaderFooterType.OddHeader || (entityOwnerTextBody as HeaderFooter).Type == HeaderFooterType.EvenHeader)
			{
				flag = true;
			}
		}
		else if (entityOwnerTextBody2.Owner is WComment && entityOwnerTextBody.Owner is WComment)
		{
			flag = CompareHierarchicalIndex(GetHierarchicalIndex(bkStart), GetHierarchicalIndex(this));
		}
		else if ((!(entityOwnerTextBody2.Owner is Shape) && !(entityOwnerTextBody2.Owner is WTextBox)) || (!(entityOwnerTextBody.Owner is Shape) && !(entityOwnerTextBody.Owner is WTextBox)))
		{
			flag = ((entityOwnerTextBody2.Owner is WFootnote && (entityOwnerTextBody2.Owner as WFootnote).FootnoteType == FootnoteType.Footnote && entityOwnerTextBody.Owner is WFootnote && (entityOwnerTextBody.Owner as WFootnote).FootnoteType == FootnoteType.Footnote) ? CompareHierarchicalIndex(GetHierarchicalIndex(bkStart), GetHierarchicalIndex(this)) : ((!(entityOwnerTextBody2.Owner is WFootnote) || (entityOwnerTextBody2.Owner as WFootnote).FootnoteType != FootnoteType.Endnote || !(entityOwnerTextBody.Owner is WFootnote) || (entityOwnerTextBody.Owner as WFootnote).FootnoteType != FootnoteType.Endnote) ? CompareHierarchicalIndex(GetHierarchicalIndex(bkStart), GetHierarchicalIndex(this)) : CompareHierarchicalIndex(GetHierarchicalIndex(bkStart), GetHierarchicalIndex(this))));
		}
		else if (entityOwnerTextBody2 == entityOwnerTextBody)
		{
			flag = CompareHierarchicalIndex(GetHierarchicalIndex(bkStart), GetHierarchicalIndex(this));
		}
		else if ((entityOwnerTextBody2.Owner as ParagraphItem).OwnerParagraph == (entityOwnerTextBody.Owner as ParagraphItem).OwnerParagraph)
		{
			if ((entityOwnerTextBody.Owner as ParagraphItem).GetIndexInOwnerCollection() < (entityOwnerTextBody2.Owner as ParagraphItem).GetIndexInOwnerCollection())
			{
				flag = true;
			}
		}
		else
		{
			flag = CompareHierarchicalIndex(GetHierarchicalIndex((entityOwnerTextBody.Owner as ParagraphItem).OwnerParagraph), GetHierarchicalIndex((entityOwnerTextBody2.Owner as ParagraphItem).OwnerParagraph));
		}
		if (!flag)
		{
			return "below";
		}
		return "above";
	}

	internal bool CompareHierarchicalIndex(string value1, string value2)
	{
		string[] array = value1.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
		string[] array2 = value2.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
		int num = ((array.Length < array2.Length) ? array.Length : array2.Length);
		for (int i = 0; i < num; i++)
		{
			if (array[i] != array2[i])
			{
				int num2 = int.Parse(Regex.Match(array[i], "\\d+").Value);
				int num3 = int.Parse(Regex.Match(array2[i], "\\d+").Value);
				if (num2 >= num3)
				{
					break;
				}
				return true;
			}
		}
		return false;
	}

	private string GetDateValue(string text, DateTime date)
	{
		int num = 0;
		int dateValue = 0;
		int num2 = 0;
		while (num2 < text.Length)
		{
			char c = text[num2];
			if ((uint)c <= 77u)
			{
				if (c == '\'')
				{
					goto IL_00ec;
				}
				if (c == 'D')
				{
					goto IL_0060;
				}
				if (c == 'M')
				{
					while (num2 < text.Length && text[num2] == 'M')
					{
						num2++;
						num++;
					}
					dateValue = UpdateCustomMonth(dateValue, date, num);
					num = 0;
					continue;
				}
			}
			else if ((uint)c <= 92u)
			{
				if (c == 'Y')
				{
					goto IL_00bf;
				}
				if (c == '\\')
				{
					goto IL_00ec;
				}
			}
			else
			{
				if (c == 'd')
				{
					goto IL_0060;
				}
				if (c == 'y')
				{
					goto IL_00bf;
				}
			}
			num2++;
			continue;
			IL_00bf:
			while (num2 < text.Length && (text[num2] == 'y' || text[num2] == 'Y'))
			{
				num2++;
				num++;
			}
			dateValue = UpdateCustomYear(dateValue, date, num);
			num = 0;
			continue;
			IL_00ec:
			num2++;
			continue;
			IL_0060:
			while (num2 < text.Length && (text[num2] == 'd' || text[num2] == 'D'))
			{
				num2++;
				num++;
			}
			dateValue = UpdateCustomDay(dateValue, date, num);
			num = 0;
		}
		return dateValue.ToString();
	}

	private int UpdateCustomDay(int dateValue, DateTime currentDateTime, int count)
	{
		int num = 0;
		num = (((uint)(count - 1) <= 1u) ? currentDateTime.Day : 0);
		dateValue += num;
		return num;
	}

	private int UpdateCustomMonth(int dateValue, DateTime currentDateTime, int count)
	{
		int num = 0;
		if ((uint)(count - 1) <= 1u)
		{
			num = currentDateTime.Month;
		}
		dateValue += num;
		return dateValue;
	}

	private int UpdateCustomYear(int dateValue, DateTime currentDateTime, int count)
	{
		int num = 0;
		num = (((uint)(count - 1) > 1u) ? currentDateTime.Year : (currentDateTime.Year % 100));
		dateValue += num;
		return dateValue;
	}

	private string GetNumber(string text, ref bool isNum)
	{
		isNum = int.TryParse(text, out var _);
		if (isNum)
		{
			return text;
		}
		string numberAlphabet = GetNumberAlphabet(text);
		if (numberAlphabet != string.Empty)
		{
			isNum = true;
			return numberAlphabet;
		}
		return text;
	}

	private string GetNumberAlphabet(string text)
	{
		string text2 = string.Empty;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (char.IsLetter(c))
			{
				break;
			}
			if (char.IsNumber(c) || c == '.')
			{
				text2 += c;
			}
		}
		return text2;
	}

	private string TrimBeginingText(string fieldCode)
	{
		while (fieldCode != string.Empty && (StartsWithExt(fieldCode, ControlChar.DoubleQuoteString) || StartsWithExt(fieldCode, ControlChar.LeftDoubleQuoteString) || StartsWithExt(fieldCode, ControlChar.RightDoubleQuoteString) || StartsWithExt(fieldCode, ControlChar.DoubleLowQuoteString)))
		{
			fieldCode = fieldCode.Remove(0, 1);
		}
		return fieldCode;
	}

	private string TrimEndText(string fieldCode)
	{
		while (fieldCode != string.Empty && (fieldCode.EndsWith(ControlChar.DoubleQuoteString) || fieldCode.EndsWith(ControlChar.LeftDoubleQuoteString) || fieldCode.EndsWith(ControlChar.RightDoubleQuoteString) || fieldCode.EndsWith(ControlChar.DoubleLowQuoteString)))
		{
			fieldCode = fieldCode.Substring(0, fieldCode.Length - 1);
		}
		return fieldCode;
	}

	protected string RemoveText(string text, string textToRevome)
	{
		return RemoveText(text, textToRevome, isTrim: true);
	}

	private string RemoveText(string text, string textToRevome, bool isTrim)
	{
		if (text.StartsWith(textToRevome, StringComparison.OrdinalIgnoreCase))
		{
			text = text.Substring(textToRevome.Length, text.Length - textToRevome.Length);
		}
		if (isTrim)
		{
			text = text.Trim();
		}
		return text;
	}

	protected List<string> SplitIfArguments(string text, ref List<int> operatorIndexForDoubleQuotes, ref string operatorValue)
	{
		List<string> list = new List<string>();
		List<string> operators = new List<string>(new string[6] { "<=", ">=", "<>", "=", "<", ">" });
		string condition = string.Empty;
		bool flag = false;
		bool isDoubleQuote = false;
		List<int> operatorIndex = GetOperatorIndex(operators, text, ref isDoubleQuote);
		if (isDoubleQuote)
		{
			operatorIndexForDoubleQuotes = new List<int>(operatorIndex);
			operatorValue = GetOperatorValue(operatorIndex, text);
		}
		if (operatorIndex.Count == 0)
		{
			return list;
		}
		try
		{
			while (text != string.Empty)
			{
				int tableStart = text.IndexOf('\u0013');
				if (StartsWithExt(text, "\""))
				{
					int length = text.Length;
					SplitFieldCode(tableStart, ref text, ref condition);
					UpdateOperatorIndex(operatorIndex, length - text.Length);
					if (isDoubleQuote && !string.IsNullOrEmpty(condition) && !flag && list.Count == 0)
					{
						UpdateOperatorIndexForDoubleQuote(operatorIndexForDoubleQuotes, condition.Length);
					}
				}
				else
				{
					SplitFieldCode(operators, list, operatorIndex, flag, ref text, ref condition);
				}
				if (flag && list.Count == 0)
				{
					list.Insert(0, condition);
					condition = string.Empty;
					flag = false;
				}
				else if (list.Count > 0)
				{
					list.Add(condition);
					condition = string.Empty;
					if (!StartsWithExt(text, "\"") && StartsWithExt(text.TrimStart(), "\""))
					{
						text = text.TrimStart();
					}
					list.Add(text.Trim('"'));
					text = string.Empty;
				}
				if (list.Count == 0)
				{
					flag = IsOperator(operators, ref text, ref condition);
				}
			}
		}
		catch
		{
			while (list.Count < 3)
			{
				list.Add(string.Empty);
			}
		}
		return list;
	}

	private void SplitFieldCode(int tableStart, ref string text, ref string condition)
	{
		text = text.Substring(text.IndexOf("\"") + 1);
		tableStart = text.IndexOf('\u0013');
		if (tableStart >= 0 && tableStart < text.IndexOf("\""))
		{
			condition += GetTextInTable(ref text);
		}
		condition += text.Substring(0, text.IndexOf("\""));
		text = text.Substring(text.IndexOf("\"") + 1);
	}

	private void SplitFieldCode(List<string> operators, List<string> arguments, List<int> operatorIndex, bool isOperator, ref string text, ref string condition)
	{
		for (int i = 0; i < operators.Count; i++)
		{
			if (arguments.Count != 0)
			{
				break;
			}
			if (text.Contains(operators[i]) && operatorIndex[0] == text.IndexOf(operators[i]))
			{
				condition += text.Substring(0, text.IndexOf(operators[i])).Trim(ControlChar.SpaceChar);
				text = text.Substring(text.IndexOf(operators[i])).Trim(ControlChar.SpaceChar);
				break;
			}
		}
		if (isOperator || arguments.Count > 0)
		{
			condition += text.Substring(0, text.IndexOf(" ")).Trim(ControlChar.SpaceChar);
			text = text.Substring(text.IndexOf(" ")).Trim(ControlChar.SpaceChar);
		}
	}

	private string GetOperatorValue(List<int> operatorIndex, string text)
	{
		if (operatorIndex != null && operatorIndex.Count > 0)
		{
			string text2 = string.Empty;
			{
				foreach (int item in operatorIndex)
				{
					text2 += text[item];
				}
				return text2;
			}
		}
		return null;
	}

	private void UpdateOperatorIndex(List<int> operatorsIndex, int count)
	{
		for (int i = 0; i < operatorsIndex.Count; i++)
		{
			operatorsIndex[i] -= count;
		}
	}

	private void UpdateOperatorIndexForDoubleQuote(List<int> operatorsIndex, int count)
	{
		for (int i = 0; i < operatorsIndex.Count; i++)
		{
			operatorsIndex[i] = count + i;
		}
	}

	private bool IsOperator(List<string> operators, ref string text, ref string condition)
	{
		for (int i = 0; i < operators.Count; i++)
		{
			if (StartsWithExt(text, operators[i]))
			{
				condition += operators[i];
				text = text.Substring(text.IndexOf(operators[i]) + operators[i].Length).Trim();
				return true;
			}
		}
		return false;
	}

	internal List<int> GetOperatorIndex(List<string> operators, string text, ref bool isDoubleQuote)
	{
		int num = -1;
		int num2 = -1;
		if (StartsWithExt(text, " "))
		{
			text = text.TrimStart(' ');
		}
		if (StartsWithExt(text, ControlChar.DoubleQuoteString))
		{
			isDoubleQuote = true;
			text.Split('"');
			num = text.IndexOf(ControlChar.DoubleQuote);
			for (int i = num + 1; i < text.Length; i++)
			{
				if (text[i] == ControlChar.DoubleQuote)
				{
					num2 = i;
					break;
				}
			}
		}
		List<int> list = new List<int>();
		for (int j = 0; j < operators.Count; j++)
		{
			if (text.Contains(operators[j]) && (num == -1 || text.IndexOf(operators[j]) > num2) && !list.Contains(text.IndexOf(operators[j])))
			{
				list.Add(text.IndexOf(operators[j]));
			}
		}
		list.Sort();
		return list;
	}

	private string GetTextInTable(ref string text)
	{
		string text2 = string.Empty;
		int num = text.IndexOf('\u0013');
		while (num >= 0 && num < text.IndexOf("\""))
		{
			string text3 = text.Substring(0, text.IndexOf('\u0015') + 1);
			text2 += text3;
			text = text.Substring(text.IndexOf('\u0015') + 1);
			for (int num2 = text3.Split('\u0013').Length - 2; num2 > 0; num2--)
			{
				text2 += text.Substring(0, text.IndexOf('\u0015') + 1);
				text = text.Substring(text.IndexOf('\u0015') + 1);
			}
			num = text.IndexOf('\u0013');
		}
		return text2;
	}

	protected string UpdateCondition(string text, List<int> operatorIndex, string operatorValue)
	{
		List<string> list = new List<string>(new string[6] { "<=", ">=", "<>", "=", "<", ">" });
		string[] array = new string[2];
		bool flag = false;
		if (!string.IsNullOrEmpty(operatorValue) && operatorIndex != null && operatorIndex[0] > 0)
		{
			flag = ValidateOperatorIndex(text, operatorValue, operatorIndex);
		}
		if (flag)
		{
			array[0] = text.Substring(0, operatorIndex[0]);
			array[1] = text.Substring(operatorIndex[operatorIndex.Count - 1] + 1);
		}
		else
		{
			array = text.Split(list.ToArray(), StringSplitOptions.RemoveEmptyEntries);
		}
		if (array.Length <= 1)
		{
			array = ((array.Length == 0) ? new string[2]
			{
				string.Empty,
				string.Empty
			} : ((string.IsNullOrEmpty(operatorValue) || text.IndexOf(operatorValue) != 0) ? new string[2]
			{
				array[0],
				string.Empty
			} : new string[2]
			{
				string.Empty,
				array[0]
			}));
		}
		else
		{
			array[0] = array[0].Trim(new char[1] { '"' });
			array[1] = array[1].Trim(new char[1] { '"' });
			if (FieldType != FieldType.FieldIf)
			{
				array[0] = array[0].Trim();
				array[1] = array[1].Trim();
			}
		}
		string text2 = "1";
		if (array.Length > 1)
		{
			double result;
			double result2;
			if (FieldType != FieldType.FieldIf)
			{
				if (!double.TryParse(array[0], out result))
				{
					try
					{
						bool isFieldCodeStartWithCurrencySymbol = false;
						array[0] = UpdateFormula(array[0].Trim(), ref isFieldCodeStartWithCurrencySymbol);
					}
					catch (Exception)
					{
					}
				}
				if (!double.TryParse(array[1], out result2))
				{
					try
					{
						bool isFieldCodeStartWithCurrencySymbol2 = false;
						array[1] = UpdateFormula(array[1].Trim(), ref isFieldCodeStartWithCurrencySymbol2);
					}
					catch (Exception)
					{
					}
				}
			}
			if (double.TryParse(array[0], out result) && double.TryParse(array[1], out result2))
			{
				foreach (string item in list)
				{
					if (text.Contains(item))
					{
						text2 = CompareExpression(result, result2, item);
						break;
					}
				}
			}
			else if (HasOperatorinText(flag, operatorValue, "=", text))
			{
				string text3 = array[1].Trim();
				if (!(text3 == "?"))
				{
					if (text3 == "*")
					{
						text2 = ((array[0].Trim().Length > 0) ? "1" : "0");
					}
					else
					{
						text2 = ((array[0] == array[1]) ? "1" : "0");
						if (text2 == "0")
						{
							text2 = (IsOperandEqual(array[0], array[1]) ? "1" : "0");
						}
					}
				}
				else
				{
					text2 = ((array[0].Trim().Length == 1) ? "1" : "0");
				}
			}
			else if (HasOperatorinText(flag, operatorValue, "<>", text))
			{
				string text3 = array[1].Trim();
				if (!(text3 == "?"))
				{
					if (text3 == "*")
					{
						text2 = ((array[0].Trim().Length > 0) ? "0" : "1");
					}
					else
					{
						text2 = ((array[0] != array[1]) ? "1" : "0");
						if (text2 == "1")
						{
							text2 = (IsOperandEqual(array[0], array[1]) ? "0" : "1");
						}
					}
				}
				else
				{
					text2 = ((array[0].Trim().Length != 1) ? "1" : "0");
				}
			}
			else if (array[0] == string.Empty && operatorValue != null && operatorValue.Contains(">"))
			{
				text2 = "0";
			}
		}
		return text2;
	}

	private bool IsOperandEqual(string operand0, string operand1)
	{
		if (!operand1.Contains("*") && !operand1.Contains("?"))
		{
			return false;
		}
		for (int i = 0; i < operand1.Length; i++)
		{
			if ((operand0.Length > i && operand0[i] == operand1[i] && operand0[i] != '*') || operand1[i] == '?')
			{
				continue;
			}
			if (operand1[i] == '*')
			{
				string text = operand0.Substring(i);
				string text2 = operand1.Substring(i + 1);
				if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2))
				{
					return false;
				}
				if (i + 1 > text2.Length || i + 1 > text.Length || text == text2)
				{
					return true;
				}
				if (i >= text.Length)
				{
					continue;
				}
				text = text.Substring(i);
				if (i < text2.Length)
				{
					text2 = text2.Substring(i);
					int num = text.Length - 1;
					if (text2.Contains(text))
					{
						int startIndex = text2.Length - text.Length;
						text2 = text2.Substring(startIndex);
						while (num >= 0)
						{
							if (num < text.Length && text2[num] != text[num] && text2[num] != '?')
							{
								return false;
							}
							num--;
						}
						return true;
					}
					int num2 = text2.Length - 1;
					if (text.Length > text2.Length)
					{
						int startIndex2 = text.Length - text2.Length;
						text = text.Substring(startIndex2);
					}
					while (num2 >= 0)
					{
						if (num2 < text.Length && text2[num2] != text[num2] && text2[num2] != '?')
						{
							return false;
						}
						num2--;
					}
					return true;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	private bool HasOperatorinText(bool isOperatorAtIndex, string operatorValue, string expectedoperator, string text)
	{
		if (!isOperatorAtIndex)
		{
			return text.Contains(expectedoperator);
		}
		return expectedoperator == operatorValue;
	}

	private bool ValidateOperatorIndex(string text, string expectedOperator, List<int> operatorIndex)
	{
		if (operatorIndex.Count > 0 && operatorIndex[operatorIndex.Count - 1] < text.Length)
		{
			string text2 = string.Empty;
			foreach (int item in operatorIndex)
			{
				text2 += text[item];
			}
			return expectedOperator == text2;
		}
		return false;
	}

	private void UpdateCompareField()
	{
		string numberFormat = string.Empty;
		string text = RemoveMergeFormat(FieldCode, ref numberFormat);
		text = RemoveText(text, "compare");
		string empty = string.Empty;
		try
		{
			empty = UpdateCondition(text, null, null);
		}
		catch
		{
			empty = "Error! Unknown op code for conditional.";
		}
		UpdateFieldResult(empty);
	}

	private string CompareExpression(double operand1, double operand2, string operation)
	{
		double num = 0.0;
		switch (operation)
		{
		case "=":
			num = ((operand1 == operand2) ? 1 : 0);
			break;
		case "<":
			num = ((operand1 < operand2) ? 1 : 0);
			break;
		case "<=":
			num = ((operand1 <= operand2) ? 1 : 0);
			break;
		case ">":
			num = ((operand1 > operand2) ? 1 : 0);
			break;
		case ">=":
			num = ((operand1 >= operand2) ? 1 : 0);
			break;
		case "<>":
			num = ((operand1 != operand2) ? 1 : 0);
			break;
		}
		return num.ToString(CultureInfo.InvariantCulture);
	}

	private void UpdateFormulaField()
	{
		string text = FieldCode;
		string text2 = string.Empty;
		string numberFormat = string.Empty;
		if (text.Contains("\\*"))
		{
			int startIndex = text.IndexOf("\\*");
			text2 = text.Substring(startIndex);
			text = text.Remove(startIndex);
			text2 = RemoveMergeFormat(text2, ref numberFormat);
		}
		text = RemoveMergeFormat(text, ref numberFormat);
		if (StartsWithExt(text, "="))
		{
			text = text.Substring(1).Trim();
		}
		bool isFieldCodeStartWithCurrencySymbol = false;
		text = RemoveCurrencySymbol(text, ref isFieldCodeStartWithCurrencySymbol);
		string empty = string.Empty;
		if (!double.TryParse(text, out var _))
		{
			try
			{
				empty = UpdateFormula(text, ref isFieldCodeStartWithCurrencySymbol);
			}
			catch (Exception ex)
			{
				empty = ex.Message;
			}
		}
		else
		{
			empty = text;
		}
		if (isFieldCodeStartWithCurrencySymbol && string.IsNullOrEmpty(text2) && string.IsNullOrEmpty(numberFormat))
		{
			empty = UpdateNumberFormat(empty, "0.00");
			empty = "$" + empty;
		}
		empty = (string.IsNullOrEmpty(text2) ? UpdateNumberFormat(empty, numberFormat) : ((FieldCode.IndexOf("\\*") <= FieldCode.IndexOf("\\#")) ? "Error! Picture switch must be first formatting switch." : UpdateTextFormat(empty, text2)));
		UpdateFieldResult(empty);
	}

	private string RemoveCurrencySymbol(string text, ref bool isFieldCodeStartWithCurrencySymbol)
	{
		char[] array = new char[1] { '$' };
		foreach (char c in array)
		{
			if (StartsWithExt(text, c.ToString()))
			{
				text = text.Substring(1).Trim();
				isFieldCodeStartWithCurrencySymbol = true;
				break;
			}
		}
		return text;
	}

	internal string UpdateNumberFormat(string text, string numberFormat)
	{
		string result = text;
		if (numberFormat.Contains("\\* MERGEFORMAT"))
		{
			numberFormat = numberFormat.Remove(numberFormat.IndexOf("\\* MERGEFORMAT")).Trim();
		}
		else if (numberFormat.Contains("\\* Mergeformat"))
		{
			numberFormat = numberFormat.Remove(numberFormat.IndexOf("\\* Mergeformat")).Trim();
		}
		if (double.TryParse(text, out var result2) && !string.IsNullOrEmpty(numberFormat))
		{
			string text2 = numberFormat;
			if (result2 == 0.0 && numberFormat.Contains(";"))
			{
				string text3 = numberFormat;
				int num = text2.IndexOf(";");
				if (num > 0)
				{
					text3 = text3.Substring(num + 1, text2.Length - (num + 1));
				}
				if (StartsWithExt(text3, '-'.ToString()))
				{
					return string.Empty;
				}
			}
			if (text2.Contains("#"))
			{
				text2 = text2.Replace("#", "0");
			}
			if (text2 != string.Empty)
			{
				if (text2.Contains("%"))
				{
					result2 /= 100.0;
				}
				text2 = GetNumberFormat(text2);
				if (!IsNeedToFormatFieldResult(text2))
				{
					text = ((!text2.StartsWith(".")) ? result2.ToString(text2, CultureInfo.CurrentCulture) : (result2 - Math.Floor(result2)).ToString(text2, CultureInfo.CurrentCulture));
				}
				else
				{
					text = GetFormattedString(text2, text);
					if (string.IsNullOrEmpty(text))
					{
						text = result2.ToString(text2, CultureInfo.CurrentCulture);
					}
				}
			}
			if (numberFormat.Contains("#") && !string.IsNullOrEmpty(text))
			{
				text2 = numberFormat;
				if (text2.Contains(";"))
				{
					CheckNumberFormatForNegativeValue(ref numberFormat, result2.ToString());
				}
				int num2 = numberFormat.Length;
				if (numberFormat.Contains(NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator))
				{
					num2 = numberFormat.IndexOf(NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator);
				}
				int num3 = numberFormat.IndexOf("#");
				if (num3 < 0)
				{
					num3 = 0;
				}
				bool flag = false;
				if (StartsWithExt(text, '-'.ToString()))
				{
					flag = true;
				}
				for (int i = num3; i < num2 && (!char.IsNumber(text[i]) || text[i] == '0'); i++)
				{
					if (numberFormat[i] != '0')
					{
						if (text[i] == '0')
						{
							text = text.Remove(i, 1);
							text = text.Insert(i, " ");
							continue;
						}
						text = text.Remove(i, 1);
						numberFormat = numberFormat.Remove(i, 1);
						i--;
						num2--;
					}
				}
				int num4 = numberFormat.IndexOf(NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator);
				int num5 = text.IndexOf(NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator);
				if (num4 != -1 && num5 != -1)
				{
					int num6 = numberFormat.Length - 1;
					int num7 = text.IndexOf(".");
					if (text.Contains(".") && text.Substring(num7 + 1).Contains("."))
					{
						return result;
					}
					if (numberFormat.Substring(num4).Contains("#"))
					{
						int num8 = text.Length - 1;
						while (num8 > num5 && (!char.IsNumber(text[num8]) || text[num8] == '0'))
						{
							if (numberFormat[num6] != '0' && text[num8] == '0')
							{
								text = text.Remove(num8, 1);
								text = text.Insert(num8, " ");
							}
							num6--;
							num8--;
						}
					}
				}
				if (flag && !StartsWithExt(text, '-'.ToString()))
				{
					text = '-' + text;
				}
			}
		}
		return text;
	}

	private bool IsNeedToFormatFieldResult(string numberFormat)
	{
		if (numberFormat.Contains("£"))
		{
			return IsNeedToFormatFieldResult(numberFormat, "£", checkContains: true);
		}
		return IsNeedToFormatFieldResult(numberFormat, "$", checkContains: false);
	}

	private bool IsNeedToFormatFieldResult(string numberFormat, string currency, bool checkContains)
	{
		string text = currency + CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator + "0" + CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator + "00";
		if (text == numberFormat)
		{
			return true;
		}
		if (checkContains && numberFormat.Contains(text))
		{
			return true;
		}
		text = text.Replace(currency, "");
		if (text == numberFormat)
		{
			return true;
		}
		return false;
	}

	private string GetFormattedString(string format, string text)
	{
		char c = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator[0];
		char c2 = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
		if (c == c2)
		{
			return text;
		}
		if (c == ' ')
		{
			return string.Empty;
		}
		if (format.Contains(";"))
		{
			CheckNumberFormatForNegativeValue(ref format, text);
		}
		string empty = string.Empty;
		string fractionalPart = string.Empty;
		int num = text.IndexOf(c2);
		if (num != -1)
		{
			empty = text.Substring(0, num);
			fractionalPart = text.Substring(num + 1, text.Length - empty.Length - 1);
		}
		else
		{
			empty = text;
		}
		StringBuilder stringBuilder = new StringBuilder();
		FormatFieldValue(format, c, c2, empty, fractionalPart, stringBuilder);
		return stringBuilder.ToString();
	}

	private void CheckNumberFormatForNegativeValue(ref string format, string text)
	{
		int num = format.IndexOf(";");
		format = (StartsWithExt(text, "-") ? format.Substring(num + 1, format.Length - (num + 1)) : format.Substring(0, num));
		if (format.IndexOf(";") > 0)
		{
			format = format.Substring(0, format.IndexOf(";"));
		}
	}

	private void FormatFieldValue(string format, char groupSeparator, char decimalSeparator, string integerPart, string fractionalPart, StringBuilder fieldResult)
	{
		string[] array = SplitNumberFormat(format, groupSeparator, decimalSeparator);
		bool flag = StartsWithExt(format, groupSeparator.ToString()) || StartsWithExt(format, "0");
		bool insertGroupSeparator = true;
		if (flag || HasValidSeperator(format, groupSeparator, decimalSeparator, ref insertGroupSeparator))
		{
			if (StartsWithExt(format, "0") && !format.Contains(groupSeparator.ToString()))
			{
				insertGroupSeparator = false;
			}
			UpdateIntegeralPart(integerPart, array[0], groupSeparator.ToString(), fieldResult, insertGroupSeparator);
			if (!flag && !StartsWithExt(fieldResult.ToString(), decimalSeparator.ToString()))
			{
				if (insertGroupSeparator)
				{
					if (StartsWithExt(fieldResult.ToString(), '-'.ToString()) && StartsWithExt(format, '-'.ToString()))
					{
						fieldResult.Remove(0, 1);
					}
					InsertBeforeText(fieldResult, format, groupSeparator);
				}
				else
				{
					InsertBeforeText(fieldResult, format, '0');
				}
			}
		}
		if (!string.IsNullOrEmpty(array[1]))
		{
			UpdateFractionalPart(fractionalPart, array[1], groupSeparator, decimalSeparator, fieldResult);
			if (StartsWithExt(fieldResult.ToString(), decimalSeparator.ToString()))
			{
				InsertBeforeText(fieldResult, format, decimalSeparator);
			}
		}
	}

	private void InsertBeforeText(StringBuilder fieldResult, string numberFormat, char separator)
	{
		string[] array = numberFormat.Split(separator);
		fieldResult.Insert(0, array[0]);
	}

	private void UpdateFractionalPart(string fractionalPart, string numberFormats, char groupSeparator, char decimalSeparator, StringBuilder fieldResult)
	{
		if (string.IsNullOrEmpty(fractionalPart))
		{
			numberFormats = numberFormats.Replace(groupSeparator.ToString(), "");
			fieldResult.Append(decimalSeparator + numberFormats);
			return;
		}
		fieldResult.Append(decimalSeparator);
		for (int i = 0; i < numberFormats.Length; i++)
		{
			char value = ((i < fractionalPart.Length) ? fractionalPart[i] : '0');
			if (numberFormats[i] == decimalSeparator && i != 0)
			{
				fieldResult.Append(decimalSeparator);
			}
			else if (numberFormats[i] == '%')
			{
				fieldResult.Append('%');
			}
			else if (numberFormats[i] != groupSeparator)
			{
				fieldResult.Append(value);
			}
		}
	}

	private void UpdateIntegeralPart(string integerPart, string numberFormats, string groupSeparator, StringBuilder fieldResult, bool insertSeparator)
	{
		if (integerPart.Contains(groupSeparator))
		{
			integerPart = UpdateFieldValueBySeparator(integerPart, groupSeparator[0]);
		}
		int num = ((!string.IsNullOrEmpty(numberFormats) && numberFormats.Length > integerPart.Length) ? numberFormats.Length : integerPart.Length);
		int num2 = integerPart.Length;
		int num3 = 0;
		bool flag = false;
		for (int num4 = num - 1; num4 >= 0; num4--)
		{
			string text = ((num2 > 0) ? integerPart[num2 - 1].ToString() : "0");
			fieldResult.Insert(0, text);
			if (insertSeparator && (fieldResult.Length - num3) % 3 == 0 && num4 != 0 && num4 != num - 1)
			{
				fieldResult.Insert(0, groupSeparator);
				num3++;
				flag = true;
			}
			else if (flag && char.IsDigit(text, 0))
			{
				flag = false;
			}
			num2--;
		}
		if (flag)
		{
			fieldResult.Remove(fieldResult.ToString().IndexOf(groupSeparator), 1);
		}
	}

	private string UpdateFieldValueBySeparator(string fieldValue, char separator)
	{
		string[] array = fieldValue.Split(separator);
		if (array.Length - 1 == 1)
		{
			int num = fieldValue.IndexOf(separator);
			if (num + 4 <= fieldValue.Length || num == 0)
			{
				return fieldValue.Replace(separator.ToString(), "");
			}
			return (ConvertToInteger(array[0]) + ConvertToInteger(array[1])).ToString();
		}
		return AddFieldValues(SplitFieldValue(fieldValue, separator));
	}

	private string AddFieldValues(List<int> fieldValues)
	{
		int num = 0;
		foreach (int fieldValue in fieldValues)
		{
			num += fieldValue;
		}
		return num.ToString();
	}

	private List<int> SplitFieldValue(string fieldValue, char separator)
	{
		List<int> list = new List<int>();
		StringBuilder stringBuilder = new StringBuilder();
		bool foundMultipleSeparator = false;
		int lastGroupLegth = 0;
		for (int num = fieldValue.Length - 1; num >= 0; num--)
		{
			if (fieldValue[num] == separator)
			{
				SplitBySeparator(ref foundMultipleSeparator, num, fieldValue, separator, stringBuilder, list, ref lastGroupLegth);
			}
			else
			{
				stringBuilder.Insert(0, fieldValue[num].ToString());
				if (foundMultipleSeparator)
				{
					foundMultipleSeparator = false;
				}
			}
		}
		list.Add(ConvertToInteger(stringBuilder.ToString()));
		return list;
	}

	private void SplitBySeparator(ref bool foundMultipleSeparator, int charIndex, string fieldValue, char separator, StringBuilder integerFieldValue, List<int> integerFieldValues, ref int lastGroupLegth)
	{
		if (!foundMultipleSeparator)
		{
			if (charIndex - 1 < 0 && fieldValue[charIndex - 1] == separator)
			{
				integerFieldValues.Add(ConvertToInteger(integerFieldValue.ToString()));
				foundMultipleSeparator = true;
				Clear(integerFieldValue);
			}
			else if (integerFieldValue.Length - lastGroupLegth < 3)
			{
				integerFieldValues.Add(ConvertToInteger(integerFieldValue.ToString()));
				lastGroupLegth = 0;
				Clear(integerFieldValue);
			}
			else
			{
				lastGroupLegth = integerFieldValue.Length;
			}
		}
	}

	private void Clear(StringBuilder stringBuilder)
	{
		stringBuilder.Length = 0;
		stringBuilder.Capacity = 0;
	}

	private int ConvertToInteger(string value)
	{
		int result = 0;
		int.TryParse(value, out result);
		return result;
	}

	private bool IsBeginWithDoubleQuote(int index)
	{
		while (index >= 0)
		{
			if (FieldCode[index] == ControlChar.DoubleQuote)
			{
				return true;
			}
			if (FieldCode[index] == '#')
			{
				return false;
			}
			index--;
		}
		return false;
	}

	private bool HasValidSeperator(string numberFormat, char seperator, char decimalSeparator, ref bool insertGroupSeparator)
	{
		int num = FieldCode.IndexOf(numberFormat);
		bool flag = num > 0 && IsBeginWithDoubleQuote(num--);
		for (int i = 0; i < numberFormat.Length; i++)
		{
			if (numberFormat[i] == seperator)
			{
				return true;
			}
			if (numberFormat[i] == '0')
			{
				if (!numberFormat.Contains(seperator.ToString()))
				{
					insertGroupSeparator = false;
				}
				return true;
			}
			if (numberFormat[i] == decimalSeparator)
			{
				return false;
			}
			if (numberFormat[i] == ' ' && !flag)
			{
				return false;
			}
		}
		return false;
	}

	private string[] SplitNumberFormat(string numberFormat, char groupSeparator, char decimalSeparator)
	{
		string[] array = new string[2];
		if (numberFormat.Contains(decimalSeparator.ToString()))
		{
			array[0] = numberFormat[..numberFormat.IndexOf(decimalSeparator)];
			array[1] = numberFormat.Substring(array[0].Length + 1, numberFormat.Length - array[0].Length - 1);
			if (!StartsWithExt(array[0], 0.ToString()))
			{
				if (array[0].Contains(0.ToString()))
				{
					int num = array[0].IndexOf('0');
					array[0] = array[0].Substring(num, array[0].Length - num);
				}
				else
				{
					array[0] = string.Empty;
				}
			}
		}
		else if (numberFormat.Contains("0"))
		{
			int num2 = numberFormat.IndexOf('0');
			array[0] = numberFormat.Substring(num2, numberFormat.Length - num2);
			array[0] = array[0].Replace(groupSeparator.ToString(), "");
			array[1] = string.Empty;
		}
		return array;
	}

	protected string RemoveMergeFormat(string text)
	{
		if (text.Contains("\\* MERGEFORMAT"))
		{
			text = text.Remove(text.IndexOf("\\* MERGEFORMAT")).Trim();
		}
		else if (text.Contains("\\* Mergeformat"))
		{
			text = text.Remove(text.IndexOf("\\* Mergeformat")).Trim();
		}
		return text.Trim();
	}

	internal string RemoveMergeFormat(string fieldCode, ref string numberFormat)
	{
		if (fieldCode.Contains("\\#"))
		{
			numberFormat = fieldCode.Substring(fieldCode.IndexOf("\\#"));
			if (!numberFormat.Contains("\""))
			{
				int num = numberFormat.IndexOf("\\#");
				if (numberFormat.Length > num + 2)
				{
					numberFormat = numberFormat.Insert(num + 2, "\"");
					numberFormat = numberFormat.Insert(numberFormat.Length, "\"");
				}
			}
			if (numberFormat.IndexOf("\"") != -1)
			{
				numberFormat = numberFormat.Substring(numberFormat.IndexOf("\"") + 1);
			}
			if (numberFormat.LastIndexOf("\"") != -1)
			{
				numberFormat = numberFormat.Remove(numberFormat.LastIndexOf("\""));
			}
			numberFormat = numberFormat.Trim();
			fieldCode = fieldCode.Remove(fieldCode.IndexOf("\\#")).Trim();
		}
		if (fieldCode.Contains("\\* MERGEFORMAT"))
		{
			fieldCode = fieldCode.Remove(fieldCode.IndexOf("\\* MERGEFORMAT")).Trim();
		}
		else if (fieldCode.Contains("\\* Mergeformat"))
		{
			fieldCode = fieldCode.Remove(fieldCode.IndexOf("\\* Mergeformat")).Trim();
		}
		return fieldCode.Trim();
	}

	private string UpdateFormula(string fieldCode, ref bool isFieldCodeStartWithCurrencySymbol)
	{
		if (IsFunction(fieldCode.ToLower()))
		{
			return UpdateFunction(fieldCode, ref isFieldCodeStartWithCurrencySymbol);
		}
		double result;
		if (IsExpression(fieldCode))
		{
			result = UpdateExpression(fieldCode, ref isFieldCodeStartWithCurrencySymbol);
		}
		else
		{
			Bookmark bookmark = base.Document.Bookmarks.FindByName(fieldCode);
			if (bookmark == null)
			{
				if (!fieldCode.Contains(CultureInfo.CurrentCulture.TextInfo.ListSeparator))
				{
					throw new Exception("!Syntax Error, " + fieldCode.ToUpper());
				}
				throw new Exception("!Undefined Bookmark, " + fieldCode.ToUpper());
			}
			string text = string.Empty;
			WParagraph ownerParagraph = bookmark.BookmarkStart.OwnerParagraph;
			if (ownerParagraph == bookmark.BookmarkEnd.OwnerParagraph)
			{
				for (int i = ((bookmark.BookmarkStart.PreviousSibling is WTextFormField) ? (bookmark.BookmarkStart.Index - 1) : (bookmark.BookmarkStart.Index + 1)); i < bookmark.BookmarkEnd.Index; i++)
				{
					Entity entity = ownerParagraph.ChildEntities[i];
					if (entity is WField)
					{
						text += (entity as WField).Text;
						i = (entity as WField).FieldEnd.Index;
					}
					else if (entity is WTextRange)
					{
						text += (entity as WTextRange).Text;
					}
				}
			}
			char[] trimChars = new char[24]
			{
				'@', '#', '%', '&', ' ', '`', '~', '{', '}', '[',
				']', ';', ':', '"', '|', '<', '>', ',', '?', '=',
				'$', '\'', '\\', '£'
			};
			double.TryParse(text.Trim(trimChars), out result);
		}
		return result.ToString(CultureInfo.CurrentCulture);
	}

	private string UpdateFunction(string fieldCode, ref bool isFieldCodeStartWithCurrencySymbol)
	{
		string text = fieldCode.ToLower();
		int num = fieldCode.IndexOf('(');
		if (fieldCode.Contains('('.ToString()))
		{
			text = fieldCode.Remove(num).ToLower().Trim();
		}
		List<double> list = new List<double>();
		int num2 = fieldCode.LastIndexOf(')');
		if (num + 1 < num2)
		{
			list = SplitOperands(fieldCode.Substring(num + 1, num2 - num - 1), ref isFieldCodeStartWithCurrencySymbol);
		}
		else if (!(text == "true") && !(text == "false"))
		{
			throw new Exception("!Syntax Error, )");
		}
		switch (text)
		{
		case "product":
			return Product(list).ToString(CultureInfo.CurrentCulture);
		case "sum":
			return Sum(list).ToString(CultureInfo.CurrentCulture);
		case "average":
			if (list.Count <= 1)
			{
				return $"!Syntax Error, {list[0]}";
			}
			return Average(list).ToString(CultureInfo.CurrentCulture);
		case "mod":
			return (list[0] % list[1]).ToString(CultureInfo.CurrentCulture);
		case "abs":
			return Math.Abs(list[0]).ToString(CultureInfo.CurrentCulture);
		case "int":
			return Math.Floor(list[0]).ToString(CultureInfo.CurrentCulture);
		case "round":
			return RoundOf(list[0], (int)list[1]).ToString(CultureInfo.CurrentCulture);
		case "sign":
			return Math.Sign(list[0]).ToString();
		case "count":
			return list.Count.ToString();
		case "defined":
			return Defined(list[0].ToString(CultureInfo.InvariantCulture)).ToString(CultureInfo.CurrentCulture);
		case "or":
			if (((int)list[0] | (int)list[1]) != 0)
			{
				return "1";
			}
			return "0";
		case "and":
			if (((int)list[0] & (int)list[1]) != 0)
			{
				return "1";
			}
			return "0";
		case "not":
			if ((int)list[0] != 0)
			{
				return "0";
			}
			return "1";
		case "max":
			list.Sort();
			return list[list.Count - 1].ToString(CultureInfo.CurrentCulture);
		case "min":
			list.Sort();
			return list[0].ToString(CultureInfo.CurrentCulture);
		case "true":
			return "1";
		case "false":
			return "0";
		case "if":
			if (list[0] != 1.0)
			{
				return list[2].ToString(CultureInfo.CurrentCulture);
			}
			return list[1].ToString(CultureInfo.CurrentCulture);
		default:
			throw new NotSupportedException("The operation" + text + "is not supported.");
		}
	}

	private double Product(List<double> operands)
	{
		double num = 1.0;
		for (int i = 0; i < operands.Count; i++)
		{
			num *= operands[i];
		}
		return num;
	}

	private double Sum(List<double> operands)
	{
		double num = 0.0;
		for (int i = 0; i < operands.Count; i++)
		{
			num += operands[i];
		}
		return num;
	}

	private double Average(List<double> operands)
	{
		return Sum(operands) / (double)operands.Count;
	}

	private double RoundOf(double operand, int decimalPoint)
	{
		if (decimalPoint >= 0)
		{
			return Math.Round(operand, decimalPoint = ((decimalPoint > 12) ? 12 : decimalPoint));
		}
		decimalPoint = Math.Abs(decimalPoint);
		double num = Math.Pow(10.0, decimalPoint);
		return (double)(long)(operand / num) * num;
	}

	private double Defined(string operand)
	{
		if (double.TryParse(operand, out var _))
		{
			return 1.0;
		}
		return 0.0;
	}

	private bool IsFunction(string text)
	{
		bool result = false;
		foreach (string function in Functions)
		{
			if (StartsWithExt(text, function))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private bool IsExpression(string text)
	{
		List<string> list = new List<string>(new string[12]
		{
			"+", "-", "*", "/", "%", "^", "=", "<", "<=", ">",
			">=", "<>"
		});
		bool result = false;
		foreach (string item in list)
		{
			if (text.Contains(item))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private double UpdateExpression(string text, ref bool isFieldCodeStartWithCurrencySymbol)
	{
		List<string> list = new List<string>(new string[12]
		{
			"=", "<", "<=", ">", ">=", "<>", "^", "%", "/", "*",
			"-", "+"
		});
		List<string> expression = SplitExpression(text, list, ref isFieldCodeStartWithCurrencySymbol);
		foreach (string item in list)
		{
			EvaluateExpression(ref expression, item, list.IndexOf(item) > 6);
		}
		return double.Parse(expression[0], CultureInfo.CurrentCulture);
	}

	private void EvaluateExpression(ref List<string> expression, string operation, bool isAritmeticOperation)
	{
		while (expression.Contains(operation))
		{
			int num = ((!isAritmeticOperation) ? expression.LastIndexOf(operation) : expression.IndexOf(operation));
			double num2 = 0.0;
			double num3 = 0.0;
			double num4 = 0.0;
			num3 = double.Parse(expression[num - 1]);
			num4 = double.Parse(expression[num + 1]);
			switch (operation)
			{
			case "+":
				num2 = num3 + num4;
				break;
			case "-":
				num2 = num3 - num4;
				break;
			case "*":
				num2 = num3 * num4;
				break;
			case "/":
				num2 = num3 / num4;
				break;
			case "%":
				num2 = num3 % num4;
				break;
			case "^":
				num2 = Math.Pow(num3, num4);
				break;
			case "=":
				num2 = ((num3 == num4) ? 1 : 0);
				break;
			case "<":
				num2 = ((num3 < num4) ? 1 : 0);
				break;
			case "<=":
				num2 = ((num3 <= num4) ? 1 : 0);
				break;
			case ">":
				num2 = ((num3 > num4) ? 1 : 0);
				break;
			case ">=":
				num2 = ((num3 >= num4) ? 1 : 0);
				break;
			case "<>":
				num2 = ((num3 != num4) ? 1 : 0);
				break;
			}
			expression.RemoveAt(num + 1);
			expression.RemoveAt(num);
			expression.RemoveAt(num - 1);
			expression.Insert(num - 1, num2.ToString());
		}
	}

	private List<string> SplitExpression(string text, List<string> operators, ref bool isFieldCodeStartWithCurrencySymbol)
	{
		string text2 = text;
		List<string> list = new List<string>();
		int num = 0;
		string text3 = string.Empty;
		string empty = string.Empty;
		int num2 = 0;
		bool flag = false;
		while (num2 < text.Length && text.Substring(num2).Contains("("))
		{
			for (int i = num2; i < text.Length; i++)
			{
				if (char.IsLetter(text[i]))
				{
					if (IsFunction(text.Substring(i).ToLower()) && i > 0 && text[i - 1] != '(')
					{
						text = text.Insert(i, "(");
						flag = true;
						num2 = i + 1;
					}
					break;
				}
			}
			if (flag)
			{
				int num3 = -1;
				for (int j = num2; j < text.Length; j++)
				{
					if (text[j] == ')')
					{
						if (num3 == 0)
						{
							flag = false;
							text = text.Insert(j + 1, ")");
							num2 = j + 1;
							break;
						}
						num3--;
					}
					else if (text[j] == '(')
					{
						num3++;
					}
				}
			}
			else
			{
				num2++;
			}
		}
		for (int k = 0; k < text.Length; k++)
		{
			empty = string.Empty;
			if (operators.Contains(text[k].ToString()))
			{
				empty = text[k].ToString();
				if (k != text.Length - 1 && (text[k + 1].ToString() == "=" || text[k + 1].ToString() == ">"))
				{
					empty += text[k++];
				}
			}
			if (k == text.Length - 1 && text[k] != ')' && !operators.Contains(text[k].ToString()))
			{
				text3 += text[k];
			}
			if (text[k] == '(')
			{
				num++;
			}
			else if (text[k] == ')')
			{
				num--;
				text3 = text3.Trim();
				text3 = RemoveCurrencySymbol(text3, ref isFieldCodeStartWithCurrencySymbol);
				if (double.TryParse(text3, out var result) && result >= 0.0)
				{
					text3 = "-" + text3;
				}
			}
			if ((operators.Contains(empty) && num == 0) || k == text.Length - 1)
			{
				text3 = text3.Trim();
				text3 = RemoveCurrencySymbol(text3, ref isFieldCodeStartWithCurrencySymbol);
				if (double.TryParse(text3, out var result2))
				{
					list.Add(text3);
					text3 = string.Empty;
				}
				else if (text2 != text3)
				{
					text3 = UpdateFormula(text3, ref isFieldCodeStartWithCurrencySymbol);
					if (!double.TryParse(text3, out result2))
					{
						throw new Exception("!Syntax Error, " + text3);
					}
					list.Add(text3);
					text3 = string.Empty;
				}
				if (k != text.Length - 1 && operators.Contains(empty))
				{
					list.Add(empty.Trim());
				}
			}
			else if ((text[k] == '(' && num > 1) || (text[k] == ')' && num > 0) || (text[k] != '(' && text[k] != ')'))
			{
				text3 += text[k];
			}
		}
		return list;
	}

	private List<double> SplitOperands(string text, ref bool isFieldCodeStartWithCurrencySymbol)
	{
		string text2 = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
		if (string.IsNullOrEmpty(text2))
		{
			text2 = ','.ToString();
		}
		List<double> list = new List<double>();
		int num = 0;
		string text3 = string.Empty;
		for (int i = 0; i < text.Length; i++)
		{
			if (i == text.Length - 1)
			{
				text3 += text[i];
			}
			if (text[i] == '(')
			{
				num++;
			}
			else if (text[i] == ')')
			{
				num--;
			}
			if ((text2.Contains(text[i].ToString()) && num == 0) || i == text.Length - 1)
			{
				text3 = text3.Trim();
				text3 = RemoveCurrencySymbol(text3, ref isFieldCodeStartWithCurrencySymbol);
				if (double.TryParse(text3, out var result))
				{
					list.Add(result);
					text3 = string.Empty;
					continue;
				}
				text3 = UpdateFormula(text3, ref isFieldCodeStartWithCurrencySymbol);
				if (!double.TryParse(text3, out result))
				{
					throw new Exception("!Syntax Error, " + text3);
				}
				list.Add(result);
				text3 = string.Empty;
			}
			else
			{
				text3 += text[i];
			}
		}
		return list;
	}

	public void Unlink()
	{
		bool isFieldSeperatorReached = false;
		bool isFieldReached = false;
		entities = new Stack<Entity>();
		nestedFields = new List<WField>();
		Range.Items.Clear();
		UpdateFieldRange();
		if (Range.Items.Count > 0)
		{
			RemoveFieldItems(ref isFieldReached, ref isFieldSeperatorReached);
		}
		Range.Items.Clear();
		entities.Clear();
		nestedFields.Clear();
	}

	private void RemoveFieldItems(ref bool isFieldReached, ref bool isFieldSeperatorReached)
	{
		if (FieldType == FieldType.FieldIndexEntry)
		{
			return;
		}
		if (FieldType == FieldType.FieldSet)
		{
			if (base.OwnerParagraph != null)
			{
				RemoveField(this);
				return;
			}
		}
		else if (FieldType != FieldType.FieldSet)
		{
			entities.Push(this);
			isFieldReached = true;
			isFieldSeperatorReached = false;
		}
		for (int i = 0; i < Range.Items.Count; i++)
		{
			Entity entity = Range.Items[i] as Entity;
			if (entity is ParagraphItem)
			{
				ParagraphItems(entity as ParagraphItem, ref isFieldReached, ref isFieldSeperatorReached);
			}
			else if (entity is TextBodyItem)
			{
				TextBodyItems(entity as TextBodyItem, ref isFieldReached, ref isFieldSeperatorReached);
			}
		}
		if (base.OwnerParagraph != null)
		{
			RemoveField(this);
		}
		RemoveNestedFields();
	}

	private void ParagraphItems(ParagraphItem paraItem, ref bool isFieldReached, ref bool isFieldSeperatorReached)
	{
		if (paraItem is WField && (paraItem as WField).FieldType == FieldType.FieldSet)
		{
			if ((paraItem as WField).FieldSeparator != null)
			{
				entities.Push(paraItem);
				isFieldReached = true;
			}
			paraItem.RemoveSelf();
		}
		else
		{
			if (paraItem is WField && (paraItem as WField).FieldType != FieldType.FieldSet)
			{
				nestedFields.Add(paraItem as WField);
				AddUnlinkNestedFieldStack(ref isFieldReached, ref isFieldSeperatorReached);
				entities.Push(paraItem);
				isFieldReached = true;
				isFieldSeperatorReached = false;
				return;
			}
			if ((paraItem is WFieldMark && (paraItem as WFieldMark).Type == FieldMarkType.FieldSeparator) & isFieldReached)
			{
				if (entities.Count > 0)
				{
					entities.Pop();
				}
				if (entities.Count == 0)
				{
					isFieldSeperatorReached = true;
				}
				paraItem.RemoveSelf();
			}
			else if ((paraItem is WFieldMark && (paraItem as WFieldMark).Type == FieldMarkType.FieldEnd) & isFieldReached)
			{
				if (entities.Count == 0)
				{
					isFieldReached = false;
					isFieldSeperatorReached = false;
				}
				paraItem.RemoveSelf();
				if (m_unlinkNestedFieldStack.Count > 0)
				{
					ResetUnlinkNestedFieldStack(ref isFieldReached, ref isFieldSeperatorReached);
				}
			}
			else if (paraItem is WTextBox)
			{
				WTextBox wTextBox = paraItem as WTextBox;
				RemoveFieldCodesInTextBody(wTextBox.TextBoxBody, ref isFieldReached, ref isFieldSeperatorReached);
			}
			else if (paraItem is Shape)
			{
				Shape shape = paraItem as Shape;
				RemoveFieldCodesInTextBody(shape.TextBody, ref isFieldReached, ref isFieldSeperatorReached);
			}
			else if (paraItem is InlineContentControl)
			{
				InlineContentControl inlineContentControl = paraItem as InlineContentControl;
				RemoveFieldCodesInParagraph(inlineContentControl.ParagraphItems, ref isFieldReached, ref isFieldSeperatorReached);
			}
		}
		if (isFieldReached && entities.Count != 0)
		{
			paraItem.RemoveSelf();
		}
	}

	private void TextBodyItems(TextBodyItem textbodyItem, ref bool isFieldReached, ref bool isFieldSeperatorReached)
	{
		switch (textbodyItem.EntityType)
		{
		case EntityType.Paragraph:
		{
			WParagraph wParagraph = textbodyItem as WParagraph;
			RemoveFieldCodesInParagraph(wParagraph.Items, ref isFieldReached, ref isFieldSeperatorReached);
			if ((wParagraph.Items.Count == 0 && !isFieldSeperatorReached) & isFieldReached)
			{
				wParagraph.RemoveSelf();
			}
			break;
		}
		case EntityType.Table:
			if (isFieldReached && entities.Count != 0)
			{
				textbodyItem.RemoveSelf();
			}
			else
			{
				RemoveFieldCodesInTable(textbodyItem as WTable, ref isFieldReached, ref isFieldSeperatorReached);
			}
			break;
		case EntityType.BlockContentControl:
		{
			BlockContentControl blockContentControl = textbodyItem as BlockContentControl;
			if (isFieldReached && entities.Count != 0)
			{
				blockContentControl.RemoveSelf();
			}
			else
			{
				RemoveFieldCodesInTextBody(blockContentControl.TextBody, ref isFieldReached, ref isFieldSeperatorReached);
			}
			break;
		}
		}
	}

	private void RemoveFieldCodesInTextBody(WTextBody textBody, ref bool isFieldReached, ref bool isFieldSeperatorReached)
	{
		for (int i = 0; i < textBody.ChildEntities.Count; i++)
		{
			if (!isFieldReached && !isFieldSeperatorReached)
			{
				break;
			}
			IEntity entity = textBody.ChildEntities[i];
			switch (entity.EntityType)
			{
			case EntityType.Paragraph:
			{
				WParagraph wParagraph = entity as WParagraph;
				RemoveFieldCodesInParagraph(wParagraph.Items, ref isFieldReached, ref isFieldSeperatorReached);
				if ((wParagraph.Items.Count == 0 && !isFieldSeperatorReached) & isFieldReached)
				{
					textBody.ChildEntities.Remove(wParagraph);
					i--;
				}
				break;
			}
			case EntityType.Table:
				if (isFieldReached && entities.Count != 0)
				{
					textBody.ChildEntities.RemoveAt(i);
					i--;
				}
				else
				{
					RemoveFieldCodesInTable(entity as WTable, ref isFieldReached, ref isFieldSeperatorReached);
				}
				break;
			case EntityType.BlockContentControl:
			{
				BlockContentControl blockContentControl = entity as BlockContentControl;
				if (isFieldReached && entities.Count != 0)
				{
					textBody.ChildEntities.RemoveAt(i);
					i--;
				}
				else
				{
					RemoveFieldCodesInTextBody(blockContentControl.TextBody, ref isFieldReached, ref isFieldSeperatorReached);
				}
				break;
			}
			}
		}
	}

	private void RemoveFieldCodesInParagraph(ParagraphItemCollection paraItems, ref bool isFieldReached, ref bool isFieldSeperatorReached)
	{
		for (int i = 0; i < paraItems.Count; i++)
		{
			if (!isFieldReached && !isFieldSeperatorReached)
			{
				break;
			}
			if (paraItems[i] is WField && (paraItems[i] as WField).FieldType == FieldType.FieldSet)
			{
				paraItems.RemoveAt(i);
				i--;
				continue;
			}
			if (paraItems[i] is WField && (paraItems[i] as WField).FieldType != FieldType.FieldSet)
			{
				nestedFields.Add(paraItems[i] as WField);
				AddUnlinkNestedFieldStack(ref isFieldReached, ref isFieldSeperatorReached);
				entities.Push(paraItems[i]);
				isFieldReached = true;
				isFieldSeperatorReached = false;
				continue;
			}
			if ((paraItems[i] is WFieldMark && (paraItems[i] as WFieldMark).Type == FieldMarkType.FieldSeparator) & isFieldReached)
			{
				if (entities.Count > 0)
				{
					entities.Pop();
				}
				if (entities.Count == 0)
				{
					isFieldSeperatorReached = true;
				}
				paraItems.RemoveAt(i);
				i--;
				continue;
			}
			if ((paraItems[i] is WFieldMark && (paraItems[i] as WFieldMark).Type == FieldMarkType.FieldEnd) & isFieldReached)
			{
				if (entities.Count == 0)
				{
					isFieldReached = false;
					isFieldSeperatorReached = false;
				}
				paraItems.RemoveAt(i);
				i--;
				if (m_unlinkNestedFieldStack.Count > 0)
				{
					ResetUnlinkNestedFieldStack(ref isFieldReached, ref isFieldSeperatorReached);
				}
				continue;
			}
			if (paraItems[i] is WTextBox)
			{
				WTextBox wTextBox = paraItems[i] as WTextBox;
				RemoveFieldCodesInTextBody(wTextBox.TextBoxBody, ref isFieldReached, ref isFieldSeperatorReached);
			}
			else if (paraItems[i] is Shape)
			{
				Shape shape = paraItems[i] as Shape;
				RemoveFieldCodesInTextBody(shape.TextBody, ref isFieldReached, ref isFieldSeperatorReached);
			}
			else if (paraItems[i] is InlineContentControl)
			{
				InlineContentControl inlineContentControl = paraItems[i] as InlineContentControl;
				RemoveFieldCodesInParagraph(inlineContentControl.ParagraphItems, ref isFieldReached, ref isFieldSeperatorReached);
			}
			if (isFieldReached && entities.Count != 0)
			{
				paraItems.RemoveAt(i);
				i--;
			}
		}
	}

	private void RemoveFieldCodesInTable(WTable table, ref bool isFieldReached, ref bool isFieldSeperatorReached)
	{
		foreach (WTableRow row in table.Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				RemoveFieldCodesInTextBody(cell, ref isFieldReached, ref isFieldSeperatorReached);
			}
		}
	}

	private void RemoveNestedFields()
	{
		foreach (WField nestedField in nestedFields)
		{
			if (nestedField.OwnerParagraph != null)
			{
				RemoveField(nestedField);
			}
		}
	}

	private void RemoveField(WField field)
	{
		WParagraph ownerParagraph = field.OwnerParagraph;
		field.RemoveSelf();
		if (ownerParagraph.Items.Count == 0)
		{
			ownerParagraph.OwnerTextBody.ChildEntities.Remove(ownerParagraph);
		}
	}

	private void AddUnlinkNestedFieldStack(ref bool isFieldReached, ref bool isFieldSeperatorReached)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("isFieldReached", isFieldReached);
		dictionary.Add("isFieldSeperatorReached", isFieldSeperatorReached);
		dictionary.Add("entities", (entities == null) ? null : new Stack<Entity>(entities.ToArray()));
		m_unlinkNestedFieldStack.Push(dictionary);
	}

	private void ResetUnlinkNestedFieldStack(ref bool isFieldReached, ref bool isFieldSeperatorReached)
	{
		Dictionary<string, object> dictionary = m_unlinkNestedFieldStack.Pop();
		isFieldReached = (bool)dictionary["isFieldReached"];
		isFieldSeperatorReached = (bool)dictionary["isFieldSeperatorReached"];
		entities = new Stack<Entity>((dictionary["entities"] as Stack<Entity>).ToArray());
	}

	internal void UpdateFieldRange()
	{
		try
		{
			if (GetOwnerParagraphValue() == null || FieldEnd == null)
			{
				m_range.Items.Clear();
				return;
			}
			int indexInOwnerCollection = GetIndexInOwnerCollection();
			if (base.OwnerParagraph == FieldEnd.OwnerParagraph)
			{
				if (base.Owner is InlineContentControl)
				{
					InlineContentControl inlineContentControl = base.Owner as InlineContentControl;
					if (base.Owner == FieldEnd.Owner)
					{
						for (int i = indexInOwnerCollection + 1; i < inlineContentControl.ParagraphItems.Count; i++)
						{
							m_range.Items.Add(inlineContentControl.ParagraphItems[i]);
							if (inlineContentControl.ParagraphItems[i] == FieldEnd)
							{
								break;
							}
						}
					}
					else
					{
						UpdateParagraphItems(inlineContentControl.ParagraphItems, indexInOwnerCollection + 1);
						for (int j = base.Owner.GetIndexInOwnerCollection() + 1; j < base.OwnerParagraph.Items.Count; j++)
						{
							m_range.Items.Add(base.OwnerParagraph.Items[j]);
							if (base.OwnerParagraph.Items[j] == FieldEnd.Owner || base.OwnerParagraph.Items[j] == FieldEnd)
							{
								break;
							}
						}
					}
				}
				else
				{
					for (int k = indexInOwnerCollection + 1; k < base.OwnerParagraph.Items.Count; k++)
					{
						if (FieldType == FieldType.FieldSet && base.OwnerParagraph.Items[k] is WField && m_range.Items[m_range.Items.Count - 1] is WTextRange && !(m_range.Items[m_range.Items.Count - 1] as WTextRange).Text.EndsWith(" "))
						{
							(m_range.Items[m_range.Items.Count - 1] as WTextRange).Text += " ";
						}
						m_range.Items.Add(base.OwnerParagraph.Items[k]);
						if (base.OwnerParagraph.Items[k] == FieldEnd)
						{
							break;
						}
					}
				}
			}
			else
			{
				int num = indexInOwnerCollection;
				if (base.Owner is InlineContentControl)
				{
					UpdateParagraphItems((base.Owner as InlineContentControl).ParagraphItems, indexInOwnerCollection + 1);
					num = base.Owner.GetIndexInOwnerCollection();
				}
				for (int l = num + 1; l < base.OwnerParagraph.Items.Count; l++)
				{
					m_range.Items.Add(base.OwnerParagraph.Items[l]);
				}
				for (int m = base.OwnerParagraph.GetIndexInOwnerCollection() + 1; m < base.OwnerParagraph.OwnerTextBody.Items.Count; m++)
				{
					m_range.Items.Add(base.OwnerParagraph.OwnerTextBody.Items[m]);
					if (base.OwnerParagraph.OwnerTextBody.Items[m] == FieldEnd.OwnerParagraph)
					{
						break;
					}
				}
			}
			IsFieldRangeUpdated = true;
		}
		catch
		{
			m_range.Items.Clear();
		}
	}

	private void UpdateParagraphItems(ParagraphItemCollection items, int startIndex)
	{
		for (int i = startIndex; i < items.Count; i++)
		{
			m_range.Items.Add(items[i]);
		}
	}

	private string UpdateNestedFieldCode(bool isUpdateNestedFields, WMergeField mergeField)
	{
		string text = string.Empty;
		IsFieldSeparator = false;
		IsSkip = false;
		m_nestedFields.Clear();
		for (int i = 0; i < Range.Items.Count; i++)
		{
			Entity entity = Range.Items[i] as Entity;
			if (entity == mergeField)
			{
				break;
			}
			if (this != null && FieldType == FieldType.FieldSet && entity is WField && !(entity as WField).IsUpdated && !(entity as WField).IsSkip && (entity as WField).FieldEnd != null)
			{
				text += " ";
			}
			if (this != null && FieldType == FieldType.FieldIf && entity is WField && !(entity as WField).IsSkip)
			{
				text = text + "\u0013" + UpdateTextForParagraphItem(entity, isUpdateNestedFields) + "\u0015";
			}
			text = ((!(entity is ParagraphItem)) ? (text + UpdateTextForTextBodyItem(entity, isUpdateNestedFields, mergeField)) : (text + UpdateTextForParagraphItem(entity, isUpdateNestedFields)));
			if (IsFieldSeparator)
			{
				break;
			}
		}
		IsFieldSeparator = false;
		IsSkip = false;
		m_nestedFields.Clear();
		return text;
	}

	protected string UpdateTextForTextBodyItem(Entity entity, bool isUpdateNestedFields)
	{
		return UpdateTextForTextBodyItem(entity, isUpdateNestedFields, null);
	}

	private string UpdateTextForTextBodyItem(Entity entity, bool isUpdateNestedFields, WMergeField mergeField)
	{
		string text = string.Empty;
		if (entity is WParagraph)
		{
			if (!IsSkip)
			{
				text += "\r";
			}
			for (int i = 0; i < (entity as WParagraph).Items.Count; i++)
			{
				ParagraphItem paragraphItem = (entity as WParagraph).Items[i];
				if (paragraphItem == mergeField)
				{
					IsFieldSeparator = true;
					break;
				}
				text += UpdateTextForParagraphItem(paragraphItem, isUpdateNestedFields);
				if (IsFieldSeparator)
				{
					return text;
				}
			}
		}
		else if (entity is WTable && !IsSkip)
		{
			text = text + "\u0013" + UpdateTextForTable(entity, isUpdateNestedFields, mergeField) + "\u0015";
		}
		return text;
	}

	private string UpdateTextForTable(Entity entity, bool isUpdateNestedFields, WMergeField mergeField)
	{
		string text = string.Empty;
		for (int i = 0; i < (entity as WTable).Rows.Count; i++)
		{
			WTableRow wTableRow = (entity as WTable).Rows[i];
			for (int j = 0; j < wTableRow.Cells.Count; j++)
			{
				WTableCell wTableCell = wTableRow.Cells[j];
				for (int k = 0; k < wTableCell.Items.Count; k++)
				{
					text += UpdateTextForTextBodyItem(wTableCell.Items[k], isUpdateNestedFields, mergeField);
					if (IsFieldSeparator)
					{
						return text;
					}
				}
				if (!IsSkip)
				{
					text += "\a";
				}
			}
			if (!IsSkip)
			{
				text += "\r\a";
			}
		}
		return text;
	}

	protected string UpdateTextForParagraphItem(Entity entity, bool isUpdateNestedFields)
	{
		string text = string.Empty;
		if (IsFieldSeparator)
		{
			return text;
		}
		if (entity is WField && !IsSkip)
		{
			WField wField = entity as WField;
			if (wField.FieldEnd != null)
			{
				if (isUpdateNestedFields)
				{
					if (wField.FieldType == FieldType.FieldMergeField || wField.FieldType == FieldType.FieldHyperlink || wField.FieldType == FieldType.FieldAutoNum || wField.FieldType == FieldType.FieldAutoNumLegal)
					{
						text = wField.Text;
					}
					else
					{
						if (!wField.IsUpdated)
						{
							wField.Range.Items.Clear();
							wField.UpdateFieldRange();
							wField.IsFieldRangeUpdated = false;
							wField.Update();
						}
						text = ((wField.FieldType != FieldType.FieldUnknown) ? wField.FieldResult : wField.Text);
					}
				}
				else
				{
					text = wField.FindFieldResult();
				}
				m_nestedFields.Push(wField);
				if (wField.FieldType != FieldType.FieldAutoNum && wField.FieldType != FieldType.FieldAutoNumLegal)
				{
					IsSkip = true;
				}
			}
		}
		else if (FieldSeparator == entity)
		{
			IsFieldSeparator = true;
		}
		else if (IsSkip && entity is WFieldMark && m_nestedFields.Peek().FieldEnd == entity)
		{
			IsSkip = false;
			m_nestedFields.Pop();
		}
		else if (entity is WTextRange && !IsSkip)
		{
			text = (entity as WTextRange).Text;
		}
		else if (entity is InlineContentControl && !IsSkip)
		{
			for (int i = 0; i < (entity as InlineContentControl).ParagraphItems.Count; i++)
			{
				Entity entity2 = (entity as InlineContentControl).ParagraphItems[i];
				text = ((!(entity2 is ParagraphItem)) ? (text + UpdateTextForTextBodyItem(entity2, isUpdateNestedFields)) : (text + UpdateTextForParagraphItem(entity2, isUpdateNestedFields)));
				if (IsFieldSeparator)
				{
					break;
				}
			}
		}
		return text;
	}

	internal string GetFieldResultValue()
	{
		string text = string.Empty;
		if (base.Owner is WParagraph && FieldSeparator != null && FieldSeparator.Owner is WParagraph && FieldEnd != null && FieldEnd.Owner is WParagraph && FieldType != FieldType.FieldIncludePicture)
		{
			int num = 0;
			if (Range.Items.Count == 0 && !IsFieldRangeUpdated)
			{
				UpdateFieldRange();
			}
			num = ((FieldSeparator.OwnerParagraph != base.OwnerParagraph) ? Range.Items.IndexOf(FieldSeparator.OwnerParagraph) : (Range.Items.IndexOf(FieldSeparator) + 1));
			if (num == -1)
			{
				return text;
			}
			for (int i = num; i < Range.Items.Count; i++)
			{
				Entity entity = Range.Items[i] as Entity;
				if (entity is WParagraph)
				{
					int startIndex = 0;
					int num2 = (entity as WParagraph).Items.Count - 1;
					if (FieldSeparator.OwnerParagraph == entity)
					{
						startIndex = FieldSeparator.GetIndexInOwnerCollection() + 1;
					}
					if (FieldEnd.OwnerParagraph == entity)
					{
						num2 = FieldEnd.GetIndexInOwnerCollection() - 1;
					}
					if (num2 != -1)
					{
						text += (entity as WParagraph).GetText(startIndex, num2);
					}
					if (FieldEnd.OwnerParagraph != entity)
					{
						text += ControlChar.ParagraphBreak;
					}
					if (num2 != -1 && num2 < (entity as WParagraph).Items.Count - 1)
					{
						text = text + (entity as WParagraph).GetText(num2 + 1, (entity as WParagraph).Items.Count - 1) + ControlChar.ParagraphBreak;
						base.Document.m_prevClonedEntity = FieldEnd.OwnerParagraph;
					}
					continue;
				}
				if (entity is WTable)
				{
					text += (entity as WTable).GetTableText();
					continue;
				}
				if (entity is WField)
				{
					int num3 = Range.Items.IndexOf((entity as WField).FieldSeparator);
					if (num3 != -1)
					{
						i = num3;
						continue;
					}
				}
				if (entity is WMergeField)
				{
					text += (entity as WField).Text;
				}
				else if (entity is WTextRange)
				{
					text += (entity as WTextRange).Text;
				}
				else if (entity is Break)
				{
					text += ControlChar.ParagraphBreak;
				}
			}
		}
		return text;
	}

	private void ParseFieldValue(string fieldValue)
	{
		Match match = new Regex("(\\w+)\\s+\"?([^\"]*)\"?").Match(fieldValue.Trim());
		if (match.Groups[2].Length == 0)
		{
			m_fieldValue = match.Groups[1].Value;
		}
		else
		{
			m_fieldValue = match.Groups[2].Value;
		}
	}

	private static string ClearStringFromOtherCharacters(string value)
	{
		string text = value.Remove(0, 1);
		char[] trimChars = new char[1] { '"' };
		return text.Trim(trimChars);
	}

	internal void RemoveFieldSeparator(WFieldMark fieldMark)
	{
		int indexInOwnerCollection = fieldMark.ParentField.FieldSeparator.GetIndexInOwnerCollection();
		WParagraph ownerParagraph = fieldMark.OwnerParagraph;
		BookmarkStart bookmarkStart = new BookmarkStart(m_doc, "_fieldBookmark");
		BookmarkEnd bookmarkEnd = new BookmarkEnd(m_doc, "_fieldBookmark");
		ownerParagraph.ChildEntities.Insert(indexInOwnerCollection, bookmarkStart);
		EnsureBookmarkStart(bookmarkStart);
		WParagraph ownerParagraph2 = fieldMark.OwnerParagraph;
		int indexInOwnerCollection2 = fieldMark.GetIndexInOwnerCollection();
		ownerParagraph2.Items.Insert(indexInOwnerCollection2, bookmarkEnd);
		EnsureBookmarkStart(bookmarkEnd);
		BookmarksNavigator bookmarksNavigator = new BookmarksNavigator(m_doc);
		bookmarksNavigator.MoveToBookmark("_fieldBookmark");
		bookmarksNavigator.DeleteBookmarkContent(saveFormatting: false);
		if (ownerParagraph.Items.Contains(bookmarkStart))
		{
			ownerParagraph.Items.Remove(bookmarkStart);
		}
		if (ownerParagraph2.Items.Contains(bookmarkEnd))
		{
			ownerParagraph2.Items.Remove(bookmarkEnd);
		}
	}

	protected void ParseField(string fieldCode)
	{
		char[] separator = new char[1] { '\\' };
		string[] array = fieldCode.Split(separator);
		ParseFieldValue(array[0]);
		ParseFieldFormat(array);
	}

	protected void ParseFieldFormat(string[] fieldValues)
	{
		for (int i = 1; i < fieldValues.Length; i++)
		{
			string text = fieldValues[i];
			if (text.Length <= 0)
			{
				continue;
			}
			string text2 = ClearStringFromOtherCharacters(text).Trim();
			switch (text[0])
			{
			case '*':
				switch (text2)
				{
				case "Upper":
					m_textFormat = TextFormat.Uppercase;
					SetTextFormatSwitchString();
					break;
				case "Lower":
					m_textFormat = TextFormat.Lowercase;
					SetTextFormatSwitchString();
					break;
				case "Caps":
					m_textFormat = TextFormat.Titlecase;
					SetTextFormatSwitchString();
					break;
				case "FirstCap":
					m_textFormat = TextFormat.FirstCapital;
					SetTextFormatSwitchString();
					break;
				default:
					m_formattingString = m_formattingString + " \\" + text;
					break;
				}
				break;
			default:
				m_formattingString = m_formattingString + " \\" + text;
				break;
			}
		}
	}

	private void ParseLocalRef(string fieldCode, int startPos)
	{
		startPos += 2;
		if (fieldCode.Length <= startPos)
		{
			return;
		}
		string text = fieldCode.Substring(startPos, fieldCode.Length - startPos).Trim();
		int num = text.IndexOf("\"");
		if (num != -1)
		{
			int num2 = text.IndexOf("\"", num + 1);
			if (num2 != -1)
			{
				m_localRef = text.Substring(num, num2 + 1 - num);
			}
		}
	}

	private string ParseSwitches(string text, int index)
	{
		text = text.Remove(0, index + 2).Trim();
		string startCharacter = string.Empty;
		string endCharacter = string.Empty;
		if (IsStartWithValidChar(text, ref startCharacter) && !string.IsNullOrEmpty(startCharacter) && IsEndWithValidChar(text, ref endCharacter) && !string.IsNullOrEmpty(endCharacter))
		{
			text = text.Remove(0, text.IndexOf(startCharacter) + 1);
			text = text.Remove(text.LastIndexOf(endCharacter));
		}
		return text;
	}

	private bool IsStartWithValidChar(string text, ref string startCharacter)
	{
		startCharacter = string.Empty;
		string[] array = new string[2]
		{
			"\"",
			'“'.ToString()
		};
		foreach (string text2 in array)
		{
			if (StartsWithExt(text, text2))
			{
				startCharacter = text2;
				return true;
			}
		}
		return false;
	}

	private bool IsEndWithValidChar(string text, ref string endCharacter)
	{
		endCharacter = string.Empty;
		string[] array = new string[2]
		{
			"\"",
			'”'.ToString()
		};
		foreach (string text2 in array)
		{
			if (text.EndsWith(text2))
			{
				endCharacter = text2;
				return true;
			}
		}
		return false;
	}

	private string RemoveMeridiem(string text, out bool isMeridiemDefined)
	{
		isMeridiemDefined = false;
		string text2 = text.ToLower();
		if (text2.Contains("am/pm"))
		{
			text = text.Remove(text2.IndexOf("am/pm"), text2.Length - text2.IndexOf("am/pm"));
			isMeridiemDefined = true;
		}
		return text;
	}

	private string UpdateMeridiem(string text, DateTime currentDateTime)
	{
		CultureInfo cultureInfo = null;
		cultureInfo = ((!base.CharacterFormat.HasValue(73)) ? CultureInfo.CurrentCulture : GetCulture((LocaleIDs)base.CharacterFormat.LocaleIdASCII));
		text = ((currentDateTime.Hour >= 12) ? (text + cultureInfo.DateTimeFormat.PMDesignator) : (text + cultureInfo.DateTimeFormat.AMDesignator));
		return text;
	}

	internal string UpdateDateValue(string text, DateTime currentDateTime)
	{
		int num = 0;
		bool flag = false;
		string text2 = string.Empty;
		int num2 = 0;
		while (num2 < text.Length)
		{
			if (flag)
			{
				switch (text[num2])
				{
				case '\'':
					flag = false;
					num2++;
					break;
				case '\\':
					num2++;
					break;
				default:
					text2 += text[num2];
					num2++;
					break;
				}
				continue;
			}
			char c = text[num2];
			if ((uint)c <= 89u)
			{
				if ((uint)c <= 72u)
				{
					if (c == '\'')
					{
						flag = true;
						num2++;
						continue;
					}
					if (c == 'D')
					{
						goto IL_00f9;
					}
					if (c == 'H')
					{
						goto IL_018e;
					}
				}
				else
				{
					if (c == 'M')
					{
						while (num2 < text.Length && text[num2] == 'M')
						{
							num2++;
							num++;
						}
						text2 = UpdateMonth(text2, currentDateTime, num);
						num = 0;
						continue;
					}
					if (c == 'S')
					{
						goto IL_020d;
					}
					if (c == 'Y')
					{
						goto IL_015e;
					}
				}
			}
			else if ((uint)c <= 104u)
			{
				if (c == '\\')
				{
					num2++;
					continue;
				}
				if (c == 'd')
				{
					goto IL_00f9;
				}
				if (c == 'h')
				{
					goto IL_018e;
				}
			}
			else
			{
				if (c == 'm')
				{
					while (num2 < text.Length && text[num2] == 'm')
					{
						num2++;
						num++;
					}
					text2 = UpdateMinute(text2, currentDateTime, num);
					num = 0;
					continue;
				}
				if (c == 's')
				{
					goto IL_020d;
				}
				if (c == 'y')
				{
					goto IL_015e;
				}
			}
			text2 += text[num2];
			num2++;
			continue;
			IL_018e:
			bool is12HoursFormat = false;
			while (num2 < text.Length && (text[num2] == 'h' || text[num2] == 'H'))
			{
				if (text[num2] == 'h')
				{
					is12HoursFormat = true;
				}
				num2++;
				num++;
			}
			text2 = UpdateHour(text2, currentDateTime, num, is12HoursFormat);
			num = 0;
			continue;
			IL_015e:
			while (num2 < text.Length && (text[num2] == 'y' || text[num2] == 'Y'))
			{
				num2++;
				num++;
			}
			text2 = UpdateYear(text2, currentDateTime, num);
			num = 0;
			continue;
			IL_020d:
			while (num2 < text.Length && (text[num2] == 's' || text[num2] == 'S'))
			{
				num2++;
				num++;
			}
			text2 = UpdateSecond(text2, currentDateTime, num);
			num = 0;
			continue;
			IL_00f9:
			while (num2 < text.Length && (text[num2] == 'd' || text[num2] == 'D'))
			{
				num2++;
				num++;
			}
			text2 = UpdateDay(text2, currentDateTime, num);
			num = 0;
		}
		return text2;
	}

	private string UpdateDay(string dateValue, DateTime currentDateTime, int count)
	{
		string empty = string.Empty;
		CultureInfo cultureFromCharFormat = GetCultureFromCharFormat();
		dateValue += count switch
		{
			1 => currentDateTime.Day.ToString(), 
			2 => (Convert.ToInt16(currentDateTime.Day.ToString()) >= 10) ? currentDateTime.Day.ToString() : ("0" + currentDateTime.Day), 
			3 => cultureFromCharFormat.DateTimeFormat.DayNames[(int)currentDateTime.DayOfWeek].Substring(0, 3), 
			_ => cultureFromCharFormat.DateTimeFormat.DayNames[(int)currentDateTime.DayOfWeek], 
		};
		return dateValue;
	}

	internal CultureInfo GetCulture(LocaleIDs localID)
	{
		string text = string.Empty;
		if (Enum.IsDefined(typeof(LocaleIDs), localID))
		{
			text = ((localID != LocaleIDs.es_ES_tradnl) ? localID.ToString().Replace('_', '-') : "es-ES_tradnl");
		}
		else
		{
			switch ((short)localID)
			{
			case 1:
				text = "ar-SA";
				break;
			case 2:
				text = "bg-BG";
				break;
			case 3:
				text = "ca-ES";
				break;
			case 4:
				text = "zh-TW";
				break;
			case 5:
				text = "cs-CZ";
				break;
			case 6:
				text = "da-DK";
				break;
			case 7:
				text = "de-DE";
				break;
			case 8:
				text = "el-GR";
				break;
			case 9:
				text = "en-US";
				break;
			case 10:
				text = "es-ES_tradnl";
				break;
			case 11:
				text = "fi-FI";
				break;
			case 12:
				text = "fr-FR";
				break;
			case 13:
				text = "he-IL";
				break;
			case 14:
				text = "hu-HU";
				break;
			case 15:
				text = "is-IS";
				break;
			case 16:
				text = "it-IT";
				break;
			case 17:
				text = "ja-JP";
				break;
			case 18:
				text = "ko-KR";
				break;
			case 19:
				text = "nl-NL";
				break;
			case 20:
				text = "nb-NO";
				break;
			case 21:
				text = "pl-PL";
				break;
			case 22:
				text = "pt-BR";
				break;
			case 23:
				text = "rm-CH";
				break;
			case 24:
				text = "ro-RO";
				break;
			case 25:
				text = "ru-RU";
				break;
			case 26:
				text = "hr-HR";
				break;
			case 27:
				text = "sk-SK";
				break;
			case 28:
				text = "sq-AL";
				break;
			case 29:
				text = "sv-SE";
				break;
			case 30:
				text = "th-TH";
				break;
			case 31:
				text = "tr-TR";
				break;
			case 32:
				text = "ur-PK";
				break;
			case 33:
				text = "id-ID";
				break;
			case 34:
				text = "uk-UA";
				break;
			case 35:
				text = "be-BY";
				break;
			case 36:
				text = "sl-SI";
				break;
			case 37:
				text = "et-EE";
				break;
			case 38:
				text = "lv-LV";
				break;
			case 39:
				text = "lt-LT";
				break;
			case 40:
				text = "tg-Cyrl-TJ";
				break;
			case 41:
				text = "fa-IR";
				break;
			case 42:
				text = "vi-VN";
				break;
			case 43:
				text = "hy-AM";
				break;
			case 44:
				text = "az-Latn-AZ";
				break;
			case 45:
				text = "eu-Es";
				break;
			case 46:
				text = "hsb-DE";
				break;
			case 47:
				text = "mk-MK";
				break;
			case 50:
				text = "tn-ZA";
				break;
			case 52:
				text = "xh-ZA";
				break;
			case 53:
				text = "zu-ZA";
				break;
			case 54:
				text = "af-ZA";
				break;
			case 55:
				text = "ka-GE";
				break;
			case 56:
				text = "fo-FO";
				break;
			case 57:
				text = "hi-IN";
				break;
			case 58:
				text = "mt-MT";
				break;
			case 59:
				text = "se-NO";
				break;
			case 62:
				text = "ms-MY";
				break;
			case 63:
				text = "kk-KZ";
				break;
			case 64:
				text = "ky-KG";
				break;
			case 65:
				text = "sw-KE";
				break;
			case 66:
				text = "tk-TM";
				break;
			case 67:
				text = "uz-Latn-UZ";
				break;
			case 68:
				text = "tt-RU";
				break;
			case 69:
				text = "bn-IN";
				break;
			case 70:
				text = "pa-IN";
				break;
			case 71:
				text = "gu-IN";
				break;
			case 72:
				text = "or-IN";
				break;
			case 73:
				text = "ta-IN";
				break;
			case 74:
				text = "te-IN";
				break;
			case 75:
				text = "kn-IN";
				break;
			case 76:
				text = "ml-IN";
				break;
			case 78:
				text = "mr-IN";
				break;
			case 79:
				text = "sa-IN";
				break;
			case 80:
				text = "mn-MN";
				break;
			case 81:
				text = "bo-CN";
				break;
			case 82:
				text = "cy-GB";
				break;
			case 84:
				text = "lo-LA";
				break;
			case 86:
				text = "gl-ES";
				break;
			case 87:
				text = "kok-IN";
				break;
			case 90:
				text = "syr-SY";
				break;
			case 91:
				text = "si-LK";
				break;
			case 92:
				text = "chr-US";
				break;
			case 93:
				text = "iu-Cans-CA";
				break;
			case 94:
				text = "am-ET";
				break;
			case 97:
				text = "ne-NP";
				break;
			case 98:
				text = "fy-NL";
				break;
			case 99:
				text = "ps-AF";
				break;
			case 100:
				text = "fil-PH";
				break;
			case 101:
				text = "dv-MV";
				break;
			case 103:
				text = "ff-NG";
				break;
			case 104:
				text = "ha-Latn-NG";
				break;
			case 107:
				text = "quz-BO";
				break;
			case 108:
				text = "nso-ZA";
				break;
			case 109:
				text = "ba-RU";
				break;
			case 110:
				text = "lb-LU";
				break;
			case 111:
				text = "kl-GL";
				break;
			case 112:
				text = "ig-NG";
				break;
			case 115:
				text = "ti-ET";
				break;
			case 117:
				text = "haw-US";
				break;
			case 120:
				text = "ii-CN";
				break;
			case 122:
				text = "arn-CL";
				break;
			case 126:
				text = "br-FR";
				break;
			case 128:
				text = "ug-CN";
				break;
			case 129:
				text = "mi-NZ";
				break;
			case 130:
				text = "oc-FR";
				break;
			case 131:
				text = "co-FR";
				break;
			case 132:
				text = "gsw-FR";
				break;
			case 133:
				text = "sah-RU";
				break;
			case 134:
				text = "qut-GT";
				break;
			case 135:
				text = "rw-Rw";
				break;
			case 140:
				text = "prs-AF";
				break;
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			return new CultureInfo(text);
		}
		return CultureInfo.CurrentCulture;
	}

	private string UpdateMonth(string dateValue, DateTime currentDateTime, int count)
	{
		string empty = string.Empty;
		CultureInfo cultureFromCharFormat = GetCultureFromCharFormat();
		dateValue += count switch
		{
			1 => currentDateTime.Month.ToString(), 
			2 => (Convert.ToInt16(currentDateTime.Month.ToString()) >= 10) ? currentDateTime.Month.ToString() : ("0" + currentDateTime.Month), 
			3 => cultureFromCharFormat.DateTimeFormat.MonthNames[currentDateTime.Month - 1].Substring(0, 3), 
			_ => cultureFromCharFormat.DateTimeFormat.MonthNames[currentDateTime.Month - 1], 
		};
		return dateValue;
	}

	private CultureInfo GetCultureFromCharFormat()
	{
		CultureInfo culture = CultureInfo.CurrentCulture;
		Entity entity = GetFirstValidFieldCodeItem();
		if (culture.TextInfo.IsRightToLeft && entity is WTextRange && !StartsWithExt((entity as WTextRange).Text, " "))
		{
			while (entity != this && entity.PreviousSibling != null)
			{
				entity = entity.PreviousSibling as Entity;
				if (entity is WTextRange && StartsWithExt((entity as WTextRange).Text, " "))
				{
					break;
				}
			}
		}
		WCharacterFormat wCharacterFormat = ((entity is WTextRange) ? (entity as WTextRange).CharacterFormat : base.CharacterFormat);
		bool isRTL = entity != this && (wCharacterFormat.HasKey(58) || wCharacterFormat.HasKey(99));
		bool flag = GetHasKeyFromCharacterFormat(wCharacterFormat, ref culture, isRTL);
		if (!flag && base.CharacterFormat.CharStyle != null)
		{
			flag = GetHasKeyFromBaseFormat(wCharacterFormat.CharStyle.CharacterFormat, ref culture, isRTL);
		}
		if (!flag)
		{
			flag = GetHasKeyFromBaseFormat(wCharacterFormat.BaseFormat, ref culture, isRTL);
		}
		if (!flag)
		{
			WParagraph ownerParagraphValue = GetOwnerParagraphValue();
			if (ownerParagraphValue.IsInCell && ownerParagraphValue.OwnerBase is WTableCell && (ownerParagraphValue.OwnerBase as WTableCell).OwnerRow != null && (ownerParagraphValue.OwnerBase as WTableCell).OwnerRow.OwnerTable != null && base.Document.Styles.FindByName((ownerParagraphValue.OwnerBase as WTableCell).OwnerRow.OwnerTable.StyleName, StyleType.TableStyle) is WTableStyle { CharacterFormat: not null } wTableStyle)
			{
				flag = GetHasKeyFromBaseFormat(wTableStyle.CharacterFormat, ref culture, isRTL);
			}
		}
		if (!flag && base.Document.DefCharFormat != null)
		{
			flag = GetHasKeyFromCharacterFormat(base.Document.DefCharFormat, ref culture, isRTL);
		}
		return culture;
	}

	private bool GetHasKeyFromCharacterFormat(WCharacterFormat characterFormat, ref CultureInfo culture, bool isRTL)
	{
		bool num = (isRTL ? characterFormat.HasKey(75) : characterFormat.HasKey(73));
		if (num)
		{
			LocaleIDs localID = (LocaleIDs)(isRTL ? characterFormat.LocaleIdBidi : characterFormat.LocaleIdASCII);
			culture = GetCulture(localID);
		}
		return num;
	}

	private bool GetHasKeyFromBaseFormat(FormatBase baseFormat, ref CultureInfo culture, bool isRTL)
	{
		bool flag = false;
		while (baseFormat != null && baseFormat is WCharacterFormat && (!(baseFormat.OwnerBase is WTableStyle) || !((baseFormat.OwnerBase as WTableStyle).Name == "Normal Table")))
		{
			flag = (isRTL ? baseFormat.HasKey(75) : baseFormat.HasKey(73));
			if (flag)
			{
				LocaleIDs localID = (LocaleIDs)(isRTL ? (baseFormat as WCharacterFormat).LocaleIdBidi : (baseFormat as WCharacterFormat).LocaleIdASCII);
				culture = GetCulture(localID);
				break;
			}
			baseFormat = baseFormat.BaseFormat;
		}
		return flag;
	}

	private Entity GetFirstValidFieldCodeItem()
	{
		foreach (Entity item in Range.Items)
		{
			Entity entity2 = null;
			entity2 = ((!(item is ParagraphItem)) ? GetValidFieldCodeForTextBodyItem(item) : GetValidFieldCodeForParagraphItem(item));
			if (entity2 != null)
			{
				return entity2;
			}
		}
		return this;
	}

	private Entity GetValidFieldCodeForTextBodyItem(Entity entity)
	{
		if (entity is WParagraph)
		{
			foreach (ParagraphItem item in (entity as WParagraph).Items)
			{
				Entity validFieldCodeForParagraphItem = GetValidFieldCodeForParagraphItem(item);
				if (validFieldCodeForParagraphItem != null)
				{
					return validFieldCodeForParagraphItem;
				}
			}
		}
		else
		{
			if (entity is WTable)
			{
				return GetValidFieldCodeForTable(entity);
			}
			if (entity is BlockContentControl)
			{
				return GetValidFieldCodeForTextBodyItem(entity);
			}
		}
		return null;
	}

	private Entity GetValidFieldCodeForTable(Entity entity)
	{
		foreach (WTableRow row in (entity as WTable).Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				foreach (TextBodyItem item in cell.Items)
				{
					Entity validFieldCodeForTextBodyItem = GetValidFieldCodeForTextBodyItem(item);
					if (validFieldCodeForTextBodyItem != null)
					{
						return validFieldCodeForTextBodyItem;
					}
				}
			}
		}
		return null;
	}

	private Entity GetValidFieldCodeForParagraphItem(Entity entity)
	{
		if (entity is WTextRange && !string.IsNullOrWhiteSpace((entity as WTextRange).Text))
		{
			return entity;
		}
		if (entity is InlineContentControl)
		{
			foreach (ParagraphItem paragraphItem in (entity as InlineContentControl).ParagraphItems)
			{
				Entity validFieldCodeForParagraphItem = GetValidFieldCodeForParagraphItem(paragraphItem);
				if (validFieldCodeForParagraphItem != null)
				{
					return validFieldCodeForParagraphItem;
				}
			}
		}
		return null;
	}

	private string UpdateYear(string dateValue, DateTime currentDateTime, int count)
	{
		string empty = string.Empty;
		dateValue += count switch
		{
			1 => currentDateTime.Year.ToString().Remove(0, 2), 
			2 => currentDateTime.Year.ToString().Remove(0, 2), 
			4 => currentDateTime.Year.ToString(), 
			_ => currentDateTime.Year.ToString(), 
		};
		return dateValue;
	}

	private string UpdateHour(string dateValue, DateTime currentDateTime, int count, bool is12HoursFormat)
	{
		int num = currentDateTime.Hour;
		string text = num.ToString();
		if (is12HoursFormat && num > 12)
		{
			num -= 12;
			text = num.ToString();
		}
		if (count == 2 && num < 10)
		{
			text = "0" + text;
		}
		dateValue += text;
		return dateValue;
	}

	private string UpdateMinute(string dateValue, DateTime currentDateTime, int count)
	{
		string empty = string.Empty;
		dateValue += count switch
		{
			1 => currentDateTime.Minute.ToString(), 
			2 => (Convert.ToInt16(currentDateTime.Minute.ToString()) >= 10) ? currentDateTime.Minute.ToString() : ("0" + currentDateTime.Minute), 
			_ => currentDateTime.Minute.ToString(), 
		};
		return dateValue;
	}

	private string UpdateSecond(string dateValue, DateTime currentDateTime, int count)
	{
		string empty = string.Empty;
		dateValue += count switch
		{
			1 => currentDateTime.Second.ToString(), 
			2 => (Convert.ToInt16(currentDateTime.Second.ToString()) >= 10) ? currentDateTime.Second.ToString() : ("0" + currentDateTime.Second), 
			_ => currentDateTime.Second.ToString(), 
		};
		return dateValue;
	}

	internal void CheckFieldSeparator()
	{
		if (FieldSeparator == null)
		{
			WFieldMark wFieldMark = new WFieldMark(base.Document, FieldMarkType.FieldSeparator);
			FieldEnd.OwnerParagraph.Items.Insert(FieldEnd.GetIndexInOwnerCollection(), wFieldMark);
			FieldSeparator = wFieldMark;
			IsFieldRangeUpdated = false;
		}
		else if (FieldType != FieldType.FieldDate && FieldSeparator.NextSibling != null)
		{
			m_resultFormat = (FieldSeparator.NextSibling as ParagraphItem).ParaItemCharFormat.CloneInt() as WCharacterFormat;
		}
	}

	private void GetFormattingForHyperLink()
	{
		if (FieldEnd.PreviousSibling == null)
		{
			m_resultFormat = FieldEnd.ParaItemCharFormat.CloneInt() as WCharacterFormat;
		}
		else
		{
			m_resultFormat = (FieldEnd.PreviousSibling as ParagraphItem).ParaItemCharFormat.CloneInt() as WCharacterFormat;
		}
	}

	protected Entity GetClonedTable(Entity entity, bool isRefFieldUpdate)
	{
		Entity entity2 = entity.Clone();
		WTable wTable = entity2 as WTable;
		WTable wTable2 = entity as WTable;
		for (int i = 0; i < wTable2.Rows.Count; i++)
		{
			for (int j = 0; j < wTable2.Rows[i].Cells.Count; j++)
			{
				UpdateClonedTextBodyItem(wTable2.Rows[i].Cells[j], wTable.Rows[i].Cells[j], isRefFieldUpdate);
			}
		}
		return entity2;
	}

	private Entity GetClonedContentControl(Entity entity, bool isRefFieldUpdate)
	{
		Entity entity2 = entity.Clone();
		BlockContentControl blockContentControl = entity2 as BlockContentControl;
		BlockContentControl blockContentControl2 = entity as BlockContentControl;
		for (int i = 0; i < blockContentControl2.TextBody.Count; i++)
		{
			UpdateClonedTextBodyItem(blockContentControl2.TextBody, blockContentControl.TextBody, isRefFieldUpdate);
		}
		return entity2;
	}

	private void UpdateClonedTextBodyItem(WTextBody source, WTextBody destination, bool isRefFieldUpdate)
	{
		destination.Items.Clear();
		int itemIndex = 0;
		for (int i = 0; i < source.Items.Count; i++)
		{
			Entity nextItem = null;
			if (source.Items[i] is WParagraph)
			{
				foreach (Entity item in GetClonedParagraph(source.Items[i], null, ref nextItem, ref itemIndex, isRefFieldUpdate))
				{
					destination.Items.Add(item);
				}
				if (nextItem != null)
				{
					i = source.Items.IndexOf(nextItem);
					if (itemIndex > 0)
					{
						i--;
					}
					m_nestedFields.Clear();
					IsSkip = false;
					nextItem = null;
				}
			}
			else if (source.Items[i] is BlockContentControl)
			{
				destination.Items.Add(GetClonedContentControl(source.Items[i], isRefFieldUpdate));
			}
			else
			{
				destination.Items.Add(GetClonedTable(source.Items[i], isRefFieldUpdate));
			}
		}
	}

	private List<Entity> GetClonedParagraph(Entity entity, string txt, ref Entity nextItem, ref int itemIndex, bool isRefFieldUpdate)
	{
		List<Entity> itemsToUpdate = new List<Entity>();
		WParagraph paragraph = entity.Clone() as WParagraph;
		paragraph.ClearItems();
		if (txt != null && txt.EndsWith("\r"))
		{
			txt = txt.Remove(txt.Length - 1);
		}
		string text = string.Empty;
		string text2 = txt;
		itemIndex = GetParagraphItemIndex(entity, txt, itemIndex);
		bool flag = false;
		for (int i = itemIndex; i < (entity as WParagraph).Items.Count; i++)
		{
			ParagraphItem paragraphItem = (entity as WParagraph).Items[i];
			Entity entity2 = paragraphItem.Clone();
			bool isFieldSeparator = IsFieldSeparator;
			IsFieldSeparator = false;
			if (!isRefFieldUpdate || !(paragraphItem is WField) || (paragraphItem as WField).FieldType != FieldType.FieldSequence)
			{
				text = UpdateTextForParagraphItem((entity as WParagraph).Items[i], isUpdateNestedFields: true);
			}
			IsFieldSeparator = isFieldSeparator;
			bool flag2 = false;
			if (((paragraphItem is WField && ((paragraphItem as WField).FieldSeparator != null || (paragraphItem as WField).FieldType == FieldType.FieldAutoNum || (paragraphItem as WField).FieldType == FieldType.FieldAutoNumLegal) && (paragraphItem as WField).FieldEnd != null) || (paragraphItem is WIfField && (paragraphItem as WField).FieldEnd != null)) && (paragraphItem as WField).FieldType != FieldType.FieldHyperlink)
			{
				bool isResultFound = false;
				if ((paragraphItem as WField).FieldSeparator == null && ((paragraphItem as WField).FieldType == FieldType.FieldAutoNum || (paragraphItem as WField).FieldType == FieldType.FieldAutoNumLegal))
				{
					WTextRange wTextRange = new WTextRange(paragraphItem.Document);
					wTextRange.Text = text;
					wTextRange.CharacterFormat.ImportContainer((paragraphItem as WField).CharacterFormat);
					wTextRange.CharacterFormat.CopyProperties((paragraphItem as WField).CharacterFormat);
					if (wTextRange.CharacterFormat.HasValue(106))
					{
						wTextRange.CharacterFormat.PropertiesHash.Remove(106);
					}
					paragraph.Items.Add(wTextRange);
				}
				else if (paragraphItem is WDropDownFormField)
				{
					WTextRange wTextRange2 = new WTextRange(paragraphItem.Document);
					wTextRange2.Text = (paragraphItem as WDropDownFormField).DropDownValue;
					wTextRange2.CharacterFormat.ImportContainer((paragraphItem as WDropDownFormField).CharacterFormat);
					paragraph.Items.Add(wTextRange2);
				}
				else
				{
					flag2 = UpdateFieldItems(paragraphItem, entity2, ref paragraph, ref itemsToUpdate, ref itemIndex, ref isResultFound);
				}
				nextItem = (paragraphItem as WField).Range.Items[(paragraphItem as WField).Range.Items.Count - 1] as Entity;
				if (nextItem is WParagraph && flag2 && IsSkipToAddEmptyPara(paragraph, paragraphItem as WField, isResultFound))
				{
					flag = true;
				}
			}
			else if (txt != null && entity2 is WTextRange)
			{
				if (i == itemIndex && text.Contains(txt) && !StartsWithExt(text, txt))
				{
					text = txt;
				}
				if (text.Contains(text2) && !text2.Contains(text))
				{
					text = text2;
				}
				(entity2 as WTextRange).Text = text;
			}
			if (!string.IsNullOrEmpty(text2))
			{
				text2 = text2.Substring(text.Length);
			}
			if (flag2)
			{
				break;
			}
			if (!(entity2 is WField) && !(entity2 is BookmarkStart) && !(entity2 is BookmarkEnd) && (!(entity2 is WFieldMark) || (entity2 as WFieldMark).ParentField.FieldType == FieldType.FieldEmbed))
			{
				paragraph.Items.Add(entity2);
			}
			if (text2 != null && text2 == string.Empty)
			{
				break;
			}
			if (nextItem != null)
			{
				i = (entity as WParagraph).Items.IndexOf(nextItem);
				m_nestedFields.Clear();
				IsSkip = false;
				nextItem = null;
			}
		}
		if (((entity as WParagraph).Items.Count == 0 || (entity as WParagraph).Items.Count > 0) && !flag)
		{
			itemsToUpdate.Add(paragraph);
		}
		return itemsToUpdate;
	}

	private bool UpdateFieldItems(ParagraphItem item, Entity entity, ref WParagraph paragraph, ref List<Entity> itemsToUpdate, ref int itemIndex, ref bool isResultFound)
	{
		bool result = false;
		int num = 0;
		WField wField = item as WField;
		if (wField.Range.Items.Contains(wField.FieldSeparator))
		{
			num = wField.Range.Items.IndexOf(wField.FieldSeparator) + 1;
		}
		else if (wField.Range.Items.Contains(wField.FieldSeparator.OwnerParagraph))
		{
			num = wField.Range.Items.IndexOf(wField.FieldSeparator.OwnerParagraph);
		}
		for (int i = num; i < wField.Range.Items.Count; i++)
		{
			if (wField.Range.Items[i] == wField.FieldSeparator.OwnerParagraph)
			{
				WParagraph wParagraph = wField.Range.Items[i] as WParagraph;
				if (wParagraph.LastItem == wField.FieldSeparator)
				{
					continue;
				}
				if (i == wField.Range.Items.Count - 1 && wParagraph.Items.Count > 0 && wParagraph.Items[0] == wField.FieldEnd)
				{
					break;
				}
				for (Entity entity2 = wField.FieldSeparator.NextSibling as Entity; entity2 != null; entity2 = entity2.NextSibling as Entity)
				{
					if (entity2 == wField.FieldEnd)
					{
						itemIndex = ((wField.FieldEnd.Index < wField.FieldEnd.OwnerParagraph.Items.Count - 1) ? (wField.FieldEnd.Index + 1) : 0);
						break;
					}
					isResultFound = true;
					paragraph.Items.Add(entity2.Clone());
				}
				entity = paragraph;
			}
			else if (wField.Range.Items[i] is WParagraph && wField.Range.Items[i] == wField.FieldEnd.OwnerParagraph)
			{
				if (wField.FieldSeparator.OwnerParagraph != wField.FieldEnd.OwnerParagraph)
				{
					if (paragraph.ChildEntities.Count > 0)
					{
						itemsToUpdate.Add(paragraph);
					}
					result = true;
					paragraph = wField.FieldEnd.OwnerParagraph.Clone() as WParagraph;
					paragraph.ClearItems();
				}
				int index = ((paragraph.ChildEntities.Count > 0) ? paragraph.ChildEntities.Count : 0);
				Entity entity3 = wField.FieldEnd.PreviousSibling as Entity;
				itemIndex = ((wField.FieldEnd.Index < wField.FieldEnd.OwnerParagraph.Items.Count - 1) ? (wField.FieldEnd.Index + 1) : 0);
				while (entity3 != null && entity3 != wField.FieldSeparator)
				{
					isResultFound = true;
					paragraph.Items.Insert(index, entity3.Clone());
					entity3 = entity3.PreviousSibling as Entity;
				}
				entity = paragraph;
			}
			else
			{
				entity = (wField.Range.Items[i] as Entity).Clone();
			}
			if (entity is TextBodyItem)
			{
				result = true;
				if (!(entity is WParagraph) || (entity is WParagraph && !IsSkipToAddEmptyPara(entity as WParagraph, wField, isResultFound)))
				{
					itemsToUpdate.Add(entity);
				}
			}
			else if (!(entity is BookmarkStart) && !(entity is BookmarkEnd) && entity is ParagraphItem)
			{
				paragraph.Items.Add(entity);
			}
			if (i == wField.Range.Items.Count - 2 && wField.Range.Items[wField.Range.Items.Count - 1] is WFieldMark && (wField.Range.Items[wField.Range.Items.Count - 1] as WFieldMark).Type == FieldMarkType.FieldEnd)
			{
				break;
			}
		}
		return result;
	}

	private bool IsSkipToAddEmptyPara(WParagraph paragraph, WField field, bool isResultFound)
	{
		if (paragraph != null && paragraph.ChildEntities.Count == 0 && field.Index != 0 && !isResultFound)
		{
			return true;
		}
		return false;
	}

	private int GetParagraphItemIndex(Entity entity, string txt, int index)
	{
		int result = index;
		string text = string.Empty;
		if (txt != null)
		{
			bool isFieldSeparator = IsFieldSeparator;
			IsFieldSeparator = false;
			string text2 = UpdateTextForTextBodyItem(entity, isUpdateNestedFields: true);
			IsFieldSeparator = isFieldSeparator;
			if (text2.EndsWith("\r"))
			{
				text2 = text2.Remove(text2.Length - 1);
			}
			int num = text2.IndexOf(txt);
			if (num > 0)
			{
				for (int i = 0; i < (entity as WParagraph).Items.Count; i++)
				{
					text += UpdateTextForParagraphItem((entity as WParagraph).Items[i], isUpdateNestedFields: true);
					if (num == text.Length)
					{
						result = i + 1;
						break;
					}
					if (num < text.Length)
					{
						txt = text.Substring(num);
						result = i;
						break;
					}
				}
			}
		}
		return result;
	}

	private int GetStartItemIndex(int index, ref string text)
	{
		string text2 = string.Empty;
		IsFieldSeparator = false;
		m_nestedFields.Clear();
		IsSkip = false;
		string empty = string.Empty;
		for (int i = 0; i < Range.Items.Count; i++)
		{
			Entity entity = Range.Items[i] as Entity;
			empty = ((!(entity is ParagraphItem)) ? UpdateTextForTextBodyItem(entity, isUpdateNestedFields: true) : UpdateTextForParagraphItem(entity, isUpdateNestedFields: true));
			text2 += empty;
			if (IsFieldSeparator)
			{
				break;
			}
			if (index == text2.Length)
			{
				index = i + 1;
				break;
			}
			if (index >= text2.Length)
			{
				continue;
			}
			if (IsSkip)
			{
				WField wField = m_nestedFields.Pop();
				if (wField.FieldSeparator.GetIndexInOwnerCollection() == wField.FieldSeparator.OwnerParagraph.Items.Count - 1)
				{
					index = Range.Items.IndexOf(wField.FieldSeparator.OwnerParagraph) + 1;
					break;
				}
				text = text2.Substring(index);
				index = Range.Items.IndexOf(wField.FieldSeparator.OwnerParagraph);
			}
			else
			{
				text = text2.Substring(index);
				index = i;
			}
			break;
		}
		IsFieldSeparator = false;
		m_nestedFields.Clear();
		IsSkip = false;
		return index;
	}

	private void MergeFieldMarkParagraphs()
	{
		WParagraph ownerParagraph = FieldEnd.OwnerParagraph;
		int count = base.OwnerParagraph.ChildEntities.Count;
		base.OwnerParagraph.ParagraphFormat.CopyFormat(ownerParagraph.ParagraphFormat);
		base.OwnerParagraph.BreakCharacterFormat.CopyFormat(ownerParagraph.BreakCharacterFormat);
		base.OwnerParagraph.ListFormat.CopyFormat(ownerParagraph.ListFormat);
		for (int num = ownerParagraph.ChildEntities.Count - 1; num >= 0; num--)
		{
			base.OwnerParagraph.ChildEntities.Insert(count, ownerParagraph.ChildEntities[num]);
			if (base.OwnerParagraph.ChildEntities[count] == FieldEnd)
			{
				FieldEnd = base.OwnerParagraph.ChildEntities[count] as WFieldMark;
			}
		}
		ownerParagraph.RemoveSelf();
	}

	internal void RemoveFieldResult()
	{
		RemovePreviousResult();
	}

	protected void RemovePreviousResult()
	{
		for (int num = Range.Count - 1; num >= 0; num--)
		{
			Entity entity = Range.Items[num] as Entity;
			if (entity != FieldEnd)
			{
				if (entity == FieldSeparator)
				{
					break;
				}
				if (entity is ParagraphItem)
				{
					Range.Items.Remove(entity);
					base.OwnerParagraph.Items.Remove(entity);
				}
				else if (entity is WParagraph)
				{
					WTextBody ownerTextBody = (entity as WParagraph).OwnerTextBody;
					if (!IsFieldSeparator)
					{
						CheckPragragh(entity as WParagraph);
					}
					if (IsFieldSeparator)
					{
						break;
					}
					if ((entity as WParagraph).Items.Count == 0)
					{
						Range.Items.Remove(entity);
						ownerTextBody.Items.Remove(entity);
					}
				}
				else if (entity is WTable)
				{
					WTextBody ownerTextBody = (entity as WTable).OwnerTextBody;
					if (!IsFieldSeparator)
					{
						Range.Items.Remove(entity);
						ownerTextBody.Items.Remove(entity);
					}
				}
			}
		}
		IsFieldRangeUpdated = false;
	}

	private void CheckPragragh(WParagraph paragraph)
	{
		for (int num = paragraph.Items.Count - 1; num >= 0; num--)
		{
			ParagraphItem paragraphItem = paragraph.Items[num];
			if (FieldEnd.OwnerParagraph == paragraph && num > paragraph.Items.IndexOf(FieldEnd))
			{
				num = paragraph.Items.IndexOf(FieldEnd);
			}
			else if (paragraphItem != FieldEnd)
			{
				if (paragraphItem == FieldSeparator)
				{
					IsFieldSeparator = true;
					break;
				}
				if (!IsFieldSeparator)
				{
					paragraph.Items.Remove(paragraphItem);
				}
			}
		}
	}

	private void UpdateParagraphText(WParagraph paragraph, bool isLastItem, ref string result)
	{
		bool isFieldSeparator = IsFieldSeparator;
		IsFieldSeparator = false;
		result = UpdateTextForTextBodyItem(paragraph, isUpdateNestedFields: true) + result;
		IsFieldSeparator = isFieldSeparator;
		if (isLastItem)
		{
			result = result.TrimEnd('\r');
		}
	}

	private string GetParagraphItemText(ParagraphItem item)
	{
		string result = string.Empty;
		if (item is WField)
		{
			if ((item as WField).FieldType == FieldType.FieldMergeField || (item as WField).FieldType == FieldType.FieldHyperlink)
			{
				result = (item as WField).Text;
			}
			else
			{
				if (!(item as WField).IsUpdated && item != this)
				{
					(item as WField).Update();
				}
				result = (item as WField).FieldResult;
			}
		}
		else if (item is WTextRange)
		{
			result = (item as WTextRange).Text;
		}
		return result;
	}

	internal void UpdateFieldResult(string text)
	{
		UpdateFieldResult(text, isFromHyperLink: false);
	}

	internal void UpdateFieldResult(string text, bool isFromHyperLink)
	{
		if (base.IsDeleteRevision)
		{
			return;
		}
		FieldResult = text;
		if (!(base.Owner is WParagraph) || FieldEnd == null || !(FieldEnd.Owner is WParagraph))
		{
			return;
		}
		CheckFieldSeparator();
		if (isFromHyperLink)
		{
			GetFormattingForHyperLink();
		}
		RemovePreviousResult();
		if (isFromHyperLink && base.OwnerParagraph != FieldEnd.OwnerParagraph)
		{
			MergeFieldMarkParagraphs();
		}
		text = text.Replace(ControlChar.CrLf, ControlChar.ParagraphBreak);
		text = text.Replace(ControlChar.LineFeedChar, '\r');
		if (base.OwnerParagraph == FieldEnd.OwnerParagraph)
		{
			int num = text.IndexOf('\r');
			if (num != -1)
			{
				string text2 = text.Substring(num);
				text = text.Substring(0, num);
				base.OwnerParagraph.Items.Insert(FieldEnd.GetIndexInOwnerCollection(), GetTextRange(text));
				base.OwnerParagraph.Items.Insert(FieldEnd.GetIndexInOwnerCollection(), GetTextRange(text2));
			}
			else
			{
				WTextRange textRange = GetTextRange(text);
				int count = textRange.RevisionsInternal.Count;
				base.OwnerParagraph.Items.Insert(FieldEnd.GetIndexInOwnerCollection(), textRange);
				if (textRange.RevisionsInternal.Count - count > 0 && base.IsInsertRevision)
				{
					for (int i = 0; i < textRange.RevisionsInternal.Count; i++)
					{
						if (textRange.RevisionsInternal[i].Range.Items.IndexOf(textRange) == -1)
						{
							continue;
						}
						textRange.RevisionsInternal[i].Range.Items.Remove(textRange);
						Revision revision = textRange.RevisionsInternal[i];
						foreach (object item in revision.Range.Items)
						{
							if (item is Entity { EntityType: EntityType.FieldMark } entity && (entity as WFieldMark).Type == FieldMarkType.FieldEnd && entity as WFieldMark == FieldEnd)
							{
								revision.Range.InnerList.Insert(revision.Range.InnerList.IndexOf(entity), textRange);
								break;
							}
						}
					}
				}
			}
		}
		else
		{
			string[] array = text.Split('\r');
			for (int j = 0; j < array.Length; j++)
			{
				WTextRange wTextRange = new WTextRange(base.Document);
				if (m_resultFormat != null)
				{
					wTextRange.CharacterFormat.ImportContainer(m_resultFormat);
					wTextRange.CharacterFormat.CopyProperties(m_resultFormat);
					m_resultFormat = null;
				}
				else
				{
					WCharacterFormat format = ((FieldType == FieldType.FieldDate || FieldType == FieldType.FieldTime) ? GetFirstFieldCodeItem().CharacterFormat : base.CharacterFormat);
					wTextRange.CharacterFormat.ImportContainer(format);
					wTextRange.CharacterFormat.CopyProperties(format);
				}
				if (wTextRange.CharacterFormat.HasValue(106))
				{
					wTextRange.CharacterFormat.PropertiesHash.Remove(106);
				}
				wTextRange.Text = array[j];
				if (j == 0)
				{
					FieldSeparator.OwnerParagraph.Items.Add(wTextRange);
					continue;
				}
				if (j == array.Length - 1)
				{
					FieldEnd.OwnerParagraph.Items.Insert(FieldEnd.GetIndexInOwnerCollection(), wTextRange);
					continue;
				}
				WParagraph wParagraph = FieldEnd.OwnerParagraph.Clone() as WParagraph;
				wParagraph.ClearItems();
				int indexInOwnerCollection = FieldEnd.OwnerParagraph.GetIndexInOwnerCollection();
				FieldEnd.OwnerParagraph.OwnerTextBody.Items.Insert(indexInOwnerCollection, wParagraph);
				wParagraph.Items.Add(wTextRange);
			}
		}
		IsFieldRangeUpdated = false;
	}

	protected WTextRange GetTextRange(string text)
	{
		WTextRange wTextRange = new WTextRange(base.Document);
		if (m_resultFormat != null)
		{
			wTextRange.CharacterFormat.ImportContainer(m_resultFormat);
			wTextRange.CharacterFormat.CopyProperties(m_resultFormat);
			m_resultFormat = null;
		}
		else
		{
			WCharacterFormat format = ((FieldType == FieldType.FieldDate || FieldType == FieldType.FieldTime) ? GetFirstFieldCodeItem().CharacterFormat : base.CharacterFormat);
			wTextRange.CharacterFormat.ImportContainer(format);
			wTextRange.CharacterFormat.CopyProperties(format);
		}
		if (wTextRange.CharacterFormat.HasValue(106))
		{
			wTextRange.CharacterFormat.PropertiesHash.Remove(106);
		}
		wTextRange.Text = text;
		return wTextRange;
	}

	internal void SkipLayoutingOfFieldCode()
	{
		SplitTextRangeByParagraphBreak();
		IsFieldSeparator = false;
		for (int i = 0; i < Range.Items.Count; i++)
		{
			Entity entity = Range.Items[i] as Entity;
			if (entity is ParagraphItem)
			{
				if (entity is WField wField && DocumentLayouter.IsLayoutingHeaderFooter)
				{
					if (FieldType == FieldType.FieldIf || FieldType == FieldType.FieldCompare || FieldType == FieldType.FieldFormula)
					{
						if (wField.FieldType == FieldType.FieldNumPages)
						{
							IsNumPagesInsideExpressionField = true;
							wField.IsNumPageUsedForEvaluation = true;
						}
						else if (wField.FieldType == FieldType.FieldPage)
						{
							HasInnerPageField = true;
							wField.IsNumPageUsedForEvaluation = true;
						}
						wField.IsNestedField = true;
					}
					else if (FieldType == FieldType.FieldUnknown)
					{
						wField.IsFieldInsideUnknownField = true;
					}
				}
				SkipLayoutingOfParagraphItem(entity);
			}
			else
			{
				SkipLayoutingOfTextBodyItem(entity);
			}
			if (IsFieldSeparator && FieldType != FieldType.FieldExpression)
			{
				break;
			}
			if (entity is ParagraphItem && base.Owner is WParagraph && (base.Owner as WParagraph).LastItem == entity && entity != FieldEnd && ((IWidget)(base.Owner as WParagraph).LastItem).LayoutInfo.IsSkip && !(base.Owner as IWidget).LayoutInfo.IsSkip)
			{
				bool flag = !IsNumPagesInsideExpressionField && DocumentLayouter.IsLayoutingHeaderFooter && (FieldType == FieldType.FieldIf || FieldType == FieldType.FieldCompare || FieldType == FieldType.FieldFormula);
				int indexInOwnerCollection = GetIndexInOwnerCollection();
				if (indexInOwnerCollection == 0)
				{
					if (flag)
					{
						(base.Owner as IWidget).LayoutInfo.IsSkip = true;
					}
					else
					{
						(base.Owner as WParagraph).IsNeedToSkip = true;
					}
					continue;
				}
				bool flag2 = true;
				for (int j = 0; j < indexInOwnerCollection; j++)
				{
					if (!((IWidget)base.OwnerParagraph.Items[j]).LayoutInfo.IsSkip)
					{
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					if (flag)
					{
						(base.Owner as IWidget).LayoutInfo.IsSkip = true;
					}
					else
					{
						(base.Owner as WParagraph).IsNeedToSkip = true;
					}
				}
			}
			else if (entity is ParagraphItem && !(entity is WFieldMark) && FieldType == FieldType.FieldExpression)
			{
				(entity as IWidget).LayoutInfo.IsSkip = true;
			}
		}
		IsFieldSeparator = false;
	}

	internal void SplitTextRangeByParagraphBreak()
	{
		for (int i = 0; i < Range.Items.Count; i++)
		{
			Entity entity = Range.Items[i] as Entity;
			SplitTextRange(entity);
		}
	}

	private void SplitTextRange(Entity entity)
	{
		if (entity is ParagraphItem)
		{
			if (entity.EntityType == EntityType.TextRange)
			{
				WTextRange wTextRange = entity as WTextRange;
				if (wTextRange.Text.Replace(ControlChar.CrLf, ControlChar.ParagraphBreak).Replace(ControlChar.LineFeedChar, '\r').Contains(ControlChar.ParagraphBreak))
				{
					wTextRange.SplitWidgets();
					IsFieldRangeUpdated = false;
				}
			}
		}
		else if (entity is WParagraph)
		{
			for (int i = 0; i < (entity as WParagraph).Items.Count; i++)
			{
				SplitTextRange((entity as WParagraph).Items[i]);
			}
		}
		else
		{
			if (!(entity is WTable))
			{
				return;
			}
			for (int j = 0; j < (entity as WTable).Rows.Count; j++)
			{
				WTableRow wTableRow = (entity as WTable).Rows[j];
				for (int k = 0; k < wTableRow.Cells.Count; k++)
				{
					WTableCell wTableCell = wTableRow.Cells[k];
					for (int l = 0; l < wTableCell.Items.Count; l++)
					{
						SplitTextRange(wTableCell.Items[l]);
					}
				}
			}
		}
	}

	private void SkipMacroButtonFieldCode()
	{
		string text = string.Empty;
		string value = "MACROBUTTON";
		for (int i = 0; i < Range.Items.Count; i++)
		{
			Entity entity = Range.Items[i] as Entity;
			if (!(entity is WTextRange))
			{
				continue;
			}
			text += (entity as WTextRange).Text;
			if (text.ToUpper().Contains(value))
			{
				string text2 = SplitByWhiteSpace(text);
				if (!string.IsNullOrEmpty(text2))
				{
					if (text2.Trim().StartsWith("\\*"))
					{
						(entity as IWidget).LayoutInfo.IsSkip = true;
					}
					else if (!((entity as WTextRange).Text.Trim() == text2))
					{
						WTextRange wTextRange = new WTextRange(m_doc);
						wTextRange.ApplyCharacterFormat(base.CharacterFormat);
						wTextRange.Text = text2;
						(entity as IWidget).LayoutInfo.IsSkip = true;
						base.OwnerParagraph.Items.Insert(Index + 1, wTextRange);
					}
					break;
				}
				(entity as IWidget).LayoutInfo.IsSkip = true;
			}
			else
			{
				(entity as IWidget).LayoutInfo.IsSkip = true;
			}
		}
	}

	private string SplitByWhiteSpace(string macroText)
	{
		string text = string.Empty;
		bool flag = false;
		macroText = macroText.TrimStart();
		for (int i = 0; i < macroText.Length; i++)
		{
			if (macroText[i].Equals(ControlChar.SpaceChar))
			{
				if (text.Length != 0)
				{
					if (flag)
					{
						flag = false;
						macroText = macroText.Remove(0, i + 1);
						break;
					}
					if (text.ToUpper() == "MACROBUTTON")
					{
						flag = true;
					}
					text = string.Empty;
				}
			}
			else
			{
				text += macroText[i];
			}
		}
		if (!string.IsNullOrEmpty(macroText) && (macroText.Trim().ToUpper() == "MACROBUTTON" || flag))
		{
			return string.Empty;
		}
		return macroText;
	}

	private void SkipLayoutingOfParagraphItem(Entity entity)
	{
		if (entity is InlineContentControl inlineContentControl)
		{
			for (int i = 0; i <= inlineContentControl.ParagraphItems.Count - 1; i++)
			{
				ParagraphItem entity2 = inlineContentControl.ParagraphItems[i];
				SkipLayoutingOfParagraphItem(entity2);
			}
		}
		else
		{
			if (IsFieldSeparator)
			{
				return;
			}
			if (FieldSeparator == entity || FieldEnd == entity)
			{
				IsFieldSeparator = true;
			}
			else if (!(entity is WField) || (((WField)entity).FieldType != FieldType.FieldPage && ((WField)entity).FieldType != FieldType.FieldNumPages && ((WField)entity).FieldType != FieldType.FieldSectionPages && ((WField)entity).FieldType != FieldType.FieldPageRef) || (!DocumentLayouter.m_UpdatingPageFields && !DocumentLayouter.IsLayoutingHeaderFooter))
			{
				if (entity is IWidget widget)
				{
					widget.LayoutInfo.IsSkip = true;
				}
				if (entity is WField && ((entity as WField).FieldType == FieldType.FieldIf || (entity as WField).FieldType == FieldType.FieldCompare || (entity as WField).FieldType == FieldType.FieldFormula) && DocumentLayouter.IsLayoutingHeaderFooter)
				{
					IsFieldRangeUpdated = false;
				}
			}
		}
	}

	private void SkipLayoutingOfTextBodyItem(Entity entity)
	{
		if (entity is WParagraph)
		{
			if ((entity as WParagraph).ChildEntities.Contains(FieldSeparator) || (entity as WParagraph).ChildEntities.Contains(FieldEnd))
			{
				for (int i = 0; i < (entity as WParagraph).Items.Count; i++)
				{
					SkipLayoutingOfParagraphItem((entity as WParagraph).Items[i]);
					if (IsFieldSeparator)
					{
						break;
					}
				}
				return;
			}
			if (DocumentLayouter.IsLayoutingHeaderFooter)
			{
				WParagraph wParagraph = entity as WParagraph;
				for (int j = 0; j < wParagraph.Items.Count; j++)
				{
					Entity entity2 = wParagraph.Items[j];
					if (entity2 is WField && ((entity2 as WField).FieldType == FieldType.FieldIf || (entity2 as WField).FieldType == FieldType.FieldCompare || (entity2 as WField).FieldType == FieldType.FieldFormula))
					{
						(entity2 as WField).IsNestedField = true;
					}
				}
			}
			(entity as IWidget).LayoutInfo.IsSkip = true;
		}
		else if (entity is WTable)
		{
			SkipLayoutingOfTable(entity);
		}
	}

	private void SkipLayoutingOfTable(Entity entity)
	{
		for (int i = 0; i < (entity as WTable).Rows.Count; i++)
		{
			WTableRow wTableRow = (entity as WTable).Rows[i];
			for (int j = 0; j < wTableRow.Cells.Count; j++)
			{
				WTableCell wTableCell = wTableRow.Cells[j];
				for (int k = 0; k < wTableCell.Items.Count; k++)
				{
					SkipLayoutingOfTextBodyItem(wTableCell.Items[k]);
					if (IsFieldSeparator)
					{
						return;
					}
				}
			}
		}
		if (!IsFieldSeparator)
		{
			(entity as IWidget).LayoutInfo.IsSkip = true;
		}
	}

	private bool SetSkip(int startIndex, EntityCollection items, bool isFieldCode, int itemsCount)
	{
		IEntity entity = (isFieldCode ? FieldSeparator : FieldEnd);
		for (int i = startIndex; i < itemsCount; i++)
		{
			Entity entity2 = items[i];
			if (entity2 is InlineContentControl && entity2 == entity.Owner)
			{
				return SetSkip(0, (entity2 as InlineContentControl).ParagraphItems, isFieldCode, (entity2 as InlineContentControl).ParagraphItems.Count);
			}
			if (entity2 is WParagraph && entity2 == (entity as ParagraphItem).OwnerParagraph)
			{
				return SetSkip(0, (entity2 as WParagraph).Items, isFieldCode, (entity2 as WParagraph).Items.Count);
			}
			if (entity2 is BlockContentControl)
			{
				if (SetSkip(0, (entity2 as BlockContentControl).TextBody.Items, isFieldCode, (entity2 as BlockContentControl).TextBody.Items.Count))
				{
					return true;
				}
				continue;
			}
			(entity2 as IWidget).LayoutInfo.IsSkip = true;
			if (entity2 == entity)
			{
				return true;
			}
		}
		return false;
	}

	private IEntity GetNextSibling()
	{
		if (base.NextSibling != null)
		{
			return base.NextSibling;
		}
		if (base.Owner is InlineContentControl && base.Owner.NextSibling != null)
		{
			if (base.Owner.NextSibling is InlineContentControl && (base.Owner.NextSibling as InlineContentControl).ParagraphItems.Count > 0)
			{
				return (base.Owner.NextSibling as InlineContentControl).ParagraphItems[0];
			}
			return base.Owner.NextSibling;
		}
		return null;
	}

	internal void SkipLayoutingFieldItems(bool isFieldCode)
	{
		IEntity entity;
		if (!isFieldCode)
		{
			IEntity fieldSeparator = FieldSeparator;
			entity = fieldSeparator;
		}
		else
		{
			entity = GetNextSibling();
		}
		Entity entity2 = entity as Entity;
		int startIndex = entity2.Index;
		if (entity2.Owner is InlineContentControl)
		{
			if (SetSkip(startIndex, (entity2.Owner as InlineContentControl).ParagraphItems, isFieldCode, (entity2.Owner as InlineContentControl).ParagraphItems.Count))
			{
				return;
			}
			startIndex = entity2.Owner.Index + 1;
		}
		WParagraph ownerParagraph = (entity2 as ParagraphItem).OwnerParagraph;
		if (SetSkip(startIndex, ownerParagraph.Items, isFieldCode, ownerParagraph.Items.Count))
		{
			return;
		}
		WTextBody ownerTextBody = ownerParagraph.OwnerTextBody;
		int itemsCount = ownerTextBody.Items.Count;
		if (FieldType == FieldType.FieldUnknown && FieldEnd != null && FieldEnd.OwnerParagraph.Owner is WTableCell && (!(base.OwnerParagraph.Owner is WTableCell) || FieldEnd.OwnerParagraph.Owner != base.OwnerParagraph.Owner))
		{
			WTable wTable = GetOwnerTable(FieldEnd) as WTable;
			while (wTable.IsInCell && GetOwnerTable(wTable) is WTable wTable2)
			{
				wTable = wTable2;
			}
			if (ownerTextBody.Items.Contains(wTable))
			{
				itemsCount = wTable.Index;
			}
			SkipTableItems(wTable, FieldEnd.OwnerParagraph.Owner as WTableCell);
		}
		SetSkip(ownerParagraph.Index + 1, ownerTextBody.Items, isFieldCode, itemsCount);
	}

	private bool SkipTableItems(WTable table, WTableCell wTableCell)
	{
		foreach (WTableRow row in table.Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				bool flag = false;
				bool flag2 = false;
				foreach (Entity childEntity in cell.ChildEntities)
				{
					if (!flag && childEntity is WParagraph)
					{
						flag = true;
					}
					else if (childEntity is WParagraph)
					{
						flag2 = (childEntity as WParagraph).Items.Contains(FieldEnd);
						if (flag2)
						{
							SkipParaItems((childEntity as WParagraph).Items);
						}
						else
						{
							(childEntity as IWidget).LayoutInfo.IsSkip = true;
						}
					}
					else if (childEntity is WTable)
					{
						flag2 = SkipTableItems(childEntity as WTable, cell);
					}
				}
				if (flag2)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void SkipParaItems(ParagraphItemCollection items)
	{
		foreach (Entity item in items)
		{
			if (item != FieldEnd)
			{
				(item as IWidget).LayoutInfo.IsSkip = true;
			}
		}
	}

	SizeF ILeafWidget.Measure(DrawingContext dc)
	{
		SizeF sizeF = SizeF.Empty;
		string text = "";
		WCharacterFormat charFormat = GetCharFormat();
		DocGen.Drawing.Font fontToRender = charFormat.GetFontToRender(base.ScriptType);
		switch (FieldType)
		{
		case FieldType.FieldExpression:
			sizeF = GetEQFieldSize(base.ScriptType, dc, charFormat);
			((IWidget)this).LayoutInfo.Size = sizeF;
			break;
		case FieldType.FieldDocVariable:
			if (FieldSeparator == null)
			{
				text = base.Document.Variables[FieldValue];
				sizeF = dc.MeasureString(text, fontToRender, null, charFormat, isMeasureFromTabList: false, base.ScriptType);
				((IWidget)this).LayoutInfo.Size = sizeF;
			}
			else
			{
				((IWidget)this).LayoutInfo.IsSkip = true;
			}
			break;
		case FieldType.FieldNumPages:
		{
			string text2 = string.Empty;
			charFormat = GetCharacterFormatValue();
			if (DocumentLayouter.IsFirstLayouting)
			{
				Text = "";
				WTextRange currentTextRange = GetCurrentTextRange();
				if (currentTextRange != null && !(currentTextRange is WField) && currentTextRange.m_layoutInfo == null)
				{
					_ = ((IWidget)currentTextRange).LayoutInfo;
				}
			}
			else
			{
				UpdateNumberFormatResult(FieldResult, skipFieldResultPartUpdate: true);
				text2 = FieldResult;
			}
			sizeF = dc.MeasureString(text2, charFormat.GetFontToRender(base.ScriptType), null, charFormat, isMeasureFromTabList: false, base.ScriptType);
			if (text2 != string.Empty)
			{
				((IWidget)this).LayoutInfo.Size = sizeF;
			}
			break;
		}
		}
		return sizeF;
	}

	private SizeF GetEQFieldSize(FontScriptType fontScriptType, DrawingContext dc, WCharacterFormat charFormat)
	{
		EquationField equationField = new EquationField();
		equationField.EQFieldEntity = this;
		equationField.LayouttedEQField = LayoutEQfileCode(fontScriptType, FieldCode, dc, charFormat, 0f, 0f);
		DocumentLayouter.EquationFields.Add(equationField);
		return equationField.LayouttedEQField.Bounds.Size;
	}

	private LayoutedEQFields LayoutEQfileCode(FontScriptType fontScriptType, string fieldCode, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		fieldCode = RemoveEQText(fieldCode);
		List<string> list = new List<string>();
		LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
		if (IsValidEqFieldCode(list, fieldCode))
		{
			LayoutEquationFieldCode(fontScriptType, layoutedEQFields, list, dc, charFormat, xPosition, yPosition);
			UpdateltEqFieldsBounds(layoutedEQFields);
		}
		else
		{
			dc.GenerateErrorFieldCode(layoutedEQFields, xPosition, yPosition, charFormat);
		}
		return layoutedEQFields;
	}

	private bool IsValidEqFieldCode(List<string> splittedFieldCodes, string fieldCode)
	{
		GetSplittedFieldCode(splittedFieldCodes, fieldCode);
		foreach (string splittedFieldCode in splittedFieldCodes)
		{
			if (!IsValidSwitch(splittedFieldCode))
			{
				return false;
			}
		}
		return true;
	}

	private bool IsValidSwitch(string fieldCode)
	{
		switch (GetFirstOccurenceEqSwitch(fieldCode).ToLower())
		{
		case "\\a":
			return IsValidArraySwitch(fieldCode);
		case "\\f":
			return IsValidFractionSwitch(fieldCode);
		case "\\s":
			return IsValidSuperscriptSwitch(fieldCode);
		case "\\l":
			return IsValidListSwitch(fieldCode);
		case "\\r":
			return IsValidRadicalSwitch(fieldCode);
		case "\\i":
			return IsValidIntegralSwitch(fieldCode);
		case "\\b":
			return IsValidBracketSwitch(fieldCode);
		case "\\x":
			return IsValidBoxSwitch(fieldCode);
		case "\\o":
			return IsValidOverstrikeSwitch(fieldCode);
		case "\\d":
			return IsValidDisplaceSwitch(fieldCode);
		default:
			if (HasSlashError(fieldCode))
			{
				return false;
			}
			return !fieldCode.Contains("\\ ");
		}
	}

	private LayoutedEQFields LayoutSwitch(FontScriptType scriptType, string fieldCode, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		switch (GetFirstOccurenceEqSwitch(fieldCode).ToLower())
		{
		case "\\a":
			return LayoutArraySwitch(scriptType, fieldCode, dc, charFormat, xPosition, yPosition);
		case "\\f":
			return LayoutFractionSwitch(scriptType, fieldCode, dc, charFormat, xPosition, yPosition);
		case "\\s":
			return LayoutSuperscriptSwitch(scriptType, fieldCode, dc, charFormat, xPosition, yPosition);
		case "\\l":
			return LayoutListSwitch(scriptType, fieldCode, dc, charFormat, xPosition, yPosition);
		case "\\r":
			return LayoutRadicalSwitch(scriptType, fieldCode, dc, charFormat, xPosition, yPosition);
		case "\\b":
			return LayoutBracketSwitch(scriptType, fieldCode, dc, charFormat, xPosition, yPosition);
		case "\\x":
			return LayoutBoxSwitch(scriptType, fieldCode, dc, charFormat, xPosition, yPosition);
		case "\\i":
			return LayoutIntegralSwitch(scriptType, fieldCode, dc, charFormat, xPosition, yPosition);
		case "\\o":
			return LayoutOverstrikeSwitch(scriptType, fieldCode, dc, charFormat, xPosition, yPosition);
		case "\\d":
			return LayoutDisplaceSwitch(scriptType, fieldCode, dc, charFormat, xPosition, yPosition);
		default:
			if (fieldCode != "")
			{
				TextEQField textEQField = new TextEQField();
				GenerateTextEQField(textEQField, fieldCode, dc, charFormat.GetFontToRender(scriptType), charFormat, xPosition, yPosition);
				return textEQField;
			}
			return null;
		}
	}

	private void GenerateSwitch(FontScriptType fontScriptType, LayoutedEQFields ltEqField, string fieldCode, DrawingContext dc, DocGen.Drawing.Font font, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		if (HasAnySwitch(fieldCode))
		{
			GenerateNestedSwitch(fontScriptType, ltEqField, fieldCode, dc, charFormat, xPosition, yPosition);
			return;
		}
		TextEQField textEQField = new TextEQField();
		GenerateTextEQField(textEQField, fieldCode, dc, font, charFormat, xPosition, yPosition);
		ltEqField.ChildEQFileds.Add(textEQField);
		UpdateltEqFieldsBounds(ltEqField);
	}

	private void GenerateNestedSwitch(FontScriptType fontScriptType, LayoutedEQFields ltEqField, string fieldCode, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
		List<string> splittedFieldCodeSwitch = new List<string>();
		GetSplittedFieldCode(splittedFieldCodeSwitch, fieldCode);
		LayoutEquationFieldCode(fontScriptType, layoutedEQFields, splittedFieldCodeSwitch, dc, charFormat, xPosition, yPosition);
		ltEqField.ChildEQFileds.Add(layoutedEQFields);
		UpdateltEqFieldsBounds(ltEqField);
	}

	private void GenerateTextEQField(TextEQField textEqField, string fieldCode, DrawingContext dc, DocGen.Drawing.Font font, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		textEqField.Text = ReplaceSymbols(fieldCode);
		string text = textEqField.Text;
		if (fieldCode == "Õ".ToString() || fieldCode == "å")
		{
			textEqField.Font = font;
		}
		textEqField.Bounds = new RectangleF(new PointF(xPosition, yPosition), dc.MeasureString(text, font, null, charFormat, isMeasureFromTabList: false, FontScriptType.English));
		float ascent = dc.GetAscent(font, FontScriptType.English);
		dc.ShiftEqFieldYPosition(textEqField, 0f - ascent);
	}

	private LayoutedEQFields LayoutRadicalSwitch(FontScriptType fontScriptType, string fieldCode, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		string element = fieldCode.Substring(fieldCode.IndexOf("\\r", StringComparison.CurrentCultureIgnoreCase) + 2);
		string[] elements = SplitElementsByComma(element);
		LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
		layoutedEQFields.SwitchType = LayoutedEQFields.EQSwitchType.Radical;
		GenerateRadicalSwitch(fontScriptType, layoutedEQFields, elements, dc, charFormat, xPosition, yPosition);
		return layoutedEQFields;
	}

	private void GenerateRadicalSwitch(FontScriptType scriptType, LayoutedEQFields radicalSwitch, string[] elements, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		float outerElementRight = 0f;
		float outerElementY = 0f;
		if (elements.Length == 1)
		{
			GenerateSwitch(scriptType, radicalSwitch, elements[0], dc, charFormat.GetFontToRender(scriptType), charFormat, xPosition, yPosition);
			GenerateRadicalSymbol(radicalSwitch, radicalSwitch.ChildEQFileds[0], xPosition, ref outerElementRight, ref outerElementY, charFormat.FontSize);
			radicalSwitch.Bounds = new RectangleF(radicalSwitch.Bounds.X, radicalSwitch.Bounds.Y - 0.7f, radicalSwitch.Bounds.Width, radicalSwitch.Bounds.Height + 0.7f);
		}
		else
		{
			GenerateSwitch(scriptType, radicalSwitch, elements[1], dc, charFormat.GetFontToRender(scriptType), charFormat, xPosition, yPosition);
			RectangleF bounds = radicalSwitch.ChildEQFileds[radicalSwitch.ChildEQFileds.Count - 1].Bounds;
			GenerateRadicalSymbol(radicalSwitch, radicalSwitch.ChildEQFileds[radicalSwitch.ChildEQFileds.Count - 1], xPosition, ref outerElementRight, ref outerElementY, charFormat.FontSize);
			GenerateRadicalOuterElement(scriptType, radicalSwitch, elements[0], dc, charFormat, xPosition, yPosition, outerElementY, outerElementRight, bounds);
		}
	}

	private void GenerateRadicalOuterElement(FontScriptType fontScriptType, LayoutedEQFields radicalSwitch, string element, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition, float outerELementY, float outerElementRight, RectangleF innerElementBounds)
	{
		LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
		GenerateSwitch(fontScriptType, layoutedEQFields, element, dc, charFormat.GetFontToRender(fontScriptType), charFormat, xPosition, yPosition);
		float num = (float)dc.FontMetric.Descent(charFormat.GetFontToRender(fontScriptType), fontScriptType);
		if (innerElementBounds.Height <= layoutedEQFields.Bounds.Height)
		{
			num = layoutedEQFields.Bounds.Bottom;
		}
		dc.ShiftEqFieldYPosition(layoutedEQFields, outerELementY);
		outerElementRight -= layoutedEQFields.Bounds.Width;
		ShiftEqFieldXPosition(layoutedEQFields, outerElementRight - layoutedEQFields.Bounds.X);
		radicalSwitch.ChildEQFileds.Add(layoutedEQFields);
		float num2 = ((radicalSwitch.Bounds.Top < layoutedEQFields.Bounds.Top) ? radicalSwitch.Bounds.Height : (radicalSwitch.Bounds.Height + (radicalSwitch.Bounds.Y - layoutedEQFields.Bounds.Y)));
		UpdateRadicalBounds(radicalSwitch);
		if (radicalSwitch.Bounds.X < xPosition)
		{
			ShiftEqFieldXPosition(radicalSwitch, xPosition - radicalSwitch.Bounds.X);
		}
		ShiftRadicalSymbolYPosition(radicalSwitch, charFormat.GetFontToRender(fontScriptType).Size / 24f);
		radicalSwitch.Bounds = new RectangleF(radicalSwitch.Bounds.X, radicalSwitch.Bounds.Y - num, radicalSwitch.Bounds.Width, num2 + num);
	}

	private void ShiftRadicalSymbolYPosition(LayoutedEQFields ltEQField, float shiftPosition)
	{
		for (int i = 0; i < ltEQField.ChildEQFileds.Count; i++)
		{
			if (ltEQField.ChildEQFileds[i] is LineEQField)
			{
				LineEQField lineEQField = ltEQField.ChildEQFileds[i] as LineEQField;
				lineEQField.Point1 = new PointF(lineEQField.Point1.X, lineEQField.Point1.Y + shiftPosition);
				lineEQField.Point2 = new PointF(lineEQField.Point2.X, lineEQField.Point2.Y + shiftPosition);
			}
		}
	}

	private void UpdateRadicalBounds(LayoutedEQFields ltEQField)
	{
		float num = GetLeftMostX(ltEQField);
		if (num > 0f)
		{
			num = 0f;
		}
		float num2 = 0f - GetTopMostY(ltEQField);
		float maximumBottom = GetMaximumBottom(ltEQField);
		ltEQField.Bounds = new RectangleF(num, 0f - num2, 0f - num + GetMaximumRight(ltEQField), num2 + maximumBottom);
	}

	private void GenerateRadicalSymbol(LayoutedEQFields radicalSwitch, LayoutedEQFields innerElement, float xPosition, ref float outerElementRight, ref float outerElementY, float fontSize)
	{
		float lineThickness = 0.7f;
		float height = 0f;
		LineEQField lineEQField = new LineEQField();
		GenerateUpwardLine(innerElement, lineEQField, lineThickness);
		LineEQField lineEQField2 = new LineEQField();
		PointF downwardStart = new PointF(xPosition, 0f);
		GenerateRadicalDownwardLine(innerElement, lineEQField, lineEQField2, lineThickness, ref downwardStart, ref height);
		LineEQField lineEQField3 = new LineEQField();
		GenerateRadicalTopHorizontalLine(innerElement, lineEQField, lineEQField3, lineThickness);
		LineEQField lineEQField4 = new LineEQField();
		GenerateRadicalHook(innerElement, lineEQField4, lineThickness, ref height, ref downwardStart);
		radicalSwitch.ChildEQFileds.Add(lineEQField4);
		radicalSwitch.ChildEQFileds.Add(lineEQField2);
		radicalSwitch.ChildEQFileds.Add(lineEQField);
		radicalSwitch.ChildEQFileds.Add(lineEQField3);
		UpdateRadicalSwitchBounds(radicalSwitch, innerElement, lineEQField4, lineEQField, lineEQField3, xPosition, lineThickness);
		GetOuterElementPositions(lineEQField4, lineEQField, lineEQField3, lineThickness, ref outerElementRight, ref outerElementY, fontSize);
	}

	private void UpdateRadicalSwitchBounds(LayoutedEQFields radicalSwitch, LayoutedEQFields innerElement, LineEQField rootHook, LineEQField upwardLine, LineEQField topHorizontalLine, float xPosition, float lineThickness)
	{
		ShiftEqFieldXPosition(innerElement, lineThickness);
		float num = rootHook.Point1.X - lineThickness / 4f;
		ShiftEqFieldXPosition(radicalSwitch, xPosition - num);
		num = xPosition;
		float num2 = topHorizontalLine.Point1.Y - lineThickness / 2f;
		float height = upwardLine.Point1.Y + (0f - num2) + lineThickness / 2f;
		float width = innerElement.Bounds.Right - num;
		radicalSwitch.Bounds = new RectangleF(num, num2, width, height);
	}

	private void GenerateUpwardLine(LayoutedEQFields innerElement, LineEQField upwardLine, float lineThickness)
	{
		float widthFromAngle = GetWidthFromAngle(innerElement.Bounds.Height, DegreeIntoRadians(80.3856f), DegreeIntoRadians(9.6143f));
		float x = innerElement.Bounds.Left - widthFromAngle - lineThickness / 2f;
		float y = innerElement.Bounds.Bottom + lineThickness / 2f;
		upwardLine.Point1 = new PointF(x, y);
		x = innerElement.Bounds.Left - lineThickness / 2f;
		y = innerElement.Bounds.Top + lineThickness / 4f;
		upwardLine.Point2 = new PointF(x, y);
	}

	private void GenerateRadicalDownwardLine(LayoutedEQFields innerElement, LineEQField upwardLine, LineEQField downwardLine, float lineThickness, ref PointF downwardStart, ref float height)
	{
		height = innerElement.Bounds.Height * 0.558f;
		float widthFromAngle = GetWidthFromAngle(height, DegreeIntoRadians(64.3483f), DegreeIntoRadians(25.6516f));
		float num = upwardLine.Point1.X - widthFromAngle;
		float num2 = innerElement.Bounds.Bottom - innerElement.Bounds.Height * 0.558f;
		downwardStart = new PointF(num, num2);
		downwardLine.Point1 = new PointF(num - lineThickness / 4f, num2 + lineThickness / 4f);
		downwardLine.Point2 = new PointF(upwardLine.Point1.X, upwardLine.Point1.Y);
	}

	private void GenerateRadicalTopHorizontalLine(LayoutedEQFields innerElement, LineEQField upwardLine, LineEQField topHorizontalLine, float lineThickness)
	{
		float y = innerElement.Bounds.Top + lineThickness / 4f;
		topHorizontalLine.Point1 = new PointF(upwardLine.Point2.X, y);
		float x = topHorizontalLine.Point1.X + innerElement.Bounds.Width;
		topHorizontalLine.Point2 = new PointF(x, topHorizontalLine.Point1.Y);
	}

	private void GenerateRadicalHook(LayoutedEQFields innerElement, LineEQField rootHook, float lineThickness, ref float height, ref PointF downwardStart)
	{
		height -= innerElement.Bounds.Height * 0.485f;
		float widthFromAngle = GetWidthFromAngle(height, DegreeIntoRadians(32.2825f), DegreeIntoRadians(57.7174f));
		float x = downwardStart.X - widthFromAngle;
		float y = innerElement.Bounds.Bottom - innerElement.Bounds.Height * 0.485f;
		rootHook.Point1 = new PointF(x, y);
		x = downwardStart.X - lineThickness / 4f;
		y = downwardStart.Y + lineThickness / 4f;
		rootHook.Point2 = new PointF(x, y);
	}

	private void GetOuterElementPositions(LineEQField rootHook, LineEQField upwardLine, LineEQField topHorizontalLine, float lineThickness, ref float outerElementRight, ref float outerElementY, float fontSize)
	{
		float num = ((fontSize > 24f) ? 0.5f : 0.6121f);
		float num2 = (upwardLine.Point2.X - (rootHook.Point1.X - lineThickness / 4f)) * num;
		outerElementRight = rootHook.Point1.X + num2;
		lineThickness *= 1.5f;
		float num3 = 0f - topHorizontalLine.Point1.Y - (0f - rootHook.Point1.Y) + lineThickness;
		outerElementY = rootHook.Point1.Y - num3 / 2f;
	}

	private float GetWidthFromAngle(float height, double angle1, double angle2)
	{
		return (float)((double)height / Math.Sin(angle1) * Math.Sin(angle2));
	}

	private double DegreeIntoRadians(float angle)
	{
		return Math.PI * (double)angle / 180.0;
	}

	private bool IsValidRadicalSwitch(string fieldCode)
	{
		if (HasParenthesisError(fieldCode))
		{
			return false;
		}
		string text = fieldCode.Substring(fieldCode.IndexOf("\\r", StringComparison.CurrentCultureIgnoreCase) + 2);
		if (!StartsWithExt(text.TrimStart(), "("))
		{
			return false;
		}
		if (SplitElementsByComma(text).Length > 2)
		{
			return false;
		}
		List<string> splittedFieldCodes = new List<string>();
		if (!IsValidEqFieldCode(splittedFieldCodes, text))
		{
			return false;
		}
		return true;
	}

	private bool IsValidListSwitch(string fieldCode)
	{
		if (HasParenthesisError(fieldCode))
		{
			return false;
		}
		string text = fieldCode.Substring(fieldCode.IndexOf("\\l", StringComparison.CurrentCultureIgnoreCase) + 2);
		if (!StartsWithExt(text.TrimStart(), "("))
		{
			return false;
		}
		text = text.Substring(1, text.Length - 2);
		List<string> splittedFieldCodes = new List<string>();
		if (!IsValidEqFieldCode(splittedFieldCodes, text))
		{
			return false;
		}
		return true;
	}

	private LayoutedEQFields LayoutListSwitch(FontScriptType fontScriptType, string fieldCode, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
		layoutedEQFields.SwitchType = LayoutedEQFields.EQSwitchType.List;
		GenerateListSwitch(fontScriptType, layoutedEQFields, fieldCode, dc, charFormat, xPosition, yPosition);
		return layoutedEQFields;
	}

	private void GenerateListSwitch(FontScriptType fontScriptType, LayoutedEQFields listSwitch, string fieldCode, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		string text = fieldCode.Substring(fieldCode.IndexOf("\\l", StringComparison.CurrentCultureIgnoreCase) + 2);
		text = text.Substring(1, text.Length - 2);
		GenerateSwitch(fontScriptType, listSwitch, text, dc, charFormat.GetFontToRender(fontScriptType), charFormat, xPosition, yPosition);
	}

	private bool IsValidSuperscriptSwitch(string fieldCode)
	{
		if (HasParenthesisError(fieldCode))
		{
			return false;
		}
		string substringTill = GetSubstringTill(fieldCode, '(');
		string fieldCode2 = fieldCode.Substring(fieldCode.IndexOf(substringTill, StringComparison.Ordinal) + substringTill.Length);
		if (!IsCorrectSuperscriptSwitchSequence(substringTill))
		{
			return false;
		}
		List<string> splittedFieldCodes = new List<string>();
		if (!IsValidEqFieldCode(splittedFieldCodes, fieldCode2))
		{
			return false;
		}
		return true;
	}

	private bool IsCorrectSuperscriptSwitchSequence(string superscriptEqCode)
	{
		if (superscriptEqCode.Contains("\\ "))
		{
			return false;
		}
		string[] array = superscriptEqCode.Split('\\');
		for (int i = 1; i < array.Length; i++)
		{
			if (array[i].StartsWith("up", StringComparison.OrdinalIgnoreCase) || array[i].StartsWith("do", StringComparison.OrdinalIgnoreCase))
			{
				if (!IsCorrectCodeFormat(array[i].Trim(), isAlsoNegative: true))
				{
					return false;
				}
			}
			else if (array[i].StartsWith("ai", StringComparison.OrdinalIgnoreCase) || array[i].StartsWith("di", StringComparison.OrdinalIgnoreCase))
			{
				if (!IsCorrectCodeFormat(array[i].Trim(), isAlsoNegative: false))
				{
					return false;
				}
			}
			else if (!array[i].TrimEnd().Equals("s", StringComparison.OrdinalIgnoreCase) || i != 1)
			{
				return false;
			}
		}
		return true;
	}

	private bool HasPositiveValue(string inputText)
	{
		int num = inputText.IndexOfAny("0123456789".ToCharArray());
		if (num != -1 && inputText[num - 1] == '-')
		{
			return false;
		}
		return true;
	}

	private LayoutedEQFields LayoutSuperscriptSwitch(FontScriptType fontScriptType, string fieldCode, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		string substringTill = GetSubstringTill(fieldCode, '(');
		string element = fieldCode.Substring(fieldCode.IndexOf(substringTill, StringComparison.Ordinal) + substringTill.Length);
		string[] array = SplitElementsByComma(element);
		LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
		layoutedEQFields.SwitchType = LayoutedEQFields.EQSwitchType.Superscript;
		GenerateSuperscriptSwitch(fontScriptType, layoutedEQFields, array, dc, charFormat, xPosition, yPosition);
		if (array.Length == 1)
		{
			ApplySuperscriptProperties(layoutedEQFields, substringTill, dc);
		}
		return layoutedEQFields;
	}

	private void GenerateSuperscriptSwitch(FontScriptType fontScriptType, LayoutedEQFields scriptSwitch, string[] elements, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		if (elements.Length == 1)
		{
			GenerateSwitch(fontScriptType, scriptSwitch, elements[0], dc, charFormat.GetFontToRender(fontScriptType), charFormat, xPosition, yPosition);
			return;
		}
		foreach (string fieldCode in elements)
		{
			GenerateSwitch(fontScriptType, scriptSwitch, fieldCode, dc, charFormat.GetFontToRender(fontScriptType), charFormat, xPosition, yPosition);
		}
		AlignColumnWise(scriptSwitch, dc);
		UpdateltEqFieldsBounds(scriptSwitch);
	}

	private void AddSpaceBelowLine(LayoutedEQFields layoutedEqFields, int spaceBelowLineValue)
	{
		layoutedEqFields.Bounds = new RectangleF(layoutedEqFields.Bounds.X, layoutedEqFields.Bounds.Y, layoutedEqFields.Bounds.Width, layoutedEqFields.Bounds.Height + (float)spaceBelowLineValue);
	}

	private void AddSpaceAboveLine(LayoutedEQFields layoutedEqFields, int spaceAboveLineValue)
	{
		layoutedEqFields.Bounds = new RectangleF(layoutedEqFields.Bounds.X, layoutedEqFields.Bounds.Y - (float)spaceAboveLineValue, layoutedEqFields.Bounds.Width, layoutedEqFields.Bounds.Height + (float)spaceAboveLineValue);
	}

	private int GetSpaceBelowLineValue(string fieldCode)
	{
		string[] array = fieldCode.Split('\\');
		for (int num = array.Length - 1; num > 0; num--)
		{
			if (array[num].Trim().StartsWith("di", StringComparison.CurrentCultureIgnoreCase))
			{
				return GetValueFromstring(array[num].Trim());
			}
		}
		return 0;
	}

	private int GetSpaceAboveLineValue(string fieldCode)
	{
		string[] array = fieldCode.Split('\\');
		for (int num = array.Length - 1; num > 0; num--)
		{
			if (array[num].Trim().StartsWith("ai", StringComparison.CurrentCultureIgnoreCase))
			{
				return GetValueFromstring(array[num].Trim());
			}
		}
		return 0;
	}

	private void AlignColumnWise(LayoutedEQFields ltEqField, DrawingContext dc)
	{
		for (int i = 1; i < ltEqField.ChildEQFileds.Count; i++)
		{
			float yPosition = ltEqField.ChildEQFileds[i - 1].Bounds.Bottom - ltEqField.ChildEQFileds[i].Bounds.Y;
			dc.ShiftEqFieldYPosition(ltEqField.ChildEQFileds[i], yPosition);
		}
		UpdateltEqFieldsBounds(ltEqField);
		float num = ltEqField.Bounds.Y - ltEqField.Bounds.Height / 2f;
		dc.ShiftEqFieldYPosition(ltEqField, num - ltEqField.Bounds.Y);
	}

	private void ApplySuperscriptProperties(LayoutedEQFields scriptSwitch, string superscriptEqCode, DrawingContext dc)
	{
		int upValue = 2;
		int downValue = 2;
		bool isUpValue = true;
		bool isDownValue = false;
		GetsSuperOrSubscriptPropertiesValue(superscriptEqCode, ref upValue, ref downValue, ref isUpValue, ref isDownValue);
		int spaceAboveLineValue = GetSpaceAboveLineValue(superscriptEqCode);
		int spaceBelowLineValue = GetSpaceBelowLineValue(superscriptEqCode);
		if (isUpValue)
		{
			AlignAsSuperScriptSwitch(dc, scriptSwitch, upValue);
		}
		else if (downValue != 0)
		{
			AlignAsSubscriptSwitch(dc, scriptSwitch, downValue);
		}
		if (spaceAboveLineValue > 0)
		{
			AddSpaceAboveLine(scriptSwitch, spaceAboveLineValue);
		}
		if (spaceBelowLineValue > 0)
		{
			AddSpaceBelowLine(scriptSwitch, spaceBelowLineValue);
		}
	}

	private void AlignAsSuperScriptSwitch(DrawingContext dc, LayoutedEQFields scriptSwitch, int upValue)
	{
		LayoutedEQFields layoutedEQFields = scriptSwitch.ChildEQFileds[0];
		dc.ShiftEqFieldYPosition(layoutedEQFields, 0f - (float)upValue);
		if (upValue > 0)
		{
			scriptSwitch.Bounds = new RectangleF(scriptSwitch.Bounds.X, scriptSwitch.Bounds.Y - (float)upValue, scriptSwitch.Bounds.Width, scriptSwitch.Bounds.Height + (float)upValue);
			return;
		}
		scriptSwitch.Bounds = new RectangleF(scriptSwitch.Bounds.X, scriptSwitch.Bounds.Y, scriptSwitch.Bounds.Width, scriptSwitch.Bounds.Height - (float)upValue);
		scriptSwitch.SwitchType = LayoutedEQFields.EQSwitchType.Subscript;
	}

	private void AlignAsSubscriptSwitch(DrawingContext dc, LayoutedEQFields scriptSwitch, int downValue)
	{
		LayoutedEQFields layoutedEQFields = scriptSwitch.ChildEQFileds[0];
		dc.ShiftEqFieldYPosition(layoutedEQFields, downValue);
		if (downValue > 0)
		{
			scriptSwitch.Bounds = new RectangleF(scriptSwitch.Bounds.X, scriptSwitch.Bounds.Y, scriptSwitch.Bounds.Width, scriptSwitch.Bounds.Height + (float)downValue);
			scriptSwitch.SwitchType = LayoutedEQFields.EQSwitchType.Subscript;
		}
		else
		{
			scriptSwitch.Bounds = new RectangleF(scriptSwitch.Bounds.X, scriptSwitch.Bounds.Y + (float)downValue, scriptSwitch.Bounds.Width, scriptSwitch.Bounds.Height - (float)downValue);
			scriptSwitch.SwitchType = LayoutedEQFields.EQSwitchType.Superscript;
		}
	}

	private void GetsSuperOrSubscriptPropertiesValue(string fieldCode, ref int upValue, ref int downValue, ref bool isUpValue, ref bool isDownValue)
	{
		string[] array = fieldCode.Split('\\');
		for (int num = array.Length - 1; num > 0; num--)
		{
			if (array[num].Trim().StartsWith("up", StringComparison.CurrentCultureIgnoreCase))
			{
				upValue = GetValueFromstring(array[num].Trim());
				if (upValue == int.MinValue)
				{
					upValue = 2;
				}
				downValue = 0;
				break;
			}
			if (array[num].Trim().StartsWith("do", StringComparison.CurrentCultureIgnoreCase))
			{
				downValue = GetValueFromstring(array[num].Trim());
				if (downValue == int.MinValue)
				{
					downValue = 2;
				}
				upValue = 0;
				isDownValue = true;
				isUpValue = false;
				break;
			}
		}
	}

	private int GetValueFromstring(string inputText)
	{
		string text = "";
		for (int i = 0; i < inputText.Length; i++)
		{
			if (char.IsDigit(inputText[i]))
			{
				text += inputText[i];
			}
		}
		if (!HasPositiveValue(inputText))
		{
			return -((text != "") ? int.Parse(text) : int.MinValue);
		}
		if (!(text != ""))
		{
			return int.MinValue;
		}
		return int.Parse(text);
	}

	private float GetMaximumBottom(LayoutedEQFields ltEqFields)
	{
		float bottom = ltEqFields.Bounds.Bottom;
		for (int i = 0; i < ltEqFields.ChildEQFileds.Count; i++)
		{
			if (i == 0)
			{
				bottom = ltEqFields.ChildEQFileds[i].Bounds.Bottom;
			}
			else if (ltEqFields.ChildEQFileds[i].Bounds.Bottom >= bottom)
			{
				bottom = ltEqFields.ChildEQFileds[i].Bounds.Bottom;
			}
		}
		return bottom;
	}

	private LayoutedEQFields LayoutFractionSwitch(FontScriptType fontScriptType, string fieldCode, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		string element = fieldCode.Substring(fieldCode.IndexOf("\\f", StringComparison.CurrentCultureIgnoreCase) + 2);
		string[] elements = SplitElementsByComma(element);
		LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
		layoutedEQFields.SwitchType = LayoutedEQFields.EQSwitchType.Fraction;
		GenerateFractionSwitch(fontScriptType, layoutedEQFields, elements, dc, charFormat, xPosition, yPosition);
		return layoutedEQFields;
	}

	private void GenerateFractionSwitch(FontScriptType fontScriptType, LayoutedEQFields fraction, string[] elements, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		for (int i = 0; i <= elements.Length - 1; i++)
		{
			GenerateSwitch(fontScriptType, fraction, elements[i], dc, charFormat.GetFontToRender(fontScriptType), charFormat, xPosition, yPosition);
		}
		InsertFractionLine(fontScriptType, fraction, fraction.ChildEQFileds[0], fraction.ChildEQFileds[1], xPosition, yPosition, dc, charFormat);
	}

	private void InsertFractionLine(FontScriptType fontScriptType, LayoutedEQFields fraction, LayoutedEQFields numerator, LayoutedEQFields denominator, float xPosition, float yPosition, DrawingContext dc, WCharacterFormat charFormat)
	{
		LineEQField lineEQField = new LineEQField();
		lineEQField.Point1 = new PointF(xPosition, yPosition - 1f);
		SetXForFractionelements(lineEQField, numerator, denominator, xPosition, yPosition);
		SetYForFractionElements(numerator, denominator, dc, xPosition, yPosition);
		fraction.ChildEQFileds.Insert(1, lineEQField);
		float num = dc.MeasureString(" ", charFormat.GetFontToRender(fontScriptType), null, charFormat, isMeasureFromTabList: false, fontScriptType).Height - dc.GetAscent(charFormat.GetFontToRender(fontScriptType), fontScriptType);
		dc.ShiftEqFieldYPosition(fraction, 0f - num);
		UpdateltEqFieldsBounds(fraction);
	}

	private void SetXForFractionelements(LineEQField fractionLine, LayoutedEQFields numerator, LayoutedEQFields denominator, float xPosition, float yPosition)
	{
		if (numerator.Bounds.Width > denominator.Bounds.Width)
		{
			fractionLine.Point2 = new PointF(fractionLine.Point1.X + numerator.Bounds.Width, fractionLine.Point1.Y);
			fractionLine.Bounds = new RectangleF(xPosition, yPosition, numerator.Bounds.Width, 0.5f);
			ShiftEqFieldXPosition(denominator, CenterAlign(fractionLine, denominator.Bounds.Width));
		}
		else
		{
			fractionLine.Point2 = new PointF(fractionLine.Point1.X + denominator.Bounds.Width, fractionLine.Point1.Y);
			fractionLine.Bounds = new RectangleF(xPosition, yPosition, denominator.Bounds.Width, 0.5f);
			ShiftEqFieldXPosition(numerator, CenterAlign(fractionLine, numerator.Bounds.Width));
		}
	}

	private void SetYForFractionElements(LayoutedEQFields numerator, LayoutedEQFields denominator, DrawingContext dc, float xPosition, float yPosition)
	{
		float yPosition2 = yPosition - (1f + GetTopMostY(denominator));
		dc.ShiftEqFieldYPosition(denominator, yPosition2);
		yPosition2 = yPosition - (numerator.Bounds.Bottom + 1f);
		dc.ShiftEqFieldYPosition(numerator, yPosition2);
	}

	private float CenterAlign(LineEQField lineEqField, float textWidth)
	{
		return (lineEqField.Point2.X - lineEqField.Point1.X - textWidth) / 2f;
	}

	private bool IsValidFractionSwitch(string fieldCode)
	{
		if (HasParenthesisError(fieldCode))
		{
			return false;
		}
		string text = fieldCode.Substring(fieldCode.IndexOf("\\f", StringComparison.CurrentCultureIgnoreCase) + 2);
		if (!StartsWithExt(text.TrimStart(), "("))
		{
			return false;
		}
		if (SplitElementsByComma(text).Length != 2)
		{
			return false;
		}
		List<string> splittedFieldCodes = new List<string>();
		if (!IsValidEqFieldCode(splittedFieldCodes, text))
		{
			return false;
		}
		return true;
	}

	private void LayoutEquationFieldCode(FontScriptType fontScriptType, LayoutedEQFields layouttedEqField, List<string> splittedFieldCodeSwitch, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		for (int i = 0; i < splittedFieldCodeSwitch.Count; i++)
		{
			LayoutedEQFields layoutedEQFields = LayoutSwitch(fontScriptType, splittedFieldCodeSwitch[i], dc, charFormat, xPosition, yPosition);
			layouttedEqField.ChildEQFileds.Add(layoutedEQFields);
			xPosition = layoutedEQFields.Bounds.Right;
		}
		UpdateltEqFieldsBounds(layouttedEqField);
	}

	private LayoutedEQFields LayoutBracketSwitch(FontScriptType fontScriptType, string fieldCode, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		string substringTill = GetSubstringTill(fieldCode, '(');
		string text = fieldCode.Substring(fieldCode.IndexOf(substringTill, StringComparison.Ordinal) + substringTill.Length);
		text = text.Substring(1, text.Length - 2);
		char openingChar = '\0';
		char closingChar = '\0';
		GetBracketsType(substringTill, ref openingChar, ref closingChar);
		if (openingChar == '\0' && closingChar == '\0')
		{
			openingChar = '(';
			closingChar = ')';
		}
		LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
		layoutedEQFields.SwitchType = LayoutedEQFields.EQSwitchType.Bracket;
		if (!string.IsNullOrEmpty(text))
		{
			GenerateBracketSwitch(fontScriptType, layoutedEQFields, text, openingChar, closingChar, dc, charFormat, xPosition, yPosition);
		}
		else
		{
			GenerateSwitch(fontScriptType, layoutedEQFields, text, dc, charFormat.GetFontToRender(fontScriptType), charFormat, xPosition, yPosition);
		}
		return layoutedEQFields;
	}

	private void GetBracketsType(string bracketFieldCode, ref char openingChar, ref char closingChar)
	{
		string[] array = bracketFieldCode.Split('\\');
		for (int i = 0; i < array.Length; i++)
		{
			if (i < array.Length - 1)
			{
				char c = array[i + 1][0];
				if (array[i].StartsWith("bc", StringComparison.CurrentCultureIgnoreCase))
				{
					openingChar = c;
					closingChar = GetCorrespondingCloseCharacter(openingChar);
				}
				else if (array[i].StartsWith("lc", StringComparison.CurrentCultureIgnoreCase))
				{
					openingChar = c;
				}
				else if (array[i].StartsWith("rc", StringComparison.CurrentCultureIgnoreCase))
				{
					closingChar = c;
				}
			}
		}
	}

	private char GetCorrespondingCloseCharacter(char openingCharacter)
	{
		return openingCharacter switch
		{
			'(' => ')', 
			'{' => '}', 
			'[' => ']', 
			'<' => '>', 
			_ => openingCharacter, 
		};
	}

	private void GenerateBracketSwitch(FontScriptType fontScriptType, LayoutedEQFields bracketSwitch, string element, char openBracket, char closeBracket, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		GenerateSwitch(fontScriptType, bracketSwitch, element, dc, charFormat.GetFontToRender(fontScriptType), charFormat, xPosition, yPosition);
		float num = dc.GetAscent(charFormat.GetFontToRender(fontScriptType), fontScriptType) / 2f;
		float extraWidth = 0f;
		bool flag = IsBracketContainsMinimumHeight(fontScriptType, openBracket, charFormat, dc, bracketSwitch.Bounds.Height);
		if (!flag && (openBracket == '[' || closeBracket == ']'))
		{
			extraWidth = 1f;
		}
		float height = bracketSwitch.Bounds.Height;
		height -= num / 2f;
		float yPosition2 = bracketSwitch.Bounds.Y + num;
		GenerateopeningBracket(fontScriptType, bracketSwitch, openBracket, dc, charFormat, flag, xPosition, yPosition2, height, extraWidth, num);
		GenerateClosingBracket(fontScriptType, bracketSwitch, closeBracket, dc, charFormat, flag, bracketSwitch.Bounds.Right, yPosition2, height, extraWidth, num);
		UpdateltEqFieldsBounds(bracketSwitch);
	}

	private void GenerateopeningBracket(FontScriptType fontScriptType, LayoutedEQFields bracketSwitch, char openBracket, DrawingContext dc, WCharacterFormat charFormat, bool isBracketContainsMinimumHeight, float xPosition, float yPosition, float maxHeight, float extraWidth, float extraYPosition)
	{
		LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
		if (isBracketContainsMinimumHeight)
		{
			AdjustFontSizeOfCharacter(fontScriptType, layoutedEQFields, openBracket, dc, charFormat, xPosition, bracketSwitch.Bounds.Height);
		}
		else
		{
			GenerateCharacter(fontScriptType, layoutedEQFields, openBracket, dc, charFormat, xPosition, yPosition, maxHeight);
		}
		layoutedEQFields.Bounds = new RectangleF(layoutedEQFields.Bounds.X, layoutedEQFields.Bounds.Y, layoutedEQFields.Bounds.Width + extraWidth, layoutedEQFields.Bounds.Height - extraYPosition / 2f);
		ShiftEqFieldXPosition(bracketSwitch, layoutedEQFields.Bounds.Width + extraWidth);
		bracketSwitch.ChildEQFileds.Add(layoutedEQFields);
	}

	private void GenerateClosingBracket(FontScriptType fontScriptType, LayoutedEQFields bracketSwitch, char closeBracket, DrawingContext dc, WCharacterFormat charFormat, bool isBracketContainsMinimumHeight, float xPosition, float yPosition, float maxHeight, float extraWidth, float extraYPosition)
	{
		LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
		isBracketContainsMinimumHeight = IsBracketContainsMinimumHeight(fontScriptType, closeBracket, charFormat, dc, bracketSwitch.Bounds.Height);
		if (isBracketContainsMinimumHeight)
		{
			AdjustFontSizeOfCharacter(fontScriptType, layoutedEQFields, closeBracket, dc, charFormat, xPosition, bracketSwitch.Bounds.Height);
		}
		else
		{
			GenerateCharacter(fontScriptType, layoutedEQFields, closeBracket, dc, charFormat, xPosition, yPosition, maxHeight);
			layoutedEQFields.Bounds = new RectangleF(layoutedEQFields.Bounds.X, layoutedEQFields.Bounds.Y, layoutedEQFields.Bounds.Width + extraWidth, layoutedEQFields.Bounds.Height - extraYPosition / 2f);
			ShiftEqFieldXPosition(layoutedEQFields, extraWidth);
		}
		bracketSwitch.ChildEQFileds.Add(layoutedEQFields);
	}

	private bool IsBracketContainsMinimumHeight(FontScriptType fontScriptType, char inputCharacter, WCharacterFormat charFormat, DrawingContext dc, float maxHeight)
	{
		float num = 23f;
		float num2 = 14f;
		float height = dc.MeasureString(inputCharacter.ToString(), charFormat.GetFontToRender(fontScriptType), null, charFormat, isMeasureFromTabList: false, fontScriptType).Height;
		switch (inputCharacter)
		{
		case '(':
		case ')':
			if (num2 >= maxHeight)
			{
				return true;
			}
			break;
		case '{':
		case '}':
			if (num >= maxHeight)
			{
				return true;
			}
			break;
		case '[':
		case ']':
			if (height >= maxHeight)
			{
				return true;
			}
			break;
		}
		return false;
	}

	private void GenerateCharacter(FontScriptType fontScriptType, LayoutedEQFields layoutedCharacter, char inputCharacter, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition, float maxHeight)
	{
		switch (inputCharacter)
		{
		case '(':
			GenerateParenthesisFromUnicodes(layoutedCharacter, '⎧', '⎪', '⎩', dc, charFormat, xPosition, yPosition, maxHeight);
			break;
		case ')':
			GenerateParenthesisFromUnicodes(layoutedCharacter, '⎫', '⎪', '⎭', dc, charFormat, xPosition, yPosition, maxHeight);
			break;
		case '{':
			GeneratCurlyBraceFromUnicodes(layoutedCharacter, '⎧', '⎨', '⎩', '⎪', dc, charFormat, xPosition, yPosition, maxHeight);
			break;
		case '}':
			GeneratCurlyBraceFromUnicodes(layoutedCharacter, '⎫', '⎬', '⎭', '⎪', dc, charFormat, xPosition, yPosition, maxHeight);
			break;
		case '[':
			GenerateSquareBracketFromUnicodes(layoutedCharacter, '⎡', '⎢', '⎣', dc, charFormat, xPosition, yPosition, maxHeight);
			break;
		case ']':
			GenerateSquareBracketFromUnicodes(layoutedCharacter, '⎤', '⎥', '⎦', dc, charFormat, xPosition, yPosition, maxHeight);
			break;
		default:
			AdjustFontSizeOfCharacter(fontScriptType, layoutedCharacter, inputCharacter, dc, charFormat, xPosition, maxHeight);
			break;
		}
	}

	private void AdjustFontSizeOfCharacter(FontScriptType fontScriptType, LayoutedEQFields layoutedCharacter, char inputCharacter, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float maxHeight)
	{
		if (maxHeight <= 0f)
		{
			maxHeight = charFormat.GetFontToRender(fontScriptType).Size;
		}
		TextEQField textEQField = new TextEQField();
		textEQField.Text = inputCharacter.ToString();
		textEQField.Font = base.Document.FontSettings.GetFont(charFormat.GetFontToRender(fontScriptType).Name, maxHeight, charFormat.GetFontToRender(fontScriptType).Style, fontScriptType);
		textEQField.Bounds = new RectangleF(new PointF(xPosition, 0f), dc.MeasureString(textEQField.Text, textEQField.Font, null, charFormat, isMeasureFromTabList: false, FontScriptType.English));
		layoutedCharacter.ChildEQFileds.Add(textEQField);
		float ascent = dc.GetAscent(textEQField.Font, FontScriptType.English);
		dc.ShiftEqFieldYPosition(layoutedCharacter, 0f - ascent);
		UpdateltEqFieldsBounds(layoutedCharacter);
	}

	private void GenerateParenthesisFromUnicodes(LayoutedEQFields charField, char upperPartUnicode, char middlePartUnicode, char lowerPartUnicode, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition, float maxHeight)
	{
		TextEQField textEQField = new TextEQField();
		GenerateUpperPartOfBracket(charField, upperPartUnicode, textEQField, dc, charFormat, xPosition, yPosition);
		TextEQField textEQField2 = new TextEQField();
		GenerateLowerPartOfBracket(charField, lowerPartUnicode, textEQField2, dc, charFormat, xPosition, yPosition + maxHeight);
		float num = textEQField2.Bounds.Top - textEQField.Bounds.Bottom;
		if (num > 0f)
		{
			LayoutedEQFields middlePart = new LayoutedEQFields();
			GenerateMiddlePartParenthesis(charField, middlePartUnicode, middlePart, textEQField2, dc, charFormat, xPosition, textEQField.Bounds.Bottom, num);
		}
		UpdateltEqFieldsBounds(charField);
	}

	private void GenerateMiddlePartParenthesis(LayoutedEQFields charField, char middlePartUnicode, LayoutedEQFields middlePart, TextEQField lowerPart, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition, float maxHeight)
	{
		GenerateRepeatedCharacter(middlePart, middlePartUnicode, dc, charFormat, xPosition, yPosition, maxHeight, 0f);
		float num = middlePart.Bounds.Height - (maxHeight + lowerPart.Font.Size / 2f);
		if (num > 0f)
		{
			dc.ShiftEqFieldYPosition(middlePart, 0f - num);
		}
		charField.ChildEQFileds.Add(middlePart);
	}

	private void GenerateSquareBracketFromUnicodes(LayoutedEQFields charField, char upperPartUnicode, char middlePartUnicode, char lowerPartUnicode, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition, float maxHeight)
	{
		TextEQField textEQField = new TextEQField();
		GenerateUpperPartOfBracket(charField, upperPartUnicode, textEQField, dc, charFormat, xPosition, yPosition);
		TextEQField textEQField2 = new TextEQField();
		GenerateLowerPartOfBracket(charField, lowerPartUnicode, textEQField2, dc, charFormat, xPosition, yPosition + maxHeight);
		float num = textEQField2.Bounds.Top - textEQField.Bounds.Bottom;
		if (num > 0f)
		{
			LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
			GenerateRepeatedCharacter(layoutedEQFields, middlePartUnicode, dc, charFormat, xPosition, textEQField.Bounds.Bottom, num, 0f);
			charField.ChildEQFileds.Add(layoutedEQFields);
		}
		UpdateltEqFieldsBounds(charField);
	}

	private void GeneratCurlyBraceFromUnicodes(LayoutedEQFields charField, char upperPartUnicode, char middlePartUnicode, char lowerPartUnicode, char extensionPartUnicode, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition, float maxHeight)
	{
		TextEQField textEQField = new TextEQField();
		GenerateUpperPartOfBracket(charField, upperPartUnicode, textEQField, dc, charFormat, xPosition, yPosition);
		TextEQField textEQField2 = new TextEQField();
		GenerateLowerPartOfBracket(charField, lowerPartUnicode, textEQField2, dc, charFormat, xPosition, yPosition + maxHeight);
		float yPosition2 = yPosition + maxHeight / 2f;
		TextEQField textEQField3 = new TextEQField();
		GenerateUpperPartOfBracket(charField, middlePartUnicode, textEQField3, dc, charFormat, xPosition, yPosition2);
		UpdateltEqFieldsBounds(charField);
		float num = textEQField3.Bounds.Top - textEQField3.Font.Size - textEQField.Bounds.Bottom;
		if (num > 0f)
		{
			GenerateUpperPartExtension(charField, extensionPartUnicode, textEQField3, dc, charFormat, xPosition, textEQField.Bounds.Bottom, num);
		}
		num = textEQField2.Bounds.Top - textEQField3.Bounds.Bottom;
		if (num > 0f)
		{
			GenerateLowerPartExtension(charField, extensionPartUnicode, textEQField2, dc, charFormat, xPosition, textEQField3.Bounds.Bottom, num);
		}
		UpdateltEqFieldsBounds(charField);
	}

	private void GenerateUpperPartExtension(LayoutedEQFields charField, char extensionPartUnicode, TextEQField middlePart, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition, float maxHeight)
	{
		LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
		GenerateRepeatedCharacter(layoutedEQFields, extensionPartUnicode, dc, charFormat, xPosition, yPosition, maxHeight, 0f);
		if (layoutedEQFields.Bounds.Height > maxHeight)
		{
			float num = layoutedEQFields.Bounds.Height - (maxHeight + middlePart.Font.Size * 0.25f);
			if (num > 0f)
			{
				dc.ShiftEqFieldYPosition(layoutedEQFields, 0f - num);
			}
		}
		charField.ChildEQFileds.Add(layoutedEQFields);
	}

	private void GenerateLowerPartExtension(LayoutedEQFields charField, char extensionPartUnicode, TextEQField lowerPart, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition, float maxHeight)
	{
		LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
		GenerateRepeatedCharacter(layoutedEQFields, extensionPartUnicode, dc, charFormat, xPosition, yPosition, maxHeight, 0f);
		if (layoutedEQFields.Bounds.Height > maxHeight)
		{
			float num = layoutedEQFields.Bounds.Height - (maxHeight + lowerPart.Font.Size / 2f);
			if (num > 0f)
			{
				dc.ShiftEqFieldYPosition(layoutedEQFields, 0f - num);
			}
		}
		charField.ChildEQFileds.Add(layoutedEQFields);
		UpdateltEqFieldsBounds(charField);
	}

	private void GenerateUpperPartOfBracket(LayoutedEQFields charField, char upperPartUnicode, TextEQField upperPart, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		upperPart.Text = upperPartUnicode.ToString();
		upperPart.Font = base.Document.FontSettings.GetFont("Cambria Math", 8f, FontStyle.Regular, FontScriptType.English);
		upperPart.Bounds = new RectangleF(new PointF(xPosition, yPosition), dc.MeasureString(upperPart.Text, upperPart.Font, null, charFormat, isMeasureFromTabList: false, FontScriptType.English));
		charField.ChildEQFileds.Add(upperPart);
	}

	private void GenerateLowerPartOfBracket(LayoutedEQFields charField, char lowerPartUnicode, TextEQField lowerPart, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		lowerPart.Text = lowerPartUnicode.ToString();
		lowerPart.Font = base.Document.FontSettings.GetFont("Cambria Math", 8f, FontStyle.Regular, FontScriptType.English);
		float y = yPosition - dc.MeasureString(lowerPart.Text, lowerPart.Font, null, charFormat, isMeasureFromTabList: false, FontScriptType.English).Height;
		lowerPart.Bounds = new RectangleF(new PointF(xPosition, y), dc.MeasureString(lowerPart.Text, lowerPart.Font, null, charFormat, isMeasureFromTabList: false, FontScriptType.English));
		charField.ChildEQFileds.Add(lowerPart);
	}

	private void GenerateRepeatedCharacter(LayoutedEQFields extensionPart, char extensionUnicode, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition, float maxHeight, float fontSize)
	{
		while (extensionPart.Bounds.Height < maxHeight)
		{
			TextEQField textEQField = new TextEQField();
			textEQField.Text = extensionUnicode.ToString();
			textEQField.Font = (textEQField.Text.Equals('ô'.ToString()) ? base.Document.FontSettings.GetFont("Symbol", fontSize, FontStyle.Regular, FontScriptType.English) : base.Document.FontSettings.GetFont("Cambria Math", 8f, FontStyle.Regular, FontScriptType.English));
			textEQField.Bounds = new RectangleF(new PointF(xPosition, yPosition), dc.MeasureString(textEQField.Text, textEQField.Font, null, charFormat, isMeasureFromTabList: false, FontScriptType.English));
			if (textEQField.Text.Equals('ô'.ToString()) && extensionPart.ChildEQFileds.Count > 0)
			{
				dc.ShiftEqFieldYPosition(textEQField, 0f - (float)dc.FontMetric.Descent(base.Document.FontSettings.GetFont(textEQField.Font.Name, textEQField.Font.Size, textEQField.Font.Style, FontScriptType.English), FontScriptType.English));
			}
			yPosition = textEQField.Bounds.Bottom;
			extensionPart.ChildEQFileds.Add(textEQField);
			UpdateltEqFieldsBounds(extensionPart);
		}
		UpdateltEqFieldsBounds(extensionPart);
	}

	private bool IsValidBracketSwitch(string fieldCode)
	{
		if (HasParenthesisError(fieldCode))
		{
			return false;
		}
		string substringTill = GetSubstringTill(fieldCode, '(');
		string text = fieldCode.Substring(fieldCode.IndexOf(substringTill, StringComparison.Ordinal) + substringTill.Length);
		if (SplitElementsByComma(text).Length != 1)
		{
			return false;
		}
		string[] fieldCodes = substringTill.Split('\\');
		if (HasInCorrectBracketSwitchSequence(fieldCodes))
		{
			return false;
		}
		List<string> splittedFieldCodes = new List<string>();
		if (!IsValidEqFieldCode(splittedFieldCodes, text))
		{
			return false;
		}
		return true;
	}

	private bool HasInCorrectBracketSwitchSequence(string[] fieldCodes)
	{
		for (int i = 1; i < fieldCodes.Length; i++)
		{
			if (!fieldCodes[i].TrimEnd().Equals("b", StringComparison.CurrentCultureIgnoreCase) || i != 1)
			{
				if (!fieldCodes[i].StartsWith("bc", StringComparison.CurrentCultureIgnoreCase) && !fieldCodes[i].StartsWith("lc", StringComparison.CurrentCultureIgnoreCase) && !fieldCodes[i].StartsWith("rc", StringComparison.CurrentCultureIgnoreCase))
				{
					return true;
				}
				if (!IsOnlyAlphabets(fieldCodes[i].Trim()))
				{
					return true;
				}
				if (i != fieldCodes.Length - 1 && (!(fieldCodes[i + 1] != "") || HasManyCharacters(fieldCodes[i + 1])))
				{
					return true;
				}
				i++;
			}
		}
		return false;
	}

	private bool IsValidBoxSwitch(string fieldCode)
	{
		if (HasParenthesisError(fieldCode))
		{
			return false;
		}
		string substringTill = GetSubstringTill(fieldCode, '(');
		string text = fieldCode.Substring(fieldCode.IndexOf(substringTill, StringComparison.Ordinal) + substringTill.Length);
		if (!StartsWithExt(text.TrimStart(), "("))
		{
			return false;
		}
		if (SplitElementsByComma(text).Length != 1)
		{
			return false;
		}
		if (!IsCorrectBoxSwitchSequence(substringTill))
		{
			return false;
		}
		List<string> splittedFieldCodes = new List<string>();
		if (!IsValidEqFieldCode(splittedFieldCodes, text))
		{
			return false;
		}
		return true;
	}

	private bool IsCorrectBoxSwitchSequence(string boxSwitchEqCode)
	{
		if (boxSwitchEqCode.Contains("\\ "))
		{
			return false;
		}
		string[] array = boxSwitchEqCode.Split('\\');
		for (int i = 1; i < array.Length; i++)
		{
			if (array[i].TrimEnd().Contains(" "))
			{
				return false;
			}
			if (!IsOnlyAlphabets(array[i].Trim()))
			{
				return false;
			}
			if (!array[i].StartsWith("le", StringComparison.CurrentCultureIgnoreCase) && !array[i].StartsWith("ri", StringComparison.CurrentCultureIgnoreCase) && !array[i].StartsWith("to", StringComparison.CurrentCultureIgnoreCase) && !array[i].StartsWith("bo", StringComparison.CurrentCultureIgnoreCase) && (!array[i].TrimEnd().Equals("x", StringComparison.OrdinalIgnoreCase) || i != 1))
			{
				return false;
			}
		}
		return true;
	}

	private LayoutedEQFields LayoutBoxSwitch(FontScriptType fontScriptType, string fieldCode, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		string substringTill = GetSubstringTill(fieldCode, '(');
		string element = fieldCode.Substring(fieldCode.IndexOf(substringTill, StringComparison.Ordinal) + substringTill.Length);
		bool hasLeftLine = false;
		bool hasTopLine = false;
		bool hasRightLine = false;
		bool hasBottomLine = false;
		GetBoxSwitchProperties(substringTill, ref hasLeftLine, ref hasRightLine, ref hasBottomLine, ref hasTopLine);
		LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
		layoutedEQFields.SwitchType = LayoutedEQFields.EQSwitchType.Box;
		GenerateBoxSwitch(fontScriptType, layoutedEQFields, element, dc, charFormat, xPosition, yPosition);
		float extraBoxWidth = dc.MeasureString(" ", charFormat.GetFontToRender(fontScriptType), null, charFormat, isMeasureFromTabList: false, FontScriptType.English).Width * 1.5f;
		InsertBox(layoutedEQFields, hasLeftLine, hasTopLine, hasRightLine, hasBottomLine, extraBoxWidth);
		UpdateltEqFieldsBounds(layoutedEQFields);
		return layoutedEQFields;
	}

	private void GetBoxSwitchProperties(string boxSwitchEqCode, ref bool hasLeftLine, ref bool hasRightLine, ref bool hasBottomLine, ref bool hasTopLine)
	{
		string[] array = boxSwitchEqCode.Split('\\');
		for (int i = 2; i < array.Length; i++)
		{
			switch (array[i].Substring(0, 2).ToLower())
			{
			case "le":
				hasLeftLine = true;
				break;
			case "ri":
				hasRightLine = true;
				break;
			case "to":
				hasTopLine = true;
				break;
			case "bo":
				hasBottomLine = true;
				break;
			}
		}
	}

	private void InsertBox(LayoutedEQFields boxSwitch, bool hasLeftLine, bool hasTopLine, bool hasRightLine, bool hasBottomLine, float extraBoxWidth)
	{
		RectangleF bounds = boxSwitch.Bounds;
		bool flag = !(hasLeftLine || hasRightLine || hasTopLine || hasBottomLine);
		if (hasLeftLine || flag)
		{
			AddLineEQChild(boxSwitch, bounds.X + 1f, bounds.Y + extraBoxWidth, bounds.X + 1f, bounds.Y + bounds.Height - extraBoxWidth, "leftLine");
		}
		if (hasRightLine || flag)
		{
			AddLineEQChild(boxSwitch, bounds.X + bounds.Width - 2f, bounds.Y + extraBoxWidth, bounds.X + bounds.Width - 2f, bounds.Y + bounds.Height - extraBoxWidth, "rightLine");
		}
		if (hasTopLine || flag)
		{
			AddLineEQChild(boxSwitch, bounds.X + 1f, bounds.Y + extraBoxWidth, bounds.X + bounds.Width - 2f, bounds.Y + extraBoxWidth, "topLine");
		}
		if (hasBottomLine || flag)
		{
			AddLineEQChild(boxSwitch, bounds.X + 1f, bounds.Y + bounds.Height - extraBoxWidth, bounds.X + bounds.Width - 2f, bounds.Y + bounds.Height - extraBoxWidth, "bottomLine");
		}
	}

	private void AddLineEQChild(LayoutedEQFields ltEqFields, float x1, float y1, float x2, float y2, string line)
	{
		LineEQField lineEQField = new LineEQField();
		lineEQField.Point1 = new PointF(x1, y1);
		lineEQField.Point2 = new PointF(x2, y2);
		switch (line)
		{
		case "leftLine":
		case "rightLine":
			lineEQField.Bounds = new RectangleF(x1, y1, 1f, y2 - y1);
			break;
		case "topLine":
		case "bottomLine":
			lineEQField.Bounds = new RectangleF(x1, y1, x2 - x1, 1f);
			break;
		}
		ltEqFields.ChildEQFileds.Add(lineEQField);
	}

	private void GenerateBoxSwitch(FontScriptType fontScriptType, LayoutedEQFields boxSwitch, string element, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		element = element.Substring(1, element.Length - 2);
		GenerateSwitch(fontScriptType, boxSwitch, element, dc, charFormat.GetFontToRender(fontScriptType), charFormat, xPosition, yPosition);
		float num = dc.MeasureString(" ", charFormat.GetFontToRender(fontScriptType), null, charFormat, isMeasureFromTabList: false, FontScriptType.English).Width * 1.5f;
		RectangleF bounds = boxSwitch.Bounds;
		boxSwitch.Bounds = new RectangleF(bounds.X - num, bounds.Y - num, bounds.Width + num * 2f, bounds.Height + num * 2f);
		ShiftEqFieldXPosition(boxSwitch, num);
	}

	private LayoutedEQFields LayoutIntegralSwitch(FontScriptType fontScriptType, string fieldCode, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		string substringTill = GetSubstringTill(fieldCode, '(');
		string element = fieldCode.Substring(fieldCode.IndexOf(substringTill, StringComparison.Ordinal) + substringTill.Length);
		bool isInline = false;
		bool isVaribleSize = false;
		string symbol = "I";
		GetIntegralProperties(substringTill, ref isInline, ref symbol, ref isVaribleSize);
		LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
		layoutedEQFields.SwitchType = LayoutedEQFields.EQSwitchType.Integral;
		GenerateIntegralSwitch(fontScriptType, layoutedEQFields, element, symbol, isInline, isVaribleSize, dc, charFormat, xPosition, yPosition);
		return layoutedEQFields;
	}

	private void GetIntegralProperties(string integralEqCode, ref bool isInline, ref string symbol, ref bool isVaribleSize)
	{
		string[] array = integralEqCode.Split('\\');
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].StartsWith("su", StringComparison.CurrentCultureIgnoreCase))
			{
				symbol = "S";
			}
			else if (array[i].StartsWith("pr", StringComparison.CurrentCultureIgnoreCase))
			{
				symbol = "P";
			}
			else if (array[i].StartsWith("fc", StringComparison.CurrentCultureIgnoreCase) && i < array.Length - 1)
			{
				symbol = array[i + 1][0].ToString();
				isVaribleSize = false;
			}
			else if (array[i].StartsWith("vc", StringComparison.CurrentCultureIgnoreCase) && i < array.Length - 1)
			{
				symbol = array[i + 1][0].ToString();
				isVaribleSize = true;
			}
			else if (array[i].StartsWith("in", StringComparison.CurrentCultureIgnoreCase))
			{
				isInline = true;
			}
		}
	}

	private void GenerateIntegralSwitch(FontScriptType fontScriptType, LayoutedEQFields integralSwitch, string element, string symbol, bool isInline, bool isVariableSize, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		string[] array = SplitElementsByComma(element);
		float maxWidth = 0f;
		LayoutedEQFields upperLimit = new LayoutedEQFields();
		LayoutedEQFields lowerLimit = new LayoutedEQFields();
		LayoutedEQFields integrand = new LayoutedEQFields();
		GenerateIntegralSwitchElements(fontScriptType, upperLimit, lowerLimit, integrand, ref maxWidth, array, dc, charFormat, xPosition, yPosition);
		float symbolheight = 0f;
		GetHeightOfSymbol(upperLimit, lowerLimit, integrand, isInline, ref symbolheight);
		LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
		if (symbol == "I" || array[0] != "" || array[1] != "" || array[2] != "")
		{
			GenerateSymbolForIntegralSwitch(fontScriptType, integrand, layoutedEQFields, symbol, isVariableSize, dc, charFormat, xPosition, yPosition, symbolheight);
		}
		maxWidth = ((maxWidth > layoutedEQFields.Bounds.Width) ? maxWidth : layoutedEQFields.Bounds.Width);
		if (layoutedEQFields.ChildEQFileds.Count > 0)
		{
			SetIntegralElementsPosition(fontScriptType, isInline, upperLimit, lowerLimit, layoutedEQFields, integrand, integralSwitch, symbol, dc, charFormat, xPosition, yPosition, maxWidth);
		}
	}

	private void GenerateIntegralSwitchElements(FontScriptType fontScriptType, LayoutedEQFields upperLimit, LayoutedEQFields lowerLimit, LayoutedEQFields integrand, ref float maxWidth, string[] integralElements, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		GenerateSwitch(fontScriptType, upperLimit, integralElements[1], dc, charFormat.GetFontToRender(fontScriptType), charFormat, xPosition, yPosition);
		maxWidth = upperLimit.Bounds.Width;
		GenerateSwitch(fontScriptType, lowerLimit, integralElements[0], dc, charFormat.GetFontToRender(fontScriptType), charFormat, xPosition, yPosition);
		maxWidth = ((maxWidth > lowerLimit.Bounds.Width) ? maxWidth : lowerLimit.Bounds.Width);
		GenerateSwitch(fontScriptType, integrand, integralElements[2], dc, charFormat.GetFontToRender(fontScriptType), charFormat, xPosition, yPosition);
	}

	private void GetHeightOfSymbol(LayoutedEQFields upperLimit, LayoutedEQFields lowerLimit, LayoutedEQFields integrand, bool isInline, ref float symbolheight)
	{
		if (isInline)
		{
			symbolheight = (((upperLimit.Bounds.Height + lowerLimit.Bounds.Height) / 2f > integrand.Bounds.Height) ? ((upperLimit.Bounds.Height + lowerLimit.Bounds.Height) / 2f) : integrand.Bounds.Height);
		}
		else
		{
			symbolheight = integrand.Bounds.Height;
		}
	}

	private void SetIntegralElementsPosition(FontScriptType fontScriptType, bool isInline, LayoutedEQFields upperLimit, LayoutedEQFields lowerLimit, LayoutedEQFields integralSymbol, LayoutedEQFields integrand, LayoutedEQFields integralSwitch, string symbol, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition, float maxWidth)
	{
		if (isInline)
		{
			AlignInlineIntegralSwitch(fontScriptType, upperLimit, lowerLimit, integralSymbol, integrand, integralSwitch, symbol, dc, charFormat, xPosition, yPosition);
		}
		else
		{
			AlignNotInlineIntegralSwitch(fontScriptType, upperLimit, lowerLimit, integralSymbol, integrand, integralSwitch, dc, charFormat, maxWidth);
		}
		float num = ((integralSwitch.ChildEQFileds.Count > 2) ? integralSwitch.ChildEQFileds[1].Bounds.Right : integralSwitch.Bounds.Right);
		ShiftEqFieldXPosition(integrand, num - integrand.Bounds.Left);
		integralSwitch.ChildEQFileds.Add(integrand);
		UpdateltEqFieldsBounds(integralSwitch);
	}

	private void AlignInlineIntegralSwitch(FontScriptType fontScriptType, LayoutedEQFields upperLimit, LayoutedEQFields lowerLimit, LayoutedEQFields integralSymbol, LayoutedEQFields integrand, LayoutedEQFields integralSwitch, string symbol, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		float yPosition2 = integrand.Bounds.Top - upperLimit.Bounds.Top;
		dc.ShiftEqFieldYPosition(upperLimit, yPosition2);
		yPosition2 = integrand.Bounds.Bottom - lowerLimit.Bounds.Bottom;
		dc.ShiftEqFieldYPosition(lowerLimit, yPosition2);
		float num = upperLimit.Bounds.Bottom - lowerLimit.Bounds.Top;
		if (num > 0f)
		{
			dc.ShiftEqFieldYPosition(upperLimit, 0f - num / 2f);
			dc.ShiftEqFieldYPosition(lowerLimit, num / 2f);
		}
		float symbolSize = lowerLimit.Bounds.Bottom - upperLimit.Bounds.Top;
		integralSymbol.ChildEQFileds.Clear();
		GenerateSymbolForIntegralSwitch(fontScriptType, integrand, integralSymbol, symbol, isVariableSize: false, dc, charFormat, xPosition, yPosition, symbolSize);
		EQFieldVerticalAlignment(integralSymbol, integrand, dc);
		yPosition2 = integralSymbol.Bounds.Right - upperLimit.Bounds.Left;
		ShiftEqFieldXPosition(upperLimit, yPosition2);
		yPosition2 = integralSymbol.Bounds.Right - lowerLimit.Bounds.Left;
		ShiftEqFieldXPosition(lowerLimit, yPosition2);
		integralSwitch.ChildEQFileds.Add(upperLimit);
		integralSwitch.ChildEQFileds.Add(lowerLimit);
		integralSwitch.ChildEQFileds.Add(integralSymbol);
		UpdateltEqFieldsBounds(integralSwitch);
	}

	private void AlignNotInlineIntegralSwitch(FontScriptType fontScriptType, LayoutedEQFields upperLimit, LayoutedEQFields lowerLimit, LayoutedEQFields integralSymbol, LayoutedEQFields integrand, LayoutedEQFields integralSwitch, DrawingContext dc, WCharacterFormat charFormat, float maxWidth)
	{
		if (integralSymbol.ChildEQFileds.Count > 1)
		{
			AlignIntegralSymbol(fontScriptType, upperLimit, lowerLimit, integralSymbol, integrand, dc, charFormat);
		}
		else
		{
			AlignPiOrSummationSymbol(upperLimit, lowerLimit, integralSymbol, integrand, dc);
		}
		ShiftEqFieldXPosition(upperLimit, GetCenterAlignSpace(maxWidth, upperLimit.Bounds.Width));
		ShiftEqFieldXPosition(integralSymbol, GetCenterAlignSpace(maxWidth, integralSymbol.Bounds.Width));
		ShiftEqFieldXPosition(lowerLimit, GetCenterAlignSpace(maxWidth, lowerLimit.Bounds.Width));
		integralSwitch.ChildEQFileds.Add(upperLimit);
		integralSwitch.ChildEQFileds.Add(integralSymbol);
		integralSwitch.ChildEQFileds.Add(lowerLimit);
		UpdateltEqFieldsBounds(integralSwitch);
	}

	private void AlignPiOrSummationSymbol(LayoutedEQFields upperLimit, LayoutedEQFields lowerLimit, LayoutedEQFields integralSymbol, LayoutedEQFields integrand, DrawingContext dc)
	{
		float yPosition = integrand.Bounds.Top - upperLimit.Bounds.Bottom;
		dc.ShiftEqFieldYPosition(upperLimit, yPosition);
		yPosition = integrand.Bounds.Bottom - lowerLimit.Bounds.Top;
		yPosition += (float)dc.FontMetric.Descent((integralSymbol.ChildEQFileds[0] as TextEQField).Font, FontScriptType.English) / 2f;
		dc.ShiftEqFieldYPosition(lowerLimit, yPosition);
		if (lowerLimit.Bounds.Height == 0f || upperLimit.Bounds.Height == 0f)
		{
			EQFieldVerticalAlignment(integralSymbol, integrand, dc);
			return;
		}
		float num = lowerLimit.Bounds.Bottom - upperLimit.Bounds.Top;
		float num2 = (num - integralSymbol.Bounds.Height) / 2f;
		yPosition = upperLimit.Bounds.Top + num2 - integralSymbol.Bounds.Y;
		dc.ShiftEqFieldYPosition(integralSymbol, yPosition);
		num2 = (num - integrand.Bounds.Height) / 2f;
		yPosition = upperLimit.Bounds.Top + num2 - integrand.Bounds.Y;
		dc.ShiftEqFieldYPosition(integrand, yPosition);
	}

	private void AlignIntegralSymbol(FontScriptType fontScriptType, LayoutedEQFields upperLimit, LayoutedEQFields lowerLimit, LayoutedEQFields integralSymbol, LayoutedEQFields integrand, DrawingContext dc, WCharacterFormat charFormat)
	{
		float yPosition = integrand.Bounds.Top - integralSymbol.Bounds.Top;
		dc.ShiftEqFieldYPosition(integralSymbol, yPosition);
		float extraHeightOfIntegralSymbol = GetExtraHeightOfIntegralSymbol(integrand.Bounds.Height, (float)dc.FontMetric.Descent(charFormat.GetFontToRender(fontScriptType), fontScriptType));
		if (upperLimit.Bounds.Height != 0f)
		{
			yPosition = integrand.Bounds.Top - upperLimit.Bounds.Bottom;
			yPosition -= (float)dc.FontMetric.Descent(charFormat.GetFontToRender(fontScriptType), fontScriptType) / 2f;
			dc.ShiftEqFieldYPosition(upperLimit, yPosition);
		}
		if (lowerLimit.Bounds.Height != 0f)
		{
			yPosition = integrand.Bounds.Bottom - lowerLimit.Bounds.Top;
			yPosition += GetExtraValueForLowerLimit(fontScriptType, integrand.Bounds.Height, dc, charFormat);
			dc.ShiftEqFieldYPosition(lowerLimit, yPosition);
		}
		if (upperLimit.Bounds.Height == 0f)
		{
			upperLimit.Bounds = new RectangleF(upperLimit.Bounds.X, upperLimit.Bounds.Y, upperLimit.Bounds.Width, upperLimit.Bounds.Height + extraHeightOfIntegralSymbol);
			yPosition = integrand.Bounds.Top - upperLimit.Bounds.Bottom;
			dc.ShiftEqFieldYPosition(upperLimit, yPosition);
		}
		if (lowerLimit.Bounds.Height == 0f)
		{
			lowerLimit.Bounds = new RectangleF(lowerLimit.Bounds.X, lowerLimit.Bounds.Y, lowerLimit.Bounds.Width, lowerLimit.Bounds.Height + extraHeightOfIntegralSymbol);
			yPosition = integrand.Bounds.Bottom - lowerLimit.Bounds.Top;
			dc.ShiftEqFieldYPosition(lowerLimit, yPosition);
		}
	}

	private float GetExtraValueForLowerLimit(FontScriptType fontScriptType, float integrandHeight, DrawingContext dc, WCharacterFormat charFormat)
	{
		float num = (float)dc.FontMetric.Descent(charFormat.GetFontToRender(fontScriptType), fontScriptType);
		if (charFormat.GetFontToRender(fontScriptType).Size >= 12f)
		{
			return 0f;
		}
		if (integrandHeight > 14f && integrandHeight <= 21f)
		{
			return num / 3f;
		}
		if (integrandHeight > 21f)
		{
			return 0f - num;
		}
		return num;
	}

	private float GetExtraHeightOfIntegralSymbol(float integrandHeight, float descent)
	{
		if (integrandHeight >= 48f && integrandHeight < 73f)
		{
			return descent / 2f;
		}
		if (integrandHeight >= 73f)
		{
			return descent / 4f;
		}
		return descent;
	}

	private float GetCenterAlignSpace(float maxSize, float elementSize)
	{
		return (maxSize - elementSize) / 2f;
	}

	private void GenerateSymbolForIntegralSwitch(FontScriptType fontScriptType, LayoutedEQFields integrand, LayoutedEQFields layoutedSymbol, string inputCharacter, bool isVariableSize, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition, float symbolSize)
	{
		switch (inputCharacter)
		{
		case "S":
		{
			float integralSymbolFontSize = GetFontSizeFromHeight("å", ref symbolSize, dc, charFormat);
			DocGen.Drawing.Font font = base.Document.FontSettings.GetFont("Symbol", integralSymbolFontSize, FontStyle.Regular, fontScriptType);
			GenerateSwitch(fontScriptType, layoutedSymbol, "å", dc, font, charFormat, xPosition, yPosition);
			layoutedSymbol.Bounds = new RectangleF(layoutedSymbol.Bounds.X, layoutedSymbol.Bounds.Y, layoutedSymbol.Bounds.Width, symbolSize);
			return;
		}
		case "I":
		{
			float integralSymbolFontSize = GetIntegralSymbolFontSize(integrand);
			if (symbolSize <= 17.726074f)
			{
				GenerateTwoPartsOfIntegral(layoutedSymbol, dc, charFormat, xPosition, yPosition, integralSymbolFontSize);
			}
			else
			{
				GenerateIntegralFromUnicodes(layoutedSymbol, dc, charFormat, xPosition, yPosition, symbolSize, integralSymbolFontSize);
			}
			return;
		}
		case "P":
		{
			float integralSymbolFontSize = GetFontSizeFromHeight("Õ", ref symbolSize, dc, charFormat);
			DocGen.Drawing.Font font = base.Document.FontSettings.GetFont("Symbol", integralSymbolFontSize, FontStyle.Regular, fontScriptType);
			GenerateSwitch(fontScriptType, layoutedSymbol, "Õ", dc, font, charFormat, xPosition, yPosition);
			return;
		}
		}
		TextEQField textEQField = new TextEQField();
		textEQField.Text = inputCharacter;
		if (isVariableSize)
		{
			textEQField.Font = base.Document.FontSettings.GetFont(charFormat.GetFontToRender(fontScriptType).Name, symbolSize, charFormat.GetFontToRender(fontScriptType).Style, fontScriptType);
		}
		else
		{
			textEQField.Font = base.Document.FontSettings.GetFont(charFormat.GetFontToRender(fontScriptType).Name, charFormat.GetFontToRender(fontScriptType).Size, charFormat.GetFontToRender(fontScriptType).Style, fontScriptType);
		}
		textEQField.Bounds = new RectangleF(new PointF(xPosition, yPosition), dc.MeasureString(textEQField.Text, textEQField.Font, null, charFormat, isMeasureFromTabList: false, FontScriptType.English));
		layoutedSymbol.ChildEQFileds.Add(textEQField);
		UpdateltEqFieldsBounds(layoutedSymbol);
	}

	private float GetIntegralSymbolFontSize(LayoutedEQFields integrand)
	{
		float result = 12f;
		if (integrand.Bounds.Height >= 48f && integrand.Bounds.Height < 73f)
		{
			result = 18f;
		}
		else if (integrand.Bounds.Height >= 73f)
		{
			result = 24f;
		}
		return result;
	}

	private void GenerateTwoPartsOfIntegral(LayoutedEQFields layoutedSymbol, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition, float fontSize)
	{
		TextEQField upperPart = new TextEQField();
		GenerateUpperPartOfIntegral(layoutedSymbol, upperPart, dc, charFormat, xPosition, yPosition, fontSize);
		TextEQField lowerPart = new TextEQField();
		GenerateLowerPartOfIntegral(layoutedSymbol, lowerPart, dc, charFormat, xPosition, yPosition + 17.726074f, fontSize);
		UpdateltEqFieldsBounds(layoutedSymbol);
	}

	private void GenerateIntegralFromUnicodes(LayoutedEQFields layoutedSymbol, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition, float maxHeight, float fontSize)
	{
		maxHeight -= 0.5f;
		TextEQField textEQField = new TextEQField();
		GenerateUpperPartOfIntegral(layoutedSymbol, textEQField, dc, charFormat, xPosition, yPosition, fontSize);
		TextEQField textEQField2 = new TextEQField();
		GenerateLowerPartOfIntegral(layoutedSymbol, textEQField2, dc, charFormat, xPosition, yPosition + maxHeight, fontSize);
		float num = (float)dc.FontMetric.Descent(textEQField.Font, FontScriptType.English);
		float num2 = textEQField2.Bounds.Top + num - (textEQField.Bounds.Bottom - num);
		if (num2 > 0f)
		{
			LayoutedEQFields middlePart = new LayoutedEQFields();
			GenerateMiddlePartOfIntegral(layoutedSymbol, middlePart, textEQField2, dc, charFormat, xPosition, textEQField.Bounds.Bottom - num, num2, fontSize);
		}
		UpdateltEqFieldsBounds(layoutedSymbol);
	}

	private void GenerateUpperPartOfIntegral(LayoutedEQFields layoutedSymbol, TextEQField upperPart, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition, float fontSize)
	{
		upperPart.Text = 'ó'.ToString();
		upperPart.Font = base.Document.FontSettings.GetFont("Symbol", fontSize, FontStyle.Regular, FontScriptType.English);
		upperPart.Bounds = new RectangleF(new PointF(xPosition, yPosition), dc.MeasureString(upperPart.Text, upperPart.Font, null, charFormat, isMeasureFromTabList: false, FontScriptType.English));
		layoutedSymbol.ChildEQFileds.Add(upperPart);
	}

	private void GenerateLowerPartOfIntegral(LayoutedEQFields layoutedSymbol, TextEQField lowerPart, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition, float fontSize)
	{
		lowerPart.Text = 'õ'.ToString();
		lowerPart.Font = base.Document.FontSettings.GetFont("Symbol", fontSize, FontStyle.Regular, FontScriptType.English);
		float y = yPosition - dc.MeasureString(lowerPart.Text, lowerPart.Font, null, charFormat, isMeasureFromTabList: false, FontScriptType.English).Height;
		lowerPart.Bounds = new RectangleF(new PointF(xPosition, y), dc.MeasureString(lowerPart.Text, lowerPart.Font, null, charFormat, isMeasureFromTabList: false, FontScriptType.English));
		layoutedSymbol.ChildEQFileds.Add(lowerPart);
	}

	private void GenerateMiddlePartOfIntegral(LayoutedEQFields layoutedSymbol, LayoutedEQFields middlePart, TextEQField lowerPart, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition, float maxHeight, float fontSize)
	{
		GenerateRepeatedCharacter(middlePart, 'ô', dc, charFormat, xPosition, yPosition, maxHeight, fontSize);
		float num = (float)dc.FontMetric.Descent((middlePart.ChildEQFileds[0] as TextEQField).Font, FontScriptType.English);
		float num2 = middlePart.Bounds.Bottom - (lowerPart.Bounds.Bottom - num * 2f);
		if (num2 > 0f)
		{
			dc.ShiftEqFieldYPosition(middlePart.ChildEQFileds[middlePart.ChildEQFileds.Count - 1], 0f - num2);
		}
		layoutedSymbol.ChildEQFileds.Add(middlePart);
	}

	private bool IsValidIntegralSwitch(string fieldCode)
	{
		if (HasParenthesisError(fieldCode))
		{
			return false;
		}
		string substringTill = GetSubstringTill(fieldCode, '(');
		string text = fieldCode.Substring(fieldCode.IndexOf(substringTill, StringComparison.Ordinal) + substringTill.Length);
		if (SplitElementsByComma(text).Length != 3)
		{
			return false;
		}
		string[] fieldCodes = substringTill.Split('\\');
		if (HasInCorrectIntegralSequence(fieldCodes))
		{
			return false;
		}
		List<string> splittedFieldCodes = new List<string>();
		if (!IsValidEqFieldCode(splittedFieldCodes, text))
		{
			return false;
		}
		return true;
	}

	private bool HasInCorrectIntegralSequence(string[] fieldCodes)
	{
		for (int i = 1; i < fieldCodes.Length; i++)
		{
			if (fieldCodes[i].TrimEnd().Equals("i", StringComparison.CurrentCultureIgnoreCase) && i == 1)
			{
				continue;
			}
			if (fieldCodes[i].StartsWith("su", StringComparison.CurrentCultureIgnoreCase) || fieldCodes[i].StartsWith("pr", StringComparison.CurrentCultureIgnoreCase) || fieldCodes[i].StartsWith("in", StringComparison.CurrentCultureIgnoreCase))
			{
				if (!IsOnlyAlphabets(fieldCodes[i].TrimEnd()))
				{
					return true;
				}
				continue;
			}
			if (fieldCodes[i].StartsWith("fc", StringComparison.CurrentCultureIgnoreCase) || fieldCodes[i].StartsWith("vc", StringComparison.CurrentCultureIgnoreCase))
			{
				if (!IsOnlyAlphabets(fieldCodes[i].TrimEnd()))
				{
					return true;
				}
				if (i == fieldCodes.Length - 1 || (fieldCodes[i + 1] != "" && !HasManyCharacters(fieldCodes[i + 1])))
				{
					i++;
					continue;
				}
				return true;
			}
			return true;
		}
		return false;
	}

	private bool IsValidOverstrikeSwitch(string fieldCode)
	{
		if (HasParenthesisError(fieldCode))
		{
			return false;
		}
		string substringTill = GetSubstringTill(fieldCode, '(');
		string fieldCode2 = fieldCode.Substring(fieldCode.IndexOf(substringTill, StringComparison.Ordinal) + substringTill.Length);
		if (!IsCorrectOverstrikeSwitchSequence(substringTill))
		{
			return false;
		}
		List<string> splittedFieldCodes = new List<string>();
		if (!IsValidEqFieldCode(splittedFieldCodes, fieldCode2))
		{
			return false;
		}
		return true;
	}

	private bool IsCorrectOverstrikeSwitchSequence(string overstrikeSwitchEqCode)
	{
		if (overstrikeSwitchEqCode.Contains("\\ "))
		{
			return false;
		}
		string[] array = overstrikeSwitchEqCode.Split('\\');
		for (int i = 1; i < array.Length; i++)
		{
			if (array[i].TrimEnd().Contains(" "))
			{
				return false;
			}
			if (!IsOnlyAlphabets(array[i].Trim()))
			{
				return false;
			}
			if (!array[i].StartsWith("al", StringComparison.CurrentCultureIgnoreCase) && !array[i].StartsWith("ar", StringComparison.CurrentCultureIgnoreCase) && !array[i].StartsWith("ac", StringComparison.CurrentCultureIgnoreCase) && !array[i].StartsWith("ad", StringComparison.CurrentCultureIgnoreCase) && (!array[i].TrimEnd().Equals("o", StringComparison.OrdinalIgnoreCase) || i != 1))
			{
				return false;
			}
		}
		return true;
	}

	private LayoutedEQFields LayoutOverstrikeSwitch(FontScriptType fontScriptType, string fieldCode, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		string substringTill = GetSubstringTill(fieldCode, '(');
		string overstrikeElement = fieldCode.Substring(fieldCode.IndexOf(substringTill, StringComparison.Ordinal) + substringTill.Length);
		string alignment = "center";
		GetAlignmentProperty(substringTill, ref alignment);
		LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
		layoutedEQFields.SwitchType = LayoutedEQFields.EQSwitchType.Overstrike;
		GenerateOverstrikeSwitch(fontScriptType, layoutedEQFields, overstrikeElement, alignment, dc, charFormat, xPosition, yPosition);
		return layoutedEQFields;
	}

	private void GenerateOverstrikeSwitch(FontScriptType fontScriptType, LayoutedEQFields overstrikeSwitch, string overstrikeElement, string alignment, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		string[] array = SplitElementsByComma(overstrikeElement);
		foreach (string fieldCode in array)
		{
			GenerateSwitch(fontScriptType, overstrikeSwitch, fieldCode, dc, charFormat.GetFontToRender(fontScriptType), charFormat, xPosition, yPosition);
		}
		UpdateltEqFieldsBounds(overstrikeSwitch);
		if (alignment == "center" || alignment == "right")
		{
			AlignOverstrikeElements(overstrikeSwitch, alignment);
		}
	}

	private void AlignOverstrikeElements(LayoutedEQFields overstrikeSwitch, string alignment)
	{
		if (!(alignment == "right"))
		{
			if (alignment == "center")
			{
				for (int i = 0; i < overstrikeSwitch.ChildEQFileds.Count; i++)
				{
					float num = overstrikeSwitch.Bounds.Width - overstrikeSwitch.ChildEQFileds[i].Bounds.Width;
					if (num > 0f)
					{
						ShiftEqFieldXPosition(overstrikeSwitch.ChildEQFileds[i], num / 2f);
					}
				}
			}
		}
		else
		{
			for (int j = 0; j < overstrikeSwitch.ChildEQFileds.Count; j++)
			{
				float num = overstrikeSwitch.Bounds.Width - overstrikeSwitch.ChildEQFileds[j].Bounds.Width;
				if (num > 0f)
				{
					ShiftEqFieldXPosition(overstrikeSwitch.ChildEQFileds[j], num);
				}
			}
		}
		UpdateltEqFieldsBounds(overstrikeSwitch);
	}

	private void GetAlignmentProperty(string overstrikeSwitchEqCode, ref string alignment)
	{
		string[] array = overstrikeSwitchEqCode.Split('\\');
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].StartsWith("al", StringComparison.CurrentCultureIgnoreCase) || array[i].StartsWith("ad", StringComparison.CurrentCultureIgnoreCase))
			{
				alignment = "left";
			}
			else if (array[i].StartsWith("ar", StringComparison.CurrentCultureIgnoreCase))
			{
				alignment = "right";
			}
			else if (array[i].StartsWith("ac", StringComparison.CurrentCultureIgnoreCase))
			{
				alignment = "center";
			}
		}
	}

	private bool IsValidArraySwitch(string fieldCode)
	{
		if (HasParenthesisError(fieldCode))
		{
			return false;
		}
		string substringTill = GetSubstringTill(fieldCode, '(');
		string fieldCode2 = fieldCode.Substring(fieldCode.IndexOf(substringTill, StringComparison.Ordinal) + substringTill.Length);
		if (!IsCorrectArraySwitchSequence(substringTill))
		{
			return false;
		}
		List<string> splittedFieldCodes = new List<string>();
		if (!IsValidEqFieldCode(splittedFieldCodes, fieldCode2))
		{
			return false;
		}
		return true;
	}

	private bool IsCorrectArraySwitchSequence(string arrayEqCode)
	{
		if (arrayEqCode.Contains("\\ "))
		{
			return false;
		}
		string[] array = arrayEqCode.Split('\\');
		for (int i = 1; i < array.Length; i++)
		{
			if ((array[i].StartsWith("al", StringComparison.OrdinalIgnoreCase) || array[i].StartsWith("ar", StringComparison.OrdinalIgnoreCase) || array[i].StartsWith("ac", StringComparison.OrdinalIgnoreCase)) && IsOnlyAlphabets(array[i].Trim()))
			{
				continue;
			}
			if (array[i].StartsWith("co", StringComparison.OrdinalIgnoreCase))
			{
				if (!IsCorrectCodeFormat(array[i].Trim(), isAlsoNegative: true))
				{
					return false;
				}
			}
			else if (array[i].StartsWith("hs", StringComparison.OrdinalIgnoreCase) || array[i].StartsWith("vs", StringComparison.OrdinalIgnoreCase))
			{
				if (!IsCorrectCodeFormat(array[i].Trim(), isAlsoNegative: false))
				{
					return false;
				}
			}
			else if (!array[i].TrimEnd().Equals("a", StringComparison.OrdinalIgnoreCase) || i != 1)
			{
				return false;
			}
		}
		return true;
	}

	private LayoutedEQFields LayoutArraySwitch(FontScriptType fontScriptType, string fieldCode, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		string substringTill = GetSubstringTill(fieldCode, '(');
		string arrayElement = fieldCode.Substring(fieldCode.IndexOf(substringTill, StringComparison.Ordinal) + substringTill.Length);
		int numberOfColumns = 1;
		int verticalSpace = 0;
		int horizontalSpace = 0;
		StringAlignment alignment = StringAlignment.Center;
		GetArraySwitchProperties(substringTill, ref numberOfColumns, ref verticalSpace, ref horizontalSpace, ref alignment);
		LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
		layoutedEQFields.SwitchType = LayoutedEQFields.EQSwitchType.Array;
		layoutedEQFields.Alignment = alignment;
		GenerateArraySwitch(fontScriptType, layoutedEQFields, arrayElement, numberOfColumns, verticalSpace, horizontalSpace, alignment, dc, charFormat, xPosition, yPosition);
		return layoutedEQFields;
	}

	private void GetArraySwitchProperties(string arraySwitchEqCode, ref int numberOfColumns, ref int verticalSpace, ref int horizontalSpace, ref StringAlignment alignment)
	{
		string[] array = arraySwitchEqCode.Split('\\');
		for (int i = 1; i < array.Length; i++)
		{
			if (array[i].Trim().StartsWith("co", StringComparison.CurrentCultureIgnoreCase))
			{
				numberOfColumns = GetValueFromstring(array[i].Trim());
				if (numberOfColumns <= 0)
				{
					numberOfColumns = 1;
				}
			}
			else if (array[i].Trim().StartsWith("vs", StringComparison.CurrentCultureIgnoreCase))
			{
				verticalSpace = GetValueFromstring(array[i].Trim());
				if (verticalSpace == int.MinValue)
				{
					verticalSpace = 0;
				}
			}
			else if (array[i].Trim().StartsWith("hs", StringComparison.CurrentCultureIgnoreCase))
			{
				horizontalSpace = GetValueFromstring(array[i].Trim());
				if (horizontalSpace == int.MinValue)
				{
					horizontalSpace = 0;
				}
			}
			else if (array[i].Trim().StartsWith("al", StringComparison.CurrentCultureIgnoreCase) || array[i].Trim().StartsWith("ar", StringComparison.CurrentCultureIgnoreCase))
			{
				GetAlignmentForArraySwitch(array[i].Trim(), ref alignment);
			}
		}
	}

	private void GetAlignmentForArraySwitch(string alignmentCode, ref StringAlignment alignment)
	{
		string text = alignmentCode.ToLower();
		if (!(text == "al"))
		{
			if (text == "ar")
			{
				alignment = StringAlignment.Far;
			}
		}
		else
		{
			alignment = StringAlignment.Near;
		}
	}

	private void GenerateArraySwitch(FontScriptType fontScriptType, LayoutedEQFields arraySwitch, string arrayElement, int numberOfColumns, int verticalSpace, int horizontalSpace, StringAlignment alignment, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		GenerateArraySwitchElements(fontScriptType, arraySwitch, arrayElement, numberOfColumns, verticalSpace, horizontalSpace, alignment, dc, charFormat, xPosition, yPosition);
		int rows = 0;
		int columns = 0;
		GetRowsAndColumnsCount(arraySwitch, ref rows, ref columns);
		SetColumnWidth(arraySwitch, rows, columns);
		SetYForArrayElements(arraySwitch, dc, rows, columns);
		UpdateltEqFieldsBounds(arraySwitch);
		AlignArraySwitch(arraySwitch, dc, charFormat.GetFontToRender(fontScriptType), fontScriptType);
	}

	private void GenerateArraySwitchElements(FontScriptType fontScriptType, LayoutedEQFields arraySwitch, string arrayElement, int numberOfColumns, int verticalSpace, int horizontalSpace, StringAlignment alignment, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		string[] array = SplitElementsByComma(arrayElement);
		int num = 0;
		int num2 = 0;
		LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
		layoutedEQFields.SwitchType = LayoutedEQFields.EQSwitchType.Array;
		layoutedEQFields.Alignment = alignment;
		for (int i = 0; i < array.Length; i++)
		{
			GenerateSwitch(fontScriptType, layoutedEQFields, array[i], dc, charFormat.GetFontToRender(fontScriptType), charFormat, xPosition, yPosition);
			RectangleF bounds = layoutedEQFields.ChildEQFileds[num2].Bounds;
			layoutedEQFields.ChildEQFileds[num2].Bounds = new RectangleF(bounds.X, bounds.Y, bounds.Width + (float)horizontalSpace, bounds.Height + (float)verticalSpace);
			num2++;
			if (num2 % numberOfColumns == 0 || i == array.Length - 1)
			{
				arraySwitch.ChildEQFileds.Add(layoutedEQFields);
				if (i + 1 < array.Length)
				{
					num++;
					num2 = 0;
					layoutedEQFields = new LayoutedEQFields();
				}
			}
		}
		UpdateltEqFieldsBounds(arraySwitch);
	}

	private void AlignArraySwitch(LayoutedEQFields arraySwitch, DrawingContext dc, DocGen.Drawing.Font font, FontScriptType scriptType)
	{
		float num = 0f - dc.GetAscent(font, scriptType) / 2f - arraySwitch.Bounds.Height / 2f;
		dc.ShiftEqFieldYPosition(arraySwitch, num - arraySwitch.Bounds.Y);
	}

	private bool HasElementInArraySwitch(LayoutedEQFields arraySwitch, int row, int column)
	{
		if (row < arraySwitch.ChildEQFileds.Count && column < arraySwitch.ChildEQFileds[row].ChildEQFileds.Count)
		{
			return true;
		}
		return false;
	}

	private void GetRowsAndColumnsCount(LayoutedEQFields arraySwitch, ref int rows, ref int columns)
	{
		rows = arraySwitch.ChildEQFileds.Count;
		columns = 1;
		if (rows > 0)
		{
			columns = arraySwitch.ChildEQFileds[0].ChildEQFileds.Count;
		}
	}

	private void SetYForArrayElements(LayoutedEQFields arraySwitch, DrawingContext dc, int rows, int columns)
	{
		for (int i = 1; i < rows; i++)
		{
			float maximumBottom = GetMaximumBottom(arraySwitch.ChildEQFileds[i - 1]);
			for (int j = 0; j < columns; j++)
			{
				if (HasElementInArraySwitch(arraySwitch, i, j))
				{
					LayoutedEQFields layoutedEQFields = arraySwitch.ChildEQFileds[i].ChildEQFileds[j];
					float num = 0f - GetTopMostY(layoutedEQFields);
					float num2 = maximumBottom - num;
					dc.ShiftEqFieldYPosition(layoutedEQFields, num2 - layoutedEQFields.Bounds.Y);
					float num3 = arraySwitch.ChildEQFileds[i - 1].ChildEQFileds[j].Bounds.Bottom - layoutedEQFields.Bounds.Y;
					if (num3 > 0f)
					{
						dc.ShiftEqFieldYPosition(arraySwitch.ChildEQFileds[i], num3);
					}
					UpdateltEqFieldsBounds(arraySwitch.ChildEQFileds[i]);
				}
			}
		}
	}

	private void SetColumnWidth(LayoutedEQFields arraySwitch, int rows, int columns)
	{
		for (int i = 0; i < columns; i++)
		{
			float maximumColumnWidth = GetMaximumColumnWidth(arraySwitch, i);
			for (int j = 0; j < rows; j++)
			{
				if (HasElementInArraySwitch(arraySwitch, j, i))
				{
					RectangleF bounds = arraySwitch.ChildEQFileds[j].ChildEQFileds[i].Bounds;
					float num = maximumColumnWidth - bounds.Width;
					if (num > 0f)
					{
						arraySwitch.ChildEQFileds[j].ChildEQFileds[i].Bounds = new RectangleF(bounds.X, bounds.Y, bounds.Width + num, bounds.Height);
					}
				}
			}
			if (i > 0 && HasElementInArraySwitch(arraySwitch, 0, i))
			{
				SetXforElementsInColumn(arraySwitch, i, rows, arraySwitch.ChildEQFileds[0].ChildEQFileds[i - 1].Bounds.Right);
			}
		}
		UpdateltEqFieldsBounds(arraySwitch);
	}

	private void SetXforElementsInColumn(LayoutedEQFields arraySwitch, int columnIndex, int rows, float xposition)
	{
		for (int i = 0; i < rows; i++)
		{
			if (HasElementInArraySwitch(arraySwitch, i, columnIndex))
			{
				LayoutedEQFields layoutedEQFields = arraySwitch.ChildEQFileds[i].ChildEQFileds[columnIndex];
				ShiftEqFieldXPosition(layoutedEQFields, xposition - layoutedEQFields.Bounds.X);
				UpdateltEqFieldsBounds(arraySwitch.ChildEQFileds[i]);
			}
		}
	}

	private float GetMaximumColumnWidth(LayoutedEQFields arraySwitch, int columnIndex)
	{
		float num = 0f;
		int rows = 0;
		int columns = 0;
		GetRowsAndColumnsCount(arraySwitch, ref rows, ref columns);
		for (int i = 0; i < rows; i++)
		{
			if (HasElementInArraySwitch(arraySwitch, i, columnIndex))
			{
				float width = arraySwitch.ChildEQFileds[i].ChildEQFileds[columnIndex].Bounds.Width;
				num = ((num < width) ? width : num);
			}
		}
		return num;
	}

	private bool IsValidDisplaceSwitch(string fieldCode)
	{
		if (HasParenthesisError(fieldCode))
		{
			return false;
		}
		string substringTill = GetSubstringTill(fieldCode, '(');
		string fieldCode2 = fieldCode.Substring(fieldCode.IndexOf(substringTill, StringComparison.Ordinal) + substringTill.Length);
		if (substringTill.Contains("\\ "))
		{
			return false;
		}
		string[] array = substringTill.Split('\\');
		for (int i = 1; i < array.Length; i++)
		{
			if (array[i].StartsWith("li", StringComparison.OrdinalIgnoreCase) && IsOnlyAlphabets(array[i].Trim()))
			{
				continue;
			}
			if (array[i].StartsWith("fo", StringComparison.OrdinalIgnoreCase) || array[i].StartsWith("ba", StringComparison.OrdinalIgnoreCase))
			{
				if (!IsCorrectCodeFormat(array[i].Trim(), isAlsoNegative: true))
				{
					return false;
				}
			}
			else if (!array[i].TrimEnd().Equals("d", StringComparison.OrdinalIgnoreCase) || i != 1)
			{
				return false;
			}
		}
		List<string> splittedFieldCodes = new List<string>();
		if (!IsValidEqFieldCode(splittedFieldCodes, fieldCode2))
		{
			return false;
		}
		return true;
	}

	private LayoutedEQFields LayoutDisplaceSwitch(FontScriptType fontScriptType, string fieldCode, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		string substringTill = GetSubstringTill(fieldCode, '(');
		string displaceElement = fieldCode.Substring(fieldCode.IndexOf(substringTill, StringComparison.Ordinal) + substringTill.Length);
		int shiftValue = 0;
		bool hasLine = false;
		GetDisplaceSwitchProperties(substringTill, ref shiftValue, ref hasLine);
		LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
		layoutedEQFields.SwitchType = LayoutedEQFields.EQSwitchType.Displace;
		GenerateDisplaceSwitch(fontScriptType, layoutedEQFields, displaceElement, shiftValue, hasLine, dc, charFormat, xPosition, yPosition);
		return layoutedEQFields;
	}

	private void GenerateDisplaceSwitch(FontScriptType fontScriptType, LayoutedEQFields displaceSwitch, string displaceElement, int shiftValue, bool hasLine, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		string[] array = SplitElementsByComma(displaceElement);
		foreach (string fieldCode in array)
		{
			GenerateSwitch(fontScriptType, displaceSwitch, fieldCode, dc, charFormat.GetFontToRender(fontScriptType), charFormat, xPosition, yPosition);
			xPosition = displaceSwitch.Bounds.Right;
		}
		ShiftDisplaceSwitch(fontScriptType, displaceSwitch, shiftValue, hasLine, dc, charFormat, xPosition, yPosition);
	}

	private void ShiftDisplaceSwitch(FontScriptType fontScriptType, LayoutedEQFields displaceSwitch, int shiftValue, bool hasLine, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		if (hasLine)
		{
			GenerateLineInDisplaceSwitch(fontScriptType, displaceSwitch, dc, charFormat, displaceSwitch.Bounds.Right, displaceSwitch.Bounds.Bottom, -shiftValue);
		}
		if (shiftValue > 0)
		{
			TextEQField textEQField = new TextEQField();
			GenerateSpaceForWidth(fontScriptType, textEQField, shiftValue, dc, charFormat, xPosition, yPosition);
			ShiftEqFieldXPosition(displaceSwitch, shiftValue);
			SetZeroWidth(displaceSwitch);
			displaceSwitch.ChildEQFileds.Add(textEQField);
			UpdateltEqFieldsBounds(displaceSwitch);
			displaceSwitch.Bounds = new RectangleF(displaceSwitch.Bounds.X, displaceSwitch.Bounds.Y, shiftValue, displaceSwitch.Bounds.Height);
		}
		else
		{
			ShiftEqFieldXPosition(displaceSwitch, shiftValue);
			SetZeroWidth(displaceSwitch);
		}
	}

	private void GenerateLineInDisplaceSwitch(FontScriptType fontScriptType, LayoutedEQFields displaceSwitch, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition, int length)
	{
		LineEQField lineEQField = new LineEQField();
		float num = dc.MeasureString(" ", charFormat.GetFontToRender(fontScriptType), null, charFormat, isMeasureFromTabList: false, FontScriptType.English).Height - dc.GetAscent(charFormat.GetFontToRender(fontScriptType), fontScriptType);
		lineEQField.Point1 = new PointF(xPosition, yPosition - num);
		lineEQField.Point2 = new PointF(lineEQField.Point1.X + (float)length, lineEQField.Point1.Y);
		displaceSwitch.ChildEQFileds.Add(lineEQField);
	}

	private void SetZeroWidth(LayoutedEQFields ltEqFields)
	{
		if (ltEqFields is TextEQField || ltEqFields is LineEQField)
		{
			ltEqFields.Bounds = new RectangleF(ltEqFields.Bounds.X, ltEqFields.Bounds.Y, 0f, ltEqFields.Bounds.Height);
		}
		else if (ltEqFields != null)
		{
			ltEqFields.Bounds = new RectangleF(ltEqFields.Bounds.X, ltEqFields.Bounds.Y, 0f, ltEqFields.Bounds.Height);
			for (int i = 0; i < ltEqFields.ChildEQFileds.Count; i++)
			{
				SetZeroWidth(ltEqFields.ChildEQFileds[i]);
			}
		}
	}

	private void GenerateSpaceForWidth(FontScriptType fontScriptType, TextEQField ltEqField, float maxWidth, DrawingContext dc, WCharacterFormat charFormat, float xPosition, float yPosition)
	{
		string text = "";
		while (maxWidth > ltEqField.Bounds.Width)
		{
			ltEqField.Text = text;
			ltEqField.Bounds = new RectangleF(xPosition, yPosition, dc.MeasureString(ltEqField.Text, charFormat.GetFontToRender(fontScriptType), null, charFormat, isMeasureFromTabList: false, FontScriptType.English).Width, dc.MeasureString(ltEqField.Text, charFormat.GetFontToRender(fontScriptType), null, charFormat, isMeasureFromTabList: false, FontScriptType.English).Height);
			text += " ";
		}
		float ascent = dc.GetAscent(charFormat.GetFontToRender(fontScriptType), fontScriptType);
		dc.ShiftEqFieldYPosition(ltEqField, 0f - ascent);
	}

	private void GetDisplaceSwitchProperties(string displaceSwitchEqCode, ref int shiftValue, ref bool hasLine)
	{
		string[] array = displaceSwitchEqCode.Split('\\');
		for (int i = 1; i < array.Length; i++)
		{
			if (array[i].Trim().StartsWith("fo", StringComparison.CurrentCultureIgnoreCase))
			{
				shiftValue = GetValueFromstring(array[i].Trim());
				if (shiftValue == int.MinValue)
				{
					shiftValue = 0;
				}
			}
			else if (array[i].Trim().StartsWith("ba", StringComparison.CurrentCultureIgnoreCase))
			{
				shiftValue = GetValueFromstring(array[i].Trim());
				if (shiftValue == int.MinValue)
				{
					shiftValue = 0;
				}
				shiftValue = -shiftValue;
			}
			else if (array[i].Trim().StartsWith("li", StringComparison.CurrentCultureIgnoreCase))
			{
				hasLine = true;
			}
		}
	}

	private bool IsCorrectCodeFormat(string eqCode, bool isAlsoNegative)
	{
		eqCode = eqCode.Trim();
		string valuePart = "";
		SplitTextAndInteger(eqCode, ref valuePart);
		if (valuePart != "")
		{
			if (!int.TryParse(valuePart, out var result))
			{
				return false;
			}
			if (!isAlsoNegative && result < 0)
			{
				return false;
			}
			if (result > 0 && valuePart.Trim().Contains("+"))
			{
				return false;
			}
		}
		return true;
	}

	private void SplitTextAndInteger(string originalString, ref string valuePart)
	{
		string text = "";
		for (int i = 0; i < originalString.Length; i++)
		{
			char c = originalString[i];
			if (char.IsLetter(c))
			{
				text += c;
				continue;
			}
			valuePart = originalString.Substring(originalString.IndexOf(text, StringComparison.Ordinal) + text.Length).TrimStart();
			break;
		}
	}

	private float GetFontSizeFromHeight(string text, ref float maxHeight, DrawingContext dc, WCharacterFormat charFormat)
	{
		maxHeight += maxHeight * 0.25f;
		float num = 11f;
		float num2 = 0f;
		while (num2 < maxHeight)
		{
			num2 = dc.MeasureString(text, base.Document.FontSettings.GetFont("Symbol", num, FontStyle.Regular, FontScriptType.English), null, charFormat, isMeasureFromTabList: false, FontScriptType.English).Height;
			if (num2 < maxHeight)
			{
				num += 0.1f;
			}
		}
		return num;
	}

	private void EQFieldVerticalAlignment(LayoutedEQFields movableEqField, LayoutedEQFields standardEqField, DrawingContext dc)
	{
		float num = standardEqField.Bounds.Top - movableEqField.Bounds.Top;
		if (num != 0f)
		{
			dc.ShiftEqFieldYPosition(movableEqField, num);
		}
		float num2 = movableEqField.Bounds.Height - standardEqField.Bounds.Height;
		dc.ShiftEqFieldYPosition(movableEqField, 0f - num2 / 2f);
	}

	private char GetSeparator()
	{
		return CultureInfo.CurrentCulture.TextInfo.ListSeparator[0];
	}

	private string[] SplitElementsByComma(string element)
	{
		element = element.Trim();
		int num = 1;
		List<string> list = new List<string>();
		List<int> indexOfBackSlashes = GetIndexOfBackSlashes(element);
		string text = "";
		for (int i = 1; i <= element.Length - 2; i++)
		{
			if (element[i] == '(' && !indexOfBackSlashes.Contains(i - 1))
			{
				num++;
			}
			else if (element[i] == ')' && !indexOfBackSlashes.Contains(i - 1))
			{
				num--;
			}
			if (element[i] == GetSeparator() && !indexOfBackSlashes.Contains(i - 1) && num == 1)
			{
				list.Add(text);
				text = "";
			}
			else
			{
				text += element[i];
			}
		}
		list.Add(text);
		return list.ToArray();
	}

	private void UpdateltEqFieldsBounds(LayoutedEQFields ltEQfields)
	{
		float num = 0f - GetTopMostY(ltEQfields);
		float maximumBottom = GetMaximumBottom(ltEQfields);
		float leftMostX = GetLeftMostX(ltEQfields);
		ltEQfields.Bounds = new RectangleF(leftMostX, 0f - num, 0f - leftMostX + GetMaximumRight(ltEQfields), num + maximumBottom);
	}

	private void ShiftEqFieldXPosition(LayoutedEQFields layoutedEqFields, float shiftX)
	{
		if (layoutedEqFields is TextEQField)
		{
			TextEQField textEQField = layoutedEqFields as TextEQField;
			textEQField.Bounds = new RectangleF(textEQField.Bounds.X + shiftX, textEQField.Bounds.Y, textEQField.Bounds.Width, textEQField.Bounds.Height);
		}
		else if (layoutedEqFields is LineEQField)
		{
			LineEQField lineEQField = layoutedEqFields as LineEQField;
			lineEQField.Bounds = new RectangleF(lineEQField.Bounds.X + shiftX, lineEQField.Bounds.Y, lineEQField.Bounds.Width, lineEQField.Bounds.Height);
			lineEQField.Point1 = new PointF(lineEQField.Point1.X + shiftX, lineEQField.Point1.Y);
			lineEQField.Point2 = new PointF(lineEQField.Point2.X + shiftX, lineEQField.Point2.Y);
		}
		else
		{
			if (layoutedEqFields == null)
			{
				return;
			}
			layoutedEqFields.Bounds = new RectangleF(layoutedEqFields.Bounds.X + shiftX, layoutedEqFields.Bounds.Y, layoutedEqFields.Bounds.Width, layoutedEqFields.Bounds.Height);
			foreach (LayoutedEQFields childEQFiled in layoutedEqFields.ChildEQFileds)
			{
				ShiftEqFieldXPosition(childEQFiled, shiftX);
			}
		}
	}

	private bool HasAnySwitch(string fieldCode)
	{
		string[] array = new string[10] { "\\a", "\\b", "\\d", "\\f", "\\i", "\\o", "\\r", "\\s", "\\l", "\\x" };
		foreach (string value in array)
		{
			if (fieldCode.ToLower().Contains(value))
			{
				return true;
			}
		}
		return false;
	}

	private string ExtractSwitch(string fieldCode)
	{
		int num = 0;
		int num2 = 0;
		fieldCode = fieldCode.TrimStart();
		List<int> indexOfBackSlashes = GetIndexOfBackSlashes(fieldCode);
		for (int i = 0; i < fieldCode.Length; i++)
		{
			if (fieldCode[i] == '(' && !indexOfBackSlashes.Contains(i - 1))
			{
				num++;
			}
			else if (fieldCode[i] == ')' && !indexOfBackSlashes.Contains(i - 1))
			{
				num2++;
			}
			if (num == num2 && num != 0 && num2 != 0)
			{
				return fieldCode.Substring(0, i + 1);
			}
		}
		return fieldCode;
	}

	private void GetSplittedFieldCode(List<string> splittedFieldCodeSwitch, string fieldCode)
	{
		string switchBeforeText = GetSwitchBeforeText(fieldCode);
		if (switchBeforeText != "")
		{
			fieldCode = fieldCode.Substring(switchBeforeText.Length);
			splittedFieldCodeSwitch.Add(switchBeforeText);
		}
		string text = ExtractSwitch(fieldCode);
		string text2 = fieldCode.Substring(text.Length);
		if (text != "")
		{
			splittedFieldCodeSwitch.Add(text);
		}
		if (text2 != "" && HasAnySwitch(text2))
		{
			GetSplittedFieldCode(splittedFieldCodeSwitch, text2);
		}
		else if (text2 != "")
		{
			splittedFieldCodeSwitch.Add(text2);
		}
	}

	private float GetTopMostY(LayoutedEQFields ltEqFields)
	{
		float y = ltEqFields.Bounds.Y;
		for (int i = 0; i < ltEqFields.ChildEQFileds.Count; i++)
		{
			if (i == 0)
			{
				y = ltEqFields.ChildEQFileds[i].Bounds.Y;
			}
			else if (ltEqFields.ChildEQFileds[i].Bounds.Y < y)
			{
				y = ltEqFields.ChildEQFileds[i].Bounds.Y;
			}
		}
		return y;
	}

	private float GetLeftMostX(LayoutedEQFields ltEqFields)
	{
		float x = ltEqFields.Bounds.X;
		for (int i = 0; i < ltEqFields.ChildEQFileds.Count; i++)
		{
			if (i == 0)
			{
				x = ltEqFields.ChildEQFileds[i].Bounds.X;
			}
			else if (ltEqFields.ChildEQFileds[i].Bounds.X < x)
			{
				x = ltEqFields.ChildEQFileds[i].Bounds.X;
			}
		}
		return x;
	}

	private float GetMaximumRight(LayoutedEQFields ltEqFields)
	{
		float right = ltEqFields.Bounds.Right;
		for (int i = 0; i < ltEqFields.ChildEQFileds.Count; i++)
		{
			if (i == 0)
			{
				right = ltEqFields.ChildEQFileds[i].Bounds.Right;
			}
			else if (ltEqFields.ChildEQFileds[i].Bounds.Right > right)
			{
				right = ltEqFields.ChildEQFileds[i].Bounds.Right;
			}
		}
		return right;
	}

	private bool HasManyCharacters(string fieldcode)
	{
		if (StartsWithExt(fieldcode, " "))
		{
			return !(fieldcode.Trim() == "");
		}
		if (fieldcode.TrimEnd().Length > 1)
		{
			return true;
		}
		return false;
	}

	private string GetSubstringTill(string originalString, char delimeter)
	{
		string text = "";
		for (int i = 0; i < originalString.Length && (originalString[i] != delimeter || ((i <= 0 || originalString[i - 1] == '\\') && (i <= 1 || originalString[i - 1] != '\\' || originalString[i - 2] != '\\'))); i++)
		{
			text += originalString[i];
		}
		return text;
	}

	private bool HasSlashError(string fieldCode)
	{
		List<int> indexOfBackSlashes = GetIndexOfBackSlashes(fieldCode);
		char[] obj = new char[4] { '\0', '(', ')', '\\' };
		obj[0] = GetSeparator();
		char[] charArray = obj;
		foreach (int item in indexOfBackSlashes)
		{
			char c = fieldCode[item + 1];
			if (!IsExistInArray(charArray, char.ToLower(c)))
			{
				return true;
			}
		}
		return false;
	}

	private List<int> GetIndexOfBackSlashes(string fieldCode)
	{
		List<int> list = new List<int>();
		for (int num = fieldCode.IndexOf('\\'); num > -1; num = fieldCode.IndexOf('\\', num + 1))
		{
			if (list.Count == 0)
			{
				list.Add(num);
			}
			else if (list[list.Count - 1] != num - 1)
			{
				list.Add(num);
			}
		}
		return list;
	}

	private bool HasParenthesisError(string element)
	{
		int num = 0;
		int num2 = 0;
		List<int> indexOfBackSlashes = GetIndexOfBackSlashes(element);
		for (int i = 0; i < element.Length; i++)
		{
			if (element[i] == '(' && i == 0)
			{
				num++;
			}
			else if (element[i] == '(' && !indexOfBackSlashes.Contains(i - 1))
			{
				num++;
			}
			else if (element[i] == ')' && !indexOfBackSlashes.Contains(i - 1))
			{
				num2++;
			}
		}
		if (num > num2)
		{
			return true;
		}
		return false;
	}

	private bool IsOnlyAlphabets(string inputText)
	{
		for (int i = 0; i < inputText.Length; i++)
		{
			if (!char.IsLetter(inputText[i]))
			{
				return false;
			}
		}
		return true;
	}

	private string GetSwitchBeforeText(string eqFieldCode)
	{
		if (eqFieldCode.Contains("\\"))
		{
			List<int> indexOfBackSlashes = GetIndexOfBackSlashes(eqFieldCode);
			char[] charArray = new char[10] { 'a', 'b', 'd', 'f', 'i', 'o', 'r', 's', 'l', 'x' };
			foreach (int item in indexOfBackSlashes)
			{
				char c = eqFieldCode[item + 1];
				if (IsExistInArray(charArray, char.ToLower(c)))
				{
					return eqFieldCode.Substring(0, item);
				}
			}
			return "";
		}
		return eqFieldCode;
	}

	private string GetFirstOccurenceEqSwitch(string eqInstruction)
	{
		string[] array = new string[10] { "\\a", "\\b", "\\d", "\\f", "\\i", "\\o", "\\r", "\\s", "\\l", "\\x" };
		string text = "";
		for (int i = 0; i < eqInstruction.Length; i++)
		{
			text += eqInstruction[i];
			int num = text.LastIndexOf('\\');
			if (num != text.Length - 2 || num == -1)
			{
				continue;
			}
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				string text3 = text.Substring(0, text.LastIndexOf('\\'));
				if (text.ToLower().Contains(text2) && !text3.EndsWith("\\"))
				{
					return text2;
				}
			}
		}
		return "";
	}

	private string RemoveEQText(string fieldCode)
	{
		string text = fieldCode;
		if (fieldCode.TrimStart().StartsWith("EQ", StringComparison.OrdinalIgnoreCase))
		{
			text = fieldCode.Substring(fieldCode.IndexOf("EQ", StringComparison.Ordinal) + "EQ".Length).TrimStart();
		}
		if (StartsWithExt(text.Trim(), "\\*"))
		{
			string switchBeforeText = GetSwitchBeforeText(text);
			text = text.Substring(switchBeforeText.Length);
		}
		return text;
	}

	private string ReplaceSymbols(string text)
	{
		List<int> indexOfBackSlashes = GetIndexOfBackSlashes(text);
		string text2 = "";
		for (int i = 0; i < text.Length; i++)
		{
			if (!indexOfBackSlashes.Contains(i))
			{
				text2 += text[i];
			}
		}
		return text2;
	}

	private bool IsExistInArray(char[] charArray, char searchingChar)
	{
		for (int i = 0; i < charArray.Length; i++)
		{
			if (charArray[i] == searchingChar)
			{
				return true;
			}
		}
		return false;
	}

	internal WCharacterFormat GetCharacterFormatValue()
	{
		if (FormattingString.ToUpper().Contains("\\* MERGEFORMAT"))
		{
			if (FieldSeparator != null && FieldSeparator.NextSibling is WTextRange)
			{
				return (FieldSeparator.NextSibling as WTextRange).CharacterFormat;
			}
		}
		else if (FieldType == FieldType.FieldNumPages || FieldType == FieldType.FieldSectionPages || FieldType == FieldType.FieldPage)
		{
			return GetFirstFieldCodeItem().CharacterFormat;
		}
		return base.CharacterFormat;
	}

	internal WCharacterFormat GetCharacterFormatValue(int paraItemIndex)
	{
		int num = Range.Items.IndexOf(FieldSeparator) + paraItemIndex;
		if (num < Range.Items.IndexOf(FieldEnd))
		{
			if (FieldSeparator != null && Range.Count >= num && Range.InnerList[num] is WTextRange)
			{
				return (Range.InnerList[num] as WTextRange).CharacterFormat;
			}
			if (FieldSeparator != null && Range.InnerList[num] is WFieldMark && (Range.InnerList[num] as WFieldMark).Type == FieldMarkType.FieldEnd)
			{
				return (Range.InnerList[num - 1] as WTextRange).CharacterFormat;
			}
		}
		else if (FieldEnd != null && FieldEnd.PreviousSibling is WTextRange)
		{
			return (FieldEnd.PreviousSibling as WTextRange).CharacterFormat;
		}
		return base.CharacterFormat;
	}

	internal FontScriptType GetFontScriptType()
	{
		if (FormattingString.ToUpper().Contains("\\* MERGEFORMAT"))
		{
			if (FieldSeparator != null && FieldSeparator.NextSibling is WTextRange)
			{
				return (FieldSeparator.NextSibling as WTextRange).ScriptType;
			}
		}
		else if (FieldType == FieldType.FieldNumPages || FieldType == FieldType.FieldSectionPages || FieldType == FieldType.FieldPage)
		{
			return GetFirstFieldCodeItem().ScriptType;
		}
		return FontScriptType.English;
	}

	private WTextRange GetFirstFieldCodeItem()
	{
		for (IEntity nextSibling = base.NextSibling; nextSibling != null; nextSibling = nextSibling.NextSibling)
		{
			if (nextSibling is WTextRange && !string.IsNullOrEmpty((nextSibling as WTextRange).Text))
			{
				if (StartsWithExt((nextSibling as WTextRange).Text, " ") || (GetOwnerTextBody(this) is HeaderFooter && (FieldType == FieldType.FieldPage || FieldType == FieldType.FieldNumPages)))
				{
					return nextSibling as WTextRange;
				}
				return this;
			}
			if (nextSibling == FieldSeparator || nextSibling == FieldEnd)
			{
				return this;
			}
		}
		return this;
	}

	internal bool IsBookmarkCrossRefField(ref string bkName)
	{
		string[] array = InternalFieldCode.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length > 2 && InternalFieldCode.Contains("\\h"))
		{
			Bookmark bookmark = base.Document.Bookmarks.FindByName(array[1]);
			if (bookmark != null && bookmark.BookmarkStart != null && bookmark.BookmarkEnd != null)
			{
				bkName = bookmark.Name;
				return true;
			}
		}
		return false;
	}

	void IWidget.InitLayoutInfo()
	{
		m_layoutInfo = null;
	}

	void IWidget.InitLayoutInfo(IWidget widget)
	{
	}

	internal WTextRange GetCurrentTextRange()
	{
		if (FormattingString.ToUpper().Contains("\\* MERGEFORMAT"))
		{
			if (FieldSeparator != null && FieldSeparator.NextSibling is WTextRange)
			{
				return FieldSeparator.NextSibling as WTextRange;
			}
		}
		else if (FieldType == FieldType.FieldPage || FieldType == FieldType.FieldNumPages || FieldType == FieldType.FieldSectionPages)
		{
			return GetFirstFieldCodeItem();
		}
		return this;
	}

	internal BookmarkStart GetBookmarkOfCrossRefField(ref bool isHiddenBookmark, bool isReturnHiddenBookmark)
	{
		string[] array = FieldCode.Split(new string[1] { "\\*" }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			for (int j = 1; j < array2.Length; j++)
			{
				if (!array2[j].Contains("\\"))
				{
					Bookmark bookmark = base.Document.Bookmarks.FindByName(array2[j]);
					if (bookmark == null)
					{
						return null;
					}
					isHiddenBookmark = StartsWithExt(bookmark.Name, "_");
					if ((!isHiddenBookmark || (isHiddenBookmark && isReturnHiddenBookmark)) && bookmark.BookmarkStart != null && bookmark.BookmarkEnd != null)
					{
						return bookmark.BookmarkStart;
					}
				}
			}
		}
		return null;
	}

	internal bool StartsWithExt(string text, string value)
	{
		return text.StartsWith(value);
	}

	internal void MarkNestedField()
	{
		for (int i = 0; i < Range.Items.Count; i++)
		{
			Entity entity = Range.Items[i] as Entity;
			if (entity is WFieldMark && (entity as WFieldMark).Type == FieldMarkType.FieldSeparator && (entity as WFieldMark).ParentField == this)
			{
				break;
			}
			if (entity is WField)
			{
				(entity as WField).IsNestedField = true;
			}
			else
			{
				MarkAsNestedField(entity);
			}
		}
	}

	private void MarkAsNestedField(Entity entity)
	{
		if (entity is WParagraph)
		{
			(entity as WParagraph).ChildEntities.OfType<WField>().ToList().ForEach(delegate(WField pItem)
			{
				pItem.IsNestedField = true;
			});
			return;
		}
		if (entity is WTable)
		{
			foreach (WTableRow row in (entity as WTable).Rows)
			{
				foreach (WTableCell cell in row.Cells)
				{
					foreach (Entity childEntity in cell.ChildEntities)
					{
						MarkAsNestedField(childEntity);
					}
				}
			}
			return;
		}
		if (!(entity is IBlockContentControl))
		{
			return;
		}
		foreach (Entity childEntity2 in (entity as IBlockContentControl).ChildEntities)
		{
			MarkAsNestedField(childEntity2);
		}
	}

	internal StringBuilder GetAsString(bool traverseTillSeparator)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		int i = 0;
		stringBuilder.Append('\u0013');
		for (int j = 0; j < Range.Count; j++)
		{
			if (Range.Items[j] is WFieldMark)
			{
				WFieldMark wFieldMark = Range.Items[j] as WFieldMark;
				if ((traverseTillSeparator && wFieldMark.Type == FieldMarkType.FieldSeparator && wFieldMark.ParentField == this) || wFieldMark.Type == FieldMarkType.FieldEnd)
				{
					break;
				}
			}
			else if (Range.Items[j] is ParagraphItem)
			{
				stringBuilder.Append(base.OwnerParagraph.GetAsString(Range.Items[j] as ParagraphItem));
				if (Range.Items[j] is WField wField)
				{
					if (Range.Items.Contains(wField.FieldEnd))
					{
						j = Range.Items.IndexOf(wField.FieldEnd);
						continue;
					}
					j = Range.Items.IndexOf(wField.FieldEnd.OwnerParagraph) - 1;
					i = wField.FieldEnd.Index + 1;
				}
			}
			else if (Range.Items[j] is WParagraph)
			{
				WParagraph wParagraph = Range.Items[j] as WParagraph;
				if (!flag)
				{
					flag = true;
					if (traverseTillSeparator)
					{
						stringBuilder.Append("\r\n");
					}
				}
				for (; i < wParagraph.Items.Count; i++)
				{
					if (wParagraph.Items[i] is WFieldMark)
					{
						WFieldMark wFieldMark2 = wParagraph.Items[i] as WFieldMark;
						if ((traverseTillSeparator && wFieldMark2.Type == FieldMarkType.FieldSeparator && wFieldMark2.ParentField == this) || wFieldMark2.Type == FieldMarkType.FieldEnd)
						{
							stringBuilder.Append('\u0013');
							return stringBuilder;
						}
					}
					else
					{
						if (wParagraph.Items[i] == null)
						{
							continue;
						}
						stringBuilder.Append(base.OwnerParagraph.GetAsString(wParagraph.Items[i]));
						WField wField2 = wParagraph.Items[i] as WField;
						WParagraph wParagraph2 = null;
						if (wField2 != null)
						{
							wParagraph2 = wField2.FieldEnd.OwnerParagraph;
							if (Range.Items.Contains(wParagraph2))
							{
								j = Range.Items.IndexOf(wParagraph2);
								wParagraph = wParagraph2;
								i = wParagraph.Items.IndexOf(wField2.FieldEnd);
							}
						}
					}
				}
				i = 0;
				if (traverseTillSeparator)
				{
					stringBuilder.Append("\r\n");
				}
			}
			else if (Range.Items[j] is WTable)
			{
				stringBuilder.Append((Range.Items[j] as WTable).GetAsString());
			}
			else if (Range.Items[j] is BlockContentControl)
			{
				stringBuilder.Append((Range.Items[j] as BlockContentControl).GetAsString());
			}
		}
		stringBuilder.Append('\u0013');
		return stringBuilder;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

public abstract class FormatBase : XDLSSerializableBase
{
	private const byte MAXPARENTLEVEL = 4;

	private const byte MAXKEY = 128;

	protected Dictionary<int, object> m_propertiesHash;

	protected Dictionary<int, object> m_oldPropertiesHash;

	private FormatBase m_baseFormat;

	private FormatBase m_parentFormat;

	private byte m_compositeKey;

	private byte m_parentLevel;

	private byte m_bFlags = 1;

	internal SinglePropertyModifierArray m_unParsedSprms;

	internal SinglePropertyModifierArray m_sprms;

	internal List<Revision> m_revisions;

	internal List<Revision> m_clonedRevisions;

	internal bool IsDefault
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			if (!value)
			{
				MarkNoDefault();
			}
		}
	}

	internal bool IsFormattingChange
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

	internal Dictionary<int, object> PropertiesHash
	{
		get
		{
			if (m_propertiesHash == null)
			{
				m_propertiesHash = new Dictionary<int, object>();
			}
			return m_propertiesHash;
		}
	}

	internal Dictionary<int, object> OldPropertiesHash
	{
		get
		{
			if (m_oldPropertiesHash == null)
			{
				m_oldPropertiesHash = new Dictionary<int, object>();
			}
			return m_oldPropertiesHash;
		}
	}

	internal FormatBase BaseFormat
	{
		get
		{
			return m_baseFormat;
		}
		set
		{
			m_baseFormat = value;
		}
	}

	protected object this[int key]
	{
		get
		{
			int fullKey = GetFullKey(key);
			object obj = null;
			obj = (PropertiesHash.ContainsKey(fullKey) ? PropertiesHash[fullKey] : GetDefComposite(key));
			if (obj == null && this is WParagraphFormat && (key == 2 || key == 5))
			{
				WListFormat wListFormat = null;
				if ((this as WParagraphFormat).OwnerBase is WParagraph)
				{
					wListFormat = ((this as WParagraphFormat).OwnerBase as WParagraph).ListFormat;
				}
				if (wListFormat != null && wListFormat.IsEmptyList)
				{
					return 0f;
				}
				if (wListFormat != null && wListFormat.CurrentListLevel != null && wListFormat.LFOStyleName == null)
				{
					if (key == 2 && wListFormat.CurrentListLevel.ParagraphFormat.PropertiesHash.ContainsKey(2))
					{
						return wListFormat.CurrentListLevel.ParagraphFormat.PropertiesHash[fullKey];
					}
					if (key == 5 && wListFormat.CurrentListLevel.ParagraphFormat.PropertiesHash.ContainsKey(5))
					{
						return wListFormat.CurrentListLevel.ParagraphFormat.PropertiesHash[fullKey];
					}
				}
			}
			if (obj == null && this is WParagraphFormat && base.OwnerBase is WParagraph && (base.OwnerBase as WParagraph).IsInCell && base.OwnerBase.OwnerBase is WTableCell && (base.OwnerBase.OwnerBase as WTableCell).OwnerRow != null && (base.OwnerBase.OwnerBase as WTableCell).OwnerRow.OwnerTable != null && m_doc.Styles.FindByName((base.OwnerBase.OwnerBase as WTableCell).OwnerRow.OwnerTable.StyleName, StyleType.TableStyle) is WTableStyle { ParagraphFormat: not null } wTableStyle && wTableStyle.ParagraphFormat.m_propertiesHash != null && !IsBaseFormatContainsKey(key))
			{
				return wTableStyle.ParagraphFormat[key];
			}
			if (obj == null && BaseFormat != null && BaseFormat.m_propertiesHash != null)
			{
				obj = GetBaseFormatValue(key);
			}
			if (CheckCharacterStyle(key))
			{
				obj = (this as WCharacterFormat).CharStyle.CharacterFormat[key];
			}
			if (obj == null)
			{
				obj = GetDefValue(key);
			}
			return obj;
		}
		set
		{
			int fullKey = GetFullKey(key);
			if (IsFormattingChange)
			{
				OldPropertiesHash[fullKey] = value;
			}
			else
			{
				PropertiesHash[fullKey] = value;
			}
			IsDefault = false;
			OnChange(this, key);
		}
	}

	internal FormatBase ParentFormat => m_parentFormat;

	internal List<Revision> Revisions
	{
		get
		{
			if (m_revisions == null)
			{
				m_revisions = new List<Revision>();
			}
			return m_revisions;
		}
	}

	private bool IsBaseFormatContainsKey(int key)
	{
		for (WParagraphFormat wParagraphFormat = BaseFormat as WParagraphFormat; wParagraphFormat != null; wParagraphFormat = wParagraphFormat.BaseFormat as WParagraphFormat)
		{
			if (wParagraphFormat.PropertiesHash.ContainsKey(key))
			{
				return true;
			}
		}
		return false;
	}

	public FormatBase()
		: this(null)
	{
	}

	internal FormatBase(IWordDocument doc, bool isTextBox)
		: this(doc)
	{
		m_propertiesHash = null;
		m_baseFormat = null;
		m_parentFormat = null;
		m_sprms = null;
	}

	public FormatBase(IWordDocument doc)
		: this(doc, null)
	{
	}

	public FormatBase(IWordDocument doc, Entity owner)
		: base(doc as WordDocument, owner)
	{
		m_propertiesHash = new Dictionary<int, object>();
		m_oldPropertiesHash = new Dictionary<int, object>();
	}

	public FormatBase(FormatBase parentFormat, int parentKey)
		: this(null)
	{
		if (parentFormat.m_parentLevel + 1 >= 4)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (parentKey > 128)
		{
			throw new ArgumentOutOfRangeException("parentKey");
		}
		m_propertiesHash = parentFormat.PropertiesHash;
		m_compositeKey = (byte)parentKey;
		m_parentFormat = parentFormat;
		m_parentLevel = (byte)(parentFormat.m_parentLevel + 1);
	}

	public FormatBase(FormatBase parent, int parentKey, int parentOffset)
		: this(parent, parentKey)
	{
	}

	protected internal void ImportContainer(FormatBase format)
	{
		if (!(format is WParagraphFormat) && !(format is WCharacterFormat) && !(format is RowFormat))
		{
			CopyProperties(format);
		}
		EnsureComposites();
		IsDefault = false;
		ImportMembers(format);
	}

	protected virtual void ImportMembers(FormatBase format)
	{
	}

	internal virtual void ApplyBase(FormatBase baseFormat)
	{
		m_baseFormat = baseFormat;
	}

	public bool HasKey(int key)
	{
		if (PropertiesHash == null)
		{
			return false;
		}
		return PropertiesHash.ContainsKey(GetFullKey(key));
	}

	public bool HasBoolKey(int key)
	{
		if (PropertiesHash == null)
		{
			return false;
		}
		if (PropertiesHash.ContainsKey(key) && (bool)PropertiesHash[key])
		{
			return true;
		}
		return false;
	}

	public virtual void ClearFormatting()
	{
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
		}
		if (m_sprms != null)
		{
			m_sprms.Clear();
		}
	}

	protected abstract object GetDefValue(int key);

	protected virtual FormatBase GetDefComposite(int key)
	{
		return null;
	}

	protected virtual void OnChange(FormatBase format, int propKey)
	{
		if (m_parentFormat != null)
		{
			ParentFormat.OnChange(format, propKey);
		}
	}

	internal virtual bool HasValue(int propertyKey)
	{
		return false;
	}

	internal virtual int GetSprmOption(int propertyKey)
	{
		return int.MaxValue;
	}

	internal override void Close()
	{
		base.Close();
		if (m_propertiesHash != null)
		{
			m_propertiesHash.Clear();
			m_propertiesHash = null;
		}
		if (m_sprms != null)
		{
			m_sprms.Close();
			m_sprms = null;
		}
		if (m_unParsedSprms != null)
		{
			m_unParsedSprms.Close();
			m_unParsedSprms = null;
		}
		if (m_baseFormat != null)
		{
			m_baseFormat = null;
		}
		if (m_parentFormat != null)
		{
			m_parentFormat = null;
		}
		if (m_revisions != null)
		{
			m_revisions.Clear();
			m_revisions = null;
		}
	}

	protected internal virtual void EnsureComposites()
	{
	}

	protected void EnsureComposites(params int[] keys)
	{
		foreach (int key in keys)
		{
			FormatBase formatBase = null;
			int fullKey = GetFullKey(key);
			formatBase = ((PropertiesHash == null || !PropertiesHash.ContainsKey(fullKey)) ? GetDefComposite(key) : (PropertiesHash[fullKey] as FormatBase));
			formatBase.EnsureComposites();
			formatBase.IsDefault = false;
		}
	}

	protected int GetBaseKey(int key)
	{
		int num = m_compositeKey;
		if (m_parentLevel > 1)
		{
			num = m_parentFormat.GetFullKey(num);
		}
		return key - (num << 8);
	}

	protected int GetFullKey(int key)
	{
		if (key > 128)
		{
			throw new ArgumentOutOfRangeException("key");
		}
		int num = m_compositeKey;
		if (m_parentLevel > 1)
		{
			num = m_parentFormat.GetFullKey(num);
		}
		return (num << 8) + key;
	}

	protected FormatBase GetDefComposite(int key, FormatBase value)
	{
		int fullKey = GetFullKey(key);
		PropertiesHash[fullKey] = value;
		if (BaseFormat != null && BaseFormat.PropertiesHash != null)
		{
			FormatBase baseFormat = ((!BaseFormat.PropertiesHash.ContainsKey(fullKey)) ? (BaseFormat[fullKey] as FormatBase) : (BaseFormat.PropertiesHash[fullKey] as FormatBase));
			value.ApplyBase(baseFormat);
		}
		return value;
	}

	private void MarkNoDefault()
	{
		m_bFlags = (byte)((m_bFlags & 0xFEu) | 0u);
		if (m_parentFormat != null)
		{
			m_parentFormat.IsDefault = false;
		}
	}

	internal virtual void RemoveChanges()
	{
		if (m_sprms == null)
		{
			return;
		}
		int changeOption = GetChangeOption();
		if (changeOption == 0)
		{
			return;
		}
		SinglePropertyModifierRecord singlePropertyModifierRecord = m_sprms[changeOption];
		if (singlePropertyModifierRecord != null)
		{
			int num;
			for (num = m_sprms.Modifiers.IndexOf(singlePropertyModifierRecord); num < m_sprms.Modifiers.Count; num++)
			{
				m_sprms.Modifiers.RemoveAt(num);
				num--;
			}
			if (m_propertiesHash != null)
			{
				m_propertiesHash.Clear();
			}
		}
	}

	internal virtual void AcceptChanges()
	{
		if (m_sprms == null || m_sprms.Length == 0)
		{
			return;
		}
		int changeOption = GetChangeOption();
		if (changeOption == 0)
		{
			return;
		}
		SinglePropertyModifierRecord singlePropertyModifierRecord = m_sprms.TryGetSprm(changeOption);
		if (singlePropertyModifierRecord == null)
		{
			return;
		}
		int styleChangeOption = GetStyleChangeOption();
		if (m_sprms.Contain(styleChangeOption))
		{
			int num = m_sprms.Modifiers.IndexOf(singlePropertyModifierRecord);
			for (int i = 0; i <= num; i++)
			{
				m_sprms.Modifiers.RemoveAt(0);
			}
			if (this is WParagraphFormat)
			{
				if (m_sprms.Contain(17931))
				{
					m_sprms.RemoveValue(styleChangeOption);
				}
			}
			else
			{
				m_sprms.RemoveValue(styleChangeOption);
			}
		}
		else
		{
			int num2 = m_sprms.Modifiers.IndexOf(singlePropertyModifierRecord) + 1;
			List<SinglePropertyModifierRecord> list = null;
			if (num2 < m_sprms.Count)
			{
				list = new List<SinglePropertyModifierRecord>();
				int j = num2;
				for (int count = m_sprms.Count; j < count; j++)
				{
					list.Add(m_sprms.GetSprmByIndex(j));
				}
				foreach (SinglePropertyModifierRecord item in list)
				{
					m_sprms.RemoveValue(item.Options);
					m_sprms.Add(item);
				}
			}
			m_sprms.RemoveValue(changeOption);
		}
		PropertiesHash.Clear();
	}

	private int GetStyleChangeOption()
	{
		if (this is WCharacterFormat)
		{
			return 18992;
		}
		if (this is WParagraphFormat)
		{
			return 17920;
		}
		return 0;
	}

	private int GetChangeOption()
	{
		if (this is WCharacterFormat)
		{
			return 10883;
		}
		if (this is WParagraphFormat)
		{
			return 9828;
		}
		if (this is RowFormat)
		{
			return 13928;
		}
		return 0;
	}

	internal virtual void RemovePositioning()
	{
	}

	internal Stream CloneStream(Stream input)
	{
		MemoryStream memoryStream = new MemoryStream();
		byte[] array = new byte[input.Length];
		input.Seek(0L, SeekOrigin.Begin);
		int num = input.Read(array, 0, array.Length);
		if (num > 0)
		{
			memoryStream.Write(array, 0, num);
		}
		return memoryStream;
	}

	internal bool ComparePropertiesCount(FormatBase format)
	{
		if (PropertiesHash.Count > 0 || format.PropertiesHash.Count > 0)
		{
			if (PropertiesHash.Count != format.PropertiesHash.Count)
			{
				return false;
			}
			foreach (KeyValuePair<int, object> item in format.PropertiesHash)
			{
				if (PropertiesHash.ContainsKey(item.Key))
				{
					object obj = format.PropertiesHash[item.Key];
					if (obj is FormatBase)
					{
						if (!(PropertiesHash[item.Key] as FormatBase).Compare(obj as FormatBase))
						{
							return false;
						}
					}
					else if (!Compare(PropertiesHash[item.Key], obj))
					{
						return false;
					}
					continue;
				}
				return false;
			}
		}
		return true;
	}

	internal virtual bool Compare(FormatBase formatBase)
	{
		return false;
	}

	internal void CompareProperties(FormatBase format)
	{
		Dictionary<int, object> dictionary = new Dictionary<int, object>();
		if (format.Document.ComparisonOptions != null && format.Document.ComparisonOptions.DetectFormatChanges)
		{
			OldPropertiesHash.Clear();
			foreach (KeyValuePair<int, object> item in PropertiesHash)
			{
				if (IsNeedToConsideredKey(format, item.Key))
				{
					dictionary.Add(item.Key, item.Value);
				}
			}
		}
		PropertiesHash.Clear();
		if (format is WCharacterFormat || format is WParagraphFormat)
		{
			ImportContainer(format);
			CopyProperties(format);
		}
		else if (format is WListFormat || format is TableStyleCellProperties || format is TableStyleRowProperties || format is TableStyleTableProperties)
		{
			ImportContainer(format);
		}
		else if (format is RowFormat)
		{
			ImportContainer(format);
			(this as RowFormat).Paddings.ImportPaddings((format as RowFormat).Paddings);
		}
		else if (format is CellFormat)
		{
			ImportContainer(format);
			(this as CellFormat).Paddings.ImportPaddings((format as CellFormat).Paddings);
		}
		else if (format is WSectionFormat)
		{
			WSection section = base.OwnerBase as WSection;
			ImportContainer(format);
			CopyProperties(format);
			(this as WSectionFormat).PageSetup = (format as WSectionFormat).PageSetup.Clone();
			(this as WSectionFormat).m_columns = new ColumnCollection(section);
			(format as WSectionFormat).m_columns.CloneTo((this as WSectionFormat).m_columns);
		}
		foreach (KeyValuePair<int, object> item2 in dictionary)
		{
			OldPropertiesHash.Add(item2.Key, item2.Value);
		}
	}

	private bool IsNeedToConsideredKey(FormatBase format, int key)
	{
		if (format is WCharacterFormat)
		{
			if (key != 59)
			{
				return key != 60;
			}
			return false;
		}
		return true;
	}

	internal bool Compare(int propertyKey, FormatBase format)
	{
		if (PropertiesHash.ContainsKey(propertyKey) && format.PropertiesHash.ContainsKey(propertyKey))
		{
			if (!Compare(PropertiesHash[propertyKey], format.PropertiesHash[propertyKey]))
			{
				return false;
			}
		}
		else if ((!PropertiesHash.ContainsKey(propertyKey) && format.PropertiesHash.ContainsKey(propertyKey)) || (PropertiesHash.ContainsKey(propertyKey) && !format.PropertiesHash.ContainsKey(propertyKey)))
		{
			return false;
		}
		return true;
	}

	internal bool Compare(object value, object currentValue)
	{
		if ((value == null && currentValue != null) || (value != null && currentValue == null))
		{
			return false;
		}
		Type type = value.GetType();
		Type type2 = currentValue.GetType();
		if (type.Name != type2.Name)
		{
			return false;
		}
		switch (type.Name.ToLower())
		{
		case "single":
			if ((float)value != (float)currentValue)
			{
				return false;
			}
			break;
		case "boolean":
			if ((bool)value != (bool)currentValue)
			{
				return false;
			}
			break;
		case "toggleoperand":
			if ((ToggleOperand)value != (ToggleOperand)currentValue)
			{
				return false;
			}
			break;
		case "string":
			if ((string)value != (string)currentValue)
			{
				return false;
			}
			break;
		case "subsuperscript":
			if ((SubSuperScript)value != (SubSuperScript)currentValue)
			{
				return false;
			}
			break;
		case "numberspacingtype":
			if ((NumberSpacingType)value != (NumberSpacingType)currentValue)
			{
				return false;
			}
			break;
		case "color":
			if (GetARGBCode((Color)value) != GetARGBCode((Color)currentValue))
			{
				return false;
			}
			break;
		case "texturestyle":
			if ((TextureStyle)value != (TextureStyle)currentValue)
			{
				return false;
			}
			break;
		case "int16":
			if ((short)value != (short)currentValue)
			{
				return false;
			}
			break;
		case "outlinelevel":
			if ((OutlineLevel)value != (OutlineLevel)currentValue)
			{
				return false;
			}
			break;
		case "framewrapmode":
			if ((FrameWrapMode)value != (FrameWrapMode)currentValue)
			{
				return false;
			}
			break;
		case "horizontalalignment":
			if ((HorizontalAlignment)value != (HorizontalAlignment)currentValue)
			{
				return false;
			}
			break;
		case "linespacingrule":
			if ((LineSpacingRule)value != (LineSpacingRule)currentValue)
			{
				return false;
			}
			break;
		case "borderstyle":
			if ((BorderStyle)value != (BorderStyle)currentValue)
			{
				return false;
			}
			break;
		case "underlinestyle":
			if ((UnderlineStyle)value != (UnderlineStyle)currentValue)
			{
				return false;
			}
			break;
		default:
			if (type.IsEnum && !value.Equals(currentValue))
			{
				return false;
			}
			break;
		}
		return true;
	}

	private string GetARGBCode(Color color)
	{
		return color.A.ToString("X2") + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
	}

	internal void CopyProperties(FormatBase format)
	{
		int num = 0;
		int num2 = 0;
		if (!(format is RowFormat.TablePositioning))
		{
			num = format.m_compositeKey;
			num2 = format.m_parentLevel;
			if (num2 > 1)
			{
				num = format.m_parentFormat.GetFullKey(format.m_compositeKey);
			}
		}
		int[] array = new int[m_propertiesHash.Count];
		m_propertiesHash.Keys.CopyTo(array, 0);
		foreach (int key in array)
		{
			if (!(m_propertiesHash[key] is FormatBase) && (num == 0 || GetCompositeParentKey(key, num2) == num))
			{
				m_propertiesHash.Remove(key);
			}
		}
		IDictionaryEnumerator dictionaryEnumerator = format.PropertiesHash.GetEnumerator();
		while (dictionaryEnumerator.MoveNext())
		{
			if (!(dictionaryEnumerator.Value is FormatBase) && (num == 0 || GetCompositeParentKey((int)dictionaryEnumerator.Key, num2) == num))
			{
				m_propertiesHash.Add((int)dictionaryEnumerator.Key, dictionaryEnumerator.Value);
			}
		}
		array = new int[m_oldPropertiesHash.Count];
		m_oldPropertiesHash.Keys.CopyTo(array, 0);
		foreach (int key2 in array)
		{
			if (!(m_oldPropertiesHash[key2] is FormatBase) && (num == 0 || GetCompositeParentKey(key2, num2) == num))
			{
				m_oldPropertiesHash.Remove(key2);
			}
		}
		dictionaryEnumerator = format.OldPropertiesHash.GetEnumerator();
		while (dictionaryEnumerator.MoveNext())
		{
			if (!(dictionaryEnumerator.Value is FormatBase) && (!(base.OwnerBase is WTableRow) || !(format.OwnerBase is WTable)) && (num == 0 || GetCompositeParentKey((int)dictionaryEnumerator.Key, num2) == num))
			{
				m_oldPropertiesHash.Add((int)dictionaryEnumerator.Key, dictionaryEnumerator.Value);
			}
		}
		if (this is WListFormat && format is WListFormat)
		{
			(this as WListFormat).IsListRemoved = (format as WListFormat).IsListRemoved;
			(this as WListFormat).IsEmptyList = (format as WListFormat).IsEmptyList;
		}
	}

	private int GetCompositeParentKey(int key, int parentLevel)
	{
		int num = parentLevel;
		int num2 = (int)Math.Pow(2.0, num * 8) - 1;
		if (key > num2)
		{
			do
			{
				num++;
				num2 = (int)Math.Pow(2.0, num * 8) - 1;
			}
			while (key > num2);
			return key >> (num - parentLevel) * 8;
		}
		return key;
	}

	internal void UpdateProperties(FormatBase format)
	{
		m_propertiesHash = format.PropertiesHash;
	}

	internal void CopyFormat(FormatBase format)
	{
		foreach (KeyValuePair<int, object> item in format.PropertiesHash)
		{
			if (!(item.Value is Borders) && !(item.Value is Border) && !(item.Value is Paddings) && !(item.Value is RowFormat.TablePositioning))
			{
				if (PropertiesHash.ContainsKey(item.Key))
				{
					PropertiesHash[item.Key] = item.Value;
				}
				else
				{
					PropertiesHash.Add(item.Key, item.Value);
				}
			}
		}
	}

	private bool CheckCharacterStyle(int key)
	{
		int fullKey = GetFullKey(key);
		if (this is WCharacterFormat && !PropertiesHash.ContainsKey(fullKey) && (this as WCharacterFormat).CharStyle != null && (this as WCharacterFormat).CharStyle.CharacterFormat[key] != null && (this as WCharacterFormat).CharStyle.CharacterFormat.HasValue(key))
		{
			return true;
		}
		return false;
	}

	private object GetBaseFormatValue(int key)
	{
		object obj = BaseFormat[key];
		if ((this is WCharacterFormat && (this as WCharacterFormat).TableStyleCharacterFormat != null && !(obj is bool)) || (this is WParagraphFormat && (this as WParagraphFormat).TableStyleParagraphFormat != null))
		{
			FormatBase baseFormat = BaseFormat;
			int fullKey = GetFullKey(key);
			while (!baseFormat.PropertiesHash.ContainsKey(fullKey))
			{
				if (baseFormat.BaseFormat != null)
				{
					baseFormat = baseFormat.BaseFormat;
					continue;
				}
				if (this is WCharacterFormat)
				{
					return (this as WCharacterFormat).TableStyleCharacterFormat[key];
				}
				return (this as WParagraphFormat).TableStyleParagraphFormat[key];
			}
			if (this is WParagraphFormat && (this as WParagraphFormat).TableStyleParagraphFormat != null && (key == 2 || key == 5))
			{
				WListFormat wListFormat = null;
				if ((this as WParagraphFormat).OwnerBase is WParagraph)
				{
					wListFormat = ((this as WParagraphFormat).OwnerBase as WParagraph).ListFormat;
				}
				else if ((this as WParagraphFormat).OwnerBase is WParagraphStyle)
				{
					wListFormat = ((this as WParagraphFormat).OwnerBase as WParagraphStyle).ListFormat;
				}
				if (wListFormat != null && wListFormat.IsEmptyList)
				{
					return null;
				}
				return obj;
			}
			return obj;
		}
		if (this is WParagraphFormat && (key == 2 || key == 5))
		{
			WListFormat wListFormat2 = null;
			if ((this as WParagraphFormat).OwnerBase is WParagraph)
			{
				wListFormat2 = ((this as WParagraphFormat).OwnerBase as WParagraph).ListFormat;
			}
			else if ((this as WParagraphFormat).OwnerBase is WParagraphStyle)
			{
				wListFormat2 = ((this as WParagraphFormat).OwnerBase as WParagraphStyle).ListFormat;
			}
			if (wListFormat2 != null && wListFormat2.IsEmptyList)
			{
				return null;
			}
			return obj;
		}
		return obj;
	}

	internal DateTime ParseDTTM(int value)
	{
		DateTime result = default(DateTime);
		if (((value >> 29) & 7) <= 6)
		{
			int num = value & 0x3F;
			if (num > 59)
			{
				return result;
			}
			int num2 = (value >> 6) & 0x1F;
			if (num2 > 23)
			{
				return result;
			}
			int num3 = (value >> 11) & 0x1F;
			if (num3 > 31 || num3 == 0)
			{
				return result;
			}
			int num4 = (value >> 16) & 0xF;
			if (num4 > 12 || num4 == 0)
			{
				return result;
			}
			int num5 = (value >> 20) & 0x1FF;
			result = new DateTime(1900 + num5, num4, num3, num2, num, 0);
		}
		return result;
	}

	internal int GetDTTMIntValue(DateTime dt)
	{
		if (dt == DateTime.MinValue)
		{
			dt = new DateTime(1900, 1, 1, 0, 0, 0);
		}
		return Convert.ToInt32(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Empty + Convert.ToString((int)dt.DayOfWeek, 2).PadLeft(3, '0'), Convert.ToString(dt.Year - 1900, 2).PadLeft(9, '0')), Convert.ToString(dt.Month, 2).PadLeft(4, '0')), Convert.ToString(dt.Day, 2).PadLeft(5, '0')), Convert.ToString(dt.Hour, 2).PadLeft(5, '0')), Convert.ToString(dt.Minute, 2).PadLeft(6, '0')), 2);
	}

	internal bool CompareArray(byte[] buffer1, byte[] buffer2)
	{
		bool result = true;
		for (int i = 0; i < buffer1.Length; i++)
		{
			if (buffer1[i] != buffer2[i])
			{
				result = false;
				break;
			}
		}
		return result;
	}

	internal object GetKeyValue(Dictionary<int, object> propertyHash, int key)
	{
		if (propertyHash.ContainsKey(key))
		{
			return propertyHash[key];
		}
		try
		{
			return GetDefValue(key);
		}
		catch (Exception)
		{
			return null;
		}
	}
}

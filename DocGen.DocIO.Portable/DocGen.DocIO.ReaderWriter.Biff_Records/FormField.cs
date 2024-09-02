using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.DocIO.DLS;
using DocGen.DocIO.Utilities;

namespace DocGen.DocIO.ReaderWriter.Biff_Records;

internal class FormField
{
	internal const int DEF_VALUE = 25;

	private FieldType m_fieldType;

	private PICF m_picf;

	private ushort m_params;

	private ushort m_maxLength;

	private ushort m_checkBoxSize;

	private string m_title;

	private string m_defaultTextInputValue;

	private bool m_defaultCheckBoxValue;

	private ushort m_defaultDropDownValue;

	private string m_textFormat;

	private string m_help;

	private string m_tooltip;

	private string m_macroOnStart;

	private string m_macroOnEnd;

	private List<string> m_dropDownItems;

	private bool m_isUnicode;

	internal FormFieldType FormFieldType => (FormFieldType)(m_params & 3);

	internal FieldType FieldType => m_fieldType;

	internal short Params
	{
		get
		{
			return (short)m_params;
		}
		set
		{
			m_params = (ushort)value;
		}
	}

	internal int MaxLength
	{
		get
		{
			return m_maxLength;
		}
		set
		{
			m_maxLength = (ushort)value;
		}
	}

	internal int CheckBoxSize
	{
		get
		{
			return m_checkBoxSize;
		}
		set
		{
			m_checkBoxSize = (ushort)value;
		}
	}

	internal string Title
	{
		get
		{
			return m_title;
		}
		set
		{
			m_title = value;
		}
	}

	internal bool Checked
	{
		get
		{
			return Value switch
			{
				0 => false, 
				1 => true, 
				25 => m_defaultCheckBoxValue, 
				_ => throw new ArgumentException("Unsupported checkbox field value found."), 
			};
		}
		set
		{
			Value = (value ? 1 : 0);
		}
	}

	internal bool DefaultCheckBoxValue
	{
		get
		{
			return m_defaultCheckBoxValue;
		}
		set
		{
			m_defaultCheckBoxValue = value;
		}
	}

	internal int DefaultDropDownValue
	{
		get
		{
			return m_defaultDropDownValue;
		}
		set
		{
			m_defaultDropDownValue = (ushort)value;
		}
	}

	internal string DefaultTextInputValue
	{
		get
		{
			return m_defaultTextInputValue;
		}
		set
		{
			m_defaultTextInputValue = value;
		}
	}

	internal string Format
	{
		get
		{
			return m_textFormat;
		}
		set
		{
			m_textFormat = value;
		}
	}

	internal int Value
	{
		get
		{
			return (m_params & 0x7C) >> 2;
		}
		set
		{
			m_params = (ushort)((m_params & 0xFFFFFF83u) | (uint)(value << 2));
		}
	}

	internal string Help
	{
		get
		{
			return m_help;
		}
		set
		{
			m_help = value;
		}
	}

	internal string Tooltip
	{
		get
		{
			return m_tooltip;
		}
		set
		{
			m_tooltip = value;
		}
	}

	internal string MacroOnStart
	{
		get
		{
			return m_macroOnStart;
		}
		set
		{
			m_macroOnStart = value;
		}
	}

	internal string MacroOnEnd
	{
		get
		{
			return m_macroOnEnd;
		}
		set
		{
			m_macroOnEnd = value;
		}
	}

	internal int DropDownIndex
	{
		get
		{
			if (Value != 25)
			{
				return Value;
			}
			return m_defaultDropDownValue;
		}
		set
		{
			Value = value;
		}
	}

	internal List<string> DropDownItems => m_dropDownItems;

	internal string DropDownValue
	{
		get
		{
			return m_dropDownItems[DropDownIndex];
		}
		set
		{
			for (int i = 0; i < m_dropDownItems.Count; i++)
			{
				if (string.Compare(m_dropDownItems[i], value) == 0)
				{
					DropDownIndex = i;
					break;
				}
			}
		}
	}

	internal bool IsCalculateOnExit
	{
		get
		{
			return (m_params & 0x4000) != 0;
		}
		set
		{
			BaseWordRecord.SetBitsByMask(m_params, 16384, 14, value ? 1 : 0);
		}
	}

	internal bool IsCheckBoxExplicitSize
	{
		get
		{
			return (m_params & 0x400) != 0;
		}
		set
		{
			BaseWordRecord.SetBitsByMask(m_params, 1024, 11, value ? 1 : 0);
		}
	}

	internal bool IsDisabled
	{
		get
		{
			return (m_params & 0x200) != 0;
		}
		set
		{
			BaseWordRecord.SetBitsByMask(m_params, 512, 10, value ? 1 : 0);
		}
	}

	internal TextFormFieldType TextFormFieldType
	{
		get
		{
			return (TextFormFieldType)((m_params & 0x3800) >> 11);
		}
		set
		{
			m_params = (ushort)((m_params & 0xFFFFC7FFu) | (uint)((int)value << 11));
		}
	}

	internal FormField(FieldType fieldType)
	{
		m_fieldType = fieldType;
		switch (fieldType)
		{
		case FieldType.FieldFormTextInput:
			m_params = 0;
			break;
		case FieldType.FieldFormCheckBox:
			m_params = 1;
			break;
		case FieldType.FieldFormDropDown:
			m_params = 32770;
			m_dropDownItems = new List<string>();
			break;
		default:
			throw new Exception("Unknown field type.");
		}
		m_picf = new PICF();
		Value = 25;
	}

	internal FormField(FieldType fieldType, BinaryReader reader)
	{
		m_fieldType = fieldType;
		long position = reader.BaseStream.Position;
		if (reader.BaseStream.Length > reader.BaseStream.Position)
		{
			m_picf = new PICF(reader);
		}
		else
		{
			m_picf = new PICF();
		}
		if (m_picf.lcb <= 68)
		{
			return;
		}
		byte b = reader.ReadByte();
		if (b == byte.MaxValue)
		{
			reader.BaseStream.Position += 3L;
			b = reader.ReadByte();
			m_isUnicode = true;
		}
		m_params = reader.ReadByte();
		if (m_isUnicode)
		{
			m_params = (ushort)((m_params << 8) + b);
		}
		else if (b != byte.MaxValue)
		{
			m_params += b;
		}
		m_maxLength = reader.ReadUInt16();
		m_checkBoxSize = reader.ReadUInt16();
		if (!m_isUnicode)
		{
			reader.BaseStream.Position += 2L;
		}
		m_title = ReadString(reader, readZero: true);
		switch (fieldType)
		{
		case FieldType.FieldFormTextInput:
			m_defaultTextInputValue = ReadString(reader, readZero: true);
			break;
		case FieldType.FieldFormCheckBox:
			m_defaultCheckBoxValue = reader.ReadUInt16() != 0;
			break;
		case FieldType.FieldFormDropDown:
			m_defaultDropDownValue = reader.ReadUInt16();
			break;
		case FieldType.FieldLink:
			throw new NotImplementedException("Link fields are not yet supported");
		}
		m_textFormat = ReadString(reader, readZero: true);
		m_help = ReadString(reader, readZero: true);
		m_tooltip = ReadString(reader, readZero: true);
		m_macroOnStart = ReadString(reader, readZero: true);
		m_macroOnEnd = ReadString(reader, readZero: true);
		if (fieldType == FieldType.FieldFormDropDown)
		{
			reader.ReadUInt16();
			if (!m_isUnicode)
			{
				reader.BaseStream.Position += 2L;
			}
			int num = reader.ReadInt32();
			m_dropDownItems = new List<string>();
			if (!m_isUnicode)
			{
				reader.BaseStream.Position += 6L;
			}
			for (int i = 0; i < num; i++)
			{
				m_dropDownItems.Add(ReadString(reader, readZero: false));
			}
		}
		long num2 = reader.BaseStream.Position - position;
		if (m_picf.lcb > num2)
		{
			reader.BaseStream.Position = position + m_picf.lcb;
		}
		else if (m_picf.lcb != num2)
		{
			throw new ArgumentException("Unrecognized format of the form field.");
		}
	}

	internal void Write(Stream stream)
	{
		BinaryWriter binaryWriter = new BinaryWriter(stream);
		long position = binaryWriter.BaseStream.Position;
		m_picf.Write(stream);
		binaryWriter.Write(uint.MaxValue);
		binaryWriter.Write(m_params);
		binaryWriter.Write(m_maxLength);
		binaryWriter.Write(m_checkBoxSize);
		WriteString(m_title, binaryWriter, writeSeparator: true);
		switch (m_fieldType)
		{
		case FieldType.FieldFormTextInput:
			WriteString(m_defaultTextInputValue, binaryWriter, writeSeparator: true);
			break;
		case FieldType.FieldFormCheckBox:
			binaryWriter.Write(m_defaultCheckBoxValue ? ((ushort)1) : ((ushort)0));
			break;
		case FieldType.FieldFormDropDown:
			binaryWriter.Write(m_defaultDropDownValue);
			break;
		default:
			throw new Exception("Unsupported form field type.");
		}
		WriteString(m_textFormat, binaryWriter, writeSeparator: true);
		WriteString(m_help, binaryWriter, writeSeparator: true);
		WriteString(m_tooltip, binaryWriter, writeSeparator: true);
		WriteString(m_macroOnStart, binaryWriter, writeSeparator: true);
		WriteString(m_macroOnEnd, binaryWriter, writeSeparator: true);
		if (m_fieldType == FieldType.FieldFormDropDown)
		{
			binaryWriter.Write(ushort.MaxValue);
			binaryWriter.Write((uint)m_dropDownItems.Count);
			foreach (string dropDownItem in m_dropDownItems)
			{
				WriteString(dropDownItem, binaryWriter, writeSeparator: false);
			}
		}
		long position2 = binaryWriter.BaseStream.Position;
		m_picf.lcb = (int)(position2 - position);
		binaryWriter.BaseStream.Position = position;
		m_picf.Write(stream);
		binaryWriter.BaseStream.Position = position2;
	}

	internal static string ReadUnicodeString(BinaryReader binaryReader, bool readZero)
	{
		int num = binaryReader.ReadInt16();
		byte[] array = binaryReader.ReadBytes(num * 2);
		string @string = Encoding.Unicode.GetString(array, 0, array.Length);
		if (readZero)
		{
			binaryReader.ReadInt16();
		}
		return @string;
	}

	internal string ReadString(BinaryReader binaryReader, bool readZero)
	{
		int count = (m_isUnicode ? (binaryReader.ReadInt16() * 2) : binaryReader.ReadByte());
		byte[] array = binaryReader.ReadBytes(count);
		string empty = string.Empty;
		empty = ((!m_isUnicode) ? DocIOEncoding.GetString(array) : Encoding.Unicode.GetString(array, 0, array.Length));
		if (readZero)
		{
			if (!m_isUnicode)
			{
				binaryReader.ReadByte();
			}
			else
			{
				binaryReader.ReadInt16();
			}
		}
		return empty;
	}

	internal static void WriteString(string text, BinaryWriter writer, bool writeSeparator)
	{
		string text2 = ((text != null) ? text : "");
		writer.Write((short)text2.Length);
		writer.Write(Encoding.Unicode.GetBytes(text2));
		if (writeSeparator)
		{
			writer.Write((short)0);
		}
	}
}

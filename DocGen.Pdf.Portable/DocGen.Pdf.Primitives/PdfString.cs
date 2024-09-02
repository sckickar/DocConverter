using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using DocGen.Pdf.IO;
using DocGen.Pdf.Security;

namespace DocGen.Pdf.Primitives;

internal class PdfString : IPdfPrimitive, IPdfDecryptable
{
	internal enum ForceEncoding
	{
		None,
		ASCII,
		Unicode
	}

	private enum SourceType
	{
		StringValue,
		ByteBuffer
	}

	public const string StringMark = "()";

	public const string HexStringMark = "<>";

	private const string HexFormatPattern = "{0:X2}";

	private bool m_bHex;

	private string m_value;

	private byte[] m_data;

	private bool m_bConverted;

	private ForceEncoding m_bForceEncoding;

	private bool m_bDecrypted;

	private ObjectStatus m_status;

	private bool m_isSaving;

	private int m_index;

	private int m_position = -1;

	internal bool m_isParentDecrypted;

	private PdfCrossTable m_crossTable;

	private PdfString m_clonedObject;

	private bool m_isPacked;

	internal bool IsFormField;

	internal bool IsColorSpace;

	internal bool IsRGB;

	internal bool m_isHexString = true;

	private byte[] m_encodedBytes;

	private bool isSaveWriter;

	public string Value
	{
		get
		{
			return m_value;
		}
		set
		{
			if (value != m_value)
			{
				m_value = value;
				m_data = null;
				Encode = ForceEncoding.None;
			}
		}
	}

	internal bool Hex => m_bHex;

	internal bool IsPacked
	{
		get
		{
			return m_isPacked;
		}
		set
		{
			m_isPacked = value;
		}
	}

	internal bool Converted
	{
		get
		{
			return m_bConverted;
		}
		set
		{
			if (!m_bHex)
			{
				m_bConverted = value;
			}
		}
	}

	internal ForceEncoding Encode
	{
		get
		{
			return m_bForceEncoding;
		}
		set
		{
			m_bForceEncoding = value;
		}
	}

	public bool Decrypted => m_bDecrypted;

	internal byte[] Bytes
	{
		get
		{
			if (m_data == null)
			{
				m_data = ObtainBytes();
			}
			return m_data;
		}
	}

	public ObjectStatus Status
	{
		get
		{
			return m_status;
		}
		set
		{
			m_status = value;
		}
	}

	public bool IsSaving
	{
		get
		{
			return m_isSaving;
		}
		set
		{
			m_isSaving = value;
		}
	}

	public int ObjectCollectionIndex
	{
		get
		{
			return m_index;
		}
		set
		{
			m_index = value;
		}
	}

	public int Position
	{
		get
		{
			return m_position;
		}
		set
		{
			m_position = value;
		}
	}

	internal bool IsParentDecrypted
	{
		get
		{
			return m_isParentDecrypted;
		}
		set
		{
			m_isParentDecrypted = value;
		}
	}

	internal PdfCrossTable CrossTable => m_crossTable;

	public IPdfPrimitive ClonedObject => m_clonedObject;

	internal byte[] EncodedBytes
	{
		get
		{
			return m_encodedBytes;
		}
		set
		{
			m_encodedBytes = value;
		}
	}

	bool IPdfDecryptable.WasEncrypted
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public PdfString()
	{
	}

	public PdfString(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.Length > 0 && value[0] == '\ufeff')
		{
			m_value = value.Substring(1);
			return;
		}
		m_value = value;
		bool isAsciiBytes = false;
		m_data = GetAsciiBytes(value, out isAsciiBytes);
		if (isAsciiBytes)
		{
			Encode = ForceEncoding.ASCII;
		}
	}

	public PdfString(string value, bool encrypted)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (!encrypted && !(value == string.Empty))
		{
			m_data = HexToBytes(value);
			if (m_data.Length != 0)
			{
				if (m_data[0] == 254 && m_data[1] == byte.MaxValue)
				{
					m_value = Encoding.BigEndianUnicode.GetString(m_data, 2, m_data.Length - 2);
					if (string.IsNullOrEmpty(m_value.ToString()))
					{
						m_bHex = false;
						m_isHexString = false;
						m_data = StringToByte(m_value);
					}
				}
				else
				{
					m_value = ByteToString(m_data);
				}
			}
			else
			{
				m_value = value;
			}
		}
		else
		{
			m_value = value;
		}
		m_bHex = true;
	}

	public PdfString(byte[] value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		m_data = value;
		if (m_data.Length != 0)
		{
			if (m_data[0] == 254 && m_data[1] == byte.MaxValue)
			{
				m_value = Encoding.BigEndianUnicode.GetString(m_data, 2, m_data.Length - 2);
			}
			else
			{
				m_value = ByteToString(m_data);
			}
		}
		m_bHex = true;
	}

	public static string ByteToString(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("stream");
		}
		return ByteToString(data, data.Length);
	}

	internal static string ByteToString(byte[] data, int length)
	{
		if (data == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (length > data.Length)
		{
			throw new ArgumentOutOfRangeException("length", "The length can't be more then the array lenght.");
		}
		char[] array = new char[length];
		for (int i = 0; i < length; i++)
		{
			array[i] = (char)data[i];
		}
		return new string(array);
	}

	public static bool IsUnicode(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return Encoding.UTF8.GetByteCount(value) != value.Length;
	}

	internal static bool IsContainsNonBrakingSpace(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		byte[] bytes = Encoding.UTF8.GetBytes(value);
		if (bytes != null)
		{
			char[] chars = Encoding.UTF8.GetChars(bytes);
			for (int i = 0; i < chars.Length; i++)
			{
				if (chars[i] == '\u00a0')
				{
					return true;
				}
			}
		}
		return false;
	}

	internal static byte[] ToEncode(ushort[] data)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < data.Length; i++)
		{
			list.Add((byte)((data[i] & 0xFF00) >> 8));
			list.Add((byte)(data[i] & 0xFFu));
		}
		return list.ToArray();
	}

	internal static byte[] ToEncode(int data)
	{
		return new List<byte>
		{
			(byte)((data & 0xFF00) >> 8),
			(byte)((uint)data & 0xFFu)
		}.ToArray();
	}

	internal static byte[] ToEncode(char[] chars)
	{
		byte[] array = new byte[chars.Length * 2];
		for (int i = 0; i < chars.Length; i++)
		{
			array[2 * i] = (byte)(chars[i] / 256);
			array[2 * i + 1] = (byte)(chars[i] % 256);
		}
		return array;
	}

	public static byte[] ToUnicodeArray(string value, bool bAddPrefix)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		int num = Encoding.BigEndianUnicode.GetByteCount(value);
		byte[] array = null;
		int num2 = 0;
		if (bAddPrefix)
		{
			array = Encoding.BigEndianUnicode.GetPreamble();
			num2 = array.Length;
			num += num2;
		}
		byte[] array2 = new byte[num];
		if (bAddPrefix)
		{
			array.CopyTo(array2, 0);
		}
		Encoding.BigEndianUnicode.GetBytes(value, 0, value.Length, array2, num2);
		return array2;
	}

	public static string FromDate(DateTime dateTime)
	{
		return dateTime.ToString("D:yyyyMMddHHmmss", CultureInfo.InvariantCulture);
	}

	internal static int ByteCompare(PdfString str1, PdfString str2)
	{
		byte[] bytes = str1.Bytes;
		byte[] bytes2 = str2.Bytes;
		int num = Math.Min(bytes.Length, bytes2.Length);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			byte num3 = bytes[i];
			byte b = bytes2[i];
			num2 = num3 - b;
			if (num2 != 0)
			{
				break;
			}
		}
		if (num2 == 0)
		{
			num2 = bytes.Length - bytes2.Length;
		}
		return num2;
	}

	public static explicit operator PdfString(string str)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		return new PdfString(str);
	}

	internal byte[] PdfEncode(PdfDocumentBase document)
	{
		byte[] array = null;
		if (!Hex)
		{
			array = ((EncodedBytes != null) ? EncodedBytes : ObtainBytes());
			PdfSecurity pdfSecurity = document?.Security;
			if (pdfSecurity == null || !pdfSecurity.Enabled)
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (array.Length > 10)
				{
					for (int i = 0; i < 10; i++)
					{
						stringBuilder.Append(Convert.ToChar(array[i]));
					}
				}
				if (stringBuilder.ToString() == "ColorFound")
				{
					byte[] array2 = new byte[array.Length - 10];
					for (int j = 0; j < array.Length - 10; j++)
					{
						array2[j] = array[j + 10];
					}
					array = new byte[array.Length - 10];
					array = array2;
				}
				else if (!IsFormField)
				{
					array = EscapeSymbols(array);
				}
				else
				{
					bool flag = !Converted && IsUnicode(m_value);
					array = ((Encode != ForceEncoding.ASCII && (Encode != 0 || flag)) ? EscapeSymbols(array) : EscapeSymbols(array, IsFormField));
				}
			}
		}
		else if ((m_data == null || (IsColorSpace && m_isHexString)) && Value != null)
		{
			bool flag2 = !Converted && IsUnicode(m_value);
			array = ((!(document.m_isImported && flag2) || !IsRGB) ? GetAsciiBytes(Value) : GetBytes(flag2));
		}
		else
		{
			array = GetAsciiBytes(BytesToHex(m_data));
		}
		MemoryStream memoryStream = new MemoryStream(array.Length + 2);
		string text = (Hex ? "<>" : "()");
		bool flag3 = false;
		array = EncryptIfNeeded(array, document);
		for (int k = 0; k < array.Length; k++)
		{
			if ((array[k] >= 48 && array[k] <= 57) || (array[k] >= 65 && array[k] <= 70) || (array[k] >= 97 && array[k] <= 102))
			{
				flag3 = true;
				continue;
			}
			flag3 = false;
			break;
		}
		if (Hex)
		{
			if (!flag3)
			{
				array = GetAsciiBytes(BytesToHex(array));
			}
			if (flag3 && IsColorSpace && m_isHexString)
			{
				array = GetAsciiBytes(BytesToHex(array));
			}
		}
		memoryStream.WriteByte((byte)text[0]);
		memoryStream.Write(array, 0, array.Length);
		memoryStream.WriteByte((byte)text[1]);
		byte[] result = memoryStream.ToArray();
		memoryStream.Dispose();
		return result;
	}

	private static byte[] GetAsciiBytes(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		byte[] array = new byte[value.Length];
		int i = 0;
		for (int length = value.Length; i < length; i++)
		{
			array[i] = (byte)value[i];
		}
		return array;
	}

	private byte[] GetAsciiBytes(string value, out bool isAsciiBytes)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		isAsciiBytes = true;
		byte[] array = new byte[value.Length];
		int i = 0;
		for (int length = value.Length; i < length; i++)
		{
			byte b = (byte)value[i];
			array[i] = b;
			if (isAsciiBytes && value[i] > 'ÿ')
			{
				isAsciiBytes = false;
			}
		}
		return array;
	}

	internal static string BytesToHex(byte[] bytes)
	{
		if (bytes == null)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int i = 0;
		for (int num = bytes.Length; i < num; i++)
		{
			stringBuilder.AppendFormat("{0:X2}", bytes[i]);
		}
		return stringBuilder.ToString();
	}

	private byte[] EncryptIfNeeded(byte[] data, PdfDocumentBase document)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		PdfSecurity pdfSecurity = document?.Security;
		if (pdfSecurity == null || !pdfSecurity.Enabled || (pdfSecurity.Encryptor != null && pdfSecurity.Encryptor.EncryptOnlyAttachment))
		{
			return data;
		}
		if (document.CurrentSavingObj != null)
		{
			long objNum = document.CurrentSavingObj.ObjNum;
			data = pdfSecurity.Encryptor.EncryptData(objNum, data, isEncryption: true);
		}
		if (pdfSecurity.Enabled && IsColorSpace && m_isHexString && Hex)
		{
			return data;
		}
		return EscapeSymbols(data);
	}

	internal static byte[] EscapeSymbols(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		MemoryStream memoryStream = new MemoryStream();
		int i = 0;
		for (int num = data.Length; i < num; i++)
		{
			byte b = data[i];
			switch (b)
			{
			case 40:
			case 41:
				memoryStream.WriteByte(92);
				memoryStream.WriteByte(b);
				break;
			case 13:
				memoryStream.WriteByte(92);
				memoryStream.WriteByte(114);
				break;
			case 92:
				memoryStream.WriteByte(92);
				memoryStream.WriteByte(b);
				break;
			default:
				memoryStream.WriteByte(b);
				break;
			}
		}
		byte[] result = PdfStream.StreamToBytes(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	internal static byte[] EscapeSymbols(byte[] data, bool isFormField)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		MemoryStream memoryStream = new MemoryStream();
		int i = 0;
		for (int num = data.Length; i < num; i++)
		{
			byte b = data[i];
			switch (b)
			{
			case 40:
			case 41:
				memoryStream.WriteByte(92);
				memoryStream.WriteByte(b);
				break;
			case 13:
				if (isFormField)
				{
					memoryStream.WriteByte(92);
					memoryStream.WriteByte(114);
				}
				break;
			case 10:
				if (isFormField)
				{
					memoryStream.WriteByte(92);
					memoryStream.WriteByte(110);
				}
				break;
			case 62:
				if (!isFormField)
				{
					memoryStream.WriteByte(92);
					memoryStream.WriteByte(b);
				}
				else
				{
					memoryStream.WriteByte(b);
				}
				break;
			case 92:
				memoryStream.WriteByte(92);
				memoryStream.WriteByte(b);
				break;
			default:
				memoryStream.WriteByte(b);
				break;
			}
		}
		byte[] result = PdfStream.StreamToBytes(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public byte[] HexToBytes(string value)
	{
		List<byte> list = new List<byte>(value.Length);
		foreach (char c in value)
		{
			if (char.IsLetterOrDigit(c))
			{
				byte item = ParseHex(c);
				list.Add(item);
			}
		}
		return HexDigitsToNumbers(list);
	}

	private byte ParseHex(char c)
	{
		byte result = 0;
		if (c >= '0' && c <= '9')
		{
			result = (byte)(c - 48);
		}
		else if (c >= 'A' && c <= 'F')
		{
			result = (byte)(c - 65 + 10);
		}
		else if (c >= 'a' && c <= 'f')
		{
			result = (byte)(c - 97 + 10);
		}
		return result;
	}

	private byte[] HexDigitsToNumbers(List<byte> hexNumbers)
	{
		byte b = 0;
		bool flag = true;
		List<byte> list = new List<byte>(hexNumbers.Count / 2 + 1);
		foreach (byte hexNumber in hexNumbers)
		{
			if (flag)
			{
				b = (byte)(hexNumber << 4);
				flag = false;
			}
			else
			{
				b += hexNumber;
				list.Add(b);
				flag = true;
			}
		}
		if (!flag)
		{
			list.Add(b);
		}
		return list.ToArray();
	}

	private byte[] ObtainBytes()
	{
		bool flag = !Converted && IsUnicode(m_value);
		bool flag2 = false;
		if (flag && isSaveWriter)
		{
			flag2 = IsContainsNonBrakingSpace(m_value);
		}
		if (!flag2 && Encode == ForceEncoding.ASCII)
		{
			flag = false;
		}
		else if (Encode == ForceEncoding.Unicode)
		{
			flag = true;
		}
		if (IsColorSpace)
		{
			flag = false;
			if (Value.Contains("ColorFound") && Value.IndexOf("ColorFound") == 0)
			{
				Value = Value.Remove(0, 10);
			}
		}
		return GetBytes(flag);
	}

	private byte[] GetBytes(bool unicode)
	{
		if (!unicode)
		{
			return GetAsciiBytes(m_value);
		}
		return ToUnicodeArray(m_value, !Converted);
	}

	public void Save(IPdfWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		isSaveWriter = true;
		writer.Write(PdfEncode(writer.Document));
		isSaveWriter = false;
	}

	internal void ToHex()
	{
		if (!Hex)
		{
			byte[] bytes = ObtainBytes();
			Value = BytesToHex(bytes);
			m_bHex = true;
		}
	}

	public IPdfPrimitive Clone(PdfCrossTable crossTable)
	{
		if (m_clonedObject != null && m_clonedObject.CrossTable == crossTable)
		{
			return m_clonedObject;
		}
		m_clonedObject = null;
		PdfString pdfString = new PdfString(m_value);
		pdfString.Encode = m_bForceEncoding;
		pdfString.Converted = m_bConverted;
		pdfString.m_bHex = m_bHex;
		pdfString.m_crossTable = crossTable;
		pdfString.IsColorSpace = IsColorSpace;
		m_clonedObject = pdfString;
		return pdfString;
	}

	internal static byte[] StringToByte(string data)
	{
		return GetAsciiBytes(data);
	}

	private void ProcessUnicodeWithPreamble(ref string text, Encoding encoding)
	{
		byte[] array = StringToByte(text.Substring(2));
		for (int i = 0; i < array.Length - 1; i++)
		{
			if (array[i] == 92 && (array[i + 1] == 40 || array[i + 1] == 41 || array[i + 1] == 13 || array[i + 1] == 62 || array[i + 1] == 92))
			{
				for (int j = i; j < array.Length - 1; j++)
				{
					array[j] = array[j + 1];
				}
				byte[] array2 = new byte[array.Length - 1];
				Buffer.BlockCopy(array, 0, array2, 0, array.Length - 1);
				array = array2;
				i--;
			}
		}
		text = encoding.GetString(array, 0, array.Length);
	}

	public void Decrypt(PdfEncryptor encryptor, long currObjNumber)
	{
		if (encryptor == null || m_bDecrypted || IsParentDecrypted || encryptor.EncryptOnlyAttachment)
		{
			return;
		}
		m_bDecrypted = true;
		Value = ByteToString(Bytes);
		byte[] data = encryptor.EncryptData(currObjNumber, Bytes, isEncryption: false);
		Value = ByteToString(data);
		m_data = data;
		string value = ByteToString(Encoding.Unicode.GetPreamble());
		string value2 = ByteToString(Encoding.BigEndianUnicode.GetPreamble());
		if (Value.Length > 1 && !IsColorSpace)
		{
			if (Value.Substring(0, 2).Equals(value))
			{
				ProcessUnicodeWithPreamble(ref m_value, Encoding.Unicode);
			}
			else if (Value.Substring(0, 2).Equals(value2))
			{
				ProcessUnicodeWithPreamble(ref m_value, Encoding.BigEndianUnicode);
			}
		}
	}
}

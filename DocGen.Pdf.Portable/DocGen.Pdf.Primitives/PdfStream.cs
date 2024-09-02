using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.Pdf.Compression;
using DocGen.Pdf.IO;
using DocGen.Pdf.Security;

namespace DocGen.Pdf.Primitives;

internal class PdfStream : PdfDictionary, IPdfDecryptable
{
	private const string Prefix = "stream";

	private const string Suffix = "endstream";

	private MemoryStream m_dataStream;

	private bool m_blockEncryption;

	private bool m_bDecrypted;

	private bool m_bCompress;

	private bool m_bEncrypted;

	private new PdfStream m_clonedObject;

	internal bool isCustomQuality;

	internal bool isImageDualFilter;

	private long m_objNumber = -1L;

	private PdfCrossTable m_pdfCrosstable;

	internal MemoryStream InternalStream
	{
		get
		{
			if (!Decrypted && PdfCrossTable != null && ObjNumber > -1)
			{
				Decrypt(PdfCrossTable.Encryptor, ObjNumber);
			}
			return m_dataStream;
		}
		set
		{
			m_dataStream = value;
		}
	}

	internal byte[] Data
	{
		get
		{
			if (!Decrypted && PdfCrossTable != null && ObjNumber > -1)
			{
				Decrypt(PdfCrossTable.Encryptor, ObjNumber);
			}
			return m_dataStream.ToArray();
		}
		set
		{
			m_dataStream.SetLength(0L);
			m_dataStream.Write(value, 0, value.Length);
			Modify();
		}
	}

	internal bool Compress
	{
		get
		{
			return m_bCompress;
		}
		set
		{
			m_bCompress = value;
			Modify();
		}
	}

	public override IPdfPrimitive ClonedObject => m_clonedObject;

	internal long ObjNumber
	{
		get
		{
			return m_objNumber;
		}
		set
		{
			m_objNumber = value;
		}
	}

	internal PdfCrossTable PdfCrossTable
	{
		get
		{
			return m_pdfCrosstable;
		}
		set
		{
			m_pdfCrosstable = value;
		}
	}

	public bool WasEncrypted
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool Decrypted => m_bDecrypted;

	internal PdfStream()
	{
		m_dataStream = new MemoryStream(100);
		m_bCompress = true;
	}

	internal PdfStream(PdfDictionary dictionary, byte[] data)
		: base(dictionary)
	{
		m_dataStream = new MemoryStream(data.Length);
		Data = data;
		m_bCompress = false;
		base["Length"] = new PdfNumber(data.Length);
		if (ContainsKey("Length3") && ContainsKey("Filter") && m_bDecrypted)
		{
			string filterName = GetFilterName(this);
			if (filterName != null && base["Length3"] is PdfReferenceHolder)
			{
				byte[] array = Decompress(data, filterName);
				string[] array2 = Encoding.UTF8.GetString(array, 0, array.Length).Split(new string[1] { "eexec" }, StringSplitOptions.None);
				string text = string.Empty;
				for (int i = 0; i < 32; i++)
				{
					text += "0";
				}
				int num = array2[0].Length + 7;
				if (array2.Length > 1)
				{
					string[] array3 = array2[1].Split(new string[1] { text }, StringSplitOptions.None);
					int num2 = 0;
					for (int j = 1; j < array3.Length; j++)
					{
						num2 += array3[j].Length;
					}
					int num3 = 32 * (array3.Length - 1) + num2;
					Encoding.UTF8.GetBytes(array3[0]);
					base["Length1"] = new PdfNumber(num);
					base["Length2"] = new PdfNumber(array.Length - num - num3);
					base["Length3"] = new PdfNumber(num3);
				}
			}
		}
		if (ContainsKey("Length1") && ContainsKey("Filter") && base["Length1"] is PdfReferenceHolder && base["Filter"] is PdfArray)
		{
			base["Length1"] = new PdfNumber(data.Length);
		}
	}

	public static byte[] StreamToBytes(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		return StreamToBytes(stream, writeWholeStream: false);
	}

	public static byte[] StreamToBytes(Stream stream, bool writeWholeStream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		long position = stream.Position;
		long num = ((stream.Position != 0L) ? stream.Position : stream.Length);
		num = (writeWholeStream ? stream.Length : num);
		byte[] array = new byte[num];
		stream.Position = 0L;
		stream.Read(array, 0, (int)num);
		stream.Position = position;
		return array;
	}

	public static byte[] StreamToBigEndian(Stream stream)
	{
		byte[] bytes = StreamToBytes(stream);
		return Encoding.Convert(Encoding.Unicode, Encoding.BigEndianUnicode, bytes);
	}

	internal void Write(char symbol)
	{
		Write(symbol.ToString());
	}

	internal void Write(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (text.Length <= 0)
		{
			throw new ArgumentException("Can't write an empty string.", "text");
		}
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		Write(bytes);
	}

	internal void Write(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data.Length == 0)
		{
			throw new ArgumentException("Can't write an empty array.", "data");
		}
		m_dataStream.Write(data, 0, data.Length);
		Modify();
	}

	internal void BlockEncryption()
	{
		m_blockEncryption = true;
	}

	internal void Decompress()
	{
		string text = "";
		IPdfPrimitive pdfPrimitive = base["Filter"];
		if (pdfPrimitive is PdfReferenceHolder)
		{
			pdfPrimitive = (pdfPrimitive as PdfReferenceHolder).Object;
		}
		if (pdfPrimitive != null)
		{
			if (pdfPrimitive is PdfName)
			{
				PdfName pdfName = pdfPrimitive as PdfName;
				if (pdfName.Value == "ASCIIHexDecode")
				{
					Data = Decode(Data);
				}
				else
				{
					Data = Decompress(Data, pdfName.Value);
				}
			}
			else
			{
				if (!(pdfPrimitive is PdfArray))
				{
					throw new PdfDocumentException("Invalid/Unknown/Unsupported formatUnexpected object for filter.");
				}
				foreach (IPdfPrimitive item in pdfPrimitive as PdfArray)
				{
					text = (item as PdfName).Value;
					if (text == null)
					{
						throw new PdfDocumentException("Invalid/Unknown/Unsupported format");
					}
					if (text == "ASCIIHexDecode")
					{
						Data = Decode(Data);
					}
					else
					{
						Data = Decompress(Data, text);
					}
				}
			}
		}
		Remove("Filter");
		m_bCompress = true;
	}

	internal byte[] GetDecompressedData()
	{
		string text = "";
		byte[] array = Data;
		IPdfPrimitive pdfPrimitive = base["Filter"];
		if (pdfPrimitive is PdfReferenceHolder)
		{
			pdfPrimitive = (pdfPrimitive as PdfReferenceHolder).Object;
		}
		if (pdfPrimitive != null)
		{
			if (pdfPrimitive is PdfName)
			{
				PdfName pdfName = pdfPrimitive as PdfName;
				array = Decompress(array, pdfName.Value);
			}
			else
			{
				if (!(pdfPrimitive is PdfArray))
				{
					throw new PdfDocumentException("Invalid/Unknown/Unsupported formatUnexpected object for filter.");
				}
				foreach (IPdfPrimitive item in pdfPrimitive as PdfArray)
				{
					text = (item as PdfName).Value;
					if (text == null)
					{
						throw new PdfDocumentException("Invalid/Unknown/Unsupported format");
					}
					array = ((!(text == "ASCIIHexDecode")) ? ((!(text == "ASCII85Decode")) ? Decompress(array, text) : DecodeASCII85Stream(array)) : Decode(array));
				}
			}
		}
		return array;
	}

	private byte[] DecodeASCII85Stream(byte[] data)
	{
		return new ASCII85().decode(data);
	}

	private byte HexToDecimalConversion(char hex)
	{
		if (hex >= '0' && hex <= '9')
		{
			return (byte)(hex - 48);
		}
		if (hex >= 'a' && hex <= 'f')
		{
			return (byte)(hex - 97 + 10);
		}
		return (byte)(hex - 65 + 10);
	}

	private byte[] Decode(byte[] inputData)
	{
		inputData = RemoveWhiteSpace(inputData);
		List<byte> list = new List<byte>(inputData.Length);
		int num = 0;
		while (num < inputData.Length)
		{
			int num2 = num + 1;
			byte hex;
			if ((hex = inputData[num]) == 62)
			{
				break;
			}
			byte b = inputData[num2];
			num = num2 + 1;
			if (b == 62)
			{
				b = 48;
			}
			byte item = (byte)((HexToDecimalConversion((char)hex) << 4) | HexToDecimalConversion((char)b));
			list.Add(item);
		}
		byte[] result = list.ToArray();
		list.Clear();
		list = null;
		return result;
	}

	private byte[] RemoveWhiteSpace(byte[] data)
	{
		List<byte> list = new List<byte>(data.Length);
		for (int i = 0; i < data.Length; i++)
		{
			if (!char.IsWhiteSpace((char)data[i]))
			{
				list.Add(data[i]);
			}
		}
		byte[] result = list.ToArray();
		list.Clear();
		list = null;
		return result;
	}

	internal new void Clear()
	{
		if (InternalStream != null && InternalStream.CanWrite)
		{
			InternalStream.SetLength(0L);
			InternalStream.Position = 0L;
		}
		Remove("Filter");
		m_bCompress = true;
		Modify();
	}

	internal void Dispose()
	{
		if (InternalStream != null)
		{
			InternalStream.Dispose();
			InternalStream = null;
			base.Items.Clear();
			m_bCompress = true;
			Modify();
		}
	}

	public override void Save(IPdfWriter writer)
	{
		SavePdfPrimitiveEventArgs args = new SavePdfPrimitiveEventArgs(writer);
		OnBeginSave(args);
		byte[] array = CompressContent(writer);
		PdfSecurity security = writer.Document.Security;
		if (security != null && security.Encryptor != null && security.Encryptor.Encrypt && security.Encryptor.EncryptOnlyAttachment)
		{
			bool flag = false;
			if (ContainsKey("Type"))
			{
				PdfName pdfName = base["Type"] as PdfName;
				if (pdfName != null && pdfName.Value == "EmbeddedFile")
				{
					PdfArray pdfArray = base["Filter"] as PdfArray;
					PdfName pdfName2 = base["Filter"] as PdfName;
					if (pdfArray == null || (pdfArray != null && !pdfArray.Contains(new PdfName("Crypt"))))
					{
						if (m_bCompress || !ContainsKey("Filter") || (pdfArray != null && !pdfArray.Contains(new PdfName("FlateDecode"))) || (pdfName2 != null && pdfName2.Value != "FlateDecode"))
						{
							array = CompressStream();
						}
						flag = true;
						array = EncryptContent(array, writer);
						AddFilter("Crypt");
					}
					if (!ContainsKey("DecodeParms"))
					{
						PdfArray pdfArray2 = new PdfArray();
						PdfDictionary pdfDictionary = new PdfDictionary();
						pdfDictionary[new PdfName("Name")] = new PdfName("StdCF");
						pdfArray2.Add(pdfDictionary);
						PdfNull element = new PdfNull();
						pdfArray2.Add(element);
						SetProperty("DecodeParms", pdfArray2);
					}
				}
			}
			if (!flag)
			{
				if (ContainsKey("DecodeParms"))
				{
					if (base["DecodeParms"] is PdfArray pdfArray3 && pdfArray3[0] is PdfDictionary pdfDictionary2)
					{
						PdfName pdfName3 = pdfDictionary2["Name"] as PdfName;
						if (pdfName3 != null && pdfName3.Value == "StdCF" && (!(base["Filter"] is PdfArray pdfArray4) || (pdfArray4 != null && !pdfArray4.Contains(new PdfName("Crypt")))))
						{
							if (m_bCompress)
							{
								array = CompressStream();
							}
							array = EncryptContent(array, writer);
							AddFilter("Crypt");
						}
					}
				}
				else if (ContainsKey("DL"))
				{
					if (m_bCompress)
					{
						array = CompressStream();
					}
					array = EncryptContent(array, writer);
					AddFilter("Crypt");
					if (!ContainsKey("DecodeParms"))
					{
						PdfArray pdfArray5 = new PdfArray();
						PdfDictionary pdfDictionary3 = new PdfDictionary();
						pdfDictionary3[new PdfName("Name")] = new PdfName("StdCF");
						pdfArray5.Add(pdfDictionary3);
						PdfNull element2 = new PdfNull();
						pdfArray5.Add(element2);
						SetProperty("DecodeParms", pdfArray5);
					}
				}
			}
		}
		else if (security != null && security.Encryptor != null && !security.Encryptor.EncryptMetaData && ContainsKey("Type"))
		{
			PdfName pdfName4 = base["Type"] as PdfName;
			if (pdfName4 == null || (pdfName4 != null && pdfName4.Value != "Metadata".ToString()))
			{
				array = EncryptContent(array, writer);
			}
		}
		else
		{
			array = EncryptContent(array, writer);
		}
		base["Length"] = new PdfNumber(array.Length);
		if (ContainsKey("Length1") && ContainsKey("Filter") && base["Length1"] is PdfReferenceHolder && base["Filter"] is PdfArray)
		{
			base["Length1"] = new PdfNumber(array.Length);
		}
		if (ContainsKey("Length3") && ContainsKey("Filter") && !m_bEncrypted)
		{
			string filterName = GetFilterName(this);
			if (filterName != null && base["Length3"] is PdfReferenceHolder)
			{
				byte[] array2 = Decompress(array, filterName);
				string[] array3 = Encoding.UTF8.GetString(array2, 0, array2.Length).Split(new string[1] { "eexec" }, StringSplitOptions.None);
				string text = string.Empty;
				for (int i = 0; i < 32; i++)
				{
					text += "0";
				}
				int num = array3[0].Length + 7;
				if (array3.Length > 1)
				{
					string[] array4 = array3[1].Split(new string[1] { text }, StringSplitOptions.None);
					int num2 = 0;
					for (int j = 1; j < array4.Length; j++)
					{
						num2 += array4[j].Length;
					}
					int num3 = 32 * (array4.Length - 1) + num2;
					Encoding.UTF8.GetBytes(array4[0]);
					base["Length1"] = new PdfNumber(num);
					base["Length2"] = new PdfNumber(array2.Length - num - num3);
					base["Length3"] = new PdfNumber(num3);
				}
			}
		}
		Save(writer, bRaiseEvent: false);
		writer.Write("stream");
		writer.Write("\r\n");
		if (array.Length != 0)
		{
			writer.Write(array);
			writer.Write("\r\n");
		}
		writer.Write("endstream");
		writer.Write("\r\n");
		SavePdfPrimitiveEventArgs args2 = new SavePdfPrimitiveEventArgs(writer);
		OnEndSave(args2);
		if (m_bCompress)
		{
			if (isImageDualFilter)
			{
				RemoveFilter();
			}
			else
			{
				Remove("Filter");
			}
		}
	}

	private void RemoveFilter()
	{
		IPdfPrimitive pdfPrimitive = base["Filter"];
		if (pdfPrimitive is PdfReferenceHolder)
		{
			pdfPrimitive = (pdfPrimitive as PdfReferenceHolder).Object;
		}
		if (pdfPrimitive is PdfArray)
		{
			PdfName element = new PdfName("FlateDecode");
			PdfArray pdfArray = pdfPrimitive as PdfArray;
			if (pdfArray.Contains(element))
			{
				pdfArray.Remove(element);
			}
		}
		else if (pdfPrimitive is PdfName)
		{
			Remove("Filter");
		}
	}

	private byte[] CompressStream()
	{
		bool flag = false;
		byte[] array = Data;
		if (array != null)
		{
			array = new PdfZlibCompressor(PdfCompressionLevel.Best).Compress(array);
			flag = true;
		}
		Clear();
		Compress = false;
		if (array != null)
		{
			InternalStream.Write(array, 0, array.Length);
			if (flag)
			{
				AddFilter("FlateDecode");
			}
		}
		return array;
	}

	public override IPdfPrimitive Clone(PdfCrossTable crossTable)
	{
		if (m_clonedObject != null && m_clonedObject.CrossTable == crossTable)
		{
			return m_clonedObject;
		}
		m_clonedObject = null;
		PdfStream pdfStream = new PdfStream(base.Clone(crossTable) as PdfDictionary, Data);
		pdfStream.Compress = m_bCompress;
		pdfStream.m_bDecrypted = m_bDecrypted;
		m_clonedObject = pdfStream;
		return pdfStream;
	}

	public void Decrypt(PdfEncryptor encryptor, long currObjNumber)
	{
		if (encryptor != null && !m_bDecrypted)
		{
			m_bDecrypted = true;
			Data = encryptor.EncryptData(currObjNumber, Data, isEncryption: false);
		}
	}

	private byte[] Decompress(byte[] data, string filter)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		if (data.Length == 0)
		{
			return data;
		}
		if (filter != "Crypt")
		{
			if (filter.Equals("RunLengthDecode"))
			{
				Stream stream = new MemoryStream(data);
				MemoryStream memoryStream = new MemoryStream();
				int num = -1;
				byte[] buffer = new byte[128];
				while ((num = stream.ReadByte()) != -1 && num != 128)
				{
					if (num <= 127)
					{
						int num2 = num + 1;
						int num3 = 0;
						while (num2 > 0)
						{
							num3 = stream.Read(buffer, 0, num2);
							memoryStream.Write(buffer, 0, num3);
							num2 -= num3;
						}
					}
					else
					{
						int num4 = stream.ReadByte();
						for (int i = 0; i < 257 - num; i++)
						{
							memoryStream.WriteByte((byte)num4);
						}
					}
				}
				memoryStream.Position = 0L;
				return memoryStream.ToArray();
			}
			IPdfCompressor pdfCompressor = DetermineCompressor(filter);
			if (filter == "FlateDecode")
			{
				try
				{
					return PostProcess(pdfCompressor.Decompress(data), filter);
				}
				catch (Exception)
				{
					return data;
				}
			}
			return PostProcess(pdfCompressor.Decompress(data), filter);
		}
		return data;
	}

	private IPdfCompressor DetermineCompressor(string filter)
	{
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		switch (filter)
		{
		case "FlateDecode":
		case "Fl":
			return new PdfZlibCompressor();
		case "LZWDecode":
		case "LZW":
			return new PdfLzwCompressor();
		case "A85":
		case "ASCII85Decode":
			return new PdfASCII85Compressor();
		default:
			throw new PdfDocumentException("Invalid/Unknown/Unsupported format Unsupported compressor (" + filter + ").");
		}
	}

	private byte[] PostProcess(byte[] data, string filter)
	{
		if (filter == "FlateDecode")
		{
			IPdfPrimitive pdfPrimitive = base["DecodeParms"];
			if (pdfPrimitive == null)
			{
				return data;
			}
			PdfDictionary pdfDictionary = pdfPrimitive as PdfDictionary;
			PdfArray pdfArray = pdfPrimitive as PdfArray;
			PdfNull pdfNull = pdfPrimitive as PdfNull;
			if (pdfPrimitive is PdfReferenceHolder)
			{
				pdfDictionary = DocGen.Pdf.IO.PdfCrossTable.Dereference(pdfPrimitive) as PdfDictionary;
				pdfArray = DocGen.Pdf.IO.PdfCrossTable.Dereference(pdfPrimitive) as PdfArray;
				pdfNull = DocGen.Pdf.IO.PdfCrossTable.Dereference(pdfPrimitive) as PdfNull;
			}
			if (pdfNull != null)
			{
				return data;
			}
			if (pdfDictionary == null && pdfArray == null)
			{
				throw new PdfDocumentException("Invalid/Unknown/Unsupported format");
			}
			if (pdfArray != null && pdfArray.Elements.Count > 0 && pdfArray[0] is PdfDictionary pdfDictionary2)
			{
				PdfName pdfName = pdfDictionary2["Name"] as PdfName;
				if (pdfName != null && pdfName.Value == "StdCF")
				{
					return data;
				}
			}
			int num = 1;
			if (pdfDictionary != null)
			{
				if (pdfDictionary["Predictor"] is PdfNumber pdfNumber)
				{
					num = pdfNumber.IntValue;
				}
			}
			else if (pdfArray != null && pdfArray.Count > 0)
			{
				num = ((!(pdfArray[0] is PdfDictionary pdfDictionary3) || !pdfDictionary3.ContainsKey("Predictor")) ? 1 : pdfDictionary3.GetInt("Predictor"));
			}
			switch (num)
			{
			case 1:
				return data;
			case 2:
				throw new PdfDocumentException("Unsupported predictor: TIFF 2.");
			case 3:
			case 4:
			case 5:
			case 6:
			case 7:
			case 8:
			case 9:
			case 10:
			case 11:
			case 12:
			case 13:
			case 14:
			case 15:
			{
				int num2 = 1;
				int num3 = 1;
				pdfPrimitive = pdfDictionary["Colors"];
				if (pdfPrimitive != null)
				{
					num2 = (pdfPrimitive as PdfNumber).IntValue;
				}
				pdfPrimitive = pdfDictionary["Columns"];
				if (pdfPrimitive != null)
				{
					num3 = (pdfPrimitive as PdfNumber).IntValue;
				}
				pdfPrimitive = pdfDictionary["BitsPerComponent"];
				if (pdfPrimitive != null)
				{
					_ = (pdfPrimitive as PdfNumber).IntValue;
				}
				return PdfPngFilter.Decompress(data, num2 * num3);
			}
			default:
				throw new PdfDocumentException("Invalid/Unknown/Unsupported format Unknown predictor code: " + num);
			}
		}
		return data;
	}

	private void NormalizeFilter()
	{
		if (base["Filter"] is PdfArray { Count: 1 } pdfArray)
		{
			base["Filter"] = pdfArray[0];
		}
	}

	private byte[] CompressContent(IPdfWriter writer)
	{
		PdfCompressionLevel pdfCompressionLevel = writer.Document.Compression;
		if (writer.Document.isCompressed)
		{
			pdfCompressionLevel = PdfCompressionLevel.Best;
		}
		bool num = pdfCompressionLevel != PdfCompressionLevel.None;
		byte[] array = Data;
		if (num && m_bCompress)
		{
			array = new PdfZlibCompressor(pdfCompressionLevel).Compress(array);
			AddFilter("FlateDecode");
		}
		return array;
	}

	internal void AddFilter(string filterName)
	{
		IPdfPrimitive pdfPrimitive = base["Filter"];
		if (pdfPrimitive is PdfReferenceHolder)
		{
			pdfPrimitive = (pdfPrimitive as PdfReferenceHolder).Object;
		}
		PdfArray pdfArray = pdfPrimitive as PdfArray;
		PdfName pdfName = pdfPrimitive as PdfName;
		if (pdfName != null)
		{
			pdfArray = new PdfArray();
			pdfArray.Insert(0, pdfName);
			base["Filter"] = pdfArray;
		}
		pdfName = new PdfName(filterName);
		if (pdfArray == null)
		{
			base["Filter"] = pdfName;
		}
		else
		{
			pdfArray.Insert(0, pdfName);
		}
	}

	private byte[] EncryptContent(byte[] data, IPdfWriter writer)
	{
		PdfDocumentBase document = writer.Document;
		PdfEncryptor encryptor = document.Security.Encryptor;
		if (encryptor.Encrypt && !m_blockEncryption)
		{
			m_bEncrypted = true;
			data = encryptor.EncryptData(document.CurrentSavingObj.ObjNum, data, isEncryption: true);
		}
		return data;
	}

	private string GetFilterName(PdfDictionary dictionary)
	{
		string result = null;
		IPdfPrimitive pdfPrimitive = base["Filter"];
		if (pdfPrimitive != null && pdfPrimitive is PdfReferenceHolder)
		{
			pdfPrimitive = (pdfPrimitive as PdfReferenceHolder).Object;
		}
		if (pdfPrimitive != null)
		{
			if (pdfPrimitive is PdfName)
			{
				result = (pdfPrimitive as PdfName).Value;
			}
			else if (pdfPrimitive is PdfArray)
			{
				foreach (IPdfPrimitive item in pdfPrimitive as PdfArray)
				{
					result = (item as PdfName).Value;
				}
			}
		}
		return result;
	}
}

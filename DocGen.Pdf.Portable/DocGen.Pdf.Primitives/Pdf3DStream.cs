using System;
using System.IO;
using System.Text;
using DocGen.Pdf.Compression;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.IO;
using DocGen.Pdf.Security;

namespace DocGen.Pdf.Primitives;

internal class Pdf3DStream : PdfDictionary, IPdfDecryptable
{
	private const string Prefix = "stream";

	private const string Suffix = "endstream";

	private byte[] m_content;

	private Pdf3DAnimation m_animation;

	private int m_defaultView;

	private string m_onInstatiate;

	private Pdf3DAnnotationType m_type;

	private Pdf3DViewCollection m_pdf3dViewCollection;

	private MemoryStream m_dataStream;

	private bool m_blockEncryption;

	private bool m_bDecrypted;

	private bool m_bCompress;

	public Pdf3DAnimation Animation
	{
		get
		{
			return m_animation;
		}
		set
		{
			m_animation = value;
		}
	}

	public byte[] Content
	{
		get
		{
			return m_content;
		}
		set
		{
			m_content = value;
		}
	}

	public int DefaultView
	{
		get
		{
			return m_defaultView;
		}
		set
		{
			m_defaultView = value;
		}
	}

	internal Pdf3DAnnotationType Type
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
		}
	}

	public string OnInstantiate
	{
		get
		{
			return m_onInstatiate;
		}
		set
		{
			m_onInstatiate = value;
		}
	}

	public Pdf3DViewCollection Views => m_pdf3dViewCollection;

	internal MemoryStream InternalStream => m_dataStream;

	internal byte[] Data
	{
		get
		{
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

	public bool WasEncrypted
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public bool Decrypted => m_bDecrypted;

	internal Pdf3DStream()
	{
		m_dataStream = new MemoryStream(100);
		m_bCompress = true;
		m_pdf3dViewCollection = new Pdf3DViewCollection();
	}

	internal Pdf3DStream(PdfDictionary dictionary, byte[] data)
		: base(dictionary)
	{
		m_dataStream = new MemoryStream(data.Length);
		Data = data;
		m_bCompress = false;
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
				Data = Decompress(Data, pdfName.Value);
			}
			else
			{
				if (!(pdfPrimitive is PdfArray))
				{
					throw new PdfDocumentException("Invalid/Unknown/Unsupported formatUnexpected object for filter.");
				}
				foreach (IPdfPrimitive item in pdfPrimitive as PdfArray)
				{
					string value = (item as PdfName).Value;
					if (value == null)
					{
						throw new PdfDocumentException("Invalid/Unknown/Unsupported format");
					}
					Data = Decompress(Data, value);
				}
			}
		}
		Remove("Filter");
		m_bCompress = true;
	}

	internal new void Clear()
	{
		InternalStream.SetLength(0L);
		InternalStream.Position = 0L;
		Remove("Filter");
		m_bCompress = true;
		Modify();
	}

	public override void Save(IPdfWriter writer)
	{
		SavePdfPrimitiveEventArgs args = new SavePdfPrimitiveEventArgs(writer);
		OnBeginSave(args);
		byte[] array = CompressContent(writer);
		base["Type"] = new PdfName("3D");
		if (m_type == Pdf3DAnnotationType.PRC)
		{
			base["Subtype"] = new PdfName("PRC");
		}
		else
		{
			base["Subtype"] = new PdfName("U3D");
		}
		if (m_animation != null)
		{
			base["AN"] = new PdfReferenceHolder(m_animation);
		}
		base["DV"] = new PdfNumber(m_defaultView);
		if (m_pdf3dViewCollection != null && m_pdf3dViewCollection.Count > 0)
		{
			PdfArray pdfArray = new PdfArray();
			for (int i = 0; i < m_pdf3dViewCollection.Count; i++)
			{
				pdfArray.Insert(i, new PdfReferenceHolder(m_pdf3dViewCollection[i]));
			}
			base["VA"] = new PdfArray(pdfArray);
		}
		if (m_onInstatiate != null)
		{
			PdfStream pdfStream = new PdfStream();
			pdfStream.Write(m_onInstatiate);
			base["OnInstantiate"] = new PdfReferenceHolder(pdfStream);
		}
		base["Length"] = new PdfNumber(array.Length);
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
			Remove("Filter");
		}
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
		IPdfCompressor pdfCompressor = DetermineCompressor(filter);
		return PostProcess(pdfCompressor.Decompress(data), filter);
	}

	private IPdfCompressor DetermineCompressor(string filter)
	{
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		return filter switch
		{
			"FlateDecode" => new PdfZlibCompressor(), 
			"LZWDecode" => new PdfLzwCompressor(), 
			_ => throw new PdfDocumentException("Invalid/Unknown/Unsupported format Unsupported compressor (" + filter + ")."), 
		};
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
			if (!(pdfPrimitive is PdfDictionary pdfDictionary))
			{
				throw new PdfDocumentException("Invalid/Unknown/Unsupported format");
			}
			int num = 1;
			num = (pdfDictionary["Predictor"] as PdfNumber).IntValue;
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
		writer.Document.Compression = PdfCompressionLevel.AboveNormal;
		PdfCompressionLevel compression = writer.Document.Compression;
		bool num = compression != PdfCompressionLevel.BestSpeed;
		byte[] array = Data;
		if (num && m_bCompress)
		{
			array = new PdfZlibCompressor(compression).Compress(array);
			AddFilter("FlateDecode");
		}
		return array;
	}

	private void AddFilter(string filterName)
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
}

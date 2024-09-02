using System;
using System.IO;
using System.IO.Compression;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class Type0 : Function
{
	private int m_bitsPerSample;

	private int m_order;

	private PdfArray m_size;

	private PdfArray m_encode;

	private PdfArray m_decode;

	private int[][] m_sampleValue;

	private int outputValuesCount;

	private string m_filter;

	internal int BitsPerSample
	{
		get
		{
			return m_bitsPerSample;
		}
		set
		{
			m_bitsPerSample = value;
		}
	}

	internal string Filter
	{
		get
		{
			return m_filter;
		}
		set
		{
			m_filter = value;
		}
	}

	internal int Order
	{
		get
		{
			return m_order;
		}
		set
		{
			m_order = value;
		}
	}

	internal PdfArray Decode
	{
		get
		{
			if (m_decode == null)
			{
				return base.Range;
			}
			return m_decode;
		}
		set
		{
			m_decode = value;
		}
	}

	internal PdfArray Encode
	{
		get
		{
			if (m_encode == null)
			{
				PdfArray pdfArray = new PdfArray();
				for (int i = 0; i < Size.Count; i++)
				{
					int intValue = (Size[i] as PdfNumber).IntValue;
					pdfArray.Add(new PdfNumber(0));
					pdfArray.Add(new PdfNumber(intValue - 1));
				}
				Encode = pdfArray;
			}
			return m_encode;
		}
		set
		{
			m_encode = value;
		}
	}

	internal PdfArray Size
	{
		get
		{
			return m_size;
		}
		set
		{
			m_size = value;
		}
	}

	public Type0(PdfDictionary dictionary)
		: base(dictionary)
	{
		m_bitsPerSample = (dictionary.Items.ContainsKey(new PdfName("BitPerSample")) ? (dictionary[new PdfName("BitPerSample")] as PdfNumber).IntValue : 8);
		m_decode = null;
		m_encode = null;
		m_size = null;
		if (dictionary != null)
		{
			if (dictionary.ContainsKey("Decode"))
			{
				m_decode = PdfCrossTable.Dereference(dictionary["Decode"]) as PdfArray;
			}
			if (dictionary.ContainsKey("Encode"))
			{
				m_encode = PdfCrossTable.Dereference(dictionary["Encode"]) as PdfArray;
			}
			if (dictionary.ContainsKey("Size"))
			{
				m_size = PdfCrossTable.Dereference(dictionary["Size"]) as PdfArray;
			}
		}
		m_filter = GetFilter(dictionary);
		Load(dictionary as PdfStream);
	}

	internal void Load(PdfStream stream)
	{
		DataReader dataReader = new DataReader(GetDecodedStream(stream), BitsPerSample);
		int num = 1;
		for (int i = 0; i < Size.Count; i++)
		{
			int intValue = (Size[i] as PdfNumber).IntValue;
			num *= intValue;
		}
		outputValuesCount = base.Range.Count / 2;
		m_sampleValue = new int[num][];
		for (int j = 0; j < num; j++)
		{
			m_sampleValue[j] = new int[outputValuesCount];
			for (int k = 0; k < outputValuesCount; k++)
			{
				m_sampleValue[j][k] = dataReader.Read();
			}
		}
	}

	protected override double[] PerformFunction(double[] inputData)
	{
		int[] array = new int[inputData.Length];
		for (int i = 0; i < inputData.Length; i++)
		{
			double minD = (base.Domain[2 * i] as PdfNumber).FloatValue;
			double maxD = (base.Domain[2 * i + 1] as PdfNumber).FloatValue;
			double minE = (Encode[2 * i] as PdfNumber).FloatValue;
			double maxE = (Encode[2 * i + 1] as PdfNumber).FloatValue;
			int intValue = (Size[i] as PdfNumber).IntValue;
			array[i] = EncodeInputData(inputData[i], minD, maxD, minE, maxE, intValue);
		}
		int index = GetIndex(array);
		int[] array2 = m_sampleValue[index];
		double[] array3 = new double[array2.Length];
		int num = (1 << BitsPerSample) - 1;
		for (int j = 0; j < array2.Length; j++)
		{
			double minD2 = (Decode[2 * j] as PdfNumber).FloatValue;
			double maxD2 = (Decode[2 * j + 1] as PdfNumber).FloatValue;
			array3[j] = DecodeOutputData(array2[j], num, minD2, maxD2);
		}
		return array3;
	}

	private static int EncodeInputData(double x, double minD, double maxD, double minE, double maxE, int size)
	{
		return (int)Math.Round(Function.ExtractData(Function.FindIntermediateData(x, minD, maxD, minE, maxE), 0.0, size - 1));
	}

	private static double DecodeOutputData(double r, double maxN, double minD, double maxD)
	{
		return Function.FindIntermediateData(r, 0.0, maxN, minD, maxD);
	}

	private int GetIndex(int[] encodedData)
	{
		int num = encodedData[0];
		int num2 = 1;
		for (int i = 1; i < encodedData.Length; i++)
		{
			int intValue = (Size[i - 1] as PdfNumber).IntValue;
			num2 *= intValue;
			num += num2 * encodedData[i];
		}
		return num;
	}

	private byte[] GetDecodedStream(PdfStream stream)
	{
		if (m_filter == "FlateDecode")
		{
			return DecodeFlateStream(stream.InternalStream).ToArray();
		}
		return stream.Data;
	}

	private MemoryStream DecodeFlateStream(MemoryStream encodedStream)
	{
		encodedStream.Position = 0L;
		encodedStream.ReadByte();
		encodedStream.ReadByte();
		DeflateStream deflateStream = new DeflateStream(encodedStream, CompressionMode.Decompress, leaveOpen: true);
		byte[] buffer = new byte[4096];
		MemoryStream memoryStream = new MemoryStream();
		while (true)
		{
			int num = deflateStream.Read(buffer, 0, 4096);
			if (num <= 0)
			{
				break;
			}
			memoryStream.Write(buffer, 0, num);
		}
		return memoryStream;
	}

	private string GetFilter(PdfDictionary dictionary)
	{
		if (dictionary.Items.ContainsKey(new PdfName("Filter")))
		{
			PdfName pdfName = dictionary.Items[new PdfName("Filter")] as PdfName;
			if (pdfName != null)
			{
				return pdfName.Value;
			}
			if (dictionary.Items[new PdfName("Filter")] is PdfArray { Count: 1 } pdfArray)
			{
				return (pdfArray[0] as PdfName).Value;
			}
		}
		return string.Empty;
	}
}

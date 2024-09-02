using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Barcode;

internal class Pdf417BarcodeHelper
{
	private Pdf417Barcode m_barcode;

	private Pdf417ErrorCorrectionLevel m_errorCorrectionLevel = Pdf417ErrorCorrectionLevel.Auto;

	private int dataRows;

	private int dataColums;

	private byte[] inputBinaryData;

	private int inputDataLength;

	private int textDataPosition;

	private List<int> codeWordsList;

	private EncodingMode encodingMode;

	private TextEncodingMode textEncodingMode;

	private EncodingControl encodingControl;

	private float barWidthPixel = 2f;

	private float rowHeightPixel = 6f;

	private float quietZonePixel = 4f;

	private bool[,] pdf417Matrix;

	private int errorCorrectionLength;

	private int[] errorCorrectionCodewords;

	private int defaultDataColumns = 3;

	private bool autoTagcheck;

	private const int SwitchToTextMode = 900;

	private const int SwitchToByteMode = 901;

	private const int SwitchToNumericMode = 902;

	private const int ShiftToByteMode = 913;

	private const int SwitchToByteModeForSix = 924;

	private const int MaxCodeWords = 929;

	private const int DataRowsMax = 90;

	private const int DataColumnsMax = 30;

	private const int CodeWordLength = 17;

	private readonly bool[] StartPattern = new bool[17]
	{
		true, true, true, true, true, true, true, true, false, true,
		false, true, false, true, false, false, false
	};

	private readonly bool[] StopPattern = new bool[18]
	{
		true, true, true, true, true, true, true, false, true, false,
		false, false, true, false, true, false, false, true
	};

	public SizeF Size
	{
		get
		{
			if (m_barcode.Size.IsEmpty)
			{
				return GetBarcodeSize();
			}
			return m_barcode.Size;
		}
		set
		{
			m_barcode.Size = value;
		}
	}

	internal int DataRows
	{
		get
		{
			return dataRows;
		}
		set
		{
			dataRows = value;
		}
	}

	internal int DataColumns
	{
		get
		{
			return dataColums;
		}
		set
		{
			dataColums = value;
		}
	}

	internal int BarcodeColumns => 17 * (DataColumns + 4) + 1;

	internal float BarcodeWidth => barWidthPixel * (float)BarcodeColumns + 2f * QuietZoneNew + (float)((!m_barcode.QuietZone.IsAll && (int)m_barcode.QuietZone.Left > 0) ? ((int)m_barcode.QuietZone.Left) : 0) + (float)((!m_barcode.QuietZone.IsAll && (int)m_barcode.QuietZone.Right > 0) ? ((int)m_barcode.QuietZone.Right) : 0);

	internal float BarcodeHeight => rowHeightPixel * (float)DataRows + 2f * QuietZoneNew + (float)((!m_barcode.QuietZone.IsAll && (int)m_barcode.QuietZone.Top > 0) ? ((int)m_barcode.QuietZone.Top) : 0) + (float)((!m_barcode.QuietZone.IsAll && (int)m_barcode.QuietZone.Bottom > 0) ? ((int)m_barcode.QuietZone.Bottom) : 0);

	internal float RowHeight
	{
		get
		{
			return rowHeightPixel;
		}
		set
		{
			rowHeightPixel = value;
		}
	}

	internal float QuietZoneNew
	{
		get
		{
			return quietZonePixel;
		}
		set
		{
			quietZonePixel = value;
		}
	}

	public Pdf417ErrorCorrectionLevel ErrorCorrectionLevel
	{
		get
		{
			return m_errorCorrectionLevel;
		}
		set
		{
			m_errorCorrectionLevel = value;
		}
	}

	public Pdf417BarcodeHelper(Pdf417Barcode barcode)
	{
		m_barcode = barcode;
	}

	public Stream ToImage()
	{
		if (string.IsNullOrEmpty(m_barcode.Text))
		{
			throw new PdfException("Input data is empty");
		}
		byte[] bytes = Encoding.UTF8.GetBytes(m_barcode.Text);
		QuietZoneNew = GetQuiteZone();
		EncodeTextData(bytes);
		if (pdf417Matrix == null)
		{
			pdf417Matrix = Create417BarcodeMatrix();
		}
		int barcodeColumns = BarcodeColumns;
		int num = DataRows;
		Color color = Color.White;
		Color color2 = Color.Black;
		if (m_barcode.BackColor.A != 0)
		{
			Color color3 = Color.FromArgb(m_barcode.BackColor.ToArgb());
			if (color3 != Color.White)
			{
				color = color3;
			}
		}
		if (m_barcode.ForeColor.A != 0)
		{
			Color color4 = Color.FromArgb(m_barcode.ForeColor.ToArgb());
			if (color4 != Color.Black)
			{
				color2 = color4;
			}
		}
		float num2 = 1f;
		float num3 = 1f;
		if (m_barcode.Size != SizeF.Empty)
		{
			num2 = m_barcode.Size.Width / BarcodeWidth;
			num3 = m_barcode.Size.Height / BarcodeHeight;
		}
		Bitmap bitmap = new Bitmap((int)(BarcodeWidth * num2), (int)(BarcodeHeight * num3));
		GraphicsHelper graphicsHelper = new GraphicsHelper(bitmap);
		graphicsHelper.FillRectangle(color, 0f, 0f, BarcodeWidth * num2, BarcodeHeight * num3);
		float num4 = 0f;
		float num5 = 0f;
		num5 += (float)((!m_barcode.QuietZone.IsAll && (int)m_barcode.QuietZone.Top > 0) ? ((int)m_barcode.QuietZone.Top) : 0);
		num4 += (float)((!m_barcode.QuietZone.IsAll && (int)m_barcode.QuietZone.Left > 0) ? ((int)m_barcode.QuietZone.Left) : 0);
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < barcodeColumns; j++)
			{
				if (pdf417Matrix[i, j])
				{
					graphicsHelper.FillRectangle(color2, num4 + QuietZoneNew, num5 + QuietZoneNew, barWidthPixel * num2, RowHeight * num3);
				}
				num4 += barWidthPixel * num2;
			}
			num4 = ((!m_barcode.QuietZone.IsAll && (int)m_barcode.QuietZone.Left > 0) ? ((int)m_barcode.QuietZone.Left) : 0);
			num5 += RowHeight * num3;
		}
		MemoryStream memoryStream = new MemoryStream();
		bitmap.Save(memoryStream, ImageFormat.Png);
		memoryStream.Position = 0L;
		return memoryStream;
	}

	public Stream ToImage(SizeF size)
	{
		m_barcode.Size = new SizeF(size.Width, size.Height);
		return ToImage();
	}

	private SizeF GetBarcodeSize()
	{
		byte[] bytes = Encoding.UTF8.GetBytes(m_barcode.Text);
		float quiteZone = GetQuiteZone();
		float num = barWidthPixel * (float)(int)m_barcode.XDimension / 2f;
		float num2 = rowHeightPixel * (float)(int)m_barcode.XDimension / 2f;
		EncodeTextData(bytes);
		float width = num * (float)BarcodeColumns + 2f * quiteZone + (float)((!m_barcode.QuietZone.IsAll && (int)m_barcode.QuietZone.Left > 0) ? ((int)m_barcode.QuietZone.Left) : 0) + (float)((!m_barcode.QuietZone.IsAll && (int)m_barcode.QuietZone.Right > 0) ? ((int)m_barcode.QuietZone.Right) : 0);
		float height = num2 * (float)DataRows + 2f * quiteZone + (float)((!m_barcode.QuietZone.IsAll && (int)m_barcode.QuietZone.Top > 0) ? ((int)m_barcode.QuietZone.Top) : 0) + (float)((!m_barcode.QuietZone.IsAll && (int)m_barcode.QuietZone.Bottom > 0) ? ((int)m_barcode.QuietZone.Bottom) : 0);
		return new SizeF(width, height);
	}

	internal float GetQuiteZone()
	{
		float result = 2f;
		if (m_barcode.QuietZone.IsAll && m_barcode.QuietZone.All > 0f)
		{
			result = (int)m_barcode.QuietZone.All;
		}
		return result;
	}

	internal bool[,] Create417BarcodeMatrix()
	{
		int[] array = new int[DataRows * DataColumns];
		int num = 0;
		int num2 = 0;
		if (codeWordsList != null)
		{
			num = codeWordsList.Count;
			for (int i = 0; i < num; i++)
			{
				array[i] = codeWordsList[i];
			}
			num2 = DataRows * DataColumns - codeWordsList.Count - errorCorrectionLength;
		}
		for (int j = 0; j < num2; j++)
		{
			array[num++] = 900;
		}
		if (num <= 928)
		{
			array[0] = num;
		}
		else
		{
			array[0] = 928;
		}
		CalculateErrorCorrection(array);
		bool[,] array2 = new bool[DataRows, BarcodeColumns];
		int num3 = DataRows - 1;
		int num4 = BarcodeColumns - StopPattern.Length;
		int col = num4 - 17;
		for (int k = 0; k < DataRows; k++)
		{
			for (int l = 0; l < StartPattern.Length; l++)
			{
				array2[k, l] = StartPattern[l];
			}
			for (int m = 0; m < StopPattern.Length; m++)
			{
				array2[k, num4 + m] = StopPattern[m];
			}
			int num5 = 30 * (k / 3);
			int num6 = num5;
			switch (k % 3)
			{
			case 0:
				num5 += num3 / 3;
				num6 += DataColumns - 1;
				break;
			case 1:
				num5 += (int)ErrorCorrectionLevel * 3 + num3 % 3;
				num6 += num3 / 3;
				break;
			default:
				num5 += DataColumns - 1;
				num6 += (int)ErrorCorrectionLevel * 3 + num3 % 3;
				break;
			}
			CodewordToModules(k, StartPattern.Length, num5, array2);
			CodewordToModules(k, col, num6, array2);
			for (int n = 0; n < DataColumns; n++)
			{
				int num7 = DataColumns * k + n;
				CodewordToModules(k, 17 * (n + 2), array[num7], array2);
			}
		}
		pdf417Matrix = array2;
		return array2;
	}

	internal void CodewordToModules(int Row, int Col, int Codeword, bool[,] Matrix)
	{
		Matrix[Row, Col] = true;
		int num = Pdf417Tables.CodewordTable[Row % 3, Codeword];
		int num2 = 16384;
		for (int i = 1; i < 17; i++)
		{
			if ((num & num2) != 0)
			{
				Matrix[Row, Col + i] = true;
			}
			num2 >>= 1;
		}
	}

	internal void CalculateErrorCorrection(int[] Codewords)
	{
		int[] array = Pdf417Tables.ErrorCorrectionTables[(int)ErrorCorrectionLevel];
		errorCorrectionCodewords = new int[errorCorrectionLength];
		int num = errorCorrectionLength - 1;
		int num2 = Codewords[0];
		for (int i = 0; i < num2; i++)
		{
			int num3 = (Codewords[i] + errorCorrectionCodewords[num]) % 929;
			for (int num4 = num; num4 > 0; num4--)
			{
				errorCorrectionCodewords[num4] = (929 + errorCorrectionCodewords[num4 - 1] - num3 * array[num4]) % 929;
			}
			errorCorrectionCodewords[0] = (929 - num3 * array[0]) % 929;
		}
		for (int num5 = num; num5 >= 0; num5--)
		{
			errorCorrectionCodewords[num5] = (929 - errorCorrectionCodewords[num5]) % 929;
		}
		for (int j = 0; j < errorCorrectionLength; j++)
		{
			Codewords[num2 + j] = errorCorrectionCodewords[num - j];
		}
	}

	internal void EncodeTextData(byte[] byteData)
	{
		pdf417Matrix = null;
		inputBinaryData = byteData;
		DataEncoding();
		DetermineCorrectionLevel();
		int num = defaultDataColumns;
		int num2 = 0;
		if (codeWordsList != null)
		{
			num2 = (codeWordsList.Count + errorCorrectionLength + num - 1) / num;
			if (num2 > 90)
			{
				num2 = 90;
				num = (codeWordsList.Count + errorCorrectionLength + num2 - 1) / num2;
				if (num > 30)
				{
					throw new BarcodeException("Data overflow for PDF417Barcode");
				}
			}
		}
		DataRows = num2;
		DataColumns = num;
	}

	internal void DetermineCorrectionLevel()
	{
		if (ErrorCorrectionLevel >= Pdf417ErrorCorrectionLevel.Level0 && ErrorCorrectionLevel <= Pdf417ErrorCorrectionLevel.Level8)
		{
			ErrorCorrectionLevel = m_errorCorrectionLevel;
		}
		else
		{
			int num = ((codeWordsList != null) ? codeWordsList.Count : 0);
			if (num <= 40)
			{
				ErrorCorrectionLevel = Pdf417ErrorCorrectionLevel.Level2;
			}
			else if (num <= 160)
			{
				ErrorCorrectionLevel = Pdf417ErrorCorrectionLevel.Level3;
			}
			else if (num <= 320)
			{
				ErrorCorrectionLevel = Pdf417ErrorCorrectionLevel.Level4;
			}
			else if (num <= 863)
			{
				ErrorCorrectionLevel = Pdf417ErrorCorrectionLevel.Level5;
			}
			else
			{
				ErrorCorrectionLevel = Pdf417ErrorCorrectionLevel.Level6;
			}
		}
		errorCorrectionLength = 1 << (int)(ErrorCorrectionLevel + 1);
	}

	private int CountText()
	{
		int num = 0;
		int i = 0;
		if (inputBinaryData != null)
		{
			for (i = textDataPosition; i < inputDataLength; i++)
			{
				int num2 = inputBinaryData[i];
				if ((num2 < 32 && num2 != 13 && num2 != 10 && num2 != 9) || num2 > 126)
				{
					break;
				}
				num++;
			}
		}
		return i - textDataPosition;
	}

	private int CountPunctuation(int CurrentTextCount)
	{
		int num = 0;
		if (inputBinaryData != null)
		{
			while (CurrentTextCount > 0)
			{
				int num2 = inputBinaryData[textDataPosition + num];
				if (Pdf417Tables.TextToPunctuation[num2] == 127)
				{
					return 0;
				}
				num++;
				if (num == 3)
				{
					return 3;
				}
			}
		}
		return 0;
	}

	private int CountBytes()
	{
		int num = 0;
		int i;
		for (i = textDataPosition; i < inputDataLength; i++)
		{
			if (inputBinaryData == null)
			{
				break;
			}
			int num2 = inputBinaryData[i];
			if ((num2 < 32 && num2 != 13 && num2 != 10 && num2 != 9) || num2 > 126)
			{
				num = 0;
				continue;
			}
			num++;
			if (num >= 5)
			{
				i -= 4;
				break;
			}
		}
		return i - textDataPosition;
	}

	private void EncodeTextSegment(int TotalCount)
	{
		if (codeWordsList == null)
		{
			return;
		}
		if (encodingMode != EncodingMode.Text)
		{
			codeWordsList.Add(900);
			encodingMode = EncodingMode.Text;
			textEncodingMode = TextEncodingMode.Upper;
		}
		List<int> list = new List<int>();
		while (TotalCount > 0 && inputBinaryData != null)
		{
			int num = inputBinaryData[textDataPosition++];
			TotalCount--;
			switch (textEncodingMode)
			{
			case TextEncodingMode.Upper:
			{
				int num2 = Pdf417Tables.TextToUpper[num];
				if (num2 != 127)
				{
					list.Add(num2);
					break;
				}
				num2 = Pdf417Tables.TextToLower[num];
				if (num2 != 127)
				{
					list.Add(27);
					list.Add(num2);
					textEncodingMode = TextEncodingMode.Lower;
					break;
				}
				num2 = Pdf417Tables.TextToMixed[num];
				if (num2 != 127)
				{
					list.Add(28);
					list.Add(num2);
					textEncodingMode = TextEncodingMode.Mixed;
					break;
				}
				num2 = Pdf417Tables.TextToPunctuation[num];
				if (num2 != 127)
				{
					if (CountPunctuation(TotalCount) > 0)
					{
						list.Add(28);
						list.Add(25);
						list.Add(num2);
						textEncodingMode = TextEncodingMode.Punctuation;
					}
					else
					{
						list.Add(29);
						list.Add(num2);
					}
				}
				break;
			}
			case TextEncodingMode.Lower:
			{
				int num2 = Pdf417Tables.TextToLower[num];
				if (num2 != 127)
				{
					list.Add(num2);
					break;
				}
				num2 = Pdf417Tables.TextToUpper[num];
				if (num2 != 127)
				{
					list.Add(27);
					list.Add(num2);
					break;
				}
				num2 = Pdf417Tables.TextToMixed[num];
				if (num2 != 127)
				{
					list.Add(28);
					list.Add(num2);
					textEncodingMode = TextEncodingMode.Mixed;
					break;
				}
				num2 = Pdf417Tables.TextToPunctuation[num];
				if (num2 != 127)
				{
					if (CountPunctuation(TotalCount) > 0)
					{
						list.Add(28);
						list.Add(25);
						list.Add(num2);
						textEncodingMode = TextEncodingMode.Punctuation;
					}
					else
					{
						list.Add(29);
						list.Add(num2);
					}
				}
				break;
			}
			case TextEncodingMode.Mixed:
			{
				int num2 = Pdf417Tables.TextToMixed[num];
				if (num2 != 127)
				{
					list.Add(num2);
					break;
				}
				num2 = Pdf417Tables.TextToLower[num];
				if (num2 != 127)
				{
					list.Add(27);
					list.Add(num2);
					textEncodingMode = TextEncodingMode.Lower;
					break;
				}
				num2 = Pdf417Tables.TextToUpper[num];
				if (num2 != 127)
				{
					list.Add(28);
					list.Add(num2);
					textEncodingMode = TextEncodingMode.Upper;
					break;
				}
				num2 = Pdf417Tables.TextToPunctuation[num];
				if (num2 != 127)
				{
					if (CountPunctuation(TotalCount) > 0)
					{
						list.Add(25);
						list.Add(num2);
						textEncodingMode = TextEncodingMode.Punctuation;
					}
					else
					{
						list.Add(29);
						list.Add(num2);
					}
				}
				break;
			}
			case TextEncodingMode.Punctuation:
			{
				int num2 = Pdf417Tables.TextToPunctuation[num];
				if (num2 != 127)
				{
					list.Add(num2);
					break;
				}
				list.Add(29);
				textEncodingMode = TextEncodingMode.Upper;
				goto case TextEncodingMode.Upper;
			}
			}
		}
		int num3 = list.Count & -2;
		for (int i = 0; i < num3; i += 2)
		{
			codeWordsList.Add(30 * list[i] + list[i + 1]);
		}
		if (((uint)list.Count & (true ? 1u : 0u)) != 0)
		{
			codeWordsList.Add(30 * list[num3] + 29);
		}
	}

	private void EncodeByteSegment(int Count)
	{
		if (codeWordsList == null)
		{
			return;
		}
		if (Count == 1 && encodingMode == EncodingMode.Text && inputBinaryData != null)
		{
			codeWordsList.Add(913);
			codeWordsList.Add(inputBinaryData[textDataPosition++]);
			return;
		}
		codeWordsList.Add((Count % 6 == 0) ? 924 : 901);
		encodingMode = EncodingMode.Byte;
		int num = textDataPosition + Count;
		if (Count >= 6)
		{
			while (num - textDataPosition >= 6 && inputBinaryData != null)
			{
				long num2 = (long)(((ulong)inputBinaryData[textDataPosition++] << 40) | ((ulong)inputBinaryData[textDataPosition++] << 32) | ((ulong)inputBinaryData[textDataPosition++] << 24) | ((ulong)inputBinaryData[textDataPosition++] << 16) | ((ulong)inputBinaryData[textDataPosition++] << 8) | inputBinaryData[textDataPosition++]);
				for (int num3 = 4; num3 > 0; num3--)
				{
					long num4 = num2 / Pdf417Tables.Factorial[num3];
					codeWordsList.Add((int)num4);
					num2 %= Pdf417Tables.Factorial[num3];
				}
				codeWordsList.Add((int)num2);
			}
		}
		while (textDataPosition < num && inputBinaryData != null)
		{
			codeWordsList.Add(inputBinaryData[textDataPosition++]);
		}
	}

	private void DataEncoding()
	{
		codeWordsList = new List<int>();
		codeWordsList.Add(0);
		textDataPosition = 0;
		inputDataLength = ((inputBinaryData != null) ? inputBinaryData.Length : 0);
		encodingMode = EncodingMode.Text;
		textEncodingMode = TextEncodingMode.Upper;
		while (textDataPosition < inputDataLength)
		{
			int num = CountText();
			if (num >= 5)
			{
				EncodeTextSegment(num);
				continue;
			}
			int count = CountBytes();
			EncodeByteSegment(count);
		}
	}
}

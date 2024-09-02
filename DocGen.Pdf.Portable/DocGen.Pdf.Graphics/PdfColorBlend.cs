using System;
using System.Text;
using DocGen.Pdf.Functions;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics;

public sealed class PdfColorBlend : PdfBlendBase
{
	private PdfColor[] m_colors;

	private PdfBrush m_brush;

	public PdfColor[] Colors
	{
		get
		{
			return m_colors;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Colors");
			}
			m_colors = SetArray(value) as PdfColor[];
		}
	}

	public PdfColorBlend()
	{
	}

	public PdfColorBlend(int count)
		: base(count)
	{
	}

	internal PdfColorBlend(PdfBrush brush)
	{
		m_brush = brush;
	}

	internal PdfFunction GetFunction(PdfColorSpace colorSpace)
	{
		float[] domain = new float[2] { 0f, 1f };
		int colorComponentsCount = GetColorComponentsCount(colorSpace);
		int maxComponentValue = GetMaxComponentValue(colorSpace);
		float[] array = SetRange(colorComponentsCount, maxComponentValue);
		PdfSampledFunction result = null;
		if (m_brush == null)
		{
			int[] array2 = new int[1];
			float step = 1f;
			int num;
			if (base.Positions.Length == 2)
			{
				num = 2;
			}
			else
			{
				float[] positions = base.Positions;
				float num2 = PdfBlendBase.Gcd(GetIntervals(positions));
				step = num2;
				num = (int)(1f / num2) + 1;
			}
			array2[0] = num;
			byte[] samplesValues = GetSamplesValues(colorSpace, num, maxComponentValue, step);
			return new PdfSampledFunction(domain, array, array2, samplesValues);
		}
		if (m_brush is PdfLinearGradientBrush || m_brush is PdfRadialGradientBrush)
		{
			PdfLinearGradientBrush pdfLinearGradientBrush = m_brush as PdfLinearGradientBrush;
			PdfRadialGradientBrush pdfRadialGradientBrush = m_brush as PdfRadialGradientBrush;
			if ((pdfLinearGradientBrush != null && pdfLinearGradientBrush.Extend == PdfExtend.Both) || pdfRadialGradientBrush != null)
			{
				PdfStitchingFunction pdfStitchingFunction = new PdfStitchingFunction();
				PdfArray pdfArray = new PdfArray();
				StringBuilder stringBuilder = new StringBuilder();
				StringBuilder stringBuilder2 = new StringBuilder();
				for (int i = 1; i < base.Positions.Length; i++)
				{
					PdfExponentialInterpolationFunction pdfExponentialInterpolationFunction = new PdfExponentialInterpolationFunction(Init: true);
					pdfExponentialInterpolationFunction.Domain = new PdfArray(new float[2] { 0f, 1f });
					pdfExponentialInterpolationFunction.Range = new PdfArray(array);
					float[] array3 = new float[3]
					{
						Colors[i - 1].Red,
						Colors[i - 1].Green,
						Colors[i - 1].Blue
					};
					float[] array4 = new float[3]
					{
						Colors[i].Red,
						Colors[i].Green,
						Colors[i].Blue
					};
					pdfExponentialInterpolationFunction.Dictionary["FunctionType"] = new PdfNumber(2);
					pdfExponentialInterpolationFunction.Dictionary["N"] = new PdfNumber(1);
					pdfExponentialInterpolationFunction.Dictionary["C0"] = new PdfArray(array3);
					pdfExponentialInterpolationFunction.Dictionary["C1"] = new PdfArray(array4);
					if (i > 1)
					{
						stringBuilder.Append(' ');
						stringBuilder2.Append(' ');
					}
					if (i < base.Positions.Length - 1)
					{
						stringBuilder.Append(base.Positions[i]);
					}
					if (pdfLinearGradientBrush != null)
					{
						stringBuilder2.Append("0 1");
					}
					else if (pdfRadialGradientBrush != null)
					{
						stringBuilder2.Append("1 0");
					}
					PdfReferenceHolder element = new PdfReferenceHolder(pdfExponentialInterpolationFunction);
					pdfArray.Add(element);
				}
				float[] array5 = new float[stringBuilder2.ToString().Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length];
				float[] array6 = new float[stringBuilder.ToString().Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length];
				for (int j = 0; j < array5.Length; j++)
				{
					array5[j] = float.Parse(stringBuilder2.ToString().Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[j]);
				}
				for (int k = 0; k < array6.Length; k++)
				{
					array6[k] = float.Parse(stringBuilder.ToString().Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[k]);
				}
				pdfStitchingFunction.Dictionary["Bounds"] = new PdfArray(array6);
				pdfStitchingFunction.Dictionary["Encode"] = new PdfArray(array5);
				if (pdfRadialGradientBrush != null)
				{
					pdfStitchingFunction.Range = new PdfArray(array);
				}
				pdfStitchingFunction.Domain = new PdfArray(new float[2] { 0f, 1f });
				pdfStitchingFunction.Dictionary["Functions"] = pdfArray;
				pdfStitchingFunction.Dictionary["FunctionType"] = new PdfNumber(3);
				return pdfStitchingFunction;
			}
			if (pdfLinearGradientBrush != null)
			{
				pdfLinearGradientBrush.Extend = PdfExtend.Both;
			}
		}
		return result;
	}

	internal PdfColorBlend CloneColorBlend()
	{
		PdfColorBlend pdfColorBlend = MemberwiseClone() as PdfColorBlend;
		if (m_colors != null)
		{
			pdfColorBlend.Colors = m_colors.Clone() as PdfColor[];
		}
		if (base.Positions != null)
		{
			pdfColorBlend.Positions = base.Positions.Clone() as float[];
		}
		return pdfColorBlend;
	}

	private static float[] SetRange(int colourComponents, float maxValue)
	{
		float[] array = new float[colourComponents * 2];
		for (int i = 0; i < colourComponents; i++)
		{
			array[i * 2] = 0f;
			array[i * 2 + 1] = 1f;
		}
		return array;
	}

	private static int GetColorComponentsCount(PdfColorSpace colorSpace)
	{
		int num = 0;
		return colorSpace switch
		{
			PdfColorSpace.RGB => 3, 
			PdfColorSpace.CMYK => 4, 
			PdfColorSpace.GrayScale => 1, 
			_ => throw new ArgumentException("Unsupported color space: " + colorSpace, "colorSpace"), 
		};
	}

	private byte[] GetSamplesValues(PdfColorSpace colorSpace, int sampleCount, int maxComponentValue, float step)
	{
		return colorSpace switch
		{
			PdfColorSpace.GrayScale => GetGrayscaleSamples(sampleCount, maxComponentValue, step), 
			PdfColorSpace.CMYK => GetCmykSamples(sampleCount, maxComponentValue, step), 
			PdfColorSpace.RGB => GetRgbSamples(sampleCount, maxComponentValue, step), 
			_ => throw new ArgumentException("Unsupported color space: " + colorSpace, "colorSpace"), 
		};
	}

	private byte[] GetGrayscaleSamples(int sampleCount, int maxComponentValue, float step)
	{
		byte[] array = new byte[sampleCount * 2];
		for (int i = 0; i < sampleCount; i++)
		{
			PdfColor nextColor = GetNextColor(i, step, PdfColorSpace.GrayScale);
			int num = i * 2;
			byte[] bytes = BitConverter.GetBytes((short)(nextColor.Gray * (float)maxComponentValue));
			array[num] = bytes[0];
			array[num + 1] = bytes[1];
		}
		return array;
	}

	private byte[] GetCmykSamples(int sampleCount, int maxComponentValue, float step)
	{
		byte[] array = new byte[sampleCount * 4];
		for (int i = 0; i < sampleCount; i++)
		{
			PdfColor nextColor = GetNextColor(i, step, PdfColorSpace.CMYK);
			int num = i * 4;
			array[num] = (byte)(nextColor.C * (float)maxComponentValue);
			array[num + 1] = (byte)(nextColor.M * (float)maxComponentValue);
			array[num + 2] = (byte)(nextColor.Y * (float)maxComponentValue);
			array[num + 3] = (byte)(nextColor.K * (float)maxComponentValue);
		}
		return array;
	}

	private byte[] GetRgbSamples(int sampleCount, int maxComponentValue, float step)
	{
		byte[] array = new byte[sampleCount * 3];
		for (int i = 0; i < sampleCount; i++)
		{
			PdfColor nextColor = GetNextColor(i, step, PdfColorSpace.RGB);
			int num = i * 3;
			array[num] = nextColor.R;
			array[num + 1] = nextColor.G;
			array[num + 2] = nextColor.B;
		}
		return array;
	}

	private PdfColor GetNextColor(int index, float step, PdfColorSpace colorSpace)
	{
		float num = step * (float)index;
		GetIndices(num, out var indexLow, out var indexHi);
		if (indexLow == indexHi)
		{
			return m_colors[indexLow];
		}
		float num2 = base.Positions[indexLow];
		float num3 = base.Positions[indexHi];
		PdfColor color = m_colors[indexLow];
		PdfColor color2 = m_colors[indexHi];
		return PdfBlendBase.Interpolate((num - num2) / (num3 - num2), color, color2, colorSpace);
	}

	private void GetIndices(float position, out int indexLow, out int indexHi)
	{
		float[] positions = base.Positions;
		indexLow = 0;
		indexHi = 0;
		for (int i = 0; i < m_colors.Length; i++)
		{
			float num = positions[i];
			if (num == position)
			{
				indexLow = (indexHi = i);
				break;
			}
			if (num > position)
			{
				indexHi = i;
				break;
			}
			indexLow = i;
			indexHi = i;
		}
	}

	private int GetMaxComponentValue(PdfColorSpace colorSpace)
	{
		int num = 0;
		switch (colorSpace)
		{
		case PdfColorSpace.RGB:
		case PdfColorSpace.CMYK:
			return 255;
		case PdfColorSpace.GrayScale:
			return 65535;
		default:
			throw new ArgumentException("Unsupported color space: " + colorSpace, "colorSpace");
		}
	}

	private float[] GetIntervals(float[] positions)
	{
		int num = positions.Length;
		float[] array = new float[num - 1];
		float num2 = positions[0];
		for (int i = 1; i < num; i++)
		{
			float num3 = positions[i];
			array[i - 1] = num3 - num2;
			num2 = num3;
		}
		return array;
	}
}

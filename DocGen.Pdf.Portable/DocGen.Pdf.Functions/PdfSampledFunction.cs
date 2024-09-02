using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Functions;

internal class PdfSampledFunction : PdfFunction
{
	internal PdfSampledFunction(float[] domain, float[] range, int[] sizes, byte[] samples)
		: this()
	{
		CheckParams(domain, range, sizes, samples);
		SetDomainAndRange(domain, range);
		SetSizeAndValues(sizes, samples);
	}

	internal PdfSampledFunction(float[] domain, float[] range, int[] sizes, int[] samples)
		: this()
	{
		CheckParams(domain, range, sizes, samples);
		SetDomainAndRange(domain, range);
		SetSizeAndValues(sizes, samples);
	}

	internal PdfSampledFunction(float[] domain, float[] range, int[] sizes, float[] samples, int bps)
		: this()
	{
		CheckParams(domain, range, sizes, samples);
		_ = base.Dictionary;
	}

	private PdfSampledFunction()
		: base(new PdfStream())
	{
		base.Dictionary.SetProperty("FunctionType", new PdfNumber(0));
	}

	private void CheckParams(float[] domain, float[] range, int[] sizes, Array samples)
	{
		if (domain == null)
		{
			throw new ArgumentNullException("domain");
		}
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		if (samples == null)
		{
			throw new ArgumentNullException("samples");
		}
		int num = range.Length;
		int num2 = domain.Length;
		int length = samples.Length;
		if (num2 <= 0)
		{
			throw new ArgumentException("The array has no enough elements", "domain");
		}
		if (num <= 0)
		{
			throw new ArgumentException("The array has no enough elements", "range");
		}
		double num3 = num * num2 / 4;
		if ((double)length < num3)
		{
			throw new ArgumentException("There is no enough samples", "samples");
		}
	}

	private void SetDomainAndRange(float[] domain, float[] range)
	{
		base.Domain = new PdfArray(domain);
		base.Range = new PdfArray(range);
	}

	private void SetSizeAndValues(int[] sizes, byte[] samples)
	{
		PdfStream obj = base.Dictionary as PdfStream;
		base.Dictionary.SetProperty("Size", new PdfArray(sizes));
		base.Dictionary.SetProperty("BitsPerSample", new PdfNumber(8));
		obj.Write(samples);
	}

	private void SetSizeAndValues(int[] sizes, int[] samples)
	{
		PdfStream pdfStream = base.Dictionary as PdfStream;
		base.Dictionary.SetProperty("Size", new PdfArray(sizes));
		base.Dictionary.SetProperty("BitsPerSample", new PdfNumber(32));
		byte[] array = new byte[samples.Length * 4];
		int num = 0;
		for (int i = 0; i < samples.Length; i++)
		{
			BitConverter.GetBytes(samples[i]).CopyTo(array, num);
			num += 4;
		}
		pdfStream.Write(array);
	}
}

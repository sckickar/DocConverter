using System;

namespace DocGen.Pdf.Security;

internal abstract class BufferedBlockPaddingBase : IBufferedCipher
{
	protected static readonly byte[] EmptyBuffer = new byte[0];

	public abstract string AlgorithmName { get; }

	public abstract int BlockSize { get; }

	public abstract void Initialize(bool isEncryption, ICipherParam parameters);

	public abstract int GetOutputSize(int inputLen);

	public abstract int GetUpdateOutputSize(int inputLen);

	public abstract byte[] ProcessByte(byte input);

	public abstract void Reset();

	public abstract byte[] ProcessBytes(byte[] input, int inOff, int length);

	public abstract byte[] DoFinal();

	public abstract byte[] DoFinal(byte[] input, int inOff, int length);

	public virtual int ProcessByte(byte input, byte[] output, int outOff)
	{
		byte[] array = ProcessByte(input);
		if (array == null)
		{
			return 0;
		}
		if (outOff + array.Length > output.Length)
		{
			throw new Exception("output buffer too short");
		}
		array.CopyTo(output, outOff);
		return array.Length;
	}

	public virtual byte[] ProcessBytes(byte[] input)
	{
		return ProcessBytes(input, 0, input.Length);
	}

	public virtual int ProcessBytes(byte[] input, byte[] output, int outOff)
	{
		return ProcessBytes(input, 0, input.Length, output, outOff);
	}

	public virtual int ProcessBytes(byte[] input, int inOff, int length, byte[] output, int outOff)
	{
		byte[] array = ProcessBytes(input, inOff, length);
		if (array == null)
		{
			return 0;
		}
		if (outOff + array.Length > output.Length)
		{
			throw new Exception("output buffer too short");
		}
		array.CopyTo(output, outOff);
		return array.Length;
	}

	public virtual byte[] DoFinal(byte[] input)
	{
		return DoFinal(input, 0, input.Length);
	}

	public virtual int DoFinal(byte[] output, int outOff)
	{
		byte[] array = DoFinal();
		if (outOff + array.Length > output.Length)
		{
			throw new Exception("output buffer too short");
		}
		array.CopyTo(output, outOff);
		return array.Length;
	}

	public virtual int DoFinal(byte[] input, byte[] output, int outOff)
	{
		return DoFinal(input, 0, input.Length, output, outOff);
	}

	public virtual int DoFinal(byte[] input, int inOff, int length, byte[] output, int outOff)
	{
		int num = ProcessBytes(input, inOff, length, output, outOff);
		return num + DoFinal(output, outOff + num);
	}
}

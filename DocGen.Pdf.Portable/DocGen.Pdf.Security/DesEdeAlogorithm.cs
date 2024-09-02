using System;

namespace DocGen.Pdf.Security;

internal class DesEdeAlogorithm : DataEncryption
{
	private int[] m_key1;

	private int[] m_Key2;

	private int[] m_Key3;

	private bool m_isEncryption;

	public override int BlockSize => 8;

	public override string AlgorithmName => "DESede";

	public override void Initialize(bool forEncryption, ICipherParam parameters)
	{
		if (!(parameters is KeyParameter))
		{
			throw new ArgumentException("Invalid parameter");
		}
		byte[] keys = ((KeyParameter)parameters).Keys;
		if (keys.Length != 24 && keys.Length != 16)
		{
			throw new ArgumentException("Invalid key size. Size must be 16 or 24 bytes.");
		}
		m_isEncryption = forEncryption;
		byte[] array = new byte[8];
		Array.Copy(keys, 0, array, 0, array.Length);
		m_key1 = DataEncryption.GenerateWorkingKey(forEncryption, array);
		byte[] array2 = new byte[8];
		Array.Copy(keys, 8, array2, 0, array2.Length);
		m_Key2 = DataEncryption.GenerateWorkingKey(!forEncryption, array2);
		if (keys.Length == 24)
		{
			byte[] array3 = new byte[8];
			Array.Copy(keys, 16, array3, 0, array3.Length);
			m_Key3 = DataEncryption.GenerateWorkingKey(forEncryption, array3);
		}
		else
		{
			m_Key3 = m_key1;
		}
	}

	public override int ProcessBlock(byte[] inputBytes, int inOffset, byte[] outputBytes, int outOffset)
	{
		if (m_key1 == null)
		{
			throw new InvalidOperationException("Data Encryption Standard - Encrypt Decrypt Encrypt not initialised");
		}
		if (inOffset + 8 > inputBytes.Length)
		{
			throw new Exception("Invalid length in input bytes");
		}
		if (outOffset + 8 > outputBytes.Length)
		{
			throw new Exception("Invalid length in output bytes");
		}
		byte[] array = new byte[8];
		if (m_isEncryption)
		{
			DataEncryption.EncryptData(m_key1, inputBytes, inOffset, array, 0);
			DataEncryption.EncryptData(m_Key2, array, 0, array, 0);
			DataEncryption.EncryptData(m_Key3, array, 0, outputBytes, outOffset);
		}
		else
		{
			DataEncryption.EncryptData(m_Key3, inputBytes, inOffset, array, 0);
			DataEncryption.EncryptData(m_Key2, array, 0, array, 0);
			DataEncryption.EncryptData(m_key1, array, 0, outputBytes, outOffset);
		}
		return 8;
	}

	public override void Reset()
	{
	}
}

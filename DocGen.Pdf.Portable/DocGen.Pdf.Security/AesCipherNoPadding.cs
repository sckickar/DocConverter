using System;

namespace DocGen.Pdf.Security;

internal class AesCipherNoPadding
{
	private ICipher m_cbc;

	public AesCipherNoPadding(bool isEncryption, byte[] key)
	{
		ICipher cipher = new AesEngine();
		m_cbc = new CipherBlockChainingMode(cipher);
		KeyParameter parameters = new KeyParameter(key);
		m_cbc.Initialize(isEncryption, parameters);
	}

	internal byte[] ProcessBlock(byte[] inp, int inpOff, int inpLen)
	{
		if (inpLen % m_cbc.BlockSize != 0)
		{
			throw new ArgumentException("Not multiple of block: " + inpLen);
		}
		byte[] array = new byte[inpLen];
		int num = 0;
		while (inpLen > 0)
		{
			m_cbc.ProcessBlock(inp, inpOff, array, num);
			inpLen -= m_cbc.BlockSize;
			num += m_cbc.BlockSize;
			inpOff += m_cbc.BlockSize;
		}
		return array;
	}
}

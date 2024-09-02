using System;

namespace DocGen.Pdf.Security;

internal class AesCipher
{
	private BufferedCipher m_bp;

	public AesCipher(bool isEncryption, byte[] key, byte[] iv)
	{
		ICipher cipher = new CipherBlockChainingMode(new AesEngine());
		m_bp = new BufferedCipher(cipher);
		InvalidParameter parameters = new InvalidParameter(new KeyParameter(key), iv);
		m_bp.Initialize(isEncryption, parameters);
	}

	internal byte[] Update(byte[] inp, int inpOff, int inpLen)
	{
		int updateOutputSize = m_bp.GetUpdateOutputSize(inpLen);
		byte[] array = null;
		if (updateOutputSize > 0)
		{
			array = new byte[updateOutputSize];
		}
		else
		{
			updateOutputSize = 0;
		}
		m_bp.ProcessBytes(inp, inpOff, inpLen, array, 0);
		return array;
	}

	internal byte[] DoFinal()
	{
		byte[] array = new byte[m_bp.GetOutputSize(0)];
		int num = 0;
		try
		{
			num = m_bp.DoFinal(array, 0);
		}
		catch
		{
			return array;
		}
		if (num != array.Length)
		{
			byte[] array2 = new byte[num];
			Array.Copy(array, 0, array2, 0, num);
			return array2;
		}
		return array;
	}
}

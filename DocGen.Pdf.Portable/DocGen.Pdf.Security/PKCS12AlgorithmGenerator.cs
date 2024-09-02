using System;

namespace DocGen.Pdf.Security;

internal class PKCS12AlgorithmGenerator : PasswordGenerator
{
	private const int m_keyMaterial = 1;

	private const int m_invaidMaterial = 2;

	private const int m_macMaterial = 3;

	private IMessageDigest m_digest;

	private int m_size;

	private int m_length;

	internal PKCS12AlgorithmGenerator(IMessageDigest digest)
	{
		m_digest = digest;
		m_size = digest.MessageDigestSize;
		m_length = digest.ByteLength;
	}

	private void Adjust(byte[] a, int offset, byte[] b)
	{
		int num = (b[^1] & 0xFF) + (a[offset + b.Length - 1] & 0xFF) + 1;
		a[offset + b.Length - 1] = (byte)num;
		num >>>= 8;
		for (int num2 = b.Length - 2; num2 >= 0; num2--)
		{
			num += (b[num2] & 0xFF) + (a[offset + num2] & 0xFF);
			a[offset + num2] = (byte)num;
			num >>>= 8;
		}
	}

	private byte[] GenerateDerivedKey(int id, int length)
	{
		byte[] array = new byte[m_length];
		byte[] array2 = new byte[length];
		for (int i = 0; i != array.Length; i++)
		{
			array[i] = (byte)id;
		}
		byte[] array3;
		if (m_value != null && m_value.Length != 0)
		{
			array3 = new byte[m_length * ((m_value.Length + m_length - 1) / m_length)];
			for (int j = 0; j != array3.Length; j++)
			{
				array3[j] = m_value[j % m_value.Length];
			}
		}
		else
		{
			array3 = new byte[0];
		}
		byte[] array4;
		if (m_password != null && m_password.Length != 0)
		{
			array4 = new byte[m_length * ((m_password.Length + m_length - 1) / m_length)];
			for (int k = 0; k != array4.Length; k++)
			{
				array4[k] = m_password[k % m_password.Length];
			}
		}
		else
		{
			array4 = new byte[0];
		}
		byte[] array5 = new byte[array3.Length + array4.Length];
		Array.Copy(array3, 0, array5, 0, array3.Length);
		Array.Copy(array4, 0, array5, array3.Length, array4.Length);
		byte[] array6 = new byte[m_length];
		int num = (length + m_size - 1) / m_size;
		byte[] array7 = new byte[m_size];
		for (int l = 1; l <= num; l++)
		{
			m_digest.Update(array, 0, array.Length);
			m_digest.Update(array5, 0, array5.Length);
			m_digest.DoFinal(array7, 0);
			for (int m = 1; m != m_count; m++)
			{
				m_digest.Update(array7, 0, array7.Length);
				m_digest.DoFinal(array7, 0);
			}
			for (int n = 0; n != array6.Length; n++)
			{
				array6[n] = array7[n % array7.Length];
			}
			for (int num2 = 0; num2 != array5.Length / m_length; num2++)
			{
				Adjust(array5, num2 * m_length, array6);
			}
			if (l == num)
			{
				Array.Copy(array7, 0, array2, (l - 1) * m_size, array2.Length - (l - 1) * m_size);
			}
			else
			{
				Array.Copy(array7, 0, array2, (l - 1) * m_size, array7.Length);
			}
		}
		return array2;
	}

	internal override ICipherParam GenerateParam(string algorithm, int keySize)
	{
		keySize /= 8;
		byte[] bytes = GenerateDerivedKey(1, keySize);
		return new ParamUtility().CreateKeyParameter(algorithm, bytes, 0, keySize);
	}

	internal override ICipherParam GenerateParam(string algorithm, int keySize, int ivSize)
	{
		keySize /= 8;
		ivSize /= 8;
		byte[] bytes = GenerateDerivedKey(1, keySize);
		KeyParameter parameters = new ParamUtility().CreateKeyParameter(algorithm, bytes, 0, keySize);
		byte[] bytes2 = GenerateDerivedKey(2, ivSize);
		return new InvalidParameter(parameters, bytes2, 0, ivSize);
	}

	internal override ICipherParam GenerateParam(int keySize)
	{
		keySize /= 8;
		return new KeyParameter(GenerateDerivedKey(3, keySize), 0, keySize);
	}
}

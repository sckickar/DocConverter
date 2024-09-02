using System;
using System.Text;

namespace DocGen.Pdf.Security;

internal abstract class PasswordGenerator
{
	protected byte[] m_password;

	protected byte[] m_value;

	protected int m_count;

	internal abstract ICipherParam GenerateParam(string algorithm, int keySize);

	internal abstract ICipherParam GenerateParam(string algorithm, int keySize, int size);

	internal abstract ICipherParam GenerateParam(int keySize);

	internal virtual void Init(byte[] password, byte[] value, int count)
	{
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		m_password = Asn1Constants.Clone(password);
		m_value = Asn1Constants.Clone(value);
		m_count = count;
	}

	internal static byte[] ToBytes(char[] password, bool isWrong)
	{
		if (password == null || password.Length < 1)
		{
			return new byte[isWrong ? 2 : 0];
		}
		byte[] array = new byte[(password.Length + 1) * 2];
		Encoding.BigEndianUnicode.GetBytes(password, 0, password.Length, array, 0);
		return array;
	}
}

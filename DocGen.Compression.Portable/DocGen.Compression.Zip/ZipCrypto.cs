using System;
using System.IO;
using System.Text;

namespace DocGen.Compression.Zip;

internal class ZipCrypto
{
	private Stream m_dataStream;

	private string m_password;

	private uint m_iCrc;

	private uint[] m_Keys;

	private ZipCrc32 m_crc32;

	internal ZipCrypto(Stream dataStream, string password, uint crc)
	{
		m_dataStream = dataStream;
		m_iCrc = crc;
		UpdatePassword(password);
	}

	internal ZipCrypto(string password, uint crc)
	{
		UpdatePassword(password);
		m_iCrc = crc;
	}

	private void Initialize()
	{
		m_Keys = new uint[3] { 305419896u, 591751049u, 878082192u };
		m_crc32 = new ZipCrc32();
	}

	private void UpdatePassword(string password)
	{
		m_password = password;
		Initialize();
	}

	private void UpdateKeys(byte byteVal)
	{
		m_Keys[0] = m_crc32.ComputeCrc(m_Keys[0], byteVal);
		m_Keys[1] = (m_Keys[1] + (m_Keys[0] & 0xFF)) * 134775813 + 1;
		m_Keys[2] = m_crc32.ComputeCrc(m_Keys[2], m_Keys[1] >> 24);
	}

	internal void InitiateCipher(string passphrase)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(passphrase);
		for (int i = 0; i < passphrase.Length; i++)
		{
			UpdateKeys(bytes[i]);
		}
	}

	internal void Write(string password)
	{
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		InitiateCipher(password);
	}

	internal byte[] EncryptMessage(byte[] plainData)
	{
		long num = plainData.Length;
		if (plainData == null)
		{
			throw new ArgumentNullException("plainData");
		}
		byte[] array = new byte[num];
		for (int i = 0; i < num; i++)
		{
			byte byteVal = plainData[i];
			array[i] = (byte)(plainData[i] ^ GetCryptoByte());
			UpdateKeys(byteVal);
		}
		return array;
	}

	internal byte[] DecryptMessage(byte[] cipherData)
	{
		long num = cipherData.Length;
		if (cipherData == null)
		{
			throw new ArgumentNullException("cipherData");
		}
		byte[] array = new byte[num];
		for (int i = 0; i < num; i++)
		{
			byte b = (byte)(cipherData[i] ^ GetCryptoByte());
			UpdateKeys(b);
			array[i] = b;
		}
		return array;
	}

	internal byte[] Decrypt(byte[] data)
	{
		int num = 12;
		byte[] array = new byte[num];
		Buffer.BlockCopy(data, 0, array, 0, num);
		byte[] array2 = new byte[data.Length - num];
		Buffer.BlockCopy(data, num, array2, 0, array2.Length);
		InitiateCipher(m_password);
		if (DecryptMessage(array)[11] != ((m_iCrc >> 24) & 0xFF))
		{
			throw new Exception("Password is Incorrect");
		}
		return DecryptMessage(array2);
	}

	internal byte[] Encrypt(byte[] data)
	{
		Write(m_password);
		byte[] array = ZipArchiveItem.CreateRandom(12);
		m_dataStream.Position = 0L;
		array[11] = (byte)((m_iCrc >> 24) & 0xFFu);
		byte[] array2 = EncryptMessage(array);
		byte[] array3 = EncryptMessage(data);
		byte[] array4 = new byte[data.Length + array.Length];
		Buffer.BlockCopy(array2, 0, array4, 0, array2.Length);
		Buffer.BlockCopy(array3, 0, array4, array2.Length, array3.Length);
		return array4;
	}

	private byte GetCryptoByte()
	{
		ushort num = (ushort)((m_Keys[2] & 0xFFFFu) | 2u);
		return (byte)(num * (num ^ 1) >> 8);
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using DocGen.CompoundFile.DocIO;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class AgileDecryptor
{
	private const int SegmentSize = 4096;

	private DataSpaceMap m_dataSpaceMap;

	private AgileEncryptionInfo m_info;

	private ICompoundStorage m_storage;

	private byte[] m_intermediateKey;

	private SecurityHelper m_securityHelper = new SecurityHelper();

	private HashAlgorithm m_hashAlgorithm = new SHA1Managed();

	private HMAC m_hmacSha = new HMACSHA1();

	internal Stream Decrypt()
	{
		if (m_intermediateKey == null)
		{
			throw new InvalidOperationException("Incorrect password.");
		}
		MemoryStream memoryStream = new MemoryStream();
		KeyData keyData = m_info.XmlEncryptionDescriptor.KeyData;
		using CompoundStream compoundStream = m_storage.OpenStream("EncryptedPackage");
		byte[] array = new byte[8];
		compoundStream.Read(array, 0, 8);
		int num = BitConverter.ToInt32(array, 0);
		int num2 = num % keyData.BlockSize;
		int num3 = ((num2 > 0) ? (num + keyData.BlockSize - num2) : num);
		byte[] array2 = new byte[num3];
		compoundStream.Read(array2, 0, num3);
		byte[] encryptedPackage = m_securityHelper.CombineArray(array, array2);
		if (!CheckEncryptedPackage(encryptedPackage))
		{
			throw new Exception("Encrypted package is invalid");
		}
		byte[] array3 = new byte[num];
		int num4 = ((num3 % 4096 == 0) ? (num3 / 4096) : (num3 / 4096 + 1));
		for (int i = 0; i < num4; i++)
		{
			int num5 = Math.Min(4096, num3 - i * 4096);
			byte[] array4 = new byte[num5];
			_ = new byte[num5];
			Buffer.BlockCopy(array2, i * 4096, array4, 0, num5);
			byte[] iV = m_hashAlgorithm.ComputeHash(m_securityHelper.CombineArray(keyData.Salt, BitConverter.GetBytes(i)));
			byte[] src = Decrypt(array4, keyData.BlockSize, m_intermediateKey, iV, num5);
			num5 = ((i == num4 - 1) ? (num5 - (num3 - num)) : num5);
			Buffer.BlockCopy(src, 0, array3, i * 4096, num5);
		}
		memoryStream.Write(array3, 0, num);
		memoryStream.Position = 0L;
		return memoryStream;
	}

	internal void Initialize(ICompoundStorage storage)
	{
		if (storage == null)
		{
			throw new ArgumentNullException("storage");
		}
		m_storage = storage;
		using (Stream stream = storage.OpenStream("EncryptionInfo"))
		{
			m_info = new AgileEncryptionInfo(stream);
			if (m_info.XmlEncryptionDescriptor.KeyEncryptors.EncryptedKey.HashAlgorithm == "SHA512")
			{
				m_hashAlgorithm = new SHA512Managed();
				m_hmacSha = new HMACSHA512();
			}
		}
		using ICompoundStorage dataSpaces = storage.OpenStorage("\u0006DataSpaces");
		ParseDataSpaceMap(dataSpaces);
		ParseTransform(dataSpaces);
	}

	internal bool CheckPassword(string password)
	{
		KeyData keyData = m_info.XmlEncryptionDescriptor.KeyData;
		EncryptedKey encryptedKey = m_info.XmlEncryptionDescriptor.KeyEncryptors.EncryptedKey;
		byte[] blockKey = new byte[8] { 254, 167, 210, 118, 59, 75, 158, 121 };
		byte[] arrKey = m_securityHelper.CreateAgileEncryptionKey(m_hashAlgorithm, password, encryptedKey.Salt, blockKey, encryptedKey.KeyBits >> 3, encryptedKey.SpinCount);
		byte[] buffer = Decrypt(encryptedKey.EncryptedVerifierHashInput, encryptedKey.BlockSize, arrKey, encryptedKey.Salt, encryptedKey.SaltSize);
		byte[] blockKey2 = new byte[8] { 215, 170, 15, 109, 48, 97, 52, 78 };
		byte[] arrKey2 = m_securityHelper.CreateAgileEncryptionKey(m_hashAlgorithm, password, encryptedKey.Salt, blockKey2, encryptedKey.KeyBits >> 3, encryptedKey.SpinCount);
		byte[] buffer2 = Decrypt(encryptedKey.EncryptedVerifierHashValue, encryptedKey.BlockSize, arrKey2, encryptedKey.Salt, encryptedKey.HashSize);
		byte[] buffer3 = m_hashAlgorithm.ComputeHash(buffer);
		bool result = m_securityHelper.CompareArray(buffer2, buffer3);
		byte[] blockKey3 = new byte[8] { 20, 110, 11, 231, 171, 172, 208, 214 };
		byte[] arrKey3 = m_securityHelper.CreateAgileEncryptionKey(m_hashAlgorithm, password, encryptedKey.Salt, blockKey3, encryptedKey.KeyBits >> 3, encryptedKey.SpinCount);
		m_intermediateKey = Decrypt(encryptedKey.EncryptedKeyValue, encryptedKey.BlockSize, arrKey3, encryptedKey.Salt, keyData.KeyBits / 8);
		return result;
	}

	private bool CheckEncryptedPackage(byte[] encryptedPackage)
	{
		KeyData keyData = m_info.XmlEncryptionDescriptor.KeyData;
		DataIntegrity dataIntegrity = m_info.XmlEncryptionDescriptor.DataIntegrity;
		byte[] buffer = new byte[8] { 95, 178, 173, 1, 12, 185, 225, 246 };
		byte[] data = m_hashAlgorithm.ComputeHash(m_securityHelper.CombineArray(keyData.Salt, buffer));
		data = m_securityHelper.CorrectSize(data, keyData.BlockSize, 0);
		byte[] data2 = Decrypt(dataIntegrity.EncryptedHmacKey, keyData.BlockSize, m_intermediateKey, data, keyData.HashSize);
		m_hmacSha.Key = m_securityHelper.CorrectSize(data2, keyData.HashSize, 0);
		byte[] buffer2 = m_hmacSha.ComputeHash(encryptedPackage);
		byte[] buffer3 = new byte[8] { 160, 103, 127, 2, 178, 44, 132, 51 };
		byte[] data3 = m_hashAlgorithm.ComputeHash(m_securityHelper.CombineArray(keyData.Salt, buffer3));
		data3 = m_securityHelper.CorrectSize(data3, keyData.BlockSize, 0);
		byte[] buffer4 = Decrypt(dataIntegrity.EncryptedHmacValue, keyData.BlockSize, m_intermediateKey, data3, keyData.HashSize);
		return m_securityHelper.CompareArray(buffer2, buffer4);
	}

	private byte[] Decrypt(byte[] data, int blockSize, byte[] arrKey, byte[] IV, int actualLength)
	{
		int num = data.Length;
		byte[] array = new byte[num];
		byte[] array2 = new byte[blockSize];
		byte[] array3 = new byte[blockSize];
		byte[] array4 = new byte[blockSize];
		Aes.KeySize keySize = Aes.KeySize.Bits128;
		if (arrKey.Length == 32)
		{
			keySize = Aes.KeySize.Bits256;
		}
		Aes aes = new Aes(keySize, arrKey);
		int i = 0;
		byte[] src;
		if (num % blockSize != 0)
		{
			num = (num / blockSize + 1) * blockSize;
			src = m_securityHelper.CorrectSize(data, num, 0);
		}
		else
		{
			src = data;
		}
		for (; i < num; i += blockSize)
		{
			if (i == 0)
			{
				Buffer.BlockCopy(IV, 0, array4, 0, blockSize);
			}
			else
			{
				Buffer.BlockCopy(array2, 0, array4, 0, blockSize);
			}
			Buffer.BlockCopy(src, i, array2, 0, blockSize);
			aes.InvCipher(array2, array3);
			array3 = m_securityHelper.ConcatenateIV(array3, array4);
			Buffer.BlockCopy(array3, 0, array, i, blockSize);
		}
		if (array.Length > actualLength)
		{
			array2 = new byte[actualLength];
			Buffer.BlockCopy(array, 0, array2, 0, actualLength);
			array = array2;
		}
		return array;
	}

	private void ParseTransform(ICompoundStorage dataSpaces)
	{
		List<DataSpaceMapEntry> mapEntries = m_dataSpaceMap.MapEntries;
		if (mapEntries.Count != 1)
		{
			throw new Exception("Invalid data");
		}
		string dataSpaceName = mapEntries[0].DataSpaceName;
		string storageName = null;
		using (ICompoundStorage compoundStorage = dataSpaces.OpenStorage("DataSpaceInfo"))
		{
			using Stream stream = compoundStorage.OpenStream(dataSpaceName);
			List<string> transformRefs = new DataSpaceDefinition(stream).TransformRefs;
			if (transformRefs.Count != 1)
			{
				throw new Exception("Invalid data");
			}
			storageName = transformRefs[0];
		}
		using ICompoundStorage compoundStorage2 = dataSpaces.OpenStorage("TransformInfo");
		using ICompoundStorage transformStorage = compoundStorage2.OpenStorage(storageName);
		ParseTransformInfo(transformStorage);
	}

	private void ParseDataSpaceMap(ICompoundStorage dataSpaces)
	{
		if (dataSpaces == null)
		{
			throw new ArgumentNullException("dataSpaces");
		}
		using CompoundStream stream = dataSpaces.OpenStream("DataSpaceMap");
		m_dataSpaceMap = new DataSpaceMap(stream);
	}

	private void ParseTransformInfo(ICompoundStorage transformStorage)
	{
		using Stream stream = transformStorage.OpenStream("\u0006Primary");
		new TransformInfoHeader(stream);
		new EncryptionTransformInfo(stream);
	}
}

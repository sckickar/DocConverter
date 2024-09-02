using System;
using System.IO;
using System.Security.Cryptography;
using DocGen.CompoundFile.DocIO;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class AgileEncryptor
{
	private const int DefaultVersion = 262148;

	private const int Reserved = 64;

	private const int SegmentSize = 4096;

	private SecurityHelper m_securityHelper = new SecurityHelper();

	private HashAlgorithm m_hashAlgorithm = new SHA1Managed();

	private HMAC m_hmacSha = new HMACSHA1();

	private string m_hashAlgorithmName = "SHA1";

	private int m_keyBits = 128;

	private int m_hashSize = 20;

	internal AgileEncryptor()
	{
	}

	internal AgileEncryptor(string hashAlgorithm, int keyBits, int hashSize)
	{
		m_hashAlgorithmName = hashAlgorithm;
		m_keyBits = keyBits;
		m_hashSize = hashSize;
		if (m_hashAlgorithmName == "SHA512")
		{
			m_hashAlgorithm = new SHA512Managed();
			m_hmacSha = new HMACSHA512();
		}
	}

	internal void Encrypt(Stream data, string password, ICompoundStorage root)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (password == null || password.Length == 0)
		{
			throw new ArgumentOutOfRangeException("password");
		}
		PrepareEncryptionInfo(data, root, password);
		PrepareDataSpaces(root);
	}

	private void PrepareEncryptionInfo(Stream data, ICompoundStorage root, string password)
	{
		using CompoundStream stream = root.CreateStream("EncryptionInfo");
		AgileEncryptionInfo agileEncryptionInfo = new AgileEncryptionInfo();
		agileEncryptionInfo.VersionInfo = 262148;
		agileEncryptionInfo.Reserved = 64;
		KeyData keyData = agileEncryptionInfo.XmlEncryptionDescriptor.KeyData;
		InitializeKeyData(keyData);
		keyData.Salt = CreateSalt(keyData.SaltSize);
		EncryptedKey encryptedKey = agileEncryptionInfo.XmlEncryptionDescriptor.KeyEncryptors.EncryptedKey;
		InitializeEncryptedKey(encryptedKey);
		encryptedKey.Salt = CreateSalt(encryptedKey.SaltSize);
		byte[] array = CreateSalt(encryptedKey.SaltSize);
		byte[] blockKey = new byte[8] { 254, 167, 210, 118, 59, 75, 158, 121 };
		byte[] key = m_securityHelper.CreateAgileEncryptionKey(m_hashAlgorithm, password, encryptedKey.Salt, blockKey, encryptedKey.KeyBits >> 3, encryptedKey.SpinCount);
		encryptedKey.EncryptedVerifierHashInput = Encrypt(array, encryptedKey.BlockSize, key, encryptedKey.Salt);
		byte[] blockKey2 = new byte[8] { 215, 170, 15, 109, 48, 97, 52, 78 };
		key = m_securityHelper.CreateAgileEncryptionKey(m_hashAlgorithm, password, encryptedKey.Salt, blockKey2, encryptedKey.KeyBits >> 3, encryptedKey.SpinCount);
		encryptedKey.EncryptedVerifierHashValue = Encrypt(m_hashAlgorithm.ComputeHash(array), encryptedKey.BlockSize, key, encryptedKey.Salt);
		byte[] array2 = CreateSalt(keyData.KeyBits / 8);
		byte[] blockKey3 = new byte[8] { 20, 110, 11, 231, 171, 172, 208, 214 };
		key = m_securityHelper.CreateAgileEncryptionKey(m_hashAlgorithm, password, encryptedKey.Salt, blockKey3, encryptedKey.KeyBits >> 3, encryptedKey.SpinCount);
		encryptedKey.EncryptedKeyValue = Encrypt(array2, encryptedKey.BlockSize, key, encryptedKey.Salt);
		DataIntegrity dataIntegrity = agileEncryptionInfo.XmlEncryptionDescriptor.DataIntegrity;
		byte[] buffer = new byte[8] { 95, 178, 173, 1, 12, 185, 225, 246 };
		byte[] data2 = m_hashAlgorithm.ComputeHash(m_securityHelper.CombineArray(keyData.Salt, buffer));
		data2 = m_securityHelper.CorrectSize(data2, keyData.BlockSize, 0);
		byte[] data3 = CreateSalt(keyData.HashSize);
		dataIntegrity.EncryptedHmacKey = Encrypt(data3, keyData.BlockSize, array2, data2);
		byte[] buffer2 = PrepareEncryptedPackage(data, root, keyData, array2);
		m_hmacSha.Key = m_securityHelper.CorrectSize(data3, keyData.HashSize, 0);
		byte[] buffer3 = new byte[8] { 160, 103, 127, 2, 178, 44, 132, 51 };
		byte[] data4 = m_hashAlgorithm.ComputeHash(m_securityHelper.CombineArray(keyData.Salt, buffer3));
		data4 = m_securityHelper.CorrectSize(data4, keyData.BlockSize, 0);
		dataIntegrity.EncryptedHmacValue = Encrypt(m_hmacSha.ComputeHash(buffer2), keyData.BlockSize, array2, data4);
		agileEncryptionInfo.Serialize(stream);
	}

	private byte[] PrepareEncryptedPackage(Stream data, ICompoundStorage root, KeyData keyData, byte[] intermediateKey)
	{
		byte[] array = BitConverter.GetBytes(data.Length);
		using CompoundStream compoundStream = root.CreateStream("EncryptedPackage");
		int num = (int)(data.Length / 4096);
		if (data.Length % 4096 != 0L)
		{
			num++;
		}
		for (int i = 0; i < num; i++)
		{
			int num2 = Math.Min(4096, (int)(data.Length - i * 4096));
			byte[] array2 = new byte[num2];
			byte[] array3 = new byte[num2];
			data.Read(array2, 0, num2);
			byte[] iV = m_hashAlgorithm.ComputeHash(m_securityHelper.CombineArray(keyData.Salt, BitConverter.GetBytes(i)));
			array3 = Encrypt(array2, keyData.BlockSize, intermediateKey, iV);
			array = m_securityHelper.CombineArray(array, array3);
		}
		compoundStream.Write(array, 0, array.Length);
		return array;
	}

	private void InitializeKeyData(KeyData keyData)
	{
		keyData.SaltSize = 16;
		keyData.BlockSize = 16;
		keyData.KeyBits = m_keyBits;
		keyData.HashSize = m_hashSize;
		keyData.CipherAlgorithm = "AES";
		keyData.CipherChaining = "ChainingModeCBC";
		keyData.HashAlgorithm = m_hashAlgorithmName;
	}

	private void InitializeEncryptedKey(EncryptedKey key)
	{
		key.SpinCount = 100000;
		key.SaltSize = 16;
		key.BlockSize = 16;
		key.KeyBits = m_keyBits;
		key.HashSize = m_hashSize;
		key.CipherAlgorithm = "AES";
		key.CipherChaining = "ChainingModeCBC";
		key.HashAlgorithm = m_hashAlgorithmName;
	}

	private void PrepareDataSpaces(ICompoundStorage root)
	{
		if (root == null)
		{
			throw new ArgumentNullException("root");
		}
		using ICompoundStorage dataSpaces = root.CreateStorage("\u0006DataSpaces");
		SerializeDataSpaceInfo(dataSpaces);
		SerializeTransformInfo(dataSpaces);
		SerializeVersion(dataSpaces);
		SerializeDataSpaceMap(dataSpaces);
	}

	private void SerializeVersion(ICompoundStorage dataSpaces)
	{
		if (dataSpaces == null)
		{
			throw new ArgumentNullException("dataSpaces");
		}
		using CompoundStream stream = dataSpaces.CreateStream("Version");
		new VersionInfo().Serialize(stream);
	}

	private void SerializeTransformInfo(ICompoundStorage dataSpaces)
	{
		using ICompoundStorage compoundStorage = dataSpaces.CreateStorage("TransformInfo");
		using ICompoundStorage compoundStorage2 = compoundStorage.CreateStorage("StrongEncryptionTransform");
		using CompoundStream stream = compoundStorage2.CreateStream("\u0006Primary");
		TransformInfoHeader transformInfoHeader = new TransformInfoHeader();
		transformInfoHeader.TransformType = 1;
		transformInfoHeader.TransformId = "{FF9A3F03-56EF-4613-BDD5-5A41C1D07246}";
		transformInfoHeader.TransformName = "Microsoft.Container.EncryptionTransform";
		transformInfoHeader.ReaderVersion = 1;
		transformInfoHeader.UpdaterVersion = 1;
		transformInfoHeader.WriterVersion = 1;
		transformInfoHeader.Serialize(stream);
		EncryptionTransformInfo encryptionTransformInfo = new EncryptionTransformInfo();
		encryptionTransformInfo.Name = string.Empty;
		encryptionTransformInfo.Serialize(stream);
	}

	private void SerializeDataSpaceInfo(ICompoundStorage dataSpaces)
	{
		using ICompoundStorage compoundStorage = dataSpaces.CreateStorage("DataSpaceInfo");
		using CompoundStream stream = compoundStorage.CreateStream("StrongEncryptionDataSpace");
		DataSpaceDefinition dataSpaceDefinition = new DataSpaceDefinition();
		dataSpaceDefinition.TransformRefs.Add("StrongEncryptionTransform");
		dataSpaceDefinition.Serialize(stream);
	}

	private void SerializeDataSpaceMap(ICompoundStorage dataSpaces)
	{
		if (dataSpaces == null)
		{
			throw new ArgumentNullException("dataSpaces");
		}
		DataSpaceMap dataSpaceMap = new DataSpaceMap();
		DataSpaceMapEntry dataSpaceMapEntry = new DataSpaceMapEntry();
		DataSpaceReferenceComponent item = new DataSpaceReferenceComponent(0, "EncryptedPackage");
		dataSpaceMap.MapEntries.Add(dataSpaceMapEntry);
		dataSpaceMapEntry.Components.Add(item);
		dataSpaceMapEntry.DataSpaceName = "StrongEncryptionDataSpace";
		using CompoundStream stream = dataSpaces.CreateStream("DataSpaceMap");
		dataSpaceMap.Serialize(stream);
	}

	private byte[] CreateSalt(int length)
	{
		if (length <= 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		byte[] array = new byte[length];
		Random random = new Random((int)DateTime.Now.Ticks);
		int maxValue = 256;
		for (int i = 0; i < length; i++)
		{
			array[i] = (byte)random.Next(maxValue);
		}
		return array;
	}

	private byte[] Encrypt(byte[] data, int blockSize, byte[] key, byte[] IV)
	{
		int num = data.Length;
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
		byte[] array = new byte[num];
		byte[] array2 = new byte[blockSize];
		byte[] array3 = new byte[blockSize];
		Aes.KeySize keySize = Aes.KeySize.Bits128;
		if (key.Length == 32)
		{
			keySize = Aes.KeySize.Bits256;
		}
		Aes aes = new Aes(keySize, key);
		for (int i = 0; i < num; i += blockSize)
		{
			Buffer.BlockCopy(src, i, array2, 0, blockSize);
			array2 = ((i != 0) ? m_securityHelper.ConcatenateIV(array2, array3) : m_securityHelper.ConcatenateIV(array2, IV));
			aes.Cipher(array2, array3);
			Buffer.BlockCopy(array3, 0, array, i, blockSize);
		}
		return array;
	}
}

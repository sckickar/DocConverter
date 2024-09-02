using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using DocGen.CompoundFile.DocIO;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class StandardDecryptor
{
	private int BlockSize = 16;

	private DataSpaceMap m_dataSpaceMap;

	private StandardEncryptionInfo m_info;

	private ICompoundStorage m_storage;

	private byte[] m_arrKey;

	private SecurityHelper m_securityHelper = new SecurityHelper();

	internal Stream Decrypt()
	{
		if (m_arrKey == null)
		{
			throw new InvalidOperationException("Incorrect password.");
		}
		MemoryStream memoryStream = new MemoryStream();
		using CompoundStream compoundStream = m_storage.OpenStream("EncryptedPackage");
		byte[] array = new byte[8];
		compoundStream.Read(array, 0, 8);
		int num = BitConverter.ToInt32(array, 0);
		int num2 = num % BlockSize;
		int num3 = ((num2 > 0) ? (num + BlockSize - num2) : num);
		byte[] array2 = new byte[num3];
		compoundStream.Read(array2, 0, num3);
		byte[] buffer = Decrypt(array2, m_arrKey);
		memoryStream.Write(buffer, 0, num);
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
			m_info = new StandardEncryptionInfo(stream);
		}
		using ICompoundStorage dataSpaces = storage.OpenStorage("\u0006DataSpaces");
		ParseDataSpaceMap(dataSpaces);
		ParseTransfrom(dataSpaces);
	}

	internal bool CheckPassword(string password)
	{
		EncryptionVerifier verifier = m_info.Verifier;
		m_arrKey = m_securityHelper.CreateKey(password, verifier.Salt, 16);
		byte[] buffer = Decrypt(verifier.EncryptedVerifier, m_arrKey);
		byte[] buffer2 = new SHA1Managed().ComputeHash(buffer);
		byte[] src = Decrypt(verifier.EncryptedVerifierHash, m_arrKey);
		byte[] array = new byte[verifier.VerifierHashSize];
		Buffer.BlockCopy(src, 0, array, 0, array.Length);
		return m_securityHelper.CompareArray(array, buffer2);
	}

	private byte[] Decrypt(byte[] data, byte[] key)
	{
		Aes @object = new Aes(Aes.KeySize.Bits128, key);
		return m_securityHelper.EncryptDecrypt(data, @object.InvCipher, key.Length);
	}

	private void ParseTransfrom(ICompoundStorage dataSpaces)
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

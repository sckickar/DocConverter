using System;
using System.IO;
using System.Security.Cryptography;
using DocGen.CompoundFile.DocIO;

namespace DocGen.DocIO.ReaderWriter.Security;

[CLSCompliant(false)]
internal class StandardEncryptor
{
	private const int KeyLength = 16;

	private const int DefaultVersion = 131075;

	private const int DefaultFlags = 36;

	private const int AES128AlgorithmId = 26126;

	private const int SHA1AlgorithmHash = 32772;

	private const int DefaultProviderType = 24;

	private const string DefaultCSPName = "Microsoft Enhanced RSA and AES Cryptographic Provider (Prototype)";

	private SecurityHelper m_securityHelper = new SecurityHelper();

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
		byte[] key = PrepareEncryptionInfo(root, password);
		PrepareDataSpaces(root);
		using CompoundStream compoundStream = root.CreateStream("EncryptedPackage");
		byte[] bytes = BitConverter.GetBytes(data.Length);
		compoundStream.Write(bytes, 0, 8);
		Encrypt(data, key, compoundStream);
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

	private byte[] PrepareEncryptionInfo(ICompoundStorage root, string password)
	{
		byte[] salt = CreateSalt(16);
		byte[] array = m_securityHelper.CreateKey(password, salt, 16);
		byte[] array2 = CreateSalt(16);
		SHA1 sHA = new SHA1Managed();
		using CompoundStream stream = root.CreateStream("EncryptionInfo");
		StandardEncryptionInfo obj = new StandardEncryptionInfo
		{
			VersionInfo = 131075,
			Flags = 36
		};
		EncryptionHeader header = obj.Header;
		header.Flags = 36;
		header.AlgorithmId = 26126;
		header.AlgorithmIdHash = 32772;
		header.KeySize = 128;
		header.ProviderType = 24;
		header.Reserved1 = 0;
		header.Reserved2 = 0;
		header.CSPName = "Microsoft Enhanced RSA and AES Cryptographic Provider (Prototype)";
		EncryptionVerifier verifier = obj.Verifier;
		verifier.Salt = salt;
		verifier.EncryptedVerifier = Encrypt(array2, array);
		byte[] array3 = sHA.ComputeHash(array2);
		int num = array3.Length % 16;
		verifier.VerifierHashSize = array3.Length;
		if (num != 0)
		{
			array3 = m_securityHelper.CombineArray(array3, new byte[16 - num]);
		}
		verifier.EncryptedVerifierHash = Encrypt(array3, array);
		obj.Serialize(stream);
		return array;
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

	private byte[] Encrypt(byte[] data, byte[] key)
	{
		Aes @object = new Aes(Aes.KeySize.Bits128, key);
		return m_securityHelper.EncryptDecrypt(data, @object.Cipher, key.Length);
	}

	private void Encrypt(Stream stream, byte[] key, Stream output)
	{
		Aes aes = new Aes(Aes.KeySize.Bits128, key);
		byte[] array = new byte[16];
		byte[] array2 = new byte[16];
		while (stream.Read(array, 0, 16) > 0)
		{
			aes.Cipher(array, array2);
			output.Write(array2, 0, 16);
		}
	}
}

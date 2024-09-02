using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Security;

internal class PdfEncryptor
{
	private const int c_newKeyOffset = 5;

	private const int c_key40 = 5;

	private const int c_key128 = 16;

	private const int c_key256 = 32;

	private const int c_40RevisionNumber = 2;

	private const int c_128RevisionNumber = 3;

	private const int c_bytesAmount = 256;

	private const int c_randomBytesAmount = 16;

	private const int c_stringLength = 32;

	private const int c_ownerLoopNum = 50;

	private const int c_ownerLoopNum2 = 20;

	private const byte c_flagNum = byte.MaxValue;

	internal const byte c_numBits = 8;

	private const int c_permissionSet = -3904;

	private const int c_permissionCleared = -4;

	private const int c_permissionRevisionTwoMask = 4095;

	private bool m_hasComputedPasswordValues;

	private IMessageDigest MD5;

	private byte[] m_customArray;

	private byte[] m_randomBytes;

	private string m_ownerPassword = string.Empty;

	private string m_userPassword = string.Empty;

	private byte[] m_ownerPasswordOut;

	private byte[] m_userPasswordOut;

	private byte[] m_encryptionKey;

	private PdfEncryptionKeySize m_keyLength = PdfEncryptionKeySize.Key128Bit;

	private PdfPermissionsFlags m_permission;

	private int m_revision;

	private bool m_bChanged;

	private static byte[] s_paddingString;

	private static object s_lockObject = new object();

	private bool m_encrypt;

	private int m_permissionValue;

	private static readonly byte[] salt = new byte[4] { 115, 65, 108, 84 };

	private PdfEncryptionAlgorithm m_encryptionAlgorithm = PdfEncryptionAlgorithm.RC4;

	private byte[] m_userEncryptionKeyOut;

	private byte[] m_ownerEncryptionKeyOut;

	private byte[] m_permissionFlag;

	private byte[] m_fileEncryptionKey;

	private byte[] m_userRandomBytes;

	private byte[] m_ownerRandomBytes;

	private SecureRandomAlgorithm m_randomArray = new SecureRandomAlgorithm();

	private bool m_encryptMetadata = true;

	private int m_revisionNumberOut;

	private int m_versionNumberOut;

	private int keyLength;

	private string[] HashAlgorithms = new string[3] { "SHA-256", "SHA-384", "SHA-512" };

	private byte[] m_documentID;

	private bool m_encryptOnlyAttachment;

	internal PdfArray FileID
	{
		get
		{
			PdfString element = new PdfString(RandomBytes);
			return new PdfArray { element, element };
		}
	}

	public string Filter => SecurityHandlers.Standard.ToString();

	public PdfEncryptionKeySize CryptographicAlgorithm
	{
		get
		{
			return m_keyLength;
		}
		set
		{
			if (m_keyLength != value)
			{
				m_keyLength = value;
				m_bChanged = true;
				m_hasComputedPasswordValues = false;
			}
		}
	}

	public PdfEncryptionAlgorithm EncryptionAlgorithm
	{
		get
		{
			return m_encryptionAlgorithm;
		}
		set
		{
			m_encryptionAlgorithm = value;
		}
	}

	internal PdfPermissionsFlags Permissions
	{
		get
		{
			return m_permission;
		}
		set
		{
			if (m_permission != value)
			{
				m_bChanged = true;
				m_permission = value;
			}
			m_permissionValue = (int)((m_permission | (PdfPermissionsFlags)(-3904)) & (PdfPermissionsFlags)(-4));
			if (RevisionNumber > 2)
			{
				m_permissionValue &= 4095;
			}
			m_hasComputedPasswordValues = false;
		}
	}

	public int RevisionNumber
	{
		get
		{
			if (m_revision == 0)
			{
				if (CryptographicAlgorithm != PdfEncryptionKeySize.Key40Bit)
				{
					return 3;
				}
				if (m_revisionNumberOut <= 2)
				{
					return 2;
				}
				return m_revisionNumberOut;
			}
			return m_revision;
		}
	}

	internal string OwnerPassword
	{
		get
		{
			if (m_encryptOnlyAttachment)
			{
				return string.Empty;
			}
			return m_ownerPassword;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("OwnerPassword");
			}
			if (m_ownerPassword != value)
			{
				m_bChanged = true;
				m_ownerPassword = value;
				m_hasComputedPasswordValues = false;
			}
		}
	}

	internal string UserPassword
	{
		get
		{
			return m_userPassword;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("UserPassword");
			}
			if (m_userPassword != value)
			{
				m_bChanged = true;
				m_userPassword = value;
				m_hasComputedPasswordValues = false;
			}
		}
	}

	protected byte[] RandomBytes
	{
		get
		{
			if (m_randomBytes == null)
			{
				m_randomBytes = new byte[16];
				m_randomArray.NextBytes(m_randomBytes);
			}
			return m_randomBytes;
		}
	}

	internal bool EncryptOnlyAttachment
	{
		get
		{
			return m_encryptOnlyAttachment;
		}
		set
		{
			m_encryptOnlyAttachment = value;
			m_hasComputedPasswordValues = false;
		}
	}

	protected byte[] CustomArray
	{
		get
		{
			return m_customArray;
		}
		set
		{
			if (m_customArray != value)
			{
				m_customArray = value;
			}
		}
	}

	protected Encoding SecurityEncoding => Encoding.UTF8;

	protected byte[] EncryptionKey
	{
		get
		{
			return m_encryptionKey;
		}
		set
		{
			if (m_encryptionKey != value)
			{
				m_encryptionKey = value;
			}
		}
	}

	internal bool Encrypt
	{
		get
		{
			bool result = Permissions != 0 || m_userPassword.Length > 0 || m_ownerPassword.Length > 0;
			if (!m_encrypt)
			{
				return false;
			}
			return result;
		}
		set
		{
			m_encrypt = value;
		}
	}

	internal byte[] UserPasswordOut
	{
		get
		{
			InitializeData();
			return m_userPasswordOut;
		}
	}

	internal byte[] OwnerPasswordOut
	{
		get
		{
			InitializeData();
			return m_ownerPasswordOut;
		}
	}

	internal bool Changed => m_bChanged;

	internal bool EncryptMetaData
	{
		get
		{
			return m_encryptMetadata;
		}
		set
		{
			m_hasComputedPasswordValues = false;
			m_encryptMetadata = value;
		}
	}

	protected static byte[] PaddingString
	{
		get
		{
			return s_paddingString;
		}
		set
		{
			lock (s_lockObject)
			{
				if (s_paddingString != value)
				{
					s_paddingString = value;
				}
			}
		}
	}

	internal PdfEncryptor()
	{
		PaddingString = new byte[32]
		{
			40, 191, 78, 94, 78, 117, 138, 65, 100, 0,
			78, 86, 255, 250, 1, 8, 46, 46, 0, 182,
			208, 104, 62, 128, 47, 12, 169, 254, 100, 83,
			105, 122
		};
		CustomArray = new byte[256];
		Encrypt = true;
		Permissions = PdfPermissionsFlags.Default;
		MessageDigestFinder messageDigestFinder = new MessageDigestFinder();
		MD5 = messageDigestFinder.GetDigest("MD5");
	}

	internal PdfEncryptor Clone()
	{
		PdfEncryptor pdfEncryptor = MemberwiseClone() as PdfEncryptor;
		pdfEncryptor.CryptographicAlgorithm = m_keyLength;
		pdfEncryptor.UserPassword = UserPassword;
		pdfEncryptor.OwnerPassword = OwnerPassword;
		pdfEncryptor.Permissions = Permissions;
		pdfEncryptor.m_randomBytes = m_randomBytes.Clone() as byte[];
		pdfEncryptor.m_customArray = m_customArray.Clone() as byte[];
		pdfEncryptor.m_revision = m_revision;
		if (m_encryptionKey != null)
		{
			pdfEncryptor.m_encryptionKey = m_encryptionKey.Clone() as byte[];
		}
		pdfEncryptor.m_customArray = m_customArray.Clone() as byte[];
		pdfEncryptor.m_ownerPasswordOut = m_ownerPasswordOut.Clone() as byte[];
		pdfEncryptor.m_userPasswordOut = m_userPasswordOut.Clone() as byte[];
		pdfEncryptor.m_hasComputedPasswordValues = m_hasComputedPasswordValues;
		pdfEncryptor.m_bChanged = m_bChanged;
		return pdfEncryptor;
	}

	internal void ReadFromDictionary(PdfDictionary dictionary)
	{
		if (dictionary == null)
		{
			throw new ArgumentNullException("dictionary");
		}
		PdfName pdfName = PdfCrossTable.Dereference(dictionary["Filter"]) as PdfName;
		if (pdfName.Value != "Standard")
		{
			throw new PdfDocumentException("Invalid Format: Unsupported security filter: " + pdfName.Value);
		}
		m_permissionValue = dictionary.GetInt("P");
		m_permission = (PdfPermissionsFlags)(m_permissionValue & 0xF3F);
		m_keyLength = (PdfEncryptionKeySize)dictionary.GetInt("V");
		m_revisionNumberOut = dictionary.GetInt("R");
		m_versionNumberOut = dictionary.GetInt("V");
		_ = m_revisionNumberOut;
		m_revision = m_revisionNumberOut;
		if (m_keyLength == PdfEncryptionKeySize.Key256BitRevision6 && m_keyLength != (PdfEncryptionKeySize)dictionary.GetInt("R"))
		{
			throw new PdfDocumentException("Invalid Format: V and R entries of the Encryption dictionary doesn't match.");
		}
		if (m_keyLength == (PdfEncryptionKeySize)5)
		{
			m_userEncryptionKeyOut = dictionary.GetString("UE").Bytes;
			m_ownerEncryptionKeyOut = dictionary.GetString("OE").Bytes;
			m_permissionFlag = dictionary.GetString("Perms").Bytes;
		}
		m_userPasswordOut = dictionary.GetString("U").Bytes;
		m_ownerPasswordOut = dictionary.GetString("O").Bytes;
		if (dictionary.ContainsKey("Length"))
		{
			keyLength = dictionary.GetInt("Length");
		}
		else if (m_keyLength == PdfEncryptionKeySize.Key40Bit)
		{
			keyLength = 40;
		}
		else if (m_keyLength == PdfEncryptionKeySize.Key128Bit)
		{
			keyLength = 128;
		}
		else
		{
			keyLength = 256;
		}
		if (keyLength == 128 && dictionary.GetInt("R") < 4)
		{
			m_keyLength = PdfEncryptionKeySize.Key128Bit;
			m_encryptionAlgorithm = PdfEncryptionAlgorithm.RC4;
		}
		else if ((keyLength == 128 || keyLength == 256) && dictionary.GetInt("R") >= 4)
		{
			if (keyLength == 128)
			{
				m_keyLength = PdfEncryptionKeySize.Key128Bit;
			}
			else
			{
				m_keyLength = PdfEncryptionKeySize.Key256Bit;
			}
			PdfDictionary obj = (dictionary["CF"] as PdfDictionary)["StdCF"] as PdfDictionary;
			PdfName pdfName2 = obj[new PdfName("AuthEvent")] as PdfName;
			if (pdfName2 != null && pdfName2.Value == "EFOpen")
			{
				EncryptOnlyAttachment = true;
			}
			if ((obj[new PdfName("CFM")] as PdfName).Value != "V2")
			{
				m_encryptionAlgorithm = PdfEncryptionAlgorithm.AES;
			}
			else
			{
				m_encryptionAlgorithm = PdfEncryptionAlgorithm.RC4;
			}
		}
		else if (keyLength == 40)
		{
			m_keyLength = PdfEncryptionKeySize.Key40Bit;
		}
		else if (keyLength <= 128 && keyLength > 40 && keyLength % 8 == 0 && dictionary.GetInt("R") < 4)
		{
			m_keyLength = PdfEncryptionKeySize.Key128Bit;
			m_encryptionAlgorithm = PdfEncryptionAlgorithm.RC4;
		}
		else
		{
			m_keyLength = PdfEncryptionKeySize.Key256Bit;
			m_encryptionAlgorithm = PdfEncryptionAlgorithm.AES;
		}
		if (m_revisionNumberOut == 6)
		{
			m_keyLength = PdfEncryptionKeySize.Key256BitRevision6;
			m_encryptionAlgorithm = PdfEncryptionAlgorithm.AES;
		}
		if (keyLength != 0 && keyLength % 8 != 0 && (m_keyLength == PdfEncryptionKeySize.Key40Bit || m_keyLength == PdfEncryptionKeySize.Key128Bit || m_keyLength == PdfEncryptionKeySize.Key256Bit))
		{
			throw new PdfDocumentException("Invalid format: Invalid/Unsupported security dictionary.");
		}
		m_hasComputedPasswordValues = true;
	}

	internal bool CheckPassword(string password, PdfString key, bool attachEncryption)
	{
		if (password == null)
		{
			throw new ArgumentNullException("password");
		}
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		bool flag = false;
		byte[] randomBytes = m_randomBytes;
		m_randomBytes = key.Bytes.Clone() as byte[];
		if (AuthenticateOwnerPassword(password))
		{
			m_ownerPassword = password;
			flag = true;
		}
		else if (AuthenticateUserPassword(password))
		{
			m_userPassword = password;
			flag = true;
		}
		else if (!attachEncryption)
		{
			flag = true;
		}
		else
		{
			m_encryptionKey = null;
			flag = false;
		}
		if (!flag)
		{
			m_randomBytes = randomBytes;
		}
		return flag;
	}

	internal byte[] EncryptData(long currObjNumber, byte[] data, bool isEncryption)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (CryptographicAlgorithm == PdfEncryptionKeySize.Key256Bit || CryptographicAlgorithm == PdfEncryptionKeySize.Key256BitRevision6)
		{
			if (isEncryption)
			{
				return EncryptData256(data);
			}
			return DecryptData256(data);
		}
		InitializeData();
		int num = 0;
		int num2 = 0;
		byte[] array = null;
		if (EncryptionKey.Length == 5)
		{
			array = new byte[EncryptionKey.Length + 5];
			int i = 0;
			for (int num3 = EncryptionKey.Length; i < num3; i++)
			{
				array[i] = EncryptionKey[i];
			}
			int num4 = EncryptionKey.Length - 1;
			array[++num4] = (byte)currObjNumber;
			array[++num4] = (byte)(currObjNumber >> 8);
			array[++num4] = (byte)(currObjNumber >> 16);
			array[++num4] = (byte)num;
			array[++num4] = (byte)(num >> 8);
			num2 = array.Length;
			array = PrepareKeyForEncryption(array);
		}
		else
		{
			array = ((EncryptionAlgorithm != PdfEncryptionAlgorithm.AES) ? new byte[EncryptionKey.Length + 5] : new byte[EncryptionKey.Length + 9]);
			Array.Copy(EncryptionKey, array, EncryptionKey.Length);
			int num5 = EncryptionKey.Length - 1;
			array[++num5] = (byte)currObjNumber;
			array[++num5] = (byte)(currObjNumber >> 8);
			array[++num5] = (byte)(currObjNumber >> 16);
			array[++num5] = (byte)num;
			array[++num5] = (byte)(num >> 8);
			if (EncryptionAlgorithm == PdfEncryptionAlgorithm.AES)
			{
				array[++num5] = salt[0];
				array[++num5] = salt[1];
				array[++num5] = salt[2];
				array[++num5] = salt[3];
			}
			MD5.Reset();
			MD5.Update(array, 0, array.Length);
			array = new byte[MD5.MessageDigestSize];
			MD5.DoFinal(array, 0);
			MD5.Reset();
			num2 = array.Length;
		}
		num2 = Math.Min(num2, array.Length);
		if (EncryptionAlgorithm == PdfEncryptionAlgorithm.AES)
		{
			if (isEncryption)
			{
				if (EncryptOnlyAttachment)
				{
					return AESEncrypt(data, EncryptionKey);
				}
				return AESEncrypt(data, array);
			}
			if (EncryptOnlyAttachment)
			{
				return AESDecrypt(data, EncryptionKey);
			}
			return AESDecrypt(data, array);
		}
		return EncryptDataByCustom(data, array, num2);
	}

	internal void SaveToDictionary(PdfDictionary dictionary)
	{
		if (Changed)
		{
			m_revisionNumberOut = 0;
			m_versionNumberOut = 0;
			m_revision = 0;
			keyLength = 0;
		}
		dictionary.SetName("Filter", "Standard");
		dictionary.SetNumber("P", m_permissionValue);
		dictionary.SetProperty("U", new PdfString(UserPasswordOut));
		dictionary.SetProperty("O", new PdfString(OwnerPasswordOut));
		if (!dictionary.ContainsKey("Length"))
		{
			dictionary.SetNumber("Length", GetKeyLength() * 8);
		}
		else
		{
			keyLength = 0;
			dictionary.SetNumber("Length", GetKeyLength() * 8);
		}
		bool flag = false;
		if (dictionary.ContainsKey("CF"))
		{
			if ((((dictionary["CF"] as PdfDictionary)["StdCF"] as PdfDictionary)[new PdfName("CFM")] as PdfName).Value != "V2")
			{
				flag = true;
			}
			if (dictionary.ContainsKey("StmF") && dictionary.ContainsKey("StrF") && m_versionNumberOut == 0 && m_revisionNumberOut == 0)
			{
				m_versionNumberOut = 4;
				m_revisionNumberOut = 4;
			}
		}
		if (m_encryptOnlyAttachment && (EncryptionAlgorithm == PdfEncryptionAlgorithm.RC4 || CryptographicAlgorithm == PdfEncryptionKeySize.Key40Bit))
		{
			throw new PdfException("Encrypt only attachment is supported in AES algorithm with 128, 256 and 256Revision6 encryptions only.");
		}
		if (!EncryptMetaData && CryptographicAlgorithm == PdfEncryptionKeySize.Key40Bit)
		{
			throw new PdfException("EncryptAllContentsExceptMetadata PdfEncryptionOptions does not supprot encrption key size key40");
		}
		if (m_encryptionAlgorithm == PdfEncryptionAlgorithm.AES || CryptographicAlgorithm == PdfEncryptionKeySize.Key256Bit || CryptographicAlgorithm == PdfEncryptionKeySize.Key256BitRevision6 || (CryptographicAlgorithm == PdfEncryptionKeySize.Key128Bit && !EncryptMetaData))
		{
			if (m_revisionNumberOut > 0 && flag)
			{
				dictionary.SetNumber("R", m_revisionNumberOut);
			}
			else
			{
				dictionary.SetNumber("R", (int)(m_keyLength + 2));
			}
			if (m_versionNumberOut > 0 && flag)
			{
				dictionary.SetNumber("V", m_versionNumberOut);
			}
			else
			{
				dictionary.SetNumber("V", (int)(m_keyLength + 2));
			}
			if (CryptographicAlgorithm == PdfEncryptionKeySize.Key256BitRevision6)
			{
				dictionary.SetNumber("V", 5);
				dictionary.SetNumber("R", 6);
			}
			else if (CryptographicAlgorithm == PdfEncryptionKeySize.Key256Bit)
			{
				dictionary.SetNumber("V", 5);
				dictionary.SetNumber("R", 5);
			}
			if (m_encryptOnlyAttachment)
			{
				dictionary.SetName("StmF", "Identity");
				dictionary.SetName("StrF", "Identity");
				dictionary.SetName("EFF", "StdCF");
				dictionary.SetBoolean("EncryptMetadata", m_encryptMetadata);
			}
			else
			{
				dictionary.SetName("StmF", "StdCF");
				dictionary.SetName("StrF", "StdCF");
				if (dictionary.ContainsKey("EFF"))
				{
					dictionary.Remove("EFF");
				}
			}
			if (!m_encryptMetadata)
			{
				if (!dictionary.ContainsKey(new PdfName("EncryptMetadata")))
				{
					dictionary.SetBoolean("EncryptMetadata", m_encryptMetadata);
				}
			}
			else if (!EncryptOnlyAttachment && dictionary.ContainsKey(new PdfName("EncryptMetadata")))
			{
				dictionary.Remove("EncryptMetadata");
			}
			dictionary.SetProperty("CF", new PdfDictionary(AESDictionary()));
			if (CryptographicAlgorithm == PdfEncryptionKeySize.Key256Bit || CryptographicAlgorithm == PdfEncryptionKeySize.Key256BitRevision6)
			{
				dictionary.SetProperty("UE", new PdfString(m_userEncryptionKeyOut));
				dictionary.SetProperty("OE", new PdfString(m_ownerEncryptionKeyOut));
				dictionary.SetProperty("Perms", new PdfString(m_permissionFlag));
			}
		}
		else
		{
			if (m_revisionNumberOut > 0 && !flag)
			{
				dictionary.SetNumber("R", m_revisionNumberOut);
			}
			else
			{
				dictionary.SetNumber("R", (int)(m_keyLength + 1));
			}
			if (m_versionNumberOut > 0 && !flag)
			{
				dictionary.SetNumber("V", m_versionNumberOut);
			}
			else
			{
				dictionary.SetNumber("V", (int)m_keyLength);
			}
		}
		dictionary.Archive = false;
	}

	private byte[] PadTrancateString(string source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		byte[] bytes = SecurityEncoding.GetBytes(source);
		return PadTrancateString(bytes);
	}

	private byte[] PadTrancateString(byte[] sourceBytes)
	{
		if (sourceBytes == null)
		{
			throw new ArgumentNullException("sourceBytes");
		}
		byte[] array = new byte[32];
		int num = sourceBytes.Length;
		if (num > 0)
		{
			Array.Copy(sourceBytes, 0, array, 0, Math.Min(num, 32));
		}
		if (num < 32)
		{
			Array.Copy(PaddingString, 0, array, num, 32 - num);
		}
		return array;
	}

	private byte[] EncryptDataByCustom(byte[] data, byte[] key)
	{
		return EncryptDataByCustom(data, key, key.Length);
	}

	private byte[] EncryptDataByCustom(byte[] data, byte[] key, int keyLen)
	{
		byte[] array = new byte[data.Length];
		RecreateCustomArray(key, keyLen);
		keyLen = data.Length;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < keyLen; i++)
		{
			num = (num + 1) % 256;
			num2 = (num2 + CustomArray[num]) % 256;
			byte b = CustomArray[num];
			CustomArray[num] = CustomArray[num2];
			CustomArray[num2] = b;
			int num3 = (CustomArray[num] + CustomArray[num2]) % 256;
			byte b2 = CustomArray[num3];
			array[i] = (byte)(data[i] ^ b2);
		}
		return array;
	}

	private byte[] AESEncrypt(byte[] data, byte[] key)
	{
		MemoryStream memoryStream = new MemoryStream();
		byte[] array = null;
		byte[] iv = GenerateIV();
		AesEncryptor aesEncryptor = new AesEncryptor(key, iv, isEncryption: true);
		array = new byte[aesEncryptor.GetBlockSize(data.Length)];
		aesEncryptor.ProcessBytes(data, 0, data.Length, array, 0);
		memoryStream.Write(array, 0, array.Length);
		array = new byte[aesEncryptor.CalculateOutputSize()];
		aesEncryptor.Finalize(array);
		memoryStream.Write(array, 0, array.Length);
		memoryStream.Dispose();
		return memoryStream.ToArray();
	}

	private byte[] AESDecrypt(byte[] data, byte[] key)
	{
		MemoryStream memoryStream = new MemoryStream();
		byte[] array = new byte[16];
		int num = data.Length;
		int num2 = 0;
		int num3 = Math.Min(array.Length - num2, num);
		Array.Copy(data, 0, array, num2, num3);
		num -= num3;
		num2 += num3;
		if (num2 == array.Length && num > 0)
		{
			AesEncryptor aesEncryptor = new AesEncryptor(key, array, isEncryption: false);
			byte[] array2 = new byte[aesEncryptor.GetBlockSize(num)];
			aesEncryptor.ProcessBytes(data, num2, num, array2, 0);
			memoryStream.Write(array2, 0, array2.Length);
			array2 = new byte[aesEncryptor.CalculateOutputSize()];
			num = aesEncryptor.Finalize(array2);
			if (array2.Length != num)
			{
				byte[] array3 = new byte[num];
				Array.Copy(array2, 0, array3, 0, num);
				memoryStream.Write(array3, 0, array3.Length);
			}
			else
			{
				memoryStream.Write(array2, 0, array2.Length);
			}
			memoryStream.Dispose();
			return memoryStream.ToArray();
		}
		return data;
	}

	private byte[] EncryptData256(byte[] data)
	{
		return AESEncrypt(data, m_fileEncryptionKey);
	}

	private byte[] DecryptData256(byte[] data)
	{
		return AESDecrypt(data, m_fileEncryptionKey);
	}

	private byte[] GenerateIV()
	{
		byte[] array = new byte[16];
		m_randomArray.NextBytes(array);
		return array;
	}

	private void RecreateCustomArray(byte[] key, int keyLen)
	{
		byte[] array = new byte[256];
		for (int i = 0; i < 256; i++)
		{
			array[i] = key[i % keyLen];
			CustomArray[i] = (byte)i;
		}
		int num = 0;
		for (int j = 0; j < 256; j++)
		{
			num = (num + CustomArray[j] + array[j]) % 256;
			byte b = CustomArray[j];
			CustomArray[j] = CustomArray[num];
			CustomArray[num] = b;
		}
	}

	protected internal int GetKeyLength()
	{
		if (keyLength != 0)
		{
			return keyLength / 8;
		}
		if (CryptographicAlgorithm == PdfEncryptionKeySize.Key40Bit)
		{
			return 5;
		}
		if (CryptographicAlgorithm == PdfEncryptionKeySize.Key128Bit)
		{
			return 16;
		}
		return 32;
	}

	private byte[] CreateOwnerPassword()
	{
		string password = ((OwnerPassword == null || OwnerPassword.Length == 0) ? UserPassword : OwnerPassword);
		byte[] keyFromOwnerPass = GetKeyFromOwnerPass(password);
		byte[] data = PadTrancateString(UserPassword);
		byte[] array = EncryptDataByCustom(data, keyFromOwnerPass, keyFromOwnerPass.Length);
		if (RevisionNumber > 2)
		{
			byte[] array2 = keyFromOwnerPass;
			for (byte b = 1; b < 20; b++)
			{
				array2 = GetKeyForOwnerPassStep7(keyFromOwnerPass, b);
				array = EncryptDataByCustom(array, array2, array2.Length);
			}
		}
		return array;
	}

	private byte[] AcrobatXComputeHash(byte[] input, byte[] password, byte[] Key)
	{
		try
		{
			byte[] array = null;
			SHA256MessageDigest sHA256MessageDigest = new SHA256MessageDigest();
			array = new byte[sHA256MessageDigest.MessageDigestSize];
			sHA256MessageDigest.Update(input, 0, input.Length);
			sHA256MessageDigest.DoFinal(array, 0);
			byte[] array2 = null;
			for (int i = 0; i < 64 || (array2[^1] & 0xFF) > i - 32; i++)
			{
				byte[] array3 = ((Key == null || Key.Length < 48) ? new byte[64 * (password.Length + array.Length)] : new byte[64 * (password.Length + array.Length + 48)]);
				int num = 0;
				try
				{
					for (int j = 0; j < 64; j++)
					{
						Array.Copy(password, 0, array3, num, password.Length);
						num += password.Length;
						Array.Copy(array, 0, array3, num, array.Length);
						num += array.Length;
						if (Key != null && Key.Length >= 48)
						{
							Array.Copy(Key, 0, array3, num, 48);
							num += 48;
						}
					}
				}
				catch (Exception ex)
				{
					throw new Exception(ex.Message);
				}
				byte[] array4 = new byte[16];
				byte[] array5 = new byte[16];
				Array.Copy(array, 0, array4, 0, 16);
				Array.Copy(array, 16, array5, 0, 16);
				_ = new byte[16];
				array2 = new AesCipher(isEncryption: true, array4, array5).Update(array3, 0, array3.Length);
				byte[] array6 = new byte[16];
				Array.Copy(array2, 0, array6, 0, 16);
				SfBigInteger sfBigInteger = new SfBigInteger(array6);
				SfBigInteger sfBigInteger2 = new SfBigInteger(3L);
				SfBigInteger sfBigInteger3 = sfBigInteger % sfBigInteger2;
				string text = HashAlgorithms[Math.Abs(sfBigInteger3.IntValue())];
				if (text == "SHA-256")
				{
					SHA256MessageDigest sHA256MessageDigest2 = new SHA256MessageDigest();
					array = new byte[sHA256MessageDigest2.MessageDigestSize];
					sHA256MessageDigest2.Update(array2, 0, array2.Length);
					sHA256MessageDigest2.DoFinal(array, 0);
				}
				else if (text == "SHA-384")
				{
					SHA384MessageDigest sHA384MessageDigest = new SHA384MessageDigest();
					array = new byte[sHA384MessageDigest.MessageDigestSize];
					sHA384MessageDigest.Update(array2, 0, array2.Length);
					sHA384MessageDigest.DoFinal(array, 0);
				}
				else
				{
					SHA512MessageDigest sHA512MessageDigest = new SHA512MessageDigest();
					array = new byte[sHA512MessageDigest.MessageDigestSize];
					sHA512MessageDigest.Update(array2, 0, array2.Length);
					sHA512MessageDigest.DoFinal(array, 0);
				}
			}
			if (array.Length > 32)
			{
				byte[] array7 = new byte[32];
				Array.Copy(array, 0, array7, 0, 32);
				return array7;
			}
			return array;
		}
		catch (Exception ex2)
		{
			throw new IOException(ex2.Message);
		}
	}

	private byte[] Create256BitOwnerPassword()
	{
		byte[] array = new byte[8];
		byte[] array2 = new byte[8];
		m_ownerRandomBytes = new byte[16];
		m_randomArray.NextBytes(m_ownerRandomBytes);
		if (string.IsNullOrEmpty(OwnerPassword))
		{
			OwnerPassword = UserPassword;
		}
		string s = ((OwnerPassword == null || OwnerPassword.Length == 0) ? UserPassword : OwnerPassword);
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		Array.Copy(m_ownerRandomBytes, 0, array, 0, 8);
		Array.Copy(m_ownerRandomBytes, 8, array2, 0, 8);
		byte[] array3 = new byte[bytes.Length + array.Length + m_userPasswordOut.Length];
		Array.Copy(bytes, 0, array3, 0, bytes.Length);
		Array.Copy(array, 0, array3, bytes.Length, array.Length);
		Array.Copy(m_userPasswordOut, 0, array3, bytes.Length + array.Length, m_userPasswordOut.Length);
		byte[] array4 = new byte[32];
		IMessageDigest digest = new MessageDigestFinder().GetDigest("SHA256");
		digest.Update(array3, 0, array3.Length);
		digest.DoFinal(array4, 0);
		byte[] array5 = new byte[array4.Length + array.Length + array2.Length];
		Array.Copy(array4, 0, array5, 0, array4.Length);
		Array.Copy(array, 0, array5, array4.Length, array.Length);
		Array.Copy(array2, 0, array5, array4.Length + array.Length, array2.Length);
		return array5;
	}

	private void CreateAcrobatX256BitOwnerPassword()
	{
		byte[] array = new byte[8];
		byte[] array2 = new byte[8];
		byte[] array3 = null;
		if (string.IsNullOrEmpty(OwnerPassword))
		{
			OwnerPassword = UserPassword;
		}
		string s = ((OwnerPassword == null || OwnerPassword.Length == 0) ? UserPassword : OwnerPassword);
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		m_randomArray.NextBytes(array);
		m_randomArray.NextBytes(array2);
		byte[] array4 = new byte[bytes.Length + array.Length + m_userPasswordOut.Length];
		Array.Copy(bytes, 0, array4, 0, bytes.Length);
		Array.Copy(array, 0, array4, bytes.Length, array.Length);
		Array.Copy(m_userPasswordOut, 0, array4, bytes.Length + array.Length, m_userPasswordOut.Length);
		array3 = AcrobatXComputeHash(array4, bytes, m_userPasswordOut);
		m_ownerPasswordOut = new byte[array3.Length + array.Length + array2.Length];
		Array.Copy(array3, 0, m_ownerPasswordOut, 0, array3.Length);
		Array.Copy(array, 0, m_ownerPasswordOut, array3.Length, array.Length);
		Array.Copy(array2, 0, m_ownerPasswordOut, array3.Length + array.Length, array2.Length);
		array3 = new byte[bytes.Length + array2.Length + m_userPasswordOut.Length];
		Array.Copy(bytes, 0, array3, 0, bytes.Length);
		Array.Copy(array2, 0, array3, bytes.Length, array2.Length);
		Array.Copy(m_userPasswordOut, 0, array3, bytes.Length + array2.Length, m_userPasswordOut.Length);
		array3 = AcrobatXComputeHash(array3, bytes, m_userPasswordOut);
		AesCipherNoPadding aesCipherNoPadding = new AesCipherNoPadding(isEncryption: true, array3);
		m_ownerEncryptionKeyOut = aesCipherNoPadding.ProcessBlock(m_fileEncryptionKey, 0, m_fileEncryptionKey.Length);
	}

	private byte[] CreateOwnerEncryptionKey()
	{
		byte[] array = new byte[8];
		byte[] array2 = new byte[8];
		byte[] bytes = Encoding.UTF8.GetBytes(m_ownerPassword);
		Array.Copy(m_ownerRandomBytes, 0, array, 0, 8);
		Array.Copy(m_ownerRandomBytes, 8, array2, 0, 8);
		byte[] array3 = new byte[bytes.Length + array.Length + m_userPasswordOut.Length];
		Array.Copy(bytes, 0, array3, 0, bytes.Length);
		Array.Copy(array2, 0, array3, bytes.Length, array.Length);
		Array.Copy(m_userPasswordOut, 0, array3, bytes.Length + array.Length, m_userPasswordOut.Length);
		byte[] array4 = new byte[32];
		IMessageDigest digest = new MessageDigestFinder().GetDigest("SHA256");
		digest.Update(array3, 0, array3.Length);
		digest.DoFinal(array4, 0);
		return new AesCipherNoPadding(isEncryption: true, array4).ProcessBlock(m_fileEncryptionKey, 0, m_fileEncryptionKey.Length);
	}

	private byte[] GetKeyFromOwnerPass(string password)
	{
		byte[] bytes = PadTrancateString(password);
		byte[] array = null;
		MessageDigestAlgorithms messageDigestAlgorithms = new MessageDigestAlgorithms();
		array = messageDigestAlgorithms.Digest("MD5", bytes);
		if (RevisionNumber > 2)
		{
			byte[] array2 = new byte[GetKeyLength()];
			IMessageDigest digest = new MessageDigestFinder().GetDigest("MD5");
			for (int i = 0; i < 50; i++)
			{
				array = messageDigestAlgorithms.Digest(digest, array, 0, array2.Length);
			}
		}
		byte[] array3 = new byte[GetKeyLength()];
		Array.Copy(array, array3, array3.Length);
		return array3;
	}

	private void FindFileEncryptionKey(string password)
	{
		byte[] array = null;
		byte[] array2 = null;
		if (m_ownerRandomBytes != null)
		{
			byte[] array3 = new byte[8];
			byte[] array4 = new byte[8];
			byte[] bytes = Encoding.UTF8.GetBytes(password);
			byte[] array5 = new byte[48];
			Array.Copy(m_userPasswordOut, 0, array5, 0, 48);
			Array.Copy(m_ownerRandomBytes, 0, array3, 0, 8);
			Array.Copy(m_ownerRandomBytes, 8, array4, 0, 8);
			byte[] array6 = new byte[bytes.Length + array3.Length + array5.Length];
			Array.Copy(bytes, 0, array6, 0, bytes.Length);
			Array.Copy(array4, 0, array6, bytes.Length, array4.Length);
			Array.Copy(array5, 0, array6, bytes.Length + array3.Length, array5.Length);
			array = new byte[32];
			IMessageDigest digest = new MessageDigestFinder().GetDigest("SHA256");
			digest.Update(array6, 0, array6.Length);
			digest.DoFinal(array, 0);
			array2 = m_ownerEncryptionKeyOut;
		}
		else if (m_userRandomBytes != null)
		{
			byte[] destinationArray = new byte[8];
			byte[] array7 = new byte[8];
			byte[] bytes2 = Encoding.UTF8.GetBytes(password);
			Array.Copy(m_userRandomBytes, 0, destinationArray, 0, 8);
			Array.Copy(m_userRandomBytes, 8, array7, 0, 8);
			byte[] array6 = new byte[bytes2.Length + array7.Length];
			Array.Copy(bytes2, 0, array6, 0, bytes2.Length);
			Array.Copy(array7, 0, array6, bytes2.Length, array7.Length);
			array = new byte[32];
			IMessageDigest digest2 = new MessageDigestFinder().GetDigest("SHA256");
			digest2.Update(array6, 0, array6.Length);
			digest2.DoFinal(array, 0);
			array2 = m_userEncryptionKeyOut;
		}
		AesCipherNoPadding aesCipherNoPadding = new AesCipherNoPadding(isEncryption: false, array);
		m_fileEncryptionKey = aesCipherNoPadding.ProcessBlock(array2, 0, array2.Length);
	}

	private void AcrobatXOwnerFileEncryptionKey(string password)
	{
		byte[] array = new byte[8];
		byte[] bytes = Encoding.UTF8.GetBytes(password);
		Array.Copy(m_ownerPasswordOut, 40, array, 0, 8);
		int num = 48;
		if (m_userPasswordOut.Length < 48)
		{
			num = m_userPasswordOut.Length;
		}
		byte[] array2 = new byte[bytes.Length + array.Length + num];
		Array.Copy(bytes, 0, array2, 0, bytes.Length);
		Array.Copy(array, 0, array2, bytes.Length, array.Length);
		Array.Copy(m_userPasswordOut, 0, array2, bytes.Length + array.Length, num);
		byte[] key = AcrobatXComputeHash(array2, bytes, m_userPasswordOut);
		byte[] ownerEncryptionKeyOut = m_ownerEncryptionKeyOut;
		AesCipherNoPadding aesCipherNoPadding = new AesCipherNoPadding(isEncryption: false, key);
		m_fileEncryptionKey = aesCipherNoPadding.ProcessBlock(ownerEncryptionKeyOut, 0, ownerEncryptionKeyOut.Length);
	}

	private void AdvanceXUserFileEncryptionKey(string password)
	{
		byte[] array = null;
		byte[] array2 = null;
		byte[] array3 = new byte[8];
		Array.Copy(m_userPasswordOut, 40, array3, 0, 8);
		byte[] bytes = Encoding.UTF8.GetBytes(password);
		byte[] array4 = new byte[bytes.Length + array3.Length];
		Array.Copy(bytes, 0, array4, 0, bytes.Length);
		Array.Copy(array3, 0, array4, bytes.Length, array3.Length);
		array = AcrobatXComputeHash(array4, bytes, null);
		array2 = m_userEncryptionKeyOut;
		AesCipherNoPadding aesCipherNoPadding = new AesCipherNoPadding(isEncryption: false, array);
		m_fileEncryptionKey = aesCipherNoPadding.ProcessBlock(array2, 0, array2.Length);
	}

	private byte[] GetKeyForOwnerPassStep7(byte[] originalKey, byte index)
	{
		if (originalKey == null)
		{
			throw new ArgumentNullException("originalKey");
		}
		byte[] array = new byte[originalKey.Length];
		int i = 0;
		for (int num = originalKey.Length; i < num; i++)
		{
			array[i] = (byte)(originalKey[i] ^ index);
		}
		return array;
	}

	private byte[] CreateEncryptionKey(string inputPass, byte[] ownerPass)
	{
		if (inputPass == null)
		{
			throw new ArgumentNullException("inputPass");
		}
		if (ownerPass == null)
		{
			throw new ArgumentNullException("ownerPass");
		}
		byte[] collection = PadTrancateString(inputPass);
		List<byte> list = new List<byte>();
		list.AddRange(collection);
		list.AddRange(ownerPass);
		byte[] collection2 = new byte[4]
		{
			(byte)m_permissionValue,
			(byte)(m_permissionValue >> 8),
			(byte)(m_permissionValue >> 16),
			(byte)(m_permissionValue >> 24)
		};
		list.AddRange(collection2);
		list.AddRange(RandomBytes);
		int num = ((m_revision == 0) ? ((int)(m_keyLength + 2)) : RevisionNumber);
		if (num > 3 && !EncryptMetaData)
		{
			list.Add(byte.MaxValue);
			list.Add(byte.MaxValue);
			list.Add(byte.MaxValue);
			list.Add(byte.MaxValue);
		}
		byte[] array = list.ToArray();
		byte[] array2 = null;
		MD5.Reset();
		MD5.Update(array, 0, array.Length);
		array2 = new byte[MD5.MessageDigestSize];
		MD5.DoFinal(array2, 0);
		byte[] array3 = new byte[GetKeyLength()];
		Array.Copy(array2, array3, array3.Length);
		MD5.Reset();
		if (RevisionNumber > 2)
		{
			MessageDigestAlgorithms messageDigestAlgorithms = new MessageDigestAlgorithms();
			byte[] array4 = new byte[GetKeyLength()];
			IMessageDigest digest = new MessageDigestFinder().GetDigest("MD5");
			for (int i = 0; i < 50; i++)
			{
				array2 = messageDigestAlgorithms.Digest(digest, array2, 0, array4.Length);
			}
		}
		EncryptionKey = new byte[GetKeyLength()];
		Array.Copy(array2, EncryptionKey, EncryptionKey.Length);
		return EncryptionKey;
	}

	private void CreateFileEncryptionKey()
	{
		m_fileEncryptionKey = new byte[32];
		m_randomArray.NextBytes(m_fileEncryptionKey);
	}

	private byte[] CreateUserPassword()
	{
		byte[] array = null;
		if (RevisionNumber == 2)
		{
			return Create40BitUserPassword();
		}
		return Create128BitUserPassword();
	}

	private byte[] Create256BitUserPassword()
	{
		byte[] array = new byte[8];
		byte[] array2 = new byte[8];
		m_userRandomBytes = new byte[16];
		m_randomArray.NextBytes(m_userRandomBytes);
		byte[] bytes = Encoding.UTF8.GetBytes(m_userPassword);
		Array.Copy(m_userRandomBytes, 0, array, 0, 8);
		Array.Copy(m_userRandomBytes, 8, array2, 0, 8);
		byte[] array3 = new byte[UserPassword.Length + array.Length];
		Array.Copy(bytes, 0, array3, 0, bytes.Length);
		Array.Copy(array, 0, array3, UserPassword.Length, array.Length);
		byte[] array4 = new byte[32];
		IMessageDigest digest = new MessageDigestFinder().GetDigest("SHA256");
		digest.Update(array3, 0, array3.Length);
		digest.DoFinal(array4, 0);
		byte[] array5 = new byte[array4.Length + array.Length + array2.Length];
		Array.Copy(array4, 0, array5, 0, array4.Length);
		Array.Copy(array, 0, array5, array4.Length, array.Length);
		Array.Copy(array2, 0, array5, array4.Length + array.Length, array2.Length);
		return array5;
	}

	private void CreateAcrobatX256BitUserPassword()
	{
		byte[] array = new byte[8];
		byte[] array2 = new byte[8];
		byte[] array3 = null;
		array3 = Encoding.UTF8.GetBytes(m_userPassword);
		m_randomArray.NextBytes(array);
		m_randomArray.NextBytes(array2);
		byte[] array4 = new byte[array3.Length + array2.Length];
		Array.Copy(array3, 0, array4, 0, array3.Length);
		Array.Copy(array, 0, array4, array3.Length, array.Length);
		array4 = AcrobatXComputeHash(array4, array3, null);
		m_userPasswordOut = new byte[array4.Length + array.Length + array2.Length];
		Array.Copy(array4, 0, m_userPasswordOut, 0, array4.Length);
		Array.Copy(array, 0, m_userPasswordOut, array4.Length, array.Length);
		Array.Copy(array2, 0, m_userPasswordOut, array4.Length + array.Length, array2.Length);
		array4 = new byte[array3.Length + array2.Length];
		Array.Copy(array3, 0, array4, 0, array3.Length);
		Array.Copy(array2, 0, array4, array3.Length, array2.Length);
		array4 = AcrobatXComputeHash(array4, array3, null);
		AesCipherNoPadding aesCipherNoPadding = new AesCipherNoPadding(isEncryption: true, array4);
		m_userEncryptionKeyOut = aesCipherNoPadding.ProcessBlock(m_fileEncryptionKey, 0, m_fileEncryptionKey.Length);
	}

	private byte[] CreateUserEncryptionKey()
	{
		byte[] destinationArray = new byte[8];
		byte[] array = new byte[8];
		byte[] bytes = Encoding.UTF8.GetBytes(m_userPassword);
		Array.Copy(m_userRandomBytes, 0, destinationArray, 0, 8);
		Array.Copy(m_userRandomBytes, 8, array, 0, 8);
		byte[] array2 = new byte[bytes.Length + array.Length];
		Array.Copy(bytes, 0, array2, 0, bytes.Length);
		Array.Copy(array, 0, array2, bytes.Length, array.Length);
		byte[] array3 = new byte[32];
		IMessageDigest digest = new MessageDigestFinder().GetDigest("SHA256");
		digest.Update(array2, 0, array2.Length);
		digest.DoFinal(array3, 0);
		return new AesCipherNoPadding(isEncryption: true, array3).ProcessBlock(m_fileEncryptionKey, 0, m_fileEncryptionKey.Length);
	}

	private byte[] CreatePermissionFlag()
	{
		byte[] array = new byte[16];
		byte[] array2 = new byte[4]
		{
			(byte)m_permissionValue,
			(byte)(m_permissionValue >> 8),
			(byte)(m_permissionValue >> 16),
			(byte)(m_permissionValue >> 24)
		};
		Array.Copy(array2, 0, array, 0, array2.Length);
		int num = array2.Length;
		array[num++] = byte.MaxValue;
		array[num++] = byte.MaxValue;
		array[num++] = byte.MaxValue;
		array[num++] = byte.MaxValue;
		if (EncryptMetaData)
		{
			array[num++] = 84;
		}
		else
		{
			array[num++] = 70;
		}
		array[num++] = 97;
		array[num++] = 100;
		array[num++] = 98;
		array[num++] = 98;
		array[num++] = 98;
		array[num++] = 98;
		array[num++] = 98;
		return new AesCipherNoPadding(isEncryption: true, m_fileEncryptionKey).ProcessBlock(array, 0, array.Length);
	}

	private byte[] Create40BitUserPassword()
	{
		if (EncryptionKey == null)
		{
			throw new ArgumentNullException("EncryptionKey");
		}
		byte[] data = PadTrancateString(string.Empty);
		return EncryptDataByCustom(data, EncryptionKey);
	}

	private byte[] Create128BitUserPassword()
	{
		if (EncryptionKey == null)
		{
			throw new ArgumentNullException("EncryptionKey");
		}
		List<byte> list = new List<byte>();
		byte[] collection = PadTrancateString(string.Empty);
		list.AddRange(collection);
		list.AddRange(RandomBytes);
		byte[] array = list.ToArray();
		byte[] array2 = null;
		MD5.Update(array, 0, array.Length);
		array2 = new byte[MD5.MessageDigestSize];
		MD5.DoFinal(array2, 0);
		MD5.Reset();
		byte[] array3 = new byte[16];
		Array.Copy(array2, 0, array3, 0, array3.Length);
		byte[] array4 = EncryptDataByCustom(array3, EncryptionKey);
		byte[] encryptionKey = EncryptionKey;
		for (byte b = 1; b < 20; b++)
		{
			encryptionKey = GetKeyForOwnerPassStep7(EncryptionKey, b);
			array4 = EncryptDataByCustom(array4, encryptionKey, encryptionKey.Length);
		}
		return PadTrancateString(array4);
	}

	private void InitializeData()
	{
		if (!m_hasComputedPasswordValues)
		{
			if (CryptographicAlgorithm == PdfEncryptionKeySize.Key256Bit)
			{
				m_userPasswordOut = Create256BitUserPassword();
				m_ownerPasswordOut = Create256BitOwnerPassword();
				CreateFileEncryptionKey();
				m_userEncryptionKeyOut = CreateUserEncryptionKey();
				m_ownerEncryptionKeyOut = CreateOwnerEncryptionKey();
				m_permissionFlag = CreatePermissionFlag();
			}
			else if (CryptographicAlgorithm == PdfEncryptionKeySize.Key256BitRevision6)
			{
				CreateFileEncryptionKey();
				CreateAcrobatX256BitUserPassword();
				CreateAcrobatX256BitOwnerPassword();
				m_permissionFlag = CreatePermissionFlag();
			}
			else
			{
				m_ownerPasswordOut = CreateOwnerPassword();
				m_encryptionKey = CreateEncryptionKey(UserPassword, m_ownerPasswordOut);
				m_userPasswordOut = CreateUserPassword();
			}
			m_hasComputedPasswordValues = true;
		}
	}

	private byte[] PrepareKeyForEncryption(byte[] originalKey)
	{
		if (originalKey == null)
		{
			throw new ArgumentNullException("originalKey");
		}
		int num = originalKey.Length;
		byte[] array = null;
		MD5.Reset();
		MD5.Update(originalKey, 0, originalKey.Length);
		array = new byte[MD5.MessageDigestSize];
		MD5.DoFinal(array, 0);
		MD5.Reset();
		byte[] array2 = array;
		if (num > 16)
		{
			int num2 = Math.Min(GetKeyLength() + 5, 16);
			array2 = new byte[num2];
			Array.Copy(array, 0, array2, 0, num2);
		}
		return array2;
	}

	private bool AuthenticateUserPassword(string password)
	{
		if (m_keyLength == PdfEncryptionKeySize.Key256Bit || m_keyLength == PdfEncryptionKeySize.Key256BitRevision6)
		{
			return Authenticate256BitUserPassword(password);
		}
		m_encryptionKey = CreateEncryptionKey(password, m_ownerPasswordOut);
		byte[] array = CreateUserPassword();
		if (RevisionNumber == 2)
		{
			return CompareByteArrays(array, m_userPasswordOut);
		}
		return CompareByteArrays(array, m_userPasswordOut, 16);
	}

	private bool Authenticate256BitUserPassword(string password)
	{
		byte[] array = new byte[8];
		byte[] array2 = new byte[8];
		byte[] array3 = new byte[32];
		m_userRandomBytes = new byte[16];
		byte[] array5;
		if (m_keyLength == PdfEncryptionKeySize.Key256BitRevision6)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(password);
			Array.Copy(m_userPasswordOut, 0, array3, 0, 32);
			Array.Copy(m_userPasswordOut, 32, array, 0, 8);
			byte[] array4 = new byte[bytes.Length + array.Length];
			Array.Copy(bytes, 0, array4, 0, bytes.Length);
			Array.Copy(array, 0, array4, bytes.Length, array.Length);
			array5 = AcrobatXComputeHash(array4, bytes, null);
			AdvanceXUserFileEncryptionKey(password);
			return CompareByteArrays(array5, array3);
		}
		byte[] bytes2 = Encoding.UTF8.GetBytes(password);
		Array.Copy(m_userPasswordOut, 0, array3, 0, array3.Length);
		Array.Copy(m_userPasswordOut, 32, m_userRandomBytes, 0, 16);
		Array.Copy(m_userRandomBytes, 0, array, 0, array.Length);
		Array.Copy(m_userRandomBytes, array.Length, array2, 0, array2.Length);
		array5 = new byte[bytes2.Length + array.Length];
		Array.Copy(bytes2, 0, array5, 0, bytes2.Length);
		Array.Copy(array, 0, array5, bytes2.Length, array.Length);
		byte[] array6 = new byte[32];
		IMessageDigest digest = new MessageDigestFinder().GetDigest("SHA256");
		digest.Update(array5, 0, array5.Length);
		digest.DoFinal(array6, 0);
		bool flag = false;
		if (array6.Length == array3.Length)
		{
			int i;
			for (i = 0; i < array6.Length && array6[i] == array3[i]; i++)
			{
			}
			if (i == array6.Length)
			{
				flag = true;
			}
		}
		if (flag)
		{
			FindFileEncryptionKey(password);
		}
		return flag;
	}

	private bool AuthenticateOwnerPassword(string password)
	{
		if (m_keyLength == PdfEncryptionKeySize.Key256Bit || m_keyLength == PdfEncryptionKeySize.Key256BitRevision6)
		{
			return Authenticate256BitOwnerPassword(password);
		}
		m_encryptionKey = GetKeyFromOwnerPass(password);
		byte[] array = m_ownerPasswordOut;
		if (RevisionNumber == 2)
		{
			array = EncryptDataByCustom(array, m_encryptionKey);
		}
		else if (RevisionNumber > 2)
		{
			array = m_ownerPasswordOut;
			for (int i = 0; i < 20; i++)
			{
				byte[] keyForOwnerPassStep = GetKeyForOwnerPassStep7(m_encryptionKey, (byte)(20 - i - 1));
				array = EncryptDataByCustom(array, keyForOwnerPassStep);
			}
		}
		m_encryptionKey = null;
		string text = ConvertToPassword(array);
		if (AuthenticateUserPassword(text))
		{
			m_userPassword = text;
			m_ownerPassword = password;
			return true;
		}
		return false;
	}

	private bool Authenticate256BitOwnerPassword(string password)
	{
		byte[] array = new byte[8];
		byte[] array2 = new byte[8];
		byte[] array3 = new byte[32];
		m_ownerRandomBytes = new byte[16];
		byte[] array5;
		if (m_keyLength == PdfEncryptionKeySize.Key256BitRevision6)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(password);
			Array.Copy(m_ownerPasswordOut, 0, array3, 0, 32);
			Array.Copy(m_ownerPasswordOut, 32, array, 0, 8);
			int num = 48;
			if (m_userPasswordOut.Length < 48)
			{
				num = m_userPasswordOut.Length;
			}
			byte[] array4 = new byte[bytes.Length + array.Length + num];
			Array.Copy(bytes, 0, array4, 0, bytes.Length);
			Array.Copy(array, 0, array4, bytes.Length, array.Length);
			Array.Copy(m_userPasswordOut, 0, array4, bytes.Length + array.Length, num);
			array5 = AcrobatXComputeHash(array4, bytes, m_userPasswordOut);
			AcrobatXOwnerFileEncryptionKey(password);
			bool num2 = CompareByteArrays(array5, array3);
			if (num2)
			{
				byte[] fileEncryptionKey = m_fileEncryptionKey;
				m_ownerRandomBytes = null;
				if (AuthenticateUserPassword(password))
				{
					m_userPassword = password;
					m_ownerPassword = password;
					return num2;
				}
				m_fileEncryptionKey = fileEncryptionKey;
				return num2;
			}
			m_ownerRandomBytes = null;
			return num2;
		}
		byte[] array6 = new byte[48];
		Array.Copy(m_userPasswordOut, 0, array6, 0, 48);
		byte[] bytes2 = Encoding.UTF8.GetBytes(password);
		Array.Copy(m_ownerPasswordOut, 0, array3, 0, array3.Length);
		Array.Copy(m_ownerPasswordOut, 32, m_ownerRandomBytes, 0, 16);
		Array.Copy(m_ownerRandomBytes, 0, array, 0, array.Length);
		Array.Copy(m_ownerRandomBytes, array.Length, array2, 0, array2.Length);
		array5 = new byte[bytes2.Length + array.Length + array6.Length];
		Array.Copy(bytes2, 0, array5, 0, bytes2.Length);
		Array.Copy(array, 0, array5, bytes2.Length, array.Length);
		Array.Copy(array6, 0, array5, bytes2.Length + array.Length, array6.Length);
		byte[] array7 = new byte[32];
		IMessageDigest digest = new MessageDigestFinder().GetDigest("SHA256");
		digest.Update(array5, 0, array5.Length);
		digest.DoFinal(array7, 0);
		bool flag = false;
		if (array7.Length == array3.Length)
		{
			int i;
			for (i = 0; i < array7.Length && array7[i] == array3[i]; i++)
			{
			}
			if (i == array7.Length)
			{
				flag = true;
			}
		}
		FindFileEncryptionKey(password);
		if (flag)
		{
			m_ownerRandomBytes = null;
			if (AuthenticateUserPassword(password))
			{
				m_userPassword = password;
				m_ownerPassword = password;
			}
		}
		else
		{
			m_ownerRandomBytes = null;
		}
		return flag;
	}

	private string ConvertToPassword(byte[] array)
	{
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			if (array[i] == s_paddingString[0] && i < num - 1 && array[i + 1] == s_paddingString[1])
			{
				num = i;
				break;
			}
		}
		return PdfString.ByteToString(array, num);
	}

	private bool CompareByteArrays(byte[] array1, byte[] array2)
	{
		bool result = true;
		if (array1 == null || array2 == null)
		{
			result = array1 == array2;
		}
		else if (array1.Length != array2.Length)
		{
			result = false;
		}
		else
		{
			int i = 0;
			for (int num = array1.Length; i < num; i++)
			{
				if (array1[i] != array2[i])
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	private bool CompareByteArrays(byte[] array1, byte[] array2, int size)
	{
		bool result = true;
		if (array1 == null || array2 == null)
		{
			result = array1 == array2;
		}
		else
		{
			if (array1.Length < size || array2.Length < size)
			{
				throw new ArgumentException("Size of one of the arrays are less then requisted size.");
			}
			if (array1.Length != array2.Length)
			{
				result = false;
			}
			else
			{
				for (int i = 0; i < size; i++)
				{
					if (array1[i] != array2[i])
					{
						result = false;
						break;
					}
				}
			}
		}
		return result;
	}

	private PdfDictionary AESDictionary()
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		PdfDictionary pdfDictionary2 = new PdfDictionary();
		if (!pdfDictionary2.ContainsKey(new PdfName("CFM")))
		{
			if (m_encryptOnlyAttachment)
			{
				pdfDictionary2[new PdfName("Type")] = new PdfName("CryptFilter");
				pdfDictionary2[new PdfName("CFM")] = new PdfName("AESV2");
			}
			else if (CryptographicAlgorithm == PdfEncryptionKeySize.Key256Bit || CryptographicAlgorithm == PdfEncryptionKeySize.Key256BitRevision6)
			{
				pdfDictionary2[new PdfName("CFM")] = new PdfName("AESV3");
			}
			else if (EncryptionAlgorithm == PdfEncryptionAlgorithm.RC4 && CryptographicAlgorithm == PdfEncryptionKeySize.Key128Bit)
			{
				pdfDictionary2[new PdfName("CFM")] = new PdfName("V2");
			}
			else
			{
				pdfDictionary2[new PdfName("CFM")] = new PdfName("AESV2");
			}
		}
		if (!pdfDictionary2.ContainsKey(new PdfName("AuthEvent")))
		{
			if (m_encryptOnlyAttachment)
			{
				pdfDictionary2[new PdfName("AuthEvent")] = new PdfName("EFOpen");
			}
			else
			{
				pdfDictionary2[new PdfName("AuthEvent")] = new PdfName("DocOpen");
			}
		}
		if (!pdfDictionary2.ContainsKey(new PdfName("Length")))
		{
			if (CryptographicAlgorithm == PdfEncryptionKeySize.Key256Bit || CryptographicAlgorithm == PdfEncryptionKeySize.Key256BitRevision6)
			{
				pdfDictionary2[new PdfName("Length")] = new PdfNumber(32);
			}
			else if (CryptographicAlgorithm == PdfEncryptionKeySize.Key128Bit)
			{
				pdfDictionary2[new PdfName("Length")] = new PdfNumber(16);
			}
			else
			{
				pdfDictionary2[new PdfName("Length")] = new PdfNumber(128);
			}
		}
		if (!pdfDictionary.ContainsKey(new PdfName("StdCF")))
		{
			pdfDictionary[new PdfName("StdCF")] = pdfDictionary2;
		}
		return pdfDictionary;
	}
}

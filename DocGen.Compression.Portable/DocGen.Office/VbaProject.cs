using System;
using System.Globalization;
using System.IO;
using System.Text;
using DocGen.Compression;
using DocGen.Compression.Zip;

namespace DocGen.Office;

internal class VbaProject : IVbaProject
{
	private VbaModulesCollection m_modules;

	private ReferenceRecordsCollection m_references;

	protected object m_parent;

	private SystemKind m_kind;

	private string m_name;

	private string m_description;

	private string m_password;

	private string m_constants;

	private bool m_IsViewLocked;

	private string m_helpFile1;

	private string m_helpFile2;

	private uint m_helpTopic;

	private uint m_lcId;

	private uint m_lcInvoke;

	private ushort m_codePage;

	private uint m_majorVersion;

	private ushort m_minorVersion;

	private string m_clsID;

	private string m_protectionData;

	private string m_passwordData;

	private string m_lockviewData;

	internal SystemKind SystemType
	{
		get
		{
			return m_kind;
		}
		set
		{
			m_kind = value;
		}
	}

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	public string Description
	{
		get
		{
			return m_description;
		}
		set
		{
			m_description = value;
		}
	}

	internal string Password
	{
		get
		{
			return m_password;
		}
		set
		{
			m_password = value;
		}
	}

	public string Constants
	{
		get
		{
			return m_constants;
		}
		set
		{
			m_constants = value;
		}
	}

	internal bool LockView
	{
		get
		{
			return m_IsViewLocked;
		}
		set
		{
			m_IsViewLocked = value;
		}
	}

	public string HelpFile
	{
		get
		{
			return m_helpFile1;
		}
		set
		{
			m_helpFile1 = value;
		}
	}

	internal string SecondaryHelpFile
	{
		get
		{
			return m_helpFile2;
		}
		set
		{
			m_helpFile2 = value;
		}
	}

	public uint HelpContextId
	{
		get
		{
			return m_helpTopic;
		}
		set
		{
			m_helpTopic = value;
		}
	}

	internal uint LcId
	{
		get
		{
			return m_lcId;
		}
		set
		{
			m_lcId = value;
		}
	}

	internal uint LcInvoke
	{
		get
		{
			return m_lcInvoke;
		}
		set
		{
			m_lcInvoke = value;
		}
	}

	internal ushort CodePage
	{
		get
		{
			return m_codePage;
		}
		set
		{
			m_codePage = value;
		}
	}

	internal uint MajorVersion
	{
		get
		{
			return m_majorVersion;
		}
		set
		{
			m_majorVersion = value;
		}
	}

	internal ushort MinorVersion
	{
		get
		{
			return m_minorVersion;
		}
		set
		{
			m_minorVersion = value;
		}
	}

	internal Encoding EncodingType => new LatinEncoding();

	internal string ProjectCLSID
	{
		get
		{
			return m_clsID;
		}
		set
		{
			m_clsID = value;
		}
	}

	public IVbaModules Modules
	{
		get
		{
			return m_modules;
		}
		set
		{
			m_modules = value as VbaModulesCollection;
		}
	}

	internal ReferenceRecordsCollection References
	{
		get
		{
			if (m_references == null)
			{
				m_references = new ReferenceRecordsCollection();
			}
			return m_references;
		}
	}

	internal VbaProject(object parent)
	{
		m_parent = parent;
		m_name = "VBAProject";
		m_lcId = 1033u;
		m_lcInvoke = 1033u;
		m_codePage = 1252;
		m_minorVersion = 3;
		m_majorVersion = 1602932494u;
		m_clsID = "{EE89B4E9-F8E5-45FE-9D4D-0A42D7254B84}";
		m_kind = SystemKind.Win32;
		Description = string.Empty;
		HelpFile = string.Empty;
		SecondaryHelpFile = string.Empty;
		m_modules = new VbaModulesCollection(this);
	}

	internal void ParseDirStream(Stream dirStream)
	{
		using (dirStream = VbaDataProcess.Decompress(dirStream))
		{
			dirStream.Position = 0L;
			ParseProjectInfo(dirStream);
			ParseReferences(dirStream);
			dirStream.Position += 6L;
			ushort num = ZipArchive.ReadUInt16(dirStream);
			dirStream.Position += 8L;
			while (num > 0)
			{
				VbaModule vbaModule = new VbaModule(Modules as VbaModulesCollection);
				vbaModule.ParseModuleRecord(dirStream);
				(Modules as VbaModulesCollection).Add(vbaModule);
				num--;
			}
			dirStream.Position += 6L;
		}
	}

	internal void ParseModuleStream(Stream dirStream, VbaModule module)
	{
		dirStream.Position += module.ModuleOffSet;
		byte[] array = new byte[dirStream.Length - module.ModuleOffSet];
		dirStream.Read(array, 0, array.Length);
		Stream compressed = new MemoryStream(array);
		using (compressed = VbaDataProcess.Decompress(compressed))
		{
			byte[] array2 = (compressed as MemoryStream).ToArray();
			string @string = EncodingType.GetString(array2, 0, array2.Length);
			int num = 0;
			int num2 = 0;
			@string = @string.Replace("\r\n", "\n");
			while (@string.IndexOf("Attribute", num) == num2)
			{
				num = @string.IndexOf("\n", num2);
				string[] array3 = @string.Substring(num2 + 10, num - (num2 + 10)).Split('=');
				VbaAttribute vbaAttribute = new VbaAttribute();
				vbaAttribute.Name = array3[0].Trim();
				vbaAttribute.Value = array3[1].Trim().Replace("\"", string.Empty);
				vbaAttribute.IsText = array3[1].Trim().StartsWith("\"");
				module.Attributes.Add(vbaAttribute);
				num2 = num + 1;
			}
			module.Code = @string.Substring(num2);
		}
	}

	internal void ParseProjectStream(Stream stream)
	{
		byte[] array = new byte[stream.Length];
		stream.Read(array, 0, array.Length);
		string[] array2 = EncodingType.GetString(array, 0, array.Length).Split(new string[1] { "\r\n" }, StringSplitOptions.None);
		string package = null;
		VbaModule vbaModule = null;
		string[] array3 = array2;
		foreach (string text in array3)
		{
			string[] array4 = text.Split('=');
			array4[0] = array4[0].Replace("\"", string.Empty);
			switch (array4[0])
			{
			case "ID":
				m_clsID = array4[1].Replace("\"", string.Empty);
				break;
			case "Package":
				package = array4[1].Replace("\"", string.Empty);
				break;
			case "Document":
				if (m_modules[array4[1].Substring(0, array4[1].IndexOf("/&H"))] is VbaModule vbaModule3)
				{
					vbaModule3.Type = VbaModuleType.Document;
				}
				break;
			case "Class":
				if (m_modules[array4[1]] is VbaModule vbaModule5)
				{
					vbaModule5.Type = VbaModuleType.ClassModule;
				}
				break;
			case "Module":
				if (m_modules[array4[1]] is VbaModule vbaModule4)
				{
					vbaModule4.Type = VbaModuleType.StdModule;
				}
				break;
			case "BaseClass":
				if (m_modules[array4[1]] is VbaModule vbaModule2)
				{
					vbaModule2.Type = VbaModuleType.MsForm;
					vbaModule2.Package = package;
				}
				break;
			case "CMG":
				m_protectionData = text;
				break;
			case "DPB":
				m_passwordData = text;
				break;
			case "GC":
				m_lockviewData = text;
				break;
			}
		}
	}

	internal void ParseProjectInfo(Stream dirData)
	{
		dirData.Position = 6L;
		SystemType = (SystemKind)ReadUInt32(dirData);
		if (ReadUInt16(dirData) == 74)
		{
			dirData.Position += 8L;
		}
		else
		{
			dirData.Position -= 2L;
		}
		dirData.Position += 6L;
		LcId = ReadUInt32(dirData);
		dirData.Position += 6L;
		LcInvoke = ReadUInt32(dirData);
		dirData.Position += 6L;
		CodePage = ReadUInt16(dirData);
		byte[] array = null;
		dirData.Position += 2L;
		int num = (int)ReadUInt32(dirData);
		array = new byte[num];
		dirData.Read(array, 0, num);
		Encoding encodingType = EncodingType;
		Name = encodingType.GetString(array, 0, array.Length);
		dirData.Position += 2L;
		num = (int)ReadUInt32(dirData);
		dirData.Position += num;
		dirData.Position += 2L;
		num = (int)ReadUInt32(dirData);
		array = new byte[num];
		dirData.Read(array, 0, num);
		Description = Encoding.Unicode.GetString(array, 0, array.Length);
		dirData.Position += 2L;
		num = (int)ReadUInt32(dirData);
		array = new byte[num];
		dirData.Read(array, 0, num);
		HelpFile = encodingType.GetString(array, 0, array.Length);
		dirData.Position += 2L;
		num = (int)ReadUInt32(dirData);
		array = new byte[num];
		dirData.Read(array, 0, num);
		SecondaryHelpFile = encodingType.GetString(array, 0, array.Length);
		dirData.Position += 6L;
		HelpContextId = ReadUInt32(dirData);
		dirData.Position += 10L;
		dirData.Position += 6L;
		MajorVersion = ReadUInt32(dirData);
		MinorVersion = ReadUInt16(dirData);
		if (ZipArchive.ReadUInt16(dirData) == 12)
		{
			num = (int)ReadUInt32(dirData);
			array = new byte[num];
			dirData.Read(array, 0, num);
			Constants = encodingType.GetString(array, 0, array.Length);
			dirData.Position += 2L;
			num = (int)ReadUInt32(dirData);
			dirData.Position += num;
		}
		else
		{
			dirData.Position -= 2L;
		}
	}

	internal void ParseReferences(Stream dirData)
	{
		for (ushort num = ReadUInt16(dirData); num != 15; num = ReadUInt16(dirData))
		{
			int num2 = 0;
			byte[] array = null;
			ReferenceRecord referenceRecord = null;
			if (num == 22)
			{
				num2 = (int)ReadUInt32(dirData);
				array = new byte[num2];
				dirData.Read(array, 0, num2);
				string @string = EncodingType.GetString(array, 0, array.Length);
				num = ReadUInt16(dirData);
				if (num == 62)
				{
					num2 = (int)ReadUInt32(dirData);
					dirData.Position += num2;
					num = ReadUInt16(dirData);
				}
				referenceRecord = References.Add((VbaReferenceType)num);
				referenceRecord.Name = @string;
			}
			else
			{
				referenceRecord = References.Add((VbaReferenceType)num);
			}
			if (referenceRecord != null)
			{
				referenceRecord.EncodingType = EncodingType;
				referenceRecord.ParseRecord(dirData);
			}
		}
		dirData.Position -= 2L;
	}

	internal void SerializeVbaStream(Stream dirStream)
	{
		dirStream.Write(BitConverter.GetBytes(25036), 0, 2);
		dirStream.Write(BitConverter.GetBytes(65535), 0, 2);
		dirStream.WriteByte(0);
		dirStream.Write(BitConverter.GetBytes(0), 0, 2);
		dirStream.Flush();
	}

	internal void SerializeDirStream(Stream dirStream)
	{
		Stream stream = new MemoryStream();
		SerializeProjectInfo(stream);
		SerializeReferences(stream);
		SerializeModules(stream);
		stream.Write(BitConverter.GetBytes(16), 0, 2);
		stream.Write(BitConverter.GetBytes(0), 0, 4);
		stream.Position = 0L;
		stream = VbaDataProcess.Compress(stream as MemoryStream);
		stream.Position = 0L;
		byte[] array = (stream as MemoryStream).ToArray();
		dirStream.Write(array, 0, array.Length);
		dirStream.Flush();
	}

	internal void SerializeProjectInfo(Stream dirStream)
	{
		dirStream.Write(BitConverter.GetBytes(1), 0, 2);
		dirStream.Write(BitConverter.GetBytes(4), 0, 4);
		dirStream.Write(BitConverter.GetBytes((int)m_kind), 0, 4);
		dirStream.Write(BitConverter.GetBytes(2), 0, 2);
		dirStream.Write(BitConverter.GetBytes(4), 0, 4);
		dirStream.Write(BitConverter.GetBytes(1033), 0, 4);
		dirStream.Write(BitConverter.GetBytes(20), 0, 2);
		dirStream.Write(BitConverter.GetBytes(4), 0, 4);
		dirStream.Write(BitConverter.GetBytes(1033), 0, 4);
		dirStream.Write(BitConverter.GetBytes(3), 0, 2);
		dirStream.Write(BitConverter.GetBytes(2), 0, 4);
		dirStream.Write(BitConverter.GetBytes(CodePage), 0, 2);
		byte[] bytes = EncodingType.GetBytes(Name);
		dirStream.Write(BitConverter.GetBytes(4), 0, 2);
		dirStream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
		dirStream.Write(bytes, 0, bytes.Length);
		bytes = EncodingType.GetBytes(Description);
		dirStream.Write(BitConverter.GetBytes(5), 0, 2);
		dirStream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
		dirStream.Write(bytes, 0, bytes.Length);
		bytes = Encoding.Unicode.GetBytes(Description);
		dirStream.Write(BitConverter.GetBytes(64), 0, 2);
		dirStream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
		dirStream.Write(bytes, 0, bytes.Length);
		bytes = EncodingType.GetBytes(m_helpFile1);
		dirStream.Write(BitConverter.GetBytes(6), 0, 2);
		dirStream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
		dirStream.Write(bytes, 0, bytes.Length);
		bytes = Encoding.Unicode.GetBytes(m_helpFile2);
		dirStream.Write(BitConverter.GetBytes(61), 0, 2);
		dirStream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
		dirStream.Write(bytes, 0, bytes.Length);
		dirStream.Write(BitConverter.GetBytes(7), 0, 2);
		dirStream.Write(BitConverter.GetBytes(4), 0, 4);
		dirStream.Write(BitConverter.GetBytes(HelpContextId), 0, 4);
		dirStream.Write(BitConverter.GetBytes(8), 0, 2);
		dirStream.Write(BitConverter.GetBytes(4), 0, 4);
		dirStream.Write(BitConverter.GetBytes(0), 0, 4);
		dirStream.Write(BitConverter.GetBytes(9), 0, 2);
		dirStream.Write(BitConverter.GetBytes(4), 0, 4);
		dirStream.Write(BitConverter.GetBytes(MajorVersion), 0, 4);
		dirStream.Write(BitConverter.GetBytes(MinorVersion), 0, 2);
		if (!string.IsNullOrEmpty(Constants))
		{
			bytes = EncodingType.GetBytes(Constants);
			if (bytes.Length > 1015)
			{
				throw new Exception("Constants length should be less than or equal to 1015 characters");
			}
			dirStream.Write(BitConverter.GetBytes(12), 0, 2);
			dirStream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
			dirStream.Write(bytes, 0, bytes.Length);
			bytes = Encoding.Unicode.GetBytes(Constants);
			dirStream.Write(BitConverter.GetBytes(60), 0, 2);
			dirStream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
			dirStream.Write(bytes, 0, bytes.Length);
		}
	}

	internal void SerializeReferences(Stream stream)
	{
		foreach (ReferenceRecord reference in References)
		{
			reference.SerializeRecord(stream);
		}
	}

	internal void SerializeModules(Stream stream)
	{
		stream.Write(BitConverter.GetBytes(15), 0, 2);
		stream.Write(BitConverter.GetBytes(2), 0, 4);
		stream.Write(BitConverter.GetBytes(m_modules.Count), 0, 2);
		long position = stream.Position - 2;
		stream.Write(BitConverter.GetBytes(19), 0, 2);
		stream.Write(BitConverter.GetBytes(2), 0, 4);
		stream.Write(BitConverter.GetBytes(65535), 0, 2);
		int num = m_modules.Count;
		foreach (VbaModule module in m_modules)
		{
			if (module.Type == VbaModuleType.MsForm && module.DesignerStorage == null)
			{
				num--;
				long position2 = stream.Position;
				stream.Position = position;
				stream.Write(BitConverter.GetBytes(num), 0, 2);
				stream.Position = position2;
			}
			else
			{
				module.SerializeModuleRecord(stream);
			}
		}
	}

	internal void SerializeModuleStream(VbaModule module, Stream stream)
	{
		string text = "";
		foreach (VbaAttribute attribute in module.Attributes)
		{
			text = text + "Attribute " + attribute.Name + " = ";
			text = ((!attribute.IsText) ? (text + attribute.Value) : (text + "\"" + attribute.Value + "\""));
			text += "\r\n";
		}
		text += module.Code;
		byte[] bytes = EncodingType.GetBytes(text);
		bytes = (VbaDataProcess.Compress(new MemoryStream(bytes)) as MemoryStream).ToArray();
		stream.Write(bytes, 0, bytes.Length);
		stream.Flush();
	}

	internal void SerializeProjectStream(Stream stream)
	{
		string text = null;
		text = text + "ID=\"" + m_clsID + "\"\r\n";
		foreach (VbaModule module in m_modules)
		{
			if (module.Type == VbaModuleType.MsForm && module.DesignerStorage == null)
			{
				continue;
			}
			switch (module.Type)
			{
			case VbaModuleType.ClassModule:
				text = text + "Class=" + module.Name + "\r\n";
				break;
			case VbaModuleType.StdModule:
				text = text + "Module=" + module.Name + "\r\n";
				break;
			case VbaModuleType.MsForm:
				if (!string.IsNullOrEmpty(module.Package))
				{
					text = text + "Package=" + module.Package + "\r\n";
				}
				text = text + "BaseClass=" + module.Name + "\r\n";
				break;
			case VbaModuleType.Document:
				text = text + "Document=" + module.Name + "/&H00000000\r\n";
				break;
			}
		}
		if (!string.IsNullOrEmpty(HelpFile))
		{
			text = text + "HelpFile=\"" + HelpFile + "\"\r\n";
		}
		text = text + "Name=\"" + Name + "\"\r\n";
		text = text + "HelpContextID=" + HelpContextId + "\r\n";
		if (!string.IsNullOrEmpty(Description))
		{
			text = text + "Description=\"" + Description + "\"\r\n";
		}
		text += "VersionCompatible32=\"393222000\"\r\n";
		if (!string.IsNullOrEmpty(m_protectionData))
		{
			text = text + m_protectionData + "\r\n";
		}
		if (!string.IsNullOrEmpty(m_passwordData))
		{
			text = text + m_passwordData + "\r\n";
		}
		if (!string.IsNullOrEmpty(m_lockviewData))
		{
			text = text + m_lockviewData + "\r\n";
		}
		text += "\r\n";
		text += "[Host Extender Info]\r\n";
		text += "&H00000001={3832D640-CF90-11CF-8E43-00A0C911005A};VBE;&H00000000\r\n";
		text += "\r\n";
		text += "[Workspace]\r\n";
		foreach (VbaModule module2 in m_modules)
		{
			if (module2.Type != VbaModuleType.MsForm || module2.DesignerStorage != null)
			{
				text = text + module2.Name + "=0, 0, 0, 0, C \r\n";
			}
		}
		byte[] bytes = EncodingType.GetBytes(text);
		stream.Write(bytes, 0, bytes.Length);
		stream.Flush();
	}

	internal void SerializeProjectWmStream(Stream projectWm)
	{
		foreach (VbaModule module in m_modules)
		{
			if (module.Type != VbaModuleType.MsForm || module.DesignerStorage != null)
			{
				byte[] bytes = EncodingType.GetBytes(module.Name + "\0");
				projectWm.Write(bytes, 0, bytes.Length);
				bytes = Encoding.Unicode.GetBytes(module.Name + "\0");
				projectWm.Write(bytes, 0, bytes.Length);
			}
		}
		projectWm.Write(BitConverter.GetBytes(0), 0, 2);
		projectWm.Flush();
	}

	private byte[] ConvertHexString(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException(value);
		}
		if (value.Length == 0)
		{
			return new byte[0];
		}
		if (value.Length % 2 != 0)
		{
			throw new ArgumentException(value);
		}
		int num = value.Length >> 1;
		byte[] array = new byte[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = byte.Parse(value.Substring(i * 2, 2), NumberStyles.HexNumber);
		}
		return array;
	}

	private string ConvertByteArray(byte[] value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value.Length == 0)
		{
			return string.Empty;
		}
		if (value.Length % 2 != 0)
		{
			throw new ArgumentException("value");
		}
		int num = value.Length;
		string text = string.Empty;
		for (int i = 0; i < num; i++)
		{
			text += $"{value[i]:x2}";
		}
		return text.ToUpperInvariant();
	}

	public static uint ReadUInt32(Stream stream)
	{
		byte[] array = new byte[4];
		if (stream.Read(array, 0, 4) != 4)
		{
			throw new Exception("Unable to read value at the specified position - end of stream was reached.");
		}
		return BitConverter.ToUInt32(array, 0);
	}

	public static short ReadInt16(Stream stream)
	{
		byte[] array = new byte[2];
		if (stream.Read(array, 0, 2) != 2)
		{
			throw new Exception("Unable to read value at the specified position - end of stream was reached.");
		}
		return BitConverter.ToInt16(array, 0);
	}

	public static ushort ReadUInt16(Stream stream)
	{
		byte[] array = new byte[2];
		if (stream.Read(array, 0, 2) != 2)
		{
			throw new Exception("Unable to read value at the specified position - end of stream was reached.");
		}
		return BitConverter.ToUInt16(array, 0);
	}

	internal void Dispose()
	{
		if (m_modules != null)
		{
			m_modules.Dispose();
			m_modules = null;
		}
		if (m_references != null)
		{
			m_references.Dispose();
			m_references = null;
		}
	}

	internal VbaProject Clone(object parent)
	{
		VbaProject vbaProject = (VbaProject)MemberwiseClone();
		vbaProject.m_parent = parent;
		if (m_references != null)
		{
			vbaProject.m_references = m_references.Clone(vbaProject);
		}
		if (m_modules != null)
		{
			vbaProject.m_modules = m_modules.Clone(vbaProject);
		}
		return vbaProject;
	}
}

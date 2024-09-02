using System;
using System.IO;
using System.Text;
using DocGen.Compression.Zip;

namespace DocGen.Office;

internal class VbaModule : IVbaModule
{
	private string m_name;

	private string m_description;

	private string m_streamName;

	private uint m_offSet;

	private uint m_helpTopic;

	private VbaAttributesCollection m_attributeCollection;

	private object m_designerStorage;

	private string m_packages;

	private VbaModuleType m_type;

	private string m_code;

	private VbaModulesCollection m_vbaModules;

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			if (m_vbaModules[value] != null && m_vbaModules[value] != this)
			{
				throw new ArgumentException("Name is already taken");
			}
			if (m_name != value && this.CodeNameChanged != null)
			{
				this.CodeNameChanged(this, value);
			}
			m_name = value;
			m_streamName = value;
		}
	}

	public VbaModuleType Type
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
		}
	}

	public string Code
	{
		get
		{
			return m_code;
		}
		set
		{
			m_code = value;
		}
	}

	internal uint HelpTopicId
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

	internal string Description
	{
		get
		{
			if (!string.IsNullOrEmpty(m_description))
			{
				return m_description;
			}
			return string.Empty;
		}
		set
		{
			m_description = value;
		}
	}

	internal string StreamName
	{
		get
		{
			if (!string.IsNullOrEmpty(m_streamName))
			{
				return m_streamName;
			}
			return m_name;
		}
		set
		{
			m_streamName = value;
		}
	}

	internal uint ModuleOffSet
	{
		get
		{
			return m_offSet;
		}
		set
		{
			m_offSet = value;
		}
	}

	internal VbaAttributesCollection Attributes
	{
		get
		{
			if (m_attributeCollection == null)
			{
				m_attributeCollection = new VbaAttributesCollection(this);
			}
			return m_attributeCollection;
		}
	}

	public object DesignerStorage
	{
		get
		{
			if (Type == VbaModuleType.MsForm)
			{
				return m_designerStorage;
			}
			throw new Exception("Not a UserForm type");
		}
		set
		{
			if (Type == VbaModuleType.MsForm)
			{
				m_designerStorage = value;
				return;
			}
			throw new Exception("Not a UserForm type");
		}
	}

	internal string Package
	{
		get
		{
			if (Type == VbaModuleType.MsForm)
			{
				return m_packages;
			}
			throw new Exception("Not a UserForm type");
		}
		set
		{
			m_packages = value;
		}
	}

	internal event NameChangedEventHandler CodeNameChanged;

	internal VbaModule(VbaModulesCollection modules)
	{
		m_vbaModules = modules;
	}

	internal VbaAttributesCollection InitializeAttributes(string name, string clsID)
	{
		Attributes.Clear();
		Attributes.AddAttribute("VB_NAME", name, isText: true);
		Attributes.AddAttribute("VB_Base", clsID, isText: true);
		Attributes.AddAttribute("VB_GlobalNameSpace", "False", isText: false);
		Attributes.AddAttribute("VB_Creatable", "False", isText: false);
		Attributes.AddAttribute("VB_TemplateDerived", "False", isText: false);
		if (Type == VbaModuleType.MsForm)
		{
			Attributes.RemoveAt(1);
			Attributes.AddAttribute("VB_Base", "0{89F37417-10A6-4162-BDD9-9C652CCF21EB}{C4140624-8198-4E53-ACEA-C890085902FD}", isText: true);
			Attributes.AddAttribute("VB_PredeclaredId", "True", isText: false);
			Attributes.AddAttribute("VB_Exposed", "False", isText: false);
			Attributes.AddAttribute("VB_Customizable", "False", isText: false);
		}
		else if (Type == VbaModuleType.ClassModule)
		{
			Attributes.AddAttribute("VB_PredeclaredId", "False", isText: false);
			Attributes.AddAttribute("VB_Exposed", "False", isText: false);
			Attributes.AddAttribute("VB_Customizable", "False", isText: false);
		}
		else
		{
			Attributes.AddAttribute("VB_PredeclaredId", "True", isText: false);
			Attributes.AddAttribute("VB_Exposed", "True", isText: false);
			Attributes.AddAttribute("VB_Customizable", "True", isText: false);
		}
		return Attributes;
	}

	internal void ParseModuleRecord(Stream moduleStream)
	{
		moduleStream.Position += 2L;
		int num = (int)ZipArchive.ReadUInt32(moduleStream);
		byte[] array = new byte[num];
		moduleStream.Read(array, 0, num);
		Encoding encodingType = m_vbaModules.Project.EncodingType;
		Name = encodingType.GetString(array, 0, array.Length);
		if (ZipArchive.ReadUInt16(moduleStream) == 71)
		{
			num = (int)ZipArchive.ReadUInt32(moduleStream);
			array = new byte[num];
			moduleStream.Read(array, 0, num);
			Name = Encoding.Unicode.GetString(array, 0, array.Length);
		}
		moduleStream.Position += 2L;
		num = (int)ZipArchive.ReadUInt32(moduleStream);
		array = new byte[num];
		moduleStream.Position += num;
		moduleStream.Position += 2L;
		num = (int)ZipArchive.ReadUInt32(moduleStream);
		array = new byte[num];
		moduleStream.Read(array, 0, num);
		StreamName = Encoding.Unicode.GetString(array, 0, array.Length);
		moduleStream.Position += 2L;
		num = (int)ZipArchive.ReadUInt32(moduleStream);
		moduleStream.Position += num;
		moduleStream.Position += 2L;
		num = (int)ZipArchive.ReadUInt32(moduleStream);
		array = new byte[num];
		moduleStream.Read(array, 0, num);
		Description = encodingType.GetString(array, 0, array.Length);
		moduleStream.Position += 6L;
		ModuleOffSet = ZipArchive.ReadUInt32(moduleStream);
		moduleStream.Position += 6L;
		HelpTopicId = ZipArchive.ReadUInt32(moduleStream);
		moduleStream.Position += 8L;
		Type = (VbaModuleType)ZipArchive.ReadUInt16(moduleStream);
		moduleStream.Position += 4L;
		if (ZipArchive.ReadUInt16(moduleStream) == 37)
		{
			moduleStream.Position += 4L;
		}
		else
		{
			moduleStream.Position -= 2L;
		}
		if (ZipArchive.ReadUInt16(moduleStream) == 40)
		{
			moduleStream.Position += 4L;
		}
		else
		{
			moduleStream.Position -= 2L;
		}
		moduleStream.Position += 6L;
	}

	internal void SerializeModuleRecord(Stream dirStream)
	{
		dirStream.Write(BitConverter.GetBytes(25), 0, 2);
		Encoding encodingType = m_vbaModules.Project.EncodingType;
		byte[] bytes = encodingType.GetBytes(Name);
		dirStream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
		dirStream.Write(bytes, 0, bytes.Length);
		dirStream.Write(BitConverter.GetBytes(71), 0, 2);
		bytes = Encoding.Unicode.GetBytes(Name);
		dirStream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
		dirStream.Write(bytes, 0, bytes.Length);
		dirStream.Write(BitConverter.GetBytes(26), 0, 2);
		bytes = encodingType.GetBytes(StreamName);
		dirStream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
		dirStream.Write(bytes, 0, bytes.Length);
		dirStream.Write(BitConverter.GetBytes(50), 0, 2);
		bytes = Encoding.Unicode.GetBytes(StreamName);
		dirStream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
		dirStream.Write(bytes, 0, bytes.Length);
		dirStream.Write(BitConverter.GetBytes(28), 0, 2);
		bytes = encodingType.GetBytes(Description);
		dirStream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
		dirStream.Write(bytes, 0, bytes.Length);
		dirStream.Write(BitConverter.GetBytes(72), 0, 2);
		bytes = Encoding.Unicode.GetBytes(Description);
		dirStream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
		dirStream.Write(bytes, 0, bytes.Length);
		dirStream.Write(BitConverter.GetBytes(49), 0, 2);
		dirStream.Write(BitConverter.GetBytes(4), 0, 4);
		dirStream.Write(BitConverter.GetBytes(0), 0, 4);
		dirStream.Write(BitConverter.GetBytes(30), 0, 2);
		dirStream.Write(BitConverter.GetBytes(4), 0, 4);
		dirStream.Write(BitConverter.GetBytes(HelpTopicId), 0, 4);
		dirStream.Write(BitConverter.GetBytes(44), 0, 2);
		dirStream.Write(BitConverter.GetBytes(2), 0, 4);
		dirStream.Write(BitConverter.GetBytes(65535), 0, 2);
		if (Type == VbaModuleType.StdModule)
		{
			dirStream.Write(BitConverter.GetBytes(33), 0, 2);
		}
		else
		{
			dirStream.Write(BitConverter.GetBytes(34), 0, 2);
		}
		dirStream.Write(BitConverter.GetBytes(0), 0, 4);
		dirStream.Write(BitConverter.GetBytes(43), 0, 2);
		dirStream.Write(BitConverter.GetBytes(0), 0, 4);
	}

	internal void Dispose()
	{
		if (m_attributeCollection != null)
		{
			m_attributeCollection.Clear();
			m_attributeCollection = null;
		}
		if (m_designerStorage != null)
		{
			m_designerStorage = null;
		}
	}

	internal VbaModule Clone(VbaModulesCollection vbaModules)
	{
		VbaModule vbaModule = (VbaModule)MemberwiseClone();
		if (m_attributeCollection != null)
		{
			vbaModule.m_attributeCollection = m_attributeCollection.Clone(vbaModule);
		}
		if (vbaModule.Type == VbaModuleType.MsForm)
		{
			vbaModule.DesignerStorage = null;
		}
		return vbaModule;
	}
}

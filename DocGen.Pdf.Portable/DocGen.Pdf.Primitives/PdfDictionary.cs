using System;
using System.Collections.Generic;
using System.Globalization;
using DocGen.Pdf.IO;
using DocGen.Pdf.Security;

namespace DocGen.Pdf.Primitives;

internal class PdfDictionary : IPdfPrimitive, IPdfChangable
{
	private const string Prefix = "<<";

	private const string Suffix = ">>";

	private static object s_syncLock = new object();

	private Dictionary<PdfName, IPdfPrimitive> m_items;

	private bool m_archive = true;

	private bool m_encrypt;

	private bool m_isDecrypted;

	private bool m_bChanged;

	private ObjectStatus m_status;

	private bool m_isSaving;

	private int m_index;

	private int m_position = -1;

	private PdfCrossTable m_crossTable;

	internal PdfDictionary m_clonedObject;

	internal bool isXfa;

	internal bool isSkip;

	private bool isFont;

	public IPdfPrimitive this[PdfName key]
	{
		get
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (m_items.ContainsKey(key))
			{
				return m_items[key];
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			m_items[key] = value;
			Modify();
		}
	}

	public IPdfPrimitive this[string key]
	{
		get
		{
			if (key == null || key == string.Empty)
			{
				throw new ArgumentNullException("key");
			}
			return this[new PdfName(key)];
		}
		set
		{
			if (key == null || key == string.Empty)
			{
				throw new ArgumentNullException("key");
			}
			PdfName name = GetName(key);
			this[name] = value;
			Modify();
		}
	}

	public int Count => m_items.Count;

	public Dictionary<PdfName, IPdfPrimitive>.ValueCollection Values => m_items.Values;

	internal bool Archive
	{
		get
		{
			return m_archive;
		}
		set
		{
			m_archive = value;
		}
	}

	internal bool Encrypt
	{
		get
		{
			return m_encrypt;
		}
		set
		{
			m_encrypt = value;
			Modify();
		}
	}

	internal bool IsDecrypted
	{
		get
		{
			return m_isDecrypted;
		}
		set
		{
			m_isDecrypted = value;
		}
	}

	internal Dictionary<PdfName, IPdfPrimitive>.KeyCollection Keys => m_items.Keys;

	internal Dictionary<PdfName, IPdfPrimitive> Items => m_items;

	public ObjectStatus Status
	{
		get
		{
			return m_status;
		}
		set
		{
			m_status = value;
		}
	}

	public bool IsSaving
	{
		get
		{
			return m_isSaving;
		}
		set
		{
			m_isSaving = value;
		}
	}

	public int ObjectCollectionIndex
	{
		get
		{
			return m_index;
		}
		set
		{
			m_index = value;
		}
	}

	public int Position
	{
		get
		{
			return m_position;
		}
		set
		{
			m_position = value;
		}
	}

	internal PdfCrossTable CrossTable => m_crossTable;

	public virtual IPdfPrimitive ClonedObject => m_clonedObject;

	internal bool IsFont
	{
		get
		{
			return isFont;
		}
		set
		{
			isFont = value;
		}
	}

	public bool Changed
	{
		get
		{
			if (!m_bChanged)
			{
				m_bChanged = CheckChanges();
			}
			return m_bChanged;
		}
	}

	internal event SavePdfPrimitiveEventHandler BeginSave;

	internal event SavePdfPrimitiveEventHandler EndSave;

	internal PdfDictionary()
	{
		m_items = new Dictionary<PdfName, IPdfPrimitive>();
		m_encrypt = true;
	}

	internal PdfDictionary(PdfDictionary dictionary)
	{
		if (dictionary == null)
		{
			throw new ArgumentNullException("dictionary");
		}
		m_items = new Dictionary<PdfName, IPdfPrimitive>();
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in dictionary.m_items)
		{
			PdfName key = item.Key;
			IPdfPrimitive value = item.Value;
			m_items[key] = value;
		}
		Status = dictionary.Status;
		FreezeChanges(this);
		m_encrypt = true;
	}

	public bool ContainsKey(string key)
	{
		return ContainsKey(new PdfName(key));
	}

	public bool ContainsKey(PdfName key)
	{
		return m_items.ContainsKey(key);
	}

	public void Remove(PdfName key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		m_items.Remove(key);
		Modify();
	}

	public void Remove(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		Remove(new PdfName(key));
	}

	internal void Clear()
	{
		m_items.Clear();
		Modify();
	}

	public virtual IPdfPrimitive Clone(PdfCrossTable crossTable)
	{
		if (!(this is PdfStream))
		{
			if (m_clonedObject != null && m_clonedObject.CrossTable == crossTable && !IsFont)
			{
				return m_clonedObject;
			}
			m_clonedObject = null;
		}
		PdfDictionary pdfDictionary = new PdfDictionary();
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in m_items)
		{
			PdfName key = item.Key;
			IPdfPrimitive pdfPrimitive = item.Value.Clone(crossTable);
			if (!(pdfPrimitive is PdfNull))
			{
				pdfDictionary[key] = pdfPrimitive;
			}
		}
		pdfDictionary.Archive = m_archive;
		pdfDictionary.IsDecrypted = m_isDecrypted;
		pdfDictionary.Status = m_status;
		pdfDictionary.Encrypt = m_encrypt;
		pdfDictionary.FreezeChanges(this);
		pdfDictionary.m_crossTable = crossTable;
		if (!(this is PdfStream))
		{
			m_clonedObject = pdfDictionary;
		}
		return pdfDictionary;
	}

	public IPdfPrimitive GetValue(PdfCrossTable crossTable, string key, string parentKey)
	{
		IPdfPrimitive pdfPrimitive = null;
		PdfDictionary pdfDictionary = this;
		pdfPrimitive = PdfCrossTable.Dereference(pdfDictionary[key]);
		while (pdfPrimitive == null && pdfDictionary != null)
		{
			pdfDictionary = PdfCrossTable.Dereference(pdfDictionary[parentKey]) as PdfDictionary;
			if (pdfDictionary != null)
			{
				pdfPrimitive = PdfCrossTable.Dereference(pdfDictionary[key]);
			}
		}
		return pdfPrimitive;
	}

	public IPdfPrimitive GetValue(string key, string parentKey)
	{
		IPdfPrimitive pdfPrimitive = null;
		PdfDictionary pdfDictionary = this;
		for (pdfPrimitive = PdfCrossTable.Dereference(pdfDictionary[key]); pdfPrimitive == null; pdfPrimitive = PdfCrossTable.Dereference(pdfDictionary[key]))
		{
			pdfDictionary = PdfCrossTable.Dereference(pdfDictionary[parentKey]) as PdfDictionary;
			if (pdfDictionary == null)
			{
				break;
			}
		}
		return pdfPrimitive;
	}

	internal PdfString GetString(string propertyName)
	{
		return PdfCrossTable.Dereference(this[propertyName]) as PdfString;
	}

	internal int GetInt(string propertyName)
	{
		PdfNumber pdfNumber = PdfCrossTable.Dereference(this[propertyName]) as PdfNumber;
		int result = 0;
		if (pdfNumber != null)
		{
			result = pdfNumber.IntValue;
		}
		return result;
	}

	internal virtual void SaveItems(IPdfWriter writer)
	{
		lock (s_syncLock)
		{
			if (writer.Document != null && writer.Document is PdfDocument && writer.Document.FileStructure.Version == PdfVersion.Version2_0 && ContainsKey("ProcSet"))
			{
				Remove("ProcSet");
			}
			bool flag = false;
			if (writer is PdfWriter && (writer as PdfWriter).isCompress)
			{
				flag = true;
			}
			else
			{
				writer.Write("\r\n");
			}
			if (writer.Document != null && writer.Document.m_isImported && ContainsKey("Type"))
			{
				PdfName pdfName = this["Type"] as PdfName;
				if (pdfName != null && pdfName.Value == "SigRef")
				{
					Remove(new PdfName("Data"));
				}
			}
			foreach (KeyValuePair<PdfName, IPdfPrimitive> item2 in m_items)
			{
				if (item2.Value == null)
				{
					continue;
				}
				PdfName key = item2.Key;
				key.Save(writer);
				if (!flag || !(item2.Value is PdfName))
				{
					writer.Write(" ");
				}
				IPdfPrimitive pdfPrimitive = item2.Value;
				if (key.Value == "Fields")
				{
					PdfArray pdfArray = pdfPrimitive as PdfArray;
					List<PdfReferenceHolder> list = new List<PdfReferenceHolder>();
					if (pdfArray != null)
					{
						for (int i = 0; i < pdfArray.Count; i++)
						{
							PdfReferenceHolder item = pdfArray.Elements[i] as PdfReferenceHolder;
							list.Add(item);
						}
						for (int j = 0; j < pdfArray.Count; j++)
						{
							PdfReferenceHolder pdfReferenceHolder = pdfArray.Elements[j] as PdfReferenceHolder;
							if (!(pdfReferenceHolder != null) || !(pdfReferenceHolder.Object is PdfDictionary pdfDictionary))
							{
								continue;
							}
							PdfName key2 = new PdfName("Kids");
							if (pdfDictionary.BeginSave != null)
							{
								SavePdfPrimitiveEventArgs ars = new SavePdfPrimitiveEventArgs(writer);
								pdfDictionary.BeginSave(pdfDictionary, ars);
							}
							if (pdfDictionary.ContainsKey(key2) || pdfDictionary.Items == null || !pdfDictionary.Items.ContainsKey(new PdfName("FT")))
							{
								continue;
							}
							PdfName pdfName2 = pdfDictionary.Items[new PdfName("FT")] as PdfName;
							if (!(pdfName2 != null) || !(pdfName2.ToString() == "/Sig"))
							{
								continue;
							}
							for (int k = 0; k < pdfArray.Count; k++)
							{
								if (k != j && PdfCrossTable.Dereference(pdfArray.Elements[k]) is PdfDictionary { Items: not null } pdfDictionary2 && pdfDictionary2.Items.ContainsKey(new PdfName("T")) && pdfDictionary.Items.ContainsKey(new PdfName("T")))
								{
									PdfString obj = pdfDictionary2.Items[new PdfName("T")] as PdfString;
									PdfString pdfString = pdfDictionary.Items[new PdfName("T")] as PdfString;
									if (obj.Value == pdfString.Value)
									{
										pdfArray.Remove(pdfReferenceHolder);
									}
								}
							}
						}
						pdfPrimitive = pdfArray;
					}
				}
				pdfPrimitive.Save(writer);
				if (!flag)
				{
					writer.Write("\r\n");
				}
			}
		}
	}

	protected internal PdfName GetName(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return new PdfName(name);
	}

	protected virtual void OnBeginSave(SavePdfPrimitiveEventArgs args)
	{
		lock (s_syncLock)
		{
			if (this.BeginSave != null)
			{
				this.BeginSave(this, args);
			}
		}
	}

	protected virtual void OnEndSave(SavePdfPrimitiveEventArgs args)
	{
		lock (s_syncLock)
		{
			if (this.EndSave != null)
			{
				this.EndSave(this, args);
			}
		}
	}

	public virtual void Save(IPdfWriter writer)
	{
		lock (new object())
		{
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			Save(writer, bRaiseEvent: true);
		}
	}

	internal void Save(IPdfWriter writer, bool bRaiseEvent)
	{
		writer.Write("<<");
		long num = 0L;
		if (writer.Document.m_isStreamCopied)
		{
			num = writer.Position;
		}
		if (bRaiseEvent)
		{
			SavePdfPrimitiveEventArgs args = new SavePdfPrimitiveEventArgs(writer);
			OnBeginSave(args);
		}
		if (writer.Document.m_isStreamCopied && num != writer.Position && num > writer.Position)
		{
			writer.Position = num;
		}
		if (Count > 0)
		{
			PdfSecurity security = writer.Document.Security;
			bool enabled = security.Enabled;
			if (!m_encrypt)
			{
				security.Enabled = false;
			}
			SaveItems(writer);
			if (!m_encrypt)
			{
				security.Enabled = enabled;
			}
		}
		writer.Write(">>");
		if (!(writer is PdfWriter) || !(writer as PdfWriter).isCompress)
		{
			writer.Write("\r\n");
		}
		if (bRaiseEvent)
		{
			SavePdfPrimitiveEventArgs args2 = new SavePdfPrimitiveEventArgs(writer);
			OnEndSave(args2);
		}
	}

	internal void SetProperty(string key, IPdfPrimitive primitive)
	{
		if (primitive == null)
		{
			m_items.Remove(new PdfName(key));
		}
		else
		{
			this[key] = primitive;
		}
	}

	internal void SetProperty(PdfName key, IPdfPrimitive primitive)
	{
		if (primitive == null)
		{
			m_items.Remove(key);
		}
		else
		{
			this[key] = primitive;
		}
	}

	internal void SetProperty(string key, IPdfWrapper wrapper)
	{
		if (wrapper == null)
		{
			m_items.Remove(new PdfName(key));
		}
		else
		{
			SetProperty(key, wrapper.Element);
		}
	}

	internal static void SetProperty(PdfDictionary dictionary, string key, IPdfWrapper wrapper)
	{
		if (wrapper == null)
		{
			dictionary.Remove(new PdfName(key));
		}
		else
		{
			SetProperty(dictionary, key, wrapper.Element);
		}
	}

	internal static void SetProperty(PdfDictionary dictionary, string key, IPdfPrimitive primitive)
	{
		if (primitive == null)
		{
			dictionary.Remove(new PdfName(key));
		}
		else
		{
			dictionary[key] = primitive;
		}
	}

	internal void SetBoolean(string key, bool value)
	{
		if (this[key] is PdfBoolean pdfBoolean)
		{
			pdfBoolean.Value = value;
			Modify();
		}
		else
		{
			this[key] = new PdfBoolean(value);
		}
	}

	internal void SetNumber(string key, int value)
	{
		if (this[key] is PdfNumber pdfNumber)
		{
			pdfNumber.IntValue = value;
			Modify();
		}
		else
		{
			this[key] = new PdfNumber(value);
		}
	}

	internal void SetNumber(string key, float value)
	{
		if (this[key] is PdfNumber)
		{
			this[key] = new PdfNumber(value);
			Modify();
		}
		else
		{
			this[key] = new PdfNumber(value);
		}
	}

	internal void SetArray(string key, params IPdfPrimitive[] list)
	{
		PdfArray pdfArray = this[key] as PdfArray;
		if (pdfArray == null)
		{
			pdfArray = (PdfArray)(this[key] = new PdfArray());
		}
		else
		{
			pdfArray.Clear();
			Modify();
		}
		foreach (IPdfPrimitive element in list)
		{
			pdfArray.Add(element);
		}
	}

	internal void SetDateTime(string key, DateTime dateTime)
	{
		if (this[key] is PdfString pdfString)
		{
			int minutes = new DateTimeOffset(dateTime).Offset.Minutes;
			string text = minutes.ToString();
			if (minutes >= 0 && minutes <= 9)
			{
				text = "0" + text;
			}
			int hours = new DateTimeOffset(dateTime).Offset.Hours;
			string text2 = hours.ToString();
			if (hours >= 0 && hours <= 9)
			{
				text2 = "0" + text2;
			}
			pdfString.Value = PdfString.FromDate(dateTime) + "+" + text2 + "'" + text + "'";
			Modify();
			return;
		}
		int minutes2 = new DateTimeOffset(dateTime).Offset.Minutes;
		string text3 = minutes2.ToString();
		if (minutes2 >= 0 && minutes2 <= 9)
		{
			text3 = "0" + text3;
		}
		int hours2 = new DateTimeOffset(dateTime).Offset.Hours;
		string text4 = hours2.ToString();
		if (hours2 >= 0 && hours2 <= 9)
		{
			text4 = "0" + text4;
		}
		if (hours2 < 0)
		{
			if (text4.Length == 2)
			{
				text4 = (-hours2).ToString();
				text4 = "-0" + text4;
			}
			this[key] = new PdfString(PdfString.FromDate(dateTime) + text4 + "'" + text3 + "'");
		}
		else
		{
			this[key] = new PdfString(PdfString.FromDate(dateTime) + "+" + text4 + "'" + text3 + "'");
		}
	}

	internal DateTime GetDateTime(PdfString dateTimeStringValue)
	{
		if (dateTimeStringValue == null)
		{
			throw new ArgumentNullException("dateTimeString");
		}
		string text = "D:";
		PdfString pdfString = new PdfString(dateTimeStringValue.Value);
		pdfString.Value = pdfString.Value.Trim('(', ')', 'D', ':');
		if (pdfString.Value.StartsWith("191"))
		{
			pdfString.Value = pdfString.Value.Remove(0, 3).Insert(0, "20");
		}
		if (pdfString.Value.Contains("ColorFound") && pdfString.Value.IndexOf("ColorFound") == 0)
		{
			pdfString.Value = pdfString.Value.Remove(0, 10);
		}
		bool flag = pdfString.Value.Contains(text);
		string text2 = "yyyyMMddHHmmss";
		if (pdfString.Value.Contains("/"))
		{
			string[] array = pdfString.Value.Split('/');
			if (array.Length > 2)
			{
				if (array[0].Length <= 2)
				{
					DateTime result = DateTime.Now;
					try
					{
						DateTimeFormatInfo dateTimeFormatInfo = new DateTimeFormatInfo();
						dateTimeFormatInfo.ShortDatePattern = "MM/dd/yyyy HH:MM TT";
						result = Convert.ToDateTime(pdfString.Value, dateTimeFormatInfo);
						pdfString.Value = result.ToString("yyyyMMddHHmmss");
					}
					catch
					{
						DateTime.TryParse(pdfString.Value, out result);
						pdfString.Value = result.ToString("yyyyMMddHHmmss");
					}
				}
				else
				{
					pdfString.Value = Convert.ToDateTime(pdfString.Value, CultureInfo.InvariantCulture).ToString("yyyyMMddHHmmss");
				}
			}
		}
		if (pdfString.Value.Length <= 8)
		{
			text2 = "yyyyMMdd";
		}
		else if (pdfString.Value.Length <= 10)
		{
			text2 = "yyyyMMddHH";
		}
		else if (pdfString.Value.Length <= 12)
		{
			text2 = "yyyyMMddHHmm";
		}
		string text3 = string.Empty.PadRight(text2.Length);
		if (pdfString.Value.Length == 0)
		{
			return DateTime.Now;
		}
		if (pdfString.Value.Length >= text3.Length)
		{
			text3 = (flag ? pdfString.Value.Substring(text.Length, text3.Length) : pdfString.Value.Substring(0, text3.Length));
		}
		DateTime result2 = DateTime.Now;
		DateTime.TryParseExact(text3, text2, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowLeadingWhite, out result2);
		if (result2 == default(DateTime))
		{
			return DateTime.Now;
		}
		return result2;
	}

	internal void SetString(string key, string str)
	{
		if (this[key] is PdfString pdfString)
		{
			pdfString.Value = str;
			Modify();
		}
		else
		{
			this[key] = new PdfString(str);
		}
	}

	internal static void SetName(PdfDictionary dictionary, string key, string name)
	{
		PdfName pdfName = dictionary[key] as PdfName;
		if (pdfName != null)
		{
			pdfName.Value = name;
			dictionary.Modify();
		}
		else
		{
			dictionary[key] = new PdfName(name);
		}
	}

	internal void SetName(string key, string name)
	{
		PdfName pdfName = this[key] as PdfName;
		if (pdfName != null)
		{
			pdfName.Value = name;
			Modify();
		}
		else
		{
			this[key] = new PdfName(name);
		}
	}

	internal void SetName(string key, string name, bool processSpecialCharacters)
	{
		PdfName pdfName = this[key] as PdfName;
		string value = name.Replace("#", "#23").Replace(" ", "#20").Replace("/", "#2F");
		if (pdfName != null)
		{
			pdfName.Value = value;
			Modify();
		}
		else
		{
			this[key] = new PdfName(value);
		}
	}

	private bool CheckChanges()
	{
		bool result = false;
		foreach (IPdfPrimitive value in Values)
		{
			if (value is IPdfChangable { Changed: not false })
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public void FreezeChanges(object freezer)
	{
		if (freezer is PdfParser || freezer is PdfDictionary)
		{
			m_bChanged = false;
		}
	}

	internal void Modify()
	{
		m_bChanged = true;
	}
}

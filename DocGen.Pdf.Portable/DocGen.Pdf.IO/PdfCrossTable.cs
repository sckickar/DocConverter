using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using DocGen.Pdf.Interactive;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Security;

namespace DocGen.Pdf.IO;

internal class PdfCrossTable : IDisposable
{
	public class RegisteredObject
	{
		private long m_objectNumber;

		public int GenerationNumber;

		private long m_offset;

		private PdfArchiveStream m_archive;

		public ObjectType Type;

		private PdfCrossTable m_xrefTable;

		internal long ObjectNumber
		{
			get
			{
				if (m_objectNumber == 0L && m_archive != null)
				{
					m_objectNumber = m_xrefTable.GetReference(m_archive).ObjNum;
				}
				return m_objectNumber;
			}
		}

		internal long Offset
		{
			get
			{
				if (m_archive != null)
				{
					return m_archive.GetIndex(m_offset);
				}
				return m_offset;
			}
		}

		public RegisteredObject(long offset, PdfReference reference)
		{
			if (reference == null)
			{
				throw new ArgumentNullException("reference");
			}
			m_offset = offset;
			GenerationNumber = reference.GenNum;
			m_objectNumber = reference.ObjNum;
			Type = ObjectType.Normal;
		}

		public RegisteredObject(long offset, PdfReference reference, bool free)
			: this(offset, reference)
		{
			if (reference == null)
			{
				throw new ArgumentNullException("reference");
			}
			Type = ((!free) ? ObjectType.Normal : ObjectType.Free);
		}

		public RegisteredObject(PdfCrossTable xrefTable, PdfArchiveStream archive, PdfReference reference)
		{
			m_xrefTable = xrefTable;
			m_archive = archive;
			m_offset = reference.ObjNum;
			Type = ObjectType.Packed;
		}
	}

	internal class ArchiveInfo
	{
		public PdfReference Reference;

		public PdfArchiveStream Archive;

		public ArchiveInfo(PdfReference reference, PdfArchiveStream archive)
		{
			Reference = reference;
			Archive = archive;
		}
	}

	private CrossTable m_crossTable;

	private PdfDictionary m_documentCatalog;

	private Stream m_stream;

	private Dictionary<long, RegisteredObject> m_objects = new Dictionary<long, RegisteredObject>();

	private int m_count;

	private bool m_bDisposed;

	private IPdfPrimitive m_trailer;

	private PdfDocumentBase m_document;

	private bool m_bForceNew;

	private Stack<PdfReference> m_objNumbers = new Stack<PdfReference>();

	private long m_maxGenNumIndex;

	private PdfArchiveStream m_archive;

	private Dictionary<PdfReference, PdfReference> m_mappedReferences;

	private int m_storedCount;

	private List<ArchiveInfo> m_archives;

	private PdfDictionary m_encryptorDictionary;

	private PdfMainObjectCollection m_items;

	private bool m_bEncrypt;

	private Dictionary<IPdfPrimitive, object> m_pageCorrespondance;

	private List<PdfReference> m_preReference;

	private bool m_isMerging;

	private bool m_isColorSpace;

	private bool m_isOutlineOrDest;

	internal bool isOpenAndRepair;

	internal bool isRepair;

	private bool isIndexGreaterthanTotalObjectCount;

	internal bool isCompletely;

	internal bool isDisposed;

	internal PdfLoadedDocument loadedPdfDocument;

	internal PdfDictionary m_pdfDocumentEncoding;

	internal bool m_closeCompletely;

	internal List<long> m_pdfObjects;

	private bool m_isPDFAppend;

	private bool m_isTagged;

	private Dictionary<long, long> m_preReferenceTable;

	internal bool isTemplateMerging;

	internal bool Encrypted
	{
		get
		{
			return m_bEncrypt;
		}
		set
		{
			m_bEncrypt = value;
		}
	}

	internal bool IsTagged
	{
		get
		{
			return m_isTagged;
		}
		set
		{
			m_isTagged = value;
		}
	}

	public PdfDictionary DocumentCatalog
	{
		get
		{
			if (m_documentCatalog == null && m_crossTable != null)
			{
				m_documentCatalog = Dereference(m_crossTable.DocumentCatalog) as PdfDictionary;
			}
			return m_documentCatalog;
		}
	}

	internal Stream Stream => m_crossTable.Stream;

	internal int NextObjNumber
	{
		get
		{
			if (Count == 0)
			{
				Count++;
			}
			return Count++;
		}
	}

	internal CrossTable CrossTable => m_crossTable;

	internal int Count
	{
		get
		{
			if (m_count == 0)
			{
				IPdfPrimitive pdfPrimitive = null;
				if (m_crossTable != null)
				{
					pdfPrimitive = m_crossTable.Trailer["Size"];
				}
				PdfNumber pdfNumber = ((pdfPrimitive == null) ? new PdfNumber(0) : (Dereference(pdfPrimitive) as PdfNumber));
				m_count = pdfNumber.IntValue;
			}
			return m_count;
		}
		set
		{
			if (value == 0)
			{
				throw new ArgumentException("The value can't be 0.", "Count");
			}
			m_count = value;
		}
	}

	internal PdfDocumentBase Document
	{
		get
		{
			return m_document;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Document");
			}
			m_document = value;
			m_items = m_document.PdfObjects;
		}
	}

	internal PdfMainObjectCollection PdfObjects => m_items;

	internal PdfDictionary Trailer
	{
		get
		{
			if (m_trailer == null)
			{
				m_trailer = ((m_crossTable == null) ? new PdfStream() : m_crossTable.Trailer);
			}
			if ((m_trailer as PdfDictionary).ContainsKey("XRefStm"))
			{
				(m_trailer as PdfDictionary).Remove(new PdfName("XRefStm"));
			}
			return m_trailer as PdfDictionary;
		}
	}

	internal bool IsMerging
	{
		get
		{
			return m_isMerging;
		}
		set
		{
			m_isMerging = value;
		}
	}

	internal PdfEncryptor Encryptor
	{
		get
		{
			if (m_crossTable != null)
			{
				return m_crossTable.Encryptor;
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Encryptor");
			}
			m_crossTable.Encryptor = value.Clone();
		}
	}

	private PdfMainObjectCollection ObjectCollection => m_document.PdfObjects;

	internal PdfDictionary EncryptorDictionary
	{
		get
		{
			if (m_encryptorDictionary == null)
			{
				m_bEncrypt = true;
				m_encryptorDictionary = Dereference(Trailer["Encrypt"]) as PdfDictionary;
			}
			m_bEncrypt = false;
			return m_encryptorDictionary;
		}
	}

	internal Dictionary<IPdfPrimitive, object> PageCorrespondance
	{
		get
		{
			if (m_pageCorrespondance == null)
			{
				m_pageCorrespondance = new Dictionary<IPdfPrimitive, object>();
			}
			return m_pageCorrespondance;
		}
		set
		{
			m_pageCorrespondance = value;
		}
	}

	internal List<PdfReference> PrevReference
	{
		get
		{
			if (m_preReference == null)
			{
				m_preReference = new List<PdfReference>();
			}
			return m_preReference;
		}
		set
		{
			m_preReference = value;
		}
	}

	internal Dictionary<long, long> PrevCloneReference
	{
		get
		{
			if (m_preReferenceTable == null)
			{
				m_preReferenceTable = new Dictionary<long, long>();
			}
			return m_preReferenceTable;
		}
		set
		{
			m_preReferenceTable = value;
		}
	}

	internal bool StructureAltered => m_crossTable.IsStructureAltered;

	internal bool IsPDFAppend
	{
		get
		{
			return m_isPDFAppend;
		}
		set
		{
			m_isPDFAppend = value;
		}
	}

	public PdfCrossTable(Stream docStream)
	{
		if (docStream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_stream = docStream;
		m_crossTable = new CrossTable(docStream, this);
	}

	public PdfCrossTable(Stream docStream, bool openAndRepair)
	{
		if (docStream == null)
		{
			throw new ArgumentNullException("stream");
		}
		isOpenAndRepair = openAndRepair;
		m_stream = docStream;
		m_crossTable = new CrossTable(docStream, this);
	}

	internal PdfCrossTable(Stream docStream, bool openAndRepair, bool repair)
	{
		if (docStream == null)
		{
			throw new ArgumentNullException("stream");
		}
		isOpenAndRepair = openAndRepair;
		isRepair = repair;
		m_stream = docStream;
		m_crossTable = new CrossTable(docStream, this);
	}

	public PdfCrossTable()
	{
	}

	internal PdfCrossTable(int count, PdfDictionary encryptionDictionary, PdfDictionary documentCatalog)
		: this()
	{
		m_storedCount = count;
		m_bForceNew = true;
		m_encryptorDictionary = encryptionDictionary;
		m_documentCatalog = documentCatalog;
	}

	internal PdfCrossTable(int count, PdfDictionary encryptionDictionary, PdfDictionary documentCatalog, CrossTable cTable)
		: this()
	{
		m_storedCount = count;
		m_bForceNew = true;
		m_encryptorDictionary = encryptionDictionary;
		m_documentCatalog = documentCatalog;
		m_crossTable = cTable;
	}

	internal PdfCrossTable(bool isFdf, Stream docStream)
	{
		if (docStream == null)
		{
			throw new ArgumentNullException("stream");
		}
		m_stream = docStream;
		m_crossTable = new CrossTable(docStream, this, isFdf);
	}

	~PdfCrossTable()
	{
		Dispose(completely: false);
	}

	internal PdfCrossTable(Stream docStream, PdfLoadedDocument ldoc)
	{
		if (docStream == null)
		{
			throw new ArgumentNullException("stream");
		}
		loadedPdfDocument = ldoc;
		m_stream = docStream;
		m_crossTable = new CrossTable(docStream, this);
	}

	public static IPdfPrimitive Dereference(IPdfPrimitive obj)
	{
		PdfReferenceHolder pdfReferenceHolder = obj as PdfReferenceHolder;
		if (pdfReferenceHolder != null && pdfReferenceHolder.Object != null)
		{
			obj = pdfReferenceHolder.Object;
		}
		return obj;
	}

	public IPdfPrimitive GetObject(IPdfPrimitive pointer)
	{
		IPdfPrimitive pdfPrimitive = pointer;
		if (pointer is PdfReferenceHolder)
		{
			pdfPrimitive = (pointer as PdfReferenceHolder).Object;
		}
		else if (pointer is PdfReference)
		{
			PdfReference pdfReference = pointer as PdfReference;
			if (pdfReference != null)
			{
				m_objNumbers.Push(pdfReference);
			}
			IPdfPrimitive pdfPrimitive2 = null;
			if (m_crossTable != null)
			{
				pdfPrimitive2 = m_crossTable.GetObject(pointer);
			}
			else if (PdfObjects.GetObjectIndex(pdfReference) == 0)
			{
				pdfPrimitive2 = PdfObjects.GetObject(pdfReference);
			}
			if (pdfReference.IsDisposed && pdfPrimitive2 != null)
			{
				return pdfPrimitive2;
			}
			pdfPrimitive2 = PageProceed(pdfPrimitive2);
			PdfMainObjectCollection pdfObjects = PdfObjects;
			if (pdfPrimitive2 != null)
			{
				if (pdfObjects.ContainsReference(pdfReference))
				{
					pdfObjects.GetObjectIndex(pdfReference);
					pdfPrimitive2 = pdfObjects.GetObject(pdfReference);
				}
				else
				{
					pdfObjects.Add(pdfPrimitive2, pdfReference);
					if (!m_isMerging)
					{
						pdfPrimitive2.Position = -1;
						pdfReference.Position = -1;
					}
				}
			}
			pdfPrimitive = pdfPrimitive2;
		}
		if (pdfPrimitive != null && Document != null && Document.WasEncrypted)
		{
			ObjectDecyptionProcess(pdfPrimitive);
		}
		if (pointer is PdfReference && m_objNumbers.Count > 0)
		{
			m_objNumbers.Pop();
		}
		return pdfPrimitive;
	}

	private void Decrypt(IPdfDecryptable obj)
	{
		if (Document.WasEncrypted && obj != null && !obj.Decrypted && m_objNumbers.Count > 0 && Encryptor != null && !Encryptor.EncryptOnlyAttachment)
		{
			PdfEncryptor encryptor = Encryptor;
			long objNum = m_objNumbers.Peek().ObjNum;
			obj.Decrypt(encryptor, objNum);
		}
	}

	private void Decrypt(IPdfPrimitive obj)
	{
		PdfDictionary pdfDictionary = obj as PdfDictionary;
		PdfArray pdfArray = obj as PdfArray;
		if (pdfDictionary != null && !pdfDictionary.IsDecrypted)
		{
			m_isOutlineOrDest = CheckForOutlinesAndDestination(pdfDictionary);
			foreach (IPdfPrimitive value in pdfDictionary.Values)
			{
				Decrypt(value);
			}
			Decrypt(pdfDictionary as IPdfDecryptable);
			m_isOutlineOrDest = false;
		}
		else if (pdfArray != null)
		{
			foreach (IPdfPrimitive item in pdfArray)
			{
				PdfName pdfName = item as PdfName;
				if (pdfName != null && pdfName.Value.Equals("Indexed"))
				{
					m_isColorSpace = true;
				}
				Decrypt(item);
			}
			m_isColorSpace = false;
		}
		else if (obj is PdfString)
		{
			PdfString pdfString = obj as PdfString;
			if (!pdfString.Decrypted && (!pdfString.Hex || m_isColorSpace || m_isOutlineOrDest) && !pdfString.IsPacked)
			{
				Decrypt(obj as IPdfDecryptable);
			}
		}
		else
		{
			Decrypt(obj as IPdfDecryptable);
		}
	}

	private bool CheckForOutlinesAndDestination(PdfDictionary dictionary)
	{
		if (dictionary.ContainsKey("Parent"))
		{
			if (Dereference(dictionary["Parent"]) is PdfDictionary pdfDictionary && DocumentCatalog != null && DocumentCatalog.ContainsKey("Outlines"))
			{
				if (!(Dereference(DocumentCatalog["Outlines"]) is PdfDictionary pdfDictionary2) || pdfDictionary2 != pdfDictionary)
				{
					return CheckForOutlinesAndDestination(pdfDictionary);
				}
				return true;
			}
		}
		else if (dictionary.ContainsKey("Limits"))
		{
			return true;
		}
		return false;
	}

	public byte[] GetStream(IPdfPrimitive streamRef)
	{
		if (streamRef == null)
		{
			throw new ArgumentNullException("streamRef");
		}
		return m_crossTable.GetStream(streamRef);
	}

	public void RegisterObject(long offset, PdfReference reference)
	{
		if (reference == null)
		{
			throw new ArgumentNullException("reference");
		}
		m_objects[reference.ObjNum] = new RegisteredObject(offset, reference);
		m_maxGenNumIndex = Math.Max(m_maxGenNumIndex, reference.GenNum);
	}

	public void RegisterObject(PdfArchiveStream archive, PdfReference reference)
	{
		m_objects[reference.ObjNum] = new RegisteredObject(this, archive, reference);
		m_maxGenNumIndex = Math.Max(m_maxGenNumIndex, archive.Count);
	}

	public void RegisterObject(long offset, PdfReference reference, bool free)
	{
		if (reference == null)
		{
			throw new ArgumentNullException("reference");
		}
		m_objects[reference.ObjNum] = new RegisteredObject(offset, reference, free);
		m_maxGenNumIndex = Math.Max(m_maxGenNumIndex, reference.GenNum);
	}

	public void Save(PdfWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (m_documentCatalog != null && m_documentCatalog.ContainsKey("Perms") && Document.FileStructure.IncrementalUpdate)
		{
			PdfDictionary pdfDictionary = null;
			if (m_documentCatalog["Perms"] is PdfDictionary)
			{
				pdfDictionary = m_documentCatalog["Perms"] as PdfDictionary;
			}
			else if (m_documentCatalog["Perms"] is PdfReferenceHolder)
			{
				pdfDictionary = (m_documentCatalog["Perms"] as PdfReferenceHolder).Object as PdfDictionary;
			}
			if (!pdfDictionary.ContainsKey("UR3"))
			{
				SaveHead(writer);
			}
		}
		else
		{
			SaveHead(writer);
		}
		bool enabled = false;
		PdfSecurity security = m_document.Security;
		m_mappedReferences = null;
		if (m_archives != null)
		{
			m_archives.Clear();
		}
		m_archive = null;
		if (m_objects != null)
		{
			m_objects.Clear();
		}
		MarkTrailerReferences();
		if (Document != null && Document is PdfDocument && Document.FileStructure.Version == PdfVersion.Version2_0)
		{
			if (security != null && security.KeySize == PdfEncryptionKeySize.Key256Bit)
			{
				security.KeySize = PdfEncryptionKeySize.Key256BitRevision6;
			}
			PdfCatalog catalog = Document.Catalog;
			if (catalog != null && catalog.ContainsKey("AcroForm"))
			{
				IPdfPrimitive pdfPrimitive = catalog["AcroForm"];
				if (((pdfPrimitive is PdfReferenceHolder) ? (pdfPrimitive as PdfReferenceHolder).Object : pdfPrimitive) is PdfDictionary pdfDictionary2 && pdfDictionary2.ContainsKey("NeedAppearances"))
				{
					pdfDictionary2.Remove("NeedAppearances");
					pdfDictionary2.Modify();
				}
				PdfForm form = (Document as PdfDocument).Form;
				if (form != null)
				{
					form.SetAppearanceDictionary = false;
				}
			}
			if (Trailer != null && Trailer.ContainsKey("Info"))
			{
				IPdfPrimitive pdfPrimitive2 = Trailer["Info"];
				if (((pdfPrimitive2 is PdfReferenceHolder) ? (pdfPrimitive2 as PdfReferenceHolder).Object : pdfPrimitive2) is PdfDictionary pdfDictionary3)
				{
					if (pdfDictionary3.ContainsKey("Title"))
					{
						pdfDictionary3.Remove("Title");
					}
					if (pdfDictionary3.ContainsKey("Author"))
					{
						pdfDictionary3.Remove("Author");
					}
					if (pdfDictionary3.ContainsKey("Subject"))
					{
						pdfDictionary3.Remove("Subject");
					}
					if (pdfDictionary3.ContainsKey("Keywords"))
					{
						pdfDictionary3.Remove("Keywords");
					}
					if (pdfDictionary3.ContainsKey("Producer"))
					{
						pdfDictionary3.Remove("Producer");
					}
					if (pdfDictionary3.ContainsKey("Creator"))
					{
						pdfDictionary3.Remove("Creator");
					}
					if (pdfDictionary3.ContainsKey("Trapped"))
					{
						pdfDictionary3.Remove("Trapped");
					}
					pdfDictionary3.Modify();
				}
			}
		}
		if (m_document.FileStructure.CrossReferenceType == PdfCrossReferenceType.CrossReferenceTable && security != null && security.Enabled && security.Encryptor.Encrypt && m_document is PdfDocument && security.Encryptor.UserPassword.Length == 0 && security.Encryptor.OwnerPassword.Length == 0)
		{
			enabled = security.Enabled;
			security.Enabled = false;
		}
		SaveObjects(writer);
		if (m_document.FileStructure.CrossReferenceType == PdfCrossReferenceType.CrossReferenceTable && security != null && security.Enabled && security.Encryptor.Encrypt && m_document is PdfDocument && security.Encryptor.UserPassword.Length == 0 && security.Encryptor.OwnerPassword.Length == 0)
		{
			security.Enabled = enabled;
		}
		int count = Count;
		SaveArchives(writer);
		if (writer.ObtainStream().CanSeek)
		{
			writer.Position = writer.Length;
		}
		long position = writer.Position;
		RegisterObject(0L, new PdfReference(0L, -1), free: true);
		long num = ((m_crossTable == null) ? 0 : m_crossTable.XRefOffset);
		num = (m_bForceNew ? 0 : num);
		if (IsCrossReferenceStream(writer.Document))
		{
			PdfReference reference;
			PdfStream pdfStream = PrepareXRefStream(num, position, out reference);
			pdfStream.BlockEncryption();
			DoSaveObject(pdfStream, reference, writer);
		}
		else
		{
			writer.Write("xref");
			writer.Write("\r\n");
			SaveSections(writer);
			SaveTrailer(writer, Count, num);
		}
		SaveTheEndess(writer, position);
		Count = count;
		for (int i = 0; i < ObjectCollection.Count; i++)
		{
			ObjectCollection[i].Object.IsSaving = false;
		}
	}

	internal PdfReference GetReference(IPdfPrimitive obj)
	{
		bool bNew;
		return GetReference(obj, out bNew);
	}

	internal PdfReference GetReference(IPdfPrimitive obj, out bool bNew)
	{
		bool flag = false;
		if (obj is PdfArchiveStream)
		{
			PdfReference result = FindArchiveReference(obj as PdfArchiveStream);
			bNew = flag;
			return result;
		}
		if (obj is PdfReferenceHolder)
		{
			obj = (obj as PdfReferenceHolder).Object;
			if (m_document is PdfDocument)
			{
				obj.IsSaving = true;
			}
		}
		if (obj is IPdfWrapper)
		{
			obj = (obj as IPdfWrapper).Element;
		}
		PdfReference pdfReference = null;
		if (obj == null)
		{
			obj = new PdfNull();
		}
		bool isNew;
		if (obj.IsSaving)
		{
			if (m_items.Count > 0 && obj.ObjectCollectionIndex > 0 && m_items.Count > obj.ObjectCollectionIndex - 1)
			{
				pdfReference = ((!m_items[obj.ObjectCollectionIndex - 1].Equals(obj)) ? Document.PdfObjects.GetReference(obj, out isNew) : Document.PdfObjects.GetReference(obj.ObjectCollectionIndex - 1));
			}
		}
		else
		{
			pdfReference = Document.PdfObjects.GetReference(obj, out isNew);
		}
		isNew = pdfReference == null && obj.Status != ObjectStatus.Registered;
		if (m_bForceNew)
		{
			if (pdfReference == null)
			{
				long num = 0L;
				if (m_storedCount > 0)
				{
					m_storedCount++;
					num = m_storedCount;
				}
				else
				{
					num = Document.PdfObjects.Count;
				}
				if (CrossTable != null && CrossTable.m_isOpenAndRepair)
				{
					bool flag2;
					while (true)
					{
						flag2 = false;
						if (!CrossTable.m_objects.ContainsKey(num))
						{
							break;
						}
						flag2 = true;
						num++;
					}
					if (flag2)
					{
						m_storedCount = (int)num;
					}
				}
				if (num <= 0)
				{
					num = 1L;
					m_storedCount = 2;
				}
				for (; Document.PdfObjects.mainObjectCollection.ContainsKey(num); num++)
				{
				}
				pdfReference = new PdfReference(num, 0);
				if (isNew)
				{
					Document.PdfObjects.Add(obj, pdfReference);
					if (!m_isMerging)
					{
						obj.Position = -1;
						pdfReference.Position = -1;
					}
				}
				else
				{
					Document.PdfObjects.TrySetReference(obj, pdfReference, out var _);
				}
			}
			pdfReference = GetMappedReference(pdfReference);
		}
		if (pdfReference == null)
		{
			int nextObjNumber = NextObjNumber;
			if (m_crossTable != null && m_crossTable.m_objects != null)
			{
				while (m_crossTable.m_objects.ContainsKey(nextObjNumber))
				{
					nextObjNumber = NextObjNumber;
				}
			}
			if (Document.PdfObjects.mainObjectCollection.ContainsKey(nextObjNumber))
			{
				pdfReference = new PdfReference(NextObjNumber, 0);
			}
			else
			{
				PdfNumber pdfNumber = null;
				IPdfPrimitive pdfPrimitive = null;
				if (m_crossTable != null)
				{
					pdfPrimitive = m_crossTable.Trailer["Size"];
				}
				if (pdfPrimitive != null)
				{
					pdfNumber = Dereference(pdfPrimitive) as PdfNumber;
				}
				pdfReference = ((pdfNumber == null || nextObjNumber != pdfNumber.IntValue) ? new PdfReference(nextObjNumber, 0) : new PdfReference(NextObjNumber, 0));
			}
			bool found2;
			if (isNew)
			{
				Document.PdfObjects.Add(obj);
				if (pdfReference != null && Document is PdfLoadedDocument && Document.PdfObjects.mainObjectCollection.ContainsKey(pdfReference.ObjNum))
				{
					pdfReference = new PdfReference(NextObjNumber, 0);
				}
				Document.PdfObjects.TrySetReference(obj, pdfReference, out found2);
				long objNum = Document.PdfObjects[Document.PdfObjects.Count - 1].Reference.ObjNum;
				if (!Document.PdfObjects.mainObjectCollection.ContainsKey(objNum))
				{
					Document.PdfObjects.mainObjectCollection.Add(objNum, Document.PdfObjects[Document.PdfObjects.Count - 1]);
				}
				if (!m_isMerging && !m_isMerging)
				{
					obj.Position = -1;
				}
			}
			else
			{
				Document.PdfObjects.TrySetReference(obj, pdfReference, out found2);
			}
			obj.ObjectCollectionIndex = (int)pdfReference.ObjNum;
			obj.Status = ObjectStatus.None;
			flag = true;
		}
		bNew = flag || m_bForceNew;
		return pdfReference;
	}

	internal void ForceNew()
	{
		m_crossTable.Trailer.Remove("Size");
		m_crossTable.Trailer.Remove("Prev");
		if (m_count > 0)
		{
			m_storedCount = m_count;
		}
		m_count = 0;
		m_bForceNew = true;
	}

	private void ObjectDecyptionProcess(IPdfPrimitive result)
	{
		bool flag = true;
		IPdfDecryptable encryptedObj = result as IPdfDecryptable;
		if (result is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("Type"))
		{
			PdfName pdfName = pdfDictionary["Type"] as PdfName;
			if (pdfName != null && pdfName.Value == "Metadata" && Encryptor != null)
			{
				flag = Encryptor.EncryptMetaData;
			}
		}
		if (flag)
		{
			DecryptionPrimitives(encryptedObj, result);
		}
	}

	private void DecryptionPrimitives(IPdfDecryptable encryptedObj, IPdfPrimitive result)
	{
		if (encryptedObj == null)
		{
			Decrypt(result);
		}
		else
		{
			if (encryptedObj == null || m_objNumbers.Count <= 0)
			{
				return;
			}
			PdfStream pdfStream = encryptedObj as PdfStream;
			PdfReference pdfReference = m_objNumbers.Peek();
			PdfName pdfName = null;
			if (pdfStream != null && pdfStream.ContainsKey("Type"))
			{
				pdfName = Dereference(pdfStream["Type"]) as PdfName;
			}
			if ((pdfStream != null && pdfReference != null && (pdfStream.ContainsKey("DL") || (pdfName != null && pdfName.Value == "EmbeddedFile"))) || IsPDFAppend)
			{
				Decrypt(result);
			}
			else if (pdfStream != null)
			{
				pdfStream.PdfCrossTable = this;
				pdfStream.ObjNumber = pdfReference.ObjNum;
				if (pdfStream.ContainsKey("ColorSpace") && Dereference(pdfStream["ColorSpace"]) is PdfArray obj)
				{
					Decrypt(obj);
				}
			}
		}
	}

	private void MarkTrailerReferences()
	{
		foreach (IPdfPrimitive value in Trailer.Values)
		{
			PdfReferenceHolder pdfReferenceHolder = value as PdfReferenceHolder;
			if (pdfReferenceHolder != null && !Document.PdfObjects.Contains(pdfReferenceHolder.Object))
			{
				Document.PdfObjects.Add(pdfReferenceHolder.Object);
				if (!m_isMerging)
				{
					pdfReferenceHolder.Object.Position = -1;
				}
			}
		}
	}

	private IPdfPrimitive PageProceed(IPdfPrimitive obj)
	{
		if (obj is PdfLoadedPage)
		{
			return obj;
		}
		if (obj is PdfDictionary pdfDictionary && !(obj is PdfPage) && pdfDictionary.ContainsKey("Type"))
		{
			IPdfPrimitive pdfPrimitive = pdfDictionary["Type"];
			if (pdfPrimitive.GetType().Name == "PdfName" && (GetObject(pdfPrimitive) as PdfName).Value == "Page" && !pdfDictionary.ContainsKey("Kids"))
			{
				obj = ((IPdfWrapper)(Document as PdfLoadedDocument).Pages.GetPage(pdfDictionary)).Element;
				PdfMainObjectCollection pdfObjects = Document.PdfObjects;
				int num = pdfObjects.IndexOf(pdfDictionary);
				if (num >= 0)
				{
					pdfObjects.ReregisterReference(num, obj);
					if (!m_isMerging)
					{
						obj.Position = -1;
					}
				}
			}
		}
		return obj;
	}

	private PdfStream PrepareXRefStream(long prevXRef, long position, out PdfReference reference)
	{
		PdfStream pdfStream = Trailer as PdfStream;
		if (pdfStream == null)
		{
			pdfStream = new PdfStream();
		}
		else
		{
			pdfStream.Remove("Filter");
			pdfStream.Remove("DecodeParms");
		}
		PdfArray pdfArray = new PdfArray();
		reference = new PdfReference(NextObjNumber, 0);
		RegisterObject(position, reference);
		long objectNum = 0L;
		long num = 0L;
		int[] array = new int[3] { 1, 8, 1 };
		array[1] = Math.Max(GetSize((ulong)position), GetSize((ulong)Count));
		array[2] = GetSize((ulong)m_maxGenNumIndex);
		using (MemoryStream memoryStream = new MemoryStream(100))
		{
			using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			while ((num = PrepareSubsection(ref objectNum)) > 0)
			{
				pdfArray.Add(new PdfNumber(objectNum));
				pdfArray.Add(new PdfNumber(num));
				SaveSubsection(binaryWriter, objectNum, num, array);
				objectNum += num;
			}
			binaryWriter.Flush();
			pdfStream.Data = memoryStream.ToArray();
		}
		pdfStream["Index"] = pdfArray;
		pdfStream["Size"] = new PdfNumber(Count);
		if (prevXRef != 0L)
		{
			pdfStream["Prev"] = new PdfNumber(prevXRef);
		}
		pdfStream["Type"] = new PdfName("XRef");
		pdfStream["W"] = new PdfArray(array);
		if (m_crossTable != null)
		{
			PdfDictionary trailer = m_crossTable.Trailer;
			foreach (PdfName key in trailer.Keys)
			{
				if (!pdfStream.ContainsKey(key) && key.Value != "DecodeParms" && key.Value != "Filter")
				{
					pdfStream[key] = trailer[key];
				}
			}
		}
		if (prevXRef == 0L && m_bForceNew && pdfStream.ContainsKey("Prev"))
		{
			pdfStream.Remove("Prev");
		}
		ForceIDHex(pdfStream);
		pdfStream.Encrypt = false;
		return pdfStream;
	}

	private int GetSize(ulong number)
	{
		int num = 0;
		if (number < uint.MaxValue)
		{
			if (number < 65535)
			{
				if (number < 255)
				{
					return 1;
				}
				return 2;
			}
			if (number < 16777215)
			{
				return 3;
			}
			return 4;
		}
		return 8;
	}

	private void SaveSubsection(BinaryWriter xRefStream, long objectNum, long count, int[] format)
	{
		for (long num = objectNum; num < objectNum + count; num++)
		{
			RegisteredObject registeredObject = m_objects[num];
			xRefStream.Write((byte)registeredObject.Type);
			switch (registeredObject.Type)
			{
			case ObjectType.Free:
				SaveLong(xRefStream, registeredObject.ObjectNumber, format[1]);
				SaveLong(xRefStream, registeredObject.GenerationNumber, format[2]);
				break;
			case ObjectType.Normal:
				SaveLong(xRefStream, registeredObject.Offset, format[1]);
				SaveLong(xRefStream, registeredObject.GenerationNumber, format[2]);
				break;
			case ObjectType.Packed:
				SaveLong(xRefStream, registeredObject.ObjectNumber, format[1]);
				SaveLong(xRefStream, registeredObject.Offset, format[2]);
				break;
			default:
				throw new PdfDocumentException("Internal error: Undefined object type.");
			}
		}
	}

	private void SaveLong(BinaryWriter xRefStream, long number, int count)
	{
		for (int num = count - 1; num >= 0; num--)
		{
			byte value = (byte)((number >> (num << 3)) & 0xFF);
			xRefStream.Write(value);
		}
	}

	private void SetSecurity()
	{
		PdfSecurity security = m_document.Security;
		Trailer.Encrypt = false;
		if (security.Encryptor.Encrypt)
		{
			PdfDictionary pdfDictionary = EncryptorDictionary;
			if (pdfDictionary == null)
			{
				pdfDictionary = new PdfDictionary();
				pdfDictionary.Encrypt = false;
				m_document.PdfObjects.Add(pdfDictionary);
				if (!m_isMerging)
				{
					pdfDictionary.Position = -1;
				}
				PdfReferenceHolder value = new PdfReferenceHolder(pdfDictionary);
				Trailer["Encrypt"] = value;
				if (Document != null && Document is PdfLoadedDocument)
				{
					PdfReference reference = GetReference(pdfDictionary);
					bool found = false;
					Document.PdfObjects.TrySetReference(pdfDictionary, reference, out found);
					long objNum = Document.PdfObjects[Document.PdfObjects.Count - 1].Reference.ObjNum;
					if (!Document.PdfObjects.mainObjectCollection.ContainsKey(objNum))
					{
						Document.PdfObjects.mainObjectCollection.Add(objNum, Document.PdfObjects[Document.PdfObjects.Count - 1]);
					}
				}
			}
			else if (!m_document.PdfObjects.Contains(pdfDictionary))
			{
				m_document.PdfObjects.Add(pdfDictionary);
				if (!m_isMerging)
				{
					pdfDictionary.Position = -1;
				}
				PdfReferenceHolder value2 = new PdfReferenceHolder(pdfDictionary);
				Trailer["Encrypt"] = value2;
			}
			security.Encryptor.SaveToDictionary(pdfDictionary);
			Trailer["ID"] = security.Encryptor.FileID;
			Trailer["Encrypt"] = new PdfReferenceHolder(pdfDictionary);
		}
		else
		{
			if (security.m_encryptOnlyAttachment)
			{
				return;
			}
			if (Trailer.ContainsKey("Encrypt"))
			{
				Trailer.Remove("Encrypt");
			}
			bool fileID = m_document.FileStructure.m_fileID;
			bool flag = false;
			if (m_document != null)
			{
				PdfLoadedDocument pdfLoadedDocument = m_document as PdfLoadedDocument;
				bool num = Trailer.ContainsKey("Info");
				if (pdfLoadedDocument != null && pdfLoadedDocument.Conformance != 0)
				{
					flag = true;
				}
				if (!num)
				{
					Trailer.Remove("Info");
				}
			}
			if (Trailer.ContainsKey("ID") && !fileID && !flag)
			{
				Trailer.Remove("ID");
			}
		}
	}

	private PdfArray GetFileID()
	{
		string text = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
		string text2 = "";
		if (CrossTable != null)
		{
			text2 = CrossTable.Stream.Length.ToString();
		}
		string s = text + text2;
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		PdfString element = new PdfString(new MessageDigestAlgorithms().Digest("MD5", bytes));
		return new PdfArray { element, element };
	}

	private void SaveObjects(PdfWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		PdfMainObjectCollection objectCollection = ObjectCollection;
		if (m_bForceNew)
		{
			Count = 1;
			m_mappedReferences = null;
		}
		SetSecurity();
		if (m_document.FileStructure.m_fileID && !m_document.Security.Enabled)
		{
			if (Trailer.ContainsKey("ID"))
			{
				PdfArray pdfArray = new PdfArray();
				if (Trailer["ID"] is PdfArray { Count: >0 } pdfArray2)
				{
					IPdfPrimitive element = pdfArray2[0];
					pdfArray.Add(element);
				}
				IPdfPrimitive element2 = GetFileID()[0];
				pdfArray.Add(element2);
				Trailer["ID"] = pdfArray;
			}
			else
			{
				Trailer["ID"] = GetFileID();
			}
		}
		List<IPdfPrimitive> list = null;
		for (int i = 0; i < objectCollection.Count; i++)
		{
			ObjectInfo objectInfo = objectCollection[i];
			if (!objectInfo.Modified && !m_bForceNew)
			{
				continue;
			}
			IPdfPrimitive @object = objectInfo.Object;
			if (@object is PdfStructTreeRoot)
			{
				if (list == null)
				{
					list = new List<IPdfPrimitive>();
				}
				if (!list.Contains(@object))
				{
					list.Add(@object);
				}
				continue;
			}
			SavePrimitive(@object, writer);
			if (i == objectCollection.Count - 1 && list != null && list.Count > 0)
			{
				for (int j = 0; j < list.Count; j++)
				{
					SavePrimitive(list[j], writer);
				}
				list.Clear();
			}
		}
	}

	private void SavePrimitive(IPdfPrimitive obj, PdfWriter writer)
	{
		if (Document is PdfDocument)
		{
			obj.IsSaving = true;
		}
		if (obj == Trailer)
		{
			return;
		}
		if (obj is PdfCatalog && m_documentCatalog != null && m_documentCatalog.ContainsKey("Perms") && Document.FileStructure.IncrementalUpdate)
		{
			PdfDictionary pdfDictionary = null;
			if (m_documentCatalog["Perms"] is PdfDictionary)
			{
				pdfDictionary = m_documentCatalog["Perms"] as PdfDictionary;
			}
			else if (m_documentCatalog["Perms"] is PdfReferenceHolder)
			{
				pdfDictionary = (m_documentCatalog["Perms"] as PdfReferenceHolder).Object as PdfDictionary;
			}
			if (pdfDictionary.ContainsKey("UR3") || pdfDictionary.ContainsKey("DocMDP"))
			{
				SaveIndirectObject(obj, writer);
			}
			return;
		}
		if (obj is PdfStructTreeRoot)
		{
			StructureRootElements(obj as PdfStructTreeRoot);
		}
		bool flag = false;
		if (obj != null && obj is PdfDictionary && (obj as PdfDictionary).isSkip)
		{
			flag = true;
		}
		if (!flag)
		{
			SaveIndirectObject(obj, writer);
		}
	}

	private void StructureRootElements(PdfStructTreeRoot treeRoot)
	{
		if (treeRoot == null || PdfDocument.ConformanceLevel != 0 || !(Dereference(treeRoot["K"]) is PdfArray pdfArray))
		{
			return;
		}
		PdfDictionary pdfDictionary = new PdfDictionary();
		for (int i = 0; i < pdfArray.Count; i++)
		{
			if (Dereference(pdfArray[i]) is PdfDictionary pdfDictionary2)
			{
				pdfDictionary2["P"] = new PdfReferenceHolder(pdfDictionary);
			}
		}
		pdfDictionary["S"] = new PdfName("Document");
		pdfDictionary["K"] = new PdfReferenceHolder(pdfArray);
		pdfDictionary["P"] = new PdfReferenceHolder(treeRoot);
		treeRoot["K"] = new PdfReferenceHolder(pdfDictionary);
	}

	private void SaveArchives(PdfWriter writer)
	{
		if (m_archives == null)
		{
			return;
		}
		foreach (ArchiveInfo archive in m_archives)
		{
			PdfReference pdfReference = archive.Reference;
			if (pdfReference == null)
			{
				pdfReference = (archive.Reference = new PdfReference(NextObjNumber, 0));
			}
			m_document.CurrentSavingObj = pdfReference;
			RegisterObject(writer.Position, pdfReference);
			DoSaveObject(archive.Archive, pdfReference, writer);
		}
	}

	private PdfReference GetMappedReference(PdfReference reference)
	{
		if (reference == null)
		{
			return null;
		}
		if (m_mappedReferences == null)
		{
			m_mappedReferences = new Dictionary<PdfReference, PdfReference>(100);
		}
		PdfReference value = null;
		m_mappedReferences.TryGetValue(reference, out value);
		if (value == null)
		{
			value = new PdfReference(NextObjNumber, 0);
			m_mappedReferences[reference] = value;
		}
		return value;
	}

	private PdfReference FindArchiveReference(PdfArchiveStream archive)
	{
		int i = 0;
		ArchiveInfo archiveInfo = null;
		for (int count = m_archives.Count; i < count; i++)
		{
			archiveInfo = m_archives[i];
			if (archiveInfo.Archive == archive)
			{
				break;
			}
		}
		PdfReference pdfReference = archiveInfo.Reference;
		if (pdfReference == null)
		{
			pdfReference = new PdfReference(NextObjNumber, 0);
		}
		archiveInfo.Reference = pdfReference;
		return pdfReference;
	}

	internal void SaveIndirectObject(IPdfPrimitive obj, PdfWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (obj is PdfEncryptor && !(obj as PdfEncryptor).Encrypt)
		{
			return;
		}
		PdfReference reference = GetReference(obj);
		if (obj is PdfCatalog)
		{
			Trailer["Root"] = reference;
			if (PdfDocument.ConformanceLevel != 0)
			{
				PdfSecurity security = m_document.Security;
				Trailer["ID"] = security.Encryptor.FileID;
			}
		}
		m_document.CurrentSavingObj = reference;
		bool flag = false;
		flag = !(obj is PdfDictionary) || (obj as PdfDictionary).Archive;
		if (obj is PdfDictionary && Document.FileStructure.CrossReferenceType == PdfCrossReferenceType.CrossReferenceStream && Document is PdfLoadedDocument)
		{
			if (Document.Catalog.LoadedForm != null)
			{
				if (Document.Catalog.LoadedForm.SignatureFlags == (SignatureFlags.SignaturesExists | SignatureFlags.AppendOnly))
				{
					flag = false;
				}
			}
			else if (Document.Catalog.ContainsKey("AcroForm") && Dereference(Document.Catalog["AcroForm"]) is PdfDictionary pdfDictionary && pdfDictionary.ContainsKey("SigFlags") && Dereference(pdfDictionary["SigFlags"]) is PdfNumber { IntValue: 3 })
			{
				flag = false;
			}
		}
		bool num = !(obj is PdfStream) && flag && !(obj is PdfCatalog) && !(obj is Pdf3DStream);
		bool flag2 = false;
		if (obj is PdfDictionary && Document.FileStructure.CrossReferenceType == PdfCrossReferenceType.CrossReferenceStream)
		{
			PdfDictionary pdfDictionary2 = obj as PdfDictionary;
			if (pdfDictionary2.ContainsKey("Type") && pdfDictionary2["Type"] as PdfName == new PdfName("Sig"))
			{
				flag2 = true;
			}
			if (pdfDictionary2.ContainsKey("FT") && pdfDictionary2["FT"] as PdfName == new PdfName("Sig"))
			{
				flag2 = true;
			}
		}
		if (num && IsCrossReferenceStream(writer.Document) && reference.GenNum == 0 && !flag2)
		{
			DoArchiveObject(obj, reference, writer);
			return;
		}
		RegisterObject(writer.Position, reference);
		DoSaveObject(obj, reference, writer);
		if (obj == m_archive)
		{
			m_archive = null;
		}
	}

	private void DoArchiveObject(IPdfPrimitive obj, PdfReference reference, PdfWriter writer)
	{
		if (m_archive == null)
		{
			m_archive = new PdfArchiveStream(m_document);
			SaveArchive(writer);
		}
		_ = m_archive.ObjCount;
		RegisterObject(m_archive, reference);
		m_archive.SaveObject(obj, reference);
		if (m_archive.ObjCount >= 100)
		{
			m_archive = null;
		}
	}

	private void SaveArchive(PdfWriter writer)
	{
		ArchiveInfo item = new ArchiveInfo(null, m_archive);
		if (m_archives == null)
		{
			m_archives = new List<ArchiveInfo>(10);
		}
		m_archives.Add(item);
	}

	private void DoSaveObject(IPdfPrimitive obj, PdfReference reference, PdfWriter writer)
	{
		bool flag = false;
		if (writer != null && writer.isCompress)
		{
			flag = true;
		}
		long length = writer.Length;
		if (writer.ObtainStream().CanSeek && writer.Position != length)
		{
			writer.Position = length;
		}
		writer.Write(reference.ObjNum.ToString(CultureInfo.InvariantCulture));
		writer.Write(" ");
		writer.Write(reference.GenNum.ToString(CultureInfo.InvariantCulture));
		writer.Write(" ");
		writer.Write("obj");
		writer.Write(flag ? " " : "\r\n");
		lock (PdfDocument.Cache)
		{
			obj.Save(writer);
		}
		if (obj is PdfName || obj is PdfNumber || obj is PdfNull)
		{
			writer.Write("\r\n");
		}
		if (writer.ObtainStream().CanRead)
		{
			Stream stream = writer.ObtainStream();
			BinaryReader binaryReader = new BinaryReader(stream);
			if (binaryReader.BaseStream.CanRead)
			{
				binaryReader.BaseStream.Position = stream.Length - 1;
				if (binaryReader.ReadChar() != '\n')
				{
					writer.Write("\r\n");
				}
			}
		}
		else if (writer.ObtainStream().CanWrite)
		{
			writer.Write("\r\n");
		}
		writer.Write("endobj");
		writer.Write("\r\n");
	}

	private PdfDictionary GeneratePagesRoot()
	{
		return ((DocumentCatalog["Pages"] ?? throw new PdfDocumentException("Invalid/Unknown/Unsupported format")) as PdfDictionary) ?? throw new PdfDocumentException("Invalid/Unknown/Unsupported format");
	}

	private void SaveSections(PdfWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		long objectNum = 0L;
		long num = 0L;
		do
		{
			num = PrepareSubsection(ref objectNum);
			SaveSubsection(writer, objectNum, num);
			objectNum += num;
		}
		while (num != 0L);
	}

	private long PrepareSubsection(ref long objectNum)
	{
		long num = 0L;
		int num2 = Count;
		if (num2 <= 0)
		{
			num2 = Document.PdfObjects.Count + 1;
		}
		if (num2 < Document.PdfObjects.m_maximumReferenceObjNumber)
		{
			num2 = Document.PdfObjects.m_maximumReferenceObjNumber;
			isIndexGreaterthanTotalObjectCount = true;
		}
		if (objectNum >= num2)
		{
			return num;
		}
		long num3;
		for (num3 = objectNum; num3 < num2 && !m_objects.ContainsKey(num3); num3++)
		{
		}
		objectNum = num3;
		for (; num3 < num2 && m_objects.ContainsKey(num3); num3++)
		{
			num++;
		}
		return num;
	}

	private void SaveSubsection(PdfWriter writer, long objectNum, long count)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (count > 0 && (objectNum < Count || isIndexGreaterthanTotalObjectCount))
		{
			writer.Write(string.Format("{0} {1}{2}", objectNum, count, "\r\n"));
			for (long num = objectNum; num < objectNum + count; num++)
			{
				RegisteredObject registeredObject = m_objects[num];
				string item = GetItem(registeredObject.Offset, registeredObject.GenerationNumber, registeredObject.Type == ObjectType.Free);
				writer.Write(item);
			}
		}
	}

	internal static string GetItem(long offset, long genNumber, bool isFree)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(offset.ToString("0000000000 "));
		stringBuilder.Append(((ushort)genNumber).ToString("00000 "));
		stringBuilder.Append(isFree ? "f" : "n");
		stringBuilder.Append("\r\n");
		return stringBuilder.ToString();
	}

	private void SaveTrailer(PdfWriter writer, long count, long prevXRef)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.Write("trailer\r\n");
		PdfDictionary trailer = Trailer;
		if (prevXRef != 0L)
		{
			trailer["Prev"] = new PdfNumber(prevXRef);
		}
		ForceIDHex(trailer);
		trailer["Size"] = new PdfNumber(m_count);
		trailer = new PdfDictionary(trailer);
		trailer.Encrypt = false;
		trailer.Save(writer);
	}

	private void ForceIDHex(PdfDictionary trailer)
	{
		if (!(Dereference(trailer["ID"]) is PdfArray pdfArray))
		{
			return;
		}
		foreach (PdfString item in pdfArray)
		{
			item.Encode = PdfString.ForceEncoding.ASCII;
			item.ToHex();
		}
	}

	private void SaveTheEndess(PdfWriter writer, long xrefPos)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.Write("\r\nstartxref\r\n");
		writer.Write(xrefPos + "\r\n");
		writer.Write("%%EOF\r\n");
	}

	private void SaveHead(PdfWriter writer)
	{
		byte[] data = new byte[5] { 37, 131, 146, 250, 254 };
		if (m_document is PdfLoadedDocument && !IsEndWithNewLine(writer))
		{
			writer.Write(" ");
		}
		writer.Write("%PDF-");
		string text = GenerateFileVersion(writer.Document);
		writer.Write(text);
		writer.Write("\r\n");
		writer.Write(data);
		writer.Write("\r\n");
	}

	private bool IsEndWithNewLine(PdfWriter writer)
	{
		int num = 2;
		long position = writer.Position;
		if (position == 0L)
		{
			return true;
		}
		bool result = false;
		Stream stream = writer.m_stream;
		if (position > num && stream.CanRead && stream.CanSeek)
		{
			stream.Seek(-num, SeekOrigin.End);
			byte[] array = new byte[num];
			stream.Read(array, 0, num);
			if (array[0] == 13 || array[0] == 10 || array[1] == 13 || array[1] == 10)
			{
				result = true;
			}
		}
		return result;
	}

	private string GenerateFileVersion(PdfDocumentBase document)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_X1A2001)
		{
			document.FileStructure.Version = PdfVersion.Version1_3;
		}
		int version = (int)document.FileStructure.Version;
		if ((PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A4 || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A4E || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A4F) && document.Catalog.ContainsKey("Version") && (document.Catalog["Version"] as PdfName).Value != document.FileStructure.Version.ToString())
		{
			string value = "2.0";
			document.Catalog["Version"] = new PdfName(value);
		}
		if (document.FileStructure.Version == PdfVersion.Version2_0)
		{
			string text = "2.0";
			if (document is PdfLoadedDocument && document.FileStructure.IncrementalUpdate)
			{
				document.Catalog["Version"] = new PdfName(text);
			}
			return text;
		}
		if (document.Catalog != null && document.Catalog.ContainsKey("AcroForm"))
		{
			PdfDictionary pdfDictionary = Dereference(document.Catalog["AcroForm"]) as PdfDictionary;
			string text2 = "1.6";
			if (pdfDictionary != null && pdfDictionary.ContainsKey("SigFlags") && document.FileStructure.Version <= PdfVersion.Version1_5 && document.FileStructure.IncrementalUpdate)
			{
				document.Catalog["Version"] = new PdfName(text2);
				return text2;
			}
			return "1." + version;
		}
		return "1." + version;
	}

	private bool IsCrossReferenceStream(PdfDocumentBase document)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		bool flag = false;
		if (m_crossTable != null)
		{
			return m_crossTable.Trailer is PdfStream;
		}
		return document.FileStructure.CrossReferenceType == PdfCrossReferenceType.CrossReferenceStream;
	}

	public void Dispose()
	{
		Dispose(completely: true);
	}

	internal void Close(bool completely)
	{
		if (completely)
		{
			if (m_archives != null)
			{
				m_archives.Clear();
				m_archives = null;
			}
			if (m_archive != null)
			{
				m_archive.Clear();
				m_archive = null;
			}
			if (m_items != null && m_items.Count > 0 && completely)
			{
				for (int num = m_items.Count - 1; num >= 0; num--)
				{
					ObjectInfo objectInfo = m_items[num];
					m_items.Remove(num);
					if (objectInfo.Object is PdfStream)
					{
						(objectInfo.Object as PdfStream).Clear();
					}
					else if (objectInfo.Object is PdfCatalog)
					{
						(objectInfo.Object as PdfCatalog).Clear();
					}
					else if (objectInfo.Object is PdfArray)
					{
						(objectInfo.Object as PdfArray).Clear();
					}
					objectInfo = null;
				}
				if (m_items.mainObjectCollection != null)
				{
					foreach (KeyValuePair<long, ObjectInfo> item in m_items.mainObjectCollection)
					{
						if (item.Value.Object is PdfStream)
						{
							(item.Value.Object as PdfStream).Clear();
						}
						else if (item.Value.Object is PdfCatalog)
						{
							(item.Value.Object as PdfCatalog).Clear();
						}
						if (item.Value.Object is PdfArray)
						{
							(item.Value.Object as PdfArray).Clear();
						}
					}
					m_items.mainObjectCollection.Clear();
				}
			}
			if (m_preReference != null)
			{
				m_preReference.Clear();
				m_preReference = null;
			}
			if (m_mappedReferences != null)
			{
				m_mappedReferences.Clear();
				m_mappedReferences = null;
			}
			if (m_objNumbers != null)
			{
				m_objNumbers.Clear();
				m_objNumbers = null;
			}
			if (m_pageCorrespondance != null)
			{
				if (isCompletely)
				{
					foreach (KeyValuePair<IPdfPrimitive, object> item2 in m_pageCorrespondance)
					{
						if (item2.Key is PdfStream)
						{
							(item2.Key as PdfStream).Dispose();
						}
						else if (item2.Key is PdfDictionary)
						{
							(item2.Key as PdfDictionary).Clear();
						}
						else if (item2.Key is PdfArray)
						{
							(item2.Key as PdfArray).Clear();
						}
						else if (item2.Key is PdfReferenceHolder)
						{
							IPdfPrimitive @object = (item2.Key as PdfReferenceHolder).Object;
							if (@object is PdfStream pdfStream)
							{
								pdfStream.Clear();
							}
							if (@object is PdfDictionary pdfDictionary)
							{
								pdfDictionary.Clear();
							}
							_ = @object as PdfReference != null;
						}
					}
				}
				m_pageCorrespondance.Clear();
				m_pageCorrespondance = null;
			}
		}
		Dispose();
		if (m_pdfObjects != null)
		{
			m_pdfObjects.Clear();
			m_pdfObjects = null;
		}
	}

	public void Dispose(bool completely)
	{
		if (!m_bDisposed)
		{
			if (m_stream != null && isDisposed && m_closeCompletely)
			{
				m_stream.Dispose();
				m_stream = null;
			}
			if (m_objects != null)
			{
				m_objects.Clear();
				m_objects = null;
			}
			if (isCompletely && m_crossTable != null)
			{
				m_crossTable.Dispose();
			}
			m_crossTable = null;
			m_documentCatalog = null;
			m_trailer = null;
			m_document = null;
			m_bDisposed = true;
			m_items = null;
		}
	}
}

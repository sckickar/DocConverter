using System;
using System.Collections.Generic;
using DocGen.DocIO.ODFConverter.Base.ODFImplementation;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class OParagraph : OTextBodyItem
{
	private string m_styleName;

	private string m_listStyleName;

	private int m_listLevelNumber;

	private string m_alphabeticalIndexMark;

	private string m_alphabeticalIndexMarkStart;

	private string m_alphabeticalIndexMarkEnd;

	private string m_authorInitals;

	private string m_authorName;

	private string m_bibliographyMark;

	private string m_bookMark;

	private string m_bookMarkStart;

	private string m_bookMarkEnd;

	private string m_bookMarkRef;

	private string m_change;

	private string m_changeStart;

	private string m_changeEnd;

	private string m_chapter;

	private string m_chapterCount;

	private string m_conditionalText;

	private DateTime m_creationDate;

	private DateTime m_creationTime;

	private string m_creator;

	private string m_databaseDisplay;

	private string m_databaseName;

	private string m_databaseNext;

	private string m_databaseRowNumber;

	private string m_databaseRowSelect;

	private DateTime m_date;

	private string m_ddeConnection;

	private string m_description;

	private string m_editingCycles;

	private string m_editingDuration;

	private string m_executeMacro;

	private string m_expression;

	private string m_fileName;

	private string m_hiddenParagraph;

	private string m_hiddenText;

	private int m_imageCount;

	private string m_initialCreator;

	private string m_keywords;

	private bool m_lineBreak;

	private DateTime m_modificationDate;

	private DateTime m_modificationTime;

	private string m_modificationNote;

	private string m_modificationNoteRef;

	private int m_objectCount;

	private string m_pageContinuation;

	private int m_pageCount;

	private PageNumber m_pageNumber;

	private string m_setvariable;

	private int m_paragraphCount;

	private string m_placeHolder;

	private DateTime m_printDate;

	private string m_printedBy;

	private DateTime m_printTime;

	private string m_referenceMark;

	private string m_referenceMarkStart;

	private string m_referenceMarkEnd;

	private string m_referenceRef;

	private string m_ruby;

	private string m_script;

	private string m_senderCity;

	private string m_senderCompany;

	private string m_senderCountry;

	private string m_senderEmail;

	private string m_senderFax;

	private string m_senderFirstName;

	private string m_senderinitials;

	private string m_senderLastName;

	private string m_senderPhonePrivate;

	private string m_senderPhoneWork;

	private string m_senderPosition;

	private string m_senderPostalCode;

	private string m_stateOrProvince;

	private string m_senderStreet;

	private string m_senderTitle;

	private string m_sequence;

	private string m_sequenceRef;

	private string m_sheetName;

	private string m_softPageBreak;

	private bool m_span;

	private string m_subject;

	private string m_tab;

	private int m_tableCount;

	private string m_tableFormula;

	private string m_templateName;

	private string m_textInput;

	private DateTime m_time;

	private string m_title;

	private string m_tocMark;

	private string m_tocMarkStart;

	private string m_tocMarkEnd;

	private string m_userDefined;

	private string m_userFieldGet;

	private string m_userFieldInput;

	private string m_userIndexMark;

	private string m_getVariable;

	private string m_setVariable;

	private string m_getVariableInput;

	private int m_wordCount;

	private Heading m_header;

	private List<OParagraphItem> m_OParagraphItemCollection;

	internal Heading Header
	{
		get
		{
			return m_header;
		}
		set
		{
			m_header = value;
		}
	}

	internal List<OParagraphItem> OParagraphItemCollection
	{
		get
		{
			if (m_OParagraphItemCollection == null)
			{
				m_OParagraphItemCollection = new List<OParagraphItem>();
			}
			return m_OParagraphItemCollection;
		}
		set
		{
			m_OParagraphItemCollection = value;
		}
	}

	internal string ListStyleName
	{
		get
		{
			return m_listStyleName;
		}
		set
		{
			m_listStyleName = value;
		}
	}

	internal int ListLevelNumber
	{
		get
		{
			return m_listLevelNumber;
		}
		set
		{
			m_listLevelNumber = value;
		}
	}

	internal int WordCount
	{
		get
		{
			return m_wordCount;
		}
		set
		{
			m_wordCount = value;
		}
	}

	internal string GetVariableInput
	{
		get
		{
			return m_getVariableInput;
		}
		set
		{
			m_getVariableInput = value;
		}
	}

	internal string SetVariable
	{
		get
		{
			return m_setVariable;
		}
		set
		{
			m_setVariable = value;
		}
	}

	internal string GetVariable1
	{
		get
		{
			return m_getVariable;
		}
		set
		{
			m_getVariable = value;
		}
	}

	internal string UserIndexMark
	{
		get
		{
			return m_userIndexMark;
		}
		set
		{
			m_userIndexMark = value;
		}
	}

	internal string UserFieldInput
	{
		get
		{
			return m_userFieldInput;
		}
		set
		{
			m_userFieldInput = value;
		}
	}

	internal string UserFieldGet
	{
		get
		{
			return m_userFieldGet;
		}
		set
		{
			m_userFieldGet = value;
		}
	}

	internal string UserDefined
	{
		get
		{
			return m_userDefined;
		}
		set
		{
			m_userDefined = value;
		}
	}

	internal string TocMarkEnd
	{
		get
		{
			return m_tocMarkEnd;
		}
		set
		{
			m_tocMarkEnd = value;
		}
	}

	internal string TocMarkStart
	{
		get
		{
			return m_tocMarkStart;
		}
		set
		{
			m_tocMarkStart = value;
		}
	}

	internal string TocMark
	{
		get
		{
			return m_tocMark;
		}
		set
		{
			m_tocMark = value;
		}
	}

	internal string Title
	{
		get
		{
			return m_title;
		}
		set
		{
			m_title = value;
		}
	}

	internal DateTime Time
	{
		get
		{
			return m_time;
		}
		set
		{
			m_time = value;
		}
	}

	internal string TextInput
	{
		get
		{
			return m_textInput;
		}
		set
		{
			m_textInput = value;
		}
	}

	internal string TemplateName
	{
		get
		{
			return m_templateName;
		}
		set
		{
			m_templateName = value;
		}
	}

	internal string TableFormula
	{
		get
		{
			return m_tableFormula;
		}
		set
		{
			m_tableFormula = value;
		}
	}

	internal int TableCount
	{
		get
		{
			return m_tableCount;
		}
		set
		{
			m_tableCount = value;
		}
	}

	internal string Tab
	{
		get
		{
			return m_tab;
		}
		set
		{
			m_tab = value;
		}
	}

	internal string Subject
	{
		get
		{
			return m_subject;
		}
		set
		{
			m_subject = value;
		}
	}

	internal bool Span
	{
		get
		{
			return m_span;
		}
		set
		{
			m_span = value;
		}
	}

	internal string SheetName
	{
		get
		{
			return m_sheetName;
		}
		set
		{
			m_sheetName = value;
		}
	}

	internal string SoftPageBreak
	{
		get
		{
			return m_softPageBreak;
		}
		set
		{
			m_softPageBreak = value;
		}
	}

	internal string SequenceRef
	{
		get
		{
			return m_sequenceRef;
		}
		set
		{
			m_sequenceRef = value;
		}
	}

	internal string Sequence
	{
		get
		{
			return m_sequence;
		}
		set
		{
			m_sequence = value;
		}
	}

	internal string SenderTitle
	{
		get
		{
			return m_senderTitle;
		}
		set
		{
			m_senderTitle = value;
		}
	}

	internal string SenderStreet
	{
		get
		{
			return m_senderStreet;
		}
		set
		{
			m_senderStreet = value;
		}
	}

	internal string StateOrProvince
	{
		get
		{
			return m_stateOrProvince;
		}
		set
		{
			m_stateOrProvince = value;
		}
	}

	internal string SenderPosition
	{
		get
		{
			return m_senderPosition;
		}
		set
		{
			m_senderPosition = value;
		}
	}

	internal string SenderPostalCode
	{
		get
		{
			return m_senderPostalCode;
		}
		set
		{
			m_senderPostalCode = value;
		}
	}

	internal string SenderPhoneWork
	{
		get
		{
			return m_senderPhoneWork;
		}
		set
		{
			m_senderPhoneWork = value;
		}
	}

	internal string SenderPhonePrivate
	{
		get
		{
			return m_senderPhonePrivate;
		}
		set
		{
			m_senderPhonePrivate = value;
		}
	}

	internal string SenderLastName
	{
		get
		{
			return m_senderLastName;
		}
		set
		{
			m_senderLastName = value;
		}
	}

	internal string Senderinitials
	{
		get
		{
			return m_senderinitials;
		}
		set
		{
			m_senderinitials = value;
		}
	}

	internal string SenderFirstName
	{
		get
		{
			return m_senderFirstName;
		}
		set
		{
			m_senderFirstName = value;
		}
	}

	internal string SenderFax
	{
		get
		{
			return m_senderFax;
		}
		set
		{
			m_senderFax = value;
		}
	}

	internal string SenderEmail
	{
		get
		{
			return m_senderEmail;
		}
		set
		{
			m_senderEmail = value;
		}
	}

	internal string SenderCountry
	{
		get
		{
			return m_senderCountry;
		}
		set
		{
			m_senderCountry = value;
		}
	}

	internal string SenderCompany
	{
		get
		{
			return m_senderCompany;
		}
		set
		{
			m_senderCompany = value;
		}
	}

	internal string SenderCity
	{
		get
		{
			return m_senderCity;
		}
		set
		{
			m_senderCity = value;
		}
	}

	internal string Script
	{
		get
		{
			return m_script;
		}
		set
		{
			m_script = value;
		}
	}

	internal string Ruby
	{
		get
		{
			return m_ruby;
		}
		set
		{
			m_ruby = value;
		}
	}

	internal string ReferenceRef
	{
		get
		{
			return m_referenceRef;
		}
		set
		{
			m_referenceRef = value;
		}
	}

	internal string ReferenceMarkEnd
	{
		get
		{
			return m_referenceMarkEnd;
		}
		set
		{
			m_referenceMarkEnd = value;
		}
	}

	internal string ReferenceMarkStart
	{
		get
		{
			return m_referenceMarkStart;
		}
		set
		{
			m_referenceMarkStart = value;
		}
	}

	internal string ReferenceMark
	{
		get
		{
			return m_referenceMark;
		}
		set
		{
			m_referenceMark = value;
		}
	}

	internal DateTime PrintTime
	{
		get
		{
			return m_printTime;
		}
		set
		{
			m_printTime = value;
		}
	}

	internal string PrintedBy
	{
		get
		{
			return m_printedBy;
		}
		set
		{
			m_printedBy = value;
		}
	}

	internal DateTime PrintDate
	{
		get
		{
			return m_printDate;
		}
		set
		{
			m_printDate = value;
		}
	}

	internal string PlaceHolder
	{
		get
		{
			return m_placeHolder;
		}
		set
		{
			m_placeHolder = value;
		}
	}

	internal int ParagraphCount
	{
		get
		{
			return m_paragraphCount;
		}
		set
		{
			m_paragraphCount = value;
		}
	}

	internal string Setvariable
	{
		get
		{
			return m_setvariable;
		}
		set
		{
			m_setvariable = value;
		}
	}

	internal string GetVariable
	{
		get
		{
			return m_getVariable;
		}
		set
		{
			m_getVariable = value;
		}
	}

	internal PageNumber PageNumber
	{
		get
		{
			return m_pageNumber;
		}
		set
		{
			m_pageNumber = value;
		}
	}

	internal int PageCount
	{
		get
		{
			return m_pageCount;
		}
		set
		{
			m_pageCount = value;
		}
	}

	internal string PageContinuation
	{
		get
		{
			return m_pageContinuation;
		}
		set
		{
			m_pageContinuation = value;
		}
	}

	internal int ObjectCount
	{
		get
		{
			return m_objectCount;
		}
		set
		{
			m_objectCount = value;
		}
	}

	internal string ModificationNoteRef
	{
		get
		{
			return m_modificationNoteRef;
		}
		set
		{
			m_modificationNoteRef = value;
		}
	}

	internal string ModificationNote
	{
		get
		{
			return m_modificationNote;
		}
		set
		{
			m_modificationNote = value;
		}
	}

	internal DateTime ModificationTime
	{
		get
		{
			return m_modificationTime;
		}
		set
		{
			m_modificationTime = value;
		}
	}

	internal DateTime ModificationDate
	{
		get
		{
			return m_modificationDate;
		}
		set
		{
			m_modificationDate = value;
		}
	}

	internal bool LineBreak
	{
		get
		{
			return m_lineBreak;
		}
		set
		{
			m_lineBreak = value;
		}
	}

	internal string Keywords
	{
		get
		{
			return m_keywords;
		}
		set
		{
			m_keywords = value;
		}
	}

	internal string InitialCreator
	{
		get
		{
			return m_initialCreator;
		}
		set
		{
			m_initialCreator = value;
		}
	}

	internal int ImageCount
	{
		get
		{
			return m_imageCount;
		}
		set
		{
			m_imageCount = value;
		}
	}

	internal string HiddenText
	{
		get
		{
			return m_hiddenText;
		}
		set
		{
			m_hiddenText = value;
		}
	}

	internal string HiddenParagraph
	{
		get
		{
			return m_hiddenParagraph;
		}
		set
		{
			m_hiddenParagraph = value;
		}
	}

	internal string FileName
	{
		get
		{
			return m_fileName;
		}
		set
		{
			m_fileName = value;
		}
	}

	internal string Expression
	{
		get
		{
			return m_expression;
		}
		set
		{
			m_expression = value;
		}
	}

	internal string ExecuteMacro
	{
		get
		{
			return m_executeMacro;
		}
		set
		{
			m_executeMacro = value;
		}
	}

	internal string EditingDuration
	{
		get
		{
			return m_editingDuration;
		}
		set
		{
			m_editingDuration = value;
		}
	}

	internal string EditingCycles
	{
		get
		{
			return m_editingCycles;
		}
		set
		{
			m_editingCycles = value;
		}
	}

	internal string Description
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

	internal string DdeConnection
	{
		get
		{
			return m_ddeConnection;
		}
		set
		{
			m_ddeConnection = value;
		}
	}

	internal DateTime Date
	{
		get
		{
			return m_date;
		}
		set
		{
			m_date = value;
		}
	}

	internal string DatabaseRowSelect
	{
		get
		{
			return m_databaseRowSelect;
		}
		set
		{
			m_databaseRowSelect = value;
		}
	}

	internal string DatabaseRowNumber
	{
		get
		{
			return m_databaseRowNumber;
		}
		set
		{
			m_databaseRowNumber = value;
		}
	}

	internal string DatabaseNext
	{
		get
		{
			return m_databaseNext;
		}
		set
		{
			m_databaseNext = value;
		}
	}

	internal string DatabaseDisplay
	{
		get
		{
			return m_databaseDisplay;
		}
		set
		{
			m_databaseDisplay = value;
		}
	}

	internal string Creator
	{
		get
		{
			return m_creator;
		}
		set
		{
			m_creator = value;
		}
	}

	internal DateTime CreationTime
	{
		get
		{
			return m_creationTime;
		}
		set
		{
			m_creationTime = value;
		}
	}

	internal DateTime CreationDate
	{
		get
		{
			return m_creationDate;
		}
		set
		{
			m_creationDate = value;
		}
	}

	internal string ConditionalText
	{
		get
		{
			return m_conditionalText;
		}
		set
		{
			m_conditionalText = value;
		}
	}

	internal string ChapterCount
	{
		get
		{
			return m_chapterCount;
		}
		set
		{
			m_chapterCount = value;
		}
	}

	internal string Chapter
	{
		get
		{
			return m_chapter;
		}
		set
		{
			m_chapter = value;
		}
	}

	internal string ChangeEnd
	{
		get
		{
			return m_changeEnd;
		}
		set
		{
			m_changeEnd = value;
		}
	}

	internal string ChangeStart
	{
		get
		{
			return m_changeStart;
		}
		set
		{
			m_changeStart = value;
		}
	}

	internal string Change
	{
		get
		{
			return m_change;
		}
		set
		{
			m_change = value;
		}
	}

	internal string BookMarkRef
	{
		get
		{
			return m_bookMarkRef;
		}
		set
		{
			m_bookMarkRef = value;
		}
	}

	internal string BookMarkEnd
	{
		get
		{
			return m_bookMarkEnd;
		}
		set
		{
			m_bookMarkEnd = value;
		}
	}

	internal string BookMarkStart
	{
		get
		{
			return m_bookMarkStart;
		}
		set
		{
			m_bookMarkStart = value;
		}
	}

	internal string BookMark
	{
		get
		{
			return m_bookMark;
		}
		set
		{
			m_bookMark = value;
		}
	}

	internal string BibliographyMark
	{
		get
		{
			return m_bibliographyMark;
		}
		set
		{
			m_bibliographyMark = value;
		}
	}

	internal string AuthorName
	{
		get
		{
			return m_authorName;
		}
		set
		{
			m_authorName = value;
		}
	}

	internal string AuthorInitals
	{
		get
		{
			return m_authorInitals;
		}
		set
		{
			m_authorInitals = value;
		}
	}

	internal string AlphabeticalIndexMarkEnd
	{
		get
		{
			return m_alphabeticalIndexMarkEnd;
		}
		set
		{
			m_alphabeticalIndexMarkEnd = value;
		}
	}

	internal string AlphabeticalIndexMarkStart
	{
		get
		{
			return m_alphabeticalIndexMarkStart;
		}
		set
		{
			m_alphabeticalIndexMarkStart = value;
		}
	}

	internal string AlphabeticalIndexMark
	{
		get
		{
			return m_alphabeticalIndexMark;
		}
		set
		{
			m_alphabeticalIndexMark = value;
		}
	}

	internal string StyleName
	{
		get
		{
			return m_styleName;
		}
		set
		{
			m_styleName = value;
		}
	}

	internal void Dispose()
	{
		if (m_OParagraphItemCollection == null)
		{
			return;
		}
		foreach (OParagraphItem item in m_OParagraphItemCollection)
		{
			item.Dispose();
		}
		m_OParagraphItemCollection.Clear();
		m_OParagraphItemCollection = null;
	}
}

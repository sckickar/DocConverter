using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Graphics.Fonts;
using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;
#pragma warning disable
internal class PdfFeatures
{
	internal float m_zoomFactor = 1f;

	private PdfUnitConvertor m_unitConvertor = new PdfUnitConvertor();

	private DeviceCMYK m_cmyk = new DeviceCMYK();

	private MatchedItemCollection m_searchObjects = new MatchedItemCollection();

	private Dictionary<int, string> m_pageTexts = new Dictionary<int, string>();

	private object m_lock = new object();

	private int m_pageCount;

	private PdfLoadedDocument m_loadedDocument;

	internal string searchstring;

	private const double CHAR_SIZE_MULTIPLIER = 0.001;

	private ArabicShapeRenderer m_arabicShapeRenderer;

	private bool m_wholeWords = true;

	internal bool continueOnError = true;

	private const int c_gapBetweenPages = 8;

	private List<Page> pagesForTextSearch;

	private void LoadPagesForTextSearch(PdfLoadedDocument loadedDocument)
	{
		pagesForTextSearch = new List<Page>();
		for (int i = 0; i < loadedDocument.PageCount; i++)
		{
			PdfPageBase page = loadedDocument.Pages[i];
			Page page2 = new Page(page);
			page2.Initialize(page, needParsing: false);
			pagesForTextSearch.Add(page2);
		}
	}

    /*
	 internal bool FindText(PdfLoadedDocument loadedDocument, List<string> searchItems, int pageIndex, TextSearchOptions textSearchOption, out List<MatchedItem> searchResults)
	{
		SearchInBackground(loadedDocument, searchItems, textSearchOption, pageIndex, pageIndex);
		return GetSearchResults(out searchResults);
	}
	 */

    internal bool FindText(PdfLoadedDocument loadedDocument, List<string> listOfTerms, TextSearchOptions textSearchOption, out TextSearchResultCollection searchResult, bool isMultiThread)
    {
        int PageCount = loadedDocument.Pages.Count;
        if (isMultiThread)
        {
            if (PageCount >= 8)
            {
                int splitCount = PageCount / 8;
                Task task = Task.Factory.StartNew(delegate
                {
                    SearchInBackground(loadedDocument, listOfTerms, textSearchOption, 0, splitCount);
                });
                Task task2 = Task.Factory.StartNew(delegate
                {
                    SearchInBackground(loadedDocument, listOfTerms, textSearchOption, splitCount + 1, 2 * splitCount);
                });
                Task task3 = Task.Factory.StartNew(delegate
                {
                    SearchInBackground(loadedDocument, listOfTerms, textSearchOption, 2 * splitCount + 1, 3 * splitCount);
                });
                Task task4 = Task.Factory.StartNew(delegate
                {
                    SearchInBackground(loadedDocument, listOfTerms, textSearchOption, 3 * splitCount + 1, 4 * splitCount);
                });
                Task task5 = Task.Factory.StartNew(delegate
                {
                    SearchInBackground(loadedDocument, listOfTerms, textSearchOption, 4 * splitCount + 1, 5 * splitCount);
                });
                Task task6 = Task.Factory.StartNew(delegate
                {
                    SearchInBackground(loadedDocument, listOfTerms, textSearchOption, 5 * splitCount + 1, 6 * splitCount);
                });
                Task task7 = Task.Factory.StartNew(delegate
                {
                    SearchInBackground(loadedDocument, listOfTerms, textSearchOption, 6 * splitCount + 1, 7 * splitCount);
                });
                Task task8 = Task.Factory.StartNew(delegate
                {
                    SearchInBackground(loadedDocument, listOfTerms, textSearchOption, 7 * splitCount + 1, PageCount - 1);
                });
                Task.WaitAll(task, task2, task3, task4, task5, task6, task7, task8);
            }
            else if (PageCount >= 4)
            {
                int splitCount = PageCount / 4;
                Task task9 = Task.Factory.StartNew(delegate
                {
                    SearchInBackground(loadedDocument, listOfTerms, textSearchOption, 0, splitCount);
                });
                Task task10 = Task.Factory.StartNew(delegate
                {
                    SearchInBackground(loadedDocument, listOfTerms, textSearchOption, splitCount + 1, 2 * splitCount);
                });
                Task task11 = Task.Factory.StartNew(delegate
                {
                    SearchInBackground(loadedDocument, listOfTerms, textSearchOption, 2 * splitCount + 1, 3 * splitCount);
                });
                Task task12 = Task.Factory.StartNew(delegate
                {
                    SearchInBackground(loadedDocument, listOfTerms, textSearchOption, 3 * splitCount + 1, PageCount - 1);
                });
                Task.WaitAll(task9, task10, task11, task12);
            }
            else if (PageCount >= 2)
            {
                Parallel.Invoke(delegate
                {
                    SearchInBackground(loadedDocument, listOfTerms, textSearchOption, 0, PageCount / 2);
                }, delegate
                {
                    SearchInBackground(loadedDocument, listOfTerms, textSearchOption, PageCount / 2 + 1, PageCount - 1);
                });
            }
            else
            {
                SearchInBackground(loadedDocument, listOfTerms, textSearchOption, 0, PageCount - 1);
            }
        }
        else
        {
            SearchInBackground(loadedDocument, listOfTerms, textSearchOption, 0, PageCount - 1);
        }
        return GetSearchResults(out searchResult);
    }


    private bool GetSearchResults(out TextSearchResultCollection searchResult)
	{
		bool result = false;
		searchResult = null;
		if (m_searchObjects.Count <= 0)
		{
			searchResult = null;
			m_pageTexts.Clear();
			return false;
		}
		m_searchObjects.Sort((MatchedItem value1, MatchedItem value2) => value1.PageNumber.CompareTo(value2.PageNumber));
		searchResult = new TextSearchResultCollection();
		int pageNumber = m_searchObjects[0].PageNumber;
		MatchedItemCollection matchedItemCollection = new MatchedItemCollection();
		foreach (MatchedItem searchObject in m_searchObjects)
		{
			if (matchedItemCollection.Count <= 0 || !CheckOverlapOrLarger(searchObject.Bounds, searchObject.PageNumber, matchedItemCollection))
			{
				if (pageNumber == searchObject.PageNumber)
				{
					matchedItemCollection.Add(searchObject);
					continue;
				}
				searchResult.Add(pageNumber, matchedItemCollection);
				matchedItemCollection = new MatchedItemCollection();
				pageNumber = searchObject.PageNumber;
				matchedItemCollection.Add(searchObject);
			}
		}
		searchResult.Add(pageNumber, matchedItemCollection);
		if (m_searchObjects.Count > 0)
		{
			result = true;
		}
		m_searchObjects.Clear();
		m_pageTexts.Clear();
		return result;
	}

	private bool GetSearchResults(out List<MatchedItem> searchResult)
	{
		bool result = false;
		searchResult = null;
		if (m_searchObjects.Count <= 0)
		{
			searchResult = null;
			m_pageTexts.Clear();
			return false;
		}
		searchResult = new List<MatchedItem>();
		_ = m_searchObjects[0].PageNumber;
		List<MatchedItem> list = new List<MatchedItem>();
		foreach (MatchedItem searchObject in m_searchObjects)
		{
			if (list.Count <= 0 || !CheckOverlapOrLarger(searchObject.Bounds, searchObject.PageNumber, list))
			{
				list.Add(searchObject);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			searchResult.Add(list[i]);
		}
		if (m_searchObjects.Count > 0)
		{
			result = true;
		}
		m_searchObjects.Clear();
		m_pageTexts.Clear();
		return result;
	}

    /*
	 internal bool FindText(PdfLoadedDocument loadedDocument, List<TextSearchItem> searchItems, int pageIndex, out List<MatchedItem> searchResults)
	{
		SearchInBackground(loadedDocument, searchItems, pageIndex, pageIndex);
		return GetSearchResults(out searchResults);
	}
	 */

    private void InvokeInParallel(params Action[] actions)
	{
		ManualResetEvent[] resetEvents = new ManualResetEvent[actions.Length];
		for (int i = 0; i < actions.Length; i++)
		{
			resetEvents[i] = new ManualResetEvent(initialState: false);
			ThreadPool.QueueUserWorkItem(delegate(object index)
			{
				int num = (int)index;
				actions[num]();
				resetEvents[num].Set();
			}, i);
		}
		ManualResetEvent[] array = resetEvents;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].WaitOne();
		}
	}

	private bool CheckOverlapOrLarger(RectangleF thisRectangle, int thisPageNumber, List<MatchedItem> capturedTermList)
	{
		List<MatchedItem> list = new List<MatchedItem>();
		foreach (MatchedItem capturedTerm in capturedTermList)
		{
			if (thisRectangle.Top <= capturedTerm.Bounds.Bottom && thisRectangle.Bottom >= capturedTerm.Bounds.Top && thisRectangle.Left <= capturedTerm.Bounds.Right && thisRectangle.Right >= capturedTerm.Bounds.Left && thisRectangle.Width <= capturedTerm.Bounds.Width && thisPageNumber == capturedTerm.PageNumber)
			{
				return true;
			}
			if (capturedTerm.Bounds.Top <= thisRectangle.Bottom && capturedTerm.Bounds.Bottom >= thisRectangle.Top && capturedTerm.Bounds.Left <= thisRectangle.Right && capturedTerm.Bounds.Right >= thisRectangle.Left && capturedTerm.Bounds.Width <= thisRectangle.Width && capturedTerm.PageNumber == thisPageNumber)
			{
				list.Add(capturedTerm);
			}
		}
		if (list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				capturedTermList.Remove(list[i]);
			}
		}
		list.Clear();
		return false;
	}

	private bool CheckOverlapOrLarger(RectangleF thisRectangle, int thisPageNumber, MatchedItemCollection capturedTermList)
	{
		MatchedItemCollection matchedItemCollection = new MatchedItemCollection();
		foreach (MatchedItem capturedTerm in capturedTermList)
		{
			if (capturedTerm.Bounds.Contains(thisRectangle) && thisPageNumber == capturedTerm.PageNumber)
			{
				return true;
			}
			if (thisRectangle.Contains(capturedTerm.Bounds) && capturedTerm.PageNumber == thisPageNumber)
			{
				matchedItemCollection.Add(capturedTerm);
			}
		}
		if (matchedItemCollection.Count > 0)
		{
			for (int i = 0; i < matchedItemCollection.Count; i++)
			{
				capturedTermList.Remove(matchedItemCollection[i]);
			}
		}
		matchedItemCollection.Clear();
		return false;
	}

	private void SearchInBackground(PdfLoadedDocument LoadedDocument, List<string> listOfTerms, TextSearchOptions textSearchOption, int startIndex, int endIndex)
	{
		Dictionary<int, List<TextProperties>> matchRect = new Dictionary<int, List<TextProperties>>();
		for (int i = startIndex; i <= endIndex; i++)
		{
			if (!m_pageTexts.ContainsKey(i))
			{
				LoadedDocument.Pages[i].m_visualOrder = false;
				m_pageTexts.Add(i, LoadedDocument.Pages[i].ExtractText(IsLayout: true));
				LoadedDocument.Pages[i].m_visualOrder = true;
			}
			foreach (string listOfTerm in listOfTerms)
			{
				List<string> list = new List<string>();
				if (m_pageTexts.ContainsKey(i) && !string.IsNullOrEmpty(m_pageTexts[i]))
				{
					list = GetMatchedTexts(m_pageTexts[i], listOfTerm);
				}
				foreach (string item2 in list)
				{
					if (m_pageTexts.ContainsKey(i) && m_pageTexts[i] == null)
					{
						continue;
					}
					if ((textSearchOption & TextSearchOptions.CaseSensitive) != TextSearchOptions.CaseSensitive)
					{
						if (!m_pageTexts.ContainsKey(i) || !m_pageTexts[i].ToLower().Contains(item2.ToLower()))
						{
							continue;
						}
						FindTextMatches(LoadedDocument, item2, textSearchOption, i, out matchRect);
					}
					else
					{
						if (!m_pageTexts.ContainsKey(i) || !m_pageTexts[i].Contains(item2))
						{
							continue;
						}
						FindTextMatches(LoadedDocument, item2, textSearchOption, i, out matchRect);
					}
					foreach (KeyValuePair<int, List<TextProperties>> item3 in matchRect)
					{
						foreach (TextProperties item4 in item3.Value)
						{
							MatchedItem item = new MatchedItem
							{
								PageNumber = item3.Key,
								Bounds = item4.Bounds,
								TextColor = item4.StrokingBrush,
								Text = item2
							};
							m_searchObjects.Add(item);
						}
					}
				}
			}
		}
	}

	private void SearchInBackground(PdfLoadedDocument LoadedDocument, List<TextSearchItem> listOfTerms, int startIndex, int endIndex)
	{
		Dictionary<int, List<TextProperties>> matchRect = new Dictionary<int, List<TextProperties>>();
		for (int i = startIndex; i <= endIndex; i++)
		{
			if (!m_pageTexts.ContainsKey(i))
			{
				LoadedDocument.Pages[i].m_visualOrder = false;
				m_pageTexts.Add(i, LoadedDocument.Pages[i].ExtractText(IsLayout: true));
				LoadedDocument.Pages[i].m_visualOrder = true;
			}
			foreach (TextSearchItem listOfTerm in listOfTerms)
			{
				List<TextSearchItem> list = new List<TextSearchItem>();
				if (m_pageTexts.ContainsKey(i) && !string.IsNullOrEmpty(m_pageTexts[i]))
				{
					foreach (string matchedText in GetMatchedTexts(m_pageTexts[i], listOfTerm.SearchWord))
					{
						list.Add(new TextSearchItem(matchedText, listOfTerm.SearchOption));
					}
				}
				foreach (TextSearchItem item2 in list)
				{
					if (m_pageTexts.ContainsKey(i) && m_pageTexts[i] == null)
					{
						continue;
					}
					if ((item2.SearchOption & TextSearchOptions.CaseSensitive) != TextSearchOptions.CaseSensitive)
					{
						if (!m_pageTexts.ContainsKey(i) || !m_pageTexts[i].ToLower().Contains(item2.SearchWord.ToLower()))
						{
							continue;
						}
						FindTextMatches(LoadedDocument, item2.SearchWord, item2.SearchOption, i, out matchRect);
					}
					else
					{
						if (!m_pageTexts.ContainsKey(i) || !m_pageTexts[i].Contains(item2.SearchWord))
						{
							continue;
						}
						FindTextMatches(LoadedDocument, item2.SearchWord, item2.SearchOption, i, out matchRect);
					}
					foreach (KeyValuePair<int, List<TextProperties>> item3 in matchRect)
					{
						foreach (TextProperties item4 in item3.Value)
						{
							MatchedItem item = new MatchedItem
							{
								PageNumber = item3.Key,
								Bounds = item4.Bounds,
								TextColor = item4.StrokingBrush,
								Text = item2.SearchWord
							};
							m_searchObjects.Add(item);
						}
					}
				}
			}
		}
	}

	internal bool SearchText(Page page, PdfLoadedDocument loadedDocument, int pageIndex, string searchText, out List<DocGen.PdfViewer.Base.Glyph> texts)
	{
		searchstring = searchText;
		texts = new List<DocGen.PdfViewer.Base.Glyph>();
		bool result = false;
		_ = page.Height;
		_ = page.Width;
		if (page.Rotation == 90.0 || page.Rotation == 270.0)
		{
			_ = page.Height;
			_ = page.Width;
		}
		if (m_cmyk == null)
		{
			m_cmyk = new DeviceCMYK();
		}
		if (page.RecordCollection == null)
		{
			page.Initialize(loadedDocument.Pages[pageIndex], needParsing: true);
		}
		ImageRenderer imageRenderer = new ImageRenderer(page.RecordCollection, page.Resources, page.Height, page.CurrentLeftLocation, m_cmyk);
		CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		imageRenderer.pageRotation = (float)page.Rotation;
		imageRenderer.IsTextSearch = true;
		imageRenderer.isFindText = true;
		imageRenderer.RenderAsImage();
		imageRenderer.isFindText = false;
		imageRenderer.IsTextSearch = false;
		Thread.CurrentThread.CurrentCulture = currentCulture;
		foreach (DocGen.PdfViewer.Base.Glyph imageRenderGlyph in imageRenderer.imageRenderGlyphList)
		{
			if (imageRenderGlyph.ToString() != "" || !string.IsNullOrEmpty(imageRenderGlyph.ToString()))
			{
				texts.Add(imageRenderGlyph);
			}
		}
		return result;
	}

	internal bool FindTextMatches(PdfLoadedDocument loadedDocument, string text, TextSearchOptions textSearchOption, int pageIndex, out Dictionary<int, List<TextProperties>> matchRect)
	{
		bool result = false;
		matchRect = new Dictionary<int, List<TextProperties>>();
		List<TextProperties> list = new List<TextProperties>();
		Page page = new Page(loadedDocument.Pages[pageIndex]);
		page.Initialize(loadedDocument.Pages[pageIndex], needParsing: false);
		if (pageIndex >= 0)
		{
			string text2 = null;
			List<int> list2 = new List<int>();
			if (m_cmyk == null)
			{
				m_cmyk = new DeviceCMYK();
			}
			List<DocGen.PdfViewer.Base.Glyph> texts;
			lock (m_lock)
			{
				SearchText(page, loadedDocument, pageIndex, text, out texts);
			}
			foreach (DocGen.PdfViewer.Base.Glyph item in texts)
			{
				text2 += item.ToUnicode;
			}
			if ((textSearchOption & TextSearchOptions.CaseSensitive) != TextSearchOptions.CaseSensitive)
			{
				if (!string.IsNullOrEmpty(text))
				{
					text = text.ToLower();
				}
				if (!string.IsNullOrEmpty(text2))
				{
					text2 = text2.ToLower();
				}
			}
			int num = 0;
			if (text2 != null && text != null && text != "")
			{
				while (text2.Contains(text))
				{
					int num2 = text2.IndexOf(text, StringComparison.Ordinal);
					if ((textSearchOption & TextSearchOptions.WholeWords) == TextSearchOptions.WholeWords)
					{
						if (Regex.IsMatch(text2, "\\b" + text + "\\b"))
						{
							num2 = Regex.Match(text2, "\\b" + text + "\\b").Index;
							m_wholeWords = true;
						}
						else if (!CheckGlyphBoundsArea(num2, texts, textSearchOption, text, num))
						{
							m_wholeWords = false;
						}
					}
					text2 = text2.Substring(num2 + text.Length);
					num += num2 + text.Length;
					if ((textSearchOption & TextSearchOptions.WholeWords) == TextSearchOptions.WholeWords)
					{
						if (m_wholeWords)
						{
							if (list2.Count == 0)
							{
								list2.Add(num2);
							}
							else
							{
								list2.Add(list2[list2.Count - 1] + num2 + text.Length);
							}
						}
					}
					else if (list2.Count == 0)
					{
						list2.Add(num2);
					}
					else if (!m_wholeWords || Bidi.HasAnyRTL(text) || textSearchOption != TextSearchOptions.WholeWords)
					{
						list2.Add(list2[list2.Count - 1] + num2 + text.Length);
					}
				}
			}
			if (list2.Count != 0)
			{
				foreach (int item2 in list2)
				{
					double num3 = texts[item2].BoundingRect._x;
					double num4 = texts[item2].BoundingRect._y;
					double num5 = 0.0;
					double num6 = texts[item2].BoundingRect._height;
					if (texts[item2].BoundingRect._y == texts[item2 + text.Length - 1].BoundingRect._y || Math.Abs(texts[item2].BoundingRect._y - texts[item2 + text.Length - 1].BoundingRect._y) < 0.001)
					{
						if (num3 > texts[item2 + text.Length - 1].BoundingRect._x)
						{
							num5 = num3 - texts[item2 + text.Length - 1].BoundingRect._x + texts[item2 + text.Length - 1].BoundingRect._width;
							if (page.Rotation == 0.0 || page.Rotation == 180.0)
							{
								num5 = texts[item2].BoundingRect._height;
								for (int i = 0; i < text.Length; i++)
								{
									num6 += texts[item2 + i].BoundingRect._width;
									if (texts[item2 + i].BoundingRect._height > num5)
									{
										num5 = texts[item2 + i].BoundingRect._height;
									}
								}
							}
							else
							{
								num5 = texts[item2].BoundingRect.Width;
								for (int j = 0; j < text.Length; j++)
								{
									num6 += texts[item2 + j].BoundingRect._height;
									if (texts[item2 + j].BoundingRect._width > num5)
									{
										num5 = texts[item2 + j].BoundingRect._width;
									}
								}
							}
							num3 -= num5;
						}
						else
						{
							num5 = texts[item2 + text.Length - 1].BoundingRect._x - num3 + texts[item2 + text.Length - 1].BoundingRect._width;
						}
						RectangleF bounds = new RectangleF((float)num3, (float)num4, (float)num5, (float)num6);
						if (page != null && page.CropBox.X != 0f && page.CropBox.Y != 0f)
						{
							bounds = new RectangleF((float)num3 - page.CropBox.X, (float)num4 + page.CropBox.Y, (float)num5, (float)num6);
						}
						list.Add(new TextProperties(bounds));
						result = true;
					}
					else
					{
						if (texts[item2].BoundingRect._x != texts[item2 + text.Length - 1].BoundingRect._x && !(Math.Abs(texts[item2].BoundingRect._x - texts[item2 + text.Length - 1].BoundingRect._x) < 0.001))
						{
							continue;
						}
						if ((texts[item2].BoundingRect._y != texts[item2 + text.Length - 1].BoundingRect._y && !(Math.Abs(texts[item2].BoundingRect._y - texts[item2 + text.Length - 1].BoundingRect._y) < 0.001)) || texts[item2].IsRotated)
						{
							num6 = 0.0;
							double num7 = 0.0;
							if (page.Rotation == 0.0 || page.Rotation == 180.0)
							{
								num5 = texts[item2].BoundingRect._height;
								for (int k = 0; k < text.Length; k++)
								{
									num6 += texts[item2 + k].BoundingRect._width;
									if (texts[item2 + k].BoundingRect._height > num7)
									{
										num5 = texts[item2 + k].BoundingRect._height;
									}
								}
							}
							else
							{
								num5 = texts[item2].BoundingRect.Width;
								for (int l = 0; l < text.Length; l++)
								{
									num6 += texts[item2 + l].BoundingRect._height;
									num5 = texts[item2 + l].BoundingRect._width;
								}
							}
							if (num4 > texts[item2 + text.Length - 1].BoundingRect._y || texts[item2].RotationAngle == 270)
							{
								num3 = texts[item2].BoundingRect._x - num5 + 1.0;
								num4 = texts[item2].BoundingRect._y - num6;
							}
							else if (num4 < texts[item2 + text.Length - 1].BoundingRect._y || texts[item2].RotationAngle == 90)
							{
								num3 = texts[item2].BoundingRect._x - 1.0;
								num4 = texts[item2].BoundingRect._y;
							}
						}
						RectangleF bounds2 = new RectangleF((float)num3, (float)num4, (float)num5, (float)num6 + 1f);
						if (page != null && page.CropBox.X != 0f && page.CropBox.Y != 0f)
						{
							bounds2 = new RectangleF((float)num3 - page.CropBox.X, (float)num4 + page.CropBox.Y, (float)num5, (float)num6);
						}
						list.Add(new TextProperties(bounds2));
						result = true;
					}
				}
			}
		}
		matchRect.Add(pageIndex, list);
		return result;
	}

	private bool CheckGlyphBoundsArea(int targetTextIndex, List<DocGen.PdfViewer.Base.Glyph> txtMatchs, TextSearchOptions textSearchOption, string text, int index)
	{
		string text2 = string.Empty;
		int num = targetTextIndex + index;
		Rect rect = txtMatchs[num].BoundingRect;
		for (int i = num; i < txtMatchs.Count; i++)
		{
			Rect boundingRect = txtMatchs[i].BoundingRect;
			if (!string.IsNullOrEmpty(text2))
			{
				double num2 = Math.Round(rect.Width + rect.X, 2);
				bool flag = (int)Math.Round(boundingRect.X, 2) == (int)num2;
				if (!(boundingRect.Top == rect.Top && boundingRect.Bottom == rect.Bottom && flag))
				{
					break;
				}
				text2 += txtMatchs[i].ToUnicode;
				if (text2.Length > text.Length + 2)
				{
					break;
				}
			}
			else
			{
				text2 += txtMatchs[i].ToUnicode;
			}
			rect = boundingRect;
		}
		if ((textSearchOption & TextSearchOptions.CaseSensitive) != TextSearchOptions.CaseSensitive && !string.IsNullOrEmpty(text2))
		{
			text2 = text2.ToLower();
		}
		if (Regex.IsMatch(text2, "\\b" + text + "\\b"))
		{
			return true;
		}
		return false;
	}

	internal bool FindTextMatches(int pageIndex, PdfLoadedDocument loadedDocument, string text, TextSearchOptions textSearchoptions, out List<RectangleF> matchRectangles)
	{
		bool result = false;
		matchRectangles = new List<RectangleF>();
		if (text != "")
		{
			int num = pageIndex;
			PdfPageBase page = loadedDocument.Pages[num];
			Page page2 = new Page(page);
			page2.Initialize(page, needParsing: false);
			page2.matchTextPositions.Clear();
			if (num < loadedDocument.Pages.Count && num >= 0)
			{
				if (m_cmyk == null)
				{
					m_cmyk = new DeviceCMYK();
				}
				if (page2.RecordCollection == null)
				{
					page2.Initialize(loadedDocument.Pages[num], needParsing: true);
				}
				ImageRenderer imageRenderer = new ImageRenderer(page2.RecordCollection, page2.Resources, page2.Height, page2.CurrentLeftLocation, m_cmyk);
				imageRenderer.pageRotation = (float)page2.Rotation;
				imageRenderer.isFindText = true;
				imageRenderer.RenderAsImage();
				imageRenderer.isFindText = false;
				string text2 = string.Empty;
				Dictionary<int, string> dictionary = new Dictionary<int, string>();
				for (int i = 0; i < imageRenderer.imageRenderGlyphList.Count; i++)
				{
					if (imageRenderer.imageRenderGlyphList[i].ToUnicode == "")
					{
						imageRenderer.imageRenderGlyphList.RemoveAt(i);
						i--;
					}
					else
					{
						text2 += imageRenderer.imageRenderGlyphList[i].ToUnicode;
					}
				}
				if ((textSearchoptions & TextSearchOptions.CaseSensitive) != TextSearchOptions.CaseSensitive)
				{
					if (text != "")
					{
						text = text.ToLower();
					}
					if (!string.IsNullOrEmpty(text2))
					{
						text2 = text2.ToLower();
					}
				}
				List<string> matchedTexts = GetMatchedTexts(text2, text);
				bool flag = false;
				for (int j = 0; j < matchedTexts.Count; j++)
				{
					string text3 = matchedTexts[j];
					if (text2.Contains(text) || !text.Contains(" ") || !(text != " "))
					{
						continue;
					}
					text3 = text3.Replace(" ", "");
					if (!text2.Contains(text3))
					{
						continue;
					}
					flag = true;
					matchedTexts[j] = text3;
					if (text3.Length >= 3)
					{
						int num2 = text2.IndexOf(text3, StringComparison.Ordinal);
						int num3 = (int)imageRenderer.imageRenderGlyphList[num2].BoundingRect._x;
						int num4 = (int)imageRenderer.imageRenderGlyphList[num2 + 1].BoundingRect._x;
						int num5 = (int)imageRenderer.imageRenderGlyphList[num2 + 2].BoundingRect._x;
						if (num3 == num4 && num3 == num5)
						{
							flag = false;
						}
					}
				}
				if (text2 != null && matchedTexts.Count > 0)
				{
					foreach (string item3 in matchedTexts)
					{
						if (item3.Length == 0)
						{
							continue;
						}
						while (text2.Contains(item3))
						{
							int num6 = text2.IndexOf(item3, StringComparison.Ordinal);
							if ((textSearchoptions & TextSearchOptions.WholeWords) == TextSearchOptions.WholeWords && Regex.IsMatch(text2, "\\b" + item3 + "\\b"))
							{
								num6 = Regex.Match(text2, "\\b" + item3 + "\\b").Index;
							}
							text2 = text2.Substring(num6 + item3.Length);
							if (dictionary.Count == 0)
							{
								dictionary.Add(num6, item3);
							}
							else
							{
								dictionary[dictionary.Last().Key + num6 + item3.Length] = item3;
							}
						}
					}
				}
				if (dictionary.Count != 0)
				{
					foreach (KeyValuePair<int, string> item4 in dictionary)
					{
						int key = item4.Key;
						text = item4.Value;
						double num7 = imageRenderer.imageRenderGlyphList[key].BoundingRect._x;
						double num8 = imageRenderer.imageRenderGlyphList[key].BoundingRect._y;
						double num9 = 0.0;
						double num10 = imageRenderer.imageRenderGlyphList[key].BoundingRect._height;
						if (imageRenderer.imageRenderGlyphList[key].BoundingRect._y == imageRenderer.imageRenderGlyphList[key + text.Length - 1].BoundingRect._y || Math.Abs(imageRenderer.imageRenderGlyphList[key].BoundingRect._y - imageRenderer.imageRenderGlyphList[key + text.Length - 1].BoundingRect._y) < 0.001 || (imageRenderer.imageRenderGlyphList[key].BoundingRect.Top <= imageRenderer.imageRenderGlyphList[key + text.Length - 1].BoundingRect.Top && imageRenderer.imageRenderGlyphList[key].BoundingRect.Bottom >= imageRenderer.imageRenderGlyphList[key + text.Length - 1].BoundingRect.Bottom) || flag)
						{
							num9 = ((!(num7 > imageRenderer.imageRenderGlyphList[key + text.Length - 1].BoundingRect._x)) ? (imageRenderer.imageRenderGlyphList[key + text.Length - 1].BoundingRect._x - num7 + imageRenderer.imageRenderGlyphList[key + text.Length - 1].BoundingRect._width) : (num7 - imageRenderer.imageRenderGlyphList[key + text.Length - 1].BoundingRect._x + imageRenderer.imageRenderGlyphList[key + text.Length - 1].BoundingRect._width));
							RectangleF item = new RectangleF((float)num7, (float)num8, (float)num9, (float)num10);
							if (page2 != null && page2.CropBox.X != 0f && page2.CropBox.Y != 0f)
							{
								item = new RectangleF((float)num7 - page2.CropBox.X, (float)num8 + page2.CropBox.Y, (float)num9, (float)num10);
							}
							matchRectangles.Add(item);
							result = true;
						}
						else
						{
							if (imageRenderer.imageRenderGlyphList[key].BoundingRect._x != imageRenderer.imageRenderGlyphList[key + text.Length - 1].BoundingRect._x && !(Math.Abs(imageRenderer.imageRenderGlyphList[key].BoundingRect._x - imageRenderer.imageRenderGlyphList[key + text.Length - 1].BoundingRect._x) < 0.001))
							{
								continue;
							}
							if ((imageRenderer.imageRenderGlyphList[key].BoundingRect._y != imageRenderer.imageRenderGlyphList[key + text.Length - 1].BoundingRect._y && !(Math.Abs(imageRenderer.imageRenderGlyphList[key].BoundingRect._y - imageRenderer.imageRenderGlyphList[key + text.Length - 1].BoundingRect._y) < 0.001)) || imageRenderer.imageRenderGlyphList[key].IsRotated)
							{
								num10 = 0.0;
								double num11 = 0.0;
								if (page2.Rotation == 0.0 || page2.Rotation == 180.0)
								{
									num9 = imageRenderer.imageRenderGlyphList[key].BoundingRect._height;
									for (int k = 0; k < text.Length; k++)
									{
										num10 += imageRenderer.imageRenderGlyphList[key + k].BoundingRect._width;
										if (imageRenderer.imageRenderGlyphList[key + k].BoundingRect._height > num11)
										{
											num9 = imageRenderer.imageRenderGlyphList[key + k].BoundingRect._height;
										}
									}
								}
								else
								{
									num9 = imageRenderer.imageRenderGlyphList[key].BoundingRect.Width;
									for (int l = 0; l < text.Length; l++)
									{
										num10 += imageRenderer.imageRenderGlyphList[key + l].BoundingRect._height;
										num9 = imageRenderer.imageRenderGlyphList[key + l].BoundingRect._width;
									}
								}
								if (num8 > imageRenderer.imageRenderGlyphList[key + text.Length - 1].BoundingRect._y || imageRenderer.imageRenderGlyphList[key].RotationAngle == 270)
								{
									num7 = imageRenderer.imageRenderGlyphList[key].BoundingRect._x - num9 + 1.0;
									num8 = imageRenderer.imageRenderGlyphList[key].BoundingRect._y - num10;
								}
								else if (num8 < imageRenderer.imageRenderGlyphList[key + text.Length - 1].BoundingRect._y || imageRenderer.imageRenderGlyphList[key].RotationAngle == 90)
								{
									num7 = imageRenderer.imageRenderGlyphList[key].BoundingRect._x - 1.0;
									num8 = imageRenderer.imageRenderGlyphList[key].BoundingRect._y;
								}
							}
							RectangleF item2 = new RectangleF((float)num7, (float)num8, (float)num9, (float)num10 + 1f);
							if (page2 != null && page2.CropBox.X != 0f && page2.CropBox.Y != 0f)
							{
								item2 = new RectangleF((float)num7 - page2.CropBox.X, (float)num8 + page2.CropBox.Y, (float)num9, (float)num10);
							}
							matchRectangles.Add(item2);
							result = true;
						}
					}
				}
				imageRenderer.imageRenderGlyphList.Clear();
				num++;
			}
		}
		return result;
	}

	private List<string> GetMatchedTexts(string pageText, string text)
	{
		return GetMatchedTexts(pageText, new List<string> { text });
	}

	private List<string> GetMatchedTexts(string pageText, List<string> searchText)
	{
		pageText = pageText.ToLower();
		List<string> list = new List<string>();
		if (searchText != null && searchText.Count > 0)
		{
			for (int i = 0; i < searchText.Count; i++)
			{
				string item = searchText[i];
				string text = searchText[i].ToLower();
				if (pageText.Contains(text) && !list.Contains(text))
				{
					list.Add(item);
				}
				if (string.IsNullOrEmpty(text) || !Bidi.HasAnyRTL(text))
				{
					continue;
				}
				Bidi bidi = new Bidi();
				if (m_arabicShapeRenderer == null)
				{
					m_arabicShapeRenderer = new ArabicShapeRenderer();
				}
				string logicalToVisualString = bidi.GetLogicalToVisualString(text, isRTL: true);
				if (pageText.Contains(logicalToVisualString) && !list.Contains(logicalToVisualString))
				{
					list.Add(logicalToVisualString);
				}
				string inputText = m_arabicShapeRenderer.Shape(text.ToCharArray(), 0);
				inputText = bidi.GetLogicalToVisualString(inputText, isRTL: true);
				if (pageText.Contains(inputText) && !list.Contains(inputText))
				{
					list.Add(inputText);
				}
				if (!(logicalToVisualString != inputText) || logicalToVisualString.Length >= 5)
				{
					continue;
				}
				List<char[]> list2 = new List<char[]>();
				for (int j = 0; j < logicalToVisualString.Length; j++)
				{
					if (m_arabicShapeRenderer.ArabicMapTable.ContainsKey(logicalToVisualString[j]))
					{
						list2.Add(m_arabicShapeRenderer.ArabicMapTable[logicalToVisualString[j]]);
						continue;
					}
					list2.Clear();
					list2 = null;
					break;
				}
				if (list2 != null && list2.Count > 0)
				{
					GetAllMatchedArabicText(list2, 0, "", pageText, list);
				}
			}
		}
		if (list.Count != 0)
		{
			return list;
		}
		return searchText;
	}

	private List<string> GetAllMatchedArabicText(List<char[]> mapTable, int i, string renderedTerm, string pageText, List<string> searchTexts)
	{
		char[] array = mapTable[i];
		for (int j = 0; j < array.Length; j++)
		{
			char c = array[j];
			if (pageText.Contains(renderedTerm + c))
			{
				if (i == mapTable.Count - 1 && !searchTexts.Contains(renderedTerm + c))
				{
					searchTexts.Add(renderedTerm + c);
				}
				else if (i + 1 < mapTable.Count)
				{
					searchTexts = GetAllMatchedArabicText(mapTable, i + 1, renderedTerm + c, pageText, searchTexts);
				}
			}
		}
		return searchTexts;
	}

	internal bool FindTextMatches(PdfLoadedDocument loadedDocument, string text, out Dictionary<int, List<RectangleF>> matchTextPositionsDict)
	{
		if (text != "")
		{
			LoadPagesForTextSearch(loadedDocument);
		}
		bool result = false;
		int num = 0;
		matchTextPositionsDict = new Dictionary<int, List<RectangleF>>();
		foreach (Page item in pagesForTextSearch)
		{
			item.matchTextPositions.Clear();
		}
		while (num < pagesForTextSearch.Count)
		{
			if (num < pagesForTextSearch.Count && num >= 0)
			{
				if (FindTextMatches(num, loadedDocument, text, TextSearchOptions.None, out List<RectangleF> matchRectangles))
				{
					result = true;
				}
				matchTextPositionsDict.Add(num, matchRectangles);
				num++;
			}
		}
		return result;
	}
}

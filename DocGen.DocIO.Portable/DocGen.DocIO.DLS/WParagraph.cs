using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DocGen.Compression.Zip;
using DocGen.DocIO.DLS.Rendering;
using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.DataStreamParser.OLEObject;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;
using DocGen.Office;

namespace DocGen.DocIO.DLS;

public class WParagraph : TextBodyItem, IWidgetContainer, IWidget, IWParagraph, ITextBodyItem, IEntity, IStyleHolder, ICompositeEntity
{
	internal class LayoutParagraphInfoImpl : ParagraphLayoutInfo
	{
		private WParagraph m_paragraph;

		private IEntity m_nextSibling;

		private IEntity m_prevSibling;

		protected IWordDocument Document => m_paragraph.Document;

		public LayoutParagraphInfoImpl(WParagraph paragraph)
			: base(ChildrenLayoutDirection.Horizontal)
		{
			base.IsLineContainer = true;
			if (paragraph.IsPreviousParagraphInDeleteRevision() && paragraph.ListFormat.ListType != ListType.NoList)
			{
				IEntity prevEntity = null;
				if (paragraph.IsPreviousParagraphMarkIsInDeletion(ref prevEntity) && prevEntity is WParagraph && (prevEntity as WParagraph).m_layoutInfo != null)
				{
					base.Justification = ((prevEntity as WParagraph).m_layoutInfo as ParagraphLayoutInfo).Justification;
				}
				base.SkipBottomBorder = (base.SkipLeftBorder = (base.SkipRightBorder = (base.SkipTopBorder = (base.SkipHorizonatalBorder = true))));
				return;
			}
			IEntity nextListPara = null;
			if (paragraph.BreakCharacterFormat != null && paragraph.IsParagraphMarkDeleted() && !paragraph.IsDeletionParagraph() && paragraph.GetNextNonDeleteRevisionListParagraph(ref nextListPara) && nextListPara != null)
			{
				m_paragraph = nextListPara as WParagraph;
				m_nextSibling = (nextListPara as WParagraph).GetPreviousOrNextNonDeleteRevisionEntity(isPrevious: false);
				m_prevSibling = paragraph.GetPreviousOrNextNonDeleteRevisionEntity(isPrevious: true);
				base.IsSectionEndMark = (nextListPara as WParagraph).SectionEndMark;
			}
			else
			{
				m_paragraph = paragraph;
				m_nextSibling = paragraph.NextSibling;
				m_prevSibling = paragraph.PreviousSibling;
				base.IsSectionEndMark = paragraph.SectionEndMark;
			}
			base.Size = GetEmptyTextRangeSize();
			UpdateLayoutInfoIsClipped();
			InitFormat();
			InitPageBreaks();
			InitListFormat();
			InitBorders();
		}

		private SizeF GetEmptyTextRangeSize()
		{
			return DocumentLayouter.DrawingContext.MeasureString(" ", m_paragraph.BreakCharacterFormat.GetFontToRender(FontScriptType.English), null, m_paragraph.BreakCharacterFormat, isMeasureFromTabList: false, FontScriptType.English);
		}

		private void UpdateLayoutInfoIsClipped()
		{
			if (m_paragraph.IsInCell)
			{
				WTableCell wTableCell = m_paragraph.GetOwnerEntity() as WTableCell;
				float rowHeight = (wTableCell.Owner as WTableRow).Height;
				if (m_paragraph.IsExactlyRowHeight(wTableCell, ref rowHeight))
				{
					if (rowHeight < 0f)
					{
						rowHeight = 0f - rowHeight;
					}
					if (rowHeight > 1f)
					{
						base.IsClipped = true;
					}
				}
				if (wTableCell.CellFormat.TextDirection == TextDirection.VerticalBottomToTop || wTableCell.CellFormat.TextDirection == TextDirection.VerticalTopToBottom)
				{
					base.IsClipped = true;
				}
				WParagraph firstParagraphInOwnerTextbody = m_paragraph.GetFirstParagraphInOwnerTextbody(m_paragraph.OwnerTextBody);
				if (firstParagraphInOwnerTextbody != null && firstParagraphInOwnerTextbody.ParagraphFormat.IsFrame && firstParagraphInOwnerTextbody.ParagraphFormat.FrameHeight != 0f && ((ushort)(firstParagraphInOwnerTextbody.ParagraphFormat.FrameHeight * 20f) & 0x8000) == 0)
				{
					base.IsClipped = true;
				}
			}
			else if (m_paragraph != null && m_paragraph.Owner != null && (GetBaseEntity(m_paragraph) is WTextBox || GetBaseEntity(m_paragraph) is Shape || GetBaseEntity(m_paragraph) is ChildShape))
			{
				base.IsClipped = true;
			}
			else if (m_paragraph != null && m_paragraph.ParagraphFormat.IsFrame && m_paragraph.ParagraphFormat.FrameHeight != 0f && ((ushort)(m_paragraph.ParagraphFormat.FrameHeight * 20f) & 0x8000) == 0)
			{
				base.IsClipped = true;
			}
		}

		private Entity GetBaseEntity(Entity entity)
		{
			if (entity == null)
			{
				return null;
			}
			Entity entity2 = entity;
			while (!(entity2 is WSection) && !(entity2 is WTextBox) && !(entity2 is Shape) && !(entity2 is ChildShape) && entity2.Owner != null)
			{
				entity2 = entity2.Owner;
			}
			return entity2;
		}

		private void InitListFormat()
		{
			if (m_paragraph.ListFormat.IsEmptyList || base.IsSectionEndMark)
			{
				return;
			}
			int tabLevelIndex = 0;
			if (m_paragraph.ParagraphFormat.Tabs.Count > 0)
			{
				tabLevelIndex++;
			}
			WListLevel listLevel = null;
			float? leftIndent = null;
			float? firstLineIndent = null;
			WListFormat listFormat = m_paragraph.GetListFormat(ref listLevel, ref tabLevelIndex, ref leftIndent, ref firstLineIndent);
			WParagraphStyle wParagraphStyle = m_paragraph.ParaStyle as WParagraphStyle;
			if (listFormat == null || listLevel == null)
			{
				return;
			}
			ListStyle currentListStyle = listFormat.CurrentListStyle;
			base.LevelNumber = listLevel.LevelNumber;
			if (currentListStyle.ListType == ListType.Numbered || currentListStyle.ListType == ListType.Bulleted)
			{
				if (leftIndent.HasValue)
				{
					base.Margins.Left = leftIndent.Value;
				}
				if (firstLineIndent.HasValue)
				{
					base.FirstLineIndent = firstLineIndent.Value;
				}
				if (base.FirstLineIndent < 0f && base.Margins.Left == 0f && !m_paragraph.ParagraphFormat.HasValue(2) && !listLevel.ParagraphFormat.HasValue(2) && wParagraphStyle != null && !wParagraphStyle.ParagraphFormat.HasValue(2))
				{
					base.Margins.Left = Math.Abs(base.FirstLineIndent);
				}
			}
			base.ListValue = (listLevel.CharacterFormat.Hidden ? string.Empty : (Document as WordDocument).UpdateListValue(m_paragraph, listFormat, listLevel));
			base.CurrentListType = listFormat.ListType;
			if (listLevel.PatternType == ListPatternType.Bullet)
			{
				base.CurrentListType = ListType.Bulleted;
			}
			else if (listFormat.ListType == ListType.Bulleted && listLevel.PatternType != ListPatternType.Bullet)
			{
				base.CurrentListType = ListType.Numbered;
			}
			base.CharacterFormat = new WCharacterFormat(Document);
			base.CharacterFormat.ImportContainer(m_paragraph.BreakCharacterFormat);
			base.CharacterFormat.CopyProperties(m_paragraph.BreakCharacterFormat);
			base.CharacterFormat.ApplyBase(m_paragraph.BreakCharacterFormat.BaseFormat);
			if (base.CharacterFormat.PropertiesHash.ContainsKey(7) && !base.CharacterFormat.Compare(base.CharacterFormat.PropertiesHash[7], UnderlineStyle.None))
			{
				base.CharacterFormat.UnderlineStyle = UnderlineStyle.None;
				base.CharacterFormat.PropertiesHash.Remove(7);
			}
			else if (base.CharacterFormat.CharStyle != null && base.CharacterFormat.CharStyle.CharacterFormat.PropertiesHash.ContainsKey(7))
			{
				base.CharacterFormat.UnderlineStyle = UnderlineStyle.None;
			}
			if (base.CurrentListType == ListType.Bulleted)
			{
				base.CharacterFormat.Bold = false;
				base.CharacterFormat.BoldBidi = false;
				base.CharacterFormat.Italic = false;
				base.CharacterFormat.ItalicBidi = false;
			}
			CopyCharacterFormatting(listLevel.CharacterFormat, base.CharacterFormat);
			if (base.CurrentListType == ListType.Numbered && m_paragraph.ParagraphFormat.Bidi && (m_paragraph.BreakCharacterFormat.LocaleIdBidi == 1037 || m_paragraph.BreakCharacterFormat.LocaleIdBidi == 1085))
			{
				base.CharacterFormat.FontName = base.CharacterFormat.GetFontNameBidiToRender(FontScriptType.English);
			}
			base.ListFont = new SyncFont(base.CharacterFormat.GetFontToRender(FontScriptType.English));
			if (base.FirstLineIndent == 0f && listLevel.CharacterFormat.Hidden)
			{
				base.ListTab = 0f;
			}
			else if (listLevel.FollowCharacter == FollowCharacterType.Tab)
			{
				UpdateListTab(listLevel, tabLevelIndex);
			}
			else
			{
				UpdateListWidth(listLevel);
			}
			base.ListAlignment = listLevel.NumberAlignment;
			if (!m_paragraph.Document.DOP.Dop2000.Copts.DontUseHTMLParagraphAutoSpacing)
			{
				UpdateListParagraphSpacing(listFormat);
			}
		}

		private void UpdateListParagraphSpacing(WListFormat currentListFormat)
		{
			if (m_nextSibling != null && m_nextSibling is WParagraph && m_paragraph.ParagraphFormat.SpaceAfterAuto)
			{
				WParagraph obj = m_nextSibling as WParagraph;
				_ = obj.ParaStyle;
				WListFormat listFormatValue = obj.GetListFormatValue();
				if (!obj.ListFormat.IsEmptyList && listFormatValue != null && listFormatValue.CurrentListStyle != null && listFormatValue.CurrentListStyle == currentListFormat.CurrentListStyle)
				{
					base.Margins.Bottom = 0f;
				}
			}
			if (m_prevSibling != null && m_prevSibling is WParagraph && m_paragraph.ParagraphFormat.SpaceBeforeAuto)
			{
				WParagraph obj2 = m_prevSibling as WParagraph;
				_ = obj2.ParaStyle;
				WListFormat listFormatValue2 = obj2.GetListFormatValue();
				if (!obj2.ListFormat.IsEmptyList && listFormatValue2 != null && listFormatValue2.CurrentListStyle != null && listFormatValue2.CurrentListStyle == currentListFormat.CurrentListStyle)
				{
					base.Margins.Top = 0f;
				}
			}
		}

		private SizeF GetListValueSize(WListLevel level)
		{
			DrawingContext drawingContext = DocumentLayouter.DrawingContext;
			DocGen.Drawing.Font font = base.ListFont.GetFont(level.Document, FontScriptType.English);
			if (base.CurrentListType == ListType.Bulleted && level.PicBullet != null)
			{
				return drawingContext.MeasurePictureBulletSize(level.PicBullet, font);
			}
			return drawingContext.MeasureString(base.ListValue, font, null, base.CharacterFormat, isMeasureFromTabList: false, FontScriptType.English);
		}

		private void UpdateListWidth(WListLevel level)
		{
			DrawingContext drawingContext = DocumentLayouter.DrawingContext;
			SizeF listValueSize = GetListValueSize(level);
			float num = 0f;
			if (level.NumberAlignment == ListNumberAlignment.Left)
			{
				num = listValueSize.Width;
			}
			else if (level.NumberAlignment == ListNumberAlignment.Center)
			{
				num = listValueSize.Width / 2f;
			}
			if (level.FollowCharacter == FollowCharacterType.Space)
			{
				base.ListTab = num + drawingContext.MeasureString(" ", base.ListFont.GetFont(level.Document, FontScriptType.English), null, FontScriptType.English).Width;
			}
			else
			{
				base.ListTab = num;
			}
		}

		private void UpdateListTab(WListLevel level, int tabLevelIndex)
		{
			ListTabs listTabs = new ListTabs(m_paragraph);
			listTabs.SortParagraphTabsCollection(m_paragraph.ParagraphFormat, level.ParagraphFormat.Tabs, tabLevelIndex);
			SizeF listValueSize = GetListValueSize(level);
			if (level.NumberAlignment == ListNumberAlignment.Left)
			{
				UpdateTabWidth(level, listTabs, listValueSize.Width);
			}
			else if (level.NumberAlignment == ListNumberAlignment.Center)
			{
				UpdateTabWidth(level, listTabs, listValueSize.Width / 2f);
			}
			else
			{
				UpdateTabWidth(level, listTabs, 0f);
			}
			base.ListTabStop = listTabs.m_currTab;
		}

		private void UpdateTabWidth(WListLevel level, ListTabs tabs, float width)
		{
			if (!(base.ListTab <= width) && (Document as WordDocument).UseHangingIndentAsListTab)
			{
				return;
			}
			float num = width + base.Margins.Left + base.FirstLineIndent;
			float num2 = (float)tabs.GetNextTabPosition(num);
			if (base.FirstLineIndent < 0f && width <= Math.Abs(base.FirstLineIndent) && (Document as WordDocument).UseHangingIndentAsListTab)
			{
				if (tabs.m_currTab.Position != 0f)
				{
					base.ListTab = Math.Min(width + num2, Math.Abs(base.FirstLineIndent));
					if (Math.Abs(base.FirstLineIndent) < tabs.m_currTab.Position)
					{
						tabs.m_currTab.TabLeader = DocGen.Layouting.TabLeader.NoLeader;
					}
				}
				else
				{
					base.ListTab = Math.Abs(base.FirstLineIndent);
				}
			}
			else if (level.Word6Legacy)
			{
				float num3 = level.LegacyIndent / 20;
				if (width + (float)(level.LegacySpace / 20) > num3)
				{
					num3 = width + (float)(level.LegacySpace / 20);
				}
				base.ListTab = Math.Max(num3, (base.FirstLineIndent < 0f) ? Math.Abs(base.FirstLineIndent) : 0f);
			}
			else if (base.FirstLineIndent < 0f && tabs.m_currTab.Position == 0f && width <= Math.Abs(base.FirstLineIndent))
			{
				base.ListTab = Math.Min(width + num2, Math.Abs(base.FirstLineIndent));
			}
			else
			{
				base.ListTab = width + num2;
			}
			if (width == 0f && base.ListTab == 0f && base.FirstLineIndent == 0f)
			{
				base.ListTab = num2;
			}
			base.ListTabWidth = num2;
		}

		private void CopyCharacterFormatting(WCharacterFormat sourceFormat, WCharacterFormat destFormat)
		{
			if (sourceFormat.HasValue(6))
			{
				destFormat.Strikeout = sourceFormat.Strikeout;
			}
			if (sourceFormat.HasValue(90))
			{
				destFormat.UnderlineColor = sourceFormat.UnderlineColor;
			}
			if (sourceFormat.HasValue(3))
			{
				destFormat.SetPropertyValue(3, sourceFormat.FontSize);
			}
			if (sourceFormat.HasValue(1))
			{
				destFormat.TextColor = sourceFormat.TextColor;
			}
			if (sourceFormat.HasValue(2))
			{
				destFormat.FontName = sourceFormat.FontName;
			}
			if (sourceFormat.HasValue(4))
			{
				destFormat.Bold = sourceFormat.Bold;
			}
			if (sourceFormat.HasValue(5))
			{
				destFormat.Italic = sourceFormat.Italic;
			}
			if (sourceFormat.HasValue(7))
			{
				destFormat.UnderlineStyle = sourceFormat.UnderlineStyle;
			}
			if (sourceFormat.HasValue(63))
			{
				destFormat.HighlightColor = sourceFormat.HighlightColor;
			}
			if (sourceFormat.HasValue(50))
			{
				destFormat.Shadow = sourceFormat.Shadow;
			}
			if (sourceFormat.HasValue(18))
			{
				destFormat.SetPropertyValue(18, sourceFormat.CharacterSpacing);
			}
			if (sourceFormat.HasValue(14))
			{
				destFormat.DoubleStrike = sourceFormat.DoubleStrike;
			}
			if (sourceFormat.HasValue(51))
			{
				destFormat.Emboss = sourceFormat.Emboss;
			}
			if (sourceFormat.HasValue(52))
			{
				destFormat.Engrave = sourceFormat.Engrave;
			}
			if (sourceFormat.HasValue(10))
			{
				destFormat.SubSuperScript = sourceFormat.SubSuperScript;
			}
			destFormat.TextBackgroundColor = sourceFormat.TextBackgroundColor;
			if (sourceFormat.HasValue(54))
			{
				destFormat.AllCaps = sourceFormat.AllCaps;
			}
			if (sourceFormat.Bidi)
			{
				destFormat.Bidi = true;
			}
			else if (!sourceFormat.Bidi && destFormat.Bidi)
			{
				destFormat.Bidi = false;
			}
			if (sourceFormat.HasValue(59))
			{
				destFormat.BoldBidi = sourceFormat.BoldBidi;
			}
			if (sourceFormat.HasValue(60))
			{
				destFormat.ItalicBidi = sourceFormat.ItalicBidi;
			}
			if (sourceFormat.HasValue(61))
			{
				destFormat.FontNameBidi = sourceFormat.FontNameBidi;
			}
			if (sourceFormat.HasValue(62))
			{
				destFormat.SetPropertyValue(62, sourceFormat.FontSizeBidi);
			}
			if (sourceFormat.HasValue(109))
			{
				destFormat.FieldVanish = sourceFormat.FieldVanish;
			}
			if (sourceFormat.HasValue(53))
			{
				destFormat.Hidden = sourceFormat.Hidden;
			}
			if (sourceFormat.HasValue(24))
			{
				destFormat.SpecVanish = sourceFormat.SpecVanish;
			}
			if (sourceFormat.HasValue(55))
			{
				destFormat.SmallCaps = sourceFormat.SmallCaps;
			}
			if (sourceFormat.HasValue(72))
			{
				destFormat.IdctHint = sourceFormat.IdctHint;
			}
			if (sourceFormat.HasValue(127))
			{
				destFormat.Scaling = sourceFormat.Scaling;
			}
			if (sourceFormat.HasValue(0))
			{
				destFormat.Font = sourceFormat.Font;
			}
			if (sourceFormat.HasValue(17))
			{
				destFormat.Position = sourceFormat.Position;
			}
			if (sourceFormat.HasValue(20))
			{
				destFormat.LineBreak = sourceFormat.LineBreak;
			}
			if (sourceFormat.HasValue(68))
			{
				destFormat.FontNameAscii = sourceFormat.FontNameAscii;
			}
			if (sourceFormat.HasValue(69))
			{
				destFormat.FontNameFarEast = sourceFormat.FontNameFarEast;
			}
			if (sourceFormat.HasValue(70))
			{
				destFormat.FontNameNonFarEast = sourceFormat.FontNameNonFarEast;
			}
			if (sourceFormat.HasValue(71))
			{
				destFormat.OutLine = sourceFormat.OutLine;
			}
			if (sourceFormat.HasValue(73))
			{
				destFormat.LocaleIdASCII = sourceFormat.LocaleIdASCII;
			}
			if (sourceFormat.HasValue(74))
			{
				destFormat.LocaleIdFarEast = sourceFormat.LocaleIdFarEast;
			}
			if (sourceFormat.HasValue(75))
			{
				destFormat.LocaleIdBidi = sourceFormat.LocaleIdBidi;
			}
			if (sourceFormat.HasValue(76))
			{
				destFormat.NoProof = sourceFormat.NoProof;
			}
			if (sourceFormat.HasValue(78))
			{
				destFormat.TextureStyle = sourceFormat.TextureStyle;
			}
			if (sourceFormat.HasValue(77))
			{
				destFormat.ForeColor = sourceFormat.ForeColor;
			}
			if (sourceFormat.HasValue(79))
			{
				destFormat.EmphasisType = sourceFormat.EmphasisType;
			}
			if (sourceFormat.HasValue(80))
			{
				destFormat.TextEffect = sourceFormat.TextEffect;
			}
			if (sourceFormat.HasValue(81))
			{
				destFormat.SnapToGrid = sourceFormat.SnapToGrid;
			}
			if (sourceFormat.HasValue(91))
			{
				destFormat.CharStyleName = sourceFormat.CharStyleName;
			}
			if (sourceFormat.HasValue(92))
			{
				destFormat.WebHidden = sourceFormat.WebHidden;
			}
			if (sourceFormat.HasValue(99))
			{
				destFormat.ComplexScript = sourceFormat.ComplexScript;
			}
			if (sourceFormat.HasValue(103))
			{
				destFormat.IsInsertRevision = sourceFormat.IsInsertRevision;
			}
			if (sourceFormat.HasValue(104))
			{
				destFormat.IsDeleteRevision = sourceFormat.IsDeleteRevision;
			}
			if (sourceFormat.HasValue(105))
			{
				destFormat.IsChangedFormat = sourceFormat.IsChangedFormat;
			}
			if (sourceFormat.HasValue(106))
			{
				destFormat.Special = sourceFormat.Special;
			}
			if (sourceFormat.HasValue(107))
			{
				destFormat.ListPictureIndex = sourceFormat.ListPictureIndex;
			}
			if (sourceFormat.HasValue(108))
			{
				destFormat.ListHasPicture = sourceFormat.ListHasPicture;
			}
			if (sourceFormat.HasValue(120))
			{
				destFormat.UseContextualAlternates = sourceFormat.UseContextualAlternates;
			}
			if (sourceFormat.HasValue(121))
			{
				destFormat.Ligatures = sourceFormat.Ligatures;
			}
			if (sourceFormat.HasValue(122))
			{
				destFormat.NumberForm = sourceFormat.NumberForm;
			}
			if (sourceFormat.HasValue(123))
			{
				destFormat.NumberSpacing = sourceFormat.NumberSpacing;
			}
			if (sourceFormat.HasValue(124))
			{
				destFormat.StylisticSet = sourceFormat.StylisticSet;
			}
			if (sourceFormat.HasValue(125))
			{
				destFormat.Kern = sourceFormat.Kern;
			}
			if (sourceFormat.HasValue(126))
			{
				destFormat.BreakClear = sourceFormat.BreakClear;
			}
			if (sourceFormat.HasValue(8))
			{
				destFormat.AuthorName = sourceFormat.AuthorName;
			}
			if (sourceFormat.HasValue(12))
			{
				destFormat.FormatChangeAuthorName = sourceFormat.FormatChangeAuthorName;
			}
			if (sourceFormat.HasValue(15))
			{
				destFormat.FormatChangeDateTime = sourceFormat.FormatChangeDateTime;
			}
			if (sourceFormat.HasValue(128))
			{
				destFormat.RevisionName = sourceFormat.RevisionName;
			}
		}

		private void InitBorders()
		{
			Borders borders = m_paragraph.ParagraphFormat.Borders;
			if ((!borders.NoBorder && !base.IsSectionEndMark) || m_paragraph.ParagraphFormat.HasBorder())
			{
				if (borders.Left.BorderType != 0)
				{
					base.Paddings.Left -= borders.Left.Space + borders.Left.GetLineWidthValue() / 2f;
				}
				else
				{
					base.SkipLeftBorder = true;
				}
				if (borders.Right.BorderType != 0)
				{
					base.Paddings.Right -= borders.Right.Space + borders.Right.GetLineWidthValue() / 2f;
				}
				else
				{
					base.SkipRightBorder = true;
				}
				if (borders.Top.BorderType != 0)
				{
					base.Paddings.Top += borders.Top.Space + borders.Top.GetLineWidthValue();
				}
				else
				{
					base.SkipTopBorder = true;
				}
				if (borders.Bottom.BorderType != 0)
				{
					base.Paddings.Bottom += borders.Bottom.Space + borders.Bottom.GetLineWidthValue();
				}
				else
				{
					base.SkipBottomBorder = true;
				}
				if (m_prevSibling is WParagraph wParagraph && wParagraph.ParagraphFormat.Borders.Horizontal.IsBorderDefined)
				{
					if (!m_paragraph.IsAdjacentParagraphHaveSameBorders(wParagraph, base.Margins.Left + ((base.FirstLineIndent > 0f) ? 0f : base.FirstLineIndent)))
					{
						base.SkipHorizonatalBorder = true;
					}
					else
					{
						base.SkipTopBorder = true;
						base.Paddings.Top = wParagraph.ParagraphFormat.Borders.Horizontal.Space + wParagraph.ParagraphFormat.Borders.Horizontal.GetLineWidthValue();
					}
				}
				else
				{
					base.SkipHorizonatalBorder = true;
				}
				WParagraph wParagraph2 = m_nextSibling as WParagraph;
				if (m_paragraph.IsEndOfSection && !m_paragraph.IsEndOfDocument)
				{
					wParagraph2 = (m_paragraph.GetOwnerSection().NextSibling as WSection).GetFirstParagraph();
				}
				if (wParagraph2 != null && wParagraph2.ParagraphFormat.HasBorder() && m_paragraph.IsAdjacentParagraphHaveSameBorders(wParagraph2, base.Margins.Left + ((base.FirstLineIndent > 0f) ? 0f : base.FirstLineIndent)))
				{
					base.SkipBottomBorder = true;
					base.Paddings.Bottom = borders.Horizontal.Space;
				}
				if (!base.SkipTopBorder && m_prevSibling != null && m_prevSibling is WParagraph && !((WParagraph)m_prevSibling).ParagraphFormat.Borders.NoBorder && !((WParagraph)m_prevSibling).SectionEndMark && m_paragraph.IsAdjacentParagraphHaveSameBorders((WParagraph)m_prevSibling, base.Margins.Left + ((base.FirstLineIndent > 0f) ? 0f : base.FirstLineIndent)))
				{
					base.Paddings.Top = 0f;
					base.SkipTopBorder = true;
				}
				if (!base.SkipBottomBorder && m_nextSibling != null && m_nextSibling is WParagraph && !((WParagraph)m_nextSibling).ParagraphFormat.Borders.NoBorder && m_paragraph.IsAdjacentParagraphHaveSameBorders((WParagraph)m_nextSibling, base.Margins.Left + ((base.FirstLineIndent > 0f) ? 0f : base.FirstLineIndent)))
				{
					base.Paddings.Bottom = 0f;
					base.SkipBottomBorder = true;
				}
			}
			else
			{
				bool flag2 = (base.SkipHorizonatalBorder = true);
				bool flag4 = (base.SkipTopBorder = flag2);
				bool flag6 = (base.SkipRightBorder = flag4);
				bool skipBottomBorder = (base.SkipLeftBorder = flag6);
				base.SkipBottomBorder = skipBottomBorder;
			}
		}

		private void InitPageBreaks()
		{
			IEntity owner = m_paragraph.Owner;
			while (owner != null && owner.EntityType != EntityType.TextBody)
			{
				owner = owner.Owner;
			}
			WTextBody obj = owner as WTextBody;
			bool flag = false;
			bool flag2 = false;
			WParagraph wParagraph = null;
			bool flag3 = m_paragraph.Text.Contains(ControlChar.LineFeed) || m_paragraph.Text.Contains(ControlChar.CrLf);
			if (obj != null)
			{
				bool flag4 = IsLastParagraphInTextBody();
				IWSection iWSection = null;
				WSection ownerSection = m_paragraph.GetOwnerSection();
				if (ownerSection != null)
				{
					int num = Document.Sections.IndexOf(ownerSection);
					iWSection = ((num + 1 < m_paragraph.Document.Sections.Count) ? Document.Sections[num + 1] : null);
				}
				if (iWSection != null)
				{
					flag = flag4 && (iWSection.BreakCode == SectionBreakCode.NewPage || iWSection.BreakCode == SectionBreakCode.NewColumn || iWSection.BreakCode == SectionBreakCode.Oddpage || iWSection.BreakCode == SectionBreakCode.EvenPage || iWSection.BreakCode == SectionBreakCode.NoBreak);
				}
				if (!flag4 && (m_paragraph.OwnerTextBody.Owner is WSection || (m_paragraph.OwnerTextBody.Owner is BlockContentControl && !m_paragraph.IsInCell)))
				{
					if (m_nextSibling != null)
					{
						wParagraph = GetFirstParagraphFromEntity(m_nextSibling);
					}
					if (wParagraph != null && !wParagraph.IsSectionEndMark())
					{
						flag2 = wParagraph.ParagraphFormat.PageBreakBefore;
					}
				}
			}
			WParagraphFormat paragraphFormat = m_paragraph.ParagraphFormat;
			base.IsPageBreak = !flag3 && (paragraphFormat.PageBreakAfter || flag2 || paragraphFormat.ColumnBreakAfter || (flag && m_paragraph.IsParagraphHasSectionBreak()));
			if (flag2)
			{
				DocumentLayouter.IsEndPage = true;
			}
		}

		private WParagraph GetFirstParagraphFromEntity(IEntity entity)
		{
			if (entity is WParagraph)
			{
				return entity as WParagraph;
			}
			if (entity is WTable)
			{
				WTable wTable = entity as WTable;
				if (wTable.Rows.Count > 0 && wTable.Rows[0].Cells.Count > 0 && wTable.Rows[0].Cells[0].ChildEntities.Count > 0)
				{
					return GetFirstParagraphFromEntity(wTable.Rows[0].Cells[0].ChildEntities[0]);
				}
			}
			else if (entity is BlockContentControl)
			{
				BlockContentControl blockContentControl = entity as BlockContentControl;
				if (blockContentControl.TextBody.Items.Count > 0)
				{
					return GetFirstParagraphFromEntity(blockContentControl.TextBody.Items[0]);
				}
			}
			return null;
		}

		private bool IsLastParagraphInTextBody()
		{
			if (m_paragraph.NextSibling == null)
			{
				return true;
			}
			return false;
		}

		private void InitFormat()
		{
			WParagraphFormat paragraphFormat = m_paragraph.ParagraphFormat;
			WSection ownerSection = m_paragraph.GetOwnerSection();
			if (!(m_paragraph.GetStyle() is WParagraphStyle))
			{
				m_paragraph.Document.Styles.FindByName("Normal");
			}
			base.Margins.Left = paragraphFormat.LeftIndent;
			if (ownerSection != null && ownerSection.PageSetup != null && paragraphFormat.RightIndent > ownerSection.PageSetup.ClientWidth)
			{
				base.Margins.Right = 0f;
			}
			else
			{
				base.Margins.Right = paragraphFormat.RightIndent;
			}
			WTableCell wTableCell = null;
			if (m_paragraph.IsInCell)
			{
				wTableCell = m_paragraph.GetOwnerEntity() as WTableCell;
			}
			base.Margins.Top = (float)paragraphFormat.GetParagraphFormat(8);
			base.Margins.Bottom = (float)paragraphFormat.GetParagraphFormat(9);
			if (!m_paragraph.Document.DOP.Dop2000.Copts.DontUseHTMLParagraphAutoSpacing)
			{
				UpdateParagraphSpacing();
			}
			if (m_paragraph.IsInCell && !m_paragraph.Document.DOP.Dop2000.Copts.DontUseHTMLParagraphAutoSpacing)
			{
				if (m_nextSibling == null && m_paragraph.ParagraphFormat.SpaceAfterAuto)
				{
					WListFormat listFormatValue = m_paragraph.GetListFormatValue();
					if (listFormatValue == null || listFormatValue.ListType == ListType.NoList)
					{
						base.Margins.Bottom = 0f;
					}
					else
					{
						base.Margins.Bottom = 14f;
					}
				}
				if (m_prevSibling == null && m_paragraph.ParagraphFormat.SpaceBeforeAuto)
				{
					base.Margins.Top = 0f;
				}
			}
			if (paragraphFormat.ContextualSpacing && (!m_paragraph.IsInCell || !m_paragraph.Document.DOP.Dop2000.Copts.AllowSpaceOfSameStyleInTable || m_paragraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013))
			{
				if (m_paragraph.IsInCell)
				{
					if (m_paragraph.Index == 0)
					{
						UpdateFirstParagraphMarginsBasedOnContextualSpacing();
					}
					else if (m_paragraph.Index == wTableCell.ChildEntities.Count - 1)
					{
						UpdateLastParagraphMarginsBasedOnContextualSpacing();
					}
				}
				IEntity prevSibling = GetPrevSibling();
				IEntity nextSibling = GetNextSibling();
				if (IsSameStyle(m_paragraph, prevSibling as WParagraph))
				{
					base.Margins.Top = 0f;
				}
				if (IsSameStyle(m_paragraph, nextSibling as WParagraph))
				{
					base.Margins.Bottom = 0f;
				}
				WTextBody ownerTextBody = m_paragraph.OwnerTextBody;
				if (ownerTextBody != null && ((m_paragraph.Index != 0 && m_paragraph.Index != ownerTextBody.ChildEntities.Count - 1) || ownerTextBody.Owner is BlockContentControl))
				{
					UpdateParaMarginsBasedOnContextualSpacing(prevSibling, nextSibling);
				}
			}
			base.IsKeepTogether = paragraphFormat.Keep;
			base.IsKeepWithNext = paragraphFormat.KeepFollow;
			base.FirstLineIndent = paragraphFormat.FirstLineIndent;
			base.Justification = (HAlignment)paragraphFormat.GetAlignmentToRender();
			if (base.IsSectionEndMark)
			{
				if (UpdateSectionBreakSpacing())
				{
					base.Margins.Bottom = m_paragraph.ParagraphFormat.AfterSpacing + base.Size.Height;
					base.Margins.Top = m_paragraph.ParagraphFormat.BeforeSpacing;
				}
				else
				{
					base.Margins.Bottom = 0f;
					base.Margins.Top = 0f;
				}
			}
		}

		internal bool UpdateSectionBreakSpacing()
		{
			if ((m_prevSibling is WTable || (m_prevSibling is WParagraph && (m_prevSibling as WParagraph).BreakCharacterFormat.Hidden)) && m_paragraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013)
			{
				return true;
			}
			return false;
		}

		private void UpdateFirstParagraphMarginsBasedOnContextualSpacing()
		{
			WTableCell wTableCell = m_paragraph.GetOwnerEntity() as WTableCell;
			WTableRow ownerRow = wTableCell.OwnerRow;
			WTable ownerTable = ownerRow.OwnerTable;
			IEntity entity;
			if (m_paragraph.NextSibling != null)
			{
				entity = m_paragraph.NextSibling;
			}
			else
			{
				IEntity entity2 = ((wTableCell.NextSibling != null) ? (wTableCell.NextSibling as WTableCell).Items[0] : null);
				entity = entity2;
			}
			IEntity entity3 = entity;
			IEntity entity4;
			if (m_paragraph.PreviousSibling != null)
			{
				entity4 = m_paragraph.PreviousSibling;
			}
			else
			{
				IEntity entity2 = ((wTableCell.PreviousSibling != null) ? (wTableCell.PreviousSibling as WTableCell).Items[0] : null);
				entity4 = entity2;
			}
			IEntity entity5 = entity4;
			if (wTableCell.Index == 0 && m_paragraph.Index == 0)
			{
				if (ownerRow.Index == 0)
				{
					if (ownerTable.IsInCell && ownerTable.Index == 0)
					{
						CheckOwnerTablePrevItem(ownerTable);
						return;
					}
					IEntity entity6 = ownerTable.PreviousSibling;
					if (entity6 is BlockContentControl)
					{
						entity6 = (entity6 as BlockContentControl).GetLastParagraphOfSDTContent();
					}
					if (IsSameStyle(m_paragraph, entity6 as WParagraph))
					{
						base.Margins.Top = 0f;
					}
					UpdateAfterSpacing(entity3 as WParagraph);
				}
				else
				{
					if (IsSameStyle(m_paragraph, entity5 as WParagraph) || (entity5 == null && m_paragraph.StyleName == "Normal" && !IsParaHaveNumberingOrBullets()))
					{
						base.Margins.Top = 0f;
					}
					UpdateAfterSpacing(entity3 as WParagraph);
				}
			}
			else
			{
				if (m_paragraph.Index != 0)
				{
					return;
				}
				WTableCell wTableCell2 = ownerRow.ChildEntities[wTableCell.Index - 1] as WTableCell;
				IEntity entity7 = wTableCell2.ChildEntities[wTableCell2.ChildEntities.Count - 1];
				if (entity7 is BlockContentControl)
				{
					entity7 = (entity7 as BlockContentControl).GetLastParagraphOfSDTContent();
				}
				if (entity7 is WTable && m_paragraph.StyleName == "Normal" && !IsParaHaveNumberingOrBullets())
				{
					base.Margins.Top = 0f;
					base.Margins.Bottom = 0f;
					return;
				}
				if (IsSameStyle(m_paragraph, entity7 as WParagraph))
				{
					base.Margins.Top = 0f;
				}
				UpdateAfterSpacing(entity3 as WParagraph);
			}
		}

		private void UpdateLastParagraphMarginsBasedOnContextualSpacing()
		{
			WTableCell wTableCell = m_paragraph.GetOwnerEntity() as WTableCell;
			WTableRow ownerRow = wTableCell.OwnerRow;
			if (wTableCell.Index == ownerRow.Cells.Count - 1 && m_paragraph.Index == wTableCell.ChildEntities.Count - 1)
			{
				if (m_paragraph.StyleName == "Normal" && !IsParaHaveNumberingOrBullets())
				{
					base.Margins.Bottom = 0f;
				}
			}
			else if (m_paragraph.Index == wTableCell.ChildEntities.Count - 1)
			{
				IEntity entity = (ownerRow.ChildEntities[wTableCell.Index + 1] as WTableCell).ChildEntities[0];
				while (entity is WTable)
				{
					entity = (entity as WTable).Rows[0].Cells[0].ChildEntities[0];
				}
				if (entity is BlockContentControl)
				{
					entity = (entity as BlockContentControl).GetFirstParagraphOfSDTContent();
				}
				if (IsSameStyle(m_paragraph, entity as WParagraph))
				{
					base.Margins.Bottom = 0f;
				}
			}
		}

		private bool IsParaHaveNumberingOrBullets()
		{
			WListFormat listFormatValue = m_paragraph.GetListFormatValue();
			if (listFormatValue == null || listFormatValue.ListType == ListType.NoList)
			{
				return false;
			}
			return true;
		}

		private void UpdateParaMarginsBasedOnContextualSpacing(IEntity prevSibling, IEntity nextSibling)
		{
			if (prevSibling is WTable && m_paragraph.StyleName == "Normal" && !IsParaHaveNumberingOrBullets())
			{
				base.Margins.Top = 0f;
			}
			if (nextSibling is WTable)
			{
				WParagraph innerTableFirstPara = GetInnerTableFirstPara(nextSibling as WTable);
				if ((innerTableFirstPara == null && m_paragraph.StyleName == "Normal") || IsSameStyle(m_paragraph, innerTableFirstPara))
				{
					base.Margins.Bottom = 0f;
				}
			}
		}

		private WParagraph GetInnerTableFirstPara(WTable table)
		{
			if (table.Rows.Count == 0 || table.Rows[0].Cells.Count == 0 || table.Rows[0].Cells[0].ChildEntities.Count == 0)
			{
				return null;
			}
			IEntity entity = table.Rows[0].Cells[0].ChildEntities[0];
			if (entity is WTable)
			{
				entity = GetInnerTableFirstPara(entity as WTable);
			}
			if (entity is BlockContentControl)
			{
				entity = (entity as BlockContentControl).GetFirstParagraphOfSDTContent();
			}
			return entity as WParagraph;
		}

		private void CheckOwnerTablePrevItem(WTable ownerTable)
		{
			WTableRow ownerRow = ownerTable.GetOwnerTableCell().OwnerRow;
			if (ownerRow.Index > 0)
			{
				if (m_paragraph.StyleName == "Normal" && !IsParaHaveNumberingOrBullets())
				{
					base.Margins.Top = 0f;
				}
				return;
			}
			if (ownerRow.OwnerTable.IsInCell && ownerRow.OwnerTable.Index == 0)
			{
				CheckOwnerTablePrevItem(ownerRow.OwnerTable);
				return;
			}
			IEntity entity = ownerRow.OwnerTable.PreviousSibling;
			if (entity is BlockContentControl)
			{
				entity = (entity as BlockContentControl).GetLastParagraphOfSDTContent();
			}
			if (IsSameStyle(m_paragraph, entity as WParagraph))
			{
				base.Margins.Top = 0f;
			}
		}

		private void UpdateParagraphSpacing()
		{
			if (m_paragraph.ParagraphFormat.SpaceBeforeAuto)
			{
				base.Margins.Top = 14f;
			}
			if (m_paragraph.ParagraphFormat.SpaceAfterAuto)
			{
				base.Margins.Bottom = 14f;
			}
			float bottom = 0f;
			IEntity entity = GetNextSibling();
			if (entity != null && entity is WParagraph)
			{
				entity = (((WParagraph)entity).IsParagraphBeforeSpacingNeedToSkip() ? null : entity);
			}
			IEntity prevSibling = GetPrevSibling();
			bool num;
			if (!m_paragraph.Text.Contains(ControlChar.ParagraphBreak))
			{
				if (entity == null || !(entity is WParagraph) || (entity as WParagraph).SectionEndMark || (((entity as WParagraph).ParagraphFormat.SpaceBeforeAuto || !((bottom = (float)(entity as WParagraph).ParagraphFormat.GetParagraphFormat(8)) > base.Margins.Bottom)) && (!(entity as WParagraph).ParagraphFormat.SpaceBeforeAuto || !(base.Margins.Bottom < 14f))) || ((entity as WParagraph).ParagraphFormat.SpaceBeforeAuto && IsParagraphContainsListtype(m_paragraph) && IsParagraphContainsListtype(entity as WParagraph)))
				{
					goto IL_01d6;
				}
				if (!(entity as WParagraph).ParagraphFormat.ContextualSpacing)
				{
					goto IL_018f;
				}
				num = !IsSameStyle(m_paragraph, entity as WParagraph);
			}
			else
			{
				num = (bottom = (float)m_paragraph.ParagraphFormat.GetParagraphFormat(8)) > base.Margins.Bottom;
			}
			if (num)
			{
				goto IL_018f;
			}
			goto IL_01d6;
			IL_01d6:
			if (prevSibling != null && prevSibling is WParagraph && (prevSibling as WParagraph).m_layoutInfo != null && ((prevSibling as WParagraph).m_layoutInfo as ParagraphLayoutInfo).Margins.Bottom >= base.Margins.Top && (m_paragraph.ParagraphFormat.IsPreviousParagraphInSameFrame() || (!(prevSibling as WParagraph).ParagraphFormat.IsInFrame() && !m_paragraph.ParagraphFormat.IsInFrame())))
			{
				base.Margins.Top = 0f;
			}
			if (prevSibling == null && !m_paragraph.Document.DOP.Dop2000.Copts.DontUseHTMLParagraphAutoSpacing)
			{
				UpdateTopMargin();
			}
			if (m_paragraph.IsFirstParagraphOfOwnerTextBody() && m_paragraph.ParagraphFormat.SpaceBeforeAuto)
			{
				base.Margins.Top = 0f;
			}
			return;
			IL_018f:
			if (!m_paragraph.Text.Contains(ControlChar.ParagraphBreak) && (entity as WParagraph).ParagraphFormat.SpaceBeforeAuto)
			{
				base.Margins.Bottom = 14f;
			}
			else
			{
				base.Margins.Bottom = bottom;
			}
			goto IL_01d6;
		}

		private bool IsSameStyle(WParagraph currParagraph, WParagraph nextOrPrevSibling)
		{
			if (currParagraph == null || nextOrPrevSibling == null)
			{
				return false;
			}
			if (currParagraph.StyleName == nextOrPrevSibling.StyleName)
			{
				return true;
			}
			return false;
		}

		private void UpdateAfterSpacing(WParagraph paragraphNextsibling)
		{
			if (IsSameStyle(m_paragraph, paragraphNextsibling) || (paragraphNextsibling == null && m_paragraph.StyleName == "Normal" && !IsParaHaveNumberingOrBullets()))
			{
				base.Margins.Bottom = 0f;
			}
		}

		private void UpdateTopMargin()
		{
			WSection ownerSection = m_paragraph.GetOwnerSection();
			if (ownerSection != null && ownerSection.Index > 0)
			{
				Entity entity = ownerSection.Body.ChildEntities.FirstItem;
				if (entity is BlockContentControl)
				{
					entity = (entity as BlockContentControl).GetFirstParagraphOfSDTContent();
				}
				if (entity == m_paragraph && GetLastItemOfPreviousSection(ownerSection.Index) is WParagraph wParagraph)
				{
					float num = (m_paragraph.ParagraphFormat.SpaceBeforeAuto ? 14f : m_paragraph.ParagraphFormat.BeforeSpacing);
					float num2 = (wParagraph.ParagraphFormat.SpaceAfterAuto ? 14f : wParagraph.ParagraphFormat.AfterSpacing);
					base.Margins.Top = ((num > num2) ? (num - num2) : 0f);
					m_paragraph.IsTopMarginValueUpdated = true;
				}
			}
		}

		private IEntity GetPrevSibling()
		{
			IEntity entity = m_prevSibling;
			if (entity == null && m_paragraph.OwnerTextBody.Owner is BlockContentControl)
			{
				entity = (m_paragraph.OwnerTextBody.Owner as BlockContentControl).PreviousSibling;
			}
			if (entity is BlockContentControl)
			{
				entity = (entity as BlockContentControl).GetLastParagraphOfSDTContent();
			}
			return entity;
		}

		private IEntity GetNextSibling()
		{
			IEntity entity = m_nextSibling;
			if (entity == null && m_paragraph.OwnerTextBody.Owner is BlockContentControl)
			{
				entity = (m_paragraph.OwnerTextBody.Owner as BlockContentControl).NextSibling;
			}
			if (entity is BlockContentControl)
			{
				entity = (entity as BlockContentControl).GetFirstParagraphOfSDTContent();
			}
			return entity;
		}

		private IEntity GetLastItemOfPreviousSection(int currentSectionIndex)
		{
			IEntity entity = m_paragraph.Document.Sections[currentSectionIndex - 1].Body.ChildEntities.LastItem;
			if (entity == null && m_paragraph.OwnerTextBody.Owner is BlockContentControl)
			{
				entity = (m_paragraph.OwnerTextBody.Owner as BlockContentControl).PreviousSibling;
			}
			if (entity is BlockContentControl)
			{
				entity = (entity as BlockContentControl).GetLastParagraphOfSDTContent();
			}
			return entity;
		}

		private bool IsParagraphContainsListtype(WParagraph para)
		{
			WParagraphStyle wParagraphStyle = para.ParaStyle as WParagraphStyle;
			if (para.ListFormat.ListType != ListType.NoList)
			{
				return true;
			}
			while (wParagraphStyle != null)
			{
				if (wParagraphStyle.ListFormat.ListType != ListType.NoList || wParagraphStyle.ListFormat.IsEmptyList)
				{
					return true;
				}
				wParagraphStyle = wParagraphStyle.BaseStyle;
			}
			return false;
		}
	}

	internal class ListTabs : TabsLayoutInfo
	{
		public ListTabs(WParagraph paragrath)
			: base(ChildrenLayoutDirection.Horizontal)
		{
			m_defaultTabWidth = paragrath.GetDefaultTabWidth();
		}
	}

	internal struct SkippedBkmkInfo
	{
		internal bool IsBkmkEnd;

		internal string BkmkName;
	}

	private const string DEF_NORMAL_STYLE = "Normal";

	private const string DEF_DEFAULTPARAGRAPHFONT_STYLE_ID = "Default Paragraph Font";

	private const int DEF_LIST_STYLE_ID = 179;

	private const int DEF_USER_STYLE_ID = 4094;

	private string m_paraId = "";

	protected IWParagraphStyle m_style;

	internal StringBuilder m_strTextBuilder = new StringBuilder(1);

	internal string m_liststring = string.Empty;

	protected WParagraphFormat m_prFormat;

	protected WListFormat m_listFormat;

	private byte m_bFlags;

	private byte m_bAFlags;

	protected ParagraphItemCollection m_pItemColl;

	private ParagraphItemCollection m_pEmptyItemColl;

	private ParagraphItemCollection m_paragraphItems;

	private WCharacterFormat m_charFormat;

	private TextBodyItem m_ownerTextBodyItem;

	internal StringBuilder m_strTextBuilder2;

	internal bool HasSDTInlineItem
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool SplitWidgetContainerDrawn
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsStyleApplied
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool IsNeedToMeasureBookMarkSize
	{
		get
		{
			if (SectionEndMark)
			{
				return false;
			}
			if (m_paragraphItems != null)
			{
				for (int i = 0; i < m_paragraphItems.Count; i++)
				{
					if (!(m_paragraphItems[i] is BookmarkStart) && !(m_paragraphItems[i] is BookmarkEnd))
					{
						return false;
					}
				}
			}
			else
			{
				for (int j = 0; j < ChildEntities.Count; j++)
				{
					if (!(ChildEntities[j] is BookmarkStart) && !(ChildEntities[j] is BookmarkEnd))
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public override EntityType EntityType => EntityType.Paragraph;

	public EntityCollection ChildEntities => m_pItemColl;

	public string StyleName
	{
		get
		{
			if (m_style == null)
			{
				return null;
			}
			return m_style.Name;
		}
	}

	public string ListString => m_liststring;

	internal string ParaId
	{
		get
		{
			return m_paraId;
		}
		set
		{
			m_paraId = value;
		}
	}

	public string Text
	{
		get
		{
			return m_strTextBuilder.ToString();
		}
		set
		{
			Items.Clear();
			AppendText(value).CharacterFormat.ImportContainer(BreakCharacterFormat);
		}
	}

	public bool EnableStyleSeparator
	{
		get
		{
			if (m_charFormat != null && m_charFormat.SpecVanish)
			{
				return m_charFormat.Hidden;
			}
			return false;
		}
		set
		{
			if (m_charFormat != null)
			{
				WCharacterFormat charFormat = m_charFormat;
				bool specVanish = (m_charFormat.Hidden = value);
				charFormat.SpecVanish = specVanish;
			}
		}
	}

	public ParagraphItem this[int index] => m_pItemColl[index];

	public ParagraphItemCollection Items => m_pItemColl;

	public WParagraphFormat ParagraphFormat => m_prFormat;

	public WCharacterFormat BreakCharacterFormat => m_charFormat;

	public WListFormat ListFormat
	{
		get
		{
			if (m_listFormat == null)
			{
				m_listFormat = new WListFormat(this);
			}
			return m_listFormat;
		}
	}

	public bool IsInCell
	{
		get
		{
			if (base.Owner is WTableCell)
			{
				return true;
			}
			if (base.Owner is WTextBody && (base.Owner as WTextBody).Owner is BlockContentControl)
			{
				return GetOwnerEntity() is WTableCell;
			}
			return false;
		}
	}

	internal bool IsFloatingItemsLayouted
	{
		get
		{
			return (m_bAFlags & 1) != 0;
		}
		set
		{
			m_bAFlags = (byte)((m_bAFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsXpositionUpated
	{
		get
		{
			return (m_bAFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bAFlags = (byte)((m_bAFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool HasParRTFTag
	{
		get
		{
			return (m_bAFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bAFlags = (byte)((m_bAFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	public bool IsEndOfSection
	{
		get
		{
			if (base.Owner.Owner is WSection)
			{
				return base.NextSibling == null;
			}
			return false;
		}
	}

	public bool IsEndOfDocument
	{
		get
		{
			if (IsEndOfSection)
			{
				return (base.Owner.Owner as WSection).NextSibling == null;
			}
			return false;
		}
	}

	internal bool IsLastItem
	{
		get
		{
			return (m_bFlags & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	internal bool IsNeedToSkip
	{
		get
		{
			return (m_bFlags & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	internal bool IsTopMarginValueUpdated
	{
		get
		{
			return (m_bFlags & 0x80) >> 7 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0x7Fu) | ((value ? 1u : 0u) << 7));
		}
	}

	int IWidgetContainer.Count => WidgetCollection.Count;

	EntityCollection IWidgetContainer.WidgetInnerCollection => WidgetCollection as EntityCollection;

	IWidget IWidgetContainer.this[int index] => (WidgetCollection as ParagraphItemCollection).GetCurrentWidget(index);

	protected IEntityCollectionBase WidgetCollection
	{
		get
		{
			int index = -1;
			if (m_pItemColl.Count == 0)
			{
				return m_pEmptyItemColl;
			}
			if (HasSDTInlineItem)
			{
				ParagraphItemCollection paragraphItems = GetParagraphItems();
				if (paragraphItems.Count == 0)
				{
					return m_pEmptyItemColl;
				}
				if ((IsLastItemBreak(ref index) && IsNeedToAddEmptyTextRangeForBreakItem(paragraphItems[index] as Break)) || IsContainFloatingItems())
				{
					paragraphItems.AddToInnerList(m_pEmptyItemColl[0]);
				}
				return paragraphItems;
			}
			if ((IsLastItemBreak(ref index) && IsNeedToAddEmptyTextRangeForBreakItem(m_pItemColl[index] as Break)) || IsContainFloatingItems())
			{
				if (m_paragraphItems == null || m_pItemColl.Owner != m_paragraphItems.Owner || m_pItemColl.Count != m_paragraphItems.Count - 1)
				{
					m_paragraphItems = GetParagraphItems();
					m_paragraphItems.AddToInnerList(m_pEmptyItemColl[0]);
				}
				return m_paragraphItems;
			}
			return m_pItemColl;
		}
	}

	internal bool RemoveEmpty
	{
		get
		{
			return (m_bFlags & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal ParagraphItem LastItem => this[m_pItemColl.Count - 1];

	internal IWParagraphStyle ParaStyle
	{
		get
		{
			return m_style;
		}
		set
		{
			WParagraphStyle wParagraphStyle = null;
			if (m_style != value && m_style is WParagraphStyle { IsRemoving: false, IsCustom: not false } wParagraphStyle2 && wParagraphStyle2.RangeCollection.Contains(this))
			{
				wParagraphStyle2.RangeCollection.Remove(this);
			}
			if (m_style != value && value is WParagraphStyle { IsCustom: not false } wParagraphStyle3)
			{
				wParagraphStyle3.RangeCollection.Add(this);
			}
			m_style = value;
			if (ParagraphFormat.IsFormattingChange)
			{
				ParagraphFormat.SetPropertyValue(47, m_style.Name);
			}
		}
	}

	internal bool SectionEndMark => IsSectionEndMark();

	internal bool IsTextReplaced
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal bool IsAppendingHTML
	{
		get
		{
			return (m_bFlags & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	internal bool IsCreatedUsingHtmlSpanTag
	{
		get
		{
			return (m_bAFlags & 0x20) >> 5 != 0;
		}
		set
		{
			m_bAFlags = (byte)((m_bAFlags & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	internal string InternalText
	{
		get
		{
			if (m_strTextBuilder2 != null && base.Document.IsComparing)
			{
				return m_strTextBuilder2.ToString();
			}
			return m_strTextBuilder.ToString();
		}
		set
		{
			if (m_strTextBuilder2 == null)
			{
				m_strTextBuilder2 = new StringBuilder(value);
				return;
			}
			m_strTextBuilder2.Clear();
			m_strTextBuilder2.Append(value);
		}
	}

	private bool IsNeedToAddEmptyTextRangeForBreakItem(Break breakItem)
	{
		if (breakItem != null && !breakItem.CharacterFormat.Hidden)
		{
			if (breakItem.BreakType != BreakType.LineBreak && breakItem.BreakType != BreakType.TextWrappingBreak && (breakItem.BreakType != 0 || ((this != base.Document.LastParagraph || base.NextSibling != null) && !base.Document.Settings.CompatibilityOptions[CompatibilityOption.SplitPgBreakAndParaMark] && (base.Document.ActualFormatType != 0 || base.Document.WordVersion > 268))))
			{
				return breakItem.BreakType == BreakType.ColumnBreak;
			}
			return true;
		}
		return false;
	}

	internal bool IsContainDinOffcFont()
	{
		bool isDinFontOccur = false;
		string text = "";
		FontScriptType scriptType = FontScriptType.English;
		for (int i = 0; i < ChildEntities.Count; i++)
		{
			if (ChildEntities[i] is WTextRange)
			{
				scriptType = (ChildEntities[i] as WTextRange).ScriptType;
				text = (ChildEntities[i] as WTextRange).CharacterFormat.GetFontNameToRender(scriptType);
				if (!(text == "DIN Offc") && !(text == "DIN OT"))
				{
					return false;
				}
				isDinFontOccur = true;
			}
		}
		return IsDinFontCreatable(text, isDinFontOccur, scriptType);
	}

	internal bool IsDinFontCreatable(string fontName, bool isDinFontOccur, FontScriptType scriptType)
	{
		if (isDinFontOccur)
		{
			DocGen.Drawing.Font font = new DocGen.Drawing.Font(fontName, 10f, FontStyle.Regular);
			if (WordDocument.RenderHelper.GetFontName(font.Name, font.Size, font.Style, scriptType) == "DIN Offc")
			{
				return true;
			}
		}
		return false;
	}

	internal bool IsContainFloatingItems()
	{
		for (int i = 0; i < ChildEntities.Count; i++)
		{
			IEntity entity = ChildEntities[i];
			TextWrappingStyle textWrappingStyle = ((entity is WTextBox) ? (entity as WTextBox).TextBoxFormat.TextWrappingStyle : ((entity is WPicture) ? (entity as WPicture).TextWrappingStyle : ((entity is Shape) ? (entity as Shape).WrapFormat.TextWrappingStyle : ((entity is GroupShape) ? (entity as GroupShape).WrapFormat.TextWrappingStyle : ((entity is WOleObject && (entity as WOleObject).OlePicture != null) ? (entity as WOleObject).OlePicture.TextWrappingStyle : ((entity is WChart) ? (entity as WChart).WrapFormat.TextWrappingStyle : TextWrappingStyle.Inline))))));
			if (!(entity is BookmarkStart) && !(entity is BookmarkEnd) && !(entity is WFieldMark) && textWrappingStyle == TextWrappingStyle.Inline)
			{
				return false;
			}
		}
		return true;
	}

	internal bool IsLastItemBreak(ref int index)
	{
		ParagraphItemCollection paragraphItemCollection = null;
		paragraphItemCollection = ((!HasSDTInlineItem) ? m_pItemColl : GetParagraphItems());
		for (int num = paragraphItemCollection.Count - 1; num >= 0; num--)
		{
			if (!IsNonRenderableItem(paragraphItemCollection[num]))
			{
				if (paragraphItemCollection[num] is Break)
				{
					index = num;
					return true;
				}
				return false;
			}
		}
		return false;
	}

	public WParagraph(IWordDocument doc)
		: base((WordDocument)doc)
	{
		m_pItemColl = new ParagraphItemCollection(this);
		m_charFormat = new WCharacterFormat(base.Document);
		m_prFormat = new WParagraphFormat(base.Document);
		m_listFormat = new WListFormat(this);
		m_charFormat.SetOwner(this);
		m_prFormat.SetOwner(this);
		m_listFormat.SetOwner(this);
		ApplyStyle("Normal", isDomChanges: false);
		CreateEmptyParagraph();
	}

	internal int GetHeadingLevel(WParagraphStyle style, WParagraph currParagraph)
	{
		WParagraphStyle wParagraphStyle = style;
		if (!currParagraph.ParagraphFormat.PropertiesHash.ContainsKey(56))
		{
			while (wParagraphStyle != null)
			{
				if (wParagraphStyle.ParagraphFormat.IsBuiltInHeadingStyle(wParagraphStyle.Name))
				{
					string name = wParagraphStyle.Name;
					for (int i = 1; i < 10; i++)
					{
						if (Style.BuiltinStyleLoader.BuiltinStyleNames[i] == name)
						{
							return i - 1;
						}
					}
				}
				else if (wParagraphStyle.ParagraphFormat.PropertiesHash.ContainsKey(56))
				{
					return Convert.ToInt32(wParagraphStyle.ParagraphFormat.PropertiesHash[56]);
				}
				wParagraphStyle = wParagraphStyle.BaseStyle;
			}
		}
		else
		{
			if (!wParagraphStyle.ParagraphFormat.IsBuiltInHeadingStyle(wParagraphStyle.Name))
			{
				return Convert.ToInt32(currParagraph.ParagraphFormat.PropertiesHash[56]);
			}
			string name2 = wParagraphStyle.Name;
			for (int j = 1; j < 10; j++)
			{
				if (Style.BuiltinStyleLoader.BuiltinStyleNames[j] == name2)
				{
					return j - 1;
				}
			}
		}
		return 9;
	}

	private void MoveParagraphItems(WParagraph targetParagraph, WParagraph sourceParagraph, int startIndex)
	{
		int num = sourceParagraph.Items.Count - startIndex;
		for (int i = 0; i < num; i++)
		{
			Entity entity = sourceParagraph.ChildEntities[startIndex];
			if (entity is WTextRange && ModifyText((entity as WTextRange).Text).IndexOf('\r') != -1)
			{
				break;
			}
			if (entity is WField || entity is WFormField)
			{
				targetParagraph.ChildEntities.AddToInnerList(entity);
				entity.SetOwner(targetParagraph);
				sourceParagraph.ChildEntities.RemoveFromInnerList(startIndex);
				(entity as ParagraphItem).StartPos = (targetParagraph.ChildEntities.InnerList[targetParagraph.ChildEntities.InnerList.Count - 2] as ParagraphItem).EndPos;
			}
			else
			{
				targetParagraph.ChildEntities.Add(entity);
			}
		}
	}

	internal bool IsNeedToFitSymbol(WParagraph ownerParagraph)
	{
		Entity ownerEntity = ownerParagraph.GetOwnerEntity();
		AutoShapeType autoShapeType = ((ownerEntity is ChildShape) ? (ownerEntity as ChildShape).AutoShapeType : ((ownerEntity is Shape) ? (ownerEntity as Shape).AutoShapeType : AutoShapeType.Line));
		bool flag = autoShapeType == AutoShapeType.RoundedRectangle || autoShapeType == AutoShapeType.Oval || autoShapeType == AutoShapeType.IsoscelesTriangle;
		if (ownerEntity is WTextBox || flag)
		{
			return true;
		}
		return false;
	}

	internal bool HasInlineItem(int endIndex)
	{
		for (int num = endIndex; num >= 0; num--)
		{
			if (!Items[num].IsFloatingItem(isTextWrapAround: false))
			{
				return true;
			}
		}
		return false;
	}

	internal void SplitTextRange()
	{
		string text = ModifyText(Text);
		if (!text.Contains(ControlChar.CarriegeReturn))
		{
			return;
		}
		int num = 0;
		WParagraph wParagraph = null;
		bool flag = false;
		WCharacterFormat wCharacterFormat = new WCharacterFormat(base.Document);
		wCharacterFormat.ImportContainer(BreakCharacterFormat);
		wCharacterFormat.CopyProperties(BreakCharacterFormat);
		for (int i = 0; i < ChildEntities.Count; i++)
		{
			if (ChildEntities[i].EntityType != EntityType.TextRange)
			{
				continue;
			}
			WTextRange wTextRange = ChildEntities[i] as WTextRange;
			text = ModifyText(wTextRange.Text);
			if (text.IndexOf('\r') == -1 || !(wTextRange.Owner is WParagraph))
			{
				continue;
			}
			if (num > 0 && wParagraph != null)
			{
				wParagraph.ChildEntities.Add(wTextRange);
				i--;
			}
			string[] array = text.Split('\r');
			wTextRange.Text = array[0];
			int indexInOwnerCollection = GetIndexInOwnerCollection();
			WTextRange wTextRange2 = null;
			for (int j = 1; j < array.Length; j++)
			{
				wParagraph = new WParagraph(base.Document);
				wTextRange2 = new WTextRange(base.Document);
				WTextBody obj = wTextRange.OwnerParagraph.Owner as WTextBody;
				wParagraph.ParagraphFormat.ImportContainer(ParagraphFormat);
				wParagraph.ParagraphFormat.CopyProperties(ParagraphFormat);
				wParagraph.ListFormat.ImportContainer(ListFormat);
				obj.ChildEntities.Insert(indexInOwnerCollection + j + num, wParagraph);
				wTextRange2.Text = array[j];
				wTextRange2.CharacterFormat.ImportContainer(wTextRange.CharacterFormat);
				wTextRange2.CharacterFormat.CopyProperties(wTextRange.CharacterFormat);
				wParagraph.Items.Insert(0, wTextRange2);
				if (StyleName != null)
				{
					wParagraph.ApplyStyle(StyleName, isDomChanges: false);
				}
				if (j == array.Length - 1)
				{
					wParagraph.BreakCharacterFormat.ImportContainer(wCharacterFormat);
					wParagraph.BreakCharacterFormat.CopyProperties(wCharacterFormat);
				}
				else
				{
					wParagraph.BreakCharacterFormat.ImportContainer(wTextRange2.CharacterFormat);
					wParagraph.BreakCharacterFormat.CopyProperties(wTextRange2.CharacterFormat);
				}
			}
			num += array.Length - 1;
			MoveParagraphItems(wParagraph, this, i + 1);
			if (!flag)
			{
				flag = true;
				BreakCharacterFormat.ClearFormatting();
				BreakCharacterFormat.ImportContainer(wTextRange.CharacterFormat);
				BreakCharacterFormat.CopyProperties(wTextRange.CharacterFormat);
			}
			if (i < ChildEntities.Count - 1)
			{
				wParagraph.BreakCharacterFormat.ImportContainer(wTextRange2.CharacterFormat);
				wParagraph.BreakCharacterFormat.CopyProperties(wTextRange2.CharacterFormat);
			}
		}
	}

	internal void InsertBreak(BreakType breakType)
	{
		if (base.Owner == null)
		{
			return;
		}
		switch (breakType)
		{
		case BreakType.PageBreak:
			ParagraphFormat.PageBreakAfter = false;
			break;
		case BreakType.ColumnBreak:
			ParagraphFormat.ColumnBreakAfter = false;
			break;
		}
		if (base.NextSibling is WParagraph)
		{
			(base.NextSibling as WParagraph).Items.Insert(0, new Break(base.Document, breakType));
			return;
		}
		int indexInOwnerCollection = GetIndexInOwnerCollection();
		WParagraph wParagraph = new WParagraph(base.Document);
		wParagraph.AppendBreak(breakType);
		wParagraph.AppendText(" ");
		ICompositeEntity compositeEntity = base.Owner as ICompositeEntity;
		if (compositeEntity.ChildEntities.Count == indexInOwnerCollection + 1)
		{
			compositeEntity.ChildEntities.Add(wParagraph);
		}
		else
		{
			compositeEntity.ChildEntities.Insert(indexInOwnerCollection + 1, wParagraph);
		}
	}

	public void ApplyStyle(string styleName)
	{
		ApplyStyle(styleName, isDomChanges: true);
	}

	internal void ApplyStyle(string styleName, bool isDomChanges)
	{
		IsStyleApplied = true;
		IStyle style = base.Document.Styles.FindByName(styleName, StyleType.ParagraphStyle) as IWParagraphStyle;
		if (style == null && styleName == "Normal")
		{
			style = (WParagraphStyle)Style.CreateBuiltinStyle(BuiltinStyle.Normal, base.Document);
		}
		if (style != null)
		{
			ApplyStyle(style as WParagraphStyle, isDomChanges);
			return;
		}
		style = base.Document.Styles.FindByName(styleName, StyleType.CharacterStyle) as WCharacterStyle;
		if (style == null && styleName == "Default Paragraph Font")
		{
			style = (WCharacterStyle)Style.CreateBuiltinCharacterStyle(BuiltinStyle.DefaultParagraphFont, base.Document);
		}
		if (style != null)
		{
			ApplyCharacterStyle(style as IWCharacterStyle);
		}
		if (style == null)
		{
			style = base.Document.Styles.FindByName(styleName, StyleType.NumberingStyle);
			if (style != null)
			{
				ApplyStyle("List Paragraph", isDomChanges);
				ListFormat.ApplyStyle((style as WNumberingStyle).ListFormat.CurrentListStyle.Name);
				if (!string.IsNullOrEmpty((style as WNumberingStyle).ListFormat.LFOStyleName))
				{
					ListFormat.LFOStyleName = (style as WNumberingStyle).ListFormat.LFOStyleName;
				}
			}
		}
		if (style != null)
		{
			return;
		}
		throw new ArgumentException("Specified style does not exist in the document style collection");
	}

	public void ApplyStyle(BuiltinStyle builtinStyle)
	{
		ApplyStyle(builtinStyle, isDomChanges: true);
	}

	internal void ApplyStyle(BuiltinStyle builtinStyle, bool isDomChanges)
	{
		bool num = Style.IsListStyle(builtinStyle);
		CheckNormalStyle();
		if (num)
		{
			ApplyListStyle(builtinStyle);
			return;
		}
		string name = Style.BuiltInToName(builtinStyle);
		if (IsBuiltInCharacterStyle(builtinStyle))
		{
			IStyle style = base.Document.Styles.FindByName(name, StyleType.CharacterStyle) as WCharacterStyle;
			if (style == null)
			{
				style = (WCharacterStyle)Style.CreateBuiltinCharacterStyle(builtinStyle, base.Document);
				if ((style as WCharacterStyle).StyleId > 10)
				{
					(style as WCharacterStyle).StyleId = 4094;
				}
				(style as Style).UnhideWhenUsed = true;
				base.Document.Styles.Add(style);
				(style as WCharacterStyle).ApplyBaseStyle("Default Paragraph Font");
			}
			ApplyCharacterStyle(style as WCharacterStyle);
			return;
		}
		IStyle style2 = base.Document.Styles.FindByName(name, StyleType.ParagraphStyle) as IWParagraphStyle;
		if (style2 == null)
		{
			style2 = (IWParagraphStyle)Style.CreateBuiltinStyle(builtinStyle, base.Document);
			if ((style2 as WParagraphStyle).StyleId > 10)
			{
				(style2 as WParagraphStyle).StyleId = 4094;
			}
			base.Document.Styles.Add(style2);
			if (builtinStyle != BuiltinStyle.MacroText && builtinStyle != BuiltinStyle.CommentSubject)
			{
				(style2 as WParagraphStyle).ApplyBaseStyle("Normal");
			}
			base.Document.UpdateNextStyle(style2 as Style);
		}
		ApplyStyle(style2 as IWParagraphStyle, isDomChanges);
	}

	public IWParagraphStyle GetStyle()
	{
		return m_style;
	}

	public void RemoveAbsPosition()
	{
		if (m_prFormat != null)
		{
			m_prFormat.RemovePositioning();
		}
	}

	internal bool IsExactlyRowHeight(WTableCell ownerTableCell, ref float rowHeight)
	{
		if ((((IWidget)ownerTableCell).LayoutInfo as CellLayoutInfo).IsRowMergeStart)
		{
			WTableRow ownerRow = ownerTableCell.GetOwnerRow(ownerTableCell);
			if (ownerRow != null)
			{
				rowHeight = ownerRow.Height;
				return ownerRow.HeightType == TableRowHeightType.Exactly;
			}
		}
		else
		{
			if (ownerTableCell == null || ownerTableCell.OwnerRow == null || ownerTableCell.OwnerRow.HeightType == TableRowHeightType.Exactly || ownerTableCell.OwnerRow.OwnerTable == null || !ownerTableCell.OwnerRow.OwnerTable.IsInCell)
			{
				return ownerTableCell.OwnerRow.HeightType == TableRowHeightType.Exactly;
			}
			ownerTableCell = ownerTableCell.OwnerRow.OwnerTable.GetOwnerTableCell();
			if (ownerTableCell != null)
			{
				rowHeight = ownerTableCell.OwnerRow.Height;
				return IsExactlyRowHeight(ownerTableCell, ref rowHeight);
			}
		}
		return false;
	}

	internal bool IsExactlyRowHeight()
	{
		if (IsInCell)
		{
			float rowHeight = 0f;
			return IsExactlyRowHeight(GetOwnerEntity() as WTableCell, ref rowHeight);
		}
		return false;
	}

	public IWTextRange AppendText(string text)
	{
		IWTextRange obj = AppendItem(ParagraphItemType.TextRange) as IWTextRange;
		obj.Text = text;
		return obj;
	}

	public IInlineContentControl AppendInlineContentControl(ContentControlType controlType)
	{
		switch (controlType)
		{
		case ContentControlType.BuildingBlockGallery:
		case ContentControlType.Group:
		case ContentControlType.RepeatingSection:
			throw new NotImplementedException("Creating a content control for the " + controlType.ToString() + "type is not implemented");
		default:
		{
			InlineContentControl inlineContentControl = AppendItem(ParagraphItemType.InlineContentControl) as InlineContentControl;
			inlineContentControl.ContentControlProperties.Type = controlType;
			if (controlType == ContentControlType.CheckBox)
			{
				inlineContentControl.ContentControlProperties.CheckedState.Value = '☒'.ToString();
				inlineContentControl.ContentControlProperties.CheckedState.Font = "MS Gothic";
				inlineContentControl.ContentControlProperties.UncheckedState.Value = '☐'.ToString();
				inlineContentControl.ContentControlProperties.UncheckedState.Font = "MS Gothic";
			}
			HasSDTInlineItem = true;
			return inlineContentControl;
		}
		}
	}

	public IWPicture AppendPicture(byte[] imageBytes)
	{
		IWPicture obj = (IWPicture)AppendItem(ParagraphItemType.Picture);
		obj.LoadImage(imageBytes);
		base.Document.HasPicture = true;
		return obj;
	}

	public IWPicture AppendPicture(byte[] svgData, byte[] imageBytes)
	{
		WPicture.EvaluateSVGImageBytes(svgData);
		WPicture obj = AppendItem(ParagraphItemType.Picture) as WPicture;
		obj.LoadImage(svgData, imageBytes);
		base.Document.HasPicture = true;
		return obj;
	}

	public WChart AppendChart(object[][] data, float width, float height)
	{
		WChart obj = AppendItem(ParagraphItemType.Chart) as WChart;
		obj.OfficeChart.SetChartData(data);
		obj.Width = width;
		obj.Height = height;
		return obj;
	}

	public WChart AppendChart(float width, float height)
	{
		WChart obj = AppendItem(ParagraphItemType.Chart) as WChart;
		obj.Width = width;
		obj.Height = height;
		return obj;
	}

	public WChart AppendChart(Stream excelStream, int sheetNumber, string dataRange, float width, float height)
	{
		DetectExcelFileFromStream(excelStream);
		if (sheetNumber <= 0)
		{
			throw new ArgumentOutOfRangeException("Sheet number should be greater than zero");
		}
		if (string.IsNullOrEmpty(dataRange))
		{
			throw new ArgumentNullException("Data range should not be null");
		}
		WChart obj = AppendItem(ParagraphItemType.Chart) as WChart;
		obj.Workbook.DataHolder.ParseDocument(excelStream);
		obj.Width = width;
		obj.Height = height;
		obj.SetDataRange(sheetNumber, dataRange);
		return obj;
	}

	private void DetectExcelFileFromStream(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("Excelstream should not be null");
		}
		long position = stream.Position;
		if (ZipArchive.ReadInt32(stream) != 67324752)
		{
			throw new ArgumentException("Excel stream should be *.xlsx format");
		}
		stream.Position = position;
	}

	public IWField AppendField(string fieldName, FieldType fieldType)
	{
		if (fieldName == null)
		{
			throw new ArgumentNullException("fieldName");
		}
		WField wField;
		switch (fieldType)
		{
		case FieldType.FieldFormCheckBox:
			return AppendCheckBox(fieldName, defaultCheckBoxValue: false);
		case FieldType.FieldFormDropDown:
			return AppendDropDownFormField(fieldName);
		case FieldType.FieldFormTextInput:
			return AppendTextFormField(fieldName, fieldName);
		case FieldType.FieldIndexEntry:
			return AppendIndexEntry(fieldName);
		case FieldType.FieldMergeField:
			return AppendMergeField(fieldName);
		case FieldType.FieldSequence:
			wField = new WSeqField(base.Document);
			break;
		case FieldType.FieldIf:
			wField = new WIfField(base.Document);
			break;
		default:
			wField = new WField(base.Document);
			break;
		}
		wField.SetFieldTypeValue(fieldType);
		if (wField.FieldType == FieldType.FieldFormula)
		{
			fieldName = fieldName.Replace(" ", string.Empty);
			fieldName = fieldName.Replace("\"", string.Empty);
			fieldName = fieldName.Replace("=", string.Empty);
		}
		WTextRange wTextRange = new WTextRange(m_doc);
		string text = FieldTypeDefiner.GetFieldCode(fieldType) + " ";
		string text2 = fieldName.TrimStart();
		if (text2.StartsWith(text))
		{
			text2 = text2.Remove(0, text.Length);
		}
		if (text2.IndexOf(' ') != -1 && wField.FieldType != FieldType.FieldIndex)
		{
			wField.m_fieldValue = "\"" + text2 + "\"";
		}
		else
		{
			wField.m_fieldValue = text2;
		}
		wTextRange.Text = text + wField.m_fieldValue;
		if (!text2.Contains(" \\* MERGEFORMAT"))
		{
			FieldType fieldType2 = wField.FieldType;
			if ((uint)(fieldType2 - 15) <= 1u || fieldType2 == FieldType.FieldDocVariable)
			{
				wTextRange.Text += " \\* MERGEFORMAT";
			}
		}
		if (wField.AddFormattingString())
		{
			wField.m_formattingString = "\\* MERGEFORMAT";
		}
		m_pItemColl.Add(wField);
		wTextRange.ApplyCharacterFormat(wField.CharacterFormat);
		m_pItemColl.Add(wTextRange);
		if (!wField.IsFieldWithoutSeparator)
		{
			wField.FieldSeparator = AppendFieldMark(FieldMarkType.FieldSeparator);
			if (fieldType != FieldType.FieldSequence)
			{
				WTextRange wTextRange2 = new WTextRange(base.Document);
				wTextRange2.Text = fieldName;
				m_pItemColl.Add(wTextRange2);
				if (fieldType == FieldType.FieldHyperlink)
				{
					wTextRange2.CharacterFormat.TextColor = Color.Blue;
					wTextRange2.CharacterFormat.UnderlineStyle = UnderlineStyle.Single;
				}
			}
		}
		WFieldMark wFieldMark = new WFieldMark(base.Document, FieldMarkType.FieldEnd);
		m_pItemColl.Add(wFieldMark);
		wField.FieldEnd = wFieldMark;
		return wField;
	}

	public IWField AppendHyperlink(string link, string text, HyperlinkType type)
	{
		return AppendHyperlink(link, text, null, type);
	}

	public IWField AppendHyperlink(string link, WPicture picture, HyperlinkType type)
	{
		return AppendHyperlink(link, null, picture, type);
	}

	public BookmarkStart AppendBookmarkStart(string name)
	{
		BookmarkStart bookmarkStart = new BookmarkStart(base.Document, name);
		Items.Add(bookmarkStart);
		return bookmarkStart;
	}

	public BookmarkEnd AppendBookmarkEnd(string name)
	{
		BookmarkEnd bookmarkEnd = new BookmarkEnd(base.Document, name);
		Items.Add(bookmarkEnd);
		return bookmarkEnd;
	}

	internal EditableRangeEnd AppendEditableRangeEnd(string id)
	{
		EditableRangeEnd editableRangeEnd = new EditableRangeEnd(base.Document, id);
		Items.Add(editableRangeEnd);
		return editableRangeEnd;
	}

	public WComment AppendComment(string text)
	{
		WComment obj = (WComment)AppendItem(ParagraphItemType.Comment);
		obj.TextBody.AddParagraph().AppendText(text);
		return obj;
	}

	public WFootnote AppendFootnote(FootnoteType type)
	{
		WFootnote obj = (WFootnote)AppendItem(ParagraphItemType.Footnote);
		obj.FootnoteType = type;
		obj.EnsureFtnStyle();
		return obj;
	}

	public IWTextBox AppendTextBox(float width, float height)
	{
		IWTextBox obj = AppendItem(ParagraphItemType.TextBox) as IWTextBox;
		obj.TextBoxFormat.Width = width;
		obj.TextBoxFormat.Height = height;
		return obj;
	}

	public WCheckBox AppendCheckBox()
	{
		string text = "Check_" + Guid.NewGuid().ToString().Replace("-", "_");
		text = text.Substring(0, 20);
		return AppendCheckBox(text, defaultCheckBoxValue: false);
	}

	public WCheckBox AppendCheckBox(string checkBoxName, bool defaultCheckBoxValue)
	{
		WCheckBox wCheckBox = base.Document.CreateParagraphItem(ParagraphItemType.CheckBox) as WCheckBox;
		wCheckBox.Name = checkBoxName;
		wCheckBox.DefaultCheckBoxValue = defaultCheckBoxValue;
		Items.Add(wCheckBox);
		return wCheckBox;
	}

	public WTextFormField AppendTextFormField(string defaultText)
	{
		string text = "Text_" + Guid.NewGuid().ToString().Replace("-", "_");
		text = text.Substring(0, 20);
		return AppendTextFormField(text, defaultText);
	}

	public WTextFormField AppendTextFormField(string formFieldName, string defaultText)
	{
		WTextFormField wTextFormField = base.Document.CreateParagraphItem(ParagraphItemType.TextFormField) as WTextFormField;
		wTextFormField.Name = formFieldName;
		Items.Add(wTextFormField);
		if (string.IsNullOrEmpty(defaultText))
		{
			wTextFormField.DefaultText = "\u2002\u2002\u2002\u2002\u2002";
			wTextFormField.Text = "\u2002\u2002\u2002\u2002\u2002";
		}
		else
		{
			wTextFormField.DefaultText = defaultText;
			wTextFormField.Text = defaultText;
		}
		return wTextFormField;
	}

	public WDropDownFormField AppendDropDownFormField()
	{
		string text = "Drop_" + Guid.NewGuid().ToString().Replace("-", "_");
		text = text.Substring(0, 20);
		return AppendDropDownFormField(text);
	}

	public WDropDownFormField AppendDropDownFormField(string dropDropDownName)
	{
		WDropDownFormField wDropDownFormField = base.Document.CreateParagraphItem(ParagraphItemType.DropDownFormField) as WDropDownFormField;
		wDropDownFormField.Name = dropDropDownName;
		Items.Add(wDropDownFormField);
		return wDropDownFormField;
	}

	public WSymbol AppendSymbol(byte characterCode)
	{
		WSymbol obj = (WSymbol)AppendItem(ParagraphItemType.Symbol);
		obj.CharacterCode = characterCode;
		return obj;
	}

	public Break AppendBreak(BreakType breakType)
	{
		Break @break = new Break(base.Document, breakType);
		if (breakType == BreakType.TextWrappingBreak)
		{
			@break.CharacterFormat.BreakClear = BreakClearType.All;
		}
		Items.Add(@break);
		return @break;
	}

	public Shape AppendShape(AutoShapeType autoShapeType, float width, float height)
	{
		Shape shape = new Shape(base.Document, autoShapeType);
		shape.Width = width;
		shape.Height = height;
		Items.Add(shape);
		return shape;
	}

	public TableOfContent AppendTOC(int lowerHeadingLevel, int upperHeadingLevel)
	{
		TableOfContent tableOfContent = AppendItem(ParagraphItemType.TOC) as TableOfContent;
		AppendText("TOC ");
		tableOfContent.LowerHeadingLevel = lowerHeadingLevel;
		tableOfContent.UpperHeadingLevel = upperHeadingLevel;
		tableOfContent.TOCField.FieldSeparator = AppendFieldMark(FieldMarkType.FieldSeparator);
		AppendText("TOC");
		tableOfContent.TOCField.FieldEnd = AppendFieldMark(FieldMarkType.FieldEnd);
		if (!base.Document.TOC.ContainsKey(tableOfContent.TOCField))
		{
			base.Document.TOC.Add(tableOfContent.TOCField, tableOfContent);
		}
		tableOfContent.UpdateTOCFieldCode();
		return tableOfContent;
	}

	public void AppendCrossReference(ReferenceType referenceType, ReferenceKind referenceKind, Entity referenceEntity, bool insertAsHyperlink, bool includePosition, bool separatorNumber, string separatorString)
	{
		FieldType fieldType = FieldType.FieldNone;
		string empty = string.Empty;
		string empty2 = string.Empty;
		if (referenceKind == ReferenceKind.PageNumber)
		{
			fieldType = FieldType.FieldPageRef;
			empty = "PAGEREF ";
			empty2 = "PageRef field";
		}
		else
		{
			fieldType = FieldType.FieldRef;
			empty = "Ref ";
			empty2 = "Ref field";
		}
		if (referenceEntity is BookmarkStart)
		{
			empty = empty + (referenceEntity as BookmarkStart).Name + " ";
		}
		switch (referenceKind)
		{
		case ReferenceKind.AboveBelow:
			empty += "\\p ";
			break;
		case ReferenceKind.NumberRelativeContext:
			empty += "\\r ";
			break;
		case ReferenceKind.NumberNoContext:
			empty += "\\n ";
			break;
		case ReferenceKind.NumberFullContext:
			empty += "\\w ";
			break;
		}
		if (includePosition)
		{
			empty += "\\p ";
		}
		if (insertAsHyperlink)
		{
			empty += "\\h ";
		}
		if (separatorNumber)
		{
			empty += "\\d ";
		}
		if (separatorNumber && separatorString != null)
		{
			empty += separatorString;
		}
		((AppendField(empty2, fieldType) as WField).NextSibling as WTextRange).Text = empty;
	}

	public IWPicture AppendPicture(Stream imageStream)
	{
		WPicture obj = AppendItem(ParagraphItemType.Picture) as WPicture;
		obj.LoadImage(imageStream);
		return obj;
	}

	public void AppendHTML(string html)
	{
		base.Document.IsOpening = true;
		IsAppendingHTML = true;
		string text = html.ToLower();
		if (!StartsWithExt(text, "<html") && !StartsWithExt(text, "<body") && !StartsWithExt(text, "<!doctype") && !StartsWithExt(text, "<?xml"))
		{
			html = "<html><head><title/></head><body>" + html + "</body></html>";
		}
		IHtmlConverter instance = HtmlConverterFactory.GetInstance();
		(instance as HTMLConverterImpl).HtmlImportSettings = base.Document.HTMLImportSettings;
		if (IsStyleApplied)
		{
			instance.AppendToTextBody(base.OwnerTextBody, html, GetIndexInOwnerCollection(), Items.Count, ParaStyle, ListFormat.CurrentListStyle);
		}
		else
		{
			instance.AppendToTextBody(base.OwnerTextBody, html, GetIndexInOwnerCollection(), Items.Count, null, ListFormat.CurrentListStyle);
		}
		base.Document.IsOpening = false;
		IsAppendingHTML = false;
	}

	public WOleObject AppendOleObject(Stream oleStream, WPicture olePicture, OleObjectType type)
	{
		if (oleStream == null || oleStream.Length == 0L)
		{
			return null;
		}
		if (type == OleObjectType.Package)
		{
			throw new ArgumentException("Please use AppendOleObject(Stream oleStream, WPicture olePicture, string fileExtension) method.  Package type is invalid in this context.");
		}
		oleStream.Position = 0L;
		WOleObject wOleObject = AppendItem(ParagraphItemType.OleObject) as WOleObject;
		wOleObject.SetOlePicture(olePicture);
		wOleObject.SetLinkType(OleLinkType.Embed);
		wOleObject.ObjectType = OleTypeConvertor.ToString(type, isWord2003: false);
		wOleObject.OleObjectType = type;
		wOleObject.ParseOleStream(oleStream);
		wOleObject.Field.FieldType = FieldType.FieldEmbed;
		wOleObject.AddFieldCodeText();
		WFieldMark wFieldMark = new WFieldMark(m_doc);
		wFieldMark.Type = FieldMarkType.FieldSeparator;
		Items.Add(wFieldMark);
		Items.Add(wOleObject.OlePicture);
		wOleObject.Field.FieldSeparator = wFieldMark;
		wOleObject.Field.FieldEnd = AppendFieldMark(FieldMarkType.FieldEnd);
		return wOleObject;
	}

	public WOleObject AppendOleObject(byte[] oleBytes, WPicture olePicture, OleObjectType type)
	{
		if (oleBytes == null || oleBytes.Length == 0)
		{
			return null;
		}
		if (type == OleObjectType.Package)
		{
			throw new ArgumentException("Please use AppendOleObject(byte[] oleBytes, WPicture olePicture, string fileExtension) method.  Package type is invalid in this context.");
		}
		MemoryStream oleStream = new MemoryStream(oleBytes);
		return AppendOleObject(oleStream, olePicture, type);
	}

	public WOleObject AppendOleObject(Stream oleStream, WPicture olePicture, OleLinkType oleLinkType)
	{
		WOleObject wOleObject = AppendItem(ParagraphItemType.OleObject) as WOleObject;
		wOleObject.SetOlePicture(olePicture);
		wOleObject.SetLinkType(oleLinkType);
		oleStream.Position = 0L;
		wOleObject.ParseOleStream(oleStream);
		if (oleLinkType == OleLinkType.Embed)
		{
			wOleObject.Field.FieldType = FieldType.FieldEmbed;
		}
		else
		{
			wOleObject.Field.FieldType = FieldType.FieldLink;
		}
		wOleObject.AddFieldCodeText();
		WFieldMark wFieldMark = new WFieldMark(m_doc);
		wFieldMark.Type = FieldMarkType.FieldSeparator;
		Items.Add(wFieldMark);
		Items.Add(wOleObject.OlePicture);
		wOleObject.Field.FieldSeparator = wFieldMark;
		wOleObject.Field.FieldEnd = AppendFieldMark(FieldMarkType.FieldEnd);
		return wOleObject;
	}

	public WOleObject AppendOleObject(byte[] oleBytes, WPicture olePicture, OleLinkType oleLinkType)
	{
		MemoryStream oleStream = new MemoryStream(oleBytes);
		return AppendOleObject(oleStream, olePicture, oleLinkType);
	}

	public WOleObject AppendOleObject(byte[] oleBytes, WPicture olePicture, string fileExtension)
	{
		WOleObject wOleObject = new WOleObject(m_doc);
		Items.Add(wOleObject);
		wOleObject.SetOlePicture(olePicture);
		wOleObject.SetLinkType(OleLinkType.Embed);
		wOleObject.ObjectType = OleTypeConvertor.ToString(OleObjectType.Package, isWord2003: false);
		wOleObject.OleObjectType = OleObjectType.Package;
		string dataPath = "Package." + fileExtension.Replace(".", string.Empty);
		wOleObject.CreateOleObjContainer(oleBytes, dataPath);
		wOleObject.Field.FieldType = FieldType.FieldEmbed;
		wOleObject.AddFieldCodeText();
		WFieldMark wFieldMark = new WFieldMark(m_doc);
		wFieldMark.Type = FieldMarkType.FieldSeparator;
		Items.Add(wFieldMark);
		Items.Add(wOleObject.OlePicture);
		wOleObject.Field.FieldSeparator = wFieldMark;
		wOleObject.Field.FieldEnd = AppendFieldMark(FieldMarkType.FieldEnd);
		return wOleObject;
	}

	public WOleObject AppendOleObject(Stream oleStream, WPicture olePicture, string fileExtension)
	{
		oleStream.Position = 0L;
		byte[] array = new byte[oleStream.Length];
		oleStream.Read(array, 0, array.Length);
		return AppendOleObject(array, olePicture, fileExtension);
	}

	public WMath AppendMath()
	{
		return AppendItem(ParagraphItemType.Math) as WMath;
	}

	public WMath AppendMath(string laTeX)
	{
		WMath obj = AppendItem(ParagraphItemType.Math) as WMath;
		(obj.MathParagraph as OfficeMathParagraph).ConvertLaTeXToMath(laTeX, m_doc.DocxLaTeXConveter);
		return obj;
	}

	internal Entity GetOwnerEntity()
	{
		Entity owner = base.Owner;
		while (!(owner is WTextBox) && !(owner is Shape) && !(owner is WSection) && !(owner is WTableCell) && !(owner is ChildShape))
		{
			if (owner == null)
			{
				return owner;
			}
			owner = owner.Owner;
		}
		return owner;
	}

	internal bool OmitHeadingStyles()
	{
		Entity owner = base.Owner;
		while (!(owner is HeaderFooter) && !(owner is WFootnote))
		{
			if (owner == null)
			{
				return false;
			}
			owner = owner.Owner;
		}
		return true;
	}

	internal bool IsInHeaderFooter()
	{
		Entity owner = base.Owner;
		while (!(owner is WSection))
		{
			if (owner == null)
			{
				return false;
			}
			if (owner is HeaderFooter)
			{
				return true;
			}
			owner = owner.Owner;
		}
		return false;
	}

	internal WParagraph GetFirstParagraphInOwnerTextbody(WTextBody textbody)
	{
		WParagraph result = null;
		WTable wTable = ((textbody is WTableCell { OwnerRow: not null } wTableCell && wTableCell.OwnerRow.OwnerTable != null) ? wTableCell.OwnerRow.OwnerTable : null);
		if (wTable != null && wTable.Rows.Count > 0 && wTable.Rows[0].Cells.Count > 0)
		{
			WTableCell wTableCell2 = wTable.Rows[0].Cells[0];
			if (wTableCell2.Items.Count > 0 && wTableCell2.Items[0] is WParagraph)
			{
				return wTableCell2.Paragraphs[0].ParagraphFormat.IsFrame ? wTableCell2.Paragraphs[0] : null;
			}
			if (wTableCell2.Items.Count > 0 && wTableCell2.Items[0] is WTable && wTableCell2.Tables[0].Rows.Count > 0 && wTableCell2.Tables[0].Rows[0].Cells.Count > 0)
			{
				result = GetFirstParagraphInOwnerTextbody(wTableCell2.Tables[0].Rows[0].Cells[0]);
			}
		}
		return result;
	}

	internal WFieldMark AppendFieldMark(FieldMarkType type)
	{
		WFieldMark obj = (WFieldMark)AppendItem(ParagraphItemType.FieldMark);
		obj.Type = type;
		return obj;
	}

	internal Break AppendLineBreak(string lineBreakText)
	{
		Break @break = new Break(base.Document, BreakType.LineBreak);
		@break.TextRange.Text = lineBreakText;
		Items.Add(@break);
		return @break;
	}

	internal IWField AppendHyperlink(string link, string text, WPicture pict, HyperlinkType type)
	{
		WField wField = new WField(base.Document);
		wField.FieldType = FieldType.FieldHyperlink;
		Items.Add(wField);
		IWTextRange iWTextRange = AppendText(string.Empty);
		wField.FieldSeparator = AppendFieldMark(FieldMarkType.FieldSeparator);
		if (text != null)
		{
			IWTextRange iWTextRange2 = AppendText(text);
			if (m_doc.Styles.FindByName(BuiltinStyle.Hyperlink.ToString()) == null)
			{
				IStyle style = Style.CreateBuiltinStyle(BuiltinStyle.Hyperlink, StyleType.CharacterStyle, m_doc);
				m_doc.Styles.Add(style);
			}
			iWTextRange2.CharacterFormat.CharStyleName = BuiltinStyle.Hyperlink.ToString();
		}
		else if (pict != null)
		{
			Items.Add(pict);
		}
		else
		{
			AppendText("Hyperlink");
		}
		WFieldMark wFieldMark = new WFieldMark(base.Document, FieldMarkType.FieldEnd);
		Items.Add(wFieldMark);
		wField.FieldEnd = wFieldMark;
		Hyperlink hyperlink = new Hyperlink(wField);
		hyperlink.Type = type;
		if (type == HyperlinkType.WebLink || type == HyperlinkType.EMailLink)
		{
			hyperlink.Uri = link;
		}
		else if (hyperlink.Type == HyperlinkType.Bookmark)
		{
			hyperlink.BookmarkName = link;
		}
		else if (hyperlink.Type == HyperlinkType.FileLink)
		{
			hyperlink.FilePath = link;
		}
		else
		{
			iWTextRange.Text = "HYPERLINK " + wField.FieldValue + " " + wField.LocalReference;
		}
		return wField;
	}

	internal void LoadPicture(WPicture picture, ImageRecord imageRecord)
	{
		picture.LoadImage(imageRecord);
		base.Document.HasPicture = true;
	}

	internal IWField AppendIndexEntry(string entryToMark)
	{
		WField wField = new WField(base.Document);
		wField.SetFieldTypeValue(FieldType.FieldIndexEntry);
		wField.CharacterFormat.FieldVanish = true;
		Items.Add(wField);
		AppendText("XE \"" + entryToMark + "\"").CharacterFormat.FieldVanish = true;
		WFieldMark wFieldMark = AppendFieldMark(FieldMarkType.FieldEnd);
		wFieldMark.CharacterFormat.FieldVanish = true;
		wField.FieldEnd = wFieldMark;
		return wField;
	}

	internal WMergeField AppendMergeField(string fieldName)
	{
		WMergeField wMergeField = new WMergeField(base.Document);
		if (!StartsWithExt(fieldName.ToUpper().TrimStart(), "MERGEFIELD "))
		{
			fieldName = "MERGEFIELD " + fieldName;
		}
		Items.Add(wMergeField);
		wMergeField.FieldCode = fieldName;
		return wMergeField;
	}

	internal WListFormat GetListFormatValue()
	{
		WListFormat result = null;
		WParagraphStyle wParagraphStyle = ParaStyle as WParagraphStyle;
		IEntity nextListPara = null;
		if (ListFormat.ListType != ListType.NoList)
		{
			result = ListFormat;
		}
		else if (!ListFormat.IsEmptyList)
		{
			while (wParagraphStyle != null)
			{
				if (wParagraphStyle.ListFormat.ListType != ListType.NoList || wParagraphStyle.ListFormat.IsEmptyList)
				{
					result = wParagraphStyle.ListFormat;
					break;
				}
				wParagraphStyle = wParagraphStyle.BaseStyle;
			}
		}
		if (BreakCharacterFormat != null && IsParagraphMarkDeleted() && !IsDeletionParagraph() && GetNextNonDeleteRevisionListParagraph(ref nextListPara) && nextListPara != null)
		{
			result = (nextListPara as WParagraph).ListFormat;
		}
		return result;
	}

	internal WListFormat GetListFormat(ref WListLevel listLevel, ref int tabLevelIndex, ref float? leftIndent, ref float? firstLineIndent)
	{
		WListFormat wListFormat = null;
		if (ListFormat.ListType != ListType.NoList || ListFormat.IsEmptyList)
		{
			wListFormat = ListFormat;
		}
		int? num = null;
		if (ListFormat.HasKey(0))
		{
			num = ListFormat.ListLevelNumber;
		}
		if (ParagraphFormat.HasValue(2))
		{
			leftIndent = ParagraphFormat.LeftIndent;
		}
		if (ParagraphFormat.HasValue(5))
		{
			firstLineIndent = ParagraphFormat.FirstLineIndent;
		}
		WParagraphStyle wParagraphStyle = ParaStyle as WParagraphStyle;
		while ((wListFormat == null || !num.HasValue) && wParagraphStyle != null)
		{
			if (wListFormat == null && !num.HasValue && wParagraphStyle.ParagraphFormat != null)
			{
				if (!leftIndent.HasValue && wParagraphStyle.ParagraphFormat.HasValue(2))
				{
					leftIndent = wParagraphStyle.ParagraphFormat.LeftIndent;
				}
				if (!firstLineIndent.HasValue && wParagraphStyle.ParagraphFormat.HasValue(5))
				{
					firstLineIndent = wParagraphStyle.ParagraphFormat.FirstLineIndent;
				}
			}
			if (wListFormat == null && (wParagraphStyle.ListFormat.ListType != ListType.NoList || wParagraphStyle.ListFormat.IsEmptyList))
			{
				wListFormat = wParagraphStyle.ListFormat;
			}
			if (!num.HasValue && wParagraphStyle.ListFormat.HasKey(0))
			{
				num = wParagraphStyle.ListFormat.ListLevelNumber;
				if (wParagraphStyle.ParagraphFormat.Tabs.Count > 0)
				{
					tabLevelIndex++;
				}
			}
			wParagraphStyle = wParagraphStyle.BaseStyle;
		}
		if (wListFormat == null || wListFormat.CurrentListStyle == null)
		{
			return wListFormat;
		}
		if (!num.HasValue)
		{
			num = 0;
		}
		ListStyle currentListStyle = wListFormat.CurrentListStyle;
		listLevel = currentListStyle.GetNearLevel(num.Value);
		ListOverrideStyle listOverrideStyle = null;
		if (wListFormat.LFOStyleName != null && wListFormat.LFOStyleName.Length > 0)
		{
			listOverrideStyle = base.Document.ListOverrides.FindByName(wListFormat.LFOStyleName);
		}
		if (listOverrideStyle != null && listOverrideStyle.OverrideLevels.HasOverrideLevel(num.Value) && listOverrideStyle.OverrideLevels[num.Value].OverrideFormatting)
		{
			listLevel = listOverrideStyle.OverrideLevels[num.Value].OverrideListLevel;
		}
		num = null;
		if (!leftIndent.HasValue && listLevel.ParagraphFormat.HasValue(2))
		{
			leftIndent = listLevel.ParagraphFormat.LeftIndent;
		}
		if (!firstLineIndent.HasValue && listLevel.ParagraphFormat.HasValue(5))
		{
			firstLineIndent = listLevel.ParagraphFormat.FirstLineIndent;
		}
		return wListFormat;
	}

	internal WListLevel GetListLevel(WListFormat listFormat, ref int tabLevelIndex)
	{
		ListStyle currentListStyle = listFormat.CurrentListStyle;
		WParagraphStyle wParagraphStyle = ParaStyle as WParagraphStyle;
		int levelNumber = 0;
		if (ListFormat.HasKey(0))
		{
			levelNumber = ListFormat.ListLevelNumber;
		}
		else
		{
			while (wParagraphStyle != null)
			{
				if (wParagraphStyle.ListFormat.HasKey(0))
				{
					levelNumber = wParagraphStyle.ListFormat.ListLevelNumber;
					if (wParagraphStyle.ParagraphFormat.Tabs.Count > 0)
					{
						tabLevelIndex++;
					}
					break;
				}
				wParagraphStyle = wParagraphStyle.BaseStyle;
			}
		}
		WListLevel result = currentListStyle.GetNearLevel(levelNumber);
		ListOverrideStyle listOverrideStyle = null;
		if (listFormat.LFOStyleName != null && listFormat.LFOStyleName.Length > 0)
		{
			listOverrideStyle = base.Document.ListOverrides.FindByName(listFormat.LFOStyleName);
		}
		if (listOverrideStyle != null && listOverrideStyle.OverrideLevels.HasOverrideLevel(levelNumber) && listOverrideStyle.OverrideLevels[levelNumber].OverrideFormatting)
		{
			result = listOverrideStyle.OverrideLevels[levelNumber].OverrideListLevel;
		}
		return result;
	}

	internal string GetListText(bool isFromTextConverter, ref bool isPicBullet)
	{
		string result = string.Empty;
		WListFormat listFormatValue = GetListFormatValue();
		if (listFormatValue != null && listFormatValue.CurrentListStyle != null && listFormatValue.ListType != ListType.NoList)
		{
			WListLevel listLevel = GetListLevel(listFormatValue);
			if (listLevel.PatternType == ListPatternType.Bullet)
			{
				result = "* ";
				if (isFromTextConverter && listLevel.PicBullet != null)
				{
					isPicBullet = true;
				}
			}
			else
			{
				result = base.Document.UpdateListValue(this, listFormatValue, listLevel) + "  ";
			}
		}
		return result;
	}

	internal WListLevel GetListLevel(WListFormat listFormat)
	{
		int tabLevelIndex = 0;
		return GetListLevel(listFormat, ref tabLevelIndex);
	}

	internal float[] GetLeftRightMargindAndFirstLineIndent(WListFormat listformat, WListLevel level, WParagraphStyle paraStyle)
	{
		float[] array = new float[3];
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		if (level != null && level.ParagraphFormat.HasValue(2))
		{
			num = level.ParagraphFormat.LeftIndent;
		}
		if (ListFormat.ListType == ListType.NoList && paraStyle != null && paraStyle.ParagraphFormat.HasValue(2))
		{
			num = paraStyle.ParagraphFormat.LeftIndent;
		}
		if (ParagraphFormat.HasValue(2))
		{
			num = ParagraphFormat.LeftIndent;
		}
		if (level != null && level.ParagraphFormat.HasValue(3))
		{
			num2 = level.ParagraphFormat.RightIndent;
		}
		if (ListFormat.ListType == ListType.NoList && paraStyle != null && paraStyle.ParagraphFormat.HasValue(3))
		{
			num2 = paraStyle.ParagraphFormat.RightIndent;
		}
		if (ParagraphFormat.HasValue(3))
		{
			num2 = ParagraphFormat.RightIndent;
		}
		if (level != null && level.ParagraphFormat.HasValue(5))
		{
			num3 = level.ParagraphFormat.FirstLineIndent;
		}
		if (ListFormat.ListType == ListType.NoList && paraStyle != null && paraStyle.ParagraphFormat.HasValue(5))
		{
			num3 = paraStyle.ParagraphFormat.FirstLineIndent;
		}
		if (ParagraphFormat.HasValue(5))
		{
			num3 = ParagraphFormat.FirstLineIndent;
		}
		array[0] = num;
		array[1] = num2;
		array[2] = num3;
		return array;
	}

	internal bool IsZeroAutoLineSpace()
	{
		if (ParagraphFormat.LineSpacing == 0f && ParagraphFormat.LineSpacingRule == LineSpacingRule.Multiple)
		{
			return true;
		}
		return false;
	}

	internal bool IsLineNumbersEnabled()
	{
		WSection ownerSection = GetOwnerSection();
		if (ownerSection != null && ownerSection.LineNumbersEnabled() && !ParagraphFormat.SuppressLineNumbers && (!SectionEndMark || ChildEntities.Count != 0))
		{
			return true;
		}
		return false;
	}

	internal bool IsContainsInLineImage()
	{
		foreach (ParagraphItem item in Items)
		{
			if (item is WPicture && (item as WPicture).TextWrappingStyle == TextWrappingStyle.Inline)
			{
				return true;
			}
		}
		return false;
	}

	public override TextSelection Find(Regex pattern)
	{
		if (FindUtils.IsPatternEmpty(pattern))
		{
			throw new ArgumentException("Search string cannot be empty");
		}
		List<TextSelection> list = TextFinder.Instance.Find(this, pattern, onlyFirstMatch: true);
		if (list.Count <= 0)
		{
			return null;
		}
		return list[0];
	}

	public TextSelection Find(string given, bool caseSensitive, bool wholeWord)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return Find(pattern);
	}

	public override int Replace(Regex pattern, string replace)
	{
		if (FindUtils.IsPatternEmpty(pattern))
		{
			throw new ArgumentException("Search string cannot be empty");
		}
		return TextReplacer.Instance.Replace(this, pattern, replace);
	}

	public override int Replace(string given, string replace, bool caseSensitive, bool wholeWord)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return Replace(pattern, replace);
	}

	public override int Replace(Regex pattern, TextSelection textSelection)
	{
		return Replace(pattern, textSelection, saveFormatting: false);
	}

	public override int Replace(Regex pattern, TextSelection textSelection, bool saveFormatting)
	{
		if (FindUtils.IsPatternEmpty(pattern))
		{
			throw new ArgumentException("Search string cannot be empty");
		}
		textSelection.CacheRanges();
		TextSelectionList textSelectionList = FindAll(pattern, isDocumentComparison: false);
		if (textSelectionList != null)
		{
			foreach (TextSelection item in textSelectionList)
			{
				WCharacterFormat srcFormat = null;
				if (item.StartTextRange != null && saveFormatting)
				{
					srcFormat = item.StartTextRange.CharacterFormat;
				}
				Entity entity = ((item.OwnerParagraph == null && item.Count > 0) ? item.StartTextRange.Owner : item.OwnerParagraph);
				int startIndex = item.SplitAndErase();
				if (entity is WParagraph)
				{
					WParagraph ownerParagraph = item.OwnerParagraph;
					textSelection.CopyTo(ownerParagraph, startIndex, saveFormatting, srcFormat);
				}
				else if (entity is InlineContentControl)
				{
					textSelection.CopyTo(entity as InlineContentControl, startIndex, saveFormatting, srcFormat);
				}
			}
			return textSelectionList.Count;
		}
		return 0;
	}

	public int Replace(string given, TextSelection textSelection, bool caseSensitive, bool wholeWord)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return Replace(pattern, textSelection, saveFormatting: false);
	}

	public int Replace(string given, TextSelection textSelection, bool caseSensitive, bool wholeWord, bool saveFormatting)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return Replace(pattern, textSelection, saveFormatting);
	}

	internal int ReplaceFirst(string given, string replace, bool caseSensitive, bool wholeWord)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return ReplaceFirst(pattern, replace);
	}

	internal int ReplaceFirst(Regex pattern, string replace)
	{
		if (FindUtils.IsPatternEmpty(pattern))
		{
			throw new ArgumentException("Search string cannot be empty");
		}
		TextReplacer instance = TextReplacer.Instance;
		bool replaceFirst = base.Document.ReplaceFirst;
		base.Document.ReplaceFirst = true;
		int result = instance.Replace(this, pattern, replace);
		base.Document.ReplaceFirst = replaceFirst;
		return result;
	}

	public WSection InsertSectionBreak()
	{
		return InsertSectionBreak(SectionBreakCode.NewPage);
	}

	public WSection InsertSectionBreak(SectionBreakCode breakType)
	{
		WSection ownerSection = GetOwnerSection();
		if (ownerSection == null)
		{
			throw new Exception("Owner section cannot be null.");
		}
		if (m_ownerTextBodyItem.Owner is HeaderFooter)
		{
			throw new NotSupportedException("Cannot insert section break for header footer items.");
		}
		int indexInOwnerCollection = ownerSection.GetIndexInOwnerCollection();
		WSection wSection = ownerSection.CloneWithoutBodyItems();
		base.Document.Sections.Insert(indexInOwnerCollection + 1, wSection);
		wSection.BreakCode = breakType;
		int indexInOwnerCollection2 = m_ownerTextBodyItem.GetIndexInOwnerCollection();
		for (int num = ownerSection.Body.Items.Count - 1; num >= indexInOwnerCollection2 + 1; num--)
		{
			wSection.Body.Items.Insert(0, ownerSection.Body.ChildEntities[num]);
		}
		return wSection;
	}

	internal WSection GetOwnerSection()
	{
		Entity entity = this;
		while (!(entity is WSection))
		{
			if (entity is TextBodyItem)
			{
				m_ownerTextBodyItem = entity as TextBodyItem;
			}
			if (entity.Owner == null)
			{
				break;
			}
			entity = entity.Owner;
		}
		return entity as WSection;
	}

	internal ParagraphItemCollection GetParagraphItems()
	{
		ParagraphItemCollection paragraphItemCollection = new ParagraphItemCollection(this);
		for (int i = 0; i < m_pItemColl.Count; i++)
		{
			if (m_pItemColl[i] is InlineContentControl)
			{
				(m_pItemColl[i] as InlineContentControl).CopyItemsTo(paragraphItemCollection);
			}
			else
			{
				paragraphItemCollection.InnerList.Add(m_pItemColl[i]);
			}
		}
		return paragraphItemCollection;
	}

	internal void ClearItems()
	{
		Items.InnerList.Clear();
		m_strTextBuilder = new StringBuilder(1);
	}

	internal override TextSelectionList FindAll(Regex pattern, bool isDocumentComparison)
	{
		return TextFinder.Instance.FindItem(this, pattern, onlyFirstMatch: false, isDocumentComparison);
	}

	internal TextSelectionList FindFirst(Regex pattern)
	{
		return TextFinder.Instance.Find(this, pattern, onlyFirstMatch: true);
	}

	internal void RemoveItems(int startIndex, bool toEnd)
	{
		if (toEnd)
		{
			while (startIndex < Items.Count)
			{
				Items.RemoveAt(Items.Count - 1);
			}
			return;
		}
		while (startIndex > -1)
		{
			Items.RemoveAt(startIndex);
			startIndex--;
		}
	}

	internal WParagraph CloneWithoutItems()
	{
		return CloneParagraph(cloneItems: false);
	}

	internal IParagraphItem AppendItem(ParagraphItemType itemType)
	{
		IParagraphItem paragraphItem = base.Document.CreateParagraphItem(itemType);
		Items.Add(paragraphItem);
		return paragraphItem;
	}

	internal void UpdateText(WTextRange pItem, string newText, bool isRemove)
	{
		UpdateText(pItem, pItem.TextLength, newText, isRemove);
	}

	internal void UpdateText(ParagraphItem pItem, int removeTextLength, string newText, bool isRemove)
	{
		if (isRemove)
		{
			m_strTextBuilder.Remove(pItem.StartPos, removeTextLength);
		}
		m_strTextBuilder.Insert(pItem.StartPos, newText);
		int offset = (isRemove ? (newText.Length - removeTextLength) : newText.Length);
		if (pItem.Owner is InlineContentControl)
		{
			base.Document.UpdateStartPosOfInlineContentControlItems(pItem.Owner as InlineContentControl, pItem.Index + 1, offset);
			Entity entity = pItem;
			while (!(entity is WParagraph) && entity != null)
			{
				entity = entity.Owner;
				if (entity.Owner is WParagraph)
				{
					UpdateStartPosOfParaItems(entity as ParagraphItem, offset);
					break;
				}
				if (entity.Owner is InlineContentControl)
				{
					base.Document.UpdateStartPosOfInlineContentControlItems(entity.Owner as InlineContentControl, entity.Index + 1, offset);
				}
			}
		}
		else
		{
			UpdateStartPosOfParaItems(pItem, offset);
		}
	}

	private void UpdateStartPosOfParaItems(ParagraphItem pItem, int offset)
	{
		int num = m_pItemColl.IndexOf(pItem);
		if (!(pItem.Owner is InlineContentControl) && num < 0)
		{
			throw new InvalidOperationException("pItem haven't found in paragraph items");
		}
		int i = num + 1;
		for (int count = m_pItemColl.Count; i < count; i++)
		{
			ParagraphItem item = m_pItemColl[i];
			base.Document.UpdateStartPos(item, offset);
		}
	}

	internal void ApplyStyle(IWParagraphStyle style, bool isDomChanges)
	{
		if (style == null)
		{
			throw new ArgumentNullException("Specified Character style not found");
		}
		if (isDomChanges && ParagraphFormat.m_unParsedSprms != null && ParagraphFormat.m_unParsedSprms.Contain(17920))
		{
			ParagraphFormat.m_unParsedSprms.RemoveValue(17920);
		}
		ParaStyle = style;
		ApplyBaseStyleFormats();
	}

	internal bool IsTextContainsNonBreakingSpaceCharacter(string text)
	{
		if (base.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 || base.Document.Settings.CompatibilityMode == CompatibilityMode.Word2010)
		{
			return text.Contains(ControlChar.NonBreakingSpace);
		}
		return false;
	}

	internal bool IsNonBreakingCharacterCombinedWithSpace(string text, int pos)
	{
		for (int i = pos + 1; i < text.Length; i++)
		{
			if (text[i] == ControlChar.NonBreakingSpaceChar)
			{
				return true;
			}
			if (text[i] != ControlChar.SpaceChar)
			{
				break;
			}
		}
		for (int num = pos - 1; num >= 0; num--)
		{
			if (text[num] == ControlChar.NonBreakingSpaceChar)
			{
				return true;
			}
			if (text[num] != ControlChar.SpaceChar)
			{
				return false;
			}
		}
		return false;
	}

	internal int GetsTheIndexOfSpaceToSplit(string text, int index)
	{
		int result = text.Length - 1;
		while (index != -1)
		{
			for (int i = index + 1; i < text.Length; i++)
			{
				if (text[i] == ControlChar.NonBreakingSpaceChar)
				{
					index = text.IndexOf(ControlChar.SpaceChar, index + 1);
					break;
				}
				if (text[i] != ControlChar.SpaceChar)
				{
					break;
				}
			}
			for (int num = index - 1; num >= 0; num--)
			{
				if (text[num] == ControlChar.NonBreakingSpaceChar)
				{
					index = text.IndexOf(ControlChar.SpaceChar, index + 1);
					break;
				}
				if (text[num] != ControlChar.SpaceChar)
				{
					result = index;
					index = -1;
					break;
				}
			}
		}
		return result;
	}

	internal void ApplyCharacterStyle(IWCharacterStyle style)
	{
		if (style == null)
		{
			throw new ArgumentNullException("Specified character style not found");
		}
		BreakCharacterFormat.CharStyleName = style.Name;
		ParagraphItem paragraphItem = null;
		for (int i = 0; i < Items.Count; i++)
		{
			paragraphItem = Items[i];
			paragraphItem.GetCharFormat().CharStyleName = style.Name;
			if (paragraphItem is Break)
			{
				(paragraphItem as Break).CharacterFormat.CharStyleName = style.Name;
			}
			else if (paragraphItem is InlineContentControl)
			{
				(paragraphItem as InlineContentControl).ApplyBaseFormatForCharacterStyle(style);
			}
		}
	}

	internal void ReplaceWithoutCorrection(int start, int length, string replacement)
	{
		m_strTextBuilder.Remove(start, length);
		m_strTextBuilder.Insert(start, replacement);
		IsTextReplaced = true;
	}

	internal override void AddSelf()
	{
		foreach (ParagraphItem item in Items)
		{
			item.AddSelf();
		}
	}

	private void ApplyDuplicateStyleFormatting(WordDocument destDocument)
	{
		if (ParaStyle == null || ListFormat == null || ListFormat.CurrentListStyle == null || ListFormat.CurrentListLevel == null || ListFormat.ListType == ListType.NoList)
		{
			return;
		}
		IWParagraphStyle iWParagraphStyle = destDocument.Styles.FindByName(StyleName) as WParagraphStyle;
		if (iWParagraphStyle == null)
		{
			return;
		}
		List<int> listParaFormatProperties = GetListParaFormatProperties();
		ParagraphFormat.UpdateSourceFormat(iWParagraphStyle.ParagraphFormat);
		foreach (int item in listParaFormatProperties)
		{
			if (ParagraphFormat.PropertiesHash.ContainsKey(item))
			{
				ParagraphFormat.PropertiesHash.Remove(item);
			}
		}
		listParaFormatProperties.Clear();
		listParaFormatProperties = null;
	}

	private List<int> GetListParaFormatProperties()
	{
		List<int> list = new List<int>();
		int[] array = new int[ListFormat.CurrentListLevel.ParagraphFormat.PropertiesHash.Count];
		ListFormat.CurrentListLevel.ParagraphFormat.PropertiesHash.Keys.CopyTo(array, 0);
		foreach (int num in array)
		{
			if (!ParagraphFormat.PropertiesHash.ContainsKey(num))
			{
				list.Add(num);
			}
		}
		return list;
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		base.CloneRelationsTo(doc, nextOwner);
		if ((doc.ImportOptions & ImportOptions.UseDestinationStyles) != 0)
		{
			if (doc != base.Document && doc.Sections.Count == 0)
			{
				if (base.Document != null && base.Document.DefCharFormat != null)
				{
					if (doc.DefCharFormat == null)
					{
						doc.DefCharFormat = new WCharacterFormat(doc);
					}
					doc.DefCharFormat.ImportContainer(base.Document.DefCharFormat);
				}
				if (base.Document != null && base.Document.m_defParaFormat != null)
				{
					if (doc.DefParaFormat == null)
					{
						doc.DefParaFormat = new WParagraphFormat(doc);
					}
					doc.DefParaFormat.ImportContainer(base.Document.DefParaFormat);
				}
			}
			if (doc.UpdateAlternateChunk)
			{
				ApplyDuplicateStyleFormatting(doc);
			}
			CloneStyleRelations(doc);
		}
		else if ((doc.ImportOptions & ImportOptions.MergeFormatting) != 0)
		{
			UpdateMergeFormatting(doc);
		}
		else
		{
			UpdateSourceFormatting(doc);
		}
		ListFormat.CloneListRelationsTo(doc, null);
		Entity ownerTextBody = GetOwnerTextBody(nextOwner as Entity);
		int i = 0;
		for (int count = Items.Count; i < count; i++)
		{
			ParagraphItem paragraphItem = Items[i];
			if (doc.UpdateAlternateChunk && IsNeedToRemoveItems(ownerTextBody, paragraphItem))
			{
				paragraphItem.RemoveSelf();
			}
			else
			{
				paragraphItem.CloneRelationsTo(doc, nextOwner);
				if (IsNeedToAddFloatingItemsCollection(paragraphItem))
				{
					doc.FloatingItems.Add(paragraphItem);
				}
			}
			if (paragraphItem is TableOfContent tableOfContent && !doc.TOC.ContainsKey(tableOfContent.TOCField))
			{
				doc.TOC.Add(tableOfContent.TOCField, paragraphItem as TableOfContent);
			}
		}
		if ((doc.ImportOptions & ImportOptions.UseDestinationStyles) == 0)
		{
			ApplyStyle(m_style, isDomChanges: false);
		}
	}

	private bool IsNeedToRemoveItems(Entity baseEntity, ParagraphItem item)
	{
		if ((baseEntity is WComment || baseEntity is HeaderFooter || baseEntity is WFootnote) && (item is WFootnote || item is WComment))
		{
			return true;
		}
		return false;
	}

	private bool IsNeedToAddFloatingItemsCollection(Entity entity)
	{
		switch (entity.EntityType)
		{
		case EntityType.Picture:
		case EntityType.TextBox:
		case EntityType.XmlParaItem:
		case EntityType.Chart:
		case EntityType.OleObject:
			return true;
		case EntityType.Shape:
		case EntityType.AutoShape:
			if (entity is Shape)
			{
				return true;
			}
			return false;
		default:
			return false;
		}
	}

	private void UpdateMergeFormatting(WordDocument doc)
	{
		WParagraph wParagraph = doc.LastParagraph;
		if (wParagraph == null)
		{
			wParagraph = new WParagraph(doc);
		}
		ParagraphFormat.ImportContainer(wParagraph.ParagraphFormat);
		ParagraphFormat.CopyProperties(wParagraph.ParagraphFormat);
		BreakCharacterFormat.MergeFormat(wParagraph.BreakCharacterFormat);
		ParaStyle = wParagraph.m_style;
	}

	private void UpdateSourceFormatting(WordDocument doc)
	{
		WParagraphStyle wParagraphStyle = doc.Styles.FindByName("Normal", StyleType.ParagraphStyle) as WParagraphStyle;
		if (wParagraphStyle == null)
		{
			wParagraphStyle = (WParagraphStyle)Style.CreateBuiltinStyle(BuiltinStyle.Normal, doc);
			if (doc.Styles.FindByName("Normal", StyleType.ParagraphStyle) == null)
			{
				doc.Styles.Add(wParagraphStyle);
			}
		}
		if ((doc.ImportOptions & ImportOptions.KeepSourceFormatting) != 0)
		{
			if (!doc.UpdateAlternateChunk || !IsLastItem || m_style == null || !(m_style.Name == "Normal"))
			{
				ParagraphFormat.UpdateSourceFormat(wParagraphStyle.ParagraphFormat);
				if (ParaStyle != null)
				{
					if (ParaStyle.ParagraphFormat.Tabs != null && ParaStyle.ParagraphFormat.Tabs.Count > 0)
					{
						ParaStyle.ParagraphFormat.Tabs.UpdateTabs(ParagraphFormat.Tabs);
					}
					if (ListFormat != null && !ListFormat.IsEmptyList && !ListFormat.HasKey(2) && ParaStyle is WParagraphStyle && (ParaStyle as WParagraphStyle).ListFormat != null && (ParaStyle as WParagraphStyle).ListFormat.CurrentListStyle != null)
					{
						ListFormat.ApplyStyle((ParaStyle as WParagraphStyle).ListFormat.CurrentListStyle.Name);
						ListFormat.PropertiesHash[0] = (ParaStyle as WParagraphStyle).ListFormat.ListLevelNumber;
					}
				}
				if (ParagraphFormat.Tabs != null && ParagraphFormat.Tabs.Count > 0 && wParagraphStyle.ParagraphFormat.Tabs != null && wParagraphStyle.ParagraphFormat.Tabs.Count > 0)
				{
					wParagraphStyle.ParagraphFormat.Tabs.UpdateSourceFormatting(ParagraphFormat.Tabs);
				}
			}
			BreakCharacterFormat.UpdateSourceFormat(wParagraphStyle.CharacterFormat);
		}
		ParaStyle = wParagraphStyle;
		IsLastItem = false;
	}

	private void CloneStyleRelations(WordDocument doc)
	{
		IStyle style = (m_style as Style).ImportStyleTo(doc, isParagraphStyle: true);
		if (style is WParagraphStyle)
		{
			if (doc == base.Document)
			{
				ApplyStyle(style as WParagraphStyle, isDomChanges: false);
				return;
			}
			ParaStyle = style as WParagraphStyle;
			m_charFormat.ApplyBase(ParaStyle.CharacterFormat);
			m_prFormat.ApplyBase(ParaStyle.ParagraphFormat);
		}
	}

	internal void ImportStyle(IWParagraphStyle style)
	{
		IStyle style2 = (style as Style).ImportStyleTo(base.Document, isParagraphStyle: false);
		if (style2 is WParagraphStyle)
		{
			ApplyStyle(style2 as WParagraphStyle, isDomChanges: false);
		}
	}

	protected override object CloneImpl()
	{
		return CloneParagraph(cloneItems: true);
	}

	private WParagraph CloneParagraph(bool cloneItems)
	{
		WParagraph wParagraph = (WParagraph)base.CloneImpl();
		wParagraph.m_strTextBuilder = new StringBuilder(Text);
		wParagraph.m_strTextBuilder2 = null;
		wParagraph.m_pItemColl = new ParagraphItemCollection(wParagraph);
		if (cloneItems)
		{
			m_pItemColl.CloneItemsTo(wParagraph.m_pItemColl);
		}
		if (m_charFormat != null)
		{
			wParagraph.m_charFormat = new WCharacterFormat(base.Document);
			wParagraph.m_charFormat.ImportContainer(m_charFormat);
			wParagraph.m_charFormat.CopyProperties(m_charFormat);
			wParagraph.m_charFormat.SetOwner(wParagraph);
			if (m_charFormat.m_revisions != null && m_charFormat.m_revisions.Count > 0)
			{
				if (base.Document.IsCloning || base.Document.IsComparing)
				{
					wParagraph.m_charFormat.m_clonedRevisions = new List<Revision>();
					foreach (Revision revision in m_charFormat.m_revisions)
					{
						wParagraph.m_charFormat.m_clonedRevisions.Add(revision.Clone());
					}
				}
				else if (wParagraph.m_charFormat.PropertiesHash.Count > 0)
				{
					foreach (int revisionKey in m_charFormat.RevisionKeys)
					{
						wParagraph.m_charFormat.PropertiesHash.Remove(revisionKey);
					}
				}
			}
		}
		if (m_prFormat != null)
		{
			wParagraph.m_prFormat = new WParagraphFormat(base.Document);
			wParagraph.m_prFormat.ImportContainer(m_prFormat);
			wParagraph.m_prFormat.CopyProperties(m_prFormat);
			wParagraph.m_prFormat.SetOwner(wParagraph);
		}
		if (m_listFormat != null)
		{
			wParagraph.m_listFormat = new WListFormat(this);
			wParagraph.m_listFormat.ImportContainer(m_listFormat);
			wParagraph.m_listFormat.SetOwner(wParagraph);
		}
		IWParagraphStyle style = GetStyle();
		if (style != null)
		{
			wParagraph.ApplyStyle(style, isDomChanges: false);
		}
		wParagraph.CreateEmptyParagraph();
		return wParagraph;
	}

	internal string GetParagraphText(bool isLastPargraph)
	{
		string text = GetText(0, m_pItemColl.Count - 1);
		if (!isLastPargraph)
		{
			return text + ControlChar.ParagraphBreak;
		}
		return text;
	}

	internal bool HasNonHiddenPara()
	{
		IEntity nextSibling = base.NextSibling;
		while (nextSibling != null)
		{
			if (IsPerviousHiddenPara(nextSibling) && !IsPreviousParagraphHasContent(nextSibling))
			{
				nextSibling = nextSibling.NextSibling;
				continue;
			}
			return nextSibling is WParagraph;
		}
		return true;
	}

	internal bool IsPreviousParagraphMarkIsHidden()
	{
		bool flag = false;
		IEntity previousSibling = base.PreviousSibling;
		while (previousSibling != null && IsPerviousHiddenPara(previousSibling))
		{
			flag = IsPreviousParagraphHasContent(previousSibling);
			if (flag)
			{
				if (previousSibling is WParagraph)
				{
					flag = !((previousSibling as WParagraph).LastItem is Break) || ((previousSibling as WParagraph).LastItem as Break).BreakType != BreakType.PageBreak;
				}
				else if (previousSibling is BlockContentControl && (previousSibling as BlockContentControl).LastChildEntity is WParagraph)
				{
					flag = !(((previousSibling as BlockContentControl).LastChildEntity as WParagraph).LastItem is Break) || (((previousSibling as BlockContentControl).LastChildEntity as WParagraph).LastItem as Break).BreakType != BreakType.PageBreak;
				}
				break;
			}
			previousSibling = previousSibling.PreviousSibling;
		}
		return flag;
	}

	internal bool IsPerviousHiddenPara(IEntity prevsibling)
	{
		if ((prevsibling is WParagraph && (prevsibling as WParagraph).BreakCharacterFormat.Hidden) || (prevsibling is BlockContentControl && (prevsibling as BlockContentControl).IsHiddenParagraphMarkIsInLastItemOfSDTContent()))
		{
			return true;
		}
		return false;
	}

	internal bool IsPreviousParagraphMarkIsInDeletion(ref IEntity prevEntity)
	{
		bool flag = false;
		IEntity previousSibling = base.PreviousSibling;
		while (previousSibling != null && IsPreviousParaInDeletion(previousSibling))
		{
			prevEntity = previousSibling;
			if (!flag)
			{
				flag = IsPreviousParagraphHasContent(previousSibling);
			}
			previousSibling = previousSibling.PreviousSibling;
		}
		return flag;
	}

	internal bool IsPreviousParaInDeletion(IEntity prevsibling)
	{
		if ((prevsibling is WParagraph && (prevsibling as WParagraph).BreakCharacterFormat.IsDeleteRevision) || (prevsibling is BlockContentControl && (prevsibling as BlockContentControl).IsDeletionParagraphMarkIsInLastItemOfSDTContent()))
		{
			return true;
		}
		return false;
	}

	internal bool GetNextNonDeleteRevisionListParagraph(ref IEntity nextListPara)
	{
		IEntity nextSibling = base.NextSibling;
		while (nextSibling is WParagraph)
		{
			if (!(nextSibling as WParagraph).BreakCharacterFormat.IsDeleteRevision && (nextSibling as WParagraph).ListFormat.ListType != ListType.NoList)
			{
				nextListPara = nextSibling;
				return true;
			}
			if (!(nextSibling as WParagraph).BreakCharacterFormat.IsDeleteRevision && (nextSibling as WParagraph).ListFormat.ListType == ListType.NoList)
			{
				return false;
			}
			nextSibling = nextSibling.NextSibling;
		}
		return false;
	}

	private bool IsParagraphMarkDeleted()
	{
		if (BreakCharacterFormat != null)
		{
			for (int i = 0; i < BreakCharacterFormat.Revisions.Count; i++)
			{
				if (BreakCharacterFormat.Revisions[i].RevisionType == RevisionType.Deletions)
				{
					return true;
				}
			}
		}
		return false;
	}

	internal IEntity GetPreviousOrNextNonDeleteRevisionEntity(bool isPrevious)
	{
		IEntity entity = (isPrevious ? base.PreviousSibling : base.NextSibling);
		while (entity is WParagraph)
		{
			if (!(entity as WParagraph).BreakCharacterFormat.IsDeleteRevision)
			{
				return entity;
			}
			entity = (isPrevious ? entity.PreviousSibling : entity.NextSibling);
		}
		return entity;
	}

	private bool IsPreviousParagraphHasContent(IEntity prevsibling)
	{
		if (prevsibling is WParagraph)
		{
			return !(prevsibling as WParagraph).IsEmptyParagraph();
		}
		if (prevsibling is BlockContentControl && (prevsibling as BlockContentControl).LastChildEntity is WParagraph)
		{
			return !((prevsibling as BlockContentControl).LastChildEntity as WParagraph).IsEmptyParagraph();
		}
		return true;
	}

	internal string GetText(int startIndex, int endIndex)
	{
		string text = string.Empty;
		bool isPicBullet = false;
		if (!ListFormat.IsEmptyList && ChildEntities.Count != 0 && !SectionEndMark)
		{
			text = GetListText(isFromTextConverter: false, ref isPicBullet);
		}
		for (int i = startIndex; i <= endIndex; i++)
		{
			ParagraphItem paragraphItem = m_pItemColl[i];
			if (paragraphItem is WField)
			{
				text += (paragraphItem as WField).Text;
				if (base.Document.m_prevClonedEntity != null)
				{
					break;
				}
				if ((paragraphItem as WField).FieldEnd != null && (paragraphItem as WField).FieldEnd.OwnerParagraph == this)
				{
					i = (paragraphItem as WField).FieldEnd.GetIndexInOwnerCollection();
				}
			}
			else if (paragraphItem is WTextRange)
			{
				WTextRange wTextRange = paragraphItem as WTextRange;
				if (!wTextRange.CharacterFormat.Hidden)
				{
					text = ((!wTextRange.CharacterFormat.AllCaps && !wTextRange.CharacterFormat.SmallCaps) ? (text + wTextRange.Text) : (text + wTextRange.Text.ToUpper()));
				}
			}
			else if (paragraphItem is Break)
			{
				text += ControlChar.ParagraphBreak;
			}
		}
		return text;
	}

	private void ApplyBaseStyleFormats()
	{
		if (m_style == null)
		{
			return;
		}
		m_charFormat.ApplyBase(m_style.CharacterFormat);
		m_prFormat.ApplyBase(m_style.ParagraphFormat);
		ParagraphItem paragraphItem = null;
		int i = 0;
		for (int count = m_pItemColl.Count; i < count; i++)
		{
			paragraphItem = m_pItemColl[i];
			paragraphItem.ParaItemCharFormat.ApplyBase(m_style.CharacterFormat);
			if (paragraphItem is WMergeField)
			{
				(paragraphItem as WMergeField).ApplyBaseFormat();
			}
			else if (paragraphItem is Break)
			{
				(paragraphItem as Break).CharacterFormat.ApplyBase(m_style.CharacterFormat);
			}
			else if (paragraphItem is InlineContentControl)
			{
				(paragraphItem as InlineContentControl).ApplyBaseFormat();
			}
			else if (paragraphItem is WMath)
			{
				(paragraphItem as WMath).ApplyBaseFormat();
			}
		}
	}

	internal bool IsEmptyParagraph()
	{
		for (int i = 0; i < ChildEntities.Count; i++)
		{
			if ((!(ChildEntities[i] is WTextRange) || (!(ChildEntities[i] is WField) && !((ChildEntities[i] as WTextRange).Text.Trim(ControlChar.SpaceChar) == string.Empty)) || (ChildEntities[i] as WTextRange).m_layoutInfo is TabsLayoutInfo) && !(ChildEntities[i] is BookmarkStart) && !(ChildEntities[i] is BookmarkEnd) && !(ChildEntities[i] is WFieldMark))
			{
				return false;
			}
		}
		return true;
	}

	private void ApplyListStyle(BuiltinStyle builtinStyle)
	{
		string name = Style.BuiltInToName(builtinStyle);
		ListStyle listStyle = base.Document.ListStyles.FindByName(name);
		if (listStyle == null)
		{
			listStyle = (ListStyle)Style.CreateBuiltinStyle(builtinStyle, StyleType.OtherStyle, base.Document);
			base.Document.ListStyles.Add(listStyle);
		}
		IWParagraphStyle iWParagraphStyle = base.Document.Styles.FindByName(listStyle.Name) as IWParagraphStyle;
		if (iWParagraphStyle == null)
		{
			iWParagraphStyle = new WParagraphStyle(base.Document);
			iWParagraphStyle.Name = name;
			(iWParagraphStyle as WParagraphStyle).ApplyBaseStyle("Normal");
			base.Document.Styles.Add(iWParagraphStyle);
			base.Document.UpdateNextStyle(iWParagraphStyle as Style);
		}
		ApplyStyle(iWParagraphStyle, isDomChanges: false);
		ListFormat.ApplyStyle(listStyle.Name);
	}

	private void CheckNormalStyle()
	{
		IStyle style = base.Document.Styles.FindByName("Normal", StyleType.ParagraphStyle) as WParagraphStyle;
		if (style == null)
		{
			style = (WParagraphStyle)Style.CreateBuiltinStyle(BuiltinStyle.Normal, base.Document);
			base.Document.Styles.Add(style);
		}
		style = base.Document.Styles.FindByName("Default Paragraph Font", StyleType.CharacterStyle) as WCharacterStyle;
		if (style == null)
		{
			style = (WCharacterStyle)Style.CreateBuiltinCharacterStyle(BuiltinStyle.DefaultParagraphFont, base.Document);
			(style as Style).IsSemiHidden = true;
			(style as Style).UnhideWhenUsed = true;
			base.Document.Styles.Add(style);
		}
	}

	internal override void Close()
	{
		if (m_pItemColl != null)
		{
			m_pItemColl.Close();
			m_pItemColl = null;
		}
		if (m_paragraphItems != null)
		{
			m_paragraphItems.Close();
			m_paragraphItems = null;
		}
		if (m_prFormat != null)
		{
			m_prFormat.Close();
			m_prFormat = null;
		}
		if (m_charFormat != null)
		{
			m_charFormat.Close();
			m_charFormat = null;
		}
		if (m_style != null)
		{
			m_style = null;
		}
		if (m_listFormat != null)
		{
			m_listFormat.Close();
			m_listFormat = null;
		}
		if (m_pEmptyItemColl != null)
		{
			m_pEmptyItemColl.Close();
			m_pEmptyItemColl = null;
		}
		if (m_strTextBuilder != null)
		{
			m_strTextBuilder = null;
		}
		if (TextFinder.Instance.SingleLinePCol.Contains(this))
		{
			TextFinder.Instance.SingleLinePCol.Remove(this);
		}
		base.Close();
	}

	internal void ApplyListParaStyle()
	{
		if (ParaStyle == null || (ParaStyle as WParagraphStyle).StyleId != 179)
		{
			WParagraphStyle wParagraphStyle = (base.Document.Styles as StyleCollection).FindById(179) as WParagraphStyle;
			if (wParagraphStyle == null)
			{
				wParagraphStyle = new WParagraphStyle(base.Document);
				wParagraphStyle.StyleId = 179;
				wParagraphStyle.Name = "List Paragraph";
				wParagraphStyle.NextStyle = "List Paragraph";
				base.Document.Styles.Add(wParagraphStyle);
			}
			ParaStyle = wParagraphStyle;
		}
	}

	internal FieldType GetLastFieldType()
	{
		IEntity entity = Items.LastItem;
		while (entity != null && !(entity is WField))
		{
			entity = entity.PreviousSibling;
		}
		if (entity is WField)
		{
			return (entity as WField).FieldType;
		}
		return FieldType.FieldUnknown;
	}

	internal WField GetLastField()
	{
		IEntity entity = Items.LastItem;
		while (entity != null && !(entity is WField))
		{
			entity = entity.PreviousSibling;
		}
		if (entity is WField)
		{
			return entity as WField;
		}
		return null;
	}

	internal WTableCell GetOwnerTableCell(WTextBody Owner)
	{
		if (Owner is WTableCell)
		{
			return Owner as WTableCell;
		}
		BlockContentControl blockContentControl = ((Owner != null && Owner.Owner is BlockContentControl) ? (Owner.Owner as BlockContentControl) : null);
		while (blockContentControl != null)
		{
			if (blockContentControl.Owner is WTableCell)
			{
				return blockContentControl.Owner as WTableCell;
			}
			if (blockContentControl.Owner is WTextBody && (blockContentControl.Owner as WTextBody).Owner is BlockContentControl)
			{
				blockContentControl = (blockContentControl.Owner as WTextBody).Owner as BlockContentControl;
				continue;
			}
			return null;
		}
		return null;
	}

	private void CreateEmptyParagraph()
	{
		m_pEmptyItemColl = new ParagraphItemCollection(this);
		WTextRange wTextRange = (WTextRange)base.Document.CreateParagraphItem(ParagraphItemType.TextRange);
		wTextRange.Text = " ";
		wTextRange.CharacterFormat.ApplyBase(m_charFormat);
		m_pEmptyItemColl.UnsafeAdd(wTextRange);
	}

	internal bool IsParagraphHasSectionBreak()
	{
		if (this != null && base.NextSibling == null && base.OwnerTextBody != null && !(base.OwnerTextBody is HeaderFooter))
		{
			if (base.OwnerTextBody.Owner != null && base.OwnerTextBody.Owner is WSection)
			{
				return true;
			}
			if (base.OwnerTextBody.Owner != null && base.OwnerTextBody.Owner is BlockContentControl)
			{
				BlockContentControl blockContentControl = base.OwnerTextBody.Owner as BlockContentControl;
				if (blockContentControl.Owner != null && !(blockContentControl.Owner is HeaderFooter))
				{
					WTextBody wTextBody = blockContentControl.Owner as WTextBody;
					if (wTextBody.Owner is WSection)
					{
						if (blockContentControl == wTextBody.Items.LastItem)
						{
							return true;
						}
					}
					else if (wTextBody.Owner != null && wTextBody.Owner is BlockContentControl)
					{
						BlockContentControl blockContentControl2 = wTextBody.Owner as BlockContentControl;
						if (blockContentControl2.Owner != null && !(blockContentControl2.Owner is HeaderFooter))
						{
							WTextBody wTextBody2 = blockContentControl2.Owner as WTextBody;
							if (wTextBody2.Owner is WSection && blockContentControl2 == wTextBody2.Items.LastItem && blockContentControl == wTextBody.Items.LastItem)
							{
								return true;
							}
						}
					}
				}
			}
		}
		return false;
	}

	private bool IsSectionEndMark()
	{
		if (this != null && base.NextSibling == null && base.OwnerTextBody != null && !(base.OwnerTextBody is HeaderFooter))
		{
			if (base.OwnerTextBody.Owner != null && base.OwnerTextBody.Owner is WSection)
			{
				if (base.OwnerTextBody.Owner is WSection { NextSibling: not null } && !ModifyText(Text).Contains('\r'.ToString()) && (ChildEntities.Count == 0 || IsEmptyParagraph()))
				{
					return true;
				}
			}
			else if (base.OwnerTextBody.Owner != null && base.OwnerTextBody.Owner is BlockContentControl)
			{
				BlockContentControl blockContentControl = base.OwnerTextBody.Owner as BlockContentControl;
				if (blockContentControl.Owner != null && !(blockContentControl.Owner is HeaderFooter))
				{
					WTextBody wTextBody = blockContentControl.Owner as WTextBody;
					if (wTextBody.Owner is WSection && blockContentControl == wTextBody.Items.LastItem)
					{
						WSection obj = wTextBody.Owner as WSection;
						string text = ModifyText(Text);
						if (obj != null && !text.Contains('\r'.ToString()) && (ChildEntities.Count == 0 || IsEmptyParagraph()))
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	private string ModifyText(string text)
	{
		char newChar = '\r';
		char oldChar = '\n';
		text = text.Replace(ControlChar.CrLf, newChar.ToString());
		text = text.Replace(oldChar, newChar);
		text = text.Replace('\a'.ToString(), string.Empty);
		text = text.Replace('\b'.ToString(), string.Empty);
		return text;
	}

	internal void UpdateBookmarkEnd(ParagraphItem item, WParagraph paragraphStart, bool isAddItem)
	{
		int num = -1;
		WParagraph wParagraph = null;
		if (item is BookmarkStart && !StartsWithExt((item as BookmarkStart).Name, "_"))
		{
			Bookmark bookmark = paragraphStart.Document.Bookmarks.FindByName((item as BookmarkStart).Name);
			if (bookmark != null && bookmark.BookmarkEnd != null)
			{
				num = bookmark.BookmarkEnd.Index;
				wParagraph = bookmark.BookmarkEnd.OwnerParagraph;
			}
		}
		if (isAddItem)
		{
			paragraphStart.Items.Add(item);
		}
		else
		{
			paragraphStart.Items.Insert(0, item);
			if (wParagraph != null && paragraphStart == wParagraph)
			{
				num++;
			}
		}
		if (wParagraph != null)
		{
			Bookmark bookmark2 = null;
			if (isAddItem && paragraphStart.LastItem is BookmarkStart)
			{
				bookmark2 = paragraphStart.Document.Bookmarks.FindByName((paragraphStart.LastItem as BookmarkStart).Name);
			}
			else if (!isAddItem && paragraphStart.Items[0] is BookmarkStart)
			{
				bookmark2 = paragraphStart.Document.Bookmarks.FindByName((paragraphStart.Items[0] as BookmarkStart).Name);
			}
			if (bookmark2 != null && bookmark2.BookmarkEnd == null && wParagraph.Items.Count > num && wParagraph.Items[num] is BookmarkEnd)
			{
				bookmark2.SetEnd(wParagraph.Items[num] as BookmarkEnd);
			}
		}
	}

	internal bool IsAtleastFrameHeight()
	{
		return ((ushort)(ParagraphFormat.FrameHeight * 20f) & 0x8000) != 0;
	}

	internal override void MakeChanges(bool acceptChanges)
	{
		if (acceptChanges && m_listFormat != null)
		{
			if (m_listFormat.OldPropertiesHash != null && m_listFormat.OldPropertiesHash.Count > 0)
			{
				m_listFormat.OldPropertiesHash.Clear();
			}
			if (ParagraphFormat.m_unParsedSprms != null && ParagraphFormat.m_unParsedSprms.Count > 0)
			{
				if (ParagraphFormat.m_unParsedSprms[50757] != null)
				{
					ParagraphFormat.m_unParsedSprms.RemoveValue(50757);
				}
				if (ParagraphFormat.m_unParsedSprms[9283] != null)
				{
					ParagraphFormat.m_unParsedSprms.RemoveValue(9283);
				}
			}
		}
		ParagraphItem paragraphItem = null;
		BreakCharacterFormat.AcceptChanges();
		for (int i = 0; i < m_pItemColl.Count; i++)
		{
			paragraphItem = m_pItemColl[i];
			if ((paragraphItem.IsDeleteRevision && acceptChanges) || (paragraphItem.IsInsertRevision && !acceptChanges))
			{
				m_pItemColl.RemoveAt(i);
				i--;
				continue;
			}
			if (paragraphItem.IsChangedCFormat && !acceptChanges)
			{
				paragraphItem.RemoveChanges();
			}
			if (paragraphItem is Break)
			{
				if ((acceptChanges && (paragraphItem as Break).CharacterFormat.IsDeleteRevision) || (!acceptChanges && (paragraphItem as Break).CharacterFormat.IsInsertRevision))
				{
					m_pItemColl.RemoveAt(i);
					i--;
					continue;
				}
				(paragraphItem as Break).TextRange.AcceptChanges();
			}
			paragraphItem.AcceptChanges();
			if (paragraphItem is WTextBox)
			{
				(paragraphItem as WTextBox).TextBoxBody.MakeChanges(acceptChanges);
			}
			else if (paragraphItem is WFootnote)
			{
				(paragraphItem as WFootnote).TextBody.MakeChanges(acceptChanges);
			}
		}
	}

	internal override void RemoveCFormatChanges()
	{
		if (m_charFormat != null)
		{
			m_charFormat.RemoveChanges();
		}
	}

	internal override void RemovePFormatChanges()
	{
		if (m_prFormat != null)
		{
			m_prFormat.RemoveChanges();
		}
	}

	internal override void AcceptCChanges()
	{
		if (m_charFormat != null)
		{
			m_charFormat.AcceptChanges();
		}
	}

	internal override void AcceptPChanges()
	{
		if (m_prFormat != null)
		{
			m_prFormat.AcceptChanges();
		}
	}

	internal override bool CheckChangedPFormat()
	{
		if (m_prFormat != null)
		{
			return m_prFormat.IsChangedFormat;
		}
		return false;
	}

	internal override bool CheckInsertRev()
	{
		if (m_charFormat != null)
		{
			return m_charFormat.IsInsertRevision;
		}
		return false;
	}

	internal override bool CheckDeleteRev()
	{
		if (m_charFormat != null)
		{
			return m_charFormat.IsDeleteRevision;
		}
		return false;
	}

	internal override bool CheckChangedCFormat()
	{
		if (m_charFormat != null)
		{
			return m_charFormat.IsChangedFormat;
		}
		return false;
	}

	internal bool CheckOnRemove()
	{
		foreach (ParagraphItem item in m_pItemColl)
		{
			if (!item.IsDeleteRevision)
			{
				return false;
			}
		}
		return true;
	}

	internal override bool HasTrackedChanges()
	{
		if (base.IsInsertRevision || base.IsDeleteRevision || base.IsChangedCFormat || base.IsChangedPFormat)
		{
			return true;
		}
		foreach (ParagraphItem item in m_pItemColl)
		{
			if (item.HasTrackedChanges())
			{
				return true;
			}
		}
		return false;
	}

	internal override void SetDeleteRev(bool check)
	{
		if (m_charFormat != null)
		{
			m_charFormat.IsDeleteRevision = check;
		}
	}

	internal override void SetInsertRev(bool check)
	{
		if (m_charFormat != null)
		{
			m_charFormat.IsInsertRevision = check;
		}
	}

	internal override void SetChangedCFormat(bool check)
	{
		if (m_charFormat != null)
		{
			m_charFormat.IsChangedFormat = check;
		}
	}

	internal override void SetChangedPFormat(bool check)
	{
		if (m_prFormat != null)
		{
			m_prFormat.IsChangedFormat = check;
		}
	}

	internal bool IsOnlyHasSpaces()
	{
		if (!IsParagraphContainsOnlyTextRange())
		{
			return false;
		}
		char[] trimChars = new char[4] { ' ', '\u00a0', '\u2005', '\u3000' };
		return string.IsNullOrEmpty(Text.Trim(trimChars));
	}

	private bool IsParagraphContainsOnlyTextRange()
	{
		foreach (Entity childEntity in ChildEntities)
		{
			if (!(childEntity is WTextRange))
			{
				return false;
			}
		}
		return true;
	}

	internal override TextBodyItem GetNextTextBodyItemValue()
	{
		if (base.NextSibling != null)
		{
			return base.NextSibling as TextBodyItem;
		}
		Entity ownerEntity = GetOwnerEntity();
		if (IsInCell)
		{
			return (ownerEntity as WTableCell).GetNextTextBodyItem();
		}
		if (base.Owner is WTextBody)
		{
			if (ownerEntity is WTextBox)
			{
				return (ownerEntity as WTextBox).GetNextTextBodyItem();
			}
			if (base.OwnerTextBody.Owner is WSection)
			{
				return GetNextInSection(base.OwnerTextBody.Owner as WSection);
			}
		}
		return null;
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("type", "Paragraph");
	}

	protected override void InitXDLSHolder()
	{
		base.XDLSHolder.AddRefElement("style", GetStyle());
		base.XDLSHolder.AddElement("paragraph-format", m_prFormat);
		base.XDLSHolder.AddElement("character-format", m_charFormat);
		base.XDLSHolder.AddElement("list-format", ListFormat);
		base.XDLSHolder.AddElement("items", m_pItemColl);
	}

	protected override void RestoreReference(string name, int index)
	{
		if (name == "style" && index > -1)
		{
			ParaStyle = base.Document.Styles[index] as IWParagraphStyle;
			ApplyBaseStyleFormats();
		}
	}

	internal bool HasNoRenderableItem()
	{
		if (ChildEntities.Count == 0)
		{
			return true;
		}
		Entity entity = LastItem;
		do
		{
			entity = ((entity is InlineContentControl) ? (entity as InlineContentControl).ParagraphItems.LastItem : entity);
			while (entity is BookmarkStart || entity is BookmarkEnd || entity is EditableRangeStart || entity is EditableRangeEnd)
			{
				entity = entity.PreviousSibling as Entity;
			}
		}
		while (entity is InlineContentControl);
		if (entity != null)
		{
			return false;
		}
		return true;
	}

	internal bool HasRenderableItemFromIndex(int pItemIndex)
	{
		int num = pItemIndex;
		while (pItemIndex >= 0 && num < ChildEntities.Count)
		{
			Entity entity = ChildEntities[num];
			if (!(entity is BookmarkStart) && !(entity is BookmarkEnd) && !(entity is EditableRangeStart) && !(entity is EditableRangeEnd))
			{
				return true;
			}
			num++;
		}
		return false;
	}

	internal Entity GetOwnerBaseEntity(Entity entity)
	{
		Entity entity2 = entity;
		do
		{
			if (entity2.Owner == null)
			{
				return entity2;
			}
			entity2 = entity2.Owner;
		}
		while (!(entity2 is WSection) && !(entity2 is BlockContentControl) && !(entity2 is WTextBox) && !(entity2 is Shape) && !(entity2 is GroupShape));
		return entity2;
	}

	internal bool IsFirstLine(LayoutedWidget ltWidget)
	{
		IWidget widget = ltWidget.Widget;
		if (widget is SplitWidgetContainer)
		{
			widget = (widget as SplitWidgetContainer).m_currentChild;
		}
		if (widget is SplitStringWidget)
		{
			if ((widget as SplitStringWidget).StartIndex > 0)
			{
				return false;
			}
			widget = (widget as SplitStringWidget).RealStringWidget;
		}
		IWidget previousSibling = GetPreviousSibling(widget);
		while (previousSibling != null && (previousSibling.LayoutInfo.IsSkip || previousSibling.LayoutInfo.IsSkipBottomAlign || previousSibling is InlineShapeObject || previousSibling is WFieldMark || previousSibling is BookmarkStart || previousSibling is BookmarkEnd || (previousSibling is Break && (previousSibling as Break).BreakType != BreakType.LineBreak && (previousSibling as Break).BreakType != BreakType.TextWrappingBreak)))
		{
			previousSibling = GetPreviousSibling(previousSibling);
		}
		return previousSibling == null;
	}

	internal void RemoveSplitStringWidget()
	{
		IWidget widget = null;
		for (int i = 0; i < ChildEntities.Count; i++)
		{
			IWidget currentWidget;
			if ((currentWidget = (ChildEntities as ParagraphItemCollection).GetCurrentWidget(i)) is SplitStringWidget || (i > 0 && widget == currentWidget))
			{
				ChildEntities.InnerList.RemoveAt(i);
				ChildEntities.UpdateIndexForDuplicateEntity(i, isAdd: false);
				i--;
			}
			widget = currentWidget;
		}
	}

	internal IWidget GetPreviousSibling(IWidget widget)
	{
		IList innerList = (WidgetCollection as ParagraphItemCollection).InnerList;
		int num = innerList.IndexOf(widget);
		if (num == 0 || num == -1)
		{
			return null;
		}
		return innerList[num - 1] as IWidget;
	}

	internal IWidget GetPreviousInlineItems(IWidget widget)
	{
		IWidget widget2 = widget;
		while ((widget2 = GetPreviousSibling(widget2)) != null)
		{
			if (!widget2.LayoutInfo.IsSkip && (widget2 is WTextRange || ((widget2 is WPicture || widget2 is Shape || widget2 is WTextBox || widget2 is GroupShape || widget2 is WChart) && !(widget2 as Entity).IsFloatingItem(isTextWrapAround: false))))
			{
				return widget2;
			}
		}
		return widget2;
	}

	internal bool IsLastLine(LayoutedWidget ltWidget)
	{
		IWidget widget = ltWidget.Widget;
		if (widget is SplitStringWidget)
		{
			SplitStringWidget splitStringWidget = widget as SplitStringWidget;
			int num = -1;
			if (!string.IsNullOrEmpty(splitStringWidget.SplittedText))
			{
				num = splitStringWidget.SplittedText.Length;
			}
			if (splitStringWidget.StartIndex + num != (splitStringWidget.RealStringWidget as WTextRange).Text.Length)
			{
				return false;
			}
			widget = (widget as SplitStringWidget).RealStringWidget;
		}
		IWidget nextSibling = GetNextSibling(widget);
		if (nextSibling != null && ltWidget.Widget is Break && ((ltWidget.Widget as Break).BreakType == BreakType.LineBreak || (ltWidget.Widget as Break).BreakType == BreakType.TextWrappingBreak))
		{
			return false;
		}
		while (nextSibling != null && (nextSibling.LayoutInfo.IsSkip || nextSibling.LayoutInfo.IsSkipBottomAlign || nextSibling is InlineShapeObject || nextSibling is WFieldMark || nextSibling is BookmarkStart || nextSibling is BookmarkEnd))
		{
			nextSibling = GetNextSibling(nextSibling);
		}
		return nextSibling == null;
	}

	internal IWidget GetNextSibling(IWidget widget)
	{
		IList innerList = (WidgetCollection as ParagraphItemCollection).InnerList;
		int num = innerList.IndexOf(widget);
		if (num < 0 || num > innerList.Count - 2)
		{
			return null;
		}
		return innerList[num + 1] as IWidget;
	}

	void IWidget.InitLayoutInfo()
	{
		if (m_layoutInfo is ParagraphLayoutInfo)
		{
			(m_layoutInfo as ParagraphLayoutInfo).InitLayoutInfo();
		}
		m_layoutInfo = null;
		m_paragraphItems = null;
	}

	void IWidget.InitLayoutInfo(IWidget widget)
	{
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutParagraphInfoImpl(this);
		WParagraphFormat paragraphFormat = ParagraphFormat;
		if ((IsHiddenParagraph() && !IsParagraphHasSectionBreak() && BreakCharacterFormat.Hidden && (!IsInCell || base.NextSibling != null)) || (IsParagraphContainsOnlyBookMarks() && IsSkipCellMark()))
		{
			m_layoutInfo.IsSkip = true;
		}
		if (IsVerticalText())
		{
			m_layoutInfo.IsVerticalText = true;
		}
		if (paragraphFormat.LineSpacing == 0f && paragraphFormat.LineSpacingRule == LineSpacingRule.Multiple && paragraphFormat.BeforeSpacing == 0f && paragraphFormat.AfterSpacing == 0f && paragraphFormat.AfterLines == 0f && paragraphFormat.BeforeLines == 0f && !paragraphFormat.SpaceAfterAuto && !paragraphFormat.SpaceBeforeAuto)
		{
			m_layoutInfo.IsSkip = true;
		}
		if (Text == string.Empty && RemoveEmpty)
		{
			m_layoutInfo.IsSkip = true;
		}
		if (BreakCharacterFormat.IsDeleteRevision && IsDeletionParagraph())
		{
			m_layoutInfo.IsSkip = true;
		}
		SplitByLineBreak(Items);
		TextSplitter splitter = new TextSplitter();
		SplitTextRangeByScriptType(Items, splitter);
		SplitLtrAndRtlText(Items, splitter);
		CombineconsecutiveRTL(Items);
		UpdateCharacterSpacingForFitTextAppliedTableCell();
	}

	private void UpdateCharacterSpacingForFitTextAppliedTableCell()
	{
		ParagraphItemCollection items = Items;
		if (!IsInCell || !(Text != string.Empty))
		{
			return;
		}
		WTableCell wTableCell = ((base.Owner is WTableCell) ? (base.Owner as WTableCell) : null);
		if (wTableCell == null || !wTableCell.CellFormat.FitText)
		{
			return;
		}
		float num = 0f;
		int num2 = 0;
		DrawingContext drawingContext = DocumentLayouter.DrawingContext;
		bool flag = false;
		for (int i = 0; i < items.Count; i++)
		{
			Entity entity = items[i];
			if (entity is WTextRange && !string.IsNullOrEmpty((entity as WTextRange).Text) && !(entity is WField))
			{
				WTextRange wTextRange = entity as WTextRange;
				num += drawingContext.MeasureTextRange(wTextRange, wTextRange.Text).Width;
				num2 += wTextRange.TextLength;
				if (!flag)
				{
					if (wTextRange.CharacterFormat.CharacterSpacing == 0f && wTextRange.CharacterFormat.Scaling == 100f)
					{
						flag = true;
					}
					else if (wTextRange.CharacterFormat.CharacterSpacing != 0f || wTextRange.CharacterFormat.Scaling != 100f)
					{
						break;
					}
				}
			}
			else if (entity is WField)
			{
				WField wField = entity as WField;
				if (wField.FieldEnd != null)
				{
					entity = ((wField.FieldSeparator == null) ? wField.FieldEnd : wField.FieldSeparator);
				}
				i = items.IndexOf(entity);
				if (i == -1)
				{
					break;
				}
			}
			else if (((entity is WPicture || entity is Shape || entity is WTextBox || entity is GroupShape || entity is WChart) && !entity.IsFloatingItem(isTextWrapAround: false)) || (entity is Break && ((entity as Break).BreakType == BreakType.LineBreak || (entity as Break).BreakType == BreakType.TextWrappingBreak)) || entity is InlineContentControl)
			{
				flag = false;
				break;
			}
		}
		if (flag && num > 0f)
		{
			UpdateCharacterSpacingOrScaling(num, num2, wTableCell);
		}
	}

	private void UpdateCharacterSpacingOrScaling(float contentWidth, int totalTextLength, WTableCell tableCell)
	{
		float num = 0f;
		float num2 = 0f;
		WTextRange lastTextRange = null;
		CellLayoutInfo cellLayoutInfo = tableCell.m_layoutInfo as CellLayoutInfo;
		float num3 = tableCell.GetCellWidth() - cellLayoutInfo.Margins.Left - cellLayoutInfo.Margins.Right - cellLayoutInfo.Paddings.Right - cellLayoutInfo.Paddings.Left - ((cellLayoutInfo.LeftBorder != null) ? cellLayoutInfo.LeftBorder.RenderingLineWidth : 0f) - ((cellLayoutInfo.RightBorder != null) ? cellLayoutInfo.RightBorder.RenderingLineWidth : 0f);
		if (m_layoutInfo != null)
		{
			ParagraphLayoutInfo paragraphLayoutInfo = m_layoutInfo as ParagraphLayoutInfo;
			if (paragraphLayoutInfo.ListValue != string.Empty)
			{
				num3 -= paragraphLayoutInfo.FirstLineIndent + paragraphLayoutInfo.ListTab;
			}
			if (paragraphLayoutInfo.FirstLineIndent > 0f)
			{
				num3 -= paragraphLayoutInfo.FirstLineIndent;
			}
			if (paragraphLayoutInfo.Margins.Left > 0f)
			{
				num3 -= paragraphLayoutInfo.Margins.Left;
			}
		}
		if (num3 > 0f)
		{
			if (num3 > contentWidth)
			{
				if (totalTextLength > 1)
				{
					num = (num3 - contentWidth) / (float)(totalTextLength - 1);
				}
				if (num != 0f)
				{
					SplitLastCharacterAsNewTextRange(ref lastTextRange);
				}
			}
			else if (num3 < contentWidth)
			{
				num2 = num3 / contentWidth * 100f;
			}
		}
		if (num == 0f && (!(num2 > 0f) || num2 == 100f))
		{
			return;
		}
		ParagraphItemCollection items = Items;
		for (int i = 0; i < items.Count; i++)
		{
			Entity entity = items[i];
			if (entity is WTextRange && !string.IsNullOrEmpty((entity as WTextRange).Text) && !(entity is WField))
			{
				WTextRange wTextRange = entity as WTextRange;
				if (num != 0f)
				{
					if (lastTextRange != null && lastTextRange == wTextRange)
					{
						break;
					}
					wTextRange.CharacterFormat.CharacterSpacing = (float)Math.Round(num, 2);
				}
				else
				{
					wTextRange.CharacterFormat.Scaling = (float)Math.Round(num2, 1);
				}
			}
			else if (entity is WField)
			{
				WField wField = entity as WField;
				if (wField.FieldEnd != null)
				{
					entity = ((wField.FieldSeparator == null) ? wField.FieldEnd : wField.FieldSeparator);
				}
				i = items.IndexOf(entity);
				if (i == -1)
				{
					break;
				}
			}
		}
	}

	private void SplitLastCharacterAsNewTextRange(ref WTextRange lastTextRange)
	{
		ParagraphItemCollection items = Items;
		for (int num = items.Count - 1; num >= 0; num--)
		{
			Entity entity = items[num];
			if (entity is WTextRange && !string.IsNullOrEmpty((entity as WTextRange).Text) && !(entity is WField))
			{
				WTextRange wTextRange = entity as WTextRange;
				if (wTextRange.TextLength > 1)
				{
					string text = wTextRange.Text;
					char c = text[text.Length - 1];
					string text2 = text.Substring(0, text.Length - 1);
					wTextRange.Text = text2;
					WTextRange wTextRange2 = new WTextRange(m_doc);
					wTextRange2.ApplyCharacterFormat(wTextRange.CharacterFormat);
					wTextRange2.Text = c.ToString();
					Items.Insert(wTextRange.Index + 1, wTextRange2);
					lastTextRange = wTextRange2;
				}
				if (lastTextRange == null)
				{
					lastTextRange = wTextRange;
				}
				break;
			}
		}
	}

	internal bool IsDeletionParagraph()
	{
		bool flag = false;
		for (int i = 0; i < Items.Count; i++)
		{
			flag = ((!(Items[i] is InlineContentControl)) ? ((Items[i] is Break) ? (Items[i] as Break).CharacterFormat.IsDeleteRevision : Items[i].ParaItemCharFormat.IsDeleteRevision) : (Items[i] as InlineContentControl).IsDeletion());
			if (!flag)
			{
				break;
			}
		}
		if (IsEmptyParagraph())
		{
			flag = true;
		}
		return flag;
	}

	internal bool IsPreviousParagraphInDeleteRevision()
	{
		IEntity previousSibling = base.PreviousSibling;
		while (previousSibling is WParagraph)
		{
			if ((previousSibling as WParagraph).BreakCharacterFormat.IsDeleteRevision && (previousSibling as WParagraph).IsDeletionParagraph())
			{
				previousSibling = previousSibling.PreviousSibling;
				continue;
			}
			if ((previousSibling as WParagraph).BreakCharacterFormat.IsDeleteRevision)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	internal void CombineconsecutiveRTL(ParagraphItemCollection paraItems)
	{
		for (int i = 0; i <= paraItems.Count - 2; i++)
		{
			if (paraItems[i] is InlineContentControl inlineContentControl)
			{
				CombineconsecutiveRTL(inlineContentControl.ParagraphItems);
			}
			if (paraItems[i] is WTextRange && paraItems[i + 1] is WTextRange && (!(paraItems[i] is WField) || !((IWidget)paraItems[i]).LayoutInfo.IsSkip))
			{
				WTextRange wTextRange = paraItems[i] as WTextRange;
				WTextRange wTextRange2 = paraItems[i + 1] as WTextRange;
				if (wTextRange.CharacterRange == CharacterRangeType.RTL && wTextRange2.CharacterRange == CharacterRangeType.RTL && !(wTextRange is WField) && !(wTextRange2 is WField) && wTextRange.Text.Length > 0 && wTextRange2.Text.Length > 0 && !TextSplitter.IsWordSplitChar(wTextRange.Text[wTextRange.Text.Length - 1]) && !TextSplitter.IsWordSplitChar(wTextRange2.Text[0]) && wTextRange.CharacterFormat.Compare(wTextRange2.CharacterFormat))
				{
					wTextRange.Text += wTextRange2.Text;
					paraItems.RemoveAt(i + 1);
					i--;
				}
			}
		}
	}

	private bool IsParagraphContainsOnlyBookMarks()
	{
		for (int i = 0; i < ChildEntities.Count; i++)
		{
			if (!(ChildEntities[i] is BookmarkStart) && !(ChildEntities[i] is BookmarkEnd))
			{
				return false;
			}
		}
		return true;
	}

	private void SplitTextRangeByScriptType(ParagraphItemCollection paraItems, TextSplitter splitter)
	{
		int num = 1;
		for (int i = 0; i < paraItems.Count; i += num)
		{
			num = 1;
			if (paraItems[i] is InlineContentControl inlineContentControl)
			{
				SplitTextRangeByScriptType(inlineContentControl.ParagraphItems, splitter);
			}
			WTextRange wTextRange = paraItems[i] as WTextRange;
			if (paraItems[i] is WFieldMark && (paraItems[i] as WFieldMark).Type == FieldMarkType.FieldEnd && (paraItems[i] as WFieldMark).ParentField != null)
			{
				(paraItems[i] as WFieldMark).ParentField.IsFieldRangeUpdated = false;
			}
			if ((paraItems[i] is WField && ((IWidget)paraItems[i]).LayoutInfo.IsSkip) || wTextRange == null || paraItems[i] is WField || (wTextRange.m_layoutInfo != null && wTextRange.m_layoutInfo.IsSkip))
			{
				continue;
			}
			_ = wTextRange.CharacterFormat.IdctHint;
			List<FontScriptType> fontScriptTypes = new List<FontScriptType>();
			bool splitTextBasedOnSubType = base.Document.FontSettings.FallbackFonts.Count > 0;
			string[] array = splitter.SplitTextByFontScriptType(wTextRange.Text, ref fontScriptTypes, splitTextBasedOnSubType);
			if (array.Length > 1)
			{
				for (int j = 0; j < array.Length; j++)
				{
					string text = array[j];
					if (j > 0)
					{
						WTextRange wTextRange2 = wTextRange.Clone() as WTextRange;
						wTextRange2.Text = text;
						wTextRange2.ScriptType = fontScriptTypes[j];
						paraItems.Insert(i + j, wTextRange2);
						num++;
					}
					else
					{
						wTextRange.Text = text;
						wTextRange.ScriptType = fontScriptTypes[j];
					}
				}
			}
			else if (array.Length != 0)
			{
				wTextRange.ScriptType = fontScriptTypes[0];
			}
			fontScriptTypes.Clear();
		}
	}

	internal void SplitLtrAndRtlText(ParagraphItemCollection paraItems, TextSplitter splitter)
	{
		bool? isPrevLTRText = null;
		int num = 1;
		bool hasRTLCharacter = false;
		List<CharacterRangeType> characterRangeTypes = new List<CharacterRangeType>();
		for (int i = 0; i < paraItems.Count; i += num)
		{
			num = 1;
			if (paraItems[i] is InlineContentControl inlineContentControl)
			{
				SplitLtrAndRtlText(inlineContentControl.ParagraphItems, splitter);
			}
			WTextRange wTextRange = paraItems[i] as WTextRange;
			if (paraItems[i] is WFieldMark && (paraItems[i] as WFieldMark).Type == FieldMarkType.FieldEnd && (paraItems[i] as WFieldMark).ParentField != null)
			{
				(paraItems[i] as WFieldMark).ParentField.IsFieldRangeUpdated = false;
			}
			if (paraItems[i] is WField)
			{
				IWidget widget = paraItems[i];
				if (widget != null && widget.LayoutInfo.IsSkip)
				{
					continue;
				}
			}
			if (wTextRange == null || paraItems[i] is WField || (wTextRange.m_layoutInfo != null && wTextRange.m_layoutInfo.IsSkip))
			{
				continue;
			}
			string text = wTextRange.Text;
			bool bidi = wTextRange.CharacterFormat.Bidi;
			bool isRTLLang = false;
			int count = characterRangeTypes.Count;
			if (bidi && wTextRange.CharacterFormat.HasValueWithParent(75))
			{
				isRTLLang = IsRightToLeftLang(wTextRange.CharacterFormat.LocaleIdBidi);
			}
			string[] array = splitter.SplitTextByConsecutiveLtrAndRtl(text, bidi, isRTLLang, ref characterRangeTypes, ref isPrevLTRText, ref hasRTLCharacter);
			if (array.Length > 1)
			{
				for (int j = 0; j < array.Length; j++)
				{
					text = array[j];
					if (j > 0)
					{
						WTextRange wTextRange2 = wTextRange.Clone() as WTextRange;
						wTextRange2.Text = text;
						wTextRange2.CharacterRange = characterRangeTypes[j + count];
						paraItems.Insert(i + j, wTextRange2);
						num++;
					}
					else
					{
						wTextRange.Text = text;
						wTextRange.CharacterRange = characterRangeTypes[count];
					}
				}
			}
			else if (array.Length != 0)
			{
				wTextRange.CharacterRange = characterRangeTypes[count];
			}
		}
		characterRangeTypes.Clear();
	}

	private bool IsRightToLeftLang(short id)
	{
		if (id != 14337 && id != 15361 && id != 5121 && id != 3073 && id != 2049 && id != 11265 && id != 13313 && id != 12289 && id != 4097 && id != 8193 && id != 16385 && id != 1025 && id != 10241 && id != 7169 && id != 9217)
		{
			return id == 1065;
		}
		return true;
	}

	internal string GetDisplayText(ParagraphItemCollection ParagraphItems)
	{
		string text = "";
		for (int i = 0; i < ParagraphItems.Count; i++)
		{
			IWidget widget = ParagraphItems[i];
			if (!(widget is WField) && widget is WTextRange && !widget.LayoutInfo.IsSkip)
			{
				text = ((!((widget as WTextRange).Text == "") || !(widget.LayoutInfo is TabsLayoutInfo)) ? (text + (widget as WTextRange).Text) : (text + "\t"));
			}
			else if (widget is InlineContentControl)
			{
				text += GetDisplayText((widget as InlineContentControl).ParagraphItems);
			}
		}
		return text;
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		m_paragraphItems = null;
		foreach (Entity childEntity in ChildEntities)
		{
			childEntity.InitLayoutInfo(entity, ref isLastTOCEntry);
			if (isLastTOCEntry)
			{
				return;
			}
		}
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}

	internal bool IsVerticalText()
	{
		Entity ownerEntity = GetOwnerEntity();
		if ((!IsInCell || !(ownerEntity is WTableCell) || ((ownerEntity as WTableCell).CellFormat.TextDirection != TextDirection.VerticalTopToBottom && (ownerEntity as WTableCell).CellFormat.TextDirection != TextDirection.VerticalBottomToTop)) && (!(ownerEntity is Shape) || !(ownerEntity as Shape).m_layoutInfo.IsVerticalText) && (!(ownerEntity is ChildShape) || ownerEntity is ChildGroupShape || !(ownerEntity as ChildShape).m_layoutInfo.IsVerticalText))
		{
			if (ownerEntity is WTextBox)
			{
				return (ownerEntity as WTextBox).m_layoutInfo.IsVerticalText;
			}
			return false;
		}
		return true;
	}

	private bool IsHiddenParagraph()
	{
		bool flag = false;
		for (int i = 0; i < Items.Count; i++)
		{
			flag = ((!(Items[i] is InlineContentControl)) ? ((Items[i] is Break) ? (Items[i] as Break).CharacterFormat.Hidden : Items[i].ParaItemCharFormat.Hidden) : (Items[i] as InlineContentControl).IsHidden());
			if (!flag)
			{
				break;
			}
		}
		if (IsEmptyParagraph())
		{
			flag = true;
		}
		return flag;
	}

	private void SplitByLineBreak(ParagraphItemCollection paraItems)
	{
		if (!Text.Contains(ControlChar.LineBreak))
		{
			return;
		}
		for (int i = 0; i < paraItems.Count; i++)
		{
			if (paraItems[i] is InlineContentControl inlineContentControl)
			{
				SplitByLineBreak(inlineContentControl.ParagraphItems);
			}
			if (!(paraItems[i] is WTextRange wTextRange) || !wTextRange.Text.Contains(ControlChar.LineBreak))
			{
				continue;
			}
			IWParagraphStyle iWParagraphStyle = null;
			if (wTextRange.Owner is InlineContentControl && wTextRange.OwnerParagraph != null && wTextRange.OwnerParagraph.ParaStyle != null)
			{
				iWParagraphStyle = wTextRange.OwnerParagraph.ParaStyle;
			}
			string[] array = wTextRange.Text.Split(ControlChar.LineBreakChar);
			wTextRange.Text = array[0];
			int num = wTextRange.Index + 1;
			for (int j = 1; j < array.Length; j++)
			{
				Break @break = new Break(base.Document, BreakType.LineBreak);
				@break.TextRange.Text = ControlChar.LineBreak;
				@break.TextRange.CharacterFormat.ImportContainer(wTextRange.CharacterFormat);
				@break.TextRange.CharacterFormat.CopyProperties(wTextRange.CharacterFormat);
				paraItems.Insert(num, @break);
				if (iWParagraphStyle != null)
				{
					@break.CharacterFormat.ApplyBase(iWParagraphStyle.CharacterFormat);
				}
				WTextRange wTextRange2 = wTextRange.Clone() as WTextRange;
				wTextRange2.Text = array[j];
				string[] array2 = wTextRange2.Text.Split(ControlChar.TabChar);
				if (wTextRange2.Text != string.Empty)
				{
					if (array.Length > 1)
					{
						wTextRange2.Text = array2[0];
					}
					paraItems.Insert(num + 1, wTextRange2);
					if (iWParagraphStyle != null)
					{
						wTextRange2.CharacterFormat.ApplyBase(iWParagraphStyle.CharacterFormat);
					}
					num = paraItems.IndexOf(wTextRange2) + 1;
				}
				else
				{
					num++;
				}
				for (int k = 1; k < array2.Length; k++)
				{
					WTextRange wTextRange3 = wTextRange.Clone() as WTextRange;
					wTextRange3.Text = ControlChar.Tab;
					paraItems.Insert(num, wTextRange3);
					if (iWParagraphStyle != null)
					{
						wTextRange3.CharacterFormat.ApplyBase(iWParagraphStyle.CharacterFormat);
					}
					num = paraItems.IndexOf(wTextRange3) + 1;
					if (array2[k] != string.Empty)
					{
						WTextRange wTextRange4 = wTextRange.Clone() as WTextRange;
						wTextRange4.Text = array2[k];
						paraItems.Insert(num, wTextRange4);
						if (iWParagraphStyle != null)
						{
							wTextRange4.CharacterFormat.ApplyBase(iWParagraphStyle.CharacterFormat);
						}
						num = paraItems.IndexOf(wTextRange4) + 1;
					}
				}
			}
		}
	}

	internal bool IsFirstParagraphOfOwnerTextBody()
	{
		if (!IsInCell && base.OwnerTextBody != null)
		{
			Entity ownerBaseEntity = GetOwnerBaseEntity(this);
			return IsFirstParagraphOfTextBody(ownerBaseEntity, this);
		}
		return false;
	}

	internal bool IsFirstParagraphOfTextBody(Entity ownerBaseEntity, Entity childEntity)
	{
		bool result = false;
		switch (ownerBaseEntity.EntityType)
		{
		case EntityType.Section:
			result = (childEntity.Owner as WTextBody).Items[0] == childEntity && GetOwnerSection(childEntity).Index == 0;
			break;
		case EntityType.BlockContentControl:
			childEntity = ownerBaseEntity as BlockContentControl;
			ownerBaseEntity = GetOwnerBaseEntity(childEntity);
			result = IsFirstParagraphOfTextBody(ownerBaseEntity, childEntity);
			break;
		case EntityType.TextBox:
			result = (ownerBaseEntity as WTextBox).TextBoxBody.Items[0] == childEntity;
			break;
		case EntityType.Shape:
		case EntityType.AutoShape:
			result = (ownerBaseEntity as Shape).TextBody.Items[0] == childEntity;
			break;
		case EntityType.ChildShape:
			result = (ownerBaseEntity as ChildShape).TextBody.Items[0] == childEntity;
			break;
		}
		return result;
	}

	private bool IsSkipCellMark()
	{
		WTableCell wTableCell = null;
		if (IsInCell)
		{
			wTableCell = GetOwnerEntity() as WTableCell;
		}
		if (wTableCell != null && wTableCell.OwnerRow.ContentControl != null && wTableCell.Index == wTableCell.OwnerRow.ChildEntities.Count - 1)
		{
			return false;
		}
		float num = 0f;
		if (wTableCell != null && wTableCell.LastParagraph != null && wTableCell.LastParagraph.Equals(this) && base.PreviousSibling != null)
		{
			WSection wSection = GetOwnerSection(wTableCell) as WSection;
			num = wSection.PageSetup.PageSize.Height - (wSection.PageSetup.HeaderDistance + wSection.PageSetup.FooterDistance);
			if ((!(base.PreviousSibling is WTable) || (base.PreviousSibling as WTable).Rows.Count <= 0 || !((base.PreviousSibling as WTable).LastRow.Height < num)) && (!(base.PreviousSibling is BlockContentControl) || !((base.PreviousSibling as BlockContentControl).LastChildEntity is WTable) || ((base.PreviousSibling as BlockContentControl).LastChildEntity as WTable).Rows.Count <= 0 || !(((base.PreviousSibling as BlockContentControl).LastChildEntity as WTable).LastRow.Height < num)))
			{
				if (wTableCell.CellFormat.HideMark)
				{
					if (ListFormat != null)
					{
						return ListFormat.CurrentListLevel == null;
					}
					return true;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	internal float GetHeight(WParagraph paragraph, ParagraphItem paraItem)
	{
		float height = DocumentLayouter.m_dc.MeasureString(" ", paragraph.BreakCharacterFormat.Font, null, paragraph.BreakCharacterFormat, isMeasureFromTabList: false, FontScriptType.English).Height;
		if (paraItem != null)
		{
			paraItem = paragraph.GetNextSibling(paraItem) as ParagraphItem;
			bool flag = true;
			while (paraItem != null)
			{
				flag = ((paraItem is WPicture) ? ((paraItem as WPicture).TextWrappingStyle == TextWrappingStyle.Inline) : ((paraItem is Shape) ? ((paraItem as Shape).WrapFormat.TextWrappingStyle == TextWrappingStyle.Inline) : ((paraItem is WTextBox) ? ((paraItem as WTextBox).TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Inline) : (!(paraItem is WOleObject) || (paraItem as WOleObject).OlePicture.TextWrappingStyle == TextWrappingStyle.Inline))));
				if (paraItem is BookmarkStart || paraItem is BookmarkEnd || !flag)
				{
					paraItem = paragraph.GetNextSibling(paraItem) as ParagraphItem;
					continue;
				}
				return paragraph.GetSize(paraItem, height);
			}
		}
		return height;
	}

	internal float GetSize(ParagraphItem paraItem, float height)
	{
		if (paraItem is WTextRange)
		{
			return DocumentLayouter.m_dc.MeasureString(" ", (paraItem as WTextRange).CharacterFormat.Font, null, (paraItem as WTextRange).CharacterFormat, isMeasureFromTabList: false, FontScriptType.English).Height;
		}
		if (paraItem is WSymbol)
		{
			return DocumentLayouter.m_dc.MeasureString(" ", (paraItem as WSymbol).CharacterFormat.Font, null, (paraItem as WSymbol).CharacterFormat, isMeasureFromTabList: false, FontScriptType.English).Height;
		}
		if (paraItem is WFootnote)
		{
			return DocumentLayouter.m_dc.MeasureString(" ", (paraItem as WFootnote).MarkerCharacterFormat.Font, null, (paraItem as WFootnote).MarkerCharacterFormat, isMeasureFromTabList: false, FontScriptType.English).Height;
		}
		if (paraItem is WPicture)
		{
			return DocumentLayouter.m_dc.MeasureImage(paraItem as WPicture).Height;
		}
		if (paraItem is WTextBox)
		{
			return (paraItem as WTextBox).TextBoxFormat.Height;
		}
		if (paraItem is Shape)
		{
			return (paraItem as Shape).Height;
		}
		if (paraItem is WOleObject)
		{
			return (paraItem as WOleObject).OlePicture.Height;
		}
		return height;
	}

	internal bool IsAdjacentParagraphHaveSameBorders(WParagraph adjacentParagraph, float currentParaLeftIndent)
	{
		if (IsSameLeftIndent(adjacentParagraph, currentParaLeftIndent) && ParagraphFormat.RightIndent == adjacentParagraph.ParagraphFormat.RightIndent && ((ParagraphFormat.IsFrame && adjacentParagraph.ParagraphFormat.IsFrame && ParagraphFormat.IsInSameFrame(adjacentParagraph.ParagraphFormat)) || (!ParagraphFormat.IsFrame && !adjacentParagraph.ParagraphFormat.IsFrame)) && IsSameAdjacentBorder(ParagraphFormat.Borders.Bottom, adjacentParagraph.ParagraphFormat.Borders.Bottom) && IsSameAdjacentBorder(ParagraphFormat.Borders.Top, adjacentParagraph.ParagraphFormat.Borders.Top) && IsSameAdjacentBorder(ParagraphFormat.Borders.Right, adjacentParagraph.ParagraphFormat.Borders.Right))
		{
			return IsSameAdjacentBorder(ParagraphFormat.Borders.Left, adjacentParagraph.ParagraphFormat.Borders.Left);
		}
		return false;
	}

	internal bool IsSameAdjacentBorder(Border border, Border adjacentBorder)
	{
		if (border.BorderType == adjacentBorder.BorderType && border.LineWidth == adjacentBorder.LineWidth && border.Space == adjacentBorder.Space)
		{
			return border.Color == adjacentBorder.Color;
		}
		return false;
	}

	internal bool IsSameLeftIndent(WParagraph adjacentParagrph, float firstParaLeftPosition)
	{
		float num = ((adjacentParagrph.ParagraphFormat.FirstLineIndent > 0f) ? 0f : adjacentParagrph.ParagraphFormat.FirstLineIndent);
		float num2 = adjacentParagrph.ParagraphFormat.LeftIndent + num;
		if (adjacentParagrph.ListFormat.IsEmptyList || IsSectionEndMark())
		{
			return firstParaLeftPosition == num2;
		}
		WListFormat listFormatValue = adjacentParagrph.GetListFormatValue();
		WParagraphStyle wParagraphStyle = adjacentParagrph.ParaStyle as WParagraphStyle;
		if (listFormatValue != null && listFormatValue.CurrentListStyle != null)
		{
			_ = listFormatValue.CurrentListStyle;
			WListLevel listLevel = adjacentParagrph.GetListLevel(listFormatValue);
			if (listLevel.ParagraphFormat.HasValue(2))
			{
				num2 = listLevel.ParagraphFormat.LeftIndent;
			}
			if (wParagraphStyle != null && adjacentParagrph.ListFormat.ListType == ListType.NoList && wParagraphStyle.ParagraphFormat.HasValue(2))
			{
				num2 = wParagraphStyle.ParagraphFormat.LeftIndent;
			}
			if (adjacentParagrph.ParagraphFormat.HasValue(2))
			{
				num2 = adjacentParagrph.ParagraphFormat.LeftIndent;
			}
			if (listLevel.ParagraphFormat.HasValue(5))
			{
				num = listLevel.ParagraphFormat.FirstLineIndent;
			}
			if (wParagraphStyle != null && adjacentParagrph.ListFormat.ListType == ListType.NoList && wParagraphStyle.ParagraphFormat.HasValue(5))
			{
				num = wParagraphStyle.ParagraphFormat.FirstLineIndent;
			}
			if (adjacentParagrph.ParagraphFormat.HasValue(5))
			{
				num = adjacentParagrph.ParagraphFormat.FirstLineIndent;
			}
			if (num < 0f && num2 == 0f && !adjacentParagrph.ParagraphFormat.HasValue(2))
			{
				num2 = Math.Abs(num);
			}
			num2 += num;
		}
		return firstParaLeftPosition == num2;
	}

	internal bool IsParagraphBeforeSpacingNeedToSkip()
	{
		foreach (ParagraphItem childEntity in ChildEntities)
		{
			if (!(childEntity is BookmarkStart) && !(childEntity is BookmarkEnd))
			{
				if (childEntity is Break && (childEntity as Break).BreakType == BreakType.ColumnBreak)
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	internal double GetDefaultTabWidth()
	{
		Entity owner = base.Owner;
		while (!(owner is WSection) && owner.Owner != null)
		{
			owner = owner.Owner;
		}
		if (owner is WSection)
		{
			return (owner as WSection).PageSetup.DefaultTabWidth;
		}
		return 36.0;
	}

	internal bool StartsWithExt(string text, string value)
	{
		return text.StartsWith(value);
	}

	internal void GetMinimumAndMaximumWordWidth(ref float minimumWordWidth, ref float maximumWordWidth, ref float paragraphWidth, bool needtoCalculateParaWidth)
	{
		float maximumWordWidthInPara = 0f;
		float minumWordWidthInPara = 0f;
		(ChildEntities as ParagraphItemCollection).GetMinimumAndMaximumWordWidthInPara(ref maximumWordWidthInPara, ref minumWordWidthInPara);
		if (maximumWordWidthInPara > 0f)
		{
			float leftIndent = ParagraphFormat.LeftIndent;
			maximumWordWidthInPara += ((!(leftIndent > 0f)) ? 0f : ((ParagraphFormat.FirstLineIndent < 0f) ? (leftIndent + ParagraphFormat.FirstLineIndent) : leftIndent)) + ((ParagraphFormat.RightIndent > 0f) ? ParagraphFormat.RightIndent : 0f);
		}
		if (minimumWordWidth == 0f || minumWordWidthInPara < minimumWordWidth)
		{
			minimumWordWidth = minumWordWidthInPara;
		}
		if (maximumWordWidth == 0f || maximumWordWidthInPara > maximumWordWidth)
		{
			maximumWordWidth = maximumWordWidthInPara;
		}
		float num = 0f;
		if (needtoCalculateParaWidth)
		{
			num = GetParagraphWidth();
			if (num > paragraphWidth)
			{
				paragraphWidth = num;
			}
		}
	}

	internal float GetParagraphWidth()
	{
		float num = GetParaItemsWidth(ChildEntities as ParagraphItemCollection);
		if (num > 0f)
		{
			num += ((ParagraphFormat.LeftIndent > 0f) ? ParagraphFormat.LeftIndent : 0f) + ((ParagraphFormat.RightIndent > 0f) ? ParagraphFormat.RightIndent : 0f);
		}
		return num;
	}

	private float GetParaItemsWidth(ParagraphItemCollection paraItems)
	{
		float num = 0f;
		DrawingContext drawingContext = DocumentLayouter.DrawingContext;
		for (int i = 0; i < paraItems.Count; i++)
		{
			Entity entity = paraItems[i];
			if (!(entity is WField) && entity is WTextRange && !string.IsNullOrEmpty((entity as WTextRange).Text))
			{
				string text = (entity as WTextRange).Text;
				if (i == paraItems.Count - 1)
				{
					text = text.TrimEnd();
				}
				num += drawingContext.MeasureString(text, (entity as WTextRange).CharacterFormat.Font, null, (entity as WTextRange).ScriptType).Width;
			}
			else if (entity is WField || entity is WOleObject)
			{
				WField wField = ((entity is WField) ? (entity as WField) : (((entity as WOleObject).Field != null) ? (entity as WOleObject).Field : null));
				if (wField != null && wField.FieldEnd != null)
				{
					entity = ((wField.FieldSeparator == null) ? wField.FieldEnd : wField.FieldSeparator);
				}
				i = paraItems.IndexOf(entity);
				if (i == -1)
				{
					break;
				}
			}
			else if (entity is WPicture && (entity as WPicture).TextWrappingStyle != TextWrappingStyle.InFrontOfText && (entity as WPicture).TextWrappingStyle != TextWrappingStyle.Behind)
			{
				num += (entity as WPicture).Width;
			}
			else if (entity is InlineContentControl && (entity as InlineContentControl).ParagraphItems.Count > 0)
			{
				num += GetParaItemsWidth((entity as InlineContentControl).ParagraphItems);
			}
			else if (entity is Break && ((entity as Break).BreakType == BreakType.LineBreak || (entity as Break).BreakType == BreakType.TextWrappingBreak))
			{
				break;
			}
		}
		return num;
	}

	internal void CompareParagraphFormats(WParagraph revisedParagraph)
	{
		bool isComparing = revisedParagraph.Document.IsComparing;
		bool isComparing2 = base.Document.IsComparing;
		revisedParagraph.Document.IsComparing = true;
		base.Document.IsComparing = true;
		CompareCharacterFormat(BreakCharacterFormat, revisedParagraph.BreakCharacterFormat);
		if ((revisedParagraph.Document.ComparisonOptions != null && !revisedParagraph.Document.ComparisonOptions.DetectFormatChanges) || !revisedParagraph.CompareParagraphFormat(this))
		{
			revisedParagraph.ApplyParaFormatChange(this);
		}
		revisedParagraph.Document.IsComparing = isComparing;
		base.Document.IsComparing = isComparing2;
	}

	private void CompareMatchedTextFormatting(TextSelection selection, ref int startRangeIndex, ref int endRangeIndex, ref int textStartIndex, ref int textEndIndex, ref int orgParaItemIndex)
	{
		SplitOrgAndRevMatchedText(selection, ref startRangeIndex, ref endRangeIndex, ref textStartIndex, ref textEndIndex, ref orgParaItemIndex);
		if (selection.StartTextRange != selection.EndTextRange)
		{
			if (startRangeIndex == endRangeIndex)
			{
				for (int i = selection.StartTextRange.Index; i <= selection.EndTextRange.Index; i++)
				{
					if (selection.OwnerParagraph.Items[i] is WTextRange)
					{
						CompareCharacterFormat((selection.OwnerParagraph.Items[i] as WTextRange).CharacterFormat, (Items[startRangeIndex] as WTextRange).CharacterFormat);
					}
				}
			}
			else
			{
				CompareMultipleTextRange(selection, ref startRangeIndex, ref endRangeIndex, ref textStartIndex, ref textEndIndex, ref orgParaItemIndex);
			}
		}
		else if (startRangeIndex == endRangeIndex)
		{
			CompareCharacterFormat(selection.StartTextRange.CharacterFormat, (Items[startRangeIndex] as WTextRange).CharacterFormat);
		}
		else
		{
			CompareMultipleTextRange(selection, ref startRangeIndex, ref endRangeIndex, ref textStartIndex, ref textEndIndex, ref orgParaItemIndex);
		}
	}

	private void CompareMultipleTextRange(TextSelection selection, ref int startRangeIndex, ref int endRangeIndex, ref int textStartIndex, ref int textEndIndex, ref int orgParaItemIndex)
	{
		if (startRangeIndex == endRangeIndex)
		{
			return;
		}
		bool flag = true;
		List<WTextRange> list = new List<WTextRange>();
		for (int i = startRangeIndex; i <= endRangeIndex; i++)
		{
			if (Items[i] is WTextRange)
			{
				list.Add((Items[i] as WTextRange).Clone() as WTextRange);
				if (i != startRangeIndex && !(Items[i] as WTextRange).CharacterFormat.Compare((Items[startRangeIndex] as WTextRange).CharacterFormat))
				{
					flag = false;
				}
			}
		}
		if (flag)
		{
			for (int j = selection.StartTextRange.Index; j <= selection.EndTextRange.Index; j++)
			{
				CompareCharacterFormat((selection.OwnerParagraph.Items[j] as WTextRange).CharacterFormat, list[0].CharacterFormat);
			}
			return;
		}
		int num = 0;
		WTextRange wTextRange = selection.EndTextRange;
		for (int k = selection.StartTextRange.Index; k <= wTextRange.Index; k++)
		{
			if (!(selection.OwnerParagraph.Items[k] is WTextRange))
			{
				continue;
			}
			WTextRange wTextRange2 = selection.OwnerParagraph.Items[k] as WTextRange;
			WTextRange wTextRange3 = list[num];
			if (wTextRange3.Text.Length > wTextRange2.Text.Length)
			{
				string text = wTextRange3.Text;
				WTextRange wTextRange4 = wTextRange3.Clone() as WTextRange;
				wTextRange3.Text = text.Substring(0, wTextRange2.Text.Length);
				wTextRange4.Text = text.Substring(wTextRange2.Text.Length);
				list.Insert(num + 1, wTextRange4);
				CompareCharacterFormat(wTextRange2.CharacterFormat, wTextRange3.CharacterFormat);
			}
			else if (wTextRange2.Text.Length > wTextRange3.Text.Length)
			{
				string text2 = wTextRange2.Text;
				WTextRange wTextRange5 = wTextRange2.Clone() as WTextRange;
				if (k == selection.StartTextRange.Index)
				{
					wTextRange2.Text = text2.Substring(0, wTextRange3.Text.Length);
					wTextRange5.Text = text2.Substring(wTextRange3.Text.Length);
					selection.OwnerParagraph.Items.Insert(k + 1, wTextRange5);
					CompareCharacterFormat(wTextRange2.CharacterFormat, wTextRange3.CharacterFormat);
					if (selection.StartTextRange.Index == selection.EndTextRange.Index)
					{
						wTextRange = wTextRange5;
					}
				}
				else
				{
					wTextRange5.Text = text2.Substring(0, wTextRange3.Text.Length);
					wTextRange2.Text = text2.Substring(wTextRange3.Text.Length);
					selection.OwnerParagraph.Items.Insert(k, wTextRange5);
					CompareCharacterFormat(wTextRange5.CharacterFormat, wTextRange3.CharacterFormat);
				}
			}
			else
			{
				CompareCharacterFormat(wTextRange2.CharacterFormat, wTextRange3.CharacterFormat);
			}
			num++;
		}
		orgParaItemIndex = wTextRange.Index;
	}

	private void SplitOrgAndRevMatchedText(TextSelection selection, ref int startRangeIndex, ref int endRangeIndex, ref int textStartIndex, ref int textEndIndex, ref int orgParaItemIndex)
	{
		if (selection.m_startCut > 0)
		{
			orgParaItemIndex++;
		}
		selection.GetRanges();
		if (textStartIndex > 0)
		{
			WTextRange obj = Items[startRangeIndex] as WTextRange;
			string text = obj.Text;
			obj.Text = text.Substring(textStartIndex);
			WTextRange wTextRange = obj.Clone() as WTextRange;
			wTextRange.Text = text.Substring(0, textStartIndex);
			Items.Insert(startRangeIndex, wTextRange);
			if (startRangeIndex == endRangeIndex)
			{
				textEndIndex -= textStartIndex;
			}
			textStartIndex = 0;
			startRangeIndex++;
			endRangeIndex++;
		}
		if (textEndIndex != (Items[endRangeIndex] as WTextRange).Text.Length - 1)
		{
			WTextRange wTextRange2 = Items[endRangeIndex] as WTextRange;
			string text2 = wTextRange2.Text;
			wTextRange2.Text = text2.Substring(0, textEndIndex + 1);
			WTextRange wTextRange3 = wTextRange2.Clone() as WTextRange;
			wTextRange3.Text = text2.Substring(textEndIndex + 1);
			Items.Insert(endRangeIndex + 1, wTextRange3);
			textEndIndex = wTextRange2.Text.Length - 1;
		}
	}

	private void CompareCharacterFormat(WCharacterFormat orgCharFormat, WCharacterFormat revCharFormat)
	{
		WordDocument document = orgCharFormat.Document;
		bool isComparing = document.IsComparing;
		bool isComparing2 = revCharFormat.Document.IsComparing;
		document.IsComparing = true;
		revCharFormat.Document.IsComparing = true;
		if ((document.ComparisonOptions != null && !document.ComparisonOptions.DetectFormatChanges) || !orgCharFormat.Compare(revCharFormat))
		{
			revCharFormat.ApplyCharFormatChange(orgCharFormat);
		}
		revCharFormat.Document.IsComparing = isComparing2;
		document.IsComparing = isComparing;
	}

	private void CompareFormatting(bool matched, ref bool firstMatchInPara, WordDocument originalDocument, WTextBody originalTextbody, int textRangeIndex, TextSelection matchedText, ref int startRangeIndex, ref int endRangeIndex, ref int textStartIndex, ref int textEndIndex, ref int orgParaItemIndex)
	{
		CompareMatchedTextFormatting(matchedText, ref startRangeIndex, ref endRangeIndex, ref textStartIndex, ref textEndIndex, ref orgParaItemIndex);
		if (!firstMatchInPara)
		{
			matchedText.OwnerParagraph.CompareParagraphFormats(this);
			firstMatchInPara = true;
			return;
		}
		WParagraph wParagraph = ((originalDocument == null) ? (originalTextbody.Items[originalTextbody.m_matchBodyItemIndex] as WParagraph) : (originalDocument.Sections[originalDocument.m_matchSectionIndex].Body.Items[originalDocument.m_matchBodyItemIndex] as WParagraph));
		int num = ((originalTextbody == null) ? base.Document.m_bodyItemIndex : base.OwnerTextBody.m_bodyItemIndex);
		if (matchedText.OwnerParagraph != wParagraph || num != Index)
		{
			matchedText.OwnerParagraph.CompareParagraphFormats(this);
		}
	}

	private void CompareParaFormattingForDelimiter(ref bool firstMatchInPara, WParagraph orgParagraph)
	{
		if (!firstMatchInPara)
		{
			orgParagraph.CompareParagraphFormats(this);
			firstMatchInPara = true;
		}
	}

	internal bool CompareParagraphFormat(WParagraph paragraph)
	{
		if (paragraph == null)
		{
			return false;
		}
		if (!paragraph.ParagraphFormat.Compare(ParagraphFormat))
		{
			return false;
		}
		if (paragraph.ParaStyle.Name != ParaStyle.Name)
		{
			return false;
		}
		if (!paragraph.ListFormat.Compare(ListFormat))
		{
			return false;
		}
		return true;
	}

	private void ApplyParaFormatChange(WParagraph orgParagraph)
	{
		orgParagraph.ListFormat.CompareProperties(ListFormat);
		if (ListFormat.CurrentListStyle != null)
		{
			orgParagraph.ListFormat.ApplyStyle(ListFormat.CurrentListStyle.Name);
		}
		string styleNameId = GetStyleNameId(orgParagraph.StyleName, orgParagraph.Document);
		if (orgParagraph.StyleName != StyleName)
		{
			orgParagraph.ApplyStyle(StyleName);
		}
		orgParagraph.ParagraphFormat.CompareProperties(ParagraphFormat);
		if (base.Document.ComparisonOptions != null && base.Document.ComparisonOptions.DetectFormatChanges)
		{
			orgParagraph.ParagraphFormat.IsFormattingChange = true;
			orgParagraph.ParagraphFormat.ParagraphStyleName = styleNameId;
			orgParagraph.ParagraphFormat.IsFormattingChange = false;
			orgParagraph.ParagraphFormat.IsChangedFormat = true;
			orgParagraph.ParagraphFormat.FormatChangeAuthorName = base.Document.m_authorName;
			orgParagraph.ParagraphFormat.FormatChangeDateTime = base.Document.m_dateTime;
			orgParagraph.Document.ParagraphFormatChange(orgParagraph.ParagraphFormat);
		}
	}

	private string GetStyleNameId(string styleName, WordDocument document)
	{
		string result = "";
		foreach (KeyValuePair<string, string> styleNameId in document.StyleNameIds)
		{
			if (styleNameId.Value == styleName)
			{
				result = styleNameId.Key;
				break;
			}
		}
		return result;
	}

	private void UpdateBodyItemIndex(int textStartIndex, int textEndIndex, int paraItemIndex, WTextBody textBody = null)
	{
		if (textBody == null)
		{
			if (textStartIndex > 0 || textEndIndex > -1)
			{
				SplitUnmatchedTextAndMoveParaItemIndex(this, ref base.Document.m_paraItemIndex, ref textStartIndex, ref textEndIndex);
			}
			base.Document.currBodyItemIndex = base.Document.LastSection.Body.Items.Count;
			base.Document.currSectionIndex = base.Document.LastSection.Index + 1;
			base.Document.currParaItemIndex = 0;
		}
		else
		{
			if (textStartIndex > 0 || textEndIndex > -1)
			{
				SplitUnmatchedTextAndMoveParaItemIndex(this, ref textBody.m_paraItemIndex, ref textStartIndex, ref textEndIndex);
			}
			base.OwnerTextBody.currBodyItemIndex = base.OwnerTextBody.Items.Count - 1;
			base.OwnerTextBody.currParaItemIndex = 0;
		}
	}

	internal override void Compare(WordDocument originalDocument)
	{
		int textStartIndex = 0;
		int startRangeIndex = 0;
		int endRangeIndex = 0;
		int textEndIndex = -1;
		int wordIndex = 0;
		bool firstMatchInPara = false;
		bool isLastWordMatched = false;
		bool hasMatch = false;
		Dictionary<int, SkippedBkmkInfo> skippedBookmarks = null;
		int paraItemIndex = base.Document.currParaItemIndex;
		while (paraItemIndex < Items.Count)
		{
			if (originalDocument.m_sectionIndex >= originalDocument.Sections.Count || (originalDocument.m_sectionIndex == originalDocument.Sections.Count - 1 && originalDocument.m_bodyItemIndex == originalDocument.Sections[originalDocument.m_sectionIndex].Body.ChildEntities.Count))
			{
				UpdateBodyItemIndex(textStartIndex, textEndIndex, paraItemIndex);
				return;
			}
			string nextWord = GetNextWord(ref paraItemIndex, ref textStartIndex, ref textEndIndex, ref startRangeIndex, ref endRangeIndex, ref wordIndex, ref skippedBookmarks);
			if (paraItemIndex >= Items.Count)
			{
				continue;
			}
			base.Document.currParaItemIndex = paraItemIndex;
			ParagraphItem paragraphItem = Items[paraItemIndex];
			if (paraItemIndex == Items.Count - 1 && paragraphItem is WTextRange && string.IsNullOrEmpty((paragraphItem as WTextRange).Text))
			{
				paragraphItem = Items[paraItemIndex--];
			}
			if ((!(paragraphItem is WTextRange) || paragraphItem is WField) && isLastWordMatched)
			{
				SplitOriginalDocumentText(originalDocument);
			}
			switch (paragraphItem.EntityType)
			{
			case EntityType.InlineContentControl:
			case EntityType.Picture:
			case EntityType.Shape:
			case EntityType.TextBox:
			case EntityType.Chart:
			case EntityType.AutoShape:
			case EntityType.GroupShape:
			case EntityType.Math:
				CompareContainerElements(originalDocument, ref paraItemIndex, ref textStartIndex, ref textEndIndex, ref wordIndex, ref isLastWordMatched, ref startRangeIndex);
				break;
			case EntityType.Field:
			case EntityType.MergeField:
			case EntityType.SeqField:
			case EntityType.EmbededField:
			case EntityType.ControlField:
			case EntityType.TextFormField:
			case EntityType.DropDownFormField:
			case EntityType.CheckBox:
			case EntityType.TOC:
				if ((paragraphItem is WField && !(paragraphItem as WField).IsNestedField) || paragraphItem is TableOfContent)
				{
					CompareField(originalDocument, ref paraItemIndex, ref textStartIndex, ref textEndIndex, ref wordIndex, ref isLastWordMatched, ref startRangeIndex);
				}
				else
				{
					UpdateNextRevDocIndex(ref paraItemIndex, ref textStartIndex, ref textEndIndex, ref wordIndex, ref startRangeIndex);
				}
				break;
			case EntityType.Break:
				if (isLastWordMatched || paraItemIndex == 0)
				{
					CompareWithCurrentItemOnly(originalDocument, ref paraItemIndex, ref textStartIndex, ref textEndIndex, ref wordIndex, ref isLastWordMatched, ref startRangeIndex);
				}
				else
				{
					paraItemIndex++;
				}
				break;
			case EntityType.Footnote:
			case EntityType.Symbol:
				CompareWithCurrentItemOnly(originalDocument, ref paraItemIndex, ref textStartIndex, ref textEndIndex, ref wordIndex, ref isLastWordMatched, ref startRangeIndex);
				break;
			case EntityType.BookmarkStart:
			case EntityType.BookmarkEnd:
			case EntityType.EditableRangeStart:
			case EntityType.EditableRangeEnd:
				CompareNonRenderableItems(originalDocument, null, ref paraItemIndex, startRangeIndex);
				break;
			case EntityType.OleObject:
				CompareOleObject(originalDocument, ref paraItemIndex, ref isLastWordMatched, ref startRangeIndex, ref textStartIndex, ref textEndIndex, ref wordIndex);
				break;
			default:
				CompareTextranges(originalDocument, nextWord, ref paraItemIndex, ref textStartIndex, ref textEndIndex, ref wordIndex, ref isLastWordMatched, ref startRangeIndex, ref endRangeIndex, ref firstMatchInPara, ref skippedBookmarks, ref hasMatch);
				break;
			case EntityType.Comment:
			case EntityType.CommentMark:
			case EntityType.CommentEnd:
				break;
			}
		}
		if (Index == base.Document.currBodyItemIndex && base.Document.currParaItemIndex >= Items.Count - 1)
		{
			base.Document.currParaItemIndex = 0;
		}
		if (Items.Count == 0 || IsEmptyParagraph())
		{
			CompareEmptyParagraphs(originalDocument);
		}
		else if (hasMatch)
		{
			ComparePreviousEmptyParagraphs(originalDocument);
		}
		if (originalDocument.m_textStartIndex > 0 || originalDocument.m_textEndIndex > -1)
		{
			WParagraph lastMatchedParagraph = originalDocument.Sections[originalDocument.m_sectionIndex].Body.Items[originalDocument.m_bodyItemIndex] as WParagraph;
			SplitUnmatchedTextAndMoveParaItemIndex(lastMatchedParagraph, ref originalDocument.m_paraItemIndex, ref originalDocument.m_textStartIndex, ref originalDocument.m_textEndIndex);
		}
		if (base.Document.m_textStartIndex > 0 || base.Document.m_textEndIndex > -1)
		{
			WParagraph lastMatchedParagraph2 = base.Document.Sections[base.Document.m_sectionIndex].Body.Items[base.Document.m_bodyItemIndex] as WParagraph;
			SplitUnmatchedTextAndMoveParaItemIndex(lastMatchedParagraph2, ref base.Document.m_paraItemIndex, ref base.Document.m_textStartIndex, ref base.Document.m_textEndIndex);
		}
		if (originalDocument.m_bodyItemIndex >= originalDocument.Sections[originalDocument.m_sectionIndex].Body.Items.Count && base.Document.m_bodyItemIndex >= base.Document.Sections[base.Document.m_sectionIndex].Body.Items.Count)
		{
			originalDocument.m_sectionIndex++;
			originalDocument.UpdateIndex(0, 0, 0, 0, 0);
			base.Document.m_sectionIndex++;
			base.Document.UpdateIndex(0, 0, 0, 0, 0);
		}
	}

	private void IsNeedToMarkInsDel(WordDocument originalDocument, ref bool isNeedToMarkInsertion, ref bool isNeedToMarkDeletion, int revparaItemIndex, int orgMatchedSecIndex, int orgMatchedBodyItemIndex, int orgMatchedParaItemIndex, WTextBody orgTextBody = null)
	{
		if (orgTextBody == null)
		{
			isNeedToMarkInsertion = base.Document.m_sectionIndex != GetOwnerSection(this).Index || base.Document.m_bodyItemIndex != Index || base.Document.m_paraItemIndex < revparaItemIndex;
			isNeedToMarkDeletion = originalDocument.m_sectionIndex != orgMatchedSecIndex || originalDocument.m_bodyItemIndex != orgMatchedBodyItemIndex || originalDocument.m_paraItemIndex < orgMatchedParaItemIndex;
		}
		else
		{
			isNeedToMarkInsertion = base.OwnerTextBody.m_bodyItemIndex != Index || base.OwnerTextBody.m_paraItemIndex < revparaItemIndex;
			isNeedToMarkDeletion = orgTextBody.m_bodyItemIndex != orgMatchedBodyItemIndex || orgTextBody.m_paraItemIndex < orgMatchedParaItemIndex;
		}
	}

	private void AddSkippedBookmarks(WordDocument originalDocument, ref Dictionary<int, SkippedBkmkInfo> skippedBookmarks, TextSelection matchedText)
	{
		if (skippedBookmarks == null || matchedText.StartTextRange != matchedText.EndTextRange)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (int key in skippedBookmarks.Keys)
		{
			num2++;
			int num3 = key - num;
			if (num2 == 1)
			{
				num3 += matchedText.m_startCut;
			}
			if (matchedText.m_endCut != -1)
			{
				matchedText.m_endCut -= num3;
			}
			num = key;
			if (num3 < matchedText.EndTextRange.TextLength)
			{
				WTextRange wTextRange = matchedText.EndTextRange.Clone() as WTextRange;
				wTextRange.Text = matchedText.EndTextRange.Text.Remove(num3);
				matchedText.EndTextRange.Text = matchedText.EndTextRange.Text.Substring(num3);
				matchedText.OwnerParagraph.Items.Insert(matchedText.EndTextRange.Index, wTextRange);
				if (skippedBookmarks[key].IsBkmkEnd)
				{
					BookmarkEnd entity = new BookmarkEnd(originalDocument, skippedBookmarks[key].BkmkName);
					matchedText.OwnerParagraph.Items.Insert(matchedText.EndTextRange.Index, entity);
				}
				else
				{
					CheckRevBkmkInOrgDoc(originalDocument, null, skippedBookmarks[key].BkmkName);
					BookmarkStart entity2 = new BookmarkStart(originalDocument, skippedBookmarks[key].BkmkName);
					matchedText.OwnerParagraph.Items.Insert(matchedText.EndTextRange.Index, entity2);
				}
				continue;
			}
			skippedBookmarks.Clear();
			skippedBookmarks = null;
			return;
		}
		skippedBookmarks.Clear();
		skippedBookmarks = null;
	}

	private void CompareNonRenderableItems(WordDocument originalDocument, WTextBody orgTextBody, ref int paraItemIndex, int startRangeIndex)
	{
		WordDocument wordDocument = ((originalDocument != null) ? originalDocument : orgTextBody.Document);
		if (Items[paraItemIndex] is BookmarkStart)
		{
			CheckRevBkmkInOrgDoc(originalDocument, orgTextBody, (Items[paraItemIndex] as BookmarkStart).Name);
		}
		if (Items[paraItemIndex] is BookmarkEnd)
		{
			Bookmark bookmark = wordDocument.Bookmarks.FindByName((Items[paraItemIndex] as BookmarkEnd).Name);
			int num = originalDocument?.m_paraItemIndex ?? orgTextBody.m_paraItemIndex;
			if (bookmark != null && num < bookmark.BookmarkStart.Index)
			{
				int index = bookmark.BookmarkStart.OwnerParagraph.Index;
				int index2 = bookmark.BookmarkEnd.OwnerParagraph.Index;
				if (index == index2)
				{
					wordDocument.Bookmarks.Remove(bookmark);
				}
			}
		}
		if (Items[paraItemIndex] is EditableRangeStart)
		{
			EditableRange editableRange = wordDocument.EditableRanges.FindById((Items[paraItemIndex] as EditableRangeStart).Id);
			if (editableRange != null)
			{
				int index3 = editableRange.EditableRangeStart.OwnerParagraph.Index;
				int index4 = editableRange.EditableRangeEnd.OwnerParagraph.Index;
				wordDocument.EditableRanges.Remove(editableRange);
				ResetParaItemIndex(originalDocument, orgTextBody, index3, index4, editableRange.EditableRangeStart.Index, editableRange.EditableRangeEnd.Index);
			}
		}
		if (originalDocument != null)
		{
			if (base.Document.m_sectionIndex == GetOwnerSection(this).Index && base.Document.m_bodyItemIndex == Index && base.Document.m_paraItemIndex >= startRangeIndex)
			{
				if (originalDocument.m_textEndIndex + 1 > 0)
				{
					SplitOriginalDocumentText(originalDocument);
				}
				WParagraph wParagraph = ((originalDocument.m_matchSectionIndex > -1 && originalDocument.m_matchBodyItemIndex > -1) ? (originalDocument.Sections[originalDocument.m_sectionIndex].Body.Items[originalDocument.m_bodyItemIndex] as WParagraph) : (originalDocument.Sections[0].Body.Items[0] as WParagraph));
				if (wParagraph != null)
				{
					int paraItemIndex2 = originalDocument.m_paraItemIndex;
					wParagraph.Items.Insert(paraItemIndex2, Items[paraItemIndex].Clone());
					MoveCurrPosition(originalDocument, paraItemIndex2, originalDocument.m_bodyItemIndex, wParagraph.Items.Count, base.Document.m_paraItemIndex, Items.Count);
					MoveCurrPosition(base.Document, base.Document.m_paraItemIndex, base.Document.m_bodyItemIndex, Items.Count, paraItemIndex2, wParagraph.Items.Count);
				}
			}
		}
		else if (base.OwnerTextBody.m_bodyItemIndex == Index && base.OwnerTextBody.m_paraItemIndex >= startRangeIndex)
		{
			if (orgTextBody.m_textEndIndex + 1 > 0)
			{
				SplitOriginalTextBodyText(orgTextBody);
			}
			WParagraph wParagraph2 = ((orgTextBody.m_matchBodyItemIndex > -1) ? (orgTextBody.Items[orgTextBody.m_bodyItemIndex] as WParagraph) : (orgTextBody.Items[0] as WParagraph));
			if (wParagraph2 != null)
			{
				int paraItemIndex3 = orgTextBody.m_paraItemIndex;
				wParagraph2.Items.Insert(paraItemIndex3, Items[paraItemIndex].Clone());
				MoveCurrPosition(null, paraItemIndex3, orgTextBody.m_bodyItemIndex, wParagraph2.Items.Count, base.OwnerTextBody.m_paraItemIndex, Items.Count, orgTextBody);
				MoveCurrPosition(null, base.OwnerTextBody.m_paraItemIndex, base.OwnerTextBody.m_bodyItemIndex, Items.Count, paraItemIndex3, wParagraph2.Items.Count, base.OwnerTextBody);
				base.OwnerTextBody.currParaItemIndex++;
			}
		}
		paraItemIndex++;
	}

	private void CheckRevBkmkInOrgDoc(WordDocument originalDocument, WTextBody textBody, string bkmkName)
	{
		WordDocument wordDocument = ((originalDocument != null) ? originalDocument : textBody.Document);
		Bookmark bookmark = wordDocument.Bookmarks.FindByName(bkmkName);
		if (bookmark != null && bookmark.BookmarkStart.OwnerParagraph == bookmark.BookmarkEnd.OwnerParagraph)
		{
			int index = bookmark.BookmarkStart.OwnerParagraph.Index;
			int index2 = bookmark.BookmarkEnd.OwnerParagraph.Index;
			wordDocument.Bookmarks.Remove(bookmark);
			ResetParaItemIndex(originalDocument, textBody, index, index2, bookmark.BookmarkStart.Index, bookmark.BookmarkEnd.Index);
		}
	}

	private void ResetParaItemIndex(WordDocument originalDocument, WTextBody orgTextBody, int startOwnerParaIndex, int endOwnerParaIndex, int startIndex, int endIndex)
	{
		if (originalDocument != null)
		{
			if (originalDocument.m_bodyItemIndex == startOwnerParaIndex && originalDocument.m_paraItemIndex > startIndex)
			{
				originalDocument.m_paraItemIndex--;
			}
			if (originalDocument.m_bodyItemIndex == endOwnerParaIndex && originalDocument.m_paraItemIndex > endIndex)
			{
				originalDocument.m_paraItemIndex--;
			}
			if (startOwnerParaIndex == originalDocument.m_matchBodyItemIndex && originalDocument.m_matchParaItemIndex > startIndex)
			{
				originalDocument.m_matchParaItemIndex--;
			}
			if (endOwnerParaIndex == originalDocument.m_matchBodyItemIndex && originalDocument.m_matchParaItemIndex > endIndex)
			{
				originalDocument.m_matchParaItemIndex--;
			}
		}
		else
		{
			if (orgTextBody.m_bodyItemIndex == startOwnerParaIndex && orgTextBody.m_paraItemIndex > startIndex)
			{
				orgTextBody.m_paraItemIndex--;
			}
			if (orgTextBody.m_bodyItemIndex == endOwnerParaIndex && orgTextBody.m_paraItemIndex > endIndex)
			{
				orgTextBody.m_paraItemIndex--;
			}
			if (startOwnerParaIndex == orgTextBody.m_matchBodyItemIndex && orgTextBody.m_matchParaItemIndex > startIndex)
			{
				orgTextBody.m_matchParaItemIndex--;
			}
			if (endOwnerParaIndex == orgTextBody.m_matchBodyItemIndex && orgTextBody.m_matchParaItemIndex > endIndex)
			{
				orgTextBody.m_matchParaItemIndex--;
			}
		}
	}

	private void SplitBothOrgAndRevText(ref int textStartIndex, ref int startRangeIndex, ref int paraItemIndex, ref int textIndex, ref int textEndIndex, string wordToMatch, WordDocument originalDocument, WTextBody orgTextBody)
	{
		SplitRevisedDocumentText(ref textStartIndex, ref startRangeIndex, ref paraItemIndex, ref textIndex, ref textEndIndex, wordToMatch, orgTextBody != null);
		if (orgTextBody == null)
		{
			SplitOriginalDocumentText(originalDocument);
		}
		else
		{
			SplitOriginalTextBodyText(orgTextBody);
		}
	}

	private void SplitTextRanges(WordDocument originalDocument, TextSelection matchedText, bool isLastWordMatched, ref int textStartIndex, ref int startRangeIndex, ref int paraItemIndex, ref int textIndex, ref int textEndIndex, string wordToMatch, WTextBody orgTextBody = null)
	{
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		int num4 = -1;
		bool flag = false;
		WTextBody wTextBody = null;
		if (orgTextBody == null)
		{
			flag = originalDocument.m_sectionIndex != matchedText.OwnerParagraph.GetOwnerSection().Index;
			num = originalDocument.m_paraItemIndex;
			num2 = originalDocument.m_bodyItemIndex;
			num3 = originalDocument.m_textEndIndex;
			num4 = originalDocument.m_textStartIndex;
			wTextBody = originalDocument.Sections[originalDocument.m_sectionIndex].Body;
		}
		else
		{
			num = orgTextBody.m_paraItemIndex;
			num2 = orgTextBody.m_bodyItemIndex;
			num3 = orgTextBody.m_textEndIndex;
			num4 = orgTextBody.m_textStartIndex;
			wTextBody = orgTextBody;
		}
		if (isLastWordMatched)
		{
			if (!flag && num2 == matchedText.OwnerParagraph.Index && !((num != matchedText.StartTextRange.Index) ? (matchedText.m_startCut > 0) : (num3 < matchedText.m_startCut - 1)))
			{
				return;
			}
			if (matchedText.m_startCut > 0 && matchedText.m_startCut < matchedText.StartTextRange.TextLength)
			{
				matchedText.GetRanges();
			}
			_ = wTextBody.Items[num2];
			if ((num4 > 0 && num3 > -1) || num3 == num4)
			{
				if (orgTextBody == null)
				{
					SplitOriginalDocumentText(originalDocument);
				}
				else
				{
					SplitOriginalTextBodyText(orgTextBody);
				}
			}
			if (textStartIndex > 0)
			{
				SplitRevisedDocumentText(ref textStartIndex, ref startRangeIndex, ref paraItemIndex, ref textIndex, ref textEndIndex, wordToMatch, orgTextBody != null);
			}
		}
		else
		{
			if (matchedText.m_startCut > 0 && matchedText.m_startCut < matchedText.StartTextRange.TextLength)
			{
				matchedText.GetRanges();
			}
			if (textStartIndex > 0)
			{
				SplitText(ref textStartIndex, ref textEndIndex, ref paraItemIndex, ref startRangeIndex, ref textIndex);
			}
		}
	}

	private void InsertionAndDeletion(WordDocument originalDocument, bool isNeedToMarkInsertion, bool isNeedToMarkDeletion, TextSelection matchedText, int startRangeIndex)
	{
		if (isNeedToMarkDeletion)
		{
			AddDeleteRevisonMark(matchedText, originalDocument, ref isNeedToMarkInsertion);
		}
		if (isNeedToMarkInsertion)
		{
			InsertParagraphItems(matchedText, originalDocument, startRangeIndex);
		}
	}

	private void CompareEmptyParagraphs(WordDocument orgDoc, WTextBody orgTextBody = null)
	{
		if (orgTextBody == null)
		{
			if (orgDoc.m_sectionIndex < orgDoc.Sections.Count && orgDoc.m_bodyItemIndex != orgDoc.Sections[orgDoc.m_sectionIndex].Body.ChildEntities.Count)
			{
				WParagraph wParagraph = orgDoc.Sections[orgDoc.m_sectionIndex].Body.Items[orgDoc.m_bodyItemIndex] as WParagraph;
				if (orgDoc.m_paraItemIndex == 0 && wParagraph != null && (wParagraph.Items.Count == 0 || wParagraph.IsEmptyParagraph()) && base.Document.m_bodyItemIndex == Index && base.Document.m_sectionIndex == GetOwnerSection().Index)
				{
					wParagraph.CompareParagraphFormats(this);
					orgDoc.UpdateMatchIndex();
					orgDoc.UpdateIndex(wParagraph.Index + 1, 0, 0, 0, 0);
					base.Document.UpdateIndex(Index + 1, 0, 0, 0, 0);
				}
			}
		}
		else if (orgTextBody.m_bodyItemIndex < orgTextBody.ChildEntities.Count)
		{
			WParagraph wParagraph2 = orgTextBody.Items[orgTextBody.m_bodyItemIndex] as WParagraph;
			if (orgTextBody.m_paraItemIndex == 0 && wParagraph2 != null && (wParagraph2.Items.Count == 0 || wParagraph2.IsEmptyParagraph()) && base.OwnerTextBody.m_bodyItemIndex == Index)
			{
				wParagraph2.CompareParagraphFormats(this);
				orgTextBody.UpdateMatchIndex();
				orgTextBody.UpdateIndex(wParagraph2.Index + 1, 0, 0, 0, 0);
				base.OwnerTextBody.UpdateIndex(Index + 1, 0, 0, 0, 0);
			}
		}
	}

	private void ComparePreviousEmptyParagraphs(WordDocument orgDoc, WTextBody orgTextBody = null)
	{
		Entity entity = base.PreviousSibling as Entity;
		int num = orgTextBody?.m_bodyItemIndex ?? orgDoc.m_bodyItemIndex;
		while (entity != null && entity is WParagraph)
		{
			WParagraph wParagraph = entity as WParagraph;
			if (wParagraph.Items.Count != 0 && !wParagraph.IsEmptyParagraph())
			{
				break;
			}
			bool num2;
			if (orgTextBody != null)
			{
				if (orgTextBody.m_paraItemIndex == 0 && orgTextBody.m_textStartIndex == 0)
				{
					num2 = orgTextBody.m_textEndIndex == -1;
					goto IL_007f;
				}
			}
			else if (orgDoc.m_paraItemIndex == 0 && orgDoc.m_textStartIndex == 0)
			{
				num2 = orgDoc.m_textEndIndex == -1;
				goto IL_007f;
			}
			goto IL_0085;
			IL_007f:
			if (num2)
			{
				num--;
			}
			goto IL_0085;
			IL_0085:
			WTextBody wTextBody = ((orgTextBody == null) ? orgDoc.Sections[orgDoc.m_sectionIndex].Body : orgTextBody);
			if (wTextBody.Items[num].PreviousSibling is Entity entity2 && entity2 is WParagraph)
			{
				WParagraph wParagraph2 = entity2 as WParagraph;
				if ((wParagraph2.Items.Count == 0 || wParagraph2.IsEmptyParagraph()) && wParagraph2.BreakCharacterFormat.IsInsertRevision)
				{
					for (TextBodyItem textBodyItem = wParagraph2.PreviousSibling as TextBodyItem; textBodyItem != null; textBodyItem = textBodyItem.PreviousSibling as TextBodyItem)
					{
						if (textBodyItem is WParagraph && (textBodyItem as WParagraph).BreakCharacterFormat.IsDeleteRevision)
						{
							if ((textBodyItem as WParagraph).Items.Count == 0 || (textBodyItem as WParagraph).IsParaHasOnlyBookmark())
							{
								WParagraph revisedParagraph = textBodyItem as WParagraph;
								wParagraph2.CompareParagraphFormats(revisedParagraph);
								wTextBody.Document.IsComparing = true;
								wTextBody.Items.RemoveAt(textBodyItem.Index);
								wTextBody.Document.IsComparing = false;
								wParagraph2.RemoveEntityRevision(isNeedToRemoveFormatRev: false);
								wParagraph2.BreakCharacterFormat.IsInsertRevision = false;
								if (orgTextBody == null)
								{
									orgDoc.m_bodyItemIndex--;
									orgDoc.m_matchBodyItemIndex--;
								}
								else
								{
									orgTextBody.m_bodyItemIndex--;
									orgTextBody.m_matchBodyItemIndex--;
								}
								num--;
							}
							break;
						}
					}
				}
			}
			entity = entity.PreviousSibling as Entity;
		}
	}

	private void CompareWithCurrentItemOnly(WordDocument originalDocument, ref int paraItemIndex, ref int textStartIndex, ref int textEndIndex, ref int textIndex, ref bool isLastWordMatched, ref int startRangeIndex, WTextBody originalTextBody = null)
	{
		Comparison comparison = ((originalTextBody != null) ? originalTextBody.Document.Comparison : originalDocument.Comparison);
		ParagraphItem paragraphItem = ((originalTextBody != null) ? GetCurrParaItem(null, originalTextBody) : GetCurrParaItem(originalDocument));
		ParagraphItem paragraphItem2 = Items[paraItemIndex];
		bool flag = false;
		if (paragraphItem != null && paragraphItem2 != null && paragraphItem.EntityType == paragraphItem2.EntityType)
		{
			switch (paragraphItem2.EntityType)
			{
			case EntityType.Break:
			{
				Break @break = paragraphItem as Break;
				Break break2 = paragraphItem2 as Break;
				if (comparison.IsComparingMatchedCells || @break.Compare(break2))
				{
					flag = true;
					MarkRevForParagraphItem(originalDocument, @break, break2, isMatchedItem: true, startRangeIndex, paraItemIndex, originalTextBody);
				}
				break;
			}
			case EntityType.Symbol:
			{
				WSymbol wSymbol = paragraphItem as WSymbol;
				WSymbol wSymbol2 = paragraphItem2 as WSymbol;
				if (comparison.IsComparingMatchedCells || wSymbol.Compare(wSymbol2))
				{
					flag = true;
					MarkRevForParagraphItem(originalDocument, wSymbol, wSymbol2, isMatchedItem: true, startRangeIndex, paraItemIndex, originalTextBody);
				}
				break;
			}
			case EntityType.Footnote:
			{
				WFootnote wFootnote = paragraphItem as WFootnote;
				WFootnote wFootnote2 = paragraphItem2 as WFootnote;
				wFootnote2.TextBody.Compare(wFootnote.TextBody);
				flag = true;
				MarkRevForParagraphItem(originalDocument, wFootnote, wFootnote2, isMatchedItem: true, startRangeIndex, paraItemIndex, originalTextBody);
				break;
			}
			}
		}
		WTextBody revTextBody = ((originalTextBody != null) ? base.OwnerTextBody : null);
		UpdateNextRevDocIndex(ref paraItemIndex, ref textStartIndex, ref textEndIndex, ref textIndex, ref startRangeIndex, revTextBody);
		isLastWordMatched = flag;
	}

	private ParagraphItem GetCurrParaItem(WordDocument document, WTextBody textBody = null)
	{
		if (textBody != null)
		{
			TextBodyItem textBodyItem = textBody.Items[textBody.m_bodyItemIndex];
			if (textBodyItem is WParagraph)
			{
				WParagraph wParagraph = textBodyItem as WParagraph;
				if (textBody.m_paraItemIndex < wParagraph.Items.Count)
				{
					return wParagraph.Items[textBody.m_paraItemIndex];
				}
			}
			return null;
		}
		TextBodyItem textBodyItem2 = document.Sections[document.m_sectionIndex].Body.Items[document.m_bodyItemIndex];
		if (textBodyItem2 is WParagraph)
		{
			WParagraph wParagraph2 = textBodyItem2 as WParagraph;
			if (document.m_paraItemIndex < wParagraph2.Items.Count)
			{
				return wParagraph2.Items[document.m_paraItemIndex];
			}
		}
		return null;
	}

	private void UpdateNextRevDocIndex(ref int paraItemIndex, ref int textStartIndex, ref int textEndIndex, ref int textIndex, ref int startRangeIndex, WTextBody revTextBody = null)
	{
		WField wField = ((Items[paraItemIndex] is WField) ? (Items[paraItemIndex] as WField) : ((Items[paraItemIndex] is WOleObject) ? (Items[paraItemIndex] as WOleObject).Field : ((Items[paraItemIndex] is TableOfContent) ? (Items[paraItemIndex] as TableOfContent).TOCField : null)));
		WParagraph wParagraph = Items.OwnerBase as WParagraph;
		if (revTextBody != null)
		{
			if (wField != null)
			{
				if (wParagraph == wField.FieldEnd.OwnerParagraph)
				{
					paraItemIndex = wField.FieldEnd.Index + 1;
					revTextBody.currParaItemIndex = paraItemIndex;
				}
				else
				{
					revTextBody.currParaItemIndex = wField.FieldEnd.Index + 1;
					revTextBody.currBodyItemIndex = wField.FieldEnd.OwnerParagraph.Index - 1;
					paraItemIndex = Items.Count;
				}
			}
			else
			{
				paraItemIndex++;
				revTextBody.currParaItemIndex = paraItemIndex;
			}
		}
		else if (wField != null)
		{
			if (wParagraph == wField.FieldEnd.OwnerParagraph)
			{
				paraItemIndex = wField.FieldEnd.Index + 1;
				base.Document.currParaItemIndex = paraItemIndex;
			}
			else
			{
				base.Document.currParaItemIndex = wField.FieldEnd.Index + 1;
				base.Document.currBodyItemIndex = wField.FieldEnd.OwnerParagraph.Index - 1;
				base.Document.currSectionIndex = wField.FieldEnd.OwnerParagraph.GetOwnerSection().Index;
				paraItemIndex = Items.Count;
			}
		}
		else
		{
			paraItemIndex++;
			base.Document.currParaItemIndex = paraItemIndex;
		}
		textStartIndex = 0;
		textEndIndex = -1;
		textIndex = 0;
		startRangeIndex = paraItemIndex;
	}

	private string GetFieldCodeString(WField field)
	{
		string text = null;
		if (field.FieldType == FieldType.FieldFormTextInput || field.FieldType == FieldType.FieldFormDropDown || field.FieldType == FieldType.FieldFormCheckBox)
		{
			text = field.GetAsString(traverseTillSeparator: false).ToString();
			return text + (field as WFormField).GetAsString().ToString();
		}
		return field.GetAsString(traverseTillSeparator: true).ToString();
	}

	private void CompareField(WordDocument orgDoc, ref int revParaItemIndex, ref int textStartIndex, ref int textEndIndex, ref int textIndex, ref bool isLastWordMatched, ref int startRangeIndex, WTextBody originalTextBody = null)
	{
		bool flag = Items[revParaItemIndex] is TableOfContent;
		WField wField = ((!flag) ? (Items[revParaItemIndex] as WField) : (Items[revParaItemIndex] as TableOfContent).TOCField);
		string fieldCodeString = GetFieldCodeString(wField);
		string text = fieldCodeString.Replace("\r\n", "");
		int index = 0;
		Comparison comparison = ((originalTextBody != null) ? originalTextBody.Document.Comparison : orgDoc.Comparison);
		int num = (flag ? comparison.TOCs.Count : comparison.Fields.Count);
		orgDoc = ((originalTextBody != null) ? originalTextBody.Document : orgDoc);
		bool flag2 = false;
		while (index < num)
		{
			WField wField2 = FindFieldInOrgDoc(comparison, ref index, Items[revParaItemIndex]);
			if (wField2 != null)
			{
				string fieldCodeString2 = GetFieldCodeString(wField2);
				if (fieldCodeString2.Replace("\r\n", "") == text)
				{
					ParagraphItem paragraphItem = (flag ? ((ParagraphItem)comparison.TOCs[index]) : ((ParagraphItem)comparison.Fields[index]));
					ParagraphItem revParaItem = Items[revParaItemIndex];
					CompareFieldCodeFormatting(paragraphItem, revParaItem);
					if (originalTextBody == null)
					{
						InsertAndDeleteUnmatchedItems(orgDoc, paragraphItem, startRangeIndex, revParaItemIndex);
					}
					else
					{
						InsertAndDeleteUnmatchedItems(null, paragraphItem, startRangeIndex, revParaItemIndex, originalTextBody);
					}
					if (wField.FieldSeparator != null && wField2.FieldSeparator != null)
					{
						if (originalTextBody == null)
						{
							CompareFieldResult(orgDoc, base.Document, null, null, paragraphItem, revParaItem);
						}
						else
						{
							CompareFieldResult(null, null, originalTextBody, base.OwnerTextBody, paragraphItem, revParaItem);
						}
					}
					if (fieldCodeString2 != fieldCodeString)
					{
						MoveFieldItemsToOrg(paragraphItem, revParaItem, orgDoc, base.Document);
					}
					if (originalTextBody == null)
					{
						orgDoc.m_paraItemIndex = wField2.FieldEnd.Index;
						orgDoc.m_bodyItemIndex = wField2.FieldEnd.OwnerParagraph.Index;
						orgDoc.UpdateMatchIndex();
						MoveCurrPosition(orgDoc, orgDoc.m_paraItemIndex, orgDoc.m_bodyItemIndex, wField2.FieldEnd.OwnerParagraph.Items.Count, wField.FieldEnd.Index, wField.FieldEnd.OwnerParagraph.Items.Count);
						MoveCurrPosition(base.Document, wField.FieldEnd.Index, wField.FieldEnd.OwnerParagraph.Index, wField.FieldEnd.OwnerParagraph.Items.Count, wField2.FieldEnd.Index, wField2.FieldEnd.OwnerParagraph.Items.Count);
					}
					else
					{
						originalTextBody.m_paraItemIndex = wField2.FieldEnd.Index;
						originalTextBody.m_bodyItemIndex = wField2.FieldEnd.OwnerParagraph.Index;
						originalTextBody.UpdateMatchIndex();
						MoveCurrPosition(null, originalTextBody.m_paraItemIndex, originalTextBody.m_bodyItemIndex, wField2.FieldEnd.OwnerParagraph.Items.Count, wField.FieldEnd.Index, wField.FieldEnd.OwnerParagraph.Items.Count, originalTextBody);
						MoveCurrPosition(null, wField.FieldEnd.Index, wField.FieldEnd.OwnerParagraph.Index, wField.FieldEnd.OwnerParagraph.Items.Count, wField2.FieldEnd.Index, wField2.FieldEnd.OwnerParagraph.Items.Count, base.OwnerTextBody);
					}
					flag2 = true;
					comparison.RemoveFromDocCollection(paragraphItem);
					break;
				}
				index++;
			}
			else
			{
				index++;
			}
		}
		if (originalTextBody == null)
		{
			UpdateNextRevDocIndex(ref revParaItemIndex, ref textStartIndex, ref textEndIndex, ref textIndex, ref startRangeIndex);
		}
		else
		{
			UpdateNextRevDocIndex(ref revParaItemIndex, ref textStartIndex, ref textEndIndex, ref textIndex, ref startRangeIndex, base.OwnerTextBody);
		}
		isLastWordMatched = flag2;
	}

	private WField FindFieldInOrgDoc(Comparison comparison, ref int index, ParagraphItem revParaItem)
	{
		if (revParaItem is TableOfContent)
		{
			TableOfContent tableOfContent = revParaItem as TableOfContent;
			TableOfContent tableOfContent2 = comparison.TOCs[index];
			if (tableOfContent2 != null && (comparison.IsComparingMatchedCells || tableOfContent2.Compare(tableOfContent)))
			{
				return tableOfContent2.TOCField;
			}
		}
		else
		{
			WField wField = revParaItem as WField;
			List<WField> fields = comparison.Fields;
			while (index < fields.Count)
			{
				WField wField2 = fields[index];
				if (wField2.FieldType == wField.FieldType && (wField2.FieldType == FieldType.FieldFormDropDown || wField2.FieldType == FieldType.FieldFormCheckBox || (wField.FieldSeparator == null && wField2.FieldSeparator == null) || (wField.FieldSeparator != null && wField2.FieldSeparator != null)))
				{
					return wField2;
				}
				index++;
			}
		}
		return null;
	}

	private void CompareFieldCodeFormatting(ParagraphItem orgParaItem, ParagraphItem revParaItem)
	{
		WParagraph ownerParagraph = orgParaItem.OwnerParagraph;
		WParagraph ownerParagraph2 = revParaItem.OwnerParagraph;
		WField wField = ((orgParaItem is TableOfContent) ? (orgParaItem as TableOfContent).TOCField : (orgParaItem as WField));
		WField wField2 = ((revParaItem is TableOfContent) ? (revParaItem as TableOfContent).TOCField : (revParaItem as WField));
		if (wField2.FieldSeparator != null && wField.FieldSeparator != null && ownerParagraph.Index == wField.FieldSeparator.OwnerParagraph.Index && ownerParagraph2.Index == wField2.FieldSeparator.OwnerParagraph.Index)
		{
			ownerParagraph.CompareParagraphFormats(ownerParagraph2);
		}
	}

	private WordDocument GetResultDocPartFromField(WordDocument document, WTextBody textBody, ParagraphItem paraItem)
	{
		WordDocument wordDocument = null;
		if (document != null)
		{
			wordDocument = document;
			document.m_sectionIndex = paraItem.OwnerParagraph.GetOwnerSection().Index;
			document.UpdateIndex(paraItem.OwnerParagraph.Index, paraItem.Index, 0, 0, 0);
		}
		else
		{
			wordDocument = textBody.Document;
			textBody.m_paraItemIndex = paraItem.Index;
			textBody.m_bodyItemIndex = paraItem.OwnerParagraph.Index;
		}
		WField wField = ((paraItem is TableOfContent) ? (paraItem as TableOfContent).TOCField : (paraItem as WField));
		WParagraph ownerParagraph = wField.FieldSeparator.OwnerParagraph;
		BookmarkStart bookmarkStart = new BookmarkStart(wordDocument, "Insertion");
		ownerParagraph.Items.Insert(wField.FieldSeparator.Index + 1, bookmarkStart);
		WParagraph ownerParagraph2 = wField.FieldEnd.OwnerParagraph;
		BookmarkEnd bookmarkEnd = new BookmarkEnd(wordDocument, bookmarkStart.Name);
		ownerParagraph2.Items.Insert(wField.FieldEnd.Index, bookmarkEnd);
		BookmarksNavigator bookmarksNavigator = new BookmarksNavigator(wordDocument);
		bookmarksNavigator.MoveToBookmark(bookmarkStart.Name);
		wordDocument.IsComparing = true;
		WordDocumentPart content = bookmarksNavigator.GetContent();
		wordDocument.IsComparing = false;
		int bkmkEndPreviosItemIndex = bookmarkEnd.GetIndexInOwnerCollection() - 1;
		if (bookmarksNavigator.IsBkmkEndInFirstItem(ownerParagraph2, bookmarkEnd, bkmkEndPreviosItemIndex))
		{
			content.Sections[content.Sections.Count - 1].Body.Items.Add(new WParagraph(wordDocument));
		}
		wordDocument.Bookmarks.Remove(wordDocument.Bookmarks.FindByName("Insertion"));
		return content.GetAsWordDocument();
	}

	private void ClearHeadersAndFooters(WordDocument document)
	{
		foreach (WSection section in document.Sections)
		{
			section.HeadersFooters.OddHeader.ChildEntities.Clear();
			section.HeadersFooters.OddFooter.ChildEntities.Clear();
			section.HeadersFooters.EvenHeader.ChildEntities.Clear();
			section.HeadersFooters.EvenFooter.ChildEntities.Clear();
			section.HeadersFooters.FirstPageHeader.ChildEntities.Clear();
			section.HeadersFooters.FirstPageFooter.ChildEntities.Clear();
		}
	}

	private void CompareFieldResult(WordDocument orgDoc, WordDocument revDoc, WTextBody orgTextBody, WTextBody revTextBody, ParagraphItem orgParaItem, ParagraphItem revParaItem)
	{
		WField wField = ((orgParaItem is TableOfContent) ? (orgParaItem as TableOfContent).TOCField : (orgParaItem as WField));
		if (revParaItem is TableOfContent)
		{
			_ = (revParaItem as TableOfContent).TOCField;
		}
		WordDocument wordDocument = ((orgDoc != null) ? orgDoc : orgTextBody.Document);
		WordDocument resultDocPartFromField = GetResultDocPartFromField(orgDoc, orgTextBody, orgParaItem);
		WordDocument resultDocPartFromField2 = GetResultDocPartFromField(revDoc, revTextBody, revParaItem);
		resultDocPartFromField.Compare(resultDocPartFromField2, base.Document.m_authorName, base.Document.m_dateTime);
		WParagraph ownerParagraph = wField.FieldSeparator.OwnerParagraph;
		BookmarkStart entity = new BookmarkStart(wordDocument, "Insertion");
		ownerParagraph.Items.Insert(wField.FieldSeparator.Index + 1, entity);
		WParagraph ownerParagraph2 = wField.FieldEnd.OwnerParagraph;
		BookmarkEnd entity2 = new BookmarkEnd(wordDocument, "Insertion");
		ownerParagraph2.Items.Insert(wField.FieldEnd.Index, entity2);
		BookmarksNavigator bookmarksNavigator = new BookmarksNavigator(wordDocument);
		bookmarksNavigator.MoveToBookmark("Insertion");
		wordDocument.IsComparing = true;
		resultDocPartFromField.IsComparing = true;
		wordDocument.UpdateRevisionOnComparing = true;
		bookmarksNavigator.ReplaceContent(resultDocPartFromField);
		wordDocument.IsComparing = false;
		resultDocPartFromField.IsComparing = false;
		wordDocument.UpdateRevisionOnComparing = false;
		wordDocument.Bookmarks.Remove(wordDocument.Bookmarks.FindByName("Insertion"));
		resultDocPartFromField.Close();
		resultDocPartFromField2.Close();
	}

	private void CompareInlineCCItems(WordDocument orgDoc, WordDocument revDoc, InlineContentControl orgInlineCC, InlineContentControl revInlineCC)
	{
		WordDocument docFromInlineCC = GetDocFromInlineCC(orgDoc, orgInlineCC);
		WordDocument docFromInlineCC2 = GetDocFromInlineCC(revDoc, revInlineCC);
		docFromInlineCC.Compare(docFromInlineCC2, base.Document.m_authorName, base.Document.m_dateTime);
		orgInlineCC.ParagraphItems.Clear();
		orgDoc.IsComparing = true;
		docFromInlineCC.IsComparing = true;
		orgDoc.UpdateRevisionOnComparing = true;
		docFromInlineCC.LastParagraph.Items.MoveParaItems(orgInlineCC.ParagraphItems, isRemoveLastInlineCC: true);
		orgDoc.IsComparing = false;
		docFromInlineCC.IsComparing = false;
		orgDoc.UpdateRevisionOnComparing = false;
		docFromInlineCC.Close();
		docFromInlineCC2.Close();
	}

	private WordDocument GetDocFromInlineCC(WordDocument document, InlineContentControl inlineCC)
	{
		WordDocument wordDocument = new WordDocument();
		wordDocument.EnsureMinimal();
		if (document.DefCharFormat != null)
		{
			wordDocument.DefCharFormat = new WCharacterFormat(wordDocument);
			wordDocument.DefCharFormat.ImportContainer(document.DefCharFormat);
		}
		foreach (ParagraphItem paragraphItem in inlineCC.ParagraphItems)
		{
			wordDocument.LastParagraph.ChildEntities.Add(paragraphItem.Clone());
		}
		return wordDocument;
	}

	private void MoveFieldItemsToOrg(ParagraphItem orgParaItem, ParagraphItem revParaItem, WordDocument orgDoc, WordDocument revDoc)
	{
		WField wField = ((orgParaItem is TableOfContent) ? (orgParaItem as TableOfContent).TOCField : (orgParaItem as WField));
		WField wField2 = ((revParaItem is TableOfContent) ? (revParaItem as TableOfContent).TOCField : (revParaItem as WField));
		WParagraph ownerParagraph = revParaItem.OwnerParagraph;
		BookmarkStart entity = new BookmarkStart(revDoc, "MoveFieldItems");
		ownerParagraph.Items.Insert(revParaItem.Index + 1, entity);
		WParagraph ownerParagraph2 = wField2.FieldSeparator.OwnerParagraph;
		BookmarkEnd entity2 = new BookmarkEnd(revDoc, "MoveFieldItems");
		ownerParagraph2.Items.Insert(wField2.FieldSeparator.Index, entity2);
		BookmarksNavigator bookmarksNavigator = new BookmarksNavigator(revDoc);
		bookmarksNavigator.MoveToBookmark("MoveFieldItems");
		revDoc.IsFieldRangeAdding = true;
		revDoc.IsComparing = true;
		WordDocumentPart content = bookmarksNavigator.GetContent();
		revDoc.IsComparing = false;
		revDoc.IsFieldRangeAdding = false;
		revDoc.Bookmarks.Remove(revDoc.Bookmarks.FindByName("MoveFieldItems"));
		orgDoc.IsSkipFieldDetach = true;
		RemoveFieldBeginToFieldSep(orgParaItem);
		orgDoc.IsSkipFieldDetach = false;
		WParagraph ownerParagraph3 = orgParaItem.OwnerParagraph;
		BookmarkStart bookmarkStart = new BookmarkStart(orgDoc, "MoveFieldItems");
		ownerParagraph3.Items.Insert(orgParaItem.Index + 1, bookmarkStart);
		WParagraph ownerParagraph4 = wField.FieldSeparator.OwnerParagraph;
		BookmarkEnd entity3 = new BookmarkEnd(orgDoc, bookmarkStart.Name);
		ownerParagraph4.Items.Insert(wField.FieldSeparator.Index, entity3);
		BookmarksNavigator bookmarksNavigator2 = new BookmarksNavigator(orgDoc);
		bookmarksNavigator2.MoveToBookmark("MoveFieldItems");
		orgDoc.IsFieldRangeAdding = true;
		bookmarksNavigator2.ReplaceContent(content);
		orgDoc.IsFieldRangeAdding = false;
		orgDoc.Bookmarks.Remove(orgDoc.Bookmarks.FindByName("MoveFieldItems"));
	}

	private void RemoveFieldBeginToFieldSep(ParagraphItem orgParaItem)
	{
		WField wField = ((orgParaItem is TableOfContent) ? (orgParaItem as TableOfContent).TOCField : (orgParaItem as WField));
		WParagraph ownerParagraph = orgParaItem.OwnerParagraph;
		WParagraph ownerParagraph2 = wField.FieldSeparator.OwnerParagraph;
		int num = orgParaItem.Index + 1;
		int num2 = wField.FieldSeparator.Index - 1;
		if (ownerParagraph == ownerParagraph2)
		{
			for (int num3 = num2; num3 >= num; num3--)
			{
				ownerParagraph.Items.RemoveAt(num3);
			}
		}
		else
		{
			if (ownerParagraph.OwnerTextBody != ownerParagraph2.OwnerTextBody)
			{
				return;
			}
			for (int num4 = wField.FieldSeparator.OwnerParagraph.Index; num4 >= wField.OwnerParagraph.Index; num4--)
			{
				if (num4 == wField.FieldSeparator.OwnerParagraph.Index)
				{
					int num5;
					for (num5 = 0; num5 <= wField.FieldSeparator.Index - 1; num5++)
					{
						wField.FieldSeparator.OwnerParagraph.Items.RemoveAt(num5);
						num5--;
					}
				}
				else if (num4 == ownerParagraph.Index)
				{
					for (int num6 = ownerParagraph.Items.Count - 1; num6 > orgParaItem.Index; num6--)
					{
						ownerParagraph.Items.RemoveAt(num6);
					}
				}
				else
				{
					ownerParagraph.OwnerTextBody.Items.RemoveAt(num4);
				}
			}
		}
	}

	private void CompareContainerElements(WordDocument orgDoc, ref int revParaItemIndex, ref int textStartIndex, ref int textEndIndex, ref int textIndex, ref bool isLastWordMatched, ref int startRangeIndex, WTextBody originalTextbody = null)
	{
		Comparison comparison = ((originalTextbody != null) ? originalTextbody.Document.Comparison : orgDoc.Comparison);
		bool flag = false;
		switch (Items[revParaItemIndex].EntityType)
		{
		case EntityType.Picture:
		{
			if (comparison.Pictures.Count <= 0)
			{
				break;
			}
			WPicture wPicture = comparison.Pictures[0];
			WPicture wPicture2 = Items[revParaItemIndex] as WPicture;
			if (wPicture != null)
			{
				if (comparison.IsComparingMatchedCells || wPicture.Compare(wPicture2))
				{
					flag = true;
				}
				MarkRevForParagraphItem(orgDoc, wPicture, wPicture2, flag, startRangeIndex, revParaItemIndex, originalTextbody);
			}
			break;
		}
		case EntityType.TextBox:
		{
			if (comparison.TextBoxes.Count <= 0)
			{
				break;
			}
			WTextBox wTextBox = comparison.TextBoxes[0];
			WTextBox wTextBox2 = Items[revParaItemIndex] as WTextBox;
			if (wTextBox != null)
			{
				if (comparison.IsComparingMatchedCells || wTextBox.Compare(wTextBox2))
				{
					wTextBox2.TextBoxBody.Compare(wTextBox.TextBoxBody);
					flag = true;
				}
				MarkRevForParagraphItem(orgDoc, wTextBox, wTextBox2, flag, startRangeIndex, revParaItemIndex, originalTextbody);
			}
			break;
		}
		case EntityType.Shape:
		case EntityType.AutoShape:
		{
			if (comparison.Shapes.Count <= 0)
			{
				break;
			}
			Shape shape = comparison.Shapes[0];
			Shape shape2 = Items[revParaItemIndex] as Shape;
			if (shape != null)
			{
				if (comparison.IsComparingMatchedCells || shape.Compare(shape2))
				{
					shape2.TextBody.Compare(shape.TextBody);
					flag = true;
				}
				MarkRevForParagraphItem(orgDoc, shape, shape2, flag, startRangeIndex, revParaItemIndex, originalTextbody);
			}
			break;
		}
		case EntityType.GroupShape:
		{
			if (comparison.GroupShapes.Count <= 0)
			{
				break;
			}
			GroupShape groupShape = comparison.GroupShapes[0];
			GroupShape groupShape2 = Items[revParaItemIndex] as GroupShape;
			if (groupShape != null)
			{
				if (comparison.IsComparingMatchedCells || groupShape.Compare(groupShape2))
				{
					flag = true;
				}
				MarkRevForParagraphItem(orgDoc, groupShape, groupShape2, flag, startRangeIndex, revParaItemIndex, originalTextbody);
			}
			break;
		}
		case EntityType.Chart:
		{
			if (comparison.Charts.Count <= 0)
			{
				break;
			}
			WChart wChart = comparison.Charts[0];
			WChart wChart2 = Items[revParaItemIndex] as WChart;
			if (wChart != null)
			{
				if (comparison.IsComparingMatchedCells || wChart.Compare(wChart2))
				{
					flag = true;
				}
				MarkRevForParagraphItem(orgDoc, wChart, wChart2, flag, startRangeIndex, revParaItemIndex, originalTextbody);
			}
			break;
		}
		case EntityType.InlineContentControl:
		{
			for (int i = 0; comparison.InlineContentControls.Count > i; i++)
			{
				InlineContentControl inlineContentControl = comparison.InlineContentControls[i];
				InlineContentControl inlineContentControl2 = Items[revParaItemIndex] as InlineContentControl;
				if (comparison.IsComparingMatchedCells || (inlineContentControl != null && inlineContentControl.ContentControlProperties.Compare(inlineContentControl2.ContentControlProperties) && inlineContentControl.ComparisonText == inlineContentControl2.ComparisonText))
				{
					if (originalTextbody == null)
					{
						CompareInlineCCItems(orgDoc, base.Document, inlineContentControl, inlineContentControl2);
					}
					else
					{
						CompareInlineCCItems(originalTextbody.Document, base.OwnerTextBody.Document, inlineContentControl, inlineContentControl2);
					}
					flag = true;
					MarkRevForParagraphItem(orgDoc, inlineContentControl, inlineContentControl2, flag, startRangeIndex, revParaItemIndex, originalTextbody);
					break;
				}
			}
			break;
		}
		case EntityType.Math:
		{
			for (int i = 0; comparison.Maths.Count > i; i++)
			{
				WMath wMath = comparison.Maths[i];
				WMath wMath2 = Items[revParaItemIndex] as WMath;
				if (comparison.IsComparingMatchedCells || (wMath != null && wMath.Compare(wMath2)))
				{
					flag = true;
					MarkRevForParagraphItem(orgDoc, wMath, wMath2, isMatchedItem: true, startRangeIndex, revParaItemIndex, originalTextbody);
					break;
				}
			}
			break;
		}
		}
		WTextBody revTextBody = ((originalTextbody != null) ? base.OwnerTextBody : null);
		UpdateNextRevDocIndex(ref revParaItemIndex, ref textStartIndex, ref textEndIndex, ref textIndex, ref startRangeIndex, revTextBody);
		isLastWordMatched = flag;
	}

	private void MarkRevForParagraphItem(WordDocument orgDoc, ParagraphItem orgParaItem, ParagraphItem revItem, bool isMatchedItem, int startRangeIndex, int revParaItemIndex, WTextBody originalTextBody = null)
	{
		WParagraph ownerParagraph = orgParaItem.OwnerParagraph;
		int index = ownerParagraph.Index;
		int index2 = orgParaItem.Index;
		_ = ownerParagraph.Items.Count;
		Comparison comparison = ((originalTextBody == null) ? orgDoc.Comparison : originalTextBody.Document.Comparison);
		InsertAndDeleteUnmatchedItems(orgDoc, orgParaItem, startRangeIndex, revParaItemIndex, originalTextBody);
		if (originalTextBody == null)
		{
			index2 = (orgDoc.m_paraItemIndex = orgParaItem.Index);
			index = (orgDoc.m_bodyItemIndex = orgParaItem.OwnerParagraph.Index);
			int index3 = ownerParagraph.GetOwnerSection(ownerParagraph).Index;
			index3 = (orgDoc.m_sectionIndex = orgParaItem.OwnerParagraph.GetOwnerSection().Index);
			if (isMatchedItem)
			{
				orgDoc.UpdateMatchIndex();
			}
			orgDoc.UpdateIndex(index, index2 + 1, 0, 0, 0);
			if (isMatchedItem)
			{
				base.Document.m_sectionIndex = revItem.GetOwnerSection(revItem).Index;
				MoveCurrPosition(orgDoc, index2, index, orgParaItem.OwnerParagraph.Items.Count, revParaItemIndex, revItem.OwnerParagraph.Items.Count);
				MoveCurrPosition(base.Document, revParaItemIndex, revItem.OwnerParagraph.Index, revItem.OwnerParagraph.Items.Count, index2, orgParaItem.OwnerParagraph.Items.Count);
				comparison.RemoveFromDocCollection(orgParaItem);
			}
			else
			{
				ApplyDelRevision(orgParaItem);
				base.Document.m_paraItemIndex = revParaItemIndex;
			}
		}
		else
		{
			index2 = (originalTextBody.m_paraItemIndex = orgParaItem.Index);
			index = (originalTextBody.m_bodyItemIndex = orgParaItem.OwnerParagraph.Index);
			if (isMatchedItem)
			{
				originalTextBody.UpdateMatchIndex();
			}
			originalTextBody.UpdateIndex(index, index2 + 1, 0, 0, 0);
			if (isMatchedItem)
			{
				MoveCurrPosition(null, index2, index, orgParaItem.OwnerParagraph.Items.Count, revParaItemIndex, revItem.OwnerParagraph.Items.Count, originalTextBody);
				MoveCurrPosition(null, revParaItemIndex, revItem.OwnerParagraph.Index, revItem.OwnerParagraph.Items.Count, index2, orgParaItem.OwnerParagraph.Items.Count, base.OwnerTextBody);
				comparison.RemoveFromDocCollection(orgParaItem);
			}
			else
			{
				ApplyDelRevision(orgParaItem);
				base.OwnerTextBody.m_paraItemIndex = revParaItemIndex;
			}
		}
	}

	private void CompareOleObject(WordDocument orgDoc, ref int revParaItemIndex, ref bool isLastWordMatched, ref int startRangeIndex, ref int textStartIndex, ref int textEndIndex, ref int textIndex, WTextBody originalTextBody = null)
	{
		Comparison comparison = ((originalTextBody != null) ? originalTextBody.Document.Comparison : orgDoc.Comparison);
		orgDoc = ((originalTextBody != null) ? originalTextBody.Document : orgDoc);
		bool flag = false;
		if (Items[revParaItemIndex] is WOleObject && comparison.OLEs.Count > 0)
		{
			WOleObject wOleObject = comparison.OLEs[0];
			WOleObject wOleObject2 = Items[revParaItemIndex] as WOleObject;
			if (wOleObject != null)
			{
				if (comparison.IsComparingMatchedCells || wOleObject.Compare(wOleObject2))
				{
					flag = true;
				}
				MarkRevForOle(orgDoc, originalTextBody, wOleObject, wOleObject.Field, wOleObject2.Field, startRangeIndex, revParaItemIndex, flag);
			}
		}
		WTextBody revTextBody = ((originalTextBody != null) ? base.OwnerTextBody : null);
		UpdateNextRevDocIndex(ref revParaItemIndex, ref textStartIndex, ref textEndIndex, ref textIndex, ref startRangeIndex, revTextBody);
		isLastWordMatched = flag;
	}

	private void MarkRevForOle(WordDocument orgDoc, WTextBody originalTextBody, ParagraphItem orgParaItem, WField orgItemField, WField revItemField, int startRangeIndex, int revParaItemIndex, bool isMatchedItem)
	{
		WParagraph ownerParagraph = orgParaItem.OwnerParagraph;
		int index = ownerParagraph.Index;
		int index2 = orgParaItem.Index;
		_ = ownerParagraph.Items.Count;
		InsertAndDeleteUnmatchedItems(orgDoc, orgParaItem, startRangeIndex, revParaItemIndex, originalTextBody);
		if (originalTextBody == null)
		{
			index2 = (orgDoc.m_paraItemIndex = orgItemField.FieldEnd.Index);
			index = (orgDoc.m_bodyItemIndex = orgItemField.FieldEnd.OwnerParagraph.Index);
			int index3 = ownerParagraph.GetOwnerSection(ownerParagraph).Index;
			index3 = (orgDoc.m_sectionIndex = orgItemField.FieldEnd.OwnerParagraph.GetOwnerSection().Index);
			if (isMatchedItem)
			{
				orgDoc.UpdateMatchIndex();
			}
			orgDoc.UpdateIndex(index, index2 + 1, 0, 0, 0);
			if (isMatchedItem)
			{
				base.Document.m_sectionIndex = revItemField.FieldEnd.GetOwnerSection(revItemField.FieldEnd).Index;
				MoveCurrPosition(orgDoc, index2, index, orgItemField.FieldEnd.OwnerParagraph.Items.Count, revParaItemIndex, revItemField.FieldEnd.OwnerParagraph.Items.Count);
				MoveCurrPosition(base.Document, revParaItemIndex, revItemField.FieldEnd.OwnerParagraph.Index, revItemField.FieldEnd.OwnerParagraph.Items.Count, index2, orgItemField.FieldEnd.OwnerParagraph.Items.Count);
				orgDoc.Comparison.RemoveFromDocCollection(orgParaItem);
			}
			else
			{
				ApplyDelRevision(orgParaItem);
				base.Document.m_paraItemIndex = revParaItemIndex;
			}
		}
		else
		{
			index2 = (originalTextBody.m_paraItemIndex = orgItemField.FieldEnd.Index);
			index = (originalTextBody.m_bodyItemIndex = orgItemField.FieldEnd.OwnerParagraph.Index);
			if (isMatchedItem)
			{
				originalTextBody.UpdateMatchIndex();
			}
			originalTextBody.UpdateIndex(index, index2 + 1, 0, 0, 0);
			if (isMatchedItem)
			{
				MoveCurrPosition(null, index2, index, orgItemField.FieldEnd.OwnerParagraph.Items.Count, revItemField.FieldEnd.Index, revItemField.FieldEnd.OwnerParagraph.Items.Count, originalTextBody);
				MoveCurrPosition(null, revItemField.FieldEnd.Index, revItemField.FieldEnd.OwnerParagraph.Index, revItemField.FieldEnd.OwnerParagraph.Items.Count, index2, orgItemField.FieldEnd.OwnerParagraph.Items.Count, base.OwnerTextBody);
				originalTextBody.Document.Comparison.RemoveFromDocCollection(orgParaItem);
			}
			else
			{
				ApplyDelRevision(orgParaItem);
				base.OwnerTextBody.m_paraItemIndex = revParaItemIndex;
			}
		}
	}

	private void InsertAndDeleteUnmatchedItems(WordDocument orgDoc, ParagraphItem paraItem, int startRangeIndex, int revParaItemIndex, WTextBody originalTextbody = null)
	{
		WordDocument revisedDocument = ((orgDoc != null) ? base.Document : null);
		WParagraph ownerParagraph = paraItem.OwnerParagraph;
		int num = ((orgDoc != null) ? ownerParagraph.GetOwnerSection(ownerParagraph).Index : (-1));
		int index = ownerParagraph.Index;
		int index2 = paraItem.Index;
		bool isNeedToMarkInsertion = false;
		bool isNeedToMarkDeletion = false;
		IsNeedToMarkInsDel(orgDoc, ref isNeedToMarkInsertion, ref isNeedToMarkDeletion, startRangeIndex, num, index, index2, originalTextbody);
		if (isNeedToMarkDeletion)
		{
			AddDeleteRevisionMark(orgDoc, revisedDocument, num, index, index2, ref isNeedToMarkInsertion, originalTextbody);
		}
		if (isNeedToMarkInsertion)
		{
			Insertion(orgDoc, revParaItemIndex, index2, index, num, originalTextbody);
		}
	}

	private void MoveCurrPosition(WordDocument document, int doc1ParaItemIndex, int doc1BodyItemIndex, int doc1MatchParaItemCount, int doc2ParaItemIndex, int doc2MatchParaItemCount, WTextBody textBody = null)
	{
		if (textBody == null)
		{
			if (doc2ParaItemIndex + 1 >= doc2MatchParaItemCount && doc1ParaItemIndex + 1 >= doc1MatchParaItemCount)
			{
				document.UpdateIndex(doc1BodyItemIndex + 1, 0, 0, 0, 0);
				return;
			}
			doc1ParaItemIndex++;
			document.UpdateIndex(doc1BodyItemIndex, doc1ParaItemIndex, doc1ParaItemIndex, doc1ParaItemIndex, 0);
		}
		else if (doc2ParaItemIndex + 1 >= doc2MatchParaItemCount && doc1ParaItemIndex + 1 >= doc1MatchParaItemCount)
		{
			textBody.UpdateIndex(doc1BodyItemIndex + 1, 0, 0, 0, 0);
		}
		else
		{
			doc1ParaItemIndex++;
			textBody.UpdateIndex(doc1BodyItemIndex, doc1ParaItemIndex, doc1ParaItemIndex, doc1ParaItemIndex, 0);
		}
	}

	private void SplitRevisedDocumentText(ref int textStartIndex, ref int startRangeIndex, ref int paraItemIndex, ref int textIndex, ref int textEndIndex, string wordToMatch, bool isTextBody)
	{
		if (textStartIndex > 0 && (textEndIndex > textStartIndex || !string.IsNullOrEmpty(wordToMatch)))
		{
			WTextRange wTextRange = Items[startRangeIndex] as WTextRange;
			WTextRange wTextRange2 = wTextRange.Clone() as WTextRange;
			wTextRange2.Text = wTextRange2.Text.Remove(textStartIndex);
			wTextRange.Text = wTextRange.Text.Substring(textStartIndex);
			Items.Insert(startRangeIndex, wTextRange2);
			paraItemIndex++;
			startRangeIndex++;
			if (startRangeIndex != paraItemIndex && textEndIndex > -1)
			{
				wTextRange = Items[paraItemIndex] as WTextRange;
				if (wTextRange.Text.Length > textEndIndex + 1)
				{
					wTextRange2 = wTextRange.Clone() as WTextRange;
					wTextRange2.Text = wTextRange2.Text.Remove(textEndIndex + 1);
					wTextRange.Text = wTextRange.Text.Substring(textEndIndex + 1);
					Items.Insert(paraItemIndex, wTextRange2);
					paraItemIndex++;
					textIndex = 0;
					textStartIndex = 0;
					textEndIndex = -1;
				}
			}
			else
			{
				textIndex = 1;
				textEndIndex -= textStartIndex;
				textStartIndex = 0;
			}
			if (isTextBody)
			{
				base.OwnerTextBody.m_paraItemIndex = startRangeIndex;
				base.OwnerTextBody.m_textStartIndex = 0;
				base.OwnerTextBody.m_textEndIndex = -1;
			}
			else
			{
				base.Document.UpdateIndex(base.Document.m_bodyItemIndex, startRangeIndex, base.Document.m_startRangeIndex, base.Document.m_endRangeIndex, 0);
			}
		}
		else if (isTextBody)
		{
			base.OwnerTextBody.m_paraItemIndex = paraItemIndex;
			base.OwnerTextBody.m_textStartIndex = 0;
			base.OwnerTextBody.m_textEndIndex = -1;
		}
		else
		{
			base.Document.UpdateIndex(base.Document.m_bodyItemIndex, paraItemIndex, base.Document.m_startRangeIndex, base.Document.m_endRangeIndex, 0);
		}
	}

	private void SplitOriginalDocumentText(WordDocument originalDocument)
	{
		_ = originalDocument.Sections[originalDocument.m_sectionIndex].Body.Items[originalDocument.m_bodyItemIndex];
		WTextBody body = originalDocument.Sections[originalDocument.m_sectionIndex].Body;
		CopyIndexesFromDoc(originalDocument, body);
		SplitOriginalTextBodyText(originalDocument.Sections[originalDocument.m_sectionIndex].Body);
		CopyIndexesFromTextBody(originalDocument, body);
	}

	private void CopyIndexesFromDoc(WordDocument originalDocument, WTextBody textBody)
	{
		textBody.m_startRangeIndex = originalDocument.m_startRangeIndex;
		textBody.m_endRangeIndex = originalDocument.m_endRangeIndex;
		textBody.m_textStartIndex = originalDocument.m_textStartIndex;
		textBody.m_textEndIndex = originalDocument.m_textEndIndex;
		textBody.m_paraItemIndex = originalDocument.m_paraItemIndex;
		textBody.m_bodyItemIndex = originalDocument.m_bodyItemIndex;
	}

	private void CopyIndexesFromTextBody(WordDocument originalDocument, WTextBody textBody)
	{
		originalDocument.m_startRangeIndex = textBody.m_startRangeIndex;
		originalDocument.m_endRangeIndex = textBody.m_endRangeIndex;
		originalDocument.m_textStartIndex = textBody.m_textStartIndex;
		originalDocument.m_textEndIndex = textBody.m_textEndIndex;
		originalDocument.m_paraItemIndex = textBody.m_paraItemIndex;
		originalDocument.m_bodyItemIndex = textBody.m_bodyItemIndex;
	}

	private void SplitOriginalTextBodyText(WTextBody textBody)
	{
		TextBodyItem textBodyItem = textBody.Items[textBody.m_bodyItemIndex];
		if (!(textBodyItem is WParagraph))
		{
			return;
		}
		WParagraph wParagraph = textBodyItem as WParagraph;
		if (((textBody.m_textStartIndex > 0 && textBody.m_textEndIndex > -1) || textBody.m_textStartIndex == textBody.m_textEndIndex) && textBody.m_paraItemIndex < wParagraph.Items.Count)
		{
			WTextRange wTextRange = wParagraph.Items[textBody.m_paraItemIndex] as WTextRange;
			if (wTextRange.TextLength > textBody.m_textEndIndex)
			{
				WTextRange wTextRange2 = wTextRange.Clone() as WTextRange;
				wTextRange2.Text = wTextRange2.Text.Remove(textBody.m_textEndIndex + 1);
				wTextRange.Text = wTextRange.Text.Substring(textBody.m_textEndIndex + 1);
				wParagraph.Items.Insert(textBody.m_paraItemIndex, wTextRange2);
				textBody.m_paraItemIndex++;
			}
		}
		int paraItemIndex = textBody.m_paraItemIndex;
		textBody.UpdateIndex(textBody.m_bodyItemIndex, paraItemIndex, paraItemIndex, paraItemIndex, 0);
	}

	private void SplitText(ref int textStartIndex, ref int textEndIndex, ref int paraItemIndex, ref int startRangeIndex, ref int textIndex)
	{
		if (textStartIndex <= 0)
		{
			return;
		}
		WTextRange wTextRange = Items[startRangeIndex] as WTextRange;
		WTextRange wTextRange2 = wTextRange.Clone() as WTextRange;
		if (textStartIndex < wTextRange.TextLength)
		{
			wTextRange.Text = wTextRange.Text.Substring(textStartIndex);
			wTextRange2.Text = wTextRange2.Text.Remove(textStartIndex);
			Items.Insert(startRangeIndex, wTextRange2);
			paraItemIndex++;
			startRangeIndex++;
			if (startRangeIndex == paraItemIndex)
			{
				textEndIndex -= textStartIndex;
			}
			textStartIndex = 0;
			textIndex = 1;
		}
	}

	private int FindMatchingWordFromSelection(TextSelection[] matches, int sectionIndex, int bodyItemIndex, int paraItemIndex, int lastMatchedPositionIndex)
	{
		int result = 0;
		for (int i = 0; i < matches.Length; i++)
		{
			TextSelection textSelection = matches[i];
			if (textSelection.StartTextRange == null || textSelection.EndTextRange == null)
			{
				continue;
			}
			int index = textSelection.OwnerParagraph.Index;
			int index2 = textSelection.OwnerParagraph.GetOwnerSection(textSelection.OwnerParagraph).Index;
			int index3 = textSelection.StartTextRange.Index;
			if (sectionIndex > index2 || (sectionIndex == index2 && (bodyItemIndex > index || (bodyItemIndex == index && paraItemIndex > index3))))
			{
				result = -1;
				continue;
			}
			if (sectionIndex == index2 && bodyItemIndex == index && index3 == paraItemIndex && textSelection.m_startCut <= lastMatchedPositionIndex && textSelection.m_endCut <= lastMatchedPositionIndex)
			{
				result = -1;
				continue;
			}
			return i;
		}
		return result;
	}

	private void InsertParagraphItems(TextSelection matchedText, WordDocument originalDocument, int revParaItemIndex)
	{
		int index = matchedText.StartTextRange.Index;
		int index2 = matchedText.OwnerParagraph.Index;
		int orgCurrMatchSecIndex = ((originalDocument != null) ? matchedText.OwnerParagraph.GetOwnerSection(matchedText.OwnerParagraph).Index : (-1));
		WTextBody orgTextBody = ((originalDocument != null) ? null : matchedText.OwnerParagraph.OwnerTextBody);
		Insertion(originalDocument, revParaItemIndex, index, index2, orgCurrMatchSecIndex, orgTextBody);
	}

	internal void Insertion(WordDocument originalDocument, int currRevParaItemIndex, int orgCurrMatchParaItemIndex, int orgCurrMatchBodyItemIndex, int orgCurrMatchSecIndex, WTextBody orgTextBody = null)
	{
		int index = Index;
		int num = -1;
		bool flag = false;
		if (orgTextBody == null)
		{
			num = base.Document.m_bodyItemIndex;
			if (base.Document.m_sectionIndex != GetOwnerSection(this).Index)
			{
				flag = true;
			}
		}
		else
		{
			num = base.OwnerTextBody.m_bodyItemIndex;
		}
		if (num != index || flag)
		{
			base.Document.Comparison.Insertion(originalDocument, currRevParaItemIndex, index, GetOwnerSection(this).Index, orgCurrMatchParaItemIndex, orgCurrMatchBodyItemIndex, orgCurrMatchSecIndex, orgTextBody, base.OwnerTextBody);
			return;
		}
		WParagraph wParagraph = null;
		int num2 = -1;
		if (orgTextBody == null)
		{
			wParagraph = originalDocument.Sections[orgCurrMatchSecIndex].Body.Items[orgCurrMatchBodyItemIndex] as WParagraph;
			num2 = base.Document.m_paraItemIndex;
			originalDocument.IsComparing = true;
		}
		else
		{
			wParagraph = orgTextBody.Items[orgCurrMatchBodyItemIndex] as WParagraph;
			num2 = base.OwnerTextBody.m_paraItemIndex;
			orgTextBody.Document.IsComparing = true;
		}
		int num3 = orgCurrMatchParaItemIndex;
		while (num3 > 0 && (wParagraph.Items[num3 - 1] is BookmarkStart || wParagraph.Items[num3 - 1] is BookmarkEnd || wParagraph.Items[num3 - 1] is EditableRangeStart || wParagraph.Items[num3 - 1] is EditableRangeEnd))
		{
			num3--;
		}
		for (int i = num2; i < currRevParaItemIndex; i++)
		{
			wParagraph.Items.Insert(num3, Items[i].Clone());
			if (!(Items[i] is BookmarkEnd) && !(Items[i] is BookmarkStart) && !(Items[i] is EditableRangeStart) && !(Items[i] is EditableRangeEnd))
			{
				ParagraphItem item = wParagraph.Items[num3];
				ApplyInsRevision(item);
			}
			num3++;
		}
		if (orgTextBody == null)
		{
			originalDocument.IsComparing = false;
			originalDocument.m_sectionIndex = orgCurrMatchSecIndex;
			if (num3 + 1 >= wParagraph.Items.Count)
			{
				originalDocument.UpdateIndex(orgCurrMatchBodyItemIndex + 1, 0, 0, 0, 0);
			}
			else
			{
				originalDocument.UpdateIndex(orgCurrMatchBodyItemIndex, num3 + 1, 0, 0, 0, 0);
			}
		}
		else
		{
			orgTextBody.Document.IsComparing = false;
			if (num3 + 1 >= wParagraph.Items.Count)
			{
				orgTextBody.UpdateIndex(orgCurrMatchBodyItemIndex + 1, 0, 0, 0, 0);
			}
			else
			{
				orgTextBody.UpdateIndex(orgCurrMatchBodyItemIndex, num3 + 1, 0, 0, 0, 0);
			}
		}
	}

	internal WParagraph AppendTextToParagraphEnd(WordDocument originalDocument, WParagraph bkmkStartParagraph, WTextBody originalTextBody = null)
	{
		WParagraph wParagraph = null;
		int num = -1;
		if (originalTextBody == null)
		{
			wParagraph = originalDocument.Sections[originalDocument.m_sectionIndex].Body.Items[originalDocument.m_bodyItemIndex] as WParagraph;
			num = base.Document.m_paraItemIndex;
		}
		else
		{
			wParagraph = originalTextBody.Items[originalTextBody.m_bodyItemIndex] as WParagraph;
			num = base.OwnerTextBody.m_paraItemIndex;
		}
		if (wParagraph != null)
		{
			for (int i = num; i < bkmkStartParagraph.Items.Count; i++)
			{
				ParagraphItem paragraphItem = bkmkStartParagraph.Items[i].Clone() as ParagraphItem;
				wParagraph.Items.Add(paragraphItem);
				ApplyInsRevision(paragraphItem);
			}
			if (wParagraph.BreakCharacterFormat.IsDeleteRevision)
			{
				wParagraph.BreakCharacterFormat.IsDeleteRevision = false;
				wParagraph.RemoveEntityRevision(isNeedToRemoveFormatRev: false);
				wParagraph.UpdateParagraphRevision(wParagraph, isIncludeParaItems: true);
			}
			if (originalTextBody == null)
			{
				base.Document.UpdateIndex(base.Document.m_bodyItemIndex + 1, 0, 0, 0, 0);
			}
			else
			{
				base.OwnerTextBody.UpdateIndex(base.OwnerTextBody.m_bodyItemIndex + 1, 0, 0, 0, 0);
			}
			if (bkmkStartParagraph.NextSibling == null)
			{
				return null;
			}
			return base.Document.GetOwnerParagraphToInsertBookmark(bkmkStartParagraph.NextSibling as Entity, isStart: true);
		}
		return bkmkStartParagraph;
	}

	internal override void AddInsMark()
	{
		BreakCharacterFormat.IsInsertRevision = true;
		BreakCharacterFormat.AuthorName = base.Document.m_authorName;
		BreakCharacterFormat.RevDateTime = base.Document.m_dateTime;
		ApplyInsRevision(this, 0, Items.Count);
		UpdateParagraphRevision(this, isIncludeParaItems: true);
	}

	internal override void AddDelMark()
	{
		BreakCharacterFormat.IsDeleteRevision = true;
		BreakCharacterFormat.AuthorName = base.Document.m_authorName;
		BreakCharacterFormat.RevDateTime = base.Document.m_dateTime;
		ApplyDelRevision(this, 0, Items.Count);
		UpdateParagraphRevision(this, isIncludeParaItems: true);
	}

	internal void RemoveDelMark()
	{
		BreakCharacterFormat.IsDeleteRevision = false;
		for (int i = 0; i < Items.Count; i++)
		{
			ParagraphItem paragraphItem = Items[i];
			if (paragraphItem is WTextRange || paragraphItem is WPicture || paragraphItem is WTextBox || paragraphItem is Break)
			{
				base.Document.RemoveDelMark(paragraphItem);
				if (paragraphItem is WField)
				{
					i = (paragraphItem as WField).FieldEnd.Index;
				}
			}
		}
	}

	internal void RemoveInsMark()
	{
		BreakCharacterFormat.IsInsertRevision = false;
		for (int i = 0; i < Items.Count; i++)
		{
			ParagraphItem paragraphItem = Items[i];
			if (paragraphItem is WTextRange || paragraphItem is WPicture || paragraphItem is WTextBox || paragraphItem is Break)
			{
				base.Document.RemoveInsMark(paragraphItem);
				if (paragraphItem is WField)
				{
					i = (paragraphItem as WField).FieldEnd.Index;
				}
			}
		}
	}

	private void DeleteItems(WordDocument originalDocument, int startSecIndex, int endSecIndex, int startBodyItemIndex, int endBodyItemIndex)
	{
		for (int i = startSecIndex; i <= endSecIndex; i++)
		{
			WSection wSection = originalDocument.Sections[i];
			int j = ((i == startSecIndex) ? startBodyItemIndex : 0);
			for (int num = ((i == endSecIndex) ? endBodyItemIndex : (wSection.Body.Items.Count - 1)); j <= num; j++)
			{
				if (wSection.Body.ChildEntities[j] is WParagraph)
				{
					(wSection.Body.ChildEntities[j] as WParagraph).AddDelMark();
				}
				else if (wSection.Body.ChildEntities[j] is WTable)
				{
					(wSection.Body.ChildEntities[j] as WTable).AddDelMark();
				}
			}
		}
	}

	internal void UpdateParagraphRevision(WParagraph paragraph, bool isIncludeParaItems)
	{
		base.Document.ParagraphFormatChange(paragraph.ParagraphFormat);
		base.Document.CharacterFormatChange(paragraph.BreakCharacterFormat, null, null);
		if (isIncludeParaItems)
		{
			for (int i = 0; i < paragraph.Items.Count; i++)
			{
				ParagraphItem paragraphItem = paragraph.Items[i];
				if (!(paragraphItem is BookmarkStart) && !(paragraphItem is BookmarkEnd))
				{
					paragraph.Items.UpdateTrackRevision(paragraphItem);
				}
			}
		}
		base.Document.UpdateLastItemRevision(paragraph, paragraph.Items);
	}

	internal void ApplyInsRevision(WParagraph paragraph, int startIndex, int endIndex)
	{
		for (int i = startIndex; i < endIndex; i++)
		{
			ParagraphItem paragraphItem = paragraph.Items[i];
			if (CheckParaItem(paragraphItem))
			{
				ApplyInsRevision(paragraphItem);
			}
			else if (paragraphItem is WFieldMark)
			{
				WField parentField = (paragraphItem as WFieldMark).ParentField;
				if (parentField.IsInsertRevision || parentField.Owner is WOleObject || parentField.Owner is TableOfContent)
				{
					ApplyInsRevision(paragraphItem);
				}
			}
		}
	}

	internal void ApplyInsRevision(ParagraphItem item)
	{
		if (item is BookmarkStart || item is BookmarkEnd || item is EditableRangeStart || item is EditableRangeEnd)
		{
			return;
		}
		if (!(item is InlineContentControl))
		{
			WCharacterFormat wCharacterFormat = ((!(item is Break)) ? item.GetCharFormat() : (item as Break).CharacterFormat);
			wCharacterFormat.IsInsertRevision = true;
			wCharacterFormat.AuthorName = base.Document.m_authorName;
			wCharacterFormat.RevDateTime = base.Document.m_dateTime;
		}
		if (!item.OwnerParagraph.IsInsertRevision || item is WOleObject)
		{
			item.UpdateParaItemRevision(item);
		}
		switch (item.EntityType)
		{
		case EntityType.TextBox:
		{
			WTextBox wTextBox = item as WTextBox;
			base.Document.Comparison.ApplyInsRev(wTextBox.TextBoxBody, 0, wTextBox.TextBoxBody.Items.Count);
			break;
		}
		case EntityType.Shape:
		{
			Shape shape = item as Shape;
			base.Document.Comparison.ApplyInsRev(shape.TextBody, 0, shape.TextBody.Items.Count);
			break;
		}
		case EntityType.InlineContentControl:
		{
			foreach (ParagraphItem paragraphItem in (item as InlineContentControl).ParagraphItems)
			{
				ApplyInsRevision(paragraphItem);
			}
			break;
		}
		}
	}

	private void AddDeleteRevisonMark(TextSelection matchedRange, WordDocument originalDocument, ref bool IsNeedToInsert)
	{
		int index = matchedRange.StartTextRange.Index;
		int index2 = matchedRange.OwnerParagraph.Index;
		int endSecIndex = ((originalDocument != null) ? matchedRange.OwnerParagraph.GetOwnerSection(matchedRange.OwnerParagraph).Index : 0);
		WTextBody originalTextBody = ((originalDocument != null) ? null : matchedRange.OwnerParagraph.OwnerTextBody);
		WordDocument revisedDocument = ((originalDocument != null) ? base.Document : null);
		AddDeleteRevisionMark(originalDocument, revisedDocument, endSecIndex, index2, index, ref IsNeedToInsert, originalTextBody);
	}

	private void AddDeleteRevisionMark(WordDocument originalDocument, WordDocument revisedDocument, int endSecIndex, int endBodyItemIndex, int endParaItemIndex, ref bool isNeedToInsert, WTextBody originalTextBody = null)
	{
		int num = originalTextBody?.m_bodyItemIndex ?? originalDocument.m_bodyItemIndex;
		if ((originalTextBody == null && originalDocument.m_sectionIndex != endSecIndex) || num != endBodyItemIndex)
		{
			((originalTextBody == null) ? originalDocument.Comparison : originalTextBody.Document.Comparison).ApplyDelRevision(originalDocument, revisedDocument, endSecIndex, endBodyItemIndex, endParaItemIndex, ref isNeedToInsert, isDocumentEnd: false, originalTextBody);
		}
		int startIndex = 0;
		if (num == endBodyItemIndex)
		{
			startIndex = originalTextBody?.m_paraItemIndex ?? originalDocument.m_paraItemIndex;
		}
		WParagraph paragraph = ((originalTextBody == null) ? (originalDocument.Sections[endSecIndex].Body.Items[endBodyItemIndex] as WParagraph) : (originalTextBody.Items[endBodyItemIndex] as WParagraph));
		ApplyDelRevision(paragraph, startIndex, endParaItemIndex);
	}

	private void AddDeleteRevisionMarkInTextBody(WTextBody textBody, int endBodyItemIndex, int endParaItemIndex)
	{
		for (int i = textBody.m_bodyItemIndex; i <= endBodyItemIndex; i++)
		{
			Entity entity = textBody.ChildEntities[i];
			if (entity is WParagraph)
			{
				WParagraph wParagraph = entity as WParagraph;
				if (i == textBody.m_bodyItemIndex)
				{
					if (textBody.m_paraItemIndex == 0)
					{
						wParagraph.AddDelMark();
					}
					else if (textBody.m_paraItemIndex == wParagraph.Items.Count)
					{
						wParagraph.BreakCharacterFormat.IsDeleteRevision = true;
						wParagraph.BreakCharacterFormat.AuthorName = base.Document.m_authorName;
						wParagraph.BreakCharacterFormat.RevDateTime = base.Document.m_dateTime;
						wParagraph.UpdateParagraphRevision(wParagraph, isIncludeParaItems: true);
					}
					else
					{
						ApplyDelRevision(wParagraph, textBody.m_paraItemIndex, wParagraph.Items.Count);
					}
				}
				else if (i == endBodyItemIndex)
				{
					ApplyDelRevision(wParagraph, 0, endParaItemIndex);
				}
				else
				{
					wParagraph.AddDelMark();
				}
			}
			else if (entity is WTable)
			{
				(entity as WTable).AddDelMark();
			}
			else if (entity is BlockContentControl)
			{
				(entity as BlockContentControl).AddDelMark();
			}
		}
	}

	internal void ApplyDelRevision(WParagraph paragraph, int startIndex, int endIndex)
	{
		for (int i = startIndex; i < endIndex; i++)
		{
			if (startIndex >= paragraph.Items.Count)
			{
				break;
			}
			ParagraphItem paragraphItem = paragraph.Items[i];
			if (CheckParaItem(paragraphItem))
			{
				ApplyDelRevision(paragraphItem);
				if (paragraphItem is WField)
				{
					i = (((paragraphItem as WField).FieldEnd.OwnerParagraph == (paragraphItem as WField).OwnerParagraph) ? (paragraphItem as WField).FieldEnd.Index : endIndex);
				}
			}
			else if (paragraphItem is WFieldMark)
			{
				WField parentField = (paragraphItem as WFieldMark).ParentField;
				if (parentField.IsDeleteRevision || parentField.Owner is WOleObject || parentField.FieldType == FieldType.FieldTOC)
				{
					ApplyDelRevision(paragraphItem);
				}
			}
		}
	}

	internal void ApplyDelRevision(ParagraphItem paraItem)
	{
		if (paraItem is BookmarkStart || paraItem is BookmarkEnd || paraItem is EditableRangeStart || paraItem is EditableRangeEnd)
		{
			return;
		}
		WParagraph ownerParagraph = paraItem.OwnerParagraph;
		if (!(paraItem is InlineContentControl))
		{
			WCharacterFormat wCharacterFormat = ((!(paraItem is Break)) ? paraItem.GetCharFormat() : (paraItem as Break).CharacterFormat);
			wCharacterFormat.IsDeleteRevision = true;
			wCharacterFormat.AuthorName = base.Document.m_authorName;
			wCharacterFormat.RevDateTime = base.Document.m_dateTime;
		}
		if (paraItem.Document != null && paraItem.Document.Comparison != null)
		{
			paraItem.Document.Comparison.RemoveFromDocCollection(paraItem);
		}
		if (!ownerParagraph.IsDeleteRevision || paraItem is WOleObject)
		{
			paraItem.UpdateParaItemRevision(paraItem);
		}
		WTableRow wTableRow = null;
		if (ownerParagraph.IsInCell)
		{
			wTableRow = ownerParagraph.GetOwnerTableCell(ownerParagraph.OwnerTextBody).OwnerRow;
		}
		switch (paraItem.EntityType)
		{
		case EntityType.TextBox:
		{
			WTextBox wTextBox = paraItem as WTextBox;
			if (wTableRow == null || !wTableRow.IsDeleteRevision)
			{
				base.Document.Comparison.ApplyDelRev(wTextBox.TextBoxBody, 0, wTextBox.TextBoxBody.ChildEntities.Count);
			}
			break;
		}
		case EntityType.Shape:
		{
			Shape shape = paraItem as Shape;
			base.Document.Comparison.ApplyDelRev(shape.TextBody, 0, shape.TextBody.ChildEntities.Count);
			break;
		}
		case EntityType.InlineContentControl:
		{
			foreach (ParagraphItem paragraphItem in (paraItem as InlineContentControl).ParagraphItems)
			{
				ApplyDelRevision(paragraphItem);
			}
			break;
		}
		case EntityType.Field:
		case EntityType.MergeField:
		case EntityType.SeqField:
		case EntityType.EmbededField:
		case EntityType.ControlField:
		case EntityType.TextFormField:
		case EntityType.DropDownFormField:
		case EntityType.CheckBox:
			if ((paraItem as WField).FieldEnd.OwnerParagraph.Index == (paraItem as WField).OwnerParagraph.Index)
			{
				ApplyDelRevision(paraItem.OwnerParagraph, paraItem.Index + 1, (paraItem as WField).FieldEnd.Index + 1);
			}
			else
			{
				ApplyDelRevision(paraItem.OwnerParagraph, paraItem.Index + 1, paraItem.OwnerParagraph.Items.Count);
			}
			break;
		case EntityType.Break:
			if ((paraItem as Break).BreakType == BreakType.PageBreak)
			{
				WParagraph ownerParagraph2 = paraItem.OwnerParagraph;
				ownerParagraph2.BreakCharacterFormat.IsDeleteRevision = true;
				ownerParagraph2.BreakCharacterFormat.AuthorName = base.Document.m_authorName;
				ownerParagraph2.BreakCharacterFormat.RevDateTime = base.Document.m_dateTime;
				ownerParagraph2.UpdateParagraphRevision(ownerParagraph2, isIncludeParaItems: true);
			}
			break;
		}
	}

	internal bool CheckParaItem(ParagraphItem item)
	{
		if (!(item is WTextRange) && !(item is WPicture) && !(item is WTextBox) && !(item is Break) && !(item is InlineContentControl) && !(item is Shape) && !(item is WOleObject) && !(item is WChart) && !(item is WMath) && !(item is GroupShape) && !(item is TableOfContent))
		{
			return item is WSymbol;
		}
		return true;
	}

	internal string GetNextWord(ref int paraItemIndex, ref int textStartIndex, ref int textEndIndex, ref int startRangeIndex, ref int endRangeIndex, ref int wordIndex, ref Dictionary<int, SkippedBkmkInfo> skippedBookmarks)
	{
		startRangeIndex = paraItemIndex;
		string text = string.Empty;
		textStartIndex = textEndIndex + 1;
		int num = textStartIndex;
		while (paraItemIndex < Items.Count)
		{
			if (Items[paraItemIndex] is WTextRange && !(Items[paraItemIndex] is WField))
			{
				WTextRange wTextRange = Items[paraItemIndex] as WTextRange;
				string[] array = wTextRange.Text.Split(wTextRange.Document.WordComparisonDelimiters);
				if (wordIndex < array.Length && num < wTextRange.Text.Length && IsSpecialDelimeter(wTextRange.Text[num]))
				{
					if (!string.IsNullOrEmpty(text))
					{
						if (wordIndex == 0)
						{
							SkipPrevNonRenderableItems(ref paraItemIndex, ref wordIndex, ref textEndIndex, wTextRange.Document.WordComparisonDelimiters);
						}
						return text;
					}
					textStartIndex = num;
					textEndIndex = textStartIndex;
					if (array[wordIndex].Length == 0)
					{
						wordIndex++;
					}
					return wTextRange.Text[textStartIndex].ToString();
				}
				if (wordIndex >= array.Length)
				{
					wordIndex = 0;
					paraItemIndex++;
					startRangeIndex = paraItemIndex;
					textEndIndex = -1;
					textStartIndex = 0;
					num = 0;
					continue;
				}
				if (array.Length > 1 && wordIndex < array.Length - 1)
				{
					text += array[wordIndex];
					textEndIndex += array[wordIndex].Length;
					wordIndex++;
					break;
				}
				if (wordIndex != array.Length - 1)
				{
					continue;
				}
				text += array[wordIndex];
				if (paraItemIndex != Items.Count - 1)
				{
					paraItemIndex++;
					if (text.Length == 0)
					{
						startRangeIndex++;
						textStartIndex = 0;
					}
					num = 0;
					wordIndex = 0;
					textEndIndex = -1;
					continue;
				}
				int num2 = paraItemIndex;
				if (string.IsNullOrEmpty((Items[paraItemIndex] as WTextRange).Text))
				{
					num2--;
				}
				string text2 = (Items[num2] as WTextRange).Text;
				wordIndex = text2.Split(Items.Document.WordComparisonDelimiters).Length;
				textEndIndex = text2.Length - 1;
				break;
			}
			if (CheckParaItem(Items[paraItemIndex]))
			{
				if (!string.IsNullOrEmpty(text))
				{
					SkipPrevNonRenderableItems(ref paraItemIndex, ref wordIndex, ref textEndIndex, Items.Document.WordComparisonDelimiters);
				}
				break;
			}
			if (string.IsNullOrEmpty(text) && IsNonRenderableItem(Items[paraItemIndex]))
			{
				break;
			}
			if (!string.IsNullOrEmpty(text))
			{
				if (!(Items[paraItemIndex] is BookmarkStart) && !(Items[paraItemIndex] is BookmarkEnd) && !(Items[paraItemIndex] is EditableRangeStart) && !(Items[paraItemIndex] is EditableRangeEnd))
				{
					paraItemIndex--;
					string text3 = (Items[paraItemIndex] as WTextRange).Text;
					wordIndex = text3.Split(Items.Document.WordComparisonDelimiters).Length;
					textEndIndex = text3.Length - 1;
					return text;
				}
				paraItemIndex++;
				wordIndex = 0;
				textEndIndex = -1;
				if (paraItemIndex >= Items.Count)
				{
					SkipPrevNonRenderableItems(ref paraItemIndex, ref wordIndex, ref textEndIndex, Items.Document.WordComparisonDelimiters);
					break;
				}
				if (Items[paraItemIndex - 1] is BookmarkStart || Items[paraItemIndex - 1] is BookmarkEnd)
				{
					SkippedBkmkInfo value = default(SkippedBkmkInfo);
					value.IsBkmkEnd = Items[paraItemIndex - 1] is BookmarkEnd;
					value.BkmkName = (value.IsBkmkEnd ? (Items[paraItemIndex - 1] as BookmarkEnd).Name : (Items[paraItemIndex - 1] as BookmarkStart).Name);
					if (skippedBookmarks == null)
					{
						skippedBookmarks = new Dictionary<int, SkippedBkmkInfo>();
					}
					skippedBookmarks.Add(text.Length, value);
				}
			}
			else
			{
				paraItemIndex++;
				wordIndex = 0;
				startRangeIndex++;
				textStartIndex = 0;
				num = 0;
				textEndIndex = -1;
			}
		}
		endRangeIndex = paraItemIndex;
		return text;
	}

	private void SkipPrevNonRenderableItems(ref int paraItemIndex, ref int wordIndex, ref int textEndIndex, char[] delimiters)
	{
		paraItemIndex--;
		while (paraItemIndex > -1)
		{
			if (Items[paraItemIndex] is WTextRange && !(Items[paraItemIndex] is WField))
			{
				string text = (Items[paraItemIndex] as WTextRange).Text;
				wordIndex = text.Split(delimiters).Length;
				textEndIndex = text.Length - 1;
				break;
			}
			if (IsNonRenderableItem(Items[paraItemIndex]))
			{
				paraItemIndex--;
			}
		}
	}

	internal bool IsNonRenderableItem(ParagraphItem item)
	{
		if (!(item is BookmarkStart) && !(item is BookmarkEnd) && !(item is EditableRangeStart))
		{
			return item is EditableRangeEnd;
		}
		return true;
	}

	internal bool IsParaHasOnlyBookmark()
	{
		for (int i = 0; i < ChildEntities.Count; i++)
		{
			if (!(ChildEntities[i] is BookmarkStart) && !(ChildEntities[i] is BookmarkEnd))
			{
				return false;
			}
		}
		return true;
	}

	internal bool IsSpecialDelimeter(char delimiter)
	{
		return new string(base.Document.WordComparisonDelimiters).Contains(delimiter.ToString());
	}

	private bool CheckForDelimiter(TextSelection matchedText, ref WTextRange originalText, ref char original, ref int i)
	{
		if (matchedText.m_startCut > 0 && matchedText.m_startCut < matchedText.StartTextRange.TextLength)
		{
			originalText = matchedText.StartTextRange;
			original = matchedText.StartTextRange.Text[matchedText.m_startCut - i];
			return false;
		}
		return matchedText.OwnerParagraph.CheckPreviousRange(ref originalText, ref original, ref i, matchedText.StartTextRange.Index - 1);
	}

	internal bool CheckPreviousRange(ref WTextRange originalText, ref char original, ref int i, int itemIndex)
	{
		SkipBookmarks(this, ref itemIndex);
		if (Items[itemIndex] is WTextRange)
		{
			originalText = Items[itemIndex] as WTextRange;
			if (originalText.TextLength >= i)
			{
				original = originalText.Text[originalText.Text.Length - i];
			}
			return false;
		}
		return true;
	}

	internal void CompareSpecialDelimiterWithPreviousText(TextSelection matchedText, int paraItemIndex, int textStartIndex)
	{
		char original = '\0';
		char c = '\0';
		WTextRange originalText = null;
		WTextRange wTextRange = null;
		string text = string.Empty;
		string text2 = string.Empty;
		int i = 1;
		while (((matchedText.StartTextRange.Index <= 0 && (matchedText.StartTextRange.Index != 0 || matchedText.m_startCut <= 0)) || !CheckForDelimiter(matchedText, ref originalText, ref original, ref i)) && IsSpecialDelimeter(original) && (paraItemIndex > 0 || (paraItemIndex == 0 && textStartIndex > 0)))
		{
			if (textStartIndex > 0 && (Items[paraItemIndex] as WTextRange).Text.Length > textStartIndex - i)
			{
				wTextRange = Items[paraItemIndex] as WTextRange;
				c = (Items[paraItemIndex] as WTextRange).Text[textStartIndex - i];
			}
			else
			{
				int paraItemIndex2 = paraItemIndex - 1;
				SkipBookmarks(this, ref paraItemIndex2);
				if (!(Items[paraItemIndex2] is WTextRange))
				{
					break;
				}
				wTextRange = Items[paraItemIndex2] as WTextRange;
				if (wTextRange.TextLength >= i)
				{
					c = wTextRange.Text[wTextRange.Text.Length - i];
				}
			}
			if (c != original)
			{
				break;
			}
			text += original;
			text2 += c;
			i++;
			if (!((matchedText.m_startCut > 0) ? (matchedText.m_startCut - i > -1) : (originalText.Text.Length - i > -1)) || !((textStartIndex > 0) ? (textStartIndex - i > -1) : (wTextRange.Text.Length - i > -1)))
			{
				break;
			}
		}
		if (wTextRange == null || originalText == null || !(text == text2) || text.Length <= 0)
		{
			return;
		}
		char[] array = text.ToCharArray();
		Array.Reverse(array);
		text = new string(array);
		if (originalText.IsInsertRevision)
		{
			ParagraphItem paragraphItem = originalText.PreviousSibling as ParagraphItem;
			if (paragraphItem == null)
			{
				return;
			}
			matchedText.OwnerParagraph.Document.IsComparing = true;
			while (paragraphItem != null)
			{
				if (paragraphItem.IsDeleteRevision)
				{
					if (!(paragraphItem is WTextRange))
					{
						return;
					}
					string text3 = (paragraphItem as WTextRange).Text;
					bool flag = false;
					if (text3 == text)
					{
						matchedText.OwnerParagraph.Items.RemoveAt(paragraphItem.Index);
						flag = true;
					}
					else if (text3.EndsWith(text) && text3.Length > text.Length)
					{
						(paragraphItem as WTextRange).Text = text3.Remove(text3.Length - text.Length);
						flag = true;
					}
					if (flag)
					{
						if (originalText.Text == text)
						{
							originalText.RemoveEntityRevision(isNeedToRemoveFormatRev: true);
							originalText.CharacterFormat.IsInsertRevision = false;
							originalText.CharacterFormat.AuthorName = string.Empty;
							originalText.CharacterFormat.RevDateTime = DateTime.MinValue;
						}
						else
						{
							WTextRange wTextRange2 = originalText.Clone() as WTextRange;
							wTextRange2.Text = text;
							originalText.Text = originalText.Text.Remove(originalText.Text.Length - text.Length);
							matchedText.OwnerParagraph.Items.Insert(originalText.Index + 1, wTextRange2);
							wTextRange2.CharacterFormat.IsInsertRevision = false;
							wTextRange2.CharacterFormat.AuthorName = string.Empty;
							wTextRange2.CharacterFormat.RevDateTime = DateTime.MinValue;
						}
					}
					break;
				}
				paragraphItem = paragraphItem.PreviousSibling as ParagraphItem;
				paragraphItem = ((paragraphItem != null && paragraphItem.Index <= matchedText.OwnerParagraph.Document.m_matchParaItemIndex) ? null : paragraphItem);
			}
			if (originalText.IsInsertRevision)
			{
				return;
			}
			CheckPreviousRange(originalText, wTextRange, i);
			matchedText.OwnerParagraph.Document.IsComparing = false;
		}
		if (!originalText.CharacterFormat.ComparePropertiesCount(wTextRange.CharacterFormat))
		{
			if (matchedText.m_startCut > 0 && matchedText.m_startCut < matchedText.StartTextRange.TextLength)
			{
				matchedText.GetRanges();
				WTextRange wTextRange3 = matchedText.OwnerParagraph.Items[matchedText.StartTextRange.Index - 1] as WTextRange;
				wTextRange3.Text = wTextRange3.Text.Remove(wTextRange3.Text.Length - text.Length);
				WTextRange wTextRange4 = wTextRange3.Clone() as WTextRange;
				wTextRange4.Text = text;
				matchedText.OwnerParagraph.Items.Insert(wTextRange3.Index, wTextRange4);
				wTextRange4.CharacterFormat.CompareProperties(wTextRange.CharacterFormat);
			}
			else
			{
				WTextRange wTextRange5 = originalText.Clone() as WTextRange;
				wTextRange5.Text = text;
				originalText.Text = originalText.Text.Remove(originalText.Text.Length - text.Length);
				matchedText.OwnerParagraph.Items.Insert(originalText.Index + 1, wTextRange5);
				wTextRange5.CharacterFormat.CompareProperties(wTextRange.CharacterFormat);
			}
		}
	}

	private void CheckPreviousRange(WTextRange originalText, WTextRange revisedText, int i)
	{
		if (originalText.Text.Length - i < 0 && originalText.PreviousSibling is WTextRange)
		{
			bool isNotMatched = false;
			WTextRange wTextRange = ((originalText.PreviousSibling != null) ? (originalText.PreviousSibling as WTextRange) : null);
			WTextRange wTextRange2 = ((revisedText.PreviousSibling != null) ? (revisedText.PreviousSibling as WTextRange) : null);
			while (!isNotMatched && wTextRange != null && wTextRange2 != null && wTextRange.Text.Length > 0 && wTextRange2.Text.Length > 0 && IsSpecialDelimeter(wTextRange.Text[wTextRange.Text.Length - 1]))
			{
				CompareSpaces(wTextRange, wTextRange2, ref isNotMatched);
				wTextRange = ((wTextRange.PreviousSibling != null) ? (wTextRange.PreviousSibling as WTextRange) : null);
				wTextRange2 = ((wTextRange2.PreviousSibling != null) ? (wTextRange2.PreviousSibling as WTextRange) : null);
			}
		}
	}

	private void CompareSpaces(WTextRange originalText, WTextRange revisedText, ref bool isNotMatched)
	{
		string text = GetCharacters(originalText);
		string text2 = GetCharacters(revisedText);
		if (text.Length != text2.Length)
		{
			if (text.Length > text2.Length)
			{
				text = text.Substring(0, text2.Length);
			}
			else
			{
				text2 = text2.Substring(0, text.Length);
			}
			isNotMatched = true;
		}
		if (text.Length != originalText.TextLength)
		{
			isNotMatched = true;
		}
		if (!(text == text2) || text.Length <= 0 || !originalText.IsInsertRevision)
		{
			return;
		}
		char[] array = text.ToCharArray();
		Array.Reverse(array);
		text = new string(array);
		ParagraphItem paragraphItem = originalText.PreviousSibling as ParagraphItem;
		if (paragraphItem == null)
		{
			return;
		}
		while (paragraphItem != null)
		{
			if (paragraphItem.IsDeleteRevision)
			{
				if (paragraphItem is WTextRange)
				{
					string text3 = (paragraphItem as WTextRange).Text;
					if (text3 == text)
					{
						originalText.OwnerParagraph.Items.RemoveAt(paragraphItem.Index);
					}
					else if (text3.EndsWith(text) && text3.Length > text.Length)
					{
						(paragraphItem as WTextRange).Text = text3.Remove(text3.Length - text.Length);
					}
					if (originalText.Text == text)
					{
						originalText.RemoveEntityRevision(isNeedToRemoveFormatRev: true);
						originalText.CharacterFormat.IsInsertRevision = false;
						originalText.CharacterFormat.AuthorName = string.Empty;
						originalText.CharacterFormat.RevDateTime = DateTime.MinValue;
					}
					else
					{
						WTextRange wTextRange = originalText.Clone() as WTextRange;
						wTextRange.Text = text;
						originalText.Text = originalText.Text.Remove(originalText.Text.Length - text.Length);
						originalText.OwnerParagraph.Items.Insert(originalText.Index + 1, wTextRange);
						wTextRange.CharacterFormat.IsInsertRevision = false;
						wTextRange.CharacterFormat.AuthorName = string.Empty;
						wTextRange.CharacterFormat.RevDateTime = DateTime.MinValue;
					}
				}
				break;
			}
			paragraphItem = paragraphItem.PreviousSibling as ParagraphItem;
		}
	}

	private string GetCharacters(WTextRange text)
	{
		string text2 = string.Empty;
		int num = text.TextLength - 1;
		while (num >= 0 && IsSpecialDelimeter(text.Text[num]))
		{
			text2 += text.Text[num];
			num--;
		}
		return text2;
	}

	internal void Compare(WTextBody orgTextBody)
	{
		int textStartIndex = 0;
		int startRangeIndex = 0;
		int endRangeIndex = 0;
		int textEndIndex = -1;
		int wordIndex = 0;
		bool firstMatchInPara = false;
		bool isLastWordMatched = false;
		bool hasMatch = false;
		Dictionary<int, SkippedBkmkInfo> skippedBookmarks = null;
		int paraItemIndex = base.OwnerTextBody.currParaItemIndex;
		while (paraItemIndex < Items.Count)
		{
			if (orgTextBody.m_bodyItemIndex == orgTextBody.Items.Count)
			{
				UpdateBodyItemIndex(textStartIndex, textEndIndex, paraItemIndex, base.OwnerTextBody);
				return;
			}
			string nextWord = GetNextWord(ref paraItemIndex, ref textStartIndex, ref textEndIndex, ref startRangeIndex, ref endRangeIndex, ref wordIndex, ref skippedBookmarks);
			if (paraItemIndex >= Items.Count)
			{
				continue;
			}
			base.OwnerTextBody.currParaItemIndex = paraItemIndex;
			ParagraphItem paragraphItem = Items[paraItemIndex];
			if (paraItemIndex == Items.Count - 1 && paragraphItem is WTextRange && string.IsNullOrEmpty((paragraphItem as WTextRange).Text))
			{
				paragraphItem = Items[paraItemIndex--];
			}
			if ((!(paragraphItem is WTextRange) || paragraphItem is WField) && isLastWordMatched)
			{
				SplitOriginalTextBodyText(orgTextBody);
			}
			switch (paragraphItem.EntityType)
			{
			case EntityType.InlineContentControl:
			case EntityType.Picture:
			case EntityType.Shape:
			case EntityType.TextBox:
			case EntityType.Chart:
			case EntityType.AutoShape:
			case EntityType.GroupShape:
			case EntityType.Math:
				CompareContainerElements(null, ref paraItemIndex, ref textStartIndex, ref textEndIndex, ref wordIndex, ref isLastWordMatched, ref startRangeIndex, orgTextBody);
				break;
			case EntityType.Field:
			case EntityType.MergeField:
			case EntityType.SeqField:
			case EntityType.EmbededField:
			case EntityType.ControlField:
			case EntityType.TextFormField:
			case EntityType.DropDownFormField:
			case EntityType.CheckBox:
			case EntityType.TOC:
				if ((paragraphItem is WField && !(paragraphItem as WField).IsNestedField) || paragraphItem is TableOfContent)
				{
					CompareField(null, ref paraItemIndex, ref textStartIndex, ref textEndIndex, ref wordIndex, ref isLastWordMatched, ref startRangeIndex, orgTextBody);
				}
				else
				{
					UpdateNextRevDocIndex(ref paraItemIndex, ref textStartIndex, ref textEndIndex, ref wordIndex, ref startRangeIndex);
				}
				break;
			case EntityType.OleObject:
				CompareOleObject(null, ref paraItemIndex, ref isLastWordMatched, ref startRangeIndex, ref textStartIndex, ref textEndIndex, ref wordIndex, orgTextBody);
				break;
			case EntityType.Break:
				if (isLastWordMatched || paraItemIndex == 0)
				{
					CompareWithCurrentItemOnly(null, ref paraItemIndex, ref textStartIndex, ref textEndIndex, ref wordIndex, ref isLastWordMatched, ref startRangeIndex, orgTextBody);
				}
				else
				{
					paraItemIndex++;
				}
				break;
			case EntityType.Footnote:
			case EntityType.Symbol:
				CompareWithCurrentItemOnly(null, ref paraItemIndex, ref textStartIndex, ref textEndIndex, ref wordIndex, ref isLastWordMatched, ref startRangeIndex, orgTextBody);
				break;
			case EntityType.BookmarkStart:
			case EntityType.BookmarkEnd:
			case EntityType.EditableRangeStart:
			case EntityType.EditableRangeEnd:
				CompareNonRenderableItems(null, orgTextBody, ref paraItemIndex, startRangeIndex);
				break;
			default:
				CompareTextranges(null, nextWord, ref paraItemIndex, ref textStartIndex, ref textEndIndex, ref wordIndex, ref isLastWordMatched, ref startRangeIndex, ref endRangeIndex, ref firstMatchInPara, ref skippedBookmarks, ref hasMatch, orgTextBody);
				break;
			case EntityType.Comment:
			case EntityType.CommentMark:
			case EntityType.CommentEnd:
				break;
			}
		}
		if (Index == base.OwnerTextBody.currBodyItemIndex && base.OwnerTextBody.currParaItemIndex >= Items.Count - 1)
		{
			base.OwnerTextBody.currParaItemIndex = 0;
		}
		if (Items.Count == 0 || IsEmptyParagraph())
		{
			CompareEmptyParagraphs(null, orgTextBody);
		}
		else if (hasMatch)
		{
			ComparePreviousEmptyParagraphs(null, orgTextBody);
		}
		if (orgTextBody.m_textStartIndex > 0 || orgTextBody.m_textEndIndex > -1)
		{
			SplitUnmatchedTextAndMoveParaItemIndex(orgTextBody.Items[orgTextBody.m_bodyItemIndex] as WParagraph, ref orgTextBody.m_paraItemIndex, ref orgTextBody.m_textStartIndex, ref orgTextBody.m_textEndIndex);
		}
		if (base.OwnerTextBody.m_textStartIndex > 0 || base.OwnerTextBody.m_textEndIndex > -1)
		{
			SplitUnmatchedTextAndMoveParaItemIndex(base.OwnerTextBody.Items[base.OwnerTextBody.m_bodyItemIndex] as WParagraph, ref base.OwnerTextBody.m_paraItemIndex, ref base.OwnerTextBody.m_textStartIndex, ref base.OwnerTextBody.m_textEndIndex);
		}
	}

	private void CompareTextranges(WordDocument originalDocument, string wordToMatch, ref int paraItemIndex, ref int textStartIndex, ref int textEndIndex, ref int wordIndex, ref bool isLastWordMatched, ref int startRangeIndex, ref int endRangeIndex, ref bool firstMatchInPara, ref Dictionary<int, SkippedBkmkInfo> skippedBookmarks, ref bool hasMatch, WTextBody orgTextBody = null)
	{
		WordDocument wordDocument = ((orgTextBody == null) ? originalDocument : orgTextBody.Document);
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		int num4 = -1;
		int num5 = -1;
		int num6 = -1;
		if (orgTextBody == null)
		{
			num = originalDocument.m_sectionIndex;
			num2 = originalDocument.m_bodyItemIndex;
			num3 = originalDocument.m_paraItemIndex;
			num4 = originalDocument.m_textEndIndex;
			num5 = originalDocument.m_matchBodyItemIndex;
			num6 = originalDocument.m_textStartIndex;
		}
		else
		{
			num = (orgTextBody.GetOwnerSection(orgTextBody) as WSection).Index;
			num2 = orgTextBody.m_bodyItemIndex;
			num3 = orgTextBody.m_paraItemIndex;
			num4 = orgTextBody.m_textEndIndex;
			num5 = orgTextBody.m_matchBodyItemIndex;
			num6 = orgTextBody.m_textStartIndex;
		}
		if (wordToMatch.Length == 1 && (IsSpecialDelimeter(wordToMatch[0]) || wordToMatch[0] == '\t'))
		{
			if (isLastWordMatched || ((num5 == -1 || (num3 == 0 && num6 == 0)) && paraItemIndex == 0 && textStartIndex == 0))
			{
				CompareSpecialDelimeters(originalDocument, wordToMatch, ref paraItemIndex, ref textStartIndex, ref textEndIndex, ref wordIndex, ref isLastWordMatched, ref startRangeIndex, ref firstMatchInPara, orgTextBody);
			}
			return;
		}
		Regex pattern = FindUtils.StringToRegex(wordToMatch, caseSensitive: true, wholeWord: true);
		TextSelection[] array = null;
		wordDocument.IsComparing = true;
		if (!string.IsNullOrEmpty(wordToMatch))
		{
			array = ((orgTextBody != null) ? orgTextBody.FindAll(pattern, isDocumentComparison: true, isFromTextbody: true)?.ToArray() : originalDocument.FindAll(pattern, isDocumentComparison: true));
		}
		wordDocument.IsComparing = false;
		if (array != null && array.Length != 0)
		{
			int num7 = FindMatchingWordFromSelection(array, num, num2, num3, num4);
			if (num7 == -1)
			{
				if (isLastWordMatched)
				{
					SplitBothOrgAndRevText(ref textStartIndex, ref startRangeIndex, ref paraItemIndex, ref wordIndex, ref textEndIndex, wordToMatch, originalDocument, orgTextBody);
				}
				isLastWordMatched = false;
				return;
			}
			TextSelection textSelection = array[num7];
			int textRangeIndex = paraItemIndex;
			SplitTextRanges(originalDocument, textSelection, isLastWordMatched, ref textStartIndex, ref startRangeIndex, ref paraItemIndex, ref wordIndex, ref textEndIndex, wordToMatch, orgTextBody);
			string[] array2 = (Items[paraItemIndex] as WTextRange).Text.Split(Items.Document.WordComparisonDelimiters);
			bool isNeedToMarkInsertion = false;
			bool isNeedToMarkDeletion = false;
			IsNeedToMarkInsDel(originalDocument, ref isNeedToMarkInsertion, ref isNeedToMarkDeletion, startRangeIndex, textSelection.OwnerParagraph.GetOwnerSection().Index, textSelection.OwnerParagraph.Index, textSelection.StartTextRange.Index, orgTextBody);
			if (isNeedToMarkDeletion || isNeedToMarkInsertion)
			{
				InsertionAndDeletion(originalDocument, isNeedToMarkInsertion, isNeedToMarkDeletion, textSelection, startRangeIndex);
			}
			if (!isLastWordMatched && (paraItemIndex != 0 || textStartIndex > 0))
			{
				CompareSpecialDelimiterWithPreviousText(textSelection, paraItemIndex, textStartIndex);
			}
			AddSkippedBookmarks(wordDocument, ref skippedBookmarks, textSelection);
			num3 = textSelection.EndTextRange.Index;
			CompareFormatting(matched: true, ref firstMatchInPara, originalDocument, orgTextBody, textRangeIndex, textSelection, ref startRangeIndex, ref paraItemIndex, ref textStartIndex, ref textEndIndex, ref num3);
			if (orgTextBody == null)
			{
				base.Document.m_sectionIndex = base.OwnerTextBody.GetOwnerSection(this).Index;
				originalDocument.m_sectionIndex = textSelection.OwnerParagraph.GetOwnerSection(textSelection.OwnerParagraph).Index;
				originalDocument.UpdateIndex(textSelection.OwnerParagraph.Index, num3, textSelection.StartTextRange.Index, num3, textSelection.m_startCut, textSelection.m_endCut - 1);
				base.Document.UpdateIndex(Index, paraItemIndex, base.Document.m_startRangeIndex, base.Document.m_endRangeIndex, textStartIndex, textEndIndex);
				base.Document.currParaItemIndex = paraItemIndex;
				originalDocument.UpdateMatchIndex();
			}
			else
			{
				orgTextBody.UpdateIndex(textSelection.OwnerParagraph.Index, num3, textSelection.StartTextRange.Index, num3, textSelection.m_startCut, textSelection.m_endCut - 1);
				base.OwnerTextBody.UpdateIndex(Index, paraItemIndex, base.OwnerTextBody.m_startRangeIndex, base.OwnerTextBody.m_endRangeIndex, textStartIndex, textEndIndex);
				base.OwnerTextBody.currParaItemIndex = paraItemIndex;
				orgTextBody.UpdateMatchIndex();
			}
			bool flag = false;
			if (wordIndex >= array2.Length)
			{
				paraItemIndex++;
				if (paraItemIndex < Items.Count && Items[paraItemIndex] is WTextRange && !(Items[paraItemIndex] is WField) && (Items[paraItemIndex] as WTextRange).TextLength == 0)
				{
					paraItemIndex++;
				}
				textEndIndex = -1;
				textStartIndex = 0;
				wordIndex = 0;
				flag = true;
			}
			bool flag2 = false;
			if (textSelection.m_endCut == -1)
			{
				num3++;
				if (textSelection.OwnerParagraph.ChildEntities.Count > num3 && textSelection.OwnerParagraph.Items[num3] is WTextRange && !(textSelection.OwnerParagraph.Items[num3] is WField) && (textSelection.OwnerParagraph.Items[num3] as WTextRange).TextLength == 0)
				{
					num3++;
				}
				flag2 = true;
			}
			WParagraph para;
			int itemIndex;
			if (originalDocument != null)
			{
				para = ((originalDocument.m_matchSectionIndex > -1 && originalDocument.m_matchBodyItemIndex > -1) ? (originalDocument.Sections[originalDocument.m_sectionIndex].Body.Items[originalDocument.m_bodyItemIndex] as WParagraph) : (originalDocument.Sections[0].Body.Items[0] as WParagraph));
				itemIndex = originalDocument.m_matchParaItemIndex + 1;
			}
			else
			{
				para = ((orgTextBody.m_matchBodyItemIndex > -1) ? (orgTextBody.Items[orgTextBody.m_bodyItemIndex] as WParagraph) : (orgTextBody.Items[0] as WParagraph));
				itemIndex = orgTextBody.m_paraItemIndex + 1;
			}
			int num8 = orgTextBody?.m_paraItemIndex ?? originalDocument.m_paraItemIndex;
			if ((paraItemIndex == Items.Count && flag && flag2 && (textSelection.OwnerParagraph.ChildEntities.Count - 1 == textSelection.EndTextRange.Index || textSelection.OwnerParagraph.ChildEntities.Count == num3 || textSelection.OwnerParagraph.ChildEntities.Count <= num8)) || (HasOnlyComment(para, ref itemIndex) && HasOnlyComment(this, ref paraItemIndex)))
			{
				if (orgTextBody == null)
				{
					base.Document.UpdateIndex(Index + 1, 0, 0, 0, 0);
					base.Document.currParaItemIndex = paraItemIndex;
					originalDocument.UpdateIndex(textSelection.OwnerParagraph.Index + 1, 0, 0, 0, 0);
				}
				else
				{
					base.OwnerTextBody.UpdateIndex(Index + 1, 0, 0, 0, 0);
					base.OwnerTextBody.currParaItemIndex = paraItemIndex;
					orgTextBody.UpdateIndex(textSelection.OwnerParagraph.Index + 1, 0, 0, 0, 0);
				}
			}
			else
			{
				if (flag)
				{
					if (orgTextBody == null)
					{
						base.Document.UpdateIndex(Index, paraItemIndex, paraItemIndex, paraItemIndex, 0);
						base.Document.currParaItemIndex = paraItemIndex;
					}
					else
					{
						base.OwnerTextBody.UpdateIndex(Index, paraItemIndex, paraItemIndex, paraItemIndex, 0);
						base.OwnerTextBody.currParaItemIndex = paraItemIndex;
					}
				}
				if (flag2)
				{
					if (orgTextBody == null)
					{
						originalDocument.UpdateIndex(originalDocument.m_bodyItemIndex, num3, num3, num3, 0);
					}
					else
					{
						orgTextBody.UpdateIndex(orgTextBody.m_bodyItemIndex, num3, num3, num3, 0);
					}
				}
			}
			isLastWordMatched = true;
			hasMatch = true;
		}
		else
		{
			if (isLastWordMatched)
			{
				SplitBothOrgAndRevText(ref textStartIndex, ref startRangeIndex, ref paraItemIndex, ref wordIndex, ref textEndIndex, wordToMatch, originalDocument, orgTextBody);
			}
			isLastWordMatched = false;
		}
	}

	private bool HasOnlyComment(WParagraph para, ref int itemIndex)
	{
		for (int i = itemIndex; i < para.Items.Count; i++)
		{
			if (!(para.Items[i] is WComment) && !(para.Items[i] is WCommentMark))
			{
				return false;
			}
		}
		itemIndex = para.Items.Count;
		return true;
	}

	internal void SplitUnmatchedTextAndMoveParaItemIndex(WParagraph lastMatchedParagraph, ref int paraItemIndex, ref int textStartCutIndex, ref int textEndCutIndex)
	{
		if (textStartCutIndex > 0 || textEndCutIndex > -1)
		{
			WTextRange wTextRange = lastMatchedParagraph.Items[paraItemIndex] as WTextRange;
			WTextRange wTextRange2 = wTextRange.Clone() as WTextRange;
			string text = wTextRange.Text;
			if (text.Length > textEndCutIndex)
			{
				wTextRange.Text = text.Remove(textEndCutIndex + 1);
				wTextRange2.Text = text.Substring(textEndCutIndex + 1);
				lastMatchedParagraph.Items.Insert(paraItemIndex + 1, wTextRange2);
			}
			paraItemIndex++;
			textStartCutIndex = 0;
			textEndCutIndex = -1;
		}
	}

	internal void CompareSpecialDelimeters(WordDocument originalDocument, string wordToMatch, ref int paraItemIndex, ref int textStartIndex, ref int textEndIndex, ref int wordIndex, ref bool isLastWordMatched, ref int startRangeIndex, ref bool firstMatchInPara, WTextBody originalTextBody = null)
	{
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		WParagraph wParagraph = null;
		if (originalTextBody == null)
		{
			num = originalDocument.m_bodyItemIndex;
			num2 = originalDocument.m_paraItemIndex;
			num3 = originalDocument.m_textEndIndex + 1;
			wParagraph = ((num < originalDocument.Sections[originalDocument.m_sectionIndex].Body.Items.Count) ? (originalDocument.Sections[originalDocument.m_sectionIndex].Body.Items[num] as WParagraph) : null);
		}
		else
		{
			num = originalTextBody.m_bodyItemIndex;
			num2 = originalTextBody.m_paraItemIndex;
			num3 = originalTextBody.m_textEndIndex + 1;
			wParagraph = ((num < originalTextBody.Items.Count) ? (originalTextBody.Items[num] as WParagraph) : null);
		}
		if (wParagraph != null && wParagraph.Items.Count > num2)
		{
			ParagraphItem paragraphItem = SkipNonRenderableItems(originalDocument, wParagraph, originalTextBody);
			if (paragraphItem == null)
			{
				return;
			}
			num2 = paragraphItem.Index;
			WTextRange wTextRange = wParagraph.Items[num2] as WTextRange;
			if (wTextRange != null && num3 < wTextRange.Text.Length)
			{
				char space = wTextRange.Text[num3];
				if (space.ToString() == wordToMatch)
				{
					WTextRange wTextRange2 = Items[paraItemIndex] as WTextRange;
					bool flag = wTextRange.CharacterFormat.Compare(wTextRange2.CharacterFormat);
					if (flag || (wTextRange.Text == wTextRange2.Text && wTextRange.Text == wordToMatch))
					{
						if (!flag)
						{
							CompareCharacterFormat(wTextRange.CharacterFormat, wTextRange2.CharacterFormat);
						}
						if (originalTextBody == null)
						{
							base.Document.UpdateIndex(Index, paraItemIndex, paraItemIndex, paraItemIndex, textStartIndex, textStartIndex);
							originalDocument.UpdateIndex(num, num2, num2, num2, num3, num3);
							originalDocument.UpdateMatchIndex();
						}
						else
						{
							base.OwnerTextBody.UpdateIndex(Index, paraItemIndex, paraItemIndex, paraItemIndex, textStartIndex, textStartIndex);
							originalTextBody.UpdateIndex(num, num2, num2, num2, num3, num3);
							originalTextBody.UpdateMatchIndex();
						}
						if (wTextRange2.Text.Length == textStartIndex + 1 && wTextRange.Text.Length == num3 + 1)
						{
							MoveCurrPosition(originalDocument, num2, num, wParagraph.Items.Count, paraItemIndex, Items.Count, originalTextBody);
							MoveCurrPosition(base.Document, paraItemIndex, Index, Items.Count, num2, wParagraph.Items.Count, (originalTextBody != null) ? base.OwnerTextBody : null);
							paraItemIndex++;
							wordIndex = 0;
							textStartIndex = 0;
							textEndIndex = -1;
							int itemIndex = num2 + 1;
							if (HasOnlyComment(wParagraph, ref itemIndex) && HasOnlyComment(this, ref paraItemIndex))
							{
								if (originalTextBody == null)
								{
									base.Document.UpdateIndex(Index + 1, 0, 0, 0, 0);
									originalDocument.UpdateIndex(num + 1, 0, 0, 0, 0);
									base.Document.currParaItemIndex = paraItemIndex;
								}
								else
								{
									originalTextBody.UpdateIndex(num + 1, 0, 0, 0, 0);
									base.OwnerTextBody.UpdateIndex(Index + 1, 0, 0, 0, 0);
									base.OwnerTextBody.currParaItemIndex = paraItemIndex;
								}
							}
						}
						else if (wTextRange.Text.Length == num3 + 1)
						{
							num2++;
							if (originalTextBody == null)
							{
								originalDocument.UpdateIndex(num, num2, num2, num2, 0);
							}
							else
							{
								originalTextBody.UpdateIndex(num, num2, num2, num2, 0);
							}
						}
						else if (wTextRange2.Text.Length == textStartIndex + 1)
						{
							paraItemIndex++;
							wordIndex = 0;
							textStartIndex = 0;
							textEndIndex = -1;
							if (originalTextBody == null)
							{
								base.Document.UpdateIndex(Index, paraItemIndex, paraItemIndex, paraItemIndex, 0);
							}
							else
							{
								base.OwnerTextBody.UpdateIndex(Index, paraItemIndex, paraItemIndex, paraItemIndex, 0);
							}
						}
					}
					else
					{
						SplitTextBasedOnDelimiter(originalDocument, ref paraItemIndex, ref textStartIndex, ref textEndIndex, ref wordIndex, ref startRangeIndex, wTextRange, space, wParagraph, originalTextBody);
					}
					while (wTextRange.Text.Length == num3 + 1 && wParagraph.ChildEntities.Count > num2 + 1 && wParagraph.ChildEntities[num2 + 1] is WTextRange && !(wParagraph.ChildEntities[num2 + 1] is WField) && (wParagraph.ChildEntities[num2 + 1] as WTextRange).Text.Length == 0)
					{
						num2++;
						if ((wParagraph.ChildEntities[num2] as WTextRange).Index == wParagraph.ChildEntities.Count - 1)
						{
							if (originalTextBody == null)
							{
								originalDocument.UpdateIndex(num + 1, 0, 0, 0, 0);
							}
							else
							{
								originalTextBody.UpdateIndex(num + 1, 0, 0, 0, 0);
							}
						}
						else if (originalTextBody == null)
						{
							originalDocument.UpdateIndex(num, num2 + 1, num2 + 1, num2 + 1, 0);
						}
						else
						{
							originalTextBody.UpdateIndex(num, num2 + 1, num2 + 1, num2 + 1, 0);
						}
					}
					while (wTextRange2.Text.Length == textStartIndex + 1 && ChildEntities.Count > paraItemIndex && ChildEntities[paraItemIndex] is WTextRange && !(ChildEntities[paraItemIndex] is WField) && (ChildEntities[paraItemIndex] as WTextRange).Text.Length == 0)
					{
						if ((ChildEntities[paraItemIndex] as WTextRange).Index == ChildEntities.Count - 1)
						{
							paraItemIndex++;
							if (originalTextBody == null)
							{
								base.Document.UpdateIndex(Index + 1, 0, 0, 0, 0);
							}
							else
							{
								base.OwnerTextBody.UpdateIndex(Index + 1, 0, 0, 0, 0);
							}
							break;
						}
						paraItemIndex++;
						if (originalTextBody == null)
						{
							base.Document.UpdateIndex(Index, paraItemIndex, paraItemIndex, paraItemIndex, 0);
						}
						else
						{
							base.OwnerTextBody.UpdateIndex(Index, paraItemIndex, paraItemIndex, paraItemIndex, 0);
						}
					}
					CompareParaFormattingForDelimiter(ref firstMatchInPara, wParagraph);
					isLastWordMatched = true;
					return;
				}
			}
			SplitRevisedDocumentText(ref textStartIndex, ref startRangeIndex, ref paraItemIndex, ref wordIndex, ref textEndIndex, wordToMatch, originalTextBody != null);
			if (wTextRange != null && !(wTextRange is WField) && num3 < wTextRange.Text.Length)
			{
				string text = wTextRange.Text;
				WTextRange wTextRange3 = wTextRange.Clone() as WTextRange;
				wTextRange.Text = text.Remove(num3);
				wTextRange3.Text = text.Substring(num3);
				wParagraph.Items.Insert(num2 + 1, wTextRange3);
				int num4 = num2 + 1;
				if (originalTextBody == null)
				{
					originalDocument.UpdateIndex(num, num4, num4, num4, 0);
				}
				else
				{
					originalTextBody.UpdateIndex(num, num4, num4, num4, 0);
				}
			}
		}
		else
		{
			SplitRevisedDocumentText(ref textStartIndex, ref startRangeIndex, ref paraItemIndex, ref wordIndex, ref textEndIndex, wordToMatch, originalTextBody != null);
		}
		isLastWordMatched = false;
	}

	private ParagraphItem SkipNonRenderableItems(WordDocument originalDocument, WParagraph paragraph, WTextBody orgTextBody)
	{
		ParagraphItem paragraphItem = null;
		if (orgTextBody == null)
		{
			paragraphItem = paragraph.Items[originalDocument.m_paraItemIndex];
			while (paragraphItem is BookmarkStart || paragraphItem is BookmarkEnd || paragraphItem is EditableRangeStart || paragraphItem is EditableRangeEnd || paragraphItem is WComment || paragraphItem is WCommentMark)
			{
				originalDocument.m_paraItemIndex++;
				if (originalDocument.m_paraItemIndex >= paragraph.Items.Count)
				{
					return null;
				}
				paragraphItem = paragraph.Items[originalDocument.m_paraItemIndex];
			}
		}
		else
		{
			paragraphItem = paragraph.Items[orgTextBody.m_paraItemIndex];
			while (paragraphItem is BookmarkStart || paragraphItem is BookmarkEnd || paragraphItem is EditableRangeStart || paragraphItem is EditableRangeEnd || paragraphItem is WComment || paragraphItem is WCommentMark)
			{
				orgTextBody.m_paraItemIndex++;
				if (orgTextBody.m_paraItemIndex >= paragraph.Items.Count)
				{
					return null;
				}
				paragraphItem = paragraph.Items[orgTextBody.m_paraItemIndex];
			}
		}
		return paragraphItem;
	}

	private WTextRange SplitSeparateTextrangeForDelimiter(WTextRange textRange, char delimiter, ref int currRangeIndex, WParagraph paragraph, int delimiterIndex)
	{
		if (string.IsNullOrEmpty(textRange.Text) || textRange.Text == delimiter.ToString())
		{
			currRangeIndex++;
			return textRange;
		}
		int num = textRange.Text.Length - 1;
		WTextRange result = null;
		if (delimiterIndex == 0 && delimiterIndex < num)
		{
			WTextRange wTextRange = textRange.Clone() as WTextRange;
			wTextRange.Text = textRange.Text.Substring(delimiterIndex + 1);
			paragraph.Items.Insert(currRangeIndex + 1, wTextRange);
			textRange.Text = textRange.Text[delimiterIndex].ToString();
			result = textRange;
			currRangeIndex++;
		}
		else if (delimiterIndex > 0 && delimiterIndex == num)
		{
			WTextRange wTextRange2 = textRange.Clone() as WTextRange;
			wTextRange2.Text = textRange.Text[delimiterIndex].ToString();
			result = wTextRange2;
			paragraph.Items.Insert(currRangeIndex + 1, wTextRange2);
			textRange.Text = textRange.Text.Substring(0, delimiterIndex--);
			currRangeIndex += 2;
		}
		else if (delimiterIndex > 0 && delimiterIndex < num)
		{
			WTextRange wTextRange3 = textRange.Clone() as WTextRange;
			wTextRange3.Text = textRange.Text[delimiterIndex].ToString();
			result = wTextRange3;
			paragraph.Items.Insert(currRangeIndex + 1, wTextRange3);
			WTextRange wTextRange4 = textRange.Clone() as WTextRange;
			wTextRange4.Text = textRange.Text.Substring(delimiterIndex + 1);
			paragraph.Items.Insert(currRangeIndex + 2, wTextRange4);
			textRange.Text = textRange.Text.Substring(0, delimiterIndex);
			currRangeIndex += 2;
		}
		return result;
	}

	private void SplitTextBasedOnDelimiter(WordDocument originalDocument, ref int paraItemIndex, ref int textStartIndex, ref int textEndIndex, ref int textIndex, ref int startRangeIndex, WTextRange endRange, char space, WParagraph paragraph, WTextBody originalTextBody = null)
	{
		WTextRange wTextRange = null;
		wTextRange = ((originalTextBody != null) ? SplitSeparateTextrangeForDelimiter(endRange, space, ref originalTextBody.m_endRangeIndex, paragraph, originalTextBody.m_textEndIndex + 1) : SplitSeparateTextrangeForDelimiter(endRange, space, ref originalDocument.m_endRangeIndex, paragraph, originalDocument.m_textEndIndex + 1));
		WTextRange wTextRange2 = SplitSeparateTextrangeForDelimiter(Items[paraItemIndex] as WTextRange, space, ref startRangeIndex, Items.Owner as WParagraph, textStartIndex);
		CompareCharacterFormat(wTextRange.CharacterFormat, wTextRange2.CharacterFormat);
		bool flag = false;
		if (originalTextBody == null)
		{
			int endRangeIndex = originalDocument.m_endRangeIndex;
			originalDocument.UpdateIndex(originalDocument.m_bodyItemIndex, endRangeIndex, endRangeIndex, endRangeIndex, 0);
			originalDocument.UpdateMatchIndex();
			WParagraph wParagraph = originalDocument.Sections[originalDocument.m_sectionIndex].Body.Items[originalDocument.m_bodyItemIndex] as WParagraph;
			if (originalDocument.m_paraItemIndex >= wParagraph.Items.Count || (originalDocument.m_paraItemIndex == wParagraph.Items.Count - 1 && wParagraph.Items[originalDocument.m_paraItemIndex] is WTextRange && (wParagraph.Items[originalDocument.m_paraItemIndex] as WTextRange).TextLength == 0))
			{
				flag = true;
			}
			base.Document.UpdateIndex(base.Document.m_bodyItemIndex, startRangeIndex, startRangeIndex, startRangeIndex, 0);
			wParagraph = base.Document.Sections[base.Document.m_sectionIndex].Body.Items[base.Document.m_bodyItemIndex] as WParagraph;
			if ((base.Document.m_paraItemIndex >= wParagraph.Items.Count || (base.Document.m_paraItemIndex == wParagraph.Items.Count - 1 && wParagraph.Items[base.Document.m_paraItemIndex] is WTextRange && (wParagraph.Items[base.Document.m_paraItemIndex] as WTextRange).TextLength == 0)) && flag)
			{
				base.Document.UpdateIndex(base.Document.m_bodyItemIndex + 1, 0, 0, 0, 0);
				originalDocument.UpdateIndex(originalDocument.m_bodyItemIndex + 1, 0, 0, 0, 0);
			}
		}
		else
		{
			int endRangeIndex2 = originalTextBody.m_endRangeIndex;
			originalTextBody.UpdateIndex(originalTextBody.m_bodyItemIndex, endRangeIndex2, endRangeIndex2, endRangeIndex2, 0);
			originalTextBody.UpdateMatchIndex();
			WParagraph wParagraph2 = originalTextBody.Items[originalTextBody.m_bodyItemIndex] as WParagraph;
			if (originalTextBody.m_paraItemIndex >= wParagraph2.Items.Count || (originalTextBody.m_paraItemIndex == wParagraph2.Items.Count - 1 && wParagraph2.Items[originalTextBody.m_paraItemIndex] is WTextRange && (wParagraph2.Items[originalTextBody.m_paraItemIndex] as WTextRange).TextLength == 0))
			{
				flag = true;
			}
			base.OwnerTextBody.UpdateIndex(base.OwnerTextBody.m_bodyItemIndex, startRangeIndex, startRangeIndex, startRangeIndex, 0);
			wParagraph2 = base.OwnerTextBody.Items[base.OwnerTextBody.m_bodyItemIndex] as WParagraph;
			if ((base.OwnerTextBody.m_paraItemIndex >= wParagraph2.Items.Count || (base.OwnerTextBody.m_paraItemIndex == wParagraph2.Items.Count - 1 && wParagraph2.Items[base.OwnerTextBody.m_paraItemIndex] is WTextRange && (wParagraph2.Items[base.OwnerTextBody.m_paraItemIndex] as WTextRange).TextLength == 0)) && flag)
			{
				base.OwnerTextBody.UpdateIndex(base.OwnerTextBody.m_bodyItemIndex + 1, 0, 0, 0, 0);
				originalTextBody.UpdateIndex(originalTextBody.m_bodyItemIndex + 1, 0, 0, 0, 0);
			}
		}
		paraItemIndex = startRangeIndex;
		if (paraItemIndex == Items.Count - 1 && (Items[paraItemIndex] as WTextRange).TextLength == 0)
		{
			paraItemIndex++;
		}
		textIndex = 0;
		textStartIndex = 0;
		textEndIndex = -1;
	}

	private void SkipParaItemIndexForBookmarks(WParagraph paragraph, ref int paraItemIndex)
	{
		if (paraItemIndex >= paragraph.Items.Count)
		{
			return;
		}
		ParagraphItem paragraphItem = paragraph.Items[paraItemIndex];
		while (paragraphItem is BookmarkEnd || paragraphItem is BookmarkStart)
		{
			paraItemIndex++;
			if (paraItemIndex < paragraph.Items.Count)
			{
				paragraphItem = paragraph.Items[paraItemIndex];
				continue;
			}
			paraItemIndex--;
			break;
		}
	}

	internal void SkipBookmarks(WParagraph paragraph, ref int paraItemIndex)
	{
		ParagraphItem paragraphItem = paragraph.Items[paraItemIndex];
		while (paragraphItem is BookmarkEnd || paragraphItem is BookmarkStart)
		{
			paraItemIndex--;
			if (paraItemIndex > -1)
			{
				paragraphItem = paragraph.Items[paraItemIndex];
				continue;
			}
			paraItemIndex++;
			break;
		}
	}

	internal StringBuilder GetAsString(ParagraphItemCollection collection)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ParagraphItem item in collection)
		{
			stringBuilder.Append(GetAsString(item));
		}
		return stringBuilder;
	}

	internal StringBuilder GetAsString(ParagraphItem item)
	{
		StringBuilder stringBuilder = new StringBuilder();
		switch (item.EntityType)
		{
		case EntityType.TextRange:
			stringBuilder.Append((item as WTextRange).Text);
			break;
		case EntityType.Field:
		case EntityType.MergeField:
		case EntityType.SeqField:
		case EntityType.EmbededField:
		case EntityType.ControlField:
		case EntityType.TextFormField:
		case EntityType.DropDownFormField:
		case EntityType.CheckBox:
			stringBuilder.Append((item as WField).GetAsString(traverseTillSeparator: false));
			if ((item as WField).FieldType == FieldType.FieldFormTextInput || (item as WField).FieldType == FieldType.FieldFormDropDown || (item as WField).FieldType == FieldType.FieldFormCheckBox)
			{
				stringBuilder.Append((item as WFormField).GetAsString().ToString());
			}
			break;
		case EntityType.InlineContentControl:
			stringBuilder.Append((item as InlineContentControl).GetAsString());
			break;
		case EntityType.AutoShape:
			stringBuilder.Append((item as Shape).GetAsString());
			break;
		case EntityType.GroupShape:
			stringBuilder.Append((item as GroupShape).GetAsString());
			break;
		case EntityType.Picture:
			stringBuilder.Append((item as WPicture).GetAsString());
			break;
		case EntityType.OleObject:
			stringBuilder.Append((item as WOleObject).GetAsString());
			break;
		case EntityType.TextBox:
			stringBuilder.Append((item as WTextBox).GetAsString());
			break;
		case EntityType.Break:
			stringBuilder.Append((item as Break).GetAsString());
			break;
		case EntityType.Symbol:
			stringBuilder.Append((item as WSymbol).GetAsString());
			break;
		case EntityType.Chart:
			stringBuilder.Append((item as WChart).GetAsString());
			break;
		case EntityType.Math:
			stringBuilder.Append((item as WMath).GetAsString());
			break;
		}
		return stringBuilder;
	}
}

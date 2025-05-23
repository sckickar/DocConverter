using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfLoadedPopupAnnotation : PdfLoadedStyledAnnotation
{
	private PdfCrossTable m_crossTable;

	private bool m_open;

	private PdfPopupIcon m_name;

	private PdfAnnotationState m_state;

	private PdfAnnotationStateModel m_statemodel;

	private string m_iconName = string.Empty;

	private bool m_isCustomIconEnabled;

	public PdfLoadedPopupAnnotationCollection ReviewHistory
	{
		get
		{
			if (m_reviewHistory == null)
			{
				m_reviewHistory = new PdfLoadedPopupAnnotationCollection(base.Page, base.Dictionary, isReview: true);
			}
			return m_reviewHistory;
		}
	}

	public PdfLoadedPopupAnnotationCollection Comments
	{
		get
		{
			if (m_comments == null)
			{
				m_comments = new PdfLoadedPopupAnnotationCollection(base.Page, base.Dictionary, isReview: false);
			}
			return m_comments;
		}
	}

	public PdfAnnotationState State
	{
		get
		{
			return ObtainState();
		}
		set
		{
			m_state = value;
			base.Dictionary.SetProperty("State", new PdfString(m_state.ToString()));
			NotifyPropertyChanged("State");
		}
	}

	public PdfAnnotationStateModel StateModel
	{
		get
		{
			return ObtainStateModel();
		}
		set
		{
			m_statemodel = value;
			base.Dictionary.SetProperty("StateModel", new PdfString(m_statemodel.ToString()));
			NotifyPropertyChanged("StateModel");
		}
	}

	public bool Open
	{
		get
		{
			return ObtainOpen();
		}
		set
		{
			m_open = value;
			base.Dictionary.SetBoolean("Open", m_open);
			NotifyPropertyChanged("Open");
		}
	}

	public PdfPopupIcon Icon
	{
		get
		{
			return ObtainIcon();
		}
		set
		{
			m_name = value;
			base.Dictionary.SetName("Name", m_name.ToString());
			NotifyPropertyChanged("Icon");
		}
	}

	public string IconName
	{
		get
		{
			m_iconName = ObtainIconName();
			return m_iconName;
		}
		set
		{
			m_iconName = value;
			Icon = GetIconName(m_iconName);
			if (m_iconName != null)
			{
				base.Dictionary["Name"] = new PdfName(PdfName.EncodeName(m_iconName));
			}
			m_isCustomIconEnabled = true;
			NotifyPropertyChanged("IconName");
		}
	}

	internal PdfLoadedPopupAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF rectangle, string text)
		: base(dictionary, crossTable)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		base.Dictionary = dictionary;
		m_crossTable = crossTable;
		m_text = text;
	}

	internal string ObtainIconName()
	{
		string result = string.Empty;
		if (base.Dictionary.ContainsKey("Name"))
		{
			if (base.Dictionary["Name"] is PdfName)
			{
				PdfName pdfName = base.Dictionary["Name"] as PdfName;
				if (pdfName != null)
				{
					result = PdfName.DecodeName(pdfName.Value);
				}
			}
			else if (base.Dictionary["Name"] is PdfString && base.Dictionary["Name"] is PdfString pdfString)
			{
				result = pdfString.Value.ToString();
			}
		}
		return result;
	}

	private bool ObtainOpen()
	{
		bool result = false;
		if (base.Dictionary.ContainsKey("Open"))
		{
			result = (base.Dictionary["Open"] as PdfBoolean).Value;
		}
		return result;
	}

	private PdfPopupIcon ObtainIcon()
	{
		PdfPopupIcon result = PdfPopupIcon.Note;
		if (base.Dictionary.ContainsKey("Name"))
		{
			if (base.Dictionary["Name"] is PdfName)
			{
				PdfName pdfName = base.Dictionary["Name"] as PdfName;
				result = GetIconName(pdfName.Value.ToString());
			}
			else if (base.Dictionary["Name"] is PdfString)
			{
				PdfString pdfString = base.Dictionary["Name"] as PdfString;
				result = GetIconName(pdfString.Value.ToString());
			}
		}
		return result;
	}

	internal override void FlattenAnnot(bool flattenPopUps)
	{
		SaveAndFlatten(isExternalFlatten: true, flattenPopUps);
	}

	private void SaveAndFlatten(bool isExternalFlatten, bool isExternalFlattenPopUps)
	{
		if (base.SetAppearanceDictionary)
		{
			CreateApperance();
		}
		if (!(base.Flatten || base.Page.Annotations.Flatten || isExternalFlatten))
		{
			return;
		}
		if (!base.Dictionary.ContainsKey("AP"))
		{
			CreateApperance();
		}
		bool flag = true;
		if (base.Dictionary.ContainsKey("F") && base.Dictionary["F"] is PdfNumber { IntValue: 30 } && (!base.FlattenPopUps || !isExternalFlattenPopUps))
		{
			flag = false;
		}
		if (base.Dictionary["AP"] != null && flag && PdfCrossTable.Dereference(base.Dictionary["AP"]) is PdfDictionary pdfDictionary && PdfCrossTable.Dereference(pdfDictionary["N"]) is PdfDictionary pdfDictionary2 && pdfDictionary2 is PdfStream template)
		{
			PdfTemplate pdfTemplate = new PdfTemplate(template);
			if (pdfTemplate != null)
			{
				PdfGraphics pdfGraphics = ObtainlayerGraphics();
				PdfGraphicsState state = base.Page.Graphics.Save();
				if (Opacity < 1f)
				{
					base.Page.Graphics.SetTransparency(Opacity);
				}
				if (pdfGraphics != null)
				{
					pdfGraphics.DrawPdfTemplate(pdfTemplate, Bounds.Location);
				}
				else
				{
					base.Page.Graphics.DrawPdfTemplate(pdfTemplate, Bounds.Location);
				}
				base.Page.Graphics.Restore(state);
			}
		}
		RemoveAnnoationFromPage(base.Page, this);
		if (base.FlattenPopUps || isExternalFlattenPopUps)
		{
			FlattenLoadedPopup();
		}
		if (Popup != null)
		{
			RemoveAnnoationFromPage(base.Page, Popup);
		}
	}

	protected override void Save()
	{
		PdfPageBase page = base.Page;
		if (page.Annotations.Flatten)
		{
			base.Page.Annotations.Flatten = page.Annotations.Flatten;
		}
		SaveAndFlatten(isExternalFlatten: false, isExternalFlattenPopUps: false);
	}

	internal void ChangeBounds(RectangleF bounds)
	{
		if (base.Dictionary["Popup"] != null && PdfCrossTable.Dereference(base.Dictionary["Popup"]) is PdfDictionary pdfDictionary)
		{
			PdfArray pdfArray = PdfCrossTable.Dereference(pdfDictionary["Rect"]) as PdfArray;
			if (pdfArray != null)
			{
				(m_crossTable.GetObject(pdfArray[0]) as PdfNumber).FloatValue = bounds.X;
				(m_crossTable.GetObject(pdfArray[1]) as PdfNumber).FloatValue = base.Page.Size.Height - (bounds.Y + bounds.Height);
				(m_crossTable.GetObject(pdfArray[2]) as PdfNumber).FloatValue = bounds.X + bounds.Width;
				(m_crossTable.GetObject(pdfArray[3]) as PdfNumber).FloatValue = base.Page.Size.Height - bounds.Y;
			}
			pdfDictionary.SetProperty("Rect", pdfArray);
		}
	}

	protected void CreateApperance()
	{
		if (m_appearance == null)
		{
			PdfGraphics graphics = base.Appearance.Normal.Graphics;
			PdfGraphics graphics2 = base.Appearance.MouseHover.Graphics;
			graphics.StreamWriter.Clear();
			graphics2.StreamWriter.Clear();
			PdfGraphicsState state = null;
			PdfGraphicsState state2 = null;
			if (Opacity < 1f)
			{
				state = graphics.Save();
				state2 = graphics2.Save();
				graphics.SetTransparency(Opacity);
				graphics2.SetTransparency(Opacity);
			}
			if (IconName == "Checkmark")
			{
				graphics.StreamWriter.Write("q 0.396 0.396 0.396 rg 1 0 0 1 13.5151 16.5 cm 0 0 m -6.7 -10.23 l -8.81 -7 l -13.22 -7 l -6.29 -15 l 4.19 0 l h f Q ");
				graphics2.StreamWriter.Write("q 0.396 0.396 0.396 rg 1 0 0 1 13.5151 16.5 cm 0 0 m -6.7 -10.23 l -8.81 -7 l -13.22 -7 l -6.29 -15 l 4.19 0 l h f Q ");
			}
			else
			{
				switch (Icon)
				{
				case PdfPopupIcon.Comment:
					graphics.StreamWriter.Write("q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d 1 0 0 1 9 5.0908 cm 7.74 12.616 m -7.74 12.616 l -8.274 12.616 -8.707 12.184 -8.707 11.649 c -8.707 -3.831 l -8.707 -4.365 -8.274 -4.798 -7.74 -4.798 c 7.74 -4.798 l 8.274 -4.798 8.707 -4.365 8.707 -3.831 c 8.707 11.649 l 8.707 12.184 8.274 12.616 7.74 12.616 c h f Q 0 G ");
					graphics.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics.StreamWriter.Write(" 0 i 0.60 w 4 M 1 j 0 J [0 100]1 d  1 0 0 1 9 5.0908 cm 1 0 m -2.325 -2.81 l  -2.325 0 l  -5.72 0 l  -5.72 8.94 l  5.51 8.94 l  5.51 0 l  1 0 l -3.50 5.01 m -3.50 5.59 l 3.29 5.59 l 3.29 5.01 l -3.50 5.01 l -3.50 3.34 m -3.50 3.92 l 2.27 3.92 l 2.27 3.34 l -3.50 3.34 l 7.74 12.616 m -7.74 12.616 l -8.274 12.616 -8.707 12.184 -8.707 11.649 c -8.707 -3.831 l -8.707 -4.365 -8.274 -4.798 -7.74 -4.798 c 7.74 -4.798 l 8.274 -4.798 8.707 -4.365 8.707 -3.831 c 8.707 11.649 l 8.707 12.184 8.274 12.616 7.74 12.616 c b ");
					graphics2.StreamWriter.Write("q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d 1 0 0 1 9 5.0908 cm 7.74 12.616 m -7.74 12.616 l -8.274 12.616 -8.707 12.184 -8.707 11.649 c h f Q 0 G ");
					graphics2.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics2.StreamWriter.Write(" 0 i 0.60 w 4 M 1 j 0 J [0 100]1 d  1 0 0 1 9 5.0908 cm 4.1 1.71 m -0.54 -2.29 l  -0.54 1.71 l  -5.5 1.71 l  -5.5 14.42 l  10.5 14.42 l  10.5 1.71 l  4.1 1.71 l -2.33 9.66 m 7.34 9.66 l 7.34 8.83 l -2.33 8.83 l -2.33 9.66 l -2.33 7.28 m 5.88 7.28 l 5.88 6.46 l -2.33 6.46 l -2.33 7.28 l 14.9 23.1235 m -14.9 23.1235 l -14.9 -20.345 l 14.9 -20.345 l 14.9 23.1235 l b ");
					break;
				case PdfPopupIcon.Paragraph:
					graphics.StreamWriter.Write("q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d 1 0 0 1 19.6973 10.0005 cm 0 0 m 0 -5.336 -4.326 -9.662 -9.663 -9.662 c -14.998 -9.662 -19.324 -5.336 -19.324 0 c -19.324 5.335 -14.998 9.662 -9.663 9.662 c -4.326 9.662 0 5.335 0 0 c h f Q ");
					graphics.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics.StreamWriter.Write("0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  q 1 0 0 1 19.6973 10.0005 cm 0 0 m 0 -5.336 -4.326 -9.662 -9.663 -9.662 c -14.998 -9.662 -19.324 -5.336 -19.324 0 c -19.324 5.335 -14.998 9.662 -9.663 9.662 c -4.326 9.662 0 5.335 0 0 c h S Q q 1 0 0 1 11.6787 2.6582 cm 0 0 m -1.141 0 l -1.227 0 -1.244 0.052 -1.227 0.139 c -0.656 1.157 -0.52 2.505 -0.52 3.317 c -0.52 3.594 l -2.833 3.783 -5.441 4.838 -5.441 8.309 c -5.441 10.778 -3.714 12.626 -0.57 13.024 c -0.535 13.508 -0.381 14.129 -0.242 14.389 c -0.207 14.44 -0.174 14.475 -0.104 14.475 c 1.088 14.475 l 1.156 14.475 1.191 14.458 1.175 14.372 c 1.105 14.095 0.881 13.127 0.881 12.402 c 0.881 9.431 0.932 7.324 0.95 4.06 c 0.95 2.298 0.708 0.813 0.189 0.07 c 0.155 0.034 0.103 0 0 0 c b Q ");
					graphics2.StreamWriter.Write("q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d 1 0 0 1 19.6973 10.0005 cm 0 0 m 0 -5.336 -4.326 -9.662 -9.663 -9.662 c -14.998 -9.662 -19.324 -5.336 -19.324 0 c -19.324 5.335 -14.998 9.662 -9.663 9.662 c -4.326 9.662 0 5.335 0 0 c h f Q ");
					graphics2.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics2.StreamWriter.Write("0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  q 1 0 0 1 19.6973 10.0005 cm 0 0 m 0 -5.336 -4.326 -9.662 -9.663 -9.662 c -14.998 -9.662 -19.324 -5.336 -19.324 0 c -19.324 5.335 -14.998 9.662 -9.663 9.662 c -4.326 9.662 0 5.335 0 0 c h S Q q 1 0 0 1 11.6787 2.6582 cm 0 0 m -1.141 0 l -1.227 0 -1.244 0.052 -1.227 0.139 c -0.656 1.157 -0.52 2.505 -0.52 3.317 c -0.52 3.594 l -2.833 3.783 -5.441 4.838 -5.441 8.309 c -5.441 10.778 -3.714 12.626 -0.57 13.024 c -0.535 13.508 -0.381 14.129 -0.242 14.389 c -0.207 14.44 -0.174 14.475 -0.104 14.475 c 1.088 14.475 l 1.156 14.475 1.191 14.458 1.175 14.372 c 1.105 14.095 0.881 13.127 0.881 12.402 c 0.881 9.431 0.932 7.324 0.95 4.06 c 0.95 2.298 0.708 0.813 0.189 0.07 c 0.155 0.034 0.103 0 0 0 c b Q ");
					break;
				case PdfPopupIcon.Help:
					graphics.StreamWriter.Write("q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d 1 0 0 1 12.1465 10.5137 cm -2.146 9.403 m -7.589 9.403 -12.001 4.99 -12.001 -0.453 c -12.001 -5.895 -7.589 -10.309 -2.146 -10.309 c 3.296 -10.309 7.709 -5.895 7.709 -0.453 c 7.709 4.99 3.296 9.403 -2.146 9.403 c h f Q ");
					graphics.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics.StreamWriter.Write(" 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 12.1465 10.5137 cm 0 0 m -0.682 -0.756 -0.958 -1.472 -0.938 -2.302 c -0.938 -2.632 l -3.385 -2.632 l -3.403 -2.154 l -3.459 -1.216 -3.147 -0.259 -2.316 0.716 c -1.729 1.433 -1.251 2.022 -1.251 2.647 c -1.251 3.291 -1.674 3.715 -2.594 3.751 c -3.202 3.751 -3.937 3.531 -4.417 3.2 c -5.041 5.205 l -4.361 5.591 -3.274 5.959 -1.968 5.959 c 0.46 5.959 1.563 4.616 1.563 3.089 c 1.563 1.691 0.699 0.771 0 0 c -2.227 -6.863 m -2.245 -6.863 l -3.202 -6.863 -3.864 -6.146 -3.864 -5.189 c -3.864 -4.196 -3.182 -3.516 -2.227 -3.516 c -1.233 -3.516 -0.589 -4.196 -0.57 -5.189 c -0.57 -6.146 -1.233 -6.863 -2.227 -6.863 c -2.146 9.403 m -7.589 9.403 -12.001 4.99 -12.001 -0.453 c -12.001 -5.895 -7.589 -10.309 -2.146 -10.309 c 3.296 -10.309 7.709 -5.895 7.709 -0.453 c 7.709 4.99 3.296 9.403 -2.146 9.403 c b ");
					graphics2.StreamWriter.Write("q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d 1 0 0 1 12.1465 10.5137 cm -2.146 9.403 m -7.589 9.403 -12.001 4.99 -12.001 -0.453 c -12.001 -5.895 -7.589 -10.309 -2.146 -10.309 c 3.296 -10.309 7.709 -5.895 7.709 -0.453 c 7.709 4.99 3.296 9.403 -2.146 9.403 c h f Q ");
					graphics2.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics2.StreamWriter.Write(" 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 12.1465 10.5137 cm 0 0 m -0.682 -0.756 -0.958 -1.472 -0.938 -2.302 c -0.938 -2.632 l -3.385 -2.632 l -3.403 -2.154 l -3.459 -1.216 -3.147 -0.259 -2.316 0.716 c -1.729 1.433 -1.251 2.022 -1.251 2.647 c -1.251 3.291 -1.674 3.715 -2.594 3.751 c -3.202 3.751 -3.937 3.531 -4.417 3.2 c -5.041 5.205 l -4.361 5.591 -3.274 5.959 -1.968 5.959 c 0.46 5.959 1.563 4.616 1.563 3.089 c 1.563 1.691 0.699 0.771 0 0 c -2.227 -6.863 m -2.245 -6.863 l -3.202 -6.863 -3.864 -6.146 -3.864 -5.189 c -3.864 -4.196 -3.182 -3.516 -2.227 -3.516 c -1.233 -3.516 -0.589 -4.196 -0.57 -5.189 c -0.57 -6.146 -1.233 -6.863 -2.227 -6.863 c -2.146 9.403 m -7.589 9.403 -12.001 4.99 -12.001 -0.453 c -12.001 -5.895 -7.589 -10.309 -2.146 -10.309 c 3.296 -10.309 7.709 -5.895 7.709 -0.453 c 7.709 4.99 3.296 9.403 -2.146 9.403 c b ");
					break;
				case PdfPopupIcon.Note:
					graphics.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics.StreamWriter.Write(" 0 G 0 i 0.61 w 4 M 0 j 0 J []0 d  q 1 0 0 1 16.959 1.3672 cm 0 0 m 0 -0.434 -0.352 -0.785 -0.784 -0.785 c -14.911 -0.785 l -15.345 -0.785 -15.696 -0.434 -15.696 0 c -15.696 17.266 l -15.696 17.699 -15.345 18.051 -14.911 18.051 c -0.784 18.051 l -0.352 18.051 0 17.699 0 17.266 c h b Q q 1 0 0 1 4.4023 13.9243 cm 0 0 m 9.418 0 l S Q q 1 0 0 1 4.4019 11.2207 cm 0 0 m 9.418 0 l S Q q 1 0 0 1 4.4023 8.5176 cm 0 0 m 9.418 0 l S Q q 1 0 0 1 4.4023 5.8135 cm 0 0 m 9.418 0 l S Q ");
					graphics2.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics2.StreamWriter.Write(" 0 G 0 i 0.61 w 4 M 0 j 0 J []0 d  q 1 0 0 1 16.959 1.3672 cm 0 0 m 0 -0.434 -0.352 -0.785 -0.784 -0.785 c -14.911 -0.785 l -15.345 -0.785 -15.696 -0.434 -15.696 0 c -15.696 17.266 l -15.696 17.699 -15.345 18.051 -14.911 18.051 c -0.784 18.051 l -0.352 18.051 0 17.699 0 17.266 c h b Q q 1 0 0 1 4.4023 13.9243 cm 0 0 m 9.418 0 l S Q q 1 0 0 1 4.4019 11.2207 cm 0 0 m 9.418 0 l S Q q 1 0 0 1 4.4023 8.5176 cm 0 0 m 9.418 0 l S Q q 1 0 0 1 4.4023 5.8135 cm 0 0 m 9.418 0 l S Q ");
					break;
				case PdfPopupIcon.Insert:
					graphics.StreamWriter.Write("0 G ");
					graphics.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics.StreamWriter.Write(" 0 i 0.59 w 4 M 0 j 0 J []0 d  1 0 0 1 8.5386 19.8545 cm 0 0 m -8.39 -19.719 l 8.388 -19.719 l h B ");
					graphics2.StreamWriter.Write("0 G ");
					graphics2.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics2.StreamWriter.Write(" 0 i 0.59 w 4 M 0 j 0 J []0 d  1 0 0 1 8.5386 19.8545 cm 0 0 m -8.39 -19.719 l 8.388 -19.719 l h B ");
					break;
				case PdfPopupIcon.Key:
					graphics.StreamWriter.Write("q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d 1 0 0 1 6.5 12.6729 cm 0.001 5.138 m -2.543 5.138 -4.604 3.077 -4.604 0.534 c -4.604 -1.368 -3.449 -3.001 -1.802 -3.702 c -1.802 -4.712 l -0.795 -5.719 l -1.896 -6.82 l -0.677 -8.039 l -1.595 -8.958 l -0.602 -9.949 l -1.479 -10.829 l -0.085 -12.483 l 1.728 -10.931 l 1.728 -3.732 l 1.737 -3.728 1.75 -3.724 1.76 -3.721 c 3.429 -3.03 4.604 -1.385 4.604 0.534 c 4.604 3.077 2.542 5.138 0.001 5.138 c f Q ");
					graphics.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics.StreamWriter.Write(" 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 6.5 12.6729 cm 0 0 m -1.076 0 -1.95 0.874 -1.95 1.95 c -1.95 3.028 -1.076 3.306 0 3.306 c 1.077 3.306 1.95 3.028 1.95 1.95 c 1.95 0.874 1.077 0 0 0 c 0.001 5.138 m -2.543 5.138 -4.604 3.077 -4.604 0.534 c -4.604 -1.368 -3.449 -3.001 -1.802 -3.702 c -1.802 -4.712 l -0.795 -5.719 l -1.896 -6.82 l -0.677 -8.039 l -1.595 -8.958 l -0.602 -9.949 l -1.479 -10.829 l -0.085 -12.483 l 1.728 -10.931 l 1.728 -3.732 l 1.737 -3.728 1.75 -3.724 1.76 -3.721 c 3.429 -3.03 4.604 -1.385 4.604 0.534 c 4.604 3.077 2.542 5.138 0.001 5.138 c b ");
					graphics2.StreamWriter.Write("q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d 1 0 0 1 6.5 12.6729 cm 0.001 5.138 m -2.543 5.138 -4.604 3.077 -4.604 0.534 c -4.604 -1.368 -3.449 -3.001 -1.802 -3.702 c -1.802 -4.712 l -0.795 -5.719 l -1.896 -6.82 l -0.677 -8.039 l -1.595 -8.958 l -0.602 -9.949 l -1.479 -10.829 l -0.085 -12.483 l 1.728 -10.931 l 1.728 -3.732 l 1.737 -3.728 1.75 -3.724 1.76 -3.721 c 3.429 -3.03 4.604 -1.385 4.604 0.534 c 4.604 3.077 2.542 5.138 0.001 5.138 c f Q ");
					graphics2.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics2.StreamWriter.Write(" 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 6.5 12.6729 cm 0 0 m -1.076 0 -1.95 0.874 -1.95 1.95 c -1.95 3.028 -1.076 3.306 0 3.306 c 1.077 3.306 1.95 3.028 1.95 1.95 c 1.95 0.874 1.077 0 0 0 c 0.001 5.138 m -2.543 5.138 -4.604 3.077 -4.604 0.534 c -4.604 -1.368 -3.449 -3.001 -1.802 -3.702 c -1.802 -4.712 l -0.795 -5.719 l -1.896 -6.82 l -0.677 -8.039 l -1.595 -8.958 l -0.602 -9.949 l -1.479 -10.829 l -0.085 -12.483 l 1.728 -10.931 l 1.728 -3.732 l 1.737 -3.728 1.75 -3.724 1.76 -3.721 c 3.429 -3.03 4.604 -1.385 4.604 0.534 c 4.604 3.077 2.542 5.138 0.001 5.138 c b ");
					break;
				case PdfPopupIcon.NewParagraph:
					graphics.StreamWriter.Write("1 0.819611 0 rg 0 G 0 i 0.58 w 4 M 0 j 0 J []0 d ");
					graphics.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics.StreamWriter.Write(" 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  q 1 0 0 1 6.4995 20 cm 0 0 m -6.205 -12.713 l 6.205 -12.713 l h b Q q 1 0 0 1 1.1909 6.2949 cm 0 0 m 1.278 0 l 1.353 0 1.362 -0.02 1.391 -0.066 c 2.128 -1.363 3.78 -4.275 3.966 -4.713 c 3.985 -4.713 l 3.976 -4.453 3.957 -3.91 3.957 -3.137 c 3.957 -0.076 l 3.957 -0.02 3.976 0 4.041 0 c 4.956 0 l 5.021 0 5.04 -0.029 5.04 -0.084 c 5.04 -6.049 l 5.04 -6.113 5.021 -6.133 4.947 -6.133 c 3.695 -6.133 l 3.621 -6.133 3.611 -6.113 3.574 -6.066 c 3.052 -4.955 1.353 -2.063 0.971 -1.186 c 0.961 -1.186 l 0.999 -1.68 0.999 -2.146 1.008 -3.025 c 1.008 -6.049 l 1.008 -6.104 0.989 -6.133 0.933 -6.133 c 0.009 -6.133 l -0.046 -6.133 -0.075 -6.123 -0.075 -6.049 c -0.075 -0.066 l -0.075 -0.02 -0.056 0 0 0 c f Q q 1 0 0 1 9.1367 3.0273 cm 0 0 m 0.075 0 0.215 -0.008 0.645 -0.008 c 1.4 -0.008 2.119 0.281 2.119 1.213 c 2.119 1.969 1.633 2.381 0.737 2.381 c 0.354 2.381 0.075 2.371 0 2.361 c h -1.146 3.201 m -1.146 3.238 -1.129 3.268 -1.082 3.268 c -0.709 3.275 0.02 3.285 0.729 3.285 c 2.613 3.285 3.248 2.314 3.258 1.232 c 3.258 -0.27 2.007 -0.914 0.607 -0.914 c 0.327 -0.914 0.057 -0.914 0 -0.904 c 0 -2.789 l 0 -2.836 -0.019 -2.865 -0.074 -2.865 c -1.082 -2.865 l -1.119 -2.865 -1.146 -2.846 -1.146 -2.799 c h f Q ");
					graphics2.StreamWriter.Write("1 0.819611 0 rg 0 G 0 i 0.58 w 4 M 0 j 0 J []0 d ");
					graphics2.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics2.StreamWriter.Write(" 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  q 1 0 0 1 6.4995 20 cm 0 0 m -6.205 -12.713 l 6.205 -12.713 l h b Q q 1 0 0 1 1.1909 6.2949 cm 0 0 m 1.278 0 l 1.353 0 1.362 -0.02 1.391 -0.066 c 2.128 -1.363 3.78 -4.275 3.966 -4.713 c 3.985 -4.713 l 3.976 -4.453 3.957 -3.91 3.957 -3.137 c 3.957 -0.076 l 3.957 -0.02 3.976 0 4.041 0 c 4.956 0 l 5.021 0 5.04 -0.029 5.04 -0.084 c 5.04 -6.049 l 5.04 -6.113 5.021 -6.133 4.947 -6.133 c 3.695 -6.133 l 3.621 -6.133 3.611 -6.113 3.574 -6.066 c 3.052 -4.955 1.353 -2.063 0.971 -1.186 c 0.961 -1.186 l 0.999 -1.68 0.999 -2.146 1.008 -3.025 c 1.008 -6.049 l 1.008 -6.104 0.989 -6.133 0.933 -6.133 c 0.009 -6.133 l -0.046 -6.133 -0.075 -6.123 -0.075 -6.049 c -0.075 -0.066 l -0.075 -0.02 -0.056 0 0 0 c f Q q 1 0 0 1 9.1367 3.0273 cm 0 0 m 0.075 0 0.215 -0.008 0.645 -0.008 c 1.4 -0.008 2.119 0.281 2.119 1.213 c 2.119 1.969 1.633 2.381 0.737 2.381 c 0.354 2.381 0.075 2.371 0 2.361 c h -1.146 3.201 m -1.146 3.238 -1.129 3.268 -1.082 3.268 c -0.709 3.275 0.02 3.285 0.729 3.285 c 2.613 3.285 3.248 2.314 3.258 1.232 c 3.258 -0.27 2.007 -0.914 0.607 -0.914 c 0.327 -0.914 0.057 -0.914 0 -0.904 c 0 -2.789 l 0 -2.836 -0.019 -2.865 -0.074 -2.865 c -1.082 -2.865 l -1.119 -2.865 -1.146 -2.846 -1.146 -2.799 c h f Q ");
					break;
				case PdfPopupIcon.Check:
					graphics.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics.StreamWriter.Write(" 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 7.1836 1.2061 cm 0 0 m 6.691 11.152 11.31 14.196 v 10.773 15.201 9.626 16.892 8.155 17.587 c 2.293 10.706 -0.255 4.205 y -4.525 9.177 l -6.883 5.608 l h b ");
					graphics2.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics2.StreamWriter.Write(" 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 7.1836 1.2061 cm 0 0 m 6.691 11.152 11.31 14.196 v 10.773 15.201 9.626 16.892 8.155 17.587 c 2.293 10.706 -0.255 4.205 y -4.525 9.177 l -6.883 5.608 l h b ");
					break;
				case PdfPopupIcon.Circle:
					graphics.StreamWriter.Write("q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d /GS0 gs 1 0 0 1 9.999 3.6387 cm 0 0 m -3.513 0 -6.36 2.85 -6.36 6.363 c -6.36 9.875 -3.513 12.724 0 12.724 c 3.514 12.724 6.363 9.875 6.363 6.363 c 6.363 2.85 3.514 0 0 0 c h f Q ");
					graphics.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics.StreamWriter.Write(" 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 9.999 3.6387 cm 0 0 m -3.513 0 -6.36 2.85 -6.36 6.363 c -6.36 9.875 -3.513 12.724 0 12.724 c 3.514 12.724 6.363 9.875 6.363 6.363 c 6.363 2.85 3.514 0 0 0 c 0 16.119 m -5.388 16.119 -9.756 11.751 -9.756 6.363 c -9.756 0.973 -5.388 -3.395 0 -3.395 c 5.391 -3.395 9.757 0.973 9.757 6.363 c 9.757 11.751 5.391 16.119 0 16.119 c b ");
					graphics2.StreamWriter.Write("q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d /GS0 gs 1 0 0 1 9.999 3.6387 cm 0 0 m -3.513 0 -6.36 2.85 -6.36 6.363 c -6.36 9.875 -3.513 12.724 0 12.724 c 3.514 12.724 6.363 9.875 6.363 6.363 c 6.363 2.85 3.514 0 0 0 c h f Q ");
					graphics2.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics2.StreamWriter.Write(" 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 9.999 3.6387 cm 0 0 m -3.513 0 -6.36 2.85 -6.36 6.363 c -6.36 9.875 -3.513 12.724 0 12.724 c 3.514 12.724 6.363 9.875 6.363 6.363 c 6.363 2.85 3.514 0 0 0 c 0 16.119 m -5.388 16.119 -9.756 11.751 -9.756 6.363 c -9.756 0.973 -5.388 -3.395 0 -3.395 c 5.391 -3.395 9.757 0.973 9.757 6.363 c 9.757 11.751 5.391 16.119 0 16.119 c b ");
					break;
				case PdfPopupIcon.Cross:
					graphics.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics.StreamWriter.Write(" 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 18.6924 3.1357 cm 0 0 m -6.363 6.364 l 0 12.728 l -2.828 15.556 l -9.192 9.192 l -15.556 15.556 l -18.384 12.728 l -12.02 6.364 l -18.384 0 l -15.556 -2.828 l -9.192 3.535 l -2.828 -2.828 l h b ");
					graphics2.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics2.StreamWriter.Write(" 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 18.6924 3.1357 cm 0 0 m -6.363 6.364 l 0 12.728 l -2.828 15.556 l -9.192 9.192 l -15.556 15.556 l -18.384 12.728 l -12.02 6.364 l -18.384 0 l -15.556 -2.828 l -9.192 3.535 l -2.828 -2.828 l h b ");
					break;
				case PdfPopupIcon.CrossHairs:
					graphics.StreamWriter.Write("q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d /GS0 gs 1 0 0 1 9.9771 1.9443 cm 0 0 m -4.448 0 -8.053 3.604 -8.053 8.053 c -8.053 12.5 -4.448 16.106 0 16.106 c 4.447 16.106 8.054 12.5 8.054 8.053 c 8.054 3.604 4.447 0 0 0 c h f Q ");
					graphics.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics.StreamWriter.Write(" 0 G 0 i 0.61 w 4 M 0 j 0 J []0 d  q 1 0 0 1 9.9771 1.9443 cm 0 0 m -4.448 0 -8.053 3.604 -8.053 8.053 c -8.053 12.5 -4.448 16.106 0 16.106 c 4.447 16.106 8.054 12.5 8.054 8.053 c 8.054 3.604 4.447 0 0 0 c 0 17.716 m -5.336 17.716 -9.663 13.39 -9.663 8.053 c -9.663 2.716 -5.336 -1.61 0 -1.61 c 5.337 -1.61 9.664 2.716 9.664 8.053 c 9.664 13.39 5.337 17.716 0 17.716 c b Q q 1 0 0 1 10.7861 14.8325 cm 0 0 m -1.611 0 l -1.611 -4.027 l -5.638 -4.027 l -5.638 -5.638 l -1.611 -5.638 l -1.611 -9.665 l 0 -9.665 l 0 -5.638 l 4.026 -5.638 l 4.026 -4.027 l 0 -4.027 l h b Q ");
					graphics2.StreamWriter.Write("q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d /GS0 gs 1 0 0 1 9.9771 1.9443 cm 0 0 m -4.448 0 -8.053 3.604 -8.053 8.053 c -8.053 12.5 -4.448 16.106 0 16.106 c 4.447 16.106 8.054 12.5 8.054 8.053 c 8.054 3.604 4.447 0 0 0 c h f Q ");
					graphics2.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics2.StreamWriter.Write(" 0 G 0 i 0.61 w 4 M 0 j 0 J []0 d  q 1 0 0 1 9.9771 1.9443 cm 0 0 m -4.448 0 -8.053 3.604 -8.053 8.053 c -8.053 12.5 -4.448 16.106 0 16.106 c 4.447 16.106 8.054 12.5 8.054 8.053 c 8.054 3.604 4.447 0 0 0 c 0 17.716 m -5.336 17.716 -9.663 13.39 -9.663 8.053 c -9.663 2.716 -5.336 -1.61 0 -1.61 c 5.337 -1.61 9.664 2.716 9.664 8.053 c 9.664 13.39 5.337 17.716 0 17.716 c b Q q 1 0 0 1 10.7861 14.8325 cm 0 0 m -1.611 0 l -1.611 -4.027 l -5.638 -4.027 l -5.638 -5.638 l -1.611 -5.638 l -1.611 -9.665 l 0 -9.665 l 0 -5.638 l 4.026 -5.638 l 4.026 -4.027 l 0 -4.027 l h b Q ");
					break;
				case PdfPopupIcon.RightArrow:
					graphics.StreamWriter.Write("q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d /GS0 gs 1 0 0 1 3.7856 11.1963 cm 6.214 -10.655 m 11.438 -10.655 15.673 -6.42 15.673 -1.196 c 15.673 4.027 11.438 8.262 6.214 8.262 c 0.991 8.262 -3.244 4.027 -3.244 -1.196 c -3.244 -6.42 0.991 -10.655 6.214 -10.655 c h f Q  ");
					graphics.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics.StreamWriter.Write(" 0 G 0 i 0.59 w 4 M 0 j 0 J []0 d  1 0 0 1 3.7856 11.1963 cm 0 0 m 8.554 0 l 6.045 2.51 l 7.236 3.702 l 12.135 -1.197 l 7.236 -6.096 l 6.088 -4.949 l 8.644 -2.394 l 0 -2.394 l h 6.214 -10.655 m 11.438 -10.655 15.673 -6.42 15.673 -1.196 c 15.673 4.027 11.438 8.262 6.214 8.262 c 0.991 8.262 -3.244 4.027 -3.244 -1.196 c -3.244 -6.42 0.991 -10.655 6.214 -10.655 c b ");
					graphics2.StreamWriter.Write("q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d /GS0 gs 1 0 0 1 3.7856 11.1963 cm 6.214 -10.655 m 11.438 -10.655 15.673 -6.42 15.673 -1.196 c 15.673 4.027 11.438 8.262 6.214 8.262 c 0.991 8.262 -3.244 4.027 -3.244 -1.196 c -3.244 -6.42 0.991 -10.655 6.214 -10.655 c h f Q ");
					graphics2.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics2.StreamWriter.Write(" 0 G 0 i 0.59 w 4 M 0 j 0 J []0 d  1 0 0 1 3.7856 11.1963 cm 0 0 m 8.554 0 l 6.045 2.51 l 7.236 3.702 l 12.135 -1.197 l 7.236 -6.096 l 6.088 -4.949 l 8.644 -2.394 l 0 -2.394 l h 6.214 -10.655 m 11.438 -10.655 15.673 -6.42 15.673 -1.196 c 15.673 4.027 11.438 8.262 6.214 8.262 c 0.991 8.262 -3.244 4.027 -3.244 -1.196 c -3.244 -6.42 0.991 -10.655 6.214 -10.655 c b ");
					break;
				case PdfPopupIcon.Star:
					graphics.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics.StreamWriter.Write(" 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 9.999 18.8838 cm 0 0 m 3.051 -6.178 l 9.867 -7.168 l 4.934 -11.978 l 6.099 -18.768 l 0 -15.562 l -6.097 -18.768 l -4.933 -11.978 l -9.866 -7.168 l -3.048 -6.178 l b ");
					graphics2.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics2.StreamWriter.Write(" 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 9.999 18.8838 cm 0 0 m 3.051 -6.178 l 9.867 -7.168 l 4.934 -11.978 l 6.099 -18.768 l 0 -15.562 l -6.097 -18.768 l -4.933 -11.978 l -9.866 -7.168 l -3.048 -6.178 l b ");
					break;
				case PdfPopupIcon.UpArrow:
					graphics.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics.StreamWriter.Write(" 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 1.1007 6.7185 cm 0 0 m 4.009 0 l 4.009 -6.719 l 11.086 -6.719 l 11.086 0 l 14.963 0 l 7.499 13.081 l b ");
					graphics2.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics2.StreamWriter.Write(" 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 1.1007 6.7185 cm 0 0 m 4.009 0 l 4.009 -6.719 l 11.086 -6.719 l 11.086 0 l 14.963 0 l 7.499 13.081 l b ");
					break;
				case PdfPopupIcon.UpLeftArrow:
					graphics.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics.StreamWriter.Write(" 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 2.8335 1.7627 cm 0 0 m -2.74 15.16 l 12.345 12.389 l 9.458 9.493 l 14.027 4.91 l 7.532 -1.607 l 2.964 2.975 l b ");
					graphics2.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics2.StreamWriter.Write(" 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 2.8335 1.7627 cm 0 0 m -2.74 15.16 l 12.345 12.389 l 9.458 9.493 l 14.027 4.91 l 7.532 -1.607 l 2.964 2.975 l b ");
					break;
				case PdfPopupIcon.RightPointer:
					graphics.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics.StreamWriter.Write(" 0 G 0.59 w 4 M 0 j 0 J []0 d  1 0 0 1 1.1871 17.0000 cm 0 0 m 4.703 -8.703 l 0 -17 l 18.813 -8.703 l b ");
					graphics2.StreamWriter.SetColorAndSpace(Color, PdfColorSpace.RGB, forStroking: false);
					graphics2.StreamWriter.Write(" 0 G 0.59 w 4 M 0 j 0 J []0 d  1 0 0 1 1.1871 17.0000 cm 0 0 m 4.703 -8.703 l 0 -17 l 18.813 -8.703 l b ");
					break;
				}
			}
			if (!base.Dictionary.ContainsKey("Name"))
			{
				base.Dictionary.SetName("Name", PdfPopupIcon.Note.ToString());
			}
			if (Opacity < 1f)
			{
				graphics.Restore(state);
				graphics2.Restore(state2);
			}
		}
		else if (!m_isCustomIconEnabled)
		{
			base.Dictionary.SetName("Name", "#23CustomIcon");
		}
		base.Dictionary.SetProperty("AP", base.Appearance);
	}
}

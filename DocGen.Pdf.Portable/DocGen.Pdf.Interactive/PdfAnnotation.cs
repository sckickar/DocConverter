using System;
using System.Collections.Generic;
using System.ComponentModel;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public abstract class PdfAnnotation : IPdfWrapper, INotifyPropertyChanged
{
	private PdfColor m_color = PdfColor.Empty;

	private PdfAnnotationBorder m_border;

	private RectangleF m_rectangle = RectangleF.Empty;

	private PdfPage m_page;

	internal PdfLoadedPage m_loadedPage;

	internal string m_text = string.Empty;

	private string m_author = string.Empty;

	private string m_subject = string.Empty;

	private DateTime m_modifiedDate;

	private PdfAnnotationFlags m_annotationFlags;

	private PdfDictionary m_dictionary = new PdfDictionary();

	private PdfColor m_innerColor;

	private float m_opacity = 1f;

	private bool m_flatten;

	private bool m_flattenPopUps;

	private PdfTag m_tag;

	internal PdfAppearance m_appearance;

	private bool m_setAppearanceDictionary;

	internal bool isAuthorExplicitSet;

	private PdfPopupAnnotation m_popup;

	private PdfLayer layer;

	private PdfAnnotationRotateAngle m_angle;

	internal bool rotationModified;

	private PdfMargins m_margins = new PdfMargins();

	internal PdfPopupAnnotationCollection m_reviewHistory;

	internal PdfPopupAnnotationCollection m_comments;

	internal const string TopCaption = "Top";

	private List<PdfAnnotation> m_popupAnnotations = new List<PdfAnnotation>();

	private string m_name;

	private float m_borderWidth;

	internal bool m_isStandardAppearance = true;

	private float rotateAngle;

	private bool m_locationDisplaced;

	internal bool m_addingOldAnnotation;

	internal bool unSupportedAnnotation;

	internal bool isPropertyChanged = true;

	internal bool isAnnotationCreation;

	private PdfConformanceLevel existingConformanceLevel;

	private int m_flagBit = 4;

	private bool beginSaveEventTriggered;

	internal const string r_Comment = "q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d 1 0 0 1 9 5.0908 cm 7.74 12.616 m -7.74 12.616 l -8.274 12.616 -8.707 12.184 -8.707 11.649 c h f Q 0 G ";

	internal const string r_Comment_Secondhalf = " 0 i 0.60 w 4 M 1 j 0 J [0 100]1 d  1 0 0 1 9 5.0908 cm 4.1 1.71 m -0.54 -2.29 l  -0.54 1.71 l  -5.5 1.71 l  -5.5 14.42 l  10.5 14.42 l  10.5 1.71 l  4.1 1.71 l -2.33 9.66 m 7.34 9.66 l 7.34 8.83 l -2.33 8.83 l -2.33 9.66 l -2.33 7.28 m 5.88 7.28 l 5.88 6.46 l -2.33 6.46 l -2.33 7.28 l 14.9 23.1235 m -14.9 23.1235 l -14.9 -20.345 l 14.9 -20.345 l 14.9 23.1235 l b ";

	internal const string n_Comment = "q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d 1 0 0 1 9 5.0908 cm 7.74 12.616 m -7.74 12.616 l -8.274 12.616 -8.707 12.184 -8.707 11.649 c -8.707 -3.831 l -8.707 -4.365 -8.274 -4.798 -7.74 -4.798 c 7.74 -4.798 l 8.274 -4.798 8.707 -4.365 8.707 -3.831 c 8.707 11.649 l 8.707 12.184 8.274 12.616 7.74 12.616 c h f Q 0 G ";

	internal const string n_Comment_Secondhalf = " 0 i 0.60 w 4 M 1 j 0 J [0 100]1 d  1 0 0 1 9 5.0908 cm 1 0 m -2.325 -2.81 l  -2.325 0 l  -5.72 0 l  -5.72 8.94 l  5.51 8.94 l  5.51 0 l  1 0 l -3.50 5.01 m -3.50 5.59 l 3.29 5.59 l 3.29 5.01 l -3.50 5.01 l -3.50 3.34 m -3.50 3.92 l 2.27 3.92 l 2.27 3.34 l -3.50 3.34 l 7.74 12.616 m -7.74 12.616 l -8.274 12.616 -8.707 12.184 -8.707 11.649 c -8.707 -3.831 l -8.707 -4.365 -8.274 -4.798 -7.74 -4.798 c 7.74 -4.798 l 8.274 -4.798 8.707 -4.365 8.707 -3.831 c 8.707 11.649 l 8.707 12.184 8.274 12.616 7.74 12.616 c b ";

	internal const string r_Note = " 0 G 0 i 0.61 w 4 M 0 j 0 J []0 d  q 1 0 0 1 16.959 1.3672 cm 0 0 m 0 -0.434 -0.352 -0.785 -0.784 -0.785 c -14.911 -0.785 l -15.345 -0.785 -15.696 -0.434 -15.696 0 c -15.696 17.266 l -15.696 17.699 -15.345 18.051 -14.911 18.051 c -0.784 18.051 l -0.352 18.051 0 17.699 0 17.266 c h b Q q 1 0 0 1 4.4023 13.9243 cm 0 0 m 9.418 0 l S Q q 1 0 0 1 4.4019 11.2207 cm 0 0 m 9.418 0 l S Q q 1 0 0 1 4.4023 8.5176 cm 0 0 m 9.418 0 l S Q q 1 0 0 1 4.4023 5.8135 cm 0 0 m 9.418 0 l S Q ";

	internal const string n_Note = " 0 G 0 i 0.61 w 4 M 0 j 0 J []0 d  q 1 0 0 1 16.959 1.3672 cm 0 0 m 0 -0.434 -0.352 -0.785 -0.784 -0.785 c -14.911 -0.785 l -15.345 -0.785 -15.696 -0.434 -15.696 0 c -15.696 17.266 l -15.696 17.699 -15.345 18.051 -14.911 18.051 c -0.784 18.051 l -0.352 18.051 0 17.699 0 17.266 c h b Q q 1 0 0 1 4.4023 13.9243 cm 0 0 m 9.418 0 l S Q q 1 0 0 1 4.4019 11.2207 cm 0 0 m 9.418 0 l S Q q 1 0 0 1 4.4023 8.5176 cm 0 0 m 9.418 0 l S Q q 1 0 0 1 4.4023 5.8135 cm 0 0 m 9.418 0 l S Q ";

	internal const string r_Help = "q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d 1 0 0 1 12.1465 10.5137 cm -2.146 9.403 m -7.589 9.403 -12.001 4.99 -12.001 -0.453 c -12.001 -5.895 -7.589 -10.309 -2.146 -10.309 c 3.296 -10.309 7.709 -5.895 7.709 -0.453 c 7.709 4.99 3.296 9.403 -2.146 9.403 c h f Q ";

	internal const string r_Help_Secondhalf = " 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 12.1465 10.5137 cm 0 0 m -0.682 -0.756 -0.958 -1.472 -0.938 -2.302 c -0.938 -2.632 l -3.385 -2.632 l -3.403 -2.154 l -3.459 -1.216 -3.147 -0.259 -2.316 0.716 c -1.729 1.433 -1.251 2.022 -1.251 2.647 c -1.251 3.291 -1.674 3.715 -2.594 3.751 c -3.202 3.751 -3.937 3.531 -4.417 3.2 c -5.041 5.205 l -4.361 5.591 -3.274 5.959 -1.968 5.959 c 0.46 5.959 1.563 4.616 1.563 3.089 c 1.563 1.691 0.699 0.771 0 0 c -2.227 -6.863 m -2.245 -6.863 l -3.202 -6.863 -3.864 -6.146 -3.864 -5.189 c -3.864 -4.196 -3.182 -3.516 -2.227 -3.516 c -1.233 -3.516 -0.589 -4.196 -0.57 -5.189 c -0.57 -6.146 -1.233 -6.863 -2.227 -6.863 c -2.146 9.403 m -7.589 9.403 -12.001 4.99 -12.001 -0.453 c -12.001 -5.895 -7.589 -10.309 -2.146 -10.309 c 3.296 -10.309 7.709 -5.895 7.709 -0.453 c 7.709 4.99 3.296 9.403 -2.146 9.403 c b ";

	internal const string n_Help = "q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d 1 0 0 1 12.1465 10.5137 cm -2.146 9.403 m -7.589 9.403 -12.001 4.99 -12.001 -0.453 c -12.001 -5.895 -7.589 -10.309 -2.146 -10.309 c 3.296 -10.309 7.709 -5.895 7.709 -0.453 c 7.709 4.99 3.296 9.403 -2.146 9.403 c h f Q ";

	internal const string n_Help_Secondhalf = " 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 12.1465 10.5137 cm 0 0 m -0.682 -0.756 -0.958 -1.472 -0.938 -2.302 c -0.938 -2.632 l -3.385 -2.632 l -3.403 -2.154 l -3.459 -1.216 -3.147 -0.259 -2.316 0.716 c -1.729 1.433 -1.251 2.022 -1.251 2.647 c -1.251 3.291 -1.674 3.715 -2.594 3.751 c -3.202 3.751 -3.937 3.531 -4.417 3.2 c -5.041 5.205 l -4.361 5.591 -3.274 5.959 -1.968 5.959 c 0.46 5.959 1.563 4.616 1.563 3.089 c 1.563 1.691 0.699 0.771 0 0 c -2.227 -6.863 m -2.245 -6.863 l -3.202 -6.863 -3.864 -6.146 -3.864 -5.189 c -3.864 -4.196 -3.182 -3.516 -2.227 -3.516 c -1.233 -3.516 -0.589 -4.196 -0.57 -5.189 c -0.57 -6.146 -1.233 -6.863 -2.227 -6.863 c -2.146 9.403 m -7.589 9.403 -12.001 4.99 -12.001 -0.453 c -12.001 -5.895 -7.589 -10.309 -2.146 -10.309 c 3.296 -10.309 7.709 -5.895 7.709 -0.453 c 7.709 4.99 3.296 9.403 -2.146 9.403 c b ";

	internal const string r_Insert = " 0 i 0.59 w 4 M 0 j 0 J []0 d  1 0 0 1 8.5386 19.8545 cm 0 0 m -8.39 -19.719 l 8.388 -19.719 l h B ";

	internal const string n_Insert = " 0 i 0.59 w 4 M 0 j 0 J []0 d  1 0 0 1 8.5386 19.8545 cm 0 0 m -8.39 -19.719 l 8.388 -19.719 l h B ";

	internal const string r_Key = "q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d 1 0 0 1 6.5 12.6729 cm 0.001 5.138 m -2.543 5.138 -4.604 3.077 -4.604 0.534 c -4.604 -1.368 -3.449 -3.001 -1.802 -3.702 c -1.802 -4.712 l -0.795 -5.719 l -1.896 -6.82 l -0.677 -8.039 l -1.595 -8.958 l -0.602 -9.949 l -1.479 -10.829 l -0.085 -12.483 l 1.728 -10.931 l 1.728 -3.732 l 1.737 -3.728 1.75 -3.724 1.76 -3.721 c 3.429 -3.03 4.604 -1.385 4.604 0.534 c 4.604 3.077 2.542 5.138 0.001 5.138 c f Q ";

	internal const string r_Key_Secondhalf = " 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 6.5 12.6729 cm 0 0 m -1.076 0 -1.95 0.874 -1.95 1.95 c -1.95 3.028 -1.076 3.306 0 3.306 c 1.077 3.306 1.95 3.028 1.95 1.95 c 1.95 0.874 1.077 0 0 0 c 0.001 5.138 m -2.543 5.138 -4.604 3.077 -4.604 0.534 c -4.604 -1.368 -3.449 -3.001 -1.802 -3.702 c -1.802 -4.712 l -0.795 -5.719 l -1.896 -6.82 l -0.677 -8.039 l -1.595 -8.958 l -0.602 -9.949 l -1.479 -10.829 l -0.085 -12.483 l 1.728 -10.931 l 1.728 -3.732 l 1.737 -3.728 1.75 -3.724 1.76 -3.721 c 3.429 -3.03 4.604 -1.385 4.604 0.534 c 4.604 3.077 2.542 5.138 0.001 5.138 c b ";

	internal const string n_Key = "q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d 1 0 0 1 6.5 12.6729 cm 0.001 5.138 m -2.543 5.138 -4.604 3.077 -4.604 0.534 c -4.604 -1.368 -3.449 -3.001 -1.802 -3.702 c -1.802 -4.712 l -0.795 -5.719 l -1.896 -6.82 l -0.677 -8.039 l -1.595 -8.958 l -0.602 -9.949 l -1.479 -10.829 l -0.085 -12.483 l 1.728 -10.931 l 1.728 -3.732 l 1.737 -3.728 1.75 -3.724 1.76 -3.721 c 3.429 -3.03 4.604 -1.385 4.604 0.534 c 4.604 3.077 2.542 5.138 0.001 5.138 c f Q ";

	internal const string n_Key_Secondhalf = " 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 6.5 12.6729 cm 0 0 m -1.076 0 -1.95 0.874 -1.95 1.95 c -1.95 3.028 -1.076 3.306 0 3.306 c 1.077 3.306 1.95 3.028 1.95 1.95 c 1.95 0.874 1.077 0 0 0 c 0.001 5.138 m -2.543 5.138 -4.604 3.077 -4.604 0.534 c -4.604 -1.368 -3.449 -3.001 -1.802 -3.702 c -1.802 -4.712 l -0.795 -5.719 l -1.896 -6.82 l -0.677 -8.039 l -1.595 -8.958 l -0.602 -9.949 l -1.479 -10.829 l -0.085 -12.483 l 1.728 -10.931 l 1.728 -3.732 l 1.737 -3.728 1.75 -3.724 1.76 -3.721 c 3.429 -3.03 4.604 -1.385 4.604 0.534 c 4.604 3.077 2.542 5.138 0.001 5.138 c b ";

	internal const string r_NewParagraph = "1 0.819611 0 rg 0 G 0 i 0.58 w 4 M 0 j 0 J []0 d ";

	internal const string r_NewParagraph_Secondhalf = " 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  q 1 0 0 1 6.4995 20 cm 0 0 m -6.205 -12.713 l 6.205 -12.713 l h b Q q 1 0 0 1 1.1909 6.2949 cm 0 0 m 1.278 0 l 1.353 0 1.362 -0.02 1.391 -0.066 c 2.128 -1.363 3.78 -4.275 3.966 -4.713 c 3.985 -4.713 l 3.976 -4.453 3.957 -3.91 3.957 -3.137 c 3.957 -0.076 l 3.957 -0.02 3.976 0 4.041 0 c 4.956 0 l 5.021 0 5.04 -0.029 5.04 -0.084 c 5.04 -6.049 l 5.04 -6.113 5.021 -6.133 4.947 -6.133 c 3.695 -6.133 l 3.621 -6.133 3.611 -6.113 3.574 -6.066 c 3.052 -4.955 1.353 -2.063 0.971 -1.186 c 0.961 -1.186 l 0.999 -1.68 0.999 -2.146 1.008 -3.025 c 1.008 -6.049 l 1.008 -6.104 0.989 -6.133 0.933 -6.133 c 0.009 -6.133 l -0.046 -6.133 -0.075 -6.123 -0.075 -6.049 c -0.075 -0.066 l -0.075 -0.02 -0.056 0 0 0 c f Q q 1 0 0 1 9.1367 3.0273 cm 0 0 m 0.075 0 0.215 -0.008 0.645 -0.008 c 1.4 -0.008 2.119 0.281 2.119 1.213 c 2.119 1.969 1.633 2.381 0.737 2.381 c 0.354 2.381 0.075 2.371 0 2.361 c h -1.146 3.201 m -1.146 3.238 -1.129 3.268 -1.082 3.268 c -0.709 3.275 0.02 3.285 0.729 3.285 c 2.613 3.285 3.248 2.314 3.258 1.232 c 3.258 -0.27 2.007 -0.914 0.607 -0.914 c 0.327 -0.914 0.057 -0.914 0 -0.904 c 0 -2.789 l 0 -2.836 -0.019 -2.865 -0.074 -2.865 c -1.082 -2.865 l -1.119 -2.865 -1.146 -2.846 -1.146 -2.799 c h f Q ";

	internal const string n_NewParagraph = "1 0.819611 0 rg 0 G 0 i 0.58 w 4 M 0 j 0 J []0 d ";

	internal const string n_NewParagraph_Secondhalf = " 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  q 1 0 0 1 6.4995 20 cm 0 0 m -6.205 -12.713 l 6.205 -12.713 l h b Q q 1 0 0 1 1.1909 6.2949 cm 0 0 m 1.278 0 l 1.353 0 1.362 -0.02 1.391 -0.066 c 2.128 -1.363 3.78 -4.275 3.966 -4.713 c 3.985 -4.713 l 3.976 -4.453 3.957 -3.91 3.957 -3.137 c 3.957 -0.076 l 3.957 -0.02 3.976 0 4.041 0 c 4.956 0 l 5.021 0 5.04 -0.029 5.04 -0.084 c 5.04 -6.049 l 5.04 -6.113 5.021 -6.133 4.947 -6.133 c 3.695 -6.133 l 3.621 -6.133 3.611 -6.113 3.574 -6.066 c 3.052 -4.955 1.353 -2.063 0.971 -1.186 c 0.961 -1.186 l 0.999 -1.68 0.999 -2.146 1.008 -3.025 c 1.008 -6.049 l 1.008 -6.104 0.989 -6.133 0.933 -6.133 c 0.009 -6.133 l -0.046 -6.133 -0.075 -6.123 -0.075 -6.049 c -0.075 -0.066 l -0.075 -0.02 -0.056 0 0 0 c f Q q 1 0 0 1 9.1367 3.0273 cm 0 0 m 0.075 0 0.215 -0.008 0.645 -0.008 c 1.4 -0.008 2.119 0.281 2.119 1.213 c 2.119 1.969 1.633 2.381 0.737 2.381 c 0.354 2.381 0.075 2.371 0 2.361 c h -1.146 3.201 m -1.146 3.238 -1.129 3.268 -1.082 3.268 c -0.709 3.275 0.02 3.285 0.729 3.285 c 2.613 3.285 3.248 2.314 3.258 1.232 c 3.258 -0.27 2.007 -0.914 0.607 -0.914 c 0.327 -0.914 0.057 -0.914 0 -0.904 c 0 -2.789 l 0 -2.836 -0.019 -2.865 -0.074 -2.865 c -1.082 -2.865 l -1.119 -2.865 -1.146 -2.846 -1.146 -2.799 c h f Q ";

	internal const string r_Paragraph = "q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d 1 0 0 1 19.6973 10.0005 cm 0 0 m 0 -5.336 -4.326 -9.662 -9.663 -9.662 c -14.998 -9.662 -19.324 -5.336 -19.324 0 c -19.324 5.335 -14.998 9.662 -9.663 9.662 c -4.326 9.662 0 5.335 0 0 c h f Q ";

	internal const string r_Paragraph_Secondhalf = "0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  q 1 0 0 1 19.6973 10.0005 cm 0 0 m 0 -5.336 -4.326 -9.662 -9.663 -9.662 c -14.998 -9.662 -19.324 -5.336 -19.324 0 c -19.324 5.335 -14.998 9.662 -9.663 9.662 c -4.326 9.662 0 5.335 0 0 c h S Q q 1 0 0 1 11.6787 2.6582 cm 0 0 m -1.141 0 l -1.227 0 -1.244 0.052 -1.227 0.139 c -0.656 1.157 -0.52 2.505 -0.52 3.317 c -0.52 3.594 l -2.833 3.783 -5.441 4.838 -5.441 8.309 c -5.441 10.778 -3.714 12.626 -0.57 13.024 c -0.535 13.508 -0.381 14.129 -0.242 14.389 c -0.207 14.44 -0.174 14.475 -0.104 14.475 c 1.088 14.475 l 1.156 14.475 1.191 14.458 1.175 14.372 c 1.105 14.095 0.881 13.127 0.881 12.402 c 0.881 9.431 0.932 7.324 0.95 4.06 c 0.95 2.298 0.708 0.813 0.189 0.07 c 0.155 0.034 0.103 0 0 0 c b Q ";

	internal const string n_Paragraph = "q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d 1 0 0 1 19.6973 10.0005 cm 0 0 m 0 -5.336 -4.326 -9.662 -9.663 -9.662 c -14.998 -9.662 -19.324 -5.336 -19.324 0 c -19.324 5.335 -14.998 9.662 -9.663 9.662 c -4.326 9.662 0 5.335 0 0 c h f Q ";

	internal const string n_Paragraph_Secondhalf = "0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  q 1 0 0 1 19.6973 10.0005 cm 0 0 m 0 -5.336 -4.326 -9.662 -9.663 -9.662 c -14.998 -9.662 -19.324 -5.336 -19.324 0 c -19.324 5.335 -14.998 9.662 -9.663 9.662 c -4.326 9.662 0 5.335 0 0 c h S Q q 1 0 0 1 11.6787 2.6582 cm 0 0 m -1.141 0 l -1.227 0 -1.244 0.052 -1.227 0.139 c -0.656 1.157 -0.52 2.505 -0.52 3.317 c -0.52 3.594 l -2.833 3.783 -5.441 4.838 -5.441 8.309 c -5.441 10.778 -3.714 12.626 -0.57 13.024 c -0.535 13.508 -0.381 14.129 -0.242 14.389 c -0.207 14.44 -0.174 14.475 -0.104 14.475 c 1.088 14.475 l 1.156 14.475 1.191 14.458 1.175 14.372 c 1.105 14.095 0.881 13.127 0.881 12.402 c 0.881 9.431 0.932 7.324 0.95 4.06 c 0.95 2.298 0.708 0.813 0.189 0.07 c 0.155 0.034 0.103 0 0 0 c b Q ";

	internal const string r_CheckMark = "q 0.396 0.396 0.396 rg 1 0 0 1 13.5151 16.5 cm 0 0 m -6.7 -10.23 l -8.81 -7 l -13.22 -7 l -6.29 -15 l 4.19 0 l h f Q ";

	internal const string n_CheckMark = "q 0.396 0.396 0.396 rg 1 0 0 1 13.5151 16.5 cm 0 0 m -6.7 -10.23 l -8.81 -7 l -13.22 -7 l -6.29 -15 l 4.19 0 l h f Q ";

	internal const string r_Check = " 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 7.1836 1.2061 cm 0 0 m 6.691 11.152 11.31 14.196 v 10.773 15.201 9.626 16.892 8.155 17.587 c 2.293 10.706 -0.255 4.205 y -4.525 9.177 l -6.883 5.608 l h b ";

	internal const string n_Check = " 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 7.1836 1.2061 cm 0 0 m 6.691 11.152 11.31 14.196 v 10.773 15.201 9.626 16.892 8.155 17.587 c 2.293 10.706 -0.255 4.205 y -4.525 9.177 l -6.883 5.608 l h b ";

	internal const string r_Circle = "q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d /GS0 gs 1 0 0 1 9.999 3.6387 cm 0 0 m -3.513 0 -6.36 2.85 -6.36 6.363 c -6.36 9.875 -3.513 12.724 0 12.724 c 3.514 12.724 6.363 9.875 6.363 6.363 c 6.363 2.85 3.514 0 0 0 c h f Q ";

	internal const string r_CircleSecondHalf = " 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 9.999 3.6387 cm 0 0 m -3.513 0 -6.36 2.85 -6.36 6.363 c -6.36 9.875 -3.513 12.724 0 12.724 c 3.514 12.724 6.363 9.875 6.363 6.363 c 6.363 2.85 3.514 0 0 0 c 0 16.119 m -5.388 16.119 -9.756 11.751 -9.756 6.363 c -9.756 0.973 -5.388 -3.395 0 -3.395 c 5.391 -3.395 9.757 0.973 9.757 6.363 c 9.757 11.751 5.391 16.119 0 16.119 c b ";

	internal const string n_Circle = "q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d /GS0 gs 1 0 0 1 9.999 3.6387 cm 0 0 m -3.513 0 -6.36 2.85 -6.36 6.363 c -6.36 9.875 -3.513 12.724 0 12.724 c 3.514 12.724 6.363 9.875 6.363 6.363 c 6.363 2.85 3.514 0 0 0 c h f Q ";

	internal const string n_CircleSecondHalf = " 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 9.999 3.6387 cm 0 0 m -3.513 0 -6.36 2.85 -6.36 6.363 c -6.36 9.875 -3.513 12.724 0 12.724 c 3.514 12.724 6.363 9.875 6.363 6.363 c 6.363 2.85 3.514 0 0 0 c 0 16.119 m -5.388 16.119 -9.756 11.751 -9.756 6.363 c -9.756 0.973 -5.388 -3.395 0 -3.395 c 5.391 -3.395 9.757 0.973 9.757 6.363 c 9.757 11.751 5.391 16.119 0 16.119 c b ";

	internal const string r_Cross = " 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 18.6924 3.1357 cm 0 0 m -6.363 6.364 l 0 12.728 l -2.828 15.556 l -9.192 9.192 l -15.556 15.556 l -18.384 12.728 l -12.02 6.364 l -18.384 0 l -15.556 -2.828 l -9.192 3.535 l -2.828 -2.828 l h b ";

	internal const string n_Cross = " 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 18.6924 3.1357 cm 0 0 m -6.363 6.364 l 0 12.728 l -2.828 15.556 l -9.192 9.192 l -15.556 15.556 l -18.384 12.728 l -12.02 6.364 l -18.384 0 l -15.556 -2.828 l -9.192 3.535 l -2.828 -2.828 l h b ";

	internal const string r_CheckHairs = "q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d /GS0 gs 1 0 0 1 9.9771 1.9443 cm 0 0 m -4.448 0 -8.053 3.604 -8.053 8.053 c -8.053 12.5 -4.448 16.106 0 16.106 c 4.447 16.106 8.054 12.5 8.054 8.053 c 8.054 3.604 4.447 0 0 0 c h f Q ";

	internal const string r_CheckHairsSecondHalf = " 0 G 0 i 0.61 w 4 M 0 j 0 J []0 d  q 1 0 0 1 9.9771 1.9443 cm 0 0 m -4.448 0 -8.053 3.604 -8.053 8.053 c -8.053 12.5 -4.448 16.106 0 16.106 c 4.447 16.106 8.054 12.5 8.054 8.053 c 8.054 3.604 4.447 0 0 0 c 0 17.716 m -5.336 17.716 -9.663 13.39 -9.663 8.053 c -9.663 2.716 -5.336 -1.61 0 -1.61 c 5.337 -1.61 9.664 2.716 9.664 8.053 c 9.664 13.39 5.337 17.716 0 17.716 c b Q q 1 0 0 1 10.7861 14.8325 cm 0 0 m -1.611 0 l -1.611 -4.027 l -5.638 -4.027 l -5.638 -5.638 l -1.611 -5.638 l -1.611 -9.665 l 0 -9.665 l 0 -5.638 l 4.026 -5.638 l 4.026 -4.027 l 0 -4.027 l h b Q ";

	internal const string n_CheckHairs = "q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d /GS0 gs 1 0 0 1 9.9771 1.9443 cm 0 0 m -4.448 0 -8.053 3.604 -8.053 8.053 c -8.053 12.5 -4.448 16.106 0 16.106 c 4.447 16.106 8.054 12.5 8.054 8.053 c 8.054 3.604 4.447 0 0 0 c h f Q ";

	internal const string n_CheckHairsSecondHalf = " 0 G 0 i 0.61 w 4 M 0 j 0 J []0 d  q 1 0 0 1 9.9771 1.9443 cm 0 0 m -4.448 0 -8.053 3.604 -8.053 8.053 c -8.053 12.5 -4.448 16.106 0 16.106 c 4.447 16.106 8.054 12.5 8.054 8.053 c 8.054 3.604 4.447 0 0 0 c 0 17.716 m -5.336 17.716 -9.663 13.39 -9.663 8.053 c -9.663 2.716 -5.336 -1.61 0 -1.61 c 5.337 -1.61 9.664 2.716 9.664 8.053 c 9.664 13.39 5.337 17.716 0 17.716 c b Q q 1 0 0 1 10.7861 14.8325 cm 0 0 m -1.611 0 l -1.611 -4.027 l -5.638 -4.027 l -5.638 -5.638 l -1.611 -5.638 l -1.611 -9.665 l 0 -9.665 l 0 -5.638 l 4.026 -5.638 l 4.026 -4.027 l 0 -4.027 l h b Q ";

	internal const string r_RightArrow = "q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d /GS0 gs 1 0 0 1 3.7856 11.1963 cm 6.214 -10.655 m 11.438 -10.655 15.673 -6.42 15.673 -1.196 c 15.673 4.027 11.438 8.262 6.214 8.262 c 0.991 8.262 -3.244 4.027 -3.244 -1.196 c -3.244 -6.42 0.991 -10.655 6.214 -10.655 c h f Q ";

	internal const string r_RightArrowSecondHalf = " 0 G 0 i 0.59 w 4 M 0 j 0 J []0 d  1 0 0 1 3.7856 11.1963 cm 0 0 m 8.554 0 l 6.045 2.51 l 7.236 3.702 l 12.135 -1.197 l 7.236 -6.096 l 6.088 -4.949 l 8.644 -2.394 l 0 -2.394 l h 6.214 -10.655 m 11.438 -10.655 15.673 -6.42 15.673 -1.196 c 15.673 4.027 11.438 8.262 6.214 8.262 c 0.991 8.262 -3.244 4.027 -3.244 -1.196 c -3.244 -6.42 0.991 -10.655 6.214 -10.655 c b ";

	internal const string n_RightArrow = "q 1 1 1 rg 0 i 1 w 4 M 1 j 0 J []0 d /GS0 gs 1 0 0 1 3.7856 11.1963 cm 6.214 -10.655 m 11.438 -10.655 15.673 -6.42 15.673 -1.196 c 15.673 4.027 11.438 8.262 6.214 8.262 c 0.991 8.262 -3.244 4.027 -3.244 -1.196 c -3.244 -6.42 0.991 -10.655 6.214 -10.655 c h f Q  ";

	internal const string n_RightArrowSecondHalf = " 0 G 0 i 0.59 w 4 M 0 j 0 J []0 d  1 0 0 1 3.7856 11.1963 cm 0 0 m 8.554 0 l 6.045 2.51 l 7.236 3.702 l 12.135 -1.197 l 7.236 -6.096 l 6.088 -4.949 l 8.644 -2.394 l 0 -2.394 l h 6.214 -10.655 m 11.438 -10.655 15.673 -6.42 15.673 -1.196 c 15.673 4.027 11.438 8.262 6.214 8.262 c 0.991 8.262 -3.244 4.027 -3.244 -1.196 c -3.244 -6.42 0.991 -10.655 6.214 -10.655 c b ";

	internal const string r_RightPointer = " 0 G 0.59 w 4 M 0 j 0 J []0 d  1 0 0 1 1.1871 17.0000 cm 0 0 m 4.703 -8.703 l 0 -17 l 18.813 -8.703 l b ";

	internal const string n_RightPointer = " 0 G 0.59 w 4 M 0 j 0 J []0 d  1 0 0 1 1.1871 17.0000 cm 0 0 m 4.703 -8.703 l 0 -17 l 18.813 -8.703 l b ";

	internal const string r_Star = " 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 9.999 18.8838 cm 0 0 m 3.051 -6.178 l 9.867 -7.168 l 4.934 -11.978 l 6.099 -18.768 l 0 -15.562 l -6.097 -18.768 l -4.933 -11.978 l -9.866 -7.168 l -3.048 -6.178 l b ";

	internal const string n_Star = " 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 9.999 18.8838 cm 0 0 m 3.051 -6.178 l 9.867 -7.168 l 4.934 -11.978 l 6.099 -18.768 l 0 -15.562 l -6.097 -18.768 l -4.933 -11.978 l -9.866 -7.168 l -3.048 -6.178 l b ";

	internal const string r_UpArrow = " 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 1.1007 6.7185 cm 0 0 m 4.009 0 l 4.009 -6.719 l 11.086 -6.719 l 11.086 0 l 14.963 0 l 7.499 13.081 l b ";

	internal const string n_UpArrow = " 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 1.1007 6.7185 cm 0 0 m 4.009 0 l 4.009 -6.719 l 11.086 -6.719 l 11.086 0 l 14.963 0 l 7.499 13.081 l b ";

	internal const string r_UpLeftArrow = " 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 2.8335 1.7627 cm 0 0 m -2.74 15.16 l 12.345 12.389 l 9.458 9.493 l 14.027 4.91 l 7.532 -1.607 l 2.964 2.975 l b ";

	internal const string n_UpLeftArrow = " 0 G 0 i 0.59 w 4 M 1 j 0 J []0 d  1 0 0 1 2.8335 1.7627 cm 0 0 m -2.74 15.16 l 12.345 12.389 l 9.458 9.493 l 14.027 4.91 l 7.532 -1.607 l 2.964 2.975 l b ";

	public virtual PdfColor Color
	{
		get
		{
			return m_color;
		}
		set
		{
			if (m_color != value)
			{
				m_color = value;
				PdfColorSpace colorSpace = PdfColorSpace.RGB;
				if (Page != null)
				{
					colorSpace = Page.Section.Parent.Document.ColorSpace;
				}
				PdfArray primitive = m_color.ToArray(colorSpace);
				if (m_color.A != 0)
				{
					m_dictionary.SetProperty("C", primitive);
				}
				else
				{
					m_dictionary.Remove("C");
				}
			}
			NotifyPropertyChanged("Color");
		}
	}

	public virtual float Opacity
	{
		get
		{
			if (m_dictionary.Items.ContainsKey(new PdfName("CA")))
			{
				m_opacity = (m_dictionary.Items[new PdfName("CA")] as PdfNumber).FloatValue;
			}
			return m_opacity;
		}
		set
		{
			if (value < 0f || value > 1f)
			{
				throw new ArgumentException("Valid value should be between 0 to 1.");
			}
			if (m_opacity != value)
			{
				m_opacity = value;
				m_dictionary.SetProperty("CA", new PdfNumber(m_opacity));
			}
			NotifyPropertyChanged("Opacity");
		}
	}

	public virtual PdfColor InnerColor
	{
		get
		{
			return m_innerColor;
		}
		set
		{
			m_innerColor = value;
			NotifyPropertyChanged("InnerColor");
		}
	}

	public virtual PdfAnnotationBorder Border
	{
		get
		{
			if (m_border == null)
			{
				m_border = new PdfAnnotationBorder();
			}
			return m_border;
		}
		set
		{
			m_border = value;
			Dictionary.SetProperty("Border", m_border);
			NotifyPropertyChanged("Border");
		}
	}

	public virtual RectangleF Bounds
	{
		get
		{
			return m_rectangle;
		}
		set
		{
			if (m_rectangle != value)
			{
				m_rectangle = value;
				RectangleF rectangle = ObtainNativeRectangle();
				Dictionary.SetProperty("Rect", PdfArray.FromRectangle(rectangle));
			}
			if (isPropertyChanged)
			{
				NotifyPropertyChanged("Bounds");
			}
		}
	}

	public virtual PointF Location
	{
		get
		{
			return m_rectangle.Location;
		}
		set
		{
			m_rectangle = Bounds;
			m_rectangle.Location = value;
			Dictionary.SetProperty("Rect", PdfArray.FromRectangle(m_rectangle));
			NotifyPropertyChanged("Location");
		}
	}

	public virtual SizeF Size
	{
		get
		{
			return m_rectangle.Size;
		}
		set
		{
			m_rectangle = Bounds;
			m_rectangle.Size = value;
			Dictionary.SetProperty("Rect", PdfArray.FromRectangle(m_rectangle));
			NotifyPropertyChanged("Size");
		}
	}

	public PdfPage Page => m_page;

	internal PdfLoadedPage LoadedPage => m_loadedPage;

	public virtual string Text
	{
		get
		{
			if (Dictionary.ContainsKey("Contents"))
			{
				m_text = (Dictionary["Contents"] as PdfString).Value;
			}
			return m_text;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Text");
			}
			if (m_text != value)
			{
				m_text = value;
				Dictionary.SetString("Contents", m_text);
			}
			NotifyPropertyChanged("Text");
		}
	}

	public virtual string Author
	{
		get
		{
			if (Dictionary.ContainsKey("Author"))
			{
				m_author = (Dictionary["Author"] as PdfString).Value;
			}
			else if (Dictionary.ContainsKey("T"))
			{
				m_author = (Dictionary["T"] as PdfString).Value;
			}
			return m_author;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Author");
			}
			if (m_author != value)
			{
				m_author = value;
				Dictionary.SetString("T", m_author);
				isAuthorExplicitSet = true;
			}
			NotifyPropertyChanged("Author");
		}
	}

	public virtual string Subject
	{
		get
		{
			if (Dictionary.ContainsKey("Subject"))
			{
				m_subject = (Dictionary["Subject"] as PdfString).Value;
			}
			else if (Dictionary.ContainsKey("Subj"))
			{
				m_subject = (Dictionary["Subj"] as PdfString).Value;
			}
			return m_subject;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Subject");
			}
			if (m_subject != value)
			{
				m_subject = value;
				Dictionary.SetString("Subj", m_subject);
			}
			NotifyPropertyChanged("Subject");
		}
	}

	public virtual DateTime ModifiedDate
	{
		get
		{
			return m_modifiedDate;
		}
		set
		{
			if (m_modifiedDate != value)
			{
				m_modifiedDate = value;
				Dictionary.SetDateTime("M", m_modifiedDate);
			}
			NotifyPropertyChanged("ModifiedDate");
		}
	}

	public virtual PdfAnnotationFlags AnnotationFlags
	{
		get
		{
			return m_annotationFlags;
		}
		set
		{
			if (m_annotationFlags != value)
			{
				m_annotationFlags = value;
				m_dictionary.SetNumber("F", (int)m_annotationFlags);
			}
			NotifyPropertyChanged("AnnotationFlags");
		}
	}

	internal PdfDictionary Dictionary
	{
		get
		{
			return m_dictionary;
		}
		set
		{
			m_dictionary = value;
		}
	}

	public bool Flatten
	{
		get
		{
			return m_flatten;
		}
		set
		{
			m_flatten = value;
			NotifyPropertyChanged("Flatten");
		}
	}

	public bool FlattenPopUps
	{
		get
		{
			return m_flattenPopUps;
		}
		set
		{
			m_flattenPopUps = value;
			NotifyPropertyChanged("FlattenPopUps");
		}
	}

	public PdfTag PdfTag
	{
		get
		{
			return m_tag;
		}
		set
		{
			m_tag = value;
			NotifyPropertyChanged("PdfTag");
		}
	}

	internal PdfAppearance Appearance
	{
		get
		{
			if (m_appearance == null)
			{
				m_appearance = new PdfAppearance(this);
				m_isStandardAppearance = false;
			}
			return m_appearance;
		}
		set
		{
			if (m_appearance != value)
			{
				m_appearance = value;
				m_isStandardAppearance = false;
			}
			NotifyPropertyChanged("Appearance");
		}
	}

	internal bool SetAppearanceDictionary
	{
		get
		{
			return m_setAppearanceDictionary;
		}
		set
		{
			m_setAppearanceDictionary = value;
			NotifyPropertyChanged("Appearance");
		}
	}

	internal PdfPopupAnnotation Popup
	{
		get
		{
			return m_popup;
		}
		set
		{
			m_popup = value;
			m_popup.Dictionary.SetProperty("Parent", new PdfReferenceHolder(this));
			Dictionary.SetProperty("Popup", new PdfReferenceHolder(m_popup));
			if (!m_popupAnnotations.Contains(m_popup))
			{
				m_popupAnnotations.Add(m_popup);
			}
			NotifyPropertyChanged("Popup");
		}
	}

	public PdfLayer Layer
	{
		get
		{
			if (layer == null)
			{
				layer = GetDocumentLayer();
			}
			return layer;
		}
		set
		{
			if (layer == null)
			{
				layer = value;
				if (layer != null)
				{
					Dictionary.SetProperty("OC", layer.ReferenceHolder);
				}
				else
				{
					Dictionary.Remove("OC");
				}
				NotifyPropertyChanged("Layer");
			}
		}
	}

	public PdfAnnotationRotateAngle Rotate
	{
		get
		{
			m_angle = GetRotateAngle();
			return m_angle;
		}
		set
		{
			m_angle = value;
			int num = 90;
			int num2 = 360;
			int num3 = (int)m_angle * num;
			if (num3 >= 360)
			{
				num3 %= num2;
			}
			PdfNumber value2 = new PdfNumber(num3);
			Dictionary["Rotate"] = value2;
			rotationModified = true;
			NotifyPropertyChanged("Rotate");
		}
	}

	public string Name
	{
		get
		{
			if (this is PdfLoadedStyledAnnotation)
			{
				return (this as PdfLoadedStyledAnnotation).Name;
			}
			return m_name;
		}
		set
		{
			if (value == null || value == string.Empty)
			{
				throw new ArgumentNullException("Name value cannot be null or Empty");
			}
			if (m_name != value)
			{
				m_name = value;
				Dictionary["NM"] = new PdfString(m_name);
			}
			NotifyPropertyChanged("Name");
		}
	}

	internal float RotateAngle
	{
		get
		{
			rotateAngle = GetRotationAngle();
			if (rotateAngle < 0f)
			{
				rotateAngle = 360f + rotateAngle;
			}
			if (rotateAngle >= 360f)
			{
				rotateAngle = 360f - rotateAngle;
			}
			return rotateAngle;
		}
		set
		{
			if (value != GetRotationAngle())
			{
				if (value < 0f)
				{
					value = 360f + value;
				}
				if (value >= 360f)
				{
					value = 360f - value;
				}
				rotateAngle = value;
				PdfNumber primitive = new PdfNumber(rotateAngle);
				Dictionary.SetProperty("Rotate", primitive);
				rotationModified = true;
				NotifyPropertyChanged("RotateAngle");
			}
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public event PropertyChangedEventHandler PropertyChanged;

	internal void NotifyPropertyChanged(string propertyName)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		if (Page != null && Page.Document != null)
		{
			Page.Document.OnAnnotationPropertyChanged(this, propertyName);
		}
		else if (LoadedPage != null && LoadedPage.Document != null)
		{
			LoadedPage.Document.OnAnnotationPropertyChanged(this, propertyName);
		}
	}

	public void SetAppearance(bool appearance)
	{
		SetAppearanceDictionary = appearance;
	}

	internal PdfAnnotation()
	{
		Initialize();
	}

	protected PdfAnnotation(PdfPageBase page, string text)
	{
		Initialize();
		m_page = page as PdfPage;
		m_text = text;
		m_dictionary.SetProperty("Contents", new PdfString(text));
	}

	protected PdfAnnotation(RectangleF bounds)
	{
		Initialize();
		Bounds = bounds;
	}

	internal PdfAnnotation(PdfDictionary dictionary, PdfCrossTable crossTable, RectangleF bounds)
	{
		Initialize();
		Bounds = bounds;
	}

	internal void SetPage(PdfPageBase page)
	{
		TriggerAnnotationSaveEvent(page);
		if (m_page != null)
		{
			m_dictionary.SetProperty("P", new PdfReferenceHolder(m_page));
		}
		else if (LoadedPage != null && !m_dictionary.ContainsKey("P"))
		{
			m_dictionary.SetProperty("P", new PdfReferenceHolder(LoadedPage));
		}
	}

	internal PdfGraphicsUnit GetEqualPdfGraphicsUnit(PdfMeasurementUnit measurementUnit, out string m_unitString)
	{
		PdfGraphicsUnit result;
		switch (measurementUnit)
		{
		case PdfMeasurementUnit.Inch:
			result = PdfGraphicsUnit.Inch;
			m_unitString = "in";
			break;
		case PdfMeasurementUnit.Centimeter:
			result = PdfGraphicsUnit.Centimeter;
			m_unitString = "cm";
			break;
		case PdfMeasurementUnit.Millimeter:
			result = PdfGraphicsUnit.Millimeter;
			m_unitString = "mm";
			break;
		case PdfMeasurementUnit.Pica:
			result = PdfGraphicsUnit.Pica;
			m_unitString = "p";
			break;
		case PdfMeasurementUnit.Point:
			result = PdfGraphicsUnit.Point;
			m_unitString = "pt";
			break;
		default:
			result = PdfGraphicsUnit.Inch;
			m_unitString = "in";
			break;
		}
		return result;
	}

	internal PdfDictionary CreateMeasureDictioanry(string m_unitString)
	{
		PdfDictionary pdfDictionary = new PdfDictionary();
		PdfArray pdfArray = new PdfArray();
		PdfArray pdfArray2 = new PdfArray();
		PdfArray pdfArray3 = new PdfArray();
		new PdfArray();
		pdfDictionary.Items.Add(new PdfName("A"), pdfArray2);
		pdfDictionary.Items.Add(new PdfName("D"), pdfArray);
		pdfDictionary.Items.Add(new PdfName("R"), new PdfString("1 " + m_unitString + " = 1 " + m_unitString));
		pdfDictionary.Items.Add(new PdfName("Type"), new PdfName("Measure"));
		pdfDictionary.Items.Add(new PdfName("X"), pdfArray3);
		pdfArray.Add(new PdfDictionary
		{
			Items = 
			{
				{
					new PdfName("C"),
					(IPdfPrimitive)new PdfNumber(1)
				},
				{
					new PdfName("D"),
					(IPdfPrimitive)new PdfNumber(100)
				},
				{
					new PdfName("F"),
					(IPdfPrimitive)new PdfName("D")
				},
				{
					new PdfName("RD"),
					(IPdfPrimitive)new PdfString(".")
				},
				{
					new PdfName("RT"),
					(IPdfPrimitive)new PdfString("")
				},
				{
					new PdfName("SS"),
					(IPdfPrimitive)new PdfString("")
				},
				{
					new PdfName("U"),
					(IPdfPrimitive)new PdfString(m_unitString)
				}
			}
		});
		pdfArray2.Add(new PdfDictionary
		{
			Items = 
			{
				{
					new PdfName("C"),
					(IPdfPrimitive)new PdfNumber(1)
				},
				{
					new PdfName("D"),
					(IPdfPrimitive)new PdfNumber(100)
				},
				{
					new PdfName("F"),
					(IPdfPrimitive)new PdfName("D")
				},
				{
					new PdfName("RD"),
					(IPdfPrimitive)new PdfString(".")
				},
				{
					new PdfName("RT"),
					(IPdfPrimitive)new PdfString("")
				},
				{
					new PdfName("SS"),
					(IPdfPrimitive)new PdfString("")
				},
				{
					new PdfName("U"),
					(IPdfPrimitive)new PdfString("sq " + m_unitString)
				}
			}
		});
		PdfDictionary pdfDictionary2 = new PdfDictionary();
		switch (m_unitString)
		{
		case "in":
			pdfDictionary2.Items.Add(new PdfName("C"), new PdfNumber(0.0138889));
			break;
		case "cm":
			pdfDictionary2.Items.Add(new PdfName("C"), new PdfNumber(0.0352778));
			break;
		case "mm":
			pdfDictionary2.Items.Add(new PdfName("C"), new PdfNumber(0.352778));
			break;
		case "pt":
			pdfDictionary2.Items.Add(new PdfName("C"), new PdfNumber(1));
			break;
		case "p":
			pdfDictionary2.Items.Add(new PdfName("C"), new PdfNumber(0.0833333));
			break;
		}
		pdfDictionary2.Items.Add(new PdfName("D"), new PdfNumber(100));
		pdfDictionary2.Items.Add(new PdfName("F"), new PdfName("D"));
		pdfDictionary2.Items.Add(new PdfName("RD"), new PdfString("."));
		pdfDictionary2.Items.Add(new PdfName("RT"), new PdfString(""));
		pdfDictionary2.Items.Add(new PdfName("SS"), new PdfString(""));
		pdfDictionary2.Items.Add(new PdfName("U"), new PdfString(m_unitString));
		pdfArray3.Add(pdfDictionary2);
		return pdfDictionary;
	}

	internal PdfTemplate CreateNormalAppearance(string overlayText, PdfFont font, bool repeat, PdfColor TextColor, PdfTextAlignment alignment, LineBorder Border)
	{
		if (Border.BorderWidth == 1)
		{
			m_borderWidth = Border.BorderLineWidth;
		}
		else
		{
			m_borderWidth = Border.BorderWidth;
		}
		RectangleF rect = new RectangleF(0f, 0f, Bounds.Width, Bounds.Height);
		PdfTemplate pdfTemplate = new PdfTemplate(rect);
		SetMatrix(pdfTemplate.m_content);
		PaintParams paintParams = new PaintParams();
		PdfGraphics graphics = pdfTemplate.Graphics;
		PdfBrush backBrush = null;
		PdfBrush pdfBrush = null;
		if (InnerColor.A != 0)
		{
			backBrush = new PdfSolidBrush(InnerColor);
		}
		pdfBrush = ((TextColor.A == 0) ? new PdfSolidBrush(new PdfColor(Color.Gray)) : new PdfSolidBrush(TextColor));
		float num = (float)Border.BorderWidth / 2f;
		paintParams.BackBrush = backBrush;
		RectangleF rectangleF = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
		if (Opacity < 1f)
		{
			PdfGraphicsState state = graphics.Save();
			graphics.SetTransparency(Opacity);
			FieldPainter.DrawRectangleAnnotation(graphics, paintParams, rectangleF.X + num, rectangleF.Y + num, rectangleF.Width - m_borderWidth, rectangleF.Height - m_borderWidth);
			graphics.Restore(state);
		}
		else
		{
			FieldPainter.DrawRectangleAnnotation(graphics, paintParams, rectangleF.X + num, rectangleF.Y + num, rectangleF.Width - m_borderWidth, rectangleF.Height - m_borderWidth);
		}
		float num2 = 0f;
		if (font == null)
		{
			font = new PdfStandardFont(PdfFontFamily.Helvetica, 10f);
		}
		int num3 = Convert.ToInt32(alignment);
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		SizeF sizeF = font.MeasureString(overlayText);
		if (repeat)
		{
			num2 = Bounds.Width / sizeF.Width;
			float num7 = (float)Math.Floor(Bounds.Height / font.Size);
			num6 = Math.Abs(Bounds.Width - (float)(Math.Floor(num2) * (double)sizeF.Width));
			if (num3 == 1)
			{
				num5 = num6 / 2f;
			}
			if (num3 == 2)
			{
				num5 = num6;
			}
			for (int i = 1; (float)i < num2; i++)
			{
				for (int j = 0; (float)j < num7; j++)
				{
					rectangleF = new RectangleF(num5, num4, 0f, 0f);
					graphics.DrawString(overlayText, font, pdfBrush, rectangleF);
					num4 += font.Size;
				}
				num5 += sizeF.Width;
				num4 = 0f;
			}
		}
		else
		{
			num6 = Math.Abs(Bounds.Width - sizeF.Width);
			if (num3 == 1)
			{
				num5 = num6 / 2f;
			}
			if (num3 == 2)
			{
				num5 = num6;
			}
			rectangleF = new RectangleF(num5, 0f, 0f, 0f);
			graphics.DrawString(overlayText, font, pdfBrush, rectangleF);
		}
		return pdfTemplate;
	}

	internal PdfTemplate CreateBorderAppearance(PdfColor BorderColor, LineBorder Border)
	{
		if (Border.BorderWidth == 1)
		{
			m_borderWidth = Border.BorderLineWidth;
		}
		else
		{
			m_borderWidth = Border.BorderWidth;
		}
		RectangleF rect = new RectangleF(0f, 0f, Bounds.Width, Bounds.Height);
		PdfTemplate pdfTemplate = new PdfTemplate(rect);
		PaintParams paintParams = new PaintParams();
		PdfGraphics graphics = pdfTemplate.Graphics;
		if (m_borderWidth > 0f && BorderColor.A != 0)
		{
			PdfPen borderPen = new PdfPen(BorderColor, m_borderWidth);
			paintParams.BorderPen = borderPen;
		}
		float num = m_borderWidth / 2f;
		paintParams.ForeBrush = new PdfSolidBrush(BorderColor);
		RectangleF rectangleF = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
		if (Opacity < 1f)
		{
			PdfGraphicsState state = graphics.Save();
			graphics.SetTransparency(Opacity);
			FieldPainter.DrawRectangleAnnotation(graphics, paintParams, rectangleF.X + num, rectangleF.Y + num, rectangleF.Width - m_borderWidth, rectangleF.Height - m_borderWidth);
			graphics.Restore(state);
			return pdfTemplate;
		}
		FieldPainter.DrawRectangleAnnotation(graphics, paintParams, rectangleF.X + num, rectangleF.Y + num, rectangleF.Width - m_borderWidth, rectangleF.Height - m_borderWidth);
		return pdfTemplate;
	}

	internal string FindOperator(int token)
	{
		return new string[79]
		{
			"b", "B", "bx", "Bx", "BDC", "BI", "BMC", "BT", "BX", "c",
			"cm", "CS", "cs", "d", "d0", "d1", "Do", "DP", "EI", "EMC",
			"ET", "EX", "f", "F", "fx", "G", "g", "gs", "h", "i",
			"ID", "j", "J", "K", "k", "l", "m", "M", "MP", "n",
			"q", "Q", "re", "RG", "rg", "ri", "s", "S", "SC", "sc",
			"SCN", "scn", "sh", "f*", "Tx", "Tc", "Td", "TD", "Tf", "Tj",
			"TJ", "TL", "Tm", "Tr", "Ts", "Tw", "Tz", "v", "w", "W",
			"W*", "Wx", "y", "T*", "b*", "B*", "'", "\"", "true"
		}.GetValue(token) as string;
	}

	internal string ColorToHex(Color c)
	{
		return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
	}

	internal void RemoveAnnoationFromPage(PdfPageBase page, PdfAnnotation annot)
	{
		if (page is PdfPage pdfPage)
		{
			pdfPage.Annotations.Remove(annot);
			return;
		}
		PdfLoadedPage pdfLoadedPage = page as PdfLoadedPage;
		PdfDictionary dictionary = pdfLoadedPage.Dictionary;
		PdfArray pdfArray = null;
		pdfArray = ((!dictionary.ContainsKey("Annots")) ? new PdfArray() : (pdfLoadedPage.CrossTable.GetObject(dictionary["Annots"]) as PdfArray));
		annot.Dictionary.SetProperty("P", new PdfReferenceHolder(pdfLoadedPage));
		pdfArray.Remove(new PdfReferenceHolder(annot));
		page.Dictionary.SetProperty("Annots", pdfArray);
	}

	internal void AssignLocation(PointF location)
	{
		m_rectangle.Location = location;
	}

	internal virtual void ApplyText(string text)
	{
		m_text = text;
		Dictionary.SetProperty("Contents", new PdfString(text));
	}

	internal void AssignSize(SizeF size)
	{
		m_rectangle.Size = size;
	}

	protected virtual void Initialize()
	{
		m_dictionary.BeginSave += Dictionary_BeginSave;
		m_dictionary.SetProperty("Type", new PdfName("Annot"));
	}

	internal virtual void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		PdfCatalog pdfCatalog = sender as PdfCatalog;
		if (IsContainsAnnotation())
		{
			Save();
			if (pdfCatalog != null)
			{
				m_dictionary.BeginSave -= Dictionary_BeginSave;
			}
		}
	}

	internal virtual void InstanceSave()
	{
		Save();
	}

	internal virtual void FlattenAnnot(bool flattenPopUps)
	{
	}

	protected virtual void Save()
	{
		if ((GetType().ToString().Contains("Pdf3DAnnotation") || GetType().ToString().Contains("PdfAttachmentAnnotation") || GetType().ToString().Contains("PdfSoundAnnotation") || GetType().ToString().Contains("PdfActionAnnotation")) && (PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A1B || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A1A || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_X1A2001))
		{
			throw new PdfConformanceException("The specified annotation type is not supported by PDF/A1-B or PDF/A1-A standard documents.");
		}
		if (m_border != null)
		{
			m_dictionary.SetProperty("Border", m_border);
		}
		RectangleF rectangle = ObtainNativeRectangle();
		if ((double)(int)m_innerColor.A != 0.0)
		{
			m_dictionary.SetProperty("IC", m_innerColor.ToArray());
		}
		m_dictionary.SetProperty("Rect", PdfArray.FromRectangle(rectangle));
		if ((double)m_opacity != 1.0)
		{
			Dictionary.SetNumber("CA", m_opacity);
		}
		if (LoadedPage == null)
		{
			return;
		}
		PdfLoadedDocument pdfLoadedDocument = LoadedPage.Document as PdfLoadedDocument;
		existingConformanceLevel = pdfLoadedDocument.Conformance;
		if (existingConformanceLevel == PdfConformanceLevel.Pdf_A1B)
		{
			if (AnnotationFlags == PdfAnnotationFlags.Invisible || AnnotationFlags == PdfAnnotationFlags.Hidden || AnnotationFlags == PdfAnnotationFlags.NoView)
			{
				m_flagBit = 0;
			}
			m_dictionary.SetNumber("F", m_flagBit);
		}
	}

	protected RectangleF CalculateBounds(RectangleF Bounds, PdfPage page, PdfLoadedPage loadedpage)
	{
		float x = Bounds.X;
		float y = Bounds.Y;
		float width = Bounds.Width;
		float height = Bounds.Height;
		SizeF sizeF = new SizeF(0f, 0f);
		PdfNumber pdfNumber = null;
		if (page != null)
		{
			PdfMargins margins = page.Document.PageSettings.Margins;
			sizeF = page.Size;
			if (page.Dictionary.ContainsKey("Rotate"))
			{
				pdfNumber = page.Dictionary["Rotate"] as PdfNumber;
			}
			else
			{
				PdfDictionary pdfDictionary = null;
				PdfReferenceHolder pdfReferenceHolder = page.Dictionary["Parent"] as PdfReferenceHolder;
				pdfDictionary = ((!(pdfReferenceHolder != null)) ? (page.Dictionary["Parent"] as PdfDictionary) : (pdfReferenceHolder.Object as PdfDictionary));
				if (pdfDictionary != null && pdfDictionary.ContainsKey("Rotate"))
				{
					pdfNumber = page.Dictionary.GetValue(page.CrossTable, "Rotate", "Parent") as PdfNumber;
				}
			}
			if (pdfNumber != null)
			{
				if (pdfNumber.IntValue == 90)
				{
					x = Bounds.Y;
					y = sizeF.Height - (margins.Left + margins.Right) - Bounds.X - Bounds.Width;
					width = Bounds.Height;
					height = Bounds.Width;
				}
				else if (pdfNumber.IntValue == 180)
				{
					x = sizeF.Width - (Bounds.X + Bounds.Width) - (margins.Left + margins.Right);
					y = sizeF.Height - Bounds.Y - (margins.Top + margins.Bottom) - Bounds.Height;
				}
				else if (pdfNumber.IntValue == 270)
				{
					x = sizeF.Width - Bounds.Bottom - (margins.Left + margins.Right);
					y = Bounds.X;
					width = Bounds.Height;
					height = Bounds.Width;
				}
			}
		}
		else if (loadedpage != null)
		{
			sizeF = loadedpage.Size;
			if (loadedpage.Dictionary.ContainsKey("Rotate"))
			{
				pdfNumber = loadedpage.Dictionary["Rotate"] as PdfNumber;
			}
			else
			{
				PdfDictionary pdfDictionary2 = null;
				PdfReferenceHolder pdfReferenceHolder2 = loadedpage.Dictionary["Parent"] as PdfReferenceHolder;
				pdfDictionary2 = ((!(pdfReferenceHolder2 != null)) ? (loadedpage.Dictionary["Parent"] as PdfDictionary) : (pdfReferenceHolder2.Object as PdfDictionary));
				if (pdfDictionary2 != null && pdfDictionary2.ContainsKey("Rotate"))
				{
					pdfNumber = loadedpage.Dictionary.GetValue(loadedpage.CrossTable, "Rotate", "Parent") as PdfNumber;
				}
			}
			if (pdfNumber != null)
			{
				if (pdfNumber.IntValue == 90)
				{
					x = sizeF.Height - Bounds.Y - Bounds.Height;
					y = Bounds.X;
					width = Bounds.Height;
					height = Bounds.Width;
				}
				else if (pdfNumber.IntValue == 180)
				{
					x = sizeF.Width - (Bounds.X + Bounds.Width);
					y = sizeF.Height - Bounds.Y - Bounds.Height;
				}
				else if (pdfNumber.IntValue == 270)
				{
					x = Bounds.Y;
					y = sizeF.Width - Bounds.X - Bounds.Width;
					width = Bounds.Height;
					height = Bounds.Width;
				}
			}
		}
		return new RectangleF(x, y, width, height);
	}

	internal void DrawAuthor(string author, string subject, RectangleF bounds, PdfBrush backBrush, PdfBrush aBrush, PdfPageBase page, out float trackingHeight, PdfAnnotationBorder border)
	{
		float num = Border.Width / 2f;
		RectangleF rectangle = new RectangleF(bounds.X + num, bounds.Y + num, bounds.Width - border.Width, 20f);
		if (subject != string.Empty && subject != null)
		{
			rectangle.Height += 20f;
			trackingHeight = rectangle.Height;
			SaveGraphics(page, PdfBlendMode.HardLight);
			page.Graphics.DrawRectangle(PdfPens.Black, backBrush, rectangle);
			page.Graphics.Restore();
			RectangleF layoutRectangle = new RectangleF(rectangle.X + 11f, rectangle.Y, rectangle.Width, rectangle.Height / 2f);
			SaveGraphics(page, PdfBlendMode.Normal);
			page.Graphics.DrawString(author, new PdfStandardFont(PdfFontFamily.Helvetica, 10.5f, PdfFontStyle.Bold), aBrush, layoutRectangle, new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Middle));
			layoutRectangle = new RectangleF(layoutRectangle.X, layoutRectangle.Y + layoutRectangle.Height - 2f, layoutRectangle.Width, rectangle.Height / 2f);
			DrawSubject(subject, layoutRectangle, page);
			page.Graphics.Restore();
		}
		else
		{
			SaveGraphics(page, PdfBlendMode.HardLight);
			page.Graphics.DrawRectangle(PdfPens.Black, backBrush, rectangle);
			page.Graphics.Restore();
			RectangleF layoutRectangle2 = new RectangleF(rectangle.X + 11f, rectangle.Y, rectangle.Width, rectangle.Height);
			SaveGraphics(page, PdfBlendMode.Normal);
			page.Graphics.DrawString(author, new PdfStandardFont(PdfFontFamily.Helvetica, 10.5f), aBrush, layoutRectangle2, new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Middle));
			trackingHeight = rectangle.Height;
			page.Graphics.Restore();
		}
	}

	internal void DrawSubject(string subject, RectangleF bounds, PdfPageBase page)
	{
		page.Graphics.DrawString(subject, new PdfStandardFont(PdfFontFamily.Helvetica, 10.5f, PdfFontStyle.Bold), PdfBrushes.Black, bounds, new PdfStringFormat(PdfTextAlignment.Left, PdfVerticalAlignment.Middle));
	}

	internal void FlattenPopup()
	{
		if (Page != null)
		{
			FlattenPopup(Page, Color, Bounds, Border, Author, Subject, Text);
		}
		else if (LoadedPage != null)
		{
			FlattenPopup(LoadedPage, Color, Bounds, Border, Author, Subject, Text);
		}
	}

	internal void FlattenPopup(PdfPageBase page, PdfColor color, RectangleF annotBounds, PdfAnnotationBorder border, string author, string subject, string text)
	{
		SizeF empty = SizeF.Empty;
		empty = ((!(page is PdfLoadedPage)) ? (page as PdfPage).GetClientSize() : (page as PdfLoadedPage).Size);
		float x = empty.Width - 180f;
		float y = ((annotBounds.Y + 142f < empty.Height) ? annotBounds.Y : (empty.Height - 142f));
		RectangleF bounds = new RectangleF(x, y, 180f, 142f);
		if (Dictionary["Popup"] != null && PdfCrossTable.Dereference(Dictionary["Popup"]) is PdfDictionary pdfDictionary)
		{
			PdfArray pdfArray = PdfCrossTable.Dereference(pdfDictionary["Rect"]) as PdfArray;
			PdfCrossTable pdfCrossTable = ((page is PdfPage) ? (page as PdfPage).CrossTable : (page as PdfLoadedPage).CrossTable);
			if (pdfArray != null)
			{
				PdfNumber pdfNumber = pdfCrossTable.GetObject(pdfArray[0]) as PdfNumber;
				PdfNumber pdfNumber2 = pdfCrossTable.GetObject(pdfArray[1]) as PdfNumber;
				PdfNumber pdfNumber3 = pdfCrossTable.GetObject(pdfArray[2]) as PdfNumber;
				PdfNumber pdfNumber4 = pdfCrossTable.GetObject(pdfArray[3]) as PdfNumber;
				bounds = new RectangleF(pdfNumber.FloatValue, pdfNumber2.FloatValue, pdfNumber3.FloatValue - pdfNumber.FloatValue, pdfNumber4.FloatValue - pdfNumber2.FloatValue);
			}
		}
		PdfBrush pdfBrush = new PdfSolidBrush(color);
		float num = border.Width / 2f;
		float trackingHeight = 0f;
		PdfBrush aBrush = new PdfSolidBrush(GetForeColor(color));
		if (author != null && author != string.Empty)
		{
			DrawAuthor(author, subject, bounds, pdfBrush, aBrush, page, out trackingHeight, border);
		}
		else if (subject != null && subject != string.Empty)
		{
			RectangleF rectangle = new RectangleF(bounds.X + num, bounds.Y + num, bounds.Width - border.Width, 40f);
			SaveGraphics(page, PdfBlendMode.HardLight);
			page.Graphics.DrawRectangle(PdfPens.Black, pdfBrush, rectangle);
			page.Graphics.Restore();
			RectangleF rectangleF = new RectangleF(rectangle.X + 11f, rectangle.Y, rectangle.Width, rectangle.Height / 2f);
			rectangleF = new RectangleF(rectangleF.X, rectangleF.Y + rectangleF.Height - 2f, rectangleF.Width, rectangle.Height / 2f);
			SaveGraphics(page, PdfBlendMode.Normal);
			DrawSubject(Subject, rectangleF, page);
			page.Graphics.Restore();
			trackingHeight = 40f;
		}
		else
		{
			SaveGraphics(page, PdfBlendMode.HardLight);
			RectangleF rectangle2 = new RectangleF(bounds.X + num, bounds.Y + num, bounds.Width - border.Width, 20f);
			page.Graphics.DrawRectangle(PdfPens.Black, pdfBrush, rectangle2);
			trackingHeight = 20f;
			page.Graphics.Restore();
		}
		RectangleF rectangleF2 = new RectangleF(bounds.X + num, bounds.Y + num + trackingHeight, bounds.Width - border.Width, bounds.Height - (trackingHeight + border.Width));
		SaveGraphics(page, PdfBlendMode.HardLight);
		page.Graphics.DrawRectangle(PdfPens.Black, PdfBrushes.White, rectangleF2);
		rectangleF2.X += 11f;
		rectangleF2.Y += 5f;
		rectangleF2.Width -= 22f;
		page.Graphics.Restore();
		SaveGraphics(page, PdfBlendMode.Normal);
		page.Graphics.DrawString(text, new PdfStandardFont(PdfFontFamily.Helvetica, 10.5f), PdfBrushes.Black, rectangleF2);
		page.Graphics.Restore();
	}

	internal void SaveGraphics(PdfPageBase page, PdfBlendMode blendMode)
	{
		page.Graphics.Save();
		page.Graphics.SetTransparency(0.8f, 0.8f, blendMode);
	}

	internal PdfColor GetForeColor(PdfColor c)
	{
		if ((c.R + c.B + c.G) / 3 <= 128)
		{
			return new PdfColor(DocGen.Drawing.Color.White);
		}
		return new PdfColor(DocGen.Drawing.Color.Black);
	}

	public void SetValues(string key, string value)
	{
		if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
		{
			Dictionary.SetProperty(key, new PdfString(value));
		}
	}

	protected PointF CalculateTemplateBounds(RectangleF Bounds, PdfPageBase page, PdfTemplate template)
	{
		PdfArray pdfArray = null;
		PdfArray pdfArray2 = null;
		PdfNumber pdfNumber = null;
		float num = Bounds.X;
		float num2 = Bounds.Y;
		if (page != null)
		{
			if (page.Dictionary.ContainsKey("CropBox"))
			{
				pdfArray = page.Dictionary["CropBox"] as PdfArray;
			}
			else if (page.Dictionary.ContainsKey("MediaBox"))
			{
				pdfArray2 = page.Dictionary["MediaBox"] as PdfArray;
			}
			if (page.Dictionary.ContainsKey("Rotate"))
			{
				pdfNumber = page.Dictionary["Rotate"] as PdfNumber;
			}
			else
			{
				PdfDictionary pdfDictionary = null;
				PdfReferenceHolder pdfReferenceHolder = page.Dictionary["Parent"] as PdfReferenceHolder;
				pdfDictionary = ((!(pdfReferenceHolder != null)) ? (page.Dictionary["Parent"] as PdfDictionary) : (pdfReferenceHolder.Object as PdfDictionary));
				if (pdfDictionary != null && pdfDictionary.ContainsKey("Rotate"))
				{
					pdfNumber = pdfDictionary["Rotate"] as PdfNumber;
				}
			}
			if (pdfNumber != null)
			{
				if (pdfNumber.IntValue == 90)
				{
					page.Graphics.TranslateTransform(template.Height, 0f);
					page.Graphics.RotateTransform(90f);
					num = Bounds.X;
					num2 = 0f - (page.Size.Height - Bounds.Y - Bounds.Height);
				}
				else if (pdfNumber.IntValue == 180)
				{
					page.Graphics.TranslateTransform(template.Width, template.Height);
					page.Graphics.RotateTransform(180f);
					num = 0f - (page.Size.Width - (Bounds.X + Bounds.Width));
					num2 = 0f - (page.Size.Height - Bounds.Y - Bounds.Height);
				}
				else if (pdfNumber.IntValue == 270)
				{
					page.Graphics.TranslateTransform(0f, template.Width);
					page.Graphics.RotateTransform(270f);
					num = 0f - (page.Size.Width - Bounds.X - Bounds.Width);
					num2 = Bounds.Y;
				}
			}
			if (pdfArray != null)
			{
				if ((pdfArray[0] as PdfNumber).FloatValue != 0f || (pdfArray[1] as PdfNumber).FloatValue != 0f)
				{
					num -= (pdfArray[0] as PdfNumber).FloatValue;
					num2 += (pdfArray[1] as PdfNumber).FloatValue;
				}
			}
			else if (pdfArray2 != null && ((pdfArray2[0] as PdfNumber).FloatValue != 0f || (pdfArray2[1] as PdfNumber).FloatValue != 0f))
			{
				num -= (pdfArray2[0] as PdfNumber).FloatValue;
				num2 += (pdfArray2[1] as PdfNumber).FloatValue;
			}
		}
		return new PointF(num, num2);
	}

	private PdfLayer GetDocumentLayer()
	{
		if (Dictionary.ContainsKey("OC"))
		{
			IPdfPrimitive pdfPrimitive = Dictionary["OC"];
			PdfLoadedPage loadedPage = m_loadedPage;
			if (pdfPrimitive != null && loadedPage != null && loadedPage.Document != null)
			{
				PdfDocumentLayerCollection layers = loadedPage.Document.Layers;
				if (layers != null)
				{
					IsMatched(layers, pdfPrimitive, loadedPage);
				}
			}
		}
		return layer;
	}

	private void IsMatched(PdfDocumentLayerCollection layerCollection, IPdfPrimitive expectedObject, PdfLoadedPage page)
	{
		for (int i = 0; i < layerCollection.Count; i++)
		{
			IPdfPrimitive referenceHolder = layerCollection[i].ReferenceHolder;
			if (referenceHolder != null && referenceHolder.Equals(expectedObject))
			{
				if (layerCollection[i].Name != null)
				{
					layer = layerCollection[i];
					break;
				}
			}
			else if (layerCollection[i].Layers != null && layerCollection[i].Layers.Count > 0)
			{
				IsMatched(layerCollection[i].Layers, expectedObject, page);
			}
		}
	}

	internal float GetRotationAngle()
	{
		PdfNumber pdfNumber = null;
		if (Dictionary.ContainsKey("Rotate"))
		{
			pdfNumber = PdfCrossTable.Dereference(Dictionary["Rotate"]) as PdfNumber;
		}
		else if (Dictionary.ContainsKey("Rotation"))
		{
			pdfNumber = PdfCrossTable.Dereference(Dictionary["Rotation"]) as PdfNumber;
		}
		if (pdfNumber == null)
		{
			pdfNumber = new PdfNumber(0);
		}
		return pdfNumber.FloatValue;
	}

	internal RectangleF GetRotatedBounds(RectangleF bounds, float rotateangle)
	{
		if (bounds.Width > 0f && bounds.Height > 0f)
		{
			PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
			pdfTransformationMatrix.Rotate(rotateangle);
			PointF[] array = new PointF[4]
			{
				new PointF(bounds.Left, bounds.Top),
				new PointF(bounds.Right, bounds.Top),
				new PointF(bounds.Right, bounds.Bottom),
				new PointF(bounds.Left, bounds.Bottom)
			};
			pdfTransformationMatrix.Matrix.TransformPoints(array);
			PdfPath pdfPath = new PdfPath();
			pdfPath.AddRectangle(bounds);
			for (int i = 0; i < 4; i++)
			{
				pdfPath.PathPoints[i] = array[i];
			}
			RectangleF result = CalculateBoundingBox(array);
			result.X = bounds.X;
			result.Y = bounds.Y;
			return result;
		}
		return bounds;
	}

	internal static RectangleF CalculateBoundingBox(PointF[] imageCoordinates)
	{
		float x = imageCoordinates[0].X;
		float x2 = imageCoordinates[3].X;
		float y = imageCoordinates[0].Y;
		float y2 = imageCoordinates[3].Y;
		for (int i = 0; i < 4; i++)
		{
			if (imageCoordinates[i].X < x)
			{
				x = imageCoordinates[i].X;
			}
			if (imageCoordinates[i].X > x2)
			{
				x2 = imageCoordinates[i].X;
			}
			if (imageCoordinates[i].Y < y)
			{
				y = imageCoordinates[i].Y;
			}
			if (imageCoordinates[i].Y > y2)
			{
				y2 = imageCoordinates[i].Y;
			}
		}
		return new RectangleF(x, y, x2 - x, y2 - y);
	}

	internal PdfTransformationMatrix GetRotatedTransformMatrix(PdfArray bbox, float angle)
	{
		PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
		if (angle == 0f)
		{
			pdfTransformationMatrix.Translate(0f - (bbox[0] as PdfNumber).FloatValue, 0f - (bbox[1] as PdfNumber).FloatValue);
		}
		else if (angle == 90f)
		{
			pdfTransformationMatrix.Translate((bbox[3] as PdfNumber).FloatValue, 0f - (bbox[0] as PdfNumber).FloatValue);
		}
		else if (angle == 180f)
		{
			pdfTransformationMatrix.Translate((bbox[2] as PdfNumber).FloatValue, (bbox[3] as PdfNumber).FloatValue);
		}
		else if (angle == 270f)
		{
			pdfTransformationMatrix.Translate(0f - (bbox[1] as PdfNumber).FloatValue, (bbox[2] as PdfNumber).FloatValue);
		}
		pdfTransformationMatrix.Rotate(angle);
		return pdfTransformationMatrix;
	}

	internal PdfTransformationMatrix GetRotatedTransformMatrixAngle(PdfArray bbox, float angle, RectangleF rectangle)
	{
		PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
		float num = 0.1f;
		float num2 = 0.1f;
		if (angle == 0f)
		{
			pdfTransformationMatrix.Translate(0f - ((PdfNumber)bbox[0]).FloatValue, 0f - ((PdfNumber)bbox[1]).FloatValue);
		}
		else if (angle == 90f)
		{
			pdfTransformationMatrix.Translate(((PdfNumber)bbox[3]).FloatValue, 0f - ((PdfNumber)bbox[0]).FloatValue);
		}
		else if (angle == 180f)
		{
			pdfTransformationMatrix.Translate(((PdfNumber)bbox[2]).FloatValue, ((PdfNumber)bbox[3]).FloatValue);
		}
		else if (angle == 270f)
		{
			pdfTransformationMatrix.Translate(0f - ((PdfNumber)bbox[1]).FloatValue, ((PdfNumber)bbox[2]).FloatValue);
		}
		if (angle % 90f != 0f)
		{
			PdfNumber pdfNumber = ((bbox != null && bbox.Count > 0) ? (PdfCrossTable.Dereference(bbox[0]) as PdfNumber) : new PdfNumber(0));
			PdfNumber pdfNumber2 = ((bbox != null && bbox.Count > 1) ? (PdfCrossTable.Dereference(bbox[1]) as PdfNumber) : new PdfNumber(0));
			float width = rectangle.Width;
			float height = rectangle.Height;
			if (pdfNumber != null && pdfNumber2 != null && pdfNumber.FloatValue != 0f && pdfNumber2.FloatValue != 0f)
			{
				float centerX = GetCenterX(angle, bbox, num);
				float centerY = GetCenterY(angle, bbox, num2);
				if (centerX > 0f)
				{
					if (width > centerX)
					{
						for (; Math.Round(width, 1) != Math.Round(GetCenterX(angle, bbox, num), 1) && Math.Round(width, 1) > Math.Round(GetCenterX(angle, bbox, num), 1); num += 0.1f)
						{
						}
					}
					else
					{
						while (Math.Round(width, 1) != Math.Round(GetCenterX(angle, bbox, num), 1) && Math.Round(width, 1) < Math.Round(GetCenterX(angle, bbox, num), 1))
						{
							num -= 0.1f;
						}
					}
				}
				else
				{
					for (; Math.Round(width, 1) != Math.Round(GetCenterX(angle, bbox, num), 1) && Math.Round(width, 1) > Math.Round(GetCenterX(angle, bbox, num), 1); num += 0.1f)
					{
					}
				}
				if (centerY > 0f)
				{
					if (height > centerY)
					{
						for (; Math.Round(height, 1) != Math.Round(GetCenterY(angle, bbox, num2), 1) && Math.Round(height, 1) > Math.Round(GetCenterY(angle, bbox, num2), 1); num2 += 0.1f)
						{
						}
					}
					else
					{
						while (Math.Round(height, 1) != Math.Round(GetCenterY(angle, bbox, num2), 1) && Math.Round(height, 1) < Math.Round(GetCenterY(angle, bbox, num2), 1))
						{
							num2 -= 0.1f;
						}
					}
				}
				else
				{
					for (; Math.Round(height, 1) != Math.Round(GetCenterY(angle, bbox, num2), 1) && Math.Round(height, 1) > Math.Round(GetCenterY(angle, bbox, num2), 1); num2 += 0.1f)
					{
					}
				}
				pdfTransformationMatrix.Translate(num, num2);
			}
			else
			{
				float num3 = Math.Abs(rectangle.Width - (float)Math.Round(GetCenterX(angle, bbox, num), 1));
				float num4 = Math.Abs(rectangle.Height - (float)Math.Round(GetCenterY(angle, bbox, num2), 1));
				if (Math.Round(rectangle.Width) > (double)num)
				{
					num = (float)Math.Round(num3 / num) * num;
				}
				if (Math.Round(rectangle.Height) > (double)num2)
				{
					num2 = (float)Math.Round(num4 / num2) * num2;
				}
				pdfTransformationMatrix.Translate(num, num2);
			}
		}
		pdfTransformationMatrix.Rotate(angle);
		return pdfTransformationMatrix;
	}

	private float GetCenterX(float angle, PdfArray bbox, float x)
	{
		PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
		pdfTransformationMatrix.Translate(x, x);
		pdfTransformationMatrix.Rotate(angle);
		RectangleF bounds = bbox.ToRectangle();
		float[] mMatrix = new float[6]
		{
			pdfTransformationMatrix.Matrix.Elements[0],
			pdfTransformationMatrix.Matrix.Elements[1],
			pdfTransformationMatrix.Matrix.Elements[2],
			pdfTransformationMatrix.Matrix.Elements[3],
			pdfTransformationMatrix.Matrix.Elements[4],
			pdfTransformationMatrix.Matrix.Elements[5]
		};
		return TransformBBoxByMatrix(bounds, mMatrix).Width;
	}

	private float GetCenterY(float angle, PdfArray bbox, float y)
	{
		PdfTransformationMatrix pdfTransformationMatrix = new PdfTransformationMatrix();
		pdfTransformationMatrix.Translate(y, y);
		pdfTransformationMatrix.Rotate(angle);
		RectangleF bounds = bbox.ToRectangle();
		float[] mMatrix = new float[6]
		{
			pdfTransformationMatrix.Matrix.Elements[0],
			pdfTransformationMatrix.Matrix.Elements[1],
			pdfTransformationMatrix.Matrix.Elements[2],
			pdfTransformationMatrix.Matrix.Elements[3],
			pdfTransformationMatrix.Matrix.Elements[4],
			pdfTransformationMatrix.Matrix.Elements[5]
		};
		return TransformBBoxByMatrix(bounds, mMatrix).Height;
	}

	private RectangleF TransformBBoxByMatrix(RectangleF bounds, float[] mMatrix)
	{
		float[] array = new float[4];
		float[] array2 = new float[4];
		PointF pointF = TransformPoint(bounds.Left, bounds.Bottom, mMatrix);
		array[0] = pointF.X;
		array2[0] = pointF.Y;
		PointF pointF2 = TransformPoint(bounds.Right, bounds.Top, mMatrix);
		array[1] = pointF2.X;
		array2[1] = pointF2.Y;
		PointF pointF3 = TransformPoint(bounds.Left, bounds.Top, mMatrix);
		array[2] = pointF3.X;
		array2[2] = pointF3.Y;
		PointF pointF4 = TransformPoint(bounds.Right, bounds.Bottom, mMatrix);
		array[3] = pointF4.X;
		array2[3] = pointF4.Y;
		return new RectangleF(MinValue(array), MinValue(array2), MaxValue(array), MaxValue(array2));
	}

	private float MaxValue(float[] value)
	{
		float num = value[0];
		for (int i = 1; i < value.Length; i++)
		{
			if (value[i] > num)
			{
				num = value[i];
			}
		}
		return num;
	}

	private float MinValue(float[] value)
	{
		float num = value[0];
		for (int i = 1; i < value.Length; i++)
		{
			if (value[i] < num)
			{
				num = value[i];
			}
		}
		return num;
	}

	private PointF TransformPoint(float x, float y, float[] matrix)
	{
		PointF result = default(PointF);
		result.X = matrix[0] * x + matrix[2] * y + matrix[4];
		result.Y = matrix[1] * x + matrix[3] * y + matrix[5];
		return result;
	}

	private void TriggerAnnotationSaveEvent(PdfPageBase page)
	{
		if (page is PdfPage)
		{
			m_page = page as PdfPage;
			if (Dictionary.ContainsKey("Subtype") || Flatten)
			{
				PdfName pdfName = Dictionary.Items[new PdfName("Subtype")] as PdfName;
				if (((pdfName != null && (pdfName.Value == "Text" || pdfName.Value == "Square" || Flatten)) || (!Dictionary.ContainsKey("Subtype") && Flatten)) && m_page.Document != null && m_page.Document.Catalog != null)
				{
					m_page.Document.Catalog.BeginSave += Dictionary_BeginSave;
					m_page.Document.Catalog.Modify();
				}
			}
		}
		else
		{
			if (!(page is PdfLoadedPage))
			{
				return;
			}
			m_loadedPage = page as PdfLoadedPage;
			if (!Dictionary.ContainsKey("Subtype") && !Flatten)
			{
				return;
			}
			PdfName pdfName2 = Dictionary.Items[new PdfName("Subtype")] as PdfName;
			if (((pdfName2 != null && (pdfName2.Value == "Circle" || pdfName2.Value == "RichMedia" || pdfName2.Value == "Square" || pdfName2.Value == "Line" || pdfName2.Value == "Polygon" || pdfName2.Value == "Ink" || pdfName2.Value == "FreeText" || pdfName2.Value == "Highlight" || pdfName2.Value == "Underline" || pdfName2.Value == "StrikeOut" || pdfName2.Value == "PolyLine" || pdfName2.Value == "Text" || pdfName2.Value == "Stamp" || pdfName2.Value == "Squiggly" || Flatten || pdfName2.Value == "Redact" || pdfName2.Value == "Link" || pdfName2.Value == "Widget" || pdfName2.Value == "FileAttachment" || pdfName2.Value == "Caret" || pdfName2.Value == "Watermark" || pdfName2.Value == "Screen" || pdfName2.Value == "3D" || pdfName2.Value == "Sound")) || (!Dictionary.ContainsKey("Subtype") && Flatten)) && LoadedPage.Document != null && LoadedPage.Document.Catalog != null)
			{
				if (!beginSaveEventTriggered)
				{
					LoadedPage.Document.Catalog.BeginSave += Dictionary_BeginSave;
					beginSaveEventTriggered = true;
				}
				LoadedPage.Document.Catalog.Modify();
			}
		}
	}

	internal PdfPopupIcon GetIconName(string name)
	{
		PdfPopupIcon pdfPopupIcon = PdfPopupIcon.NewParagraph;
		return name switch
		{
			"Note" => PdfPopupIcon.Note, 
			"Comment" => PdfPopupIcon.Comment, 
			"Help" => PdfPopupIcon.Help, 
			"Insert" => PdfPopupIcon.Insert, 
			"Key" => PdfPopupIcon.Key, 
			"NewParagraph" => PdfPopupIcon.NewParagraph, 
			"Paragraph" => PdfPopupIcon.Paragraph, 
			"Check" => PdfPopupIcon.Check, 
			"Circle" => PdfPopupIcon.Circle, 
			"Cross" => PdfPopupIcon.Cross, 
			"CrossHairs" => PdfPopupIcon.CrossHairs, 
			"RightArrow" => PdfPopupIcon.RightArrow, 
			"RightPointer" => PdfPopupIcon.RightPointer, 
			"Star" => PdfPopupIcon.Star, 
			"UpArrow" => PdfPopupIcon.UpArrow, 
			"UpLeftArrow" => PdfPopupIcon.UpLeftArrow, 
			_ => PdfPopupIcon.Note, 
		};
	}

	private PdfAnnotationRotateAngle GetRotateAngle()
	{
		int num = 90;
		PdfNumber pdfNumber = null;
		if (Dictionary.ContainsKey("Rotate"))
		{
			pdfNumber = PdfCrossTable.Dereference(Dictionary["Rotate"]) as PdfNumber;
		}
		else if (Dictionary.ContainsKey("Rotation") && this is PdfLoadedPolygonAnnotation)
		{
			pdfNumber = PdfCrossTable.Dereference(Dictionary["Rotation"]) as PdfNumber;
		}
		if (pdfNumber == null)
		{
			pdfNumber = new PdfNumber(0);
		}
		if (pdfNumber.IntValue < 0)
		{
			pdfNumber.IntValue = 360 + pdfNumber.IntValue;
		}
		return (PdfAnnotationRotateAngle)(pdfNumber.IntValue / num);
	}

	internal bool ValidateTemplateMatrix(PdfDictionary dictionary)
	{
		bool result = false;
		if (dictionary.ContainsKey("Matrix"))
		{
			if (PdfCrossTable.Dereference(dictionary["Matrix"]) is PdfArray { Count: >3 } pdfArray && pdfArray[0] != null && pdfArray[1] != null && pdfArray[2] != null && pdfArray[3] != null && (pdfArray[0] as PdfNumber).FloatValue == 1f && (pdfArray[1] as PdfNumber).FloatValue == 0f && (pdfArray[2] as PdfNumber).FloatValue == 0f && (pdfArray[3] as PdfNumber).FloatValue == 1f)
			{
				result = true;
				float num = 0f;
				float num2 = 0f;
				float num3 = 0f;
				float num4 = 0f;
				PdfArray pdfArray2 = null;
				if (Dictionary.ContainsKey("Rect"))
				{
					pdfArray2 = PdfCrossTable.Dereference(Dictionary["Rect"]) as PdfArray;
				}
				if (pdfArray.Count > 4)
				{
					num3 = 0f - (pdfArray[4] as PdfNumber).FloatValue;
					if (pdfArray.Count > 5)
					{
						num4 = 0f - (pdfArray[5] as PdfNumber).FloatValue;
					}
				}
				if (pdfArray2 != null && pdfArray2.Count > 1)
				{
					num = (PdfCrossTable.Dereference(pdfArray2[0]) as PdfNumber).FloatValue;
					num2 = (PdfCrossTable.Dereference(pdfArray2[1]) as PdfNumber).FloatValue;
				}
				if ((num != num3 || num2 != num4) && num3 == 0f && num4 == 0f)
				{
					m_locationDisplaced = true;
				}
			}
		}
		else
		{
			result = true;
		}
		return result;
	}

	internal int ObtainGraphicsRotation(PdfTransformationMatrix matrix)
	{
		int num = 0;
		num = (int)Math.Round(Math.Atan2(matrix.Matrix.Elements[2], matrix.Matrix.Elements[0]) * 180.0 / Math.PI);
		switch (num)
		{
		case -90:
			num = 90;
			break;
		case -180:
			num = 180;
			break;
		case 90:
			num = 270;
			break;
		}
		return num;
	}

	protected RectangleF CalculateTemplateBounds(RectangleF bounds, PdfPageBase page, PdfTemplate template, bool isNormalMatrix, PdfGraphics graphics)
	{
		RectangleF rectangleF = bounds;
		float x = bounds.X;
		float y = bounds.Y;
		float width = bounds.Width;
		float height = bounds.Height;
		if (!isNormalMatrix && PdfCrossTable.Dereference(Dictionary["Rect"]) is PdfArray pdfArray)
		{
			rectangleF = pdfArray.ToRectangle();
		}
		if (page != null)
		{
			int num = ObtainGraphicsRotation(graphics.Matrix);
			if (page is PdfPage)
			{
				if (num == 0 && !isNormalMatrix && (Rotate == PdfAnnotationRotateAngle.RotateAngle90 || Rotate == PdfAnnotationRotateAngle.RotateAngle270))
				{
					x = bounds.X;
					y = bounds.Y + bounds.Height - bounds.Width;
					width = bounds.Height;
					height = bounds.Width;
				}
			}
			else if (page is PdfLoadedPage)
			{
				switch (num)
				{
				case 90:
					graphics.TranslateTransform(template.Height, 0f);
					graphics.RotateTransform(90f);
					if (isNormalMatrix || Rotate == PdfAnnotationRotateAngle.RotateAngle180)
					{
						if (page.Origin.Y > 0f && Rotate == PdfAnnotationRotateAngle.RotateAngle0)
						{
							x = bounds.X;
							y = 0f - (page.Size.Height - (bounds.Height + bounds.Y) + (bounds.Width - template.Height));
						}
						else
						{
							x = bounds.X;
							y = (m_locationDisplaced ? ((page.Origin.Y == 0f) ? (0f - (page.Size.Height - (bounds.Height + bounds.Y) + (bounds.Height - template.Height))) : (bounds.Y + bounds.Height)) : (0f - (page.Size.Height - bounds.Y - bounds.Height)));
						}
					}
					else
					{
						x = bounds.X;
						y = 0f - (page.Size.Height - (bounds.Height + bounds.Y) + (bounds.Width - template.Height));
						width = bounds.Height;
						height = bounds.Width;
					}
					break;
				case 180:
					graphics.TranslateTransform(template.Width, template.Height);
					graphics.RotateTransform(180f);
					if (isNormalMatrix)
					{
						x = 0f - (page.Size.Width - (bounds.X + bounds.Width));
						y = 0f - (page.Size.Height - bounds.Y - bounds.Height);
						break;
					}
					x = 0f - (page.Size.Width - (bounds.X + template.Width));
					y = 0f - (page.Size.Height - bounds.Y - template.Height);
					if (Rotate == PdfAnnotationRotateAngle.RotateAngle90 || Rotate == PdfAnnotationRotateAngle.RotateAngle270)
					{
						y = 0f - (page.Size.Height - bounds.Y - template.Height) - (bounds.Width - bounds.Height);
						width = bounds.Height;
						height = bounds.Width;
					}
					break;
				case 270:
				{
					graphics.TranslateTransform(0f, template.Width);
					graphics.RotateTransform(270f);
					if (isNormalMatrix || Rotate == PdfAnnotationRotateAngle.RotateAngle180)
					{
						x = 0f - (page.Size.Width - bounds.X - bounds.Width);
						y = bounds.Y;
						break;
					}
					bool flag2 = false;
					PdfArray pdfArray4 = PdfCrossTable.Dereference(template.m_content["Matrix"]) as PdfArray;
					PdfArray pdfArray5 = PdfCrossTable.Dereference(template.m_content["BBox"]) as PdfArray;
					if (pdfArray4 != null && pdfArray5 != null)
					{
						PdfNumber obj3 = pdfArray4.Elements[5] as PdfNumber;
						PdfNumber pdfNumber3 = pdfArray5.Elements[2] as PdfNumber;
						if (obj3.FloatValue != pdfNumber3.FloatValue)
						{
							flag2 = true;
						}
					}
					x = 0f - (page.Size.Width - rectangleF.X - template.Width);
					PdfName pdfName = Dictionary.Items[new PdfName("Subtype")] as PdfName;
					if (!flag2)
					{
						y = ((!(pdfName.Value == "Screen") || Rotate != 0) ? (bounds.Y + bounds.Height - bounds.Width) : bounds.Y);
					}
					else if (pdfName.Value == "FreeText" && Rotate == PdfAnnotationRotateAngle.RotateAngle270)
					{
						x = template.Width - bounds.Width;
						y = bounds.Y - bounds.Width;
					}
					else
					{
						y = bounds.Y - (bounds.Height - bounds.Width);
					}
					width = bounds.Height;
					height = bounds.Width;
					break;
				}
				case 0:
				{
					bool flag = false;
					PdfArray pdfArray2 = PdfCrossTable.Dereference(template.m_content["Matrix"]) as PdfArray;
					PdfArray pdfArray3 = PdfCrossTable.Dereference(template.m_content["BBox"]) as PdfArray;
					if (pdfArray2 != null && pdfArray3 != null)
					{
						PdfNumber obj = pdfArray2.Elements[5] as PdfNumber;
						PdfNumber pdfNumber = pdfArray3.Elements[2] as PdfNumber;
						if (obj.FloatValue != pdfNumber.FloatValue)
						{
							flag = true;
						}
					}
					if (isNormalMatrix || (Rotate != PdfAnnotationRotateAngle.RotateAngle90 && Rotate != PdfAnnotationRotateAngle.RotateAngle270 && Rotate != 0))
					{
						break;
					}
					PdfName obj2 = Dictionary.Items[new PdfName("Subtype")] as PdfName;
					PdfNumber pdfNumber2 = pdfArray2.Elements[3] as PdfNumber;
					if (obj2.Value == "Polygon")
					{
						if (pdfNumber2.IntValue == 1)
						{
							break;
						}
						if (flag)
						{
							if (Rotate == PdfAnnotationRotateAngle.RotateAngle270)
							{
								x = bounds.X + Location.X - (bounds.Width + template.Height);
								y = bounds.Y - (bounds.Width * 2f + (template.Height + Size.Width + bounds.Height / 2f));
								width = bounds.Height;
								height = bounds.Width;
							}
							else if (Rotate == PdfAnnotationRotateAngle.RotateAngle0)
							{
								x = bounds.X - rectangleF.Y + (bounds.Height + bounds.Width / 2f);
								y = bounds.Y - rectangleF.Y - (bounds.Width + bounds.Height);
								width = bounds.Width;
								height = bounds.Height;
								graphics.bScaleTranform = false;
							}
						}
						else
						{
							x = bounds.X;
							y = bounds.Y + bounds.Height - bounds.Width;
						}
					}
					else
					{
						x = bounds.X;
						y = bounds.Y + bounds.Height - bounds.Width;
						width = bounds.Height;
						height = bounds.Width;
					}
					break;
				}
				}
				RectangleF rectangleF2 = new RectangleF(x, y, width, height);
				if (page != null && !page.Dictionary.ContainsKey("CropBox") && page.Dictionary.ContainsKey("MediaBox"))
				{
					PdfArray pdfArray6 = null;
					if (PdfCrossTable.Dereference(page.Dictionary["MediaBox"]) is PdfArray)
					{
						pdfArray6 = PdfCrossTable.Dereference(page.Dictionary["MediaBox"]) as PdfArray;
					}
					if (pdfArray6 != null && pdfArray6.Count > 3)
					{
						PdfNumber pdfNumber4 = pdfArray6[0] as PdfNumber;
						PdfNumber pdfNumber5 = pdfArray6[1] as PdfNumber;
						if (pdfNumber4 != null && pdfNumber5 != null && (pdfNumber4.FloatValue != 0f || pdfNumber5.FloatValue != 0f))
						{
							float x2 = rectangleF2.X;
							float y2 = rectangleF2.Y;
							if (!isAnnotationCreation)
							{
								x = 0f - pdfNumber4.FloatValue - (0f - x2);
								y = pdfNumber5.FloatValue + y2;
							}
						}
					}
				}
			}
		}
		return new RectangleF(x, y, width, height);
	}

	protected RectangleF CalculateTemplateBounds(RectangleF bounds, PdfPageBase page, PdfTemplate template, bool isNormalMatrix)
	{
		return CalculateTemplateBounds(bounds, page, template, isNormalMatrix, page.Graphics);
	}

	internal void SetMatrix(PdfDictionary template)
	{
		if (template["BBox"] is PdfArray pdfArray)
		{
			float[] array = new float[6]
			{
				1f,
				0f,
				0f,
				1f,
				0f - (pdfArray[0] as PdfNumber).FloatValue,
				0f - (pdfArray[1] as PdfNumber).FloatValue
			};
			template["Matrix"] = new PdfArray(array);
		}
	}

	internal RectangleF ObtainNativeRectangle()
	{
		RectangleF result = new RectangleF(m_rectangle.X, m_rectangle.Bottom, m_rectangle.Width, m_rectangle.Height);
		PdfArray pdfArray = null;
		SizeF sizeF = new SizeF(0f, 0f);
		if (m_page != null)
		{
			PdfSection section = m_page.Section;
			result.Location = section.PointToNativePdf(Page, result.Location);
			pdfArray = GetCropOrMediaBox(m_page, pdfArray);
		}
		else if (m_loadedPage != null)
		{
			result.Y = m_loadedPage.Size.Height - m_rectangle.Bottom;
			pdfArray = GetCropOrMediaBox(m_loadedPage, pdfArray);
		}
		if (pdfArray != null && pdfArray.Count > 2 && pdfArray[0] != null && pdfArray[1] != null)
		{
			if ((pdfArray[3] as PdfNumber).FloatValue < 0f)
			{
				float floatValue = (pdfArray[1] as PdfNumber).FloatValue;
				float floatValue2 = (pdfArray[3] as PdfNumber).FloatValue;
				(pdfArray[1] as PdfNumber).FloatValue = floatValue2;
				(pdfArray[3] as PdfNumber).FloatValue = floatValue;
			}
			if ((pdfArray[0] as PdfNumber).FloatValue != 0f || (pdfArray[1] as PdfNumber).FloatValue != 0f)
			{
				result.X += (pdfArray[0] as PdfNumber).FloatValue;
				if (m_loadedPage != null && m_loadedPage.Dictionary.ContainsKey("MediaBox") && !m_loadedPage.Dictionary.ContainsKey("CropBox") && pdfArray[3] is PdfNumber && (pdfArray[3] as PdfNumber).FloatValue == 0f && (pdfArray[1] as PdfNumber).FloatValue > 0f)
				{
					result.Y += (pdfArray[3] as PdfNumber).FloatValue;
				}
				else
				{
					result.Y += (pdfArray[1] as PdfNumber).FloatValue;
				}
			}
		}
		return result;
	}

	private bool IsContainsAnnotation()
	{
		bool flag = false;
		PdfArray pdfArray = null;
		if (Page != null && Page.Dictionary.ContainsKey("Annots"))
		{
			if (PdfCrossTable.Dereference(Page.Dictionary["Annots"]) is PdfArray pdfArray2 && pdfArray2.Contains(new PdfReferenceHolder(this)))
			{
				flag = true;
			}
		}
		else if (LoadedPage != null && LoadedPage.Dictionary.ContainsKey("Annots") && PdfCrossTable.Dereference(LoadedPage.Dictionary["Annots"]) is PdfArray pdfArray3 && pdfArray3.Contains(new PdfReferenceHolder(this)))
		{
			flag = true;
		}
		if (flag)
		{
			AddPopUpAnnotation();
		}
		return flag;
	}

	internal PdfArray GetCropOrMediaBox(PdfPageBase page, PdfArray cropOrMediaBox)
	{
		if (page != null && page.Dictionary.ContainsKey("CropBox"))
		{
			cropOrMediaBox = PdfCrossTable.Dereference(page.Dictionary["CropBox"]) as PdfArray;
		}
		else if (page != null && page.Dictionary.ContainsKey("MediaBox"))
		{
			cropOrMediaBox = PdfCrossTable.Dereference(page.Dictionary["MediaBox"]) as PdfArray;
		}
		return cropOrMediaBox;
	}

	internal PdfMargins ObtainMargin()
	{
		if (Page != null && Page.Section != null && Page.Section.PageSettings != null && Page.Section.PageSettings.Margins != null)
		{
			m_margins = Page.Section.PageSettings.Margins;
		}
		return m_margins;
	}

	protected void CheckFlatten()
	{
		if (LoadedPage != null)
		{
			PdfLoadedAnnotationCollection annotations = ((PdfPageBase)LoadedPage).Annotations;
			PdfLoadedAnnotationCollection annotations2 = LoadedPage.Annotations;
			if (annotations2 != null && annotations != null && (annotations2.Count > 0 || annotations.Count > 0) && (annotations.Flatten || annotations2.Flatten))
			{
				Flatten = true;
			}
		}
	}

	internal double GetAngle(float x1, float y1, float x2, float y2)
	{
		double num = 0.0;
		num = Math.Atan((double)(y2 - y1) / (double)(x2 - x1)) * (180.0 / Math.PI);
		if (x2 - x1 < 0f || y2 - y1 < 0f)
		{
			num += 180.0;
		}
		if (x2 - x1 > 0f && y2 - y1 < 0f)
		{
			num -= 180.0;
		}
		if (num < 0.0)
		{
			num += 360.0;
		}
		return num;
	}

	internal RectangleF CalculateLineBounds(float[] linePoints, int m_leaderLineExt, int m_leaderLine, int leaderOffset, PdfArray lineStyle, double borderLength)
	{
		RectangleF bounds = Bounds;
		PdfPath pdfPath = new PdfPath();
		if (linePoints != null && linePoints.Length == 4)
		{
			float num = linePoints[0];
			float num2 = linePoints[1];
			float num3 = linePoints[2];
			float num4 = linePoints[3];
			double num5 = 0.0;
			num5 = ((num3 - num != 0f) ? GetAngle(num, num2, num3, num4) : ((!(num4 > num2)) ? 270.0 : 90.0));
			int num6 = 0;
			double num7 = 0.0;
			if (m_leaderLine < 0)
			{
				num6 = -m_leaderLine;
				num7 = num5 + 180.0;
			}
			else
			{
				num6 = m_leaderLine;
				num7 = num5;
			}
			float[] value = new float[2] { num, num2 };
			float[] value2 = new float[2] { num3, num4 };
			if (leaderOffset != 0)
			{
				float[] axisValue = GetAxisValue(value, num7 + 90.0, leaderOffset);
				float[] axisValue2 = GetAxisValue(value2, num7 + 90.0, leaderOffset);
				linePoints[0] = (int)axisValue[0];
				linePoints[1] = (int)axisValue[1];
				linePoints[2] = (int)axisValue2[0];
				linePoints[3] = (int)axisValue2[1];
			}
			float[] axisValue3 = GetAxisValue(value, num7 + 90.0, num6 + leaderOffset);
			float[] axisValue4 = GetAxisValue(value2, num7 + 90.0, num6 + leaderOffset);
			float[] axisValue5 = GetAxisValue(value, num7 + 90.0, m_leaderLineExt + num6 + leaderOffset);
			float[] axisValue6 = GetAxisValue(value2, num7 + 90.0, m_leaderLineExt + num6 + leaderOffset);
			List<PointF> list = new List<PointF>();
			for (int i = 0; i < lineStyle.Count; i++)
			{
				PdfName pdfName = lineStyle[i] as PdfName;
				PointF item = default(PointF);
				if (pdfName != null)
				{
					switch (pdfName.Value.ToString())
					{
					case "Circle":
					case "Square":
					case "Diamond":
						item.X = 3f;
						item.Y = 3f;
						break;
					case "OpenArrow":
					case "ClosedArrow":
						item.X = 1f;
						item.Y = 5f;
						break;
					case "ROpenArrow":
					case "RClosedArrow":
						item.X = 9f + (float)(borderLength / 2.0);
						item.Y = 5f + (float)(borderLength / 2.0);
						break;
					case "Slash":
						item.X = 5f;
						item.Y = 9f;
						break;
					case "Butt":
						item.X = 1f;
						item.Y = 3f;
						break;
					default:
						item.X = 0f;
						item.Y = 0f;
						break;
					}
				}
				list.Add(item);
			}
			float[] array = new float[2];
			float[] array2 = new float[2];
			if ((num7 >= 45.0 && num7 <= 135.0) || (num7 >= 225.0 && num7 <= 315.0))
			{
				array[0] = list[0].Y;
				array2[0] = list[0].X;
				array[1] = list[1].Y;
				array2[1] = list[1].X;
			}
			else
			{
				array[0] = list[0].X;
				array2[0] = list[0].Y;
				array[1] = list[1].X;
				array2[1] = list[1].Y;
			}
			float num8 = Math.Max(array[0], array[1]);
			float num9 = Math.Max(array2[0], array2[1]);
			if (num8 == 0f)
			{
				num8 = 1f;
			}
			if (num9 == 0f)
			{
				num9 = 1f;
			}
			if (axisValue3[0] == Math.Min(axisValue3[0], axisValue4[0]))
			{
				axisValue3[0] -= num8 * (float)borderLength;
				axisValue4[0] += num8 * (float)borderLength;
				axisValue3[0] = Math.Min(axisValue3[0], linePoints[0]);
				axisValue3[0] = Math.Min(axisValue3[0], axisValue5[0]);
				axisValue4[0] = Math.Max(axisValue4[0], linePoints[2]);
				axisValue4[0] = Math.Max(axisValue4[0], axisValue6[0]);
			}
			else
			{
				axisValue3[0] += num8 * (float)borderLength;
				axisValue4[0] -= num8 * (float)borderLength;
				axisValue3[0] = Math.Max(axisValue3[0], linePoints[0]);
				axisValue3[0] = Math.Max(axisValue3[0], axisValue5[0]);
				axisValue4[0] = Math.Min(axisValue4[0], linePoints[2]);
				axisValue4[0] = Math.Min(axisValue4[0], axisValue6[0]);
			}
			if (axisValue3[1] == Math.Min(axisValue3[1], axisValue4[1]))
			{
				axisValue3[1] -= num9 * (float)borderLength;
				axisValue4[1] += num9 * (float)borderLength;
				axisValue3[1] = Math.Min(axisValue3[1], linePoints[1]);
				axisValue3[1] = Math.Min(axisValue3[1], axisValue5[1]);
				axisValue4[1] = Math.Max(axisValue4[1], linePoints[3]);
				axisValue4[1] = Math.Max(axisValue4[1], axisValue6[1]);
			}
			else
			{
				axisValue3[1] += num9 * (float)borderLength;
				axisValue4[1] -= num9 * (float)borderLength;
				axisValue3[1] = Math.Max(axisValue3[1], linePoints[1]);
				axisValue3[1] = Math.Max(axisValue3[1], axisValue5[1]);
				axisValue4[1] = Math.Min(axisValue4[1], linePoints[3]);
				axisValue4[1] = Math.Min(axisValue4[1], axisValue6[1]);
			}
			pdfPath.AddLine(axisValue3[0], axisValue3[1], axisValue4[0], axisValue4[1]);
			bounds = pdfPath.GetBounds();
		}
		return bounds;
	}

	internal PdfGraphics GetLayerGraphics()
	{
		PdfGraphics result = null;
		if (Layer != null)
		{
			PdfLayer pdfLayer = Layer;
			if (pdfLayer.LayerId == null)
			{
				pdfLayer.LayerId = "OCG_" + Guid.NewGuid();
			}
			PdfPageBase page = ((Page != null) ? ((PdfPageBase)Page) : ((PdfPageBase)LoadedPage));
			result = pdfLayer.CreateGraphics(page);
		}
		return result;
	}

	internal float[] GetAxisValue(float[] value, double angle, double length)
	{
		double num = Math.PI / 180.0;
		return new float[2]
		{
			value[0] + (float)(Math.Cos(angle * num) * length),
			value[1] + (float)(Math.Sin(angle * num) * length)
		};
	}

	internal void SetLineEndingStyles(float[] startingPoint, float[] endingPoint, PdfGraphics graphics, double angle, PdfPen m_borderPen, PdfBrush m_backBrush, PdfArray lineStyle, double borderLength)
	{
		float[] array = new float[2];
		if (borderLength == 0.0)
		{
			borderLength = 1.0;
			m_borderPen = null;
		}
		if (m_backBrush is PdfSolidBrush && (m_backBrush as PdfSolidBrush).Color.IsEmpty)
		{
			m_backBrush = null;
		}
		for (int i = 0; i < lineStyle.Count; i++)
		{
			PdfName pdfName = lineStyle[i] as PdfName;
			array = ((i != 0) ? endingPoint : startingPoint);
			if (pdfName != null)
			{
				switch (pdfName.Value.ToString())
				{
				case "Square":
				{
					RectangleF rectangle = new RectangleF(array[0] - (float)(3.0 * borderLength), 0f - (array[1] + (float)(3.0 * borderLength)), (float)(6.0 * borderLength), (float)(6.0 * borderLength));
					graphics.DrawRectangle(m_borderPen, m_backBrush, rectangle);
					break;
				}
				case "Circle":
				{
					RectangleF rectangle2 = new RectangleF(array[0] - (float)(3.0 * borderLength), 0f - (array[1] + (float)(3.0 * borderLength)), (float)(6.0 * borderLength), (float)(6.0 * borderLength));
					graphics.DrawEllipse(m_borderPen, m_backBrush, rectangle2);
					break;
				}
				case "OpenArrow":
				{
					int num4 = 0;
					num4 = ((i != 0) ? 150 : 30);
					double length7 = 9.0 * borderLength;
					float[] array5 = ((i != 0) ? GetAxisValue(array, angle, 0.0 - borderLength) : GetAxisValue(array, angle, borderLength));
					float[] axisValue15 = GetAxisValue(array5, angle + (double)num4, length7);
					float[] axisValue16 = GetAxisValue(array5, angle - (double)num4, length7);
					PdfPath pdfPath2 = new PdfPath(m_borderPen);
					pdfPath2.AddLine(array5[0], 0f - array5[1], axisValue15[0], 0f - axisValue15[1]);
					pdfPath2.AddLine(array5[0], 0f - array5[1], axisValue16[0], 0f - axisValue16[1]);
					graphics.DrawPath(m_borderPen, pdfPath2);
					break;
				}
				case "ClosedArrow":
				{
					int num3 = 0;
					num3 = ((i != 0) ? 150 : 30);
					double length6 = 9.0 * borderLength;
					float[] array4 = ((i != 0) ? GetAxisValue(array, angle, 0.0 - borderLength) : GetAxisValue(array, angle, borderLength));
					float[] axisValue13 = GetAxisValue(array4, angle + (double)num3, length6);
					float[] axisValue14 = GetAxisValue(array4, angle - (double)num3, length6);
					PointF[] points3 = new PointF[3]
					{
						new PointF(array4[0], 0f - array4[1]),
						new PointF(axisValue13[0], 0f - axisValue13[1]),
						new PointF(axisValue14[0], 0f - axisValue14[1])
					};
					graphics.DrawPolygon(m_borderPen, m_backBrush, points3);
					break;
				}
				case "ROpenArrow":
				{
					int num2 = 0;
					num2 = ((i != 0) ? 30 : 150);
					double length5 = 9.0 * borderLength;
					float[] array3 = ((i != 0) ? GetAxisValue(array, angle, borderLength) : GetAxisValue(array, angle, 0.0 - borderLength));
					float[] axisValue11 = GetAxisValue(array3, angle + (double)num2, length5);
					float[] axisValue12 = GetAxisValue(array3, angle - (double)num2, length5);
					PdfPath pdfPath = new PdfPath(m_borderPen);
					pdfPath.AddLine(array3[0], 0f - array3[1], axisValue11[0], 0f - axisValue11[1]);
					pdfPath.AddLine(array3[0], 0f - array3[1], axisValue12[0], 0f - axisValue12[1]);
					graphics.DrawPath(m_borderPen, pdfPath);
					break;
				}
				case "RClosedArrow":
				{
					int num = 0;
					num = ((i != 0) ? 30 : 150);
					double length4 = 9.0 * borderLength;
					float[] array2 = ((i != 0) ? GetAxisValue(array, angle, borderLength) : GetAxisValue(array, angle, 0.0 - borderLength));
					float[] axisValue9 = GetAxisValue(array2, angle + (double)num, length4);
					float[] axisValue10 = GetAxisValue(array2, angle - (double)num, length4);
					PointF[] points2 = new PointF[3]
					{
						new PointF(array2[0], 0f - array2[1]),
						new PointF(axisValue9[0], 0f - axisValue9[1]),
						new PointF(axisValue10[0], 0f - axisValue10[1])
					};
					graphics.DrawPolygon(m_borderPen, m_backBrush, points2);
					break;
				}
				case "Slash":
				{
					double length3 = 9.0 * borderLength;
					float[] axisValue7 = GetAxisValue(array, angle + 60.0, length3);
					float[] axisValue8 = GetAxisValue(array, angle - 120.0, length3);
					graphics.DrawLine(m_borderPen, array[0], 0f - array[1], axisValue7[0], 0f - axisValue7[1]);
					graphics.DrawLine(m_borderPen, array[0], 0f - array[1], axisValue8[0], 0f - axisValue8[1]);
					break;
				}
				case "Diamond":
				{
					double length2 = 3.0 * borderLength;
					float[] axisValue3 = GetAxisValue(array, 180.0, length2);
					float[] axisValue4 = GetAxisValue(array, 90.0, length2);
					float[] axisValue5 = GetAxisValue(array, 0.0, length2);
					float[] axisValue6 = GetAxisValue(array, -90.0, length2);
					PointF[] points = new PointF[4]
					{
						new PointF(axisValue3[0], 0f - axisValue3[1]),
						new PointF(axisValue4[0], 0f - axisValue4[1]),
						new PointF(axisValue5[0], 0f - axisValue5[1]),
						new PointF(axisValue6[0], 0f - axisValue6[1])
					};
					graphics.DrawPolygon(m_borderPen, m_backBrush, points);
					break;
				}
				case "Butt":
				{
					double length = 3.0 * borderLength;
					float[] axisValue = GetAxisValue(array, angle + 90.0, length);
					float[] axisValue2 = GetAxisValue(array, angle - 90.0, length);
					graphics.DrawLine(m_borderPen, axisValue[0], 0f - axisValue[1], axisValue2[0], 0f - axisValue2[1]);
					break;
				}
				}
			}
		}
	}

	private void AddPopUpAnnotation()
	{
		if (Dictionary != null && Dictionary.ContainsKey("Subtype"))
		{
			PdfName pdfName = PdfCrossTable.Dereference(Dictionary["Subtype"]) as PdfName;
			if (pdfName != null && (pdfName.Value == "FreeText" || pdfName.Value == "Sound" || pdfName.Value == "FileAttachment"))
			{
				return;
			}
		}
		if (Popup != null && m_popupAnnotations.Contains(Popup))
		{
			bool flag = false;
			if (Popup.Dictionary != null && Popup.Dictionary.ContainsKey("Subtype"))
			{
				PdfName pdfName2 = PdfCrossTable.Dereference(Popup.Dictionary["Subtype"]) as PdfName;
				if (pdfName2 != null)
				{
					flag = pdfName2.Value == "Popup";
				}
			}
			if (flag && Page != null && !Page.Annotations.Contains(Popup))
			{
				Page.Annotations.m_savePopup = true;
				Page.Annotations.Add(Popup);
				Page.Annotations.m_savePopup = false;
				m_popupAnnotations.Remove(Popup);
			}
			else if (flag && LoadedPage != null && !LoadedPage.Annotations.Contains(Popup))
			{
				LoadedPage.Annotations.m_savePopup = true;
				LoadedPage.Annotations.Add(Popup);
				LoadedPage.Annotations.m_savePopup = false;
				m_popupAnnotations.Remove(Popup);
			}
		}
		else if (LoadedPage != null && this is PdfLoadedAnnotation)
		{
			PdfAnnotation popup = (this as PdfLoadedAnnotation).Popup;
			if (popup != null && popup is PdfPopupAnnotation)
			{
				LoadedPage.Annotations.m_savePopup = true;
				LoadedPage.Annotations.Add(popup);
				LoadedPage.Annotations.m_savePopup = false;
			}
		}
	}

	internal string GetXmlFormattedString(string value)
	{
		value = value.Replace("&", "&amp;");
		value = value.Replace("<", "&lt;");
		value = value.Replace(">", "&gt;");
		return value;
	}
}

using SupportApi.Models;
using GrapeCity.Documents.Pdf;
using GrapeCity.Documents.Pdf.AcroForms;
using GrapeCity.Documents.Pdf.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Collections.Concurrent;
using System.IO;

namespace SupportApi.Utils
{
    public class DataParser
    {

        public RectangleF rect;
        public string title; // (user name)
        public string contents;
        public bool isRichContents;
        public bool open;
        public string subject;
        public string name;
        public Color color;
        public float? borderWidth;
        public BorderStyle? borderStyleType;
        public float[] lineDashPattern;
        public Dictionary<string, dynamic> annotationData;
        public Page page;

        public static Dictionary<string, dynamic> DeserializeAnnotationFromJson(string json)
        {
            var annotationData = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);
            if (annotationData.ContainsKey("rect"))
            {
                try
                {
                    JArray arr = annotationData["rect"];
                    int x1 = arr[0].ToObject<int>(), y1 = arr[1].ToObject<int>(), x2 = arr[2].ToObject<int>(), y2 = arr[3].ToObject<int>();
                    annotationData["rect"] = new int[] { x1, y1, x2, y2 };
                }
                catch (Exception)
                {
                    throw;
                }
            }
            if (annotationData.ContainsKey("inverted_rect"))
                annotationData.Remove("inverted_rect");
            if (annotationData.ContainsKey("__contentsStateValue"))
                annotationData.Remove("__contentsStateValue");
            if (annotationData.ContainsKey("color"))
            {
                Color? colorVal = ParseColorValue(annotationData["color"], null);
                if (colorVal.HasValue)
                {
                    annotationData["color"] = ColorUtil.ColorToRgbCss(colorVal.Value);
                }
                else {
                    annotationData["color"] = null;
                }
            }
            return annotationData;
        }

        public DocumentOptions documentOptions { get; }

        public RectangleF pageBounds;
        private readonly ConcurrentDictionary<string, byte[]> _attachedFiles;



        /// <summary>
        /// 現在のデータパーサによって作成または更新された注釈オブジェクト
        /// </summary>
        public AnnotationBase Annotation { get; internal set; }

        /// <summary>
        /// 現在のデータパーサによって作成または更新されたフィールドオブジェクト
        /// </summary>
        public Field Field { get; internal set; }

        /// <summary>
        /// データパーサを使用して作成または更新されたウィジェットオブジェクト
        /// </summary>
        public WidgetAnnotation Widget { get; internal set; } = null;

        public DataParser(dynamic annotation, Page page, DocumentOptions documentOptions, ConcurrentDictionary<string, byte[]> attachedFiles)
        {
            _attachedFiles = attachedFiles;
            Dictionary<string, dynamic> annotationData = annotation.annotation;
            this.annotationData = annotationData;
            this.page = page;
            this.documentOptions = documentOptions;
            pageBounds = page.Bounds; ;
            RectangleF rect;
            if (TryParseFloatArray("rect", out float[] floatArr))
            {
                float x1 = floatArr[0], y1 = floatArr[1], x2 = floatArr[2], y2 = floatArr[3];
                rect = new RectangleF(x1, ChangeOriginFromBottomToTop(y1 + (y2 - y1)), x2 - x1, y2 - y1);
            }
            else
            {
                rect = new RectangleF(0, 0, 0, 0);
            }
            
            this.rect = rect;
            title = annotationData.ContainsKey("title") ? annotationData["title"].ToString() : string.Empty;
            contents = annotationData.ContainsKey("contents") ? annotationData["contents"].ToString() : string.Empty;
            isRichContents = ParseBool("isRichContents");
            open = ParseBool("open");
            subject = ParseString("subject");
            name = ParseString("name");
            color = ParseColor("color", Color.Transparent).Value;
            float? borderWidth = null;
            BorderStyle? borderStyleType = null;
            float[] lineDashPattern = null;
            if (annotationData.ContainsKey("borderStyle"))
            {
                var borderStyle = annotationData["borderStyle"] as JObject;
                if (borderStyle != null)
                {
                    if (borderStyle.ContainsKey("width"))
                    {
                        if (float.TryParse(borderStyle["width"].ToString(), out var borderWidthVal))
                        {
                            borderWidth = borderWidthVal;
                        }

                    }
                    if (borderStyle.ContainsKey("style"))
                    {
                        if (int.TryParse(borderStyle["style"].ToString(), out var borderStyleTypeVal))
                        {
                            if (borderStyleTypeVal < 0 || borderStyleTypeVal > 6)
                                borderStyleType = BorderStyle.Unknown;
                            else
                                borderStyleType = (BorderStyle)borderStyleTypeVal;
                        }
                    }

                    if (TryParseFloatArray(borderStyle, "dashArray", out float[] dashArrayVal))
                    {
                        lineDashPattern = dashArrayVal;
                    }

                }
            }
            this.borderWidth = borderWidth;
            this.borderStyleType = borderStyleType;
            this.lineDashPattern = lineDashPattern;
        }


        /// <summary>
        /// 注釈ID（注：新しい注釈のための一時的なIDの可能性があります）
        /// </summary>
        public string id
        {
            get
            {
                if (!annotationData.ContainsKey("id"))
                    return "";
                return annotationData["id"];
            }
        }



        public string fileId
        {
            get
            {
                if (!annotationData.ContainsKey("fileId"))
                    return "";
                return annotationData["fileId"];
            }
        }

        public string fileName
        {
            get
            {
                if (!annotationData.ContainsKey("fileName"))
                    return "";
                return annotationData["fileName"];
            }
        }

        public byte[] GetAttachedFileBytes(string fileId)
        {
            if (_attachedFiles != null && _attachedFiles.ContainsKey(fileId))
            {
                if (_attachedFiles.TryGetValue(fileId, out var bytes))
                {
                    return bytes;
                }
            }
            return null;
        }

        public Stream GetAttachedFileStream(string fileId)
        {
            if(_attachedFiles != null && _attachedFiles.ContainsKey(fileId))
            {
                if(_attachedFiles.TryGetValue(fileId, out var bytes))
                {
                    return new MemoryStream(bytes);
                }
            }
            return null;
        }

        /// <summary>
        /// ポップアップ注釈の親の注釈ID
        /// </summary>
        public string parentId
        {
            get
            {
                if (!annotationData.ContainsKey("parentId"))
                    return "";
                return annotationData["parentId"];
            }
        }

        /**
        * この注釈が属する大元の注釈のID（"返信先"の値）
        * */
        public string referenceAnnotationId
        {
            get
            {
                if (!annotationData.ContainsKey("referenceAnnotationId"))
                    return "";
                return annotationData["referenceAnnotationId"];
            }
        }

        public PdfObjID pdfObjID
        {
            get
            {
                return IdToPdfObjID(id);
            }
        }

        /// <summary>
        /// 順番のインデックス。注釈は順序のインデックスに応じてソート。"-1" の値は順序が未指定という意味
        /// </summary>
        public int orderIndex
        {
            get
            {
                return ParseNumber("orderIndex", -1);
            }
        }

        public AnnotationTypeCode annotationType
        {
            get
            {
                if (annotationData.ContainsKey("annotationType"))
                    return (AnnotationTypeCode)DataParser.ConvertToIntValue(annotationData["annotationType"]);
                else
                    return AnnotationTypeCode.UNKNOWN;
            }
        }

        public bool IsWidget
        {
            get
            {
                var annotationType = this.annotationType;
                return annotationType == AnnotationTypeCode.WIDGET || annotationType == AnnotationTypeCode.SIGNATURE;
            }
        }
        public bool IsRadioButton
        {
            get
            {
                return ParseString("fieldType") == "Btn" && ParseBool("radioButton");
            }
        }

        public string FieldName
        {
            get { 
                return ParseString("fieldName");
            }
        }

        public bool HasPrintableFlag
        {
            get
            {
                return ParseBool("printableFlag");
            }
        }

        public string GetDocumentCurrentUserName()
        {
            string userName = documentOptions.userName;
            if (string.IsNullOrEmpty(userName))
                userName = string.IsNullOrEmpty(title) ? "Unknown" : title;
            return userName;
        }

        public Color BorderColor
        {
            get
            {
                return ParseColor("borderColor", Color.Empty).Value;
            }
        }

        public Color NonStrokeColor
        {
            get
            {
                Color fillColor = ParseColor("nonStrokeColor", Color.Empty).Value;
                if (fillColor.IsEmpty)
                {
                    // appearanceColorプロパティのための下位互換
                    fillColor = ParseColor("appearanceColor", Color.Empty).Value;
                }
                return fillColor;
            }
        }

        public Color StrokeColor
        {
            get
            {
                Color strokeColor = ParseColor("strokeColor", Color.Empty).Value;
                if (strokeColor.IsEmpty)
                {
                    // appearanceColorプロパティのための下位互換
                    strokeColor = ParseColor("appearanceColor", Color.Empty).Value;
                }
                return strokeColor;
            }
        }

        public static PdfObjID IdToPdfObjID(string annotationId)
        {
            if (string.IsNullOrEmpty(annotationId))
                return PdfObjID.Empty;
            if (int.TryParse(annotationId.ToUpperInvariant().Replace("R", ""), out int result))
            {
                return new PdfObjID(result, 0);
            }
            return PdfObjID.Empty;
        }

        public static string  PdfObjIDtoId(PdfObjID pdfObjId)
        {
            if (pdfObjId.IsEmpty())
                return "";
            string[] arr = pdfObjId.ToString().Split(' ');
            string res = arr[0] + "R";
            return res;
        }


        public float ChangeOriginFromBottomToTop(float top)
        {
            RectangleF pageBounds = this.pageBounds;
            return (pageBounds.Y + pageBounds.Height) - top;
        }

        public bool ParseBool( string key, bool defaultValue = false)
        {
            Dictionary<string, dynamic> data = this.annotationData;
            if (data.ContainsKey(key))
            {
                if (data[key] is bool)
                    return (bool)data[key];
                else
                {
                    return $"{data[key]}".ToBoolean(defaultValue);
                }
            }
            return defaultValue;
        }

        internal static void CollectAnnotationsData(out List<DataParser> annotationsToUpdate, out List<DataParser> annotationsToCreate,
                        out List<KeyValuePair<int, PdfObjID>> annotationToRemove, out List<DataParser> allClientAnnotations, 
                        DocumentModifications _modifications, GcPdfDocument _doc, OpenDocumentInfo documentInfo,
                       ConcurrentDictionary<string, byte[]> attachedFiles)
        {
            DocumentOptions documentOptions = documentInfo != null ? documentInfo.documentOptions : null;
            if (documentOptions == null)
        {
                documentOptions = new DocumentOptions();
            }
            annotationsToUpdate = new List<DataParser>();
            annotationsToCreate = new List<DataParser>();
            allClientAnnotations = new List<DataParser>();
            annotationToRemove = new List<KeyValuePair<int, PdfObjID>>();

            var updatedAnnotations = _modifications.annotationsData.updatedAnnotations;
            var newAnnotations = _modifications.annotationsData.newAnnotations;
            if (updatedAnnotations != null)
            {
                foreach (var updatedAnnotation in updatedAnnotations)
                {
                    Page page = _doc.Pages[updatedAnnotation.pageIndex];
                    var dataParser = new DataParser(updatedAnnotation, page, documentOptions, attachedFiles);
                    annotationsToUpdate.Add(dataParser);
                }
            }
            if (newAnnotations != null)
            {
                foreach (var newAnnotation in newAnnotations)
                {
                    var page = _doc.Pages[newAnnotation.pageIndex];
                    var dataParser = new DataParser(newAnnotation, page, documentOptions, attachedFiles);
                    annotationsToCreate.Add(dataParser);
                }
            }
            allClientAnnotations.AddRange(annotationsToUpdate);
            allClientAnnotations.AddRange(annotationsToCreate);

            var removedAnnotations = _modifications.annotationsData.removedAnnotations;
            if (removedAnnotations != null)
            {
                foreach (var removedAnnotation in removedAnnotations)
                {
                    if (!string.IsNullOrEmpty(removedAnnotation.annotationId))
                    {
                        var removedAnnotationId = removedAnnotation.annotationId;
                        PdfObjID objID = DataParser.IdToPdfObjID(removedAnnotationId);
                        annotationToRemove.Add(new KeyValuePair<int, PdfObjID>(removedAnnotation.pageIndex, objID));
                    }
                }
            }
        }

        public bool ConvertToContent
        {
            get
            {
                return ParseBool("convertToContent");
            }
        }

        public bool ContainsDataKey(string key)
        {
            return annotationData.ContainsKey(key);
        }

        public static bool HasFlag(AnnotationBase annotation, AnnotationFlags flag)
        {
            return (annotation.Flags & flag) == flag;
        }

        public static void RemoveFlag(AnnotationBase annotation, AnnotationFlags flag)
        {
            if (HasFlag(annotation, flag))
                annotation.Flags ^= flag;
        }

        public static void SetFlag(AnnotationBase annotation, AnnotationFlags flag)
        {
            if (!HasFlag(annotation, flag))
                annotation.Flags |= flag;
        }

        public string ParseString(string key, string defaultValue = "")
        {
            return annotationData.ContainsKey(key) ? (annotationData[key] != null ? annotationData[key].ToString() : defaultValue) : defaultValue;
        }

        public string ParseString(JObject obj, string key, string defaultValue = "")
        {
            if (obj == null)
                return defaultValue;
            return obj.ContainsKey(key) ? (obj[key] != null ? obj[key].ToString() : defaultValue) : defaultValue;
        }

        public int ParseNumber(string key, int defaultValue = 0)
        {
            return annotationData.ContainsKey(key) ? (annotationData[key] != null ? DataParser.ConvertToIntValue(annotationData[key]) : defaultValue) : defaultValue;
        }

        public float ParseFloat(string key, float defaultValue = 0f)
        {
            return annotationData.ContainsKey(key) ? (annotationData[key] != null ? DataParser.ConvertToFloatValue(annotationData[key]) : defaultValue) : defaultValue;
        }

        public bool TryParseArray(string key, out JToken[] arrayValue)
        {
            if (!annotationData.ContainsKey(key))
            {
                arrayValue = null;
                return false;
            }
            return TryParseArrayInternal(annotationData[key], out arrayValue);
        }

        public bool TryParseArray(JObject dictionary, string key, out JToken[] arrayValue)
        {
            if (!dictionary.ContainsKey(key))
            {
                arrayValue = null;
                return false;
            }
            return TryParseArrayInternal(dictionary[key], out arrayValue);
        }

        public bool TryParseFloatArray(string key, out float[] arrayValue)
        {
            if (!annotationData.ContainsKey(key))
            {
                arrayValue = null;
                return false;
            }
            return TryParseFloatArrayInternal(annotationData[key], out arrayValue);
        }

        public bool TryParseFloatArray(JObject dictionary, string key, out float[] arrayValue)
        {
            if (!dictionary.ContainsKey(key))
            {
                arrayValue = null;
                return false;
            }
            return TryParseFloatArrayInternal(dictionary[key], out arrayValue);
        }

        public bool TryParseColor(string key, out Color color)
        {
            var c = ParseColor(key);
            if (c.HasValue)
            {
                color = c.Value;
                return true;
            }
            color = Color.Empty;
            return false;
        }

        public static Color? ParseColorValue(dynamic obj, Color? defaultValue = null)
        {
            try
            {
                if (obj == null)
                    return defaultValue;
                if(obj is Color)
                {
                    return (Color)obj;
                }
                JArray colorArr = obj as JArray;
                if (colorArr != null)
                {
                    int r = int.Parse(colorArr[0].ToString());
                    int g = int.Parse(colorArr[1].ToString());
                    int b = int.Parse(colorArr[2].ToString());
                    int a = colorArr.Count > 3 ? ConvertToIntValue(colorArr[3]) * 255 : -1;
                    if (a != -1)
                        return Color.FromArgb(a, r, g, b);
                    else
                        return Color.FromArgb(r, g, b);
                }
                JObject colorObj = obj as JObject;
                if (colorObj != null)
                {
                    int r = int.Parse(colorObj.GetValue("0").ToString());
                    int g = int.Parse(colorObj.GetValue("1").ToString());
                    int b = int.Parse(colorObj.GetValue("2").ToString());
                    int a = -1;
                    if (colorObj.TryGetValue("3", out var token))
                        a = ConvertToIntValue(token) * 255;
                    if (a != -1)
                        return Color.FromArgb(a, r, g, b);
                    else
                        return Color.FromArgb(r, g, b);
                }
                Color? hexColor = ColorUtil.HexToColor(obj.ToString());
                return hexColor ?? defaultValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public Color? ParseColor(string key, Color? defaultValue = null)
        {
            if (annotationData.ContainsKey(key))
                return ParseColorValue(annotationData[key], defaultValue);
            else
                return defaultValue;
        }

        #region ** private implementations

        private bool TryParseFloatArrayInternal(dynamic obj, out float[] arrayValue)
        {
            try
            {
                if (obj == null)
                {
                    arrayValue = null;
                    return false;
                }
                float[] floatArr = obj as float[];
                if (floatArr != null)
                {
                    arrayValue = floatArr;
                    return true;
                }
                int[] intArr = obj as int[];
                if (intArr != null)
                {
                    arrayValue = new float[intArr.Length];
                    for (int i = 0; i < intArr.Length; i++)
                        arrayValue[i] = intArr[i];
                    return true;
                }
                JArray arr = obj as JArray;
                if (arr != null)
                {
                    arrayValue = new float[arr.Count];
                    for (int i = 0; i < arr.Count; i++)
                        arrayValue[i] = ConvertToFloatValue(arr[i]);
                    return true;
                }
                JObject arrObj = obj as JObject;
                if (arrObj != null)
                {
                    arrayValue = new float[arrObj.Count];
                    int i = 0;
                    while (i < arrayValue.Length)
                    {
                        if (arrObj.ContainsKey(i.ToString()))
                        {
                            arrayValue[i] = ConvertToFloatValue(arrObj[i.ToString()]);
                        }
                        else
                        {
                            arrayValue[i] = 0;
                        }
                        i++;
                    }
                    return true;
                }
                arrayValue = null;
                return false;
            }
            catch (Exception)
            {
                arrayValue = null;
                return false;
            }
        }

        public float? ParseLinkDestFloatValue(string key, JToken[] destArray = null, int? destArrayIndex = null, float? defaultValue = null)
        {
            float? result = null;
            if (destArray != null && destArrayIndex.HasValue && destArrayIndex.Value < destArray.Length)
            {
                result = ConvertToNullableFloatValue(destArray[destArrayIndex.Value]);
            }
            if (!string.IsNullOrEmpty(key) && !result.HasValue && ContainsDataKey(key))
            {
                if(annotationData.TryGetValue(key, out var val))
                    result = ConvertToNullableFloatValue(val);
            }
            if (!result.HasValue)
                return defaultValue;
            return result;
        }

        public static float ConvertToFloatValue(JToken jToken)
        {
            try
            {
                float value = jToken.ToObject<float>();
                return value;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static float? ConvertToNullableFloatValue(dynamic val)
        {
            if (val is int @int)
            {
                return @int;
            }
            else if (val is long @long)
            {
                return @long;
            }
            else if (val is double @double)
            {
                return (float)@double;
            }
            else if (val is decimal @decimal)
            {
                return (float)@decimal;
            }
            else
            {
                try
                {
                    JToken jToken = val as JToken;
                    if (jToken == null)
                        return null;
                    float value = jToken.ToObject<float>();
                    return value;
                }
                catch (Exception)
                {
                    return null;
                }
            }

        }

        public static int ConvertToIntValue(JToken jToken)
        {
            try
            {
                int value = jToken.ToObject<int>();
                return value;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private bool TryParseArrayInternal(JToken obj, out JToken[] arrayValue)
        {
            try
            {
                if (obj == null)
                {
                    arrayValue = null;
                    return false;
                }
                JArray arr = obj as JArray;
                if (arr != null)
                {
                    arrayValue = new JToken[arr.Count];
                    for (int i = 0; i < arr.Count; i++)
                        arrayValue[i] = arr[i];
                    return true;
                }
                JObject arrObj = obj as JObject;
                if (arrObj != null)
                {
                    arrayValue = new JToken[arrObj.Count];
                    int i = 0;
                    while (i < arrayValue.Length)
                    {
                        if (arrObj.ContainsKey(i.ToString()))
                        {
                            arrayValue[i] = arrObj[i.ToString()];
                        }
                        else
                        {
                            arrayValue[i] = null;
                        }
                        i++;
                    }
                    return true;
                }
                arrayValue = null;
                return false;
            }
            catch (Exception)
            {
                arrayValue = null;
                return false;
            }
        }

        public string FontName
        {
            get
            {
                return ParseString("fontName", "");
            }
        }

        public GrapeCity.Documents.Text.Font StandardFont
        {
            get
            {
                var fontName = FontName;
                switch (fontName)
                {
                    case "Helv":
                    case "Helvetica":
                        return StandardFonts.Helvetica;
                    case "HelveticaItalic":
                        return StandardFonts.HelveticaItalic;
                    case "HelveticaBold":
                        return StandardFonts.HelveticaBold;
                    case "HelveticaBoldItalic":
                        return StandardFonts.HelveticaBoldItalic;
                    case "Times":
                    case "TimesRegular":
                        return StandardFonts.Times;
                    case "TimesItalic":
                        return StandardFonts.TimesItalic;
                    case "TimesBold":
                        return StandardFonts.TimesBold;
                    case "TimesBoldItalic":
                        return StandardFonts.TimesBoldItalic;
                    case "Courier":
                    case "CourierRegular":
                        return StandardFonts.Courier;
                    case "CourierItalic":
                        return StandardFonts.CourierItalic;
                    case "CourierBold":
                        return StandardFonts.CourierBold;
                    case "CourierBoldItalic":
                        return StandardFonts.CourierBoldItalic;
                    case "Symbol":
                        return StandardFonts.Symbol;
                    case "ZaDb":
                    case "ZapfDingbats":
                        return StandardFonts.ZapfDingbats;
                    default:
                        return null;
                }
            }
        }

        internal string getDefaultAppearanceString(Color? strokeColor = null, Color? nonStrokeColor = null)
        {
            if (!strokeColor.HasValue)
                strokeColor = StrokeColor;
            if (!nonStrokeColor.HasValue)
                nonStrokeColor = NonStrokeColor;

            string fontSizePart = $"{ParseNumber("fontSize", 12)} Tf";
            string fontNamePart = "";
            string fontName = this.FontName;
            if (!string.IsNullOrEmpty(fontName))
            {
                fontNamePart = $"/{fontName}"; //例: "/Helv"
            }
            string nonStrokeColorPart = "";
            string fillColorStr = ColorToPdfString(nonStrokeColor.Value);
            if (!string.IsNullOrEmpty(fillColorStr))
                nonStrokeColorPart = $"{fillColorStr} rg";
            string strokeColorPart = "";
            string strokeColorStr = ColorToPdfString(strokeColor.Value);
            if (!string.IsNullOrEmpty(strokeColorStr))
                strokeColorPart = $"{strokeColorStr} RG";
            string da = $"{strokeColorPart} {nonStrokeColorPart} {fontNamePart} {fontSizePart}".Replace("  ", " ");
            return da;
        }

        private static string ColorToPdfString(Color color)
        {
            if (color.IsEmpty)
                return "";
            return $"{(color.R / 255f).ToString(CultureInfo.InvariantCulture)} {(color.G / 255f).ToString(CultureInfo.InvariantCulture)} {(color.B / 255f).ToString(CultureInfo.InvariantCulture)}";
        }

        #endregion

    }

}

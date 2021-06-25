using System;
using System.Collections.Generic;
using System.Drawing;
using GrapeCity.Documents.Pdf;
using GrapeCity.Documents.Pdf.Annotations;
using SupportApi.Models;
using Newtonsoft.Json.Linq;
using GrapeCity.Documents.Pdf.Actions;
using System.Linq;
using GrapeCity.Documents.Pdf.Graphics;

namespace SupportApi.Utils
{

    /// <summary>
    /// AnnotationFactory クラス
    /// </summary>
    public class AnnotationFactory
    {
        #region ** public methods

        /// <summary>
        /// 注釈を作成
        /// </summary>
        /// <param name="annotationData"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static AnnotationBase CreateAnnotation(DataParser dataParser, Page page)
        {
            AnnotationBase annotation = null;
            AnnotationTypeCode annotationType = dataParser.annotationType;
            switch (annotationType)
            {
                case AnnotationTypeCode.STAMP:
                    annotation = UpdateAnnotation(new StampAnnotation(), dataParser);
                    break;
                case AnnotationTypeCode.CIRCLE:
                    annotation = UpdateAnnotation(new CircleAnnotation(), dataParser);
                    break;
                case AnnotationTypeCode.LINE:
                    annotation = UpdateAnnotation(new LineAnnotation(), dataParser);
                    break;
                case AnnotationTypeCode.LINK:
                    annotation = UpdateAnnotation(new LinkAnnotation(), dataParser);
                    break;
                case AnnotationTypeCode.SQUARE:
                    annotation = UpdateAnnotation(new SquareAnnotation(), dataParser);
                    break;
                case AnnotationTypeCode.REDACT:
                    annotation = UpdateAnnotation(new RedactAnnotation(), dataParser);
                    break;
                case AnnotationTypeCode.FREETEXT:
                    annotation = UpdateAnnotation(new FreeTextAnnotation(), dataParser);
                    break;
                case AnnotationTypeCode.TEXT:
                    annotation = UpdateAnnotation(new TextAnnotation(), dataParser);
                    break;
                case AnnotationTypeCode.INK:
                    annotation = UpdateAnnotation(new InkAnnotation(), dataParser);
                    break;
                case AnnotationTypeCode.POLYLINE:
                    annotation = UpdateAnnotation(new PolyLineAnnotation(), dataParser);
                    break;
                case AnnotationTypeCode.POLYGON:
                    annotation = UpdateAnnotation(new PolygonAnnotation(), dataParser);
                    break;
                case AnnotationTypeCode.FILEATTACHMENT:
                    annotation = UpdateAnnotation(new FileAttachmentAnnotation(), dataParser);
                    break;
                case AnnotationTypeCode.SOUND:
                    annotation = UpdateAnnotation(new SoundAnnotation(), dataParser);
                    break;
                case AnnotationTypeCode.POPUP:
                    annotation = UpdateAnnotation(new PopupAnnotation(), dataParser);
                    break;
            }
            return annotation;
        }

        /// <summary>
        /// 指定した注釈をページのコンテンツに変換し、ページから削除
        /// </summary>
        /// <param name="annotations"></param>
        public static void ConvertAnnotationsToContent(List<AnnotationBase> annotations)
        {
            List<IGrouping<Page, AnnotationBase>> pageGroups = annotations.GroupBy(a => a.Page).ToList();
            foreach(var group in pageGroups)
            {
                Page page = group.Key;
                List<AnnotationBase> pageAnnotations = group.ToList();
                page.DrawAnnotations(page.Graphics, page.Bounds, pageAnnotations);
                foreach (var annotation in pageAnnotations)
                {
                    page.Annotations.Remove(annotation);
                }
            }
        }

        /// <summary>
        /// 注釈を更新
        /// </summary>
        /// <param name="annotation"></param>
        /// <param name="annotationData"></param>
        /// <param name="page"></param>
        public static void UpdateAnnotation(AnnotationTypeCode annotationType, AnnotationBase annotation, DataParser dataParser)
        {            
            switch (annotationType)
            {
                case AnnotationTypeCode.STAMP:
                    UpdateAnnotation((StampAnnotation)annotation, dataParser);
                    break;
                case AnnotationTypeCode.CIRCLE:
                    UpdateAnnotation((CircleAnnotation)annotation, dataParser);
                    break;
                case AnnotationTypeCode.LINE:
                    UpdateAnnotation((LineAnnotation)annotation, dataParser);
                    break;
                case AnnotationTypeCode.LINK:
                    UpdateAnnotation((LinkAnnotation)annotation, dataParser);
                    break;
                case AnnotationTypeCode.SQUARE:
                    UpdateAnnotation((SquareAnnotation)annotation, dataParser);
                    break;
                case AnnotationTypeCode.REDACT:
                    UpdateAnnotation((RedactAnnotation)annotation, dataParser);
                    break;
                case AnnotationTypeCode.FREETEXT:
                    UpdateAnnotation((FreeTextAnnotation)annotation, dataParser);
                    break;
                case AnnotationTypeCode.TEXT:
                    UpdateAnnotation((TextAnnotation)annotation, dataParser);
                    break;
                case AnnotationTypeCode.INK:
                    UpdateAnnotation((InkAnnotation)annotation, dataParser);
                    break;
                case AnnotationTypeCode.POLYLINE:
                    UpdateAnnotation((PolyLineAnnotation)annotation, dataParser);
                    break;
                case AnnotationTypeCode.POLYGON:
                    UpdateAnnotation((PolygonAnnotation)annotation, dataParser);
                    break;
                case AnnotationTypeCode.FILEATTACHMENT:
                    UpdateAnnotation((FileAttachmentAnnotation)annotation, dataParser);
                    break;
                case AnnotationTypeCode.SOUND:
                    UpdateAnnotation((SoundAnnotation)annotation, dataParser);
                    break;
                case AnnotationTypeCode.POPUP:
                    UpdateAnnotation((PopupAnnotation)annotation, dataParser);
                    break;
            }
        }

        #endregion

        #region ** private implementation

        private static void _UpdateAnnotationBase(AnnotationBase annotation, DataParser dataParser)
        {
            if (dataParser.HasPrintableFlag)
                DataParser.SetFlag(annotation, AnnotationFlags.Print);
            else
                DataParser.RemoveFlag(annotation, AnnotationFlags.Print);
        }

        private static void _UpdateMarkupAnnotation(MarkupAnnotation annotation, DataParser dataParser)
        {
            _UpdateAnnotationBase(annotation, dataParser);
            annotation.Rect = dataParser.rect;
            if (!string.IsNullOrEmpty(dataParser.subject))
                annotation.Subject = dataParser.subject;
            if (!string.IsNullOrEmpty(dataParser.name))
                annotation.Name = dataParser.name;
            if (!dataParser.color.IsEmpty)
                annotation.Color = dataParser.color;
            if (dataParser.ContainsDataKey("contents") || dataParser.ContainsDataKey("isRichContents"))
            {
                if (dataParser.isRichContents)
                {
                    annotation.Contents = null;
                    annotation.RichText = dataParser.contents;
                }
                else
                {
                    annotation.RichText = null;
                    annotation.Contents = dataParser.contents;
                }
            }
            annotation.UserName = dataParser.title;
            string referenceType = dataParser.ParseString("referenceType");
            if (!string.IsNullOrEmpty(referenceType))
            {
                if (referenceType.Equals("Group"))
                {
                    annotation.ReferenceType = AnnotationReferenceType.Group;
                }
                else
                {
                    annotation.ReferenceType = AnnotationReferenceType.Reply;                    
                }
            }
            if (annotation.ReferenceAnnotation != null && string.IsNullOrEmpty(dataParser.referenceAnnotationId))
            {
                annotation.ReferenceAnnotation = null;
            }
            var opacity = 1f;
            if (dataParser.ContainsDataKey("opacity"))
                opacity = dataParser.ParseFloat("opacity", 1f);
            annotation.Opacity = opacity;
        }

        private static AnnotationBase UpdateAnnotation(PopupAnnotation annotation, DataParser dataParser)
        {
            _UpdateAnnotationBase(annotation, dataParser);
            annotation.Rect = dataParser.rect;
            if (!string.IsNullOrEmpty(dataParser.name))
                annotation.Name = dataParser.name;
            if (!dataParser.color.IsEmpty)
                annotation.Color = dataParser.color;
            annotation.Contents = dataParser.contents;
            annotation.Open = dataParser.open;            
            return annotation;
        }

        private static AnnotationBase UpdateAnnotation(FileAttachmentAnnotation annotation, DataParser dataParser)
        {
            _UpdateMarkupAnnotation(annotation, dataParser);
            var data = dataParser.annotationData;
            // 添付ファイル:
            if (!string.IsNullOrEmpty(dataParser.fileId))
            {
                byte[] bytes = dataParser.GetAttachedFileBytes(dataParser.fileId);
                if (bytes != null)
                {
                    try
                    {
                        annotation.File = FileSpecification.FromEmbeddedStream(dataParser.fileName, EmbeddedFileStream.FromBytes(dataParser.page.Doc, bytes));
                    }
                    catch (Exception)
                    {
#if DEBUG
                        throw;
#endif
                    }
                }
                else if (annotation.File != null)
                {
                    annotation.File.Desc = dataParser.fileName;
                }
            }
            else
            {
                annotation.File = null;
            }
            // アイコン:
            if (!string.IsNullOrEmpty(dataParser.name))
            {
                annotation.Name = dataParser.name;
                if (Enum.TryParse(dataParser.name, out FileAttachmentAnnotationIcon icon))
                {
                    annotation.Icon = icon;
                }
            }
            return annotation;
        }

        private static AnnotationBase UpdateAnnotation(SoundAnnotation annotation, DataParser dataParser)
        {
            _UpdateMarkupAnnotation(annotation, dataParser);
            var data = dataParser.annotationData;
            // 音声注釈:
            if (!string.IsNullOrEmpty(dataParser.fileId))
            {
                var stream = dataParser.GetAttachedFileStream(dataParser.fileId);
                if (stream != null)
                {
                    try
                    {
                        annotation.Sound = SoundObject.FromStream(stream);
                    }
                    catch (Exception)
                    {
#if DEBUG
                        throw;
#endif
                    }
                }
            }
            else
            {
                annotation.Sound = null;
            }
            // アイコン
            if (!string.IsNullOrEmpty(dataParser.name))
            {
                annotation.Name = dataParser.name;
                if (!string.IsNullOrEmpty(dataParser.name))
                {
                    annotation.Icon = dataParser.name;
                }
            }
            return annotation;
        }

        private static AnnotationBase UpdateAnnotation(StampAnnotation annotation, DataParser dataParser)
        {
            _UpdateMarkupAnnotation(annotation, dataParser);
            if (!string.IsNullOrEmpty(dataParser.fileId))
            {
                var stream = dataParser.GetAttachedFileStream(dataParser.fileId);
                if (stream != null)
                {
                    try
                    {
                        var image = GrapeCity.Documents.Drawing.Image.FromStream(stream);
                        var doc = dataParser.page.Doc;
                        var imageSize = new SizeF(image.Width, image.Height);
                        var fxo = new FormXObject(doc, new RectangleF(PointF.Empty, imageSize));
                        fxo.Graphics.DrawImage(image, fxo.Bounds, null, GrapeCity.Documents.Drawing.ImageAlign.StretchImage);
                        annotation.AppearanceStreams.Normal.Default = fxo;

                    }
                    catch (Exception)
                    {
                        stream.Dispose();
                        // Possible error: "Could not load file or assembly 'System.Runtime.CompilerServices.Unsafe'"
                        // Please, check the assemblyBinding in the Web.config.
                        // Minimum assembly versions for the GcImaging library are:
                        //   System.Runtime.CompilerServices.Unsafe - 4.5.0
                        //   System.Buffers - 4.4.0
                        //   System.Memory - 4.5.0
                        //   System.ValueTuple - 4.5.0
                        // Use nuget package manager to install recent assembly versions, e.g.:
                        //   Install-Package System.Runtime.CompilerServices.Unsafe -Version 5.0.0
#if DEBUG
                        throw;
#endif
                    }
                }
            }
            return annotation;
        }

        private static AnnotationBase UpdateAnnotation(CircleAnnotation annotation, DataParser dataParser)
        {
            _UpdateMarkupAnnotation(annotation, dataParser);
            if (dataParser.borderWidth.HasValue)
                annotation.LineWidth = dataParser.borderWidth.Value;
            if (dataParser.lineDashPattern != null)
                annotation.LineDashPattern = dataParser.lineDashPattern;
            if (dataParser.borderStyleType.HasValue)
            {
                if (dataParser.borderStyleType.Value != BorderStyle.Dashed)
                    annotation.LineDashPattern = null;
            }
            annotation.FillColor = dataParser.ParseColor("interiorColor", Color.Transparent).Value;            
            return annotation;
        }

        private static AnnotationBase UpdateAnnotation(LinkAnnotation annotation, DataParser dataParser)
        {
            annotation.Rect = dataParser.rect;

            if (dataParser.borderWidth.HasValue)
                annotation.Border.Width = dataParser.borderWidth.Value;
            if (dataParser.lineDashPattern != null)
                annotation.Border.DashPattern = dataParser.lineDashPattern;
            if (dataParser.borderStyleType.HasValue)
                annotation.Border.Style = dataParser.borderStyleType.Value;
            else
                annotation.Border.Style = BorderStyle.None;
            annotation.Border.Color = dataParser.color;

            var url = dataParser.ParseString("url");
            var action = dataParser.ParseString("action");
            var js = dataParser.ParseString("jsAction");
            

            string linkType = dataParser.ParseString("linkType");
            if (string.IsNullOrEmpty(linkType) || linkType == "unknown")
            {
                if (!string.IsNullOrEmpty(url))
                {
                    linkType = "url";
                }
                else if (dataParser.ContainsDataKey("dest"))
                {                    
                    linkType = "dest";
                }
                else if (!string.IsNullOrEmpty(action))
                {
                    linkType = "action";
                }
                else if (!string.IsNullOrEmpty(js))
                {
                    linkType = "js";
                }
                else
                {
                    linkType = "unknown";
                }
                //'url' | 'action' | 'dest' | 'js' | 'unknown'
            }
            switch (linkType)
            {
                case "url":
                    var newWindow = dataParser.ParseBool("newWindow");
                    if (newWindow)
                    {
                        annotation.Action = new ActionGoToR(url, null, true);
                    }
                    else
                    {
                        annotation.Action = new ActionURI(url);
                    }
                    break;
                case "dest":
                    {
                        JToken[] dest = new JToken[0];
                        dataParser.TryParseArray("dest", out dest);
                        int destPageNumber = dataParser.ParseNumber("destPageNumber", -1);
                        string destType = dataParser.ParseString("destType");
                        if (dest.Length > 1)
                        {
                            if (int.TryParse(dest[0].ToString(), out int intVal))
                            {
                                destPageNumber = intVal + 1;
                            }
                            JToken destTypeName = dest[1];
                            destType = destTypeName.Value<string>("name");
                        }
                        if (destPageNumber < 1)
                        {
#if DEBUG
                            throw new Exception("LinkAnnotation: dest array is incorrect.");
#else
                            return annotation;
#endif

                        }
                        var pageIndex = destPageNumber - 1;
                        switch (destType)
                        {
                            case "XYZ":
                                {
                                    float? left = dataParser.ParseLinkDestFloatValue("destX", dest, 2);
                                    float? top = dataParser.ParseLinkDestFloatValue("destY", dest, 3);
                                    float? zoom = dataParser.ParseLinkDestFloatValue("destScale", dest, 4);
                                    var xyz = new DestinationXYZ(pageIndex, left, top, zoom);
                                    annotation.Dest = xyz;
                                    break;
                                }
                            case "Fit":
                                var destFit = new DestinationFit(pageIndex);
                                annotation.Dest = destFit;
                                break;
                            case "FitB":
                                var fitB = new DestinationFitB(pageIndex);
                                annotation.Dest = fitB;
                                break;
                            case "FitH":
                                {
                                    float? top = dataParser.ParseLinkDestFloatValue("destY", dest, 2);
                                    var fitH = new DestinationFitH(pageIndex, top);
                                    annotation.Dest = fitH;
                                    break;
                                }
                            case "FitBH":
                                {
                                    float? top = dataParser.ParseLinkDestFloatValue("destY", dest, 2);
                                    var fitBH = new DestinationFitBH(pageIndex, top);
                                    annotation.Dest = fitBH;
                                    break;
                                }
                            case "FitV":
                                {
                                    float? left = dataParser.ParseLinkDestFloatValue("destX", dest, 2);
                                    var fitV = new DestinationFitV(pageIndex, left);
                                    annotation.Dest = fitV;
                                    break;
                                }
                            case "FitBV":
                                {
                                    float? left = dataParser.ParseLinkDestFloatValue("destX", dest, 2);
                                    var fitBV = new DestinationFitBV(pageIndex, left);
                                    annotation.Dest = fitBV;
                                    break;
                                }
                            case "FitR":
                                {
                                    float? destX1 = dataParser.ParseLinkDestFloatValue("destX", dest, 2, 0);
                                    float? destY1 = dataParser.ParseLinkDestFloatValue("destY", dest, 3, 0);
                                    float? destX2 = dataParser.ParseLinkDestFloatValue(null, dest, 4, null);
                                    if (!destX2.HasValue)
                                    {
                                        float? destW = dataParser.ParseLinkDestFloatValue("destW", null, null, 0);
                                        destX2 = destX1.Value + destW.Value;
                                    }
                                    float? destY2 = dataParser.ParseLinkDestFloatValue(null, dest, 5, null);
                                    if (!destY2.HasValue)
                                    {
                                        float? destH = dataParser.ParseLinkDestFloatValue("destH", null, null, 0);
                                        destY2 = destY1.Value + destH.Value;
                                    }
                                    var rectangleWidth = Math.Abs(destX2.Value - destX1.Value);
                                    var rectangleHeight = Math.Abs(destY2.Value - destY1.Value);
                                    RectangleF bounds = new RectangleF(destX1.Value, destY1.Value, rectangleWidth, rectangleHeight);
                                    var fitR = new DestinationFitR(pageIndex, bounds);
                                    annotation.Dest = fitR;
                                    break;
                                }
                            default:
                                break;
                        }
                        break;
                    }
                case "action":
                    annotation.Action = new ActionNamed(action);
                    break;
                case "js":
                    ActionJavaScript jsAction = new ActionJavaScript(js);
                    annotation.Action = jsAction;
                    break;
                default:
#if DEBUG
                    throw new Exception($"Unknown linkType: {linkType}");
#else
                break;
#endif

            }
            return annotation;
        }

        private static AnnotationBase UpdateAnnotation(LineAnnotation annotation, DataParser dataParser)
        {
            _UpdateMarkupAnnotation(annotation, dataParser);
            if (dataParser.borderWidth.HasValue)
                annotation.LineWidth = dataParser.borderWidth.Value;
            if (dataParser.lineDashPattern != null)
                annotation.LineDashPattern = dataParser.lineDashPattern;
            
            if (dataParser.TryParseFloatArray("lineCoordinates", out float[] lineCoordinates))
            {
                annotation.Start = new PointF(lineCoordinates[0], dataParser.ChangeOriginFromBottomToTop(lineCoordinates[1]));
                annotation.End = new PointF(lineCoordinates[2], dataParser.ChangeOriginFromBottomToTop(lineCoordinates[3]));
            }
            if (dataParser.borderStyleType.HasValue)
            {
                if (dataParser.borderStyleType.Value != BorderStyle.Dashed)
                    annotation.LineDashPattern = null;
            }
            if (dataParser.ContainsDataKey("lineStart"))
            {
                if (Enum.TryParse(dataParser.annotationData["lineStart"], out LineEndingStyle lineStartStyle))
                {
                    annotation.LineStartStyle = lineStartStyle;
                }
            }
            if (dataParser.ContainsDataKey("lineEnd"))
            {
                if (Enum.TryParse(dataParser.annotationData["lineEnd"], out LineEndingStyle lineEndStyle))
                {
                    annotation.LineEndStyle = lineEndStyle;
                }
            }
            annotation.LineEndingsFillColor = dataParser.ParseColor("interiorColor", Color.Transparent).Value;
            return annotation;
        }

        private static AnnotationBase UpdateAnnotation(SquareAnnotation annotation, DataParser dataParser)
        {
            _UpdateMarkupAnnotation(annotation, dataParser);
            if (dataParser.borderWidth.HasValue)
                annotation.LineWidth = dataParser.borderWidth.Value;
            if (dataParser.lineDashPattern != null)
                annotation.LineDashPattern = dataParser.lineDashPattern;
            if (dataParser.borderStyleType.HasValue)
            {
                if (dataParser.borderStyleType.Value != BorderStyle.Dashed)
                    annotation.LineDashPattern = null;
            }
            annotation.FillColor = dataParser.ParseColor("interiorColor", Color.Transparent).Value;
            return annotation;
        }

        private static AnnotationBase UpdateAnnotation(RedactAnnotation annotation, DataParser dataParser)
        {
            _UpdateMarkupAnnotation(annotation, dataParser);

            annotation.MarkBorderColor = dataParser.ParseColor("markBorderColor");
            annotation.MarkFillColor = dataParser.ParseColor("markFillColor");
            annotation.OverlayFillColor = dataParser.ParseColor("overlayFillColor", Color.Transparent).Value;
            annotation.OverlayText = dataParser.ParseString("overlayText");
            annotation.OverlayTextRepeat = dataParser.ParseBool("overlayTextRepeat");
            if (Enum.TryParse(dataParser.ParseString("overlayTextJustification"), out VariableTextJustification align)) 
            {
                annotation.Justification = align;
            }
            return annotation;
        }
        
        private static AnnotationBase UpdateAnnotation(TextAnnotation annotation, DataParser dataParser)
        {
            _UpdateMarkupAnnotation(annotation, dataParser);
            if (!string.IsNullOrEmpty(dataParser.name))
            {
                annotation.Name = dataParser.name;
                if (Enum.TryParse(dataParser.name, out TextAnnotationIcon textAnnotationIcon))
                {
                    annotation.Icon = textAnnotationIcon;
                }
            }            
            annotation.Open = dataParser.open;
            annotation.State = dataParser.ParseString("state");
            annotation.StateModel = dataParser.ParseString("stateModel");
            return annotation;
        }

        private static AnnotationBase UpdateAnnotation(FreeTextAnnotation annotation, DataParser dataParser)
        {
            annotation.TextOffsets = /* GcPdfViewerでは常にRectをテキストの矩形として使用 */ null; 
            _UpdateMarkupAnnotation(annotation, dataParser);
            if (dataParser.borderWidth.HasValue)
                annotation.LineWidth = dataParser.borderWidth.Value;
            
            if (dataParser.borderStyleType.HasValue)
            {
                if (dataParser.borderStyleType.Value != BorderStyle.Dashed)
                {
                    annotation.LineDashPattern = null;
                }
                else
                {
                    if (dataParser.lineDashPattern != null)
                        annotation.LineDashPattern = dataParser.lineDashPattern;
                    else
                        annotation.LineDashPattern = new float[] { 2, 2 };
                }
            } 
            else if (dataParser.lineDashPattern != null)
            {
                annotation.LineDashPattern = dataParser.lineDashPattern;
            }
            if (dataParser.TryParseFloatArray("calloutLine", out float[] array))
            {
                List<PointF> calloutPoints = new List<PointF>();
                for (int i = 0; i < array.Length; i += 2)
                {
                    if (array.Length > i + 1)
                    {
                        var p = new PointF(array[i], dataParser.ChangeOriginFromBottomToTop(array[i + 1]));
                        calloutPoints.Add(p);
                    }
                }
                annotation.CalloutLine = calloutPoints.ToArray();
            }
            if (dataParser.ContainsDataKey("calloutLineEnd"))
            {
                if (Enum.TryParse(dataParser.annotationData["calloutLineEnd"], out LineEndingStyle calloutLineEndStyle))
                {
                    annotation.LineEndStyle = calloutLineEndStyle;
                }
            }
            if (Enum.TryParse(dataParser.ParseString("textAlignment"), out VariableTextJustification justification))
            {
                annotation.Justification = justification;
            }
            annotation.DefaultAppearance.Text = dataParser.getDefaultAppearanceString();
            return annotation;
        }

        private static AnnotationBase UpdateAnnotation(InkAnnotation annotation, DataParser dataParser)
        {
            _UpdateMarkupAnnotation(annotation, dataParser);
            if (dataParser.borderWidth.HasValue)
                annotation.LineWidth = dataParser.borderWidth.Value;
            if (dataParser.lineDashPattern != null)
                annotation.LineDashPattern = dataParser.lineDashPattern;
            annotation.Paths.Clear();
            if (dataParser.ContainsDataKey("inkLists"))
            {
                JArray inkLists = dataParser.annotationData["inkLists"];
                foreach (var inkList1 in inkLists)
                {
                    var inkList = inkList1.ToObject<JArray>();
                    List<PointF> path = new List<PointF>();
                    foreach (JObject point in inkList)
                    {                        
                        float x = DataParser.ConvertToFloatValue(point.GetValue("x"));
                        float y = DataParser.ConvertToFloatValue(point.GetValue("y"));
                        path.Add(new PointF(x, dataParser.ChangeOriginFromBottomToTop(y)));
                    }
                    annotation.Paths.Add(path.ToArray());
                }
                
            }
            if (dataParser.borderStyleType.HasValue)
            {
                if (dataParser.borderStyleType.Value != BorderStyle.Dashed)
                    annotation.LineDashPattern = null;
            }
            return annotation;
        }

        private static void _UpdatePolygonAnnotationBase(PolygonAnnotationBase annotation, DataParser dataParser)
        {
            if (dataParser.borderWidth.HasValue)
                annotation.LineWidth = dataParser.borderWidth.Value;
            if (dataParser.lineDashPattern != null)
                annotation.LineDashPattern = dataParser.lineDashPattern;
            annotation.FillColor = dataParser.ParseColor("interiorColor", Color.Transparent).Value;
            if (dataParser.ContainsDataKey("vertices"))
            {
                JArray vertices = dataParser.annotationData["vertices"];
                List<PointF> path = new List<PointF>();
                foreach (JObject point in vertices)
                {
                    float x = DataParser.ConvertToFloatValue(point.GetValue("x"));
                    float y = DataParser.ConvertToFloatValue(point.GetValue("y"));
                    path.Add(new PointF(x, dataParser.ChangeOriginFromBottomToTop(y)));
                }
                annotation.Points = path;
            }
            if (dataParser.borderStyleType.HasValue)
            {
                if (dataParser.borderStyleType.Value != BorderStyle.Dashed)
                    annotation.LineDashPattern = null;
            }
        }

        private static AnnotationBase UpdateAnnotation(PolyLineAnnotation annotation, DataParser dataParser)
        {
            _UpdateMarkupAnnotation(annotation, dataParser);
            _UpdatePolygonAnnotationBase(annotation, dataParser);
            if (dataParser.ContainsDataKey("lineStart"))
            {
                if (Enum.TryParse(dataParser.annotationData["lineStart"], out LineEndingStyle lineStartStyle))
                {
                    annotation.LineStartStyle = lineStartStyle;
                }
            }
            if (dataParser.ContainsDataKey("lineEnd"))
            {
                if (Enum.TryParse(dataParser.annotationData["lineEnd"], out LineEndingStyle lineEndStyle))
                {
                    annotation.LineEndStyle = lineEndStyle;
                }
            }
            annotation.LineEndingsFillColor = dataParser.ParseColor("interiorColor", Color.Transparent).Value;
            return annotation;
        }

        private static AnnotationBase UpdateAnnotation(PolygonAnnotation annotation, DataParser dataParser)
        {
            _UpdateMarkupAnnotation(annotation, dataParser);
            _UpdatePolygonAnnotationBase(annotation, dataParser);
            // BorderEffect
            return annotation;
        }


#endregion
    }

}

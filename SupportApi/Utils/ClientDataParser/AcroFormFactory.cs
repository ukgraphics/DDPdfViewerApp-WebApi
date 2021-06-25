using SupportApi.Models;
using GrapeCity.Documents.Pdf;
using GrapeCity.Documents.Pdf.AcroForms;
using GrapeCity.Documents.Pdf.Actions;
using GrapeCity.Documents.Pdf.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using GrapeCity.Documents.Pdf.Graphics;
using GrapeCity.Documents.Drawing;
using Image = GrapeCity.Documents.Drawing.Image;

namespace SupportApi.Utils
{
    public class AcroFormFactory 
    {
        #region ** public methods

        public static Field CreateField(DataParser dataParser, Page page)
        {
            AnnotationTypeCode annotationType = dataParser.annotationType;
            switch (annotationType)
            {
                case AnnotationTypeCode.WIDGET:
                    string fieldType = dataParser.ParseString("fieldType");
                    
                    if (fieldType == "Tx")
                    {
                        // テキストフィールドウィジェット
                        bool comb = dataParser.ParseBool("comb");
                        bool multiLine = dataParser.ParseBool("multiLine");
                        bool hasPasswordFlag = dataParser.ParseBool("hasPasswordFlag");
                        if (comb)
                        {
                            return UpdateField(new CombTextField(), dataParser);
                        }
                        else if (multiLine)
                        {
                            return UpdateField(new TextField() { Multiline = true }, dataParser);
                        }
                        else if (hasPasswordFlag)
                        {
                            return UpdateField(new TextField() { Password = true }, dataParser);
                        }
                        else
                        {
                            return UpdateField(new TextField(), dataParser);
                        }
                    }
                    else if (fieldType == "Btn")
                    {
                        // ボタンウィジェット
                        bool checkBox = dataParser.ParseBool("checkBox");
                        bool radioButton = dataParser.ParseBool("radioButton");
                        bool pushButton = dataParser.ParseBool("pushButton");
                        if (checkBox)
                        {
                            return UpdateField(new CheckBoxField(), dataParser);
                        }
                        else if (radioButton)
                        {
                            var radioButtonField = new RadioButtonField();
                            var widget = new WidgetAnnotation(dataParser.page, dataParser.rect);
                            radioButtonField.Widgets.Add(widget);
                            UpdateFieldWidget(radioButtonField, widget, dataParser);
                            return radioButtonField;

                        }
                        else if (pushButton)
                        {
                            return UpdateField(new PushButtonField(), dataParser);
                        }
                        else
                        {
                            return UpdateField(new PushButtonField(), dataParser);
                        }
                    }
                    else if (fieldType == "Ch")
                    {
                        // 選択ウィジェット
                        bool combo = dataParser.ParseBool("combo");
                        bool multiSelect = dataParser.ParseBool("multiSelect");
                        if (combo)
                        {
                            return UpdateField(new ComboBoxField(), dataParser);
                        }
                        else if (multiSelect)
                        {
                            return UpdateField(new ListBoxField(), dataParser);
                        }
                        else
                        {                            
                            throw new Exception(Controllers.GcPdfViewerController.Settings.ErrorMessages.UnknownChoiceField);
                        }
                    }
                    return null;
                case AnnotationTypeCode.SIGNATURE:
                    return UpdateField(new SignatureField(), dataParser);
                default:
                    return null;
            }
        }

        public static void UpdateField(AnnotationTypeCode annotationType, Field field, DataParser dataParser)
        {
            switch (annotationType)
            {
                case AnnotationTypeCode.WIDGET:
                    string fieldType = dataParser.ParseString("fieldType");

                    if (fieldType == "Tx")
                    {
                        // テキストフィールドウィジェット
                        bool comb = dataParser.ParseBool("comb");
                        bool multiLine = dataParser.ParseBool("multiLine");
                        bool hasPasswordFlag = dataParser.ParseBool("hasPasswordFlag");
                        if (comb)
                        {
                            UpdateField(field as CombTextField, dataParser);
                        }
                        else if (multiLine)
                        {
                            UpdateField(field as TextField, dataParser);
                        }
                        else if (hasPasswordFlag)
                        {
                            UpdateField(field as TextField, dataParser);
                        }
                        else
                        {
                            UpdateField(field as TextField, dataParser);
                        }
                    }
                    else if (fieldType == "Btn")
                    {
                        // ボタンウィジェット
                        bool checkBox = dataParser.ParseBool("checkBox");
                        bool radioButton = dataParser.ParseBool("radioButton");

                        bool pushButton = dataParser.ParseBool("pushButton");
                        if (checkBox)
                        {
                            UpdateField(field as CheckBoxField, dataParser);
                        }
                        else if (radioButton)
                        {
                            UpdateField(field as RadioButtonField, dataParser);
                        }
                        else if (pushButton)
                        {
                            UpdateField(field as PushButtonField, dataParser);
                        }
                        else
                        {
                            UpdateField(field as PushButtonField, dataParser);
                        }
                    }
                    else if (fieldType == "Ch")
                    {
                        // 選択ウィジェット
                        bool combo = dataParser.ParseBool("combo");                        
                        if (combo)
                        {
                            UpdateField(field as ComboBoxField, dataParser);
                        }
                        else 
                        {
                            UpdateField(field as ListBoxField, dataParser);
                        }
                    }
                    break;
                case AnnotationTypeCode.SIGNATURE:
                    UpdateField(field as SignatureField, dataParser);
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region ** private implementation

        public static void UpdateFieldWidget(Field field, WidgetAnnotation widget, DataParser dataParser)
        {
            if (widget == null)
            {
                widget = new WidgetAnnotation(dataParser.page, dataParser.rect);
                field.Widgets.Add(widget);
            }
            else
            {
                widget.Page = dataParser.page;
                widget.Rect = dataParser.rect;
            }
            
            dataParser.Widget = widget;

            if (dataParser.HasPrintableFlag)
            {
                DataParser.SetFlag(widget, AnnotationFlags.Print);
            }
            else
            {
                DataParser.RemoveFlag(widget, AnnotationFlags.Print);
            }

            if (!string.IsNullOrEmpty(dataParser.name))
                widget.Name = dataParser.name;
            if (!string.IsNullOrEmpty(dataParser.FieldName))
            {
                field.Name = dataParser.FieldName;
            }
            if (dataParser.ContainsDataKey("backgroundColor"))
            {
                widget.BackColor = dataParser.ParseColor("backgroundColor", Color.Transparent).Value;
            }            
            if (dataParser.ContainsDataKey("borderColor"))
            {
                widget.Border.Color = dataParser.ParseColor("borderColor", Color.Transparent).Value;
            }
            Color nonStrokeColor = dataParser.NonStrokeColor;
            if (!nonStrokeColor.IsEmpty)
                widget.DefaultAppearance.ForeColor = nonStrokeColor;
            widget.Contents = dataParser.contents;
            if (dataParser.borderWidth.HasValue)
            {
                widget.Border.Width = dataParser.borderWidth.Value;
            }
            if (dataParser.borderStyleType.HasValue)
            {
                widget.Border.Style = dataParser.borderStyleType.Value;
            }
            if (dataParser.ContainsDataKey("fontSize"))
            {
                int fontSize = dataParser.ParseNumber("fontSize", 12);
                widget.DefaultAppearance.FontSize = fontSize;
            }
            widget.DefaultAppearance.Font = dataParser.StandardFont;
            string fieldName = dataParser.FieldName;
            if (string.IsNullOrEmpty(fieldName))
            {
                fieldName = $"autogenerated_name_{Guid.NewGuid().ToString()}";
            }
            int childFieldNameIndex = fieldName.LastIndexOf(".");
            if(childFieldNameIndex != -1 && childFieldNameIndex < fieldName.Length - 1)
            {
                // Acrobat Reader DC (2020.006.20042) では、入れ子になったフィールド名をドットで分割しているため、
                // GcPdfViewerでも子フィールドに対して同様の処理を実施
                fieldName = fieldName.Substring(childFieldNameIndex + 1);
            }
            field.Name = fieldName;
            string fieldValue = dataParser.ParseString("fieldValue");
            field.Value = fieldValue;
            if (field is RadioButtonField)
            {
                var buttonName = dataParser.ParseString("buttonValue");
                widget.Name = buttonName;
                if (dataParser.ContainsDataKey("radiosInUnison"))
                {
                    ((RadioButtonField)field).RadiosInUnison = dataParser.ParseBool("radiosInUnison");
                }
            }
            else if (field is PushButtonField)
            {
                widget.ButtonAppearance.Caption = fieldValue;
            }
            else if (field is CheckBoxField)
            {
                (field as CheckBoxField).Value = dataParser.ParseBool("fieldValue");
            }

            field.ReadOnly = dataParser.ParseBool("readOnly");
            field.Required = dataParser.ParseBool("required");

            VariableTextJustification? justification = null;
            string textAlignmentStr = dataParser.ParseString("textAlignment", "-1");
            if (textAlignmentStr != "-1" && Enum.TryParse(textAlignmentStr, out VariableTextJustification val))
                justification = val;
            if (justification.HasValue)
            {
                field.Justification = justification;
                widget.Justification = justification;
            }
            else
            {
                field.Justification = null;
                widget.Justification = null;
            }
        }

        private static Field UpdateField(SignatureField field, DataParser dataParser)
        {
            UpdateFieldWidget(field, field.Widget, dataParser);
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
                        field.Widget.AppearanceStreams.Normal.Default = fxo;
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
            return field;
        }

        private static Field UpdateField(CombTextField field, DataParser dataParser)
        {
            UpdateField((TextField)field, dataParser);
            return field;
        }

        private static Field UpdateField(TextField field, DataParser dataParser)
        {
            UpdateFieldWidget(field, field.Widget, dataParser);
            if (dataParser.ContainsDataKey("maxLen"))
                field.MaxLen = dataParser.ParseNumber("maxLen");
            return field;
        }

        private static Field UpdateField(CheckBoxField field, DataParser dataParser)
        {
            UpdateFieldWidget(field, field.Widget, dataParser);
            return field;
        }

        private static Field UpdateField(RadioButtonField field, DataParser dataParser)
        {
            var widget = field.Widgets.Where(w => w.ParsedObjID == dataParser.pdfObjID).FirstOrDefault();
            UpdateFieldWidget(field, widget, dataParser);
            return field;
        }

        private static Field UpdateField(PushButtonField field, DataParser dataParser)
        {
            bool SubmitForm = dataParser.ParseBool("SubmitForm");
            bool ResetForm = dataParser.ParseBool("ResetForm");
            UpdateFieldWidget(field, field.Widget, dataParser);
            if (ResetForm)
            {
                field.Widget.Events.Activate = new ActionResetForm();
            }
            else if (SubmitForm)
            {
                string submitUrl = dataParser.ParseString("submitUrl");
                field.Widget.Events.Activate = new ActionSubmitForm(submitUrl);
            }
            return field;
        }

        private static Field UpdateField(ComboBoxField field, DataParser dataParser)
        {
            UpdateFieldWidget(field, field.Widget, dataParser);
            _ReadItems(field, dataParser);
            return field;
        }

        private static Field UpdateField(ListBoxField field, DataParser dataParser)
        {
            UpdateFieldWidget(field, field.Widget, dataParser);
            field.MultiSelect = dataParser.ParseBool("multiSelect");
            _ReadItems(field, dataParser);           
            return field;
        }

        private static void _ReadItems(ChoiceField choiceField, DataParser dataParser)
        {
            var items = choiceField.Items;
            items.Clear();

            List<string> fieldValues = new List<string>();
            if(dataParser.TryParseArray("fieldValue", out var arr))
            {
                foreach(var value in arr)
                {
                    fieldValues.Add(value.ToString());
                }
            }
            else
            {
                string fieldValueAsString = dataParser.ParseString("fieldValue");
                if (!string.IsNullOrEmpty(fieldValueAsString))
                {
                    fieldValues.Add(fieldValueAsString);
                }
            }

            List<int> selectedIndexes = new List<int>();
            if (dataParser.TryParseArray("options", out JToken[] array))
            {

                int i = 0;
                foreach (JToken token in array)
                {
                    var obj = token as JObject;
                    var displayValue = dataParser.ParseString(obj, "displayValue", "");
                    var exportValue = dataParser.ParseString(obj, "exportValue", "");
                    if (!string.IsNullOrEmpty(exportValue) && fieldValues.Contains(exportValue))
                    {
                        selectedIndexes.Add(i);
                    }
                    else if (!string.IsNullOrEmpty(displayValue) && fieldValues.Contains(displayValue))
                    {
                        selectedIndexes.Add(i);
                    }
                    items.Add(new ChoiceFieldItem(displayValue, exportValue));
                    i++;
                }

            }
            ListBoxField listBoxField = choiceField as ListBoxField;
            if (listBoxField != null && listBoxField.MultiSelect)
            {
                listBoxField.SelectedIndexes = selectedIndexes.ToArray();
            }
            else if (selectedIndexes.Count > 0)
            {
                choiceField.SelectedIndex = selectedIndexes.Last();
            }
        }

        #endregion

    }
}

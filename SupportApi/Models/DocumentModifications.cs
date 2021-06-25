using Newtonsoft.Json;
using ProtoBuf;
using SupportApi.Collaboration;
using SupportApi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SupportApi.Models
{

    /// <summary>
    /// ドキュメントの変更に関する情報
    /// </summary>
    public class DocumentModifications
    {

        /// <summary>
        /// ドキュメントの回転
        /// </summary>
        public int? rotation { get; set; }

        /// <summary>
        /// 未使用
        /// </summary>
        public bool? renderInteractiveForms { get; set; }

        /// <summary>
        /// フォームのデータ（キー：page index, value, form fields data）
        /// </summary>
        public Dictionary<string, Dictionary<string, string[]>> formData { get; set; }

        /// <summary>
        /// 注釈のデータ
        /// </summary>
        public ModificationsState annotationsData { get; set; }

        /// <summary>
        /// ドキュメント構造の変更に関する情報
        /// </summary>
        public List<StructureChange> structureChanges { get; set; }

        /// <summary>
        /// 注釈の順番
        /// </summary>
        public Dictionary<int, string[]> annotationsOrderTable { get; set; }

    }

    /// <summary>
    /// 削除された注釈の情報
    /// </summary>
    [ProtoContract]
    public class RemovedAnnotationInfo
    {
        [ProtoMember(1)]
        public int pageIndex;

        [ProtoMember(2)]
        public string annotationId;

    }

    /// <summary>
    /// ドキュメント構造の変更
    /// </summary>
    public class StructureChange
    {
        /// <summary>
        /// 変更されたページインデックス
        /// </summary>
        [ProtoMember(1)]
        public int pageIndex { get; set; }
        /// <summary>
        /// true：ページ追加, false：ページ削除
        /// </summary>
        [ProtoMember(2)]
        public bool add { get; set; }

        /// <summary>
        /// 変更前のページ数
        /// </summary>
        [ProtoMember(3)]
        public int checkNumPages { get; set; }

    }


    [ProtoContract]
    public class PdfInfo
    {
        /// <summary>
        /// PDFに含まれるページの総数
        /// </summary>
        [ProtoMember(1)]
        public int numPages { get; set; }
        /// <summary>
        /// PDFを識別するための固有のID（必ず存在するわけではない）
        /// </summary>
        [ProtoMember(2)]
        public string fingerprint { get; set; }

    }

    [ProtoContract]
    public class StructureChanges 
    {
        [ProtoMember(1)]
        public int[] resultStructure { get; set; }
        [ProtoMember(2)]
        public StructureChange[] structureChanges { get; set; }
        [ProtoMember(3)]
        public PdfInfo pdfInfo { get; set; }
        [ProtoMember(4)]
        public RemovedAnnotationInfo[] touchedAnnotations { get; set; }

    }



}

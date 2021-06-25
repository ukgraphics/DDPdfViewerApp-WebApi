using SupportApi.Models;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportApi.Collaboration
{

    /// <summary>
    /// 共有ドキュメントの変更
    /// </summary>
    [ProtoContract]
    public class ModificationsState
    {
        public ModificationsState()
        {

        }

        [ProtoMember(1)]
        public List<AnnotationInfo> newAnnotations { get; set; } = new List<AnnotationInfo>();

        [ProtoMember(2)]
        public List<AnnotationInfo> updatedAnnotations { get; set; } = new List<AnnotationInfo>();

        [ProtoMember(3)]
        public List<RemovedAnnotationInfo> removedAnnotations { get; set; } = new List<RemovedAnnotationInfo>();

        [ProtoMember(4)]
        public int undoCount { get; set; } = 0;

        [ProtoMember(5)]
        public int undoIndex { get; set; } = 0;

        [ProtoMember(6)]
        public int version { get; set; } = 0;

        [ProtoMember(7)]
        public StructureChanges structure { get; set; } = null;

        [ProtoMember(8)]
        public ModificationType lastChangeType { get; set; }

    }
    /*

    /// <summary>
    /// 注釈のデータ
    /// </summary>
    public class AnnotationsState
    {
        public IList<AnnotationInfo> newAnnotations { get; set; }

        public IList<AnnotationInfo> updatedAnnotations { get; set; }
        
        public IList<RemovedAnnotationInfo> removedAnnotations { get; set; }

    }

     */
}

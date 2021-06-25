using SupportApi.Utils;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SupportApi.Models
{
    /// <summary>
    /// 注釈の情報
    /// </summary>
    [ProtoContract]
    public class AnnotationInfo
    {

        private Dictionary<string, dynamic> _annotation;

        [ProtoMember(1)]
        public int pageIndex;

        [ProtoMember(2)]
        [Browsable(false)]
        private string _annotationData;

        public Dictionary<string, dynamic> annotation
        {
            get
            {
                if (_annotation == null && !string.IsNullOrEmpty(_annotationData))
                {
                    _annotation = DataParser.DeserializeAnnotationFromJson(_annotationData);
                }
                return _annotation;
            }
            set
            {
                _annotation = null; _annotationData = null;
                if (value != null)
                {
                    _annotationData = JsonConvert.SerializeObject(value);
                }
            }
        }

    }

}

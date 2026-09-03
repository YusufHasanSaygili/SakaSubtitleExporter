using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SakaSubtitleExporter
{
    [DataContract]
    internal sealed class ProbeResult
    {
        [DataMember]
        public List<SubtitleStream> streams = new List<SubtitleStream>();
    }

    [DataContract]
    internal sealed class SubtitleStream
    {
        [DataMember]
        public int index;

        [DataMember]
        public string codec_type;

        [DataMember]
        public string codec_name;

        [DataMember]
        public StreamTags tags;

        [DataMember]
        public StreamDisposition disposition;
    }

    [DataContract]
    internal sealed class StreamTags
    {
        [DataMember]
        public string language;

        [DataMember]
        public string title;
    }

    [DataContract]
    internal sealed class StreamDisposition
    {
        [DataMember]
        public int @default;

        [DataMember]
        public int forced;
    }

    internal sealed class ProcessResult
    {
        public int ExitCode;
        public string StandardOutput;
        public string StandardError;
    }
}

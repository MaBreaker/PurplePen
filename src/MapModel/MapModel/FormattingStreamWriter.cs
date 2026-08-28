using System;
using System.IO;
using System.Text;

namespace PurplePen.MapModel
{
    /// <summary>
    /// Custom StreamWriter with IFormatProvider
    /// </summary>
    public class FormattingStreamWriter : StreamWriter
    {
        private readonly IFormatProvider _internalFormatProvider;

        /// <summary>
        /// Get IFormatProvider
        /// </summary>
        public override IFormatProvider FormatProvider { get => _internalFormatProvider; }

        /// <summary>
        /// Initialize instance for Stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="formatProvider"></param>
        public FormattingStreamWriter(Stream stream, IFormatProvider formatProvider)
            : base(stream)
        {
            _internalFormatProvider = formatProvider;
        }

        /// <summary>
        /// Initialize instance for path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="formatProvider"></param>
        public FormattingStreamWriter(string path, IFormatProvider formatProvider)
            : base(path)
        {
            _internalFormatProvider = formatProvider; ;
        }

        /// <summary>
        /// Initialize instance for path with append and encoding
        /// </summary>
        /// <param name="path"></param>
        /// <param name="append"></param>
        /// <param name="encoding"></param>
        /// <param name="formatProvider"></param>
        public FormattingStreamWriter(string path, bool append, Encoding? encoding, IFormatProvider formatProvider)
            : base(path, append, encoding)
        {
            _internalFormatProvider = formatProvider;
        }
    }
}
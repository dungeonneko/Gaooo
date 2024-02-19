using System;
using System.Collections.Generic;
using System.IO;

namespace Gaooo
{
    internal class GaoooPeekableStream : IDisposable
    {
        public GaoooFilePath Path { get; private set; }
        public int Line { get; private set; }
        public bool EndOfStream { get; private set; }
        private MemoryStream _stream;
        private StreamReader _streamReader;
        private Queue<string> _peekLine;

        public GaoooPeekableStream()
        {
            Path = GaoooFilePath.Empty;
            Line = 0;
            EndOfStream = true;
            _stream = new MemoryStream();
            _streamReader = new StreamReader(_stream);
            _peekLine = new Queue<string>();
        }

        public GaoooPeekableStream(GaoooFilePath filepath)
        {
            Path = filepath;
            Line = 0;
            EndOfStream = false;
            _stream = new MemoryStream(File.ReadAllBytes(Path.AbsPath));
            _streamReader = new StreamReader(_stream);
            _peekLine = new Queue<string>();
        }

        public void Dispose()
        {
            EndOfStream = true;
            _streamReader.Dispose();
            _stream.Dispose();
            _peekLine.Clear();
        }

        public string ReadLine()
        {
            if (_peekLine.Count > 0)
            {
                Line++;
                return _peekLine.Dequeue();
            }

            var ret = _streamReader.ReadLine();
            if (ret == null)
            {
                EndOfStream = true;
                return string.Empty;
            }

            Line++;
            return ret;
        }

        public string PeekLine()
        {
            if (_peekLine.Count == 0)
            {
                var ret = _streamReader.ReadLine();
                if (ret == null)
                {
                    return string.Empty;
                }
                else
                {
                    _peekLine.Enqueue(ret);
                }
            }

            return _peekLine.Peek();
        }
    }
}

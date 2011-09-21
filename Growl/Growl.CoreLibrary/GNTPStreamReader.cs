using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Growl.CoreLibrary
{
    /// <summary>
    /// This class operates exactly like a normal StreamReader with one difference:
    /// the ReadLine method only looks for '\r\n' as line terminators (as opposed 
    /// to the StreamReader implementation that looks for '\r', '\n', and '\r\n').
    /// </summary>
    public class GNTPStreamReader : StreamReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GNTPStreamReader"/> class.
        /// </summary>
        /// <param name="stream">The underlying stream to read.</param>
        public GNTPStreamReader(Stream stream)
            : base(stream)
        {
        }

        /// <summary>
        /// Reads a line of characters from the current stream and returns the data as a string.
        /// </summary>
        /// <returns>
        /// The next line from the input stream, or null if the end of the input stream is reached.
        /// </returns>
        /// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <remarks>
        /// This method differs from the base StreamReader implementation in that it  only looks for '\r\n' as line terminators (as opposed 
        /// to the StreamReader implementation that looks for '\r', '\n', and '\r\n').
        /// </remarks>
        public override string ReadLine()
        {
            if (this.BaseStream == null)
            {
                throw new ObjectDisposedException(null, "The underlying stream is null");
            }
            if (base.EndOfStream)
            {
                return null;
            }

            StringBuilder currentLine = new StringBuilder();
            int i;
            while ((i = base.Read()) > 0)
            {
                char c = (char)i;
                if (c == '\r')
                {
                    bool endOfLine = false;
                    if(!base.EndOfStream)
                    {
                        int p = base.Peek();
                        char n = (char)p;
                        if (n == '\n')
                        {
                            base.Read();    // consume the \n as well
                            endOfLine = true;
                        }
                    }
                    else
                        endOfLine = true;

                    if(endOfLine)
                    {
                        return currentLine.ToString();
                    }
                }
                currentLine.Append((char)c);
            }
            return currentLine.ToString();
        }
    }
}

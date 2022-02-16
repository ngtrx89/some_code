using System.IO;
using System.Text;

namespace Common.Helpers
{
    public class FileWriterHelper
    {
        #region Private fields
        private FileStream _outputFile;

        #endregion  Private fields

        #region Constructors
        public FileWriterHelper(string path)
        {
            Path = path;
            OpenFile();
        }
        #endregion Constructors

        #region Public props
        public string Path { get; }
        #endregion Public props

        #region Public methods
        public void WriteText(string text)
        {
            WriteBytes(Encoding.UTF8.GetBytes(text));
        }
        public void WriteBytes(byte[] bytes)
        {
                if (null == bytes || bytes.Length == 0)
                    return;
                lock (_outputFile)
                {
                    _outputFile?.Write(bytes);
                    _outputFile?.Flush();
                }
        }

        ~FileWriterHelper()
        {
            _outputFile?.Close();
        }
        #endregion Public methods


        #region Private methods
        private void OpenFile()
        {
            _outputFile = File.Open(Path, FileMode.Append);
        }
        #endregion Private methods

    }
}

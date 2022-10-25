using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Web;
using SFBCommon.NLLog;

namespace NLCscpExtension.ResponseFilters
{
    class ResponseFilter : Stream
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(ResponseFilter));
        #endregion

        #region Members
        protected Stream m_oldFilter = null;
        protected MemoryStream m_streamContentBuf = null;
        protected HttpResponse m_httpResponse = null;
        #endregion

        #region Constructor
        public ResponseFilter(HttpResponse response)
        {
            m_oldFilter = response.Filter;
            m_streamContentBuf = new MemoryStream(4096);
            m_httpResponse = response;
        }
        #endregion

        #region Override: Stream fields
        public override bool CanRead
        {
            get { return true; }
        }
        public override bool CanSeek
        {
            get { return true; }
        }
        public override bool CanWrite
        {
            get { return true; }
        }
        public override long Length
        {
            get { return 0; }
        }
        public override long Position
        {
            get { return m_streamContentBuf.Position; }
            set { m_streamContentBuf.Position = value; }
        }
        #endregion

        #region Override: Stream functions
        public override void Close()
        {
            //write content back to the old stream
            m_streamContentBuf.Position = 0;
            m_streamContentBuf.CopyTo(m_oldFilter);
            m_oldFilter.Flush();
            m_oldFilter.Close();
        }
        public override void Flush()
        {
            m_streamContentBuf.Flush();
        }
        public override long Seek(long offset, System.IO.SeekOrigin direction)
        {
            return 0;
        }
        public override void SetLength(long length)
        {
            //do nothing
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            return m_streamContentBuf.Read(buffer, offset, count);
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            m_streamContentBuf.Write(buffer, offset, count);
        }
        #endregion

        #region Public methods
        public string GetResponseAsString()
        {
            long oldPosition = m_streamContentBuf.Position;//save old position

            m_streamContentBuf.Position = 0;
            StreamReader contentReader = new StreamReader(m_streamContentBuf);
            String strContent = contentReader.ReadToEnd();

            m_streamContentBuf.Position = oldPosition;
            return strContent;
        }
        #endregion

        #region Inner tools
        protected void WriteToOldStream(string strContent)
        {
            try
            {
                byte[] byteContent = System.Text.Encoding.UTF8.GetBytes(strContent);
                m_oldFilter.Write(byteContent, 0, byteContent.Length);
                m_oldFilter.Flush();
            }
            catch (Exception ex)
            {
                theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, ex.ToString());
            }
        }
        #endregion
    }
}

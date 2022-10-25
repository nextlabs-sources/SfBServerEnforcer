using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Diagnostics;

using SFBCommon.NLLog;

namespace NLCscpExtension.RequestFilters
{
    class RequestFilter : Stream
    {
        #region Logger
        static protected CLog theLog = CLog.GetLogger(typeof(RequestFilter));
        #endregion

        #region Members
        protected Stream m_oldFilter = null;
        protected MemoryStream m_streamContentBuf;
        protected HttpRequest m_httpRequest;
        protected bool m_bRequestFilteDone;
        #endregion

        #region Constructors
        public RequestFilter(HttpRequest request)
        {
            m_oldFilter = request.Filter;
            m_httpRequest = request;
            m_streamContentBuf = new MemoryStream(1024);
            m_bRequestFilteDone = false;
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
            get { return false; }
        }
        public override long Length
        {
            get { return m_streamContentBuf.Length; }
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
            return;
        }
        public override void Flush()
        {
            m_oldFilter.Flush();
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
            if(!m_bRequestFilteDone)
            {
                DoRequestFilter();
                m_bRequestFilteDone = true;
            }

            return m_streamContentBuf.Read(buffer, offset, count);
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
        }
        #endregion

        #region Virtual functions: RequestFilter
        protected virtual void DoRequestFilter()
        {
            long lOldPosition = m_oldFilter.Position;

            m_streamContentBuf.Position = 0;
            m_oldFilter.CopyTo(m_streamContentBuf); //default implement just copy the source data

            m_streamContentBuf.Position = 0;
            m_oldFilter.Position = lOldPosition;
        }
        #endregion

        #region Inner tools
        protected string GetOldRequestContent()
        {
            string strContent = "";
            long nOldFilterPosition = m_oldFilter.Position;
            try
            {
                m_oldFilter.Position = 0;
                StreamReader reader = new StreamReader(m_oldFilter);
                strContent = reader.ReadToEnd();
            }
            finally
            {
                m_oldFilter.Position = nOldFilterPosition;
            }
            return strContent;
        }
        #endregion 
    }
}

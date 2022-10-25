using ChatRoomClassifyAddIn.Models;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Room;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Browser;
using System.IO;
using System.Text;
using ChatRoomClassifyAddIn.Helpers;
using System.Threading;
using System.Security;
using System.Windows.Threading;

namespace ChatRoomClassifyAddIn
{
    public partial class MainPage: UserControl
    {
        #region classification xml node&attributes definitions
        private const string kstrClassificationNodeName = "Classification";
        private const string kstrTagNodeName = "Tag";
        private const string kstrTypeAttrName = "Type";
        private const string kstrNameAttrName = "name";
        private const string kstrValueAttrName = "values";
        #endregion

        private const int interval = 5;

        private ObservableCollection<Tag> tagCollection;
        private List<Tag> tagList;
        private string strChatRoomId = "";
        private string strCurUrl = "";
        private static SynchronizationContext context = SynchronizationContext.Current;
        private DispatcherTimer timer = null;

        #region .Ctor
        public MainPage()
        {
            InitializeComponent();
            InitData();
            StartTimer();
        }
        #endregion

        #region Inner Tools

        private void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 5, 0);
            timer.Tick += new EventHandler(TickHandler);
            timer.Start();
        }

        private void InitData()
        {
            timer = new DispatcherTimer();
            tagCollection = new ObservableCollection<Tag>();
            BindDataSource();
            strChatRoomId = GetCurrentChatRoomId();
            strCurUrl = GetRequestHostUrl();

            ThreadHelper.WorkInBackground(CreateHttpRequest);
        }

        private void CreateHttpRequest()
        {
            string requestUrl = strCurUrl;
            HttpWebRequest request = null;

            if (!string.IsNullOrEmpty(strChatRoomId) && !string.IsNullOrEmpty(requestUrl))
            {
                try
                {
                    request = (HttpWebRequest)WebRequest.Create(requestUrl);
                    request.Method = "Post";
                    request.ContentType = "application/x-www-form-urlencoded";

                    request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamHandler), request);
                }
                catch(UriFormatException e)
                {
                    if (request != null)
                    {
                        request.Abort();
                        request = null;
                    }
                    context.Post(state => { LogHelper.Log("CreateHttpRequest method failed, uri format exception", e); }, null);
                }
                catch(Exception e)
                {
                    if (request != null)
                    {
                        request.Abort();
                        request = null;
                    }
                    context.Post(state => { LogHelper.Log("CreateHttpRequest method failed, unknown exception", e); }, null);
                    throw e;
                }
            }
            else
            {
                //do logging
            }
        }

        private string GetRequestHostUrl()
        {
            string strRequestUrl = "";
            string strServicePath = "../../Services/ChatRoomClassifyAddInService.asmx/GetChatRoomClassifyTagsByUri";// "Services/ChatRoomClassifyAddInService.asmx/GetChatRoomClassifyTagsByUri";

            Uri baseUri = HtmlPage.Document.DocumentUri;
            strRequestUrl = string.Format("{0}/{1}", baseUri.ToString(), strServicePath);

            return strRequestUrl;
        }

        private string GetContentStringFromResponse(string response)
        {
            XDocument docElement = null;
            XElement contentNode = null;
            string strTagsXml = "";

            if(!string.IsNullOrEmpty(response))
            {
                try
                {
                    docElement = XDocument.Parse(response);
                    contentNode = docElement.Root;

                    if (contentNode != null)
                    {
                        strTagsXml = contentNode.Value;
                    }
                    else
                    {
                        //do logging
                    }
                }
                catch(Exception e)
                {
                    context.Post(state => { LogHelper.Log("GetContentStringFromResponse method failed, unknown exception", e); }, null);
                }
            }
            else
            {
                //do logging
            }

            return strTagsXml;
        }

        private List<Tag> GetTagListFromXMLString(string xml)
        {
            List<Tag> tagList = new List<Tag>();

            if(!string.IsNullOrEmpty(xml))
            {
                try
                {
                    XElement xmlElement = XElement.Parse(xml);
                    IEnumerable<XElement> tagElements = xmlElement.Elements(kstrTagNodeName);

                    foreach (XElement tagElement in tagElements)
                    {
                        if (tagElement.NodeType == XmlNodeType.Element && tagElement.HasAttributes)
                        {
                            string strTagName = tagElement.Attribute(kstrNameAttrName).Value;
                            string strTagValue = tagElement.Attribute(kstrValueAttrName).Value;

                            if (!string.IsNullOrEmpty(strTagName))
                            {
                                tagList.Add(new Tag(strTagName, strTagValue));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    context.Post(state => { LogHelper.Log("GetTagListFromXMLString method failed, unknown exception", e); }, null);
                }
            }
            else
            {
                //do logging
            }

            return tagList;
        }

        private void FillCollection()
        {
            tagCollection.Clear();
            if(tagList != null)
            {
                foreach (Tag tag in tagList)
                {
                    tagCollection.Add(tag);
                }
            }
            else
            {
                //do logging
            }
        }

        private void BindDataSource()
        {
            if(TagList != null)
            {
                TagList.ItemsSource = tagCollection;
            }
            else
            {
                //do logging
            }
        }

        private string GetCurrentChatRoomId()
        {
            string strId = "";

            Room chatRoom = LyncClient.GetHostingRoom();

            if (chatRoom != null)
            {
                string strUri = chatRoom.Properties[RoomProperty.Uri].ToString();
                strId = GetCurrentChatRoomIdFromUri(strUri);
            }
            else
            {
                //do logging
            }

            return strId;
        }

        private string GetCurrentChatRoomIdFromUri(string uri)
        {
            string strId = "";
            
            if(!string.IsNullOrEmpty(uri))
            {
                string[] szResult = uri.Split('/');
                if(szResult.Length == 4)
                {
                    strId = szResult[3];
                }
            }
            else
            {
                //do logging
            }

            return strId;
        }
        #endregion

        #region Callback

        private void GetRequestStreamHandler(IAsyncResult result)
        {
            if (result == null)
            {
                return;
            }

            HttpWebRequest request = result.AsyncState as HttpWebRequest;

            if (request != null)
            {
                try
                {
                    using (Stream requestStream = request.EndGetRequestStream(result))
                    {
                        if (!string.IsNullOrEmpty(strChatRoomId))
                        {
                            byte[] postData = Encoding.UTF8.GetBytes(string.Format("uri={0}", strChatRoomId));
                            requestStream.Write(postData, 0, postData.Length);
                        }
                    }
                }
                catch (WebException e)
                {
                    context.Post(state => { LogHelper.Log("GetRequestStreamHandler method failed, web exception", e); }, null);
                }
                catch(Exception e)
                {
                    context.Post(state => { LogHelper.Log("GetRequestStreamHandler method failed, unknown exception", e); }, null);
                    throw e;
                }
            }
            else
            {
                //do logging
            }

            request.BeginGetResponse(new AsyncCallback(GetResponseStreamHandler), request);
        }

        private void GetResponseStreamHandler(IAsyncResult result)
        {
            if (result == null)
            {
                return;
            }

            HttpWebRequest request = result.AsyncState as HttpWebRequest;

            if (request != null)
            {
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);

                    if (response != null)
                    {
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            using (StreamReader sr = new StreamReader(responseStream))
                            {
                                string strResponse = sr.ReadToEnd();
                                
                                if (!string.IsNullOrEmpty(strResponse))
                                {
                                    string strTagsXml = GetContentStringFromResponse(strResponse);
                                    tagList = GetTagListFromXMLString(strTagsXml);
                                    context.Post(state => { FillCollection(); }, null);
                                }
                                else
                                {
                                    //do logging
                                }
                            }
                        }
                    }
                    else
                    {
                        //do logging
                    }
                }
                catch (WebException e)
                {
                    context.Post(state => { LogHelper.Log("GetResponseStreamHandler method failed, web exception", e); }, null);
                }
                catch (Exception e)
                {
                    context.Post(state => { LogHelper.Log("GetResponseStreamHandler method failed, unknown exception", e); }, null);
                    throw e;
                }
            }
            else
            {
                //do logging
            }
        }

        private void TickHandler(object sender, EventArgs e)
        {
            //context.Post(state => { MessageBox.Show("timer start"); }, null);
            ThreadHelper.WorkInBackground(CreateHttpRequest);
        }
        #endregion
    }
}

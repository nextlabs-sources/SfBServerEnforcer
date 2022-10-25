using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using SFBCommon.NLLog;

namespace NLAssistantWebService
{
    public static class XmlVariable
    {
        public const string SEARCH = "Search";
        public const string ClASSIFY = "Classify";
        public const string SAVE = "Save";
        public const string SHOW = "Show";

        public const string OPERATION_SUCCEED= "0";
        public const string OPERATION_FAILED = "1";
        public const string OPERATION_UNAUTHORIZED = "2";
        public const string OPERATION_TOKENINVALID = "3";
        public const string OPERATION_PROCESSING = "4";
        public const string OPERATION_NoManualClassify = "5";
        public const string OPERATION_MeetingNotExist = "6";

        public const string operationFieldName = "OperationType";
        public const string sipUriFieldName = "SipUri";
        public const string idFieldName = "Id";

        public const string intervalFilterFieldName = "Interval";
        public const string showMeetingsFieldName = "ShowMeetings";
        public const string allValueName = "all";
        public const string doneValueName = "done";
        public const string undoneValueName = "undone";

        public const string uriFieldName = "Uri";
        public const string entryInfoFieldName = "EntryInfo";
        public const string creatorFieldName = "Creator";
        public const string createTimeFieldName = "CreateTime";

        public const string nameFieldName = "TagName";
        public const string valueFieldName = "TagValue";


        public const string meetingCommandNodeName = "MeetingCommand";
        public const string meetingInfoNodeName = "MeetingInfo";
        public const string meetingNodeName = "Meetings";
        public const string tagsNodeName = "Tags";
        public const string tagNodeName = "Tag";
        public const string resultCodeNodeName = "ResultCode";
        public const string filterNodeName = "Filters";
        public const string classificationNodeName = "Classification";
        public const string queryIdentifyNodeName = "QueryIdentify";
      

        public static CLog HandlerLog = CLog.GetLogger(typeof(XmlVariable));

        public static XmlDocument CreateDocument()
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-16", null);

            doc.AppendChild(dec);

            return doc;
        }

        public static XmlElement CreateElement(XmlDocument xmlDoc , string elementName , IDictionary<string,string> attributes , string innerText)
        {
            XmlElement element = null ;

            if(xmlDoc != null)
            {
                element = xmlDoc.CreateElement(elementName);

                if(attributes != null)
                {
                    foreach (KeyValuePair<string, string> attr in attributes)
                    {
                        if(attr.Key.ToLower() == XmlVariable.sipUriFieldName.ToLower() || attr.Key.ToLower() == XmlVariable.uriFieldName.ToLower() || attr.Key.ToLower() == XmlVariable.entryInfoFieldName.ToLower())
                        {
                            element.SetAttribute(attr.Key, HttpUtility.HtmlEncode(attr.Value));
                        }
                        else
                        {
                            element.SetAttribute(attr.Key, attr.Value);
                        }
                    }
                }

                if(!string.IsNullOrEmpty(innerText))
                {
                    element.InnerText = HttpUtility.HtmlEncode(innerText);
                }
            }
            else
            {
                HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "XmlVariable.CreateElement() failed , xmlDoc is null");
            }

            return element;
        }

        public static string ConvertXmlDoc2String(XmlDocument doc)
        {
            string xmlStr = "";

            if(doc != null)
            {
               using(MemoryStream ms = new MemoryStream())
               {
                   try
                   {
                       using (XmlTextWriter xw = new XmlTextWriter(ms, System.Text.Encoding.UTF8))
                       {
                           doc.Save(xw);
                           using (StreamReader sr = new StreamReader(ms))
                           {
                               ms.Position = 0;
                               xmlStr = sr.ReadToEnd().Trim();
                           }
                       }
                   }
                   catch (ArgumentNullException e)
                   {
                       HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "ConvertXmlDoc2String failed, Error: {0}, \nStackTrace: {1}", e.Message, e.StackTrace);
                   }
                   catch (ArgumentException e)
                   {
                       HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "ConvertXmlDoc2String failed, Error: {0}, \nStackTrace: {1}", e.Message, e.StackTrace);
                   }
               }
            }
            else
            {
                HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelError, "XmlVariable.CreateElement() failed , xmlDoc is null");
            }

            return xmlStr;
        }

        public static bool IsExpired(string targetTime , DateTime baseTime)
        {
            DateTime _targetTime;
            bool isExpired = false;

            try
            {
                bool isSuccess = DateTime.TryParse(targetTime, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out _targetTime);
                if(isSuccess)
                {
                    isExpired = _targetTime.CompareTo(baseTime) == -1 ? true : false ;
                }
                else
                {
                    HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "XmlVariable.IsExpired() failed , DateTime.TryParse() failed , target_time:{0}",targetTime);
                }
            }
            catch (Exception e)
            {
                HandlerLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug," Time Expired Parse Error : {0} , target_time :{1} , base_time:{2}" , e.Message,targetTime,baseTime.ToString());
            }

            return isExpired;
        }

        /// <summary>
        /// convert time filter string to time span
        /// </summary>
        /// <param name="filterStr">
        /// 8 H/D/W/M  8 hours/days/weeks/months
        /// </param>
        /// <returns></returns>
        public static TimeSpan GetFilterTimeSpan(string filterStr)
        {
            if (string.IsNullOrEmpty(filterStr))
                filterStr = "1M";

            int number = -1 ;
            char unit = 'M' ;
            char[] filterArray = filterStr.ToCharArray();
            TimeSpan timeSpan = new TimeSpan(31 * number, 0, 0, 0, 0);

            if(filterArray.Length == 2)
            {
                number = int.Parse(filterArray[0].ToString()) * (-1);
                unit = filterArray[1];
            }
            switch (unit)
            {
                case 'H':
                    return new TimeSpan(number, 0, 0);
                case 'D':
                    return new TimeSpan(number, 0, 0, 0);
                case 'W':
                    return new TimeSpan(7 * number, 0, 0, 0);
                case 'M':
                    return new TimeSpan(31 * number, 0, 0, 0, 0);
                default:
                    break;
            }

            return timeSpan ;
        }
    }
}
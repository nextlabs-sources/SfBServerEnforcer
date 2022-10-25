using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SFBCommon.Common;
using System.Xml;

namespace NLAssistantWebService.Models
{
    public static class MeetingXmlBuilder
    {

        public static string BuildResponseXml(TransModel model)
        {
            string xmlStr = "";
            XmlDocument xmlDoc = XmlVariable.CreateDocument();
            XmlElement mCmdElement = XmlVariable.CreateElement(xmlDoc, XmlVariable.meetingCommandNodeName, model.CommandAttriutes, null);
            XmlElement resultCodeElement = XmlVariable.CreateElement(xmlDoc, XmlVariable.resultCodeNodeName, null, model.ResultCode);
            XmlElement filterElement = XmlVariable.CreateElement(xmlDoc, XmlVariable.filterNodeName, model.FilterAttributes, null);
            XmlElement meetingsElement = XmlVariable.CreateElement(xmlDoc, XmlVariable.meetingNodeName, null, null);

            try
            {
                foreach (MeetingModel m in model.Meetings)
                {
                    XmlElement meetingInfoElement = XmlVariable.CreateElement(xmlDoc, XmlVariable.meetingInfoNodeName, m.MeetingInfoAttributes, null);
                    XmlElement tagsElement = XmlVariable.CreateElement(xmlDoc, XmlVariable.tagsNodeName, null, null);
                    XmlElement classificationElement = XmlVariable.CreateElement(xmlDoc, XmlVariable.classificationNodeName, null, null);
                    XmlCDataSection classContent = xmlDoc.CreateCDataSection(m.Classification);

                    foreach (KeyValuePair<string, string> tag in m.Tags)
                    {
                        XmlElement tagElement = XmlVariable.CreateElement(xmlDoc, XmlVariable.tagNodeName, null, null);
                        tagElement.SetAttribute(XmlVariable.nameFieldName, tag.Key.ToLower());
                        tagElement.SetAttribute(XmlVariable.valueFieldName, HttpUtility.HtmlEncode(tag.Value.ToLower()));
                        tagsElement.AppendChild(tagElement);
                    }
                    classificationElement.AppendChild(classContent);
                    meetingInfoElement.AppendChild(classificationElement);
                    meetingInfoElement.AppendChild(tagsElement);
                    meetingsElement.AppendChild(meetingInfoElement);
                }
            }
            catch (ArgumentNullException e)
            {
                XmlVariable.HandlerLog.OutputLog(SFBCommon.NLLog.EMSFB_LOGLEVEL.emLogLevelError, "Build Response Xml Error : {0}", e.Message);
            }


            mCmdElement.AppendChild(resultCodeElement);
            mCmdElement.AppendChild(filterElement);
            mCmdElement.AppendChild(meetingsElement);

            xmlDoc.AppendChild(mCmdElement);

            xmlStr = XmlVariable.ConvertXmlDoc2String(xmlDoc);

            return xmlStr;
        }

    }
}
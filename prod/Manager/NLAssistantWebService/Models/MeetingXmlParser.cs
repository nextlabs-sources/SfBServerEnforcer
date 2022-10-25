using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using SFBCommon.Common;

namespace NLAssistantWebService.Models
{
    public static class MeetingXmlParser
    {
        //private static CLog theLog = CLog.GetLogger("SfbMeeting_Log");

        public static TransModel ParseRequestXml(XmlNode cmdNode)
        {
            var model = new TransModel();
            try
            {
                XmlNode filters = cmdNode.SelectSingleNode("//" + XmlVariable.filterNodeName);
                XmlNodeList meetingInfos = cmdNode.SelectNodes("//" + XmlVariable.meetingInfoNodeName);
                XmlNodeList tags = cmdNode.SelectNodes("//" + XmlVariable.tagNodeName);

                model.CommandAttriutes[XmlVariable.operationFieldName] = XMLTools.GetAttributeValue(cmdNode.Attributes, XmlVariable.operationFieldName);
                model.CommandAttriutes[XmlVariable.sipUriFieldName] = HttpUtility.HtmlDecode(XMLTools.GetAttributeValue(cmdNode.Attributes, XmlVariable.sipUriFieldName));
                model.CommandAttriutes[XmlVariable.idFieldName] = XMLTools.GetAttributeValue(cmdNode.Attributes, XmlVariable.idFieldName);

                model.ResultCode = cmdNode.SelectSingleNode("//" + XmlVariable.resultCodeNodeName).Value;

                model.FilterAttributes[XmlVariable.intervalFilterFieldName] = XMLTools.GetAttributeValue(filters.Attributes, XmlVariable.intervalFilterFieldName);
                model.FilterAttributes[XmlVariable.showMeetingsFieldName] = XMLTools.GetAttributeValue(filters.Attributes, XmlVariable.showMeetingsFieldName);

                foreach (XmlNode m in meetingInfos)
                {
                    MeetingModel meeting = new MeetingModel();
                    meeting.MeetingInfoAttributes[XmlVariable.uriFieldName] = HttpUtility.HtmlDecode(XMLTools.GetAttributeValue(m.Attributes, XmlVariable.uriFieldName));
                    meeting.MeetingInfoAttributes[XmlVariable.entryInfoFieldName] = HttpUtility.HtmlDecode(XMLTools.GetAttributeValue(m.Attributes, XmlVariable.entryInfoFieldName));
                    meeting.MeetingInfoAttributes[XmlVariable.creatorFieldName] = XMLTools.GetAttributeValue(m.Attributes, XmlVariable.creatorFieldName);
                    meeting.MeetingInfoAttributes[XmlVariable.createTimeFieldName] = XMLTools.GetAttributeValue(m.Attributes, XmlVariable.createTimeFieldName);

                    meeting.Classification = m.SelectSingleNode("//" + XmlVariable.classificationNodeName).Value;

                    foreach (XmlNode tag in tags)
                    {
                        meeting.Tags[HttpUtility.HtmlDecode(XMLTools.GetAttributeValue(tag.Attributes, XmlVariable.nameFieldName).ToLower())] = HttpUtility.HtmlDecode(XMLTools.GetAttributeValue(tag.Attributes, XmlVariable.valueFieldName).ToLower());
                    }

                    model.Meetings.Add(meeting);
                }

            }
            catch (ArgumentNullException e)
            {
                XmlVariable.HandlerLog.OutputLog(SFBCommon.NLLog.EMSFB_LOGLEVEL.emLogLevelError, "Parse Request Xml Error : {0} ", e.Message);
            }

            return model;
        }

    }
}
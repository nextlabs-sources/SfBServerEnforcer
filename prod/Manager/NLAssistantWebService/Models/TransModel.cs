using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NLAssistantWebService.Models
{
    public class TransModel
    {
        public string ResultCode { get; set; }

        public IDictionary<string, string> CommandAttriutes { get; set; }
        public IDictionary<string, string> FilterAttributes;
        public IList<MeetingModel> Meetings { get; set; }

        public TransModel()
        {
            CommandAttriutes = new Dictionary<string, string>();
            FilterAttributes = new Dictionary<string, string>();
            Meetings = new List<MeetingModel>();
        }
    }

    /// <summary>
    /// Model for meeting list item
    /// </summary>
    public class MeetingModel
    {

        public MeetingModel()
        {
            Tags = new Dictionary<string, string>();
            MeetingInfoAttributes = new Dictionary<string, string>();
        }

        public IDictionary<string, string> MeetingInfoAttributes { get; set; }

        public IDictionary<string, string> Tags { get; set; }

        public string Classification { get; set; }
    }
}
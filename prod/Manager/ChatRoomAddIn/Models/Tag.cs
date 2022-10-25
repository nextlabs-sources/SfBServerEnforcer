using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ChatRoomClassifyAddIn.Models
{
    public class Tag
    {
        private string strTagName = "";
        private string strTagValue = "";

        public string TagName {
            get
            {
                return this.strTagName;
            }
            set 
            {
                if(!string.IsNullOrEmpty(value))
                {
                    this.strTagName = value.ToLower();
                }
            } 
        }
        public string TagValue 
        { 
            get
            {
                return this.strTagValue;
            }
            set
            {
                if(!string.IsNullOrEmpty(value))
                {
                    this.strTagValue = value.ToLower();
                }
            }
        }

        public Tag() { }
        public Tag(string name, string value)
        {
            this.TagName = name;
            this.TagValue = value;
        }
    }
}

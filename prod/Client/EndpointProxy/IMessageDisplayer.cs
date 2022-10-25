using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLLyncEndpointProxy
{
    interface IMessageDisplayer
    {
        void SaveMessage(string strMessage);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JarvisBot.Exchange.AlfaBankInSyncRates
{

    [XmlRoot(ElementName = "filials")]
    public class Filials
    {

        [XmlElement(ElementName = "filial")]
        public Filial Filial { get; set; }
    }
}

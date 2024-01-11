using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JarvisBot.Exchange.AlfaBankInSyncRates
{

    [XmlRoot(ElementName = "filial")]
    public class Filial
    {

        [XmlElement(ElementName = "rates")]
        public Rates Rates { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }
}

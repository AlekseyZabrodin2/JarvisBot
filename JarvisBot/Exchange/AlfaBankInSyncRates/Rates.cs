using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JarvisBot.Exchange.AlfaBankInSyncRates
{

    [XmlRoot(ElementName = "rates")]
    public class Rates
    {

        [XmlElement(ElementName = "exch_rate")]
        public ExchRate ExchRate { get; set; }

        [XmlElement(ElementName = "cross_rate")]
        public CrossRate CrossRate { get; set; }
    }
}

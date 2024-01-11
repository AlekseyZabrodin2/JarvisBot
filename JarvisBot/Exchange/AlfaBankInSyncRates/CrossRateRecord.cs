using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JarvisBot.Exchange.AlfaBankInSyncRates
{
    [XmlRoot(ElementName = "cross_rate_record")]
    public class CrossRateRecord
    {

        [XmlAttribute(AttributeName = "mnem")]
        public string Mnem { get; set; }

        [XmlAttribute(AttributeName = "scale")]
        public string Scale { get; set; }

        [XmlAttribute(AttributeName = "rate")]
        public string Rate { get; set; }
    }
}

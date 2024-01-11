using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JarvisBot.Exchange.AlfaBankInSyncRates
{


    [XmlRoot(ElementName = "cross_rate")]
    public class CrossRate
    {

        [XmlElement(ElementName = "cross_rate_record")]
        public List<CrossRateRecord> CrossRateRecord { get; set; }

        [XmlAttribute(AttributeName = "time")]
        public string Time { get; set; }

        [XmlAttribute(AttributeName = "code")]
        public string Code { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }
}

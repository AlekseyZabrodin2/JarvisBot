using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JarvisBot.Exchange.AlfaBankInSyncRates
{
    [XmlRoot(ElementName = "exch_rate_record")]
    public class ExchRateRecord
    {

        [XmlAttribute(AttributeName = "mnem")]
        public string Mnem { get; set; }

        [XmlAttribute(AttributeName = "scale")]
        public string Scale { get; set; }

        [XmlAttribute(AttributeName = "rate_buy")]
        public string RateBuy { get; set; }

        [XmlAttribute(AttributeName = "rate_sell")]
        public string RateSell { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EfficientWallet.ECB.Http.Contracts
{
    [XmlRoot(ElementName = "Envelope", Namespace = "http://www.gesmes.org/xml/2002-08-01")]
    public class EcbEnvelope
    {
        [XmlElement(ElementName = "subject", Namespace = "http://www.gesmes.org/xml/2002-08-01")]
        public string Subject { get; set; } = string.Empty;

        [XmlElement(ElementName = "Sender", Namespace = "http://www.gesmes.org/xml/2002-08-01")]
        public EcbSender Sender { get; set; } = new();

        // Default (eurofxref) namespace from here down
        [XmlElement(ElementName = "Cube", Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
        public EcbCubeContainer Cube { get; set; } = new();
    }

    public class EcbSender
    {
        [XmlElement(ElementName = "name", Namespace = "http://www.gesmes.org/xml/2002-08-01")]
        public string Name { get; set; } = string.Empty;
    }

    public class EcbCubeContainer
    {
        [XmlElement(ElementName = "Cube", Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
        public List<EcbCube> DateRates { get; set; } = new();
    }

    public class EcbCube
    {
        [XmlAttribute("time")]
        public string Date { get; set; } = string.Empty;

        [XmlElement(ElementName = "Cube", Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
        public List<EcbCubeRate> Rates { get; set; } = new();
    }

    public class EcbCubeRate
    {
        [XmlAttribute("currency")]
        public string Currency { get; set; } = string.Empty;

        [XmlAttribute("rate")]
        public decimal Rate { get; set; }
    }
}

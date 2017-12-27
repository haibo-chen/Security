using insp.Utility.Bean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace insp.Security.Strategy
{
    [XmlRoot(ElementName ="configuration")]
    public class StrategyConfiguration
    {
        #region 基本信息
        [XmlElement]
        public PropertiesElement backtest = new PropertiesElement();

        [XmlArray(ElementName = "strategys")]
        [XmlArrayItem(Type=typeof(TypePropertyElement),ElementName = "strategy")]
        public List<TypePropertyElement> strategys = new List<TypePropertyElement>();

        [XmlArray(ElementName = "buyers")]
        [XmlArrayItem(Type = typeof(TypeDescriptorElement), ElementName = "buyer")]
        public List<TypeDescriptorElement> buyers = new List<TypeDescriptorElement>();

        [XmlArray(ElementName = "sellers")]
        [XmlArrayItem(Type = typeof(TypeDescriptorElement), ElementName = "seller")]
        public List<TypeDescriptorElement> sellers = new List<TypeDescriptorElement>();

        #endregion

        #region XML与Properties之间的相互转换

        #endregion

        
    }
    
}

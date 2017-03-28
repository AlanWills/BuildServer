using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BuildServerUtils
{
    public class HTMLElement : HTMLNode
    { 
        public HTMLElement(XmlDocument document, XmlElement element) :
            base(document, element)
        {
        }

        public HTMLElement AddAttribute(string name, string value)
        {
            XmlAttribute attr = XML.CreateAttribute(name);
            attr.Value = value;
            Element.Attributes.Append(attr);

            return this;
        }

        public HTMLElement AddStyling(params Tuple<string, string>[] styling)
        {
            StringBuilder stylingStr = new StringBuilder();
            for (int i = 0; i < styling.Length; ++i)
            {
                stylingStr.Append(styling[i].Item1);
                stylingStr.Append(":");
                stylingStr.Append(styling[i].Item2);
                stylingStr.Append(";");
            }

            return AddAttribute("style", stylingStr.ToString());
        }
    }
}

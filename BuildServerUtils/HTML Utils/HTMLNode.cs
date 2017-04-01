using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BuildServerUtils
{
    public class HTMLNode
    {
        #region Properties and Fields

        protected XmlDocument XML { get; set; }

        protected XmlElement Element { get; set; }

        #endregion

        public HTMLNode(XmlDocument document)
        {
            XML = document;
            Element = XML.CreateElement("span");
            XML.AppendChild(Element);
        }

        public HTMLNode(XmlDocument document, XmlElement element)
        {
            XML = document;
            Element = element;
        }

        private HTMLElement CreateChild(string elementName, string elementValue)
        {
            XmlElement header = XML.CreateElement(elementName);
            header.InnerText = elementValue;
            Element.AppendChild(header);

            HTMLElement element = new HTMLElement(XML, header);
            return element;
        }

        public HTMLElement CreateH2(string text)
        {
            return CreateChild("h2", text);
        }

        public HTMLElement CreateH3(string text)
        {
            return CreateChild("h3", text);
        }

        public HTMLElement CreateSpan(string text)
        {
            return CreateChild("span", text);
        }

        public HTMLElement CreateDiv()
        {
            return CreateChild("div", "");
        }

        public HTMLElement CreateParagraph(string text)
        {
            return CreateChild("p", text);
        }

        public HTMLElement CreateLineBreak()
        {
            return CreateChild("br", "");
        }

        public HTMLElement CreateButton(string label, string destinationURL)
        {
            return CreateChild("form", "")
                        .AddAttribute("action", destinationURL)
                        .AddAttribute("method", "post")
                        .AddAttribute("target", "_blank")
                        .CreateChild("input", "")
                            .AddAttribute("type", "submit")
                            .AddAttribute("value", label);
        }

        public HTMLElement CreatePreservedParagraph(string text)
        {
            return CreateChild("pre", text);
        }

        public HTMLElement CreateLink(string link, string text)
        {
            return CreateChild("a", text).
                        AddAttribute("href", link);
        }
    }
}

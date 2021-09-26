using System.Xml;

namespace LastEpoch
{
    public class Filter
    {
        public static XmlDocument Execute()
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement element1 = doc.CreateElement(string.Empty, "Mainbody", string.Empty);
            doc.AppendChild(element1);

            XmlElement element2 = doc.CreateElement(string.Empty, "level1", string.Empty);
            XmlElement element3 = doc.CreateElement(string.Empty, "level2", string.Empty);

            XmlText text1 = doc.CreateTextNode("Demo Text");

            element1.AppendChild(element2);
            element2.AppendChild(element3);
            element3.AppendChild(text1);

            XmlElement element4 = doc.CreateElement(string.Empty, "level2", string.Empty);
            XmlText text2 = doc.CreateTextNode("other text");
            element4.AppendChild(text2);
            element2.AppendChild(element4);

            return doc;
        }
    }
}

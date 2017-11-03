using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GolanAvraham.ConfigurationTransform.Transform
{
    public static class XmlExtension
    {
        public static IEnumerable<XElement> ElementsAnyNs<T>(this IEnumerable<T> source, string localName)
            where T : XContainer
        {
            return source.Elements().Where(e => e.Name.LocalName == localName);
        }

        public static IEnumerable<XElement> DescendantsAnyNs<T>(this IEnumerable<T> source, string localName)
            where T : XContainer
        {
            return source.Descendants().Where(e => e.Name.LocalName == localName);
        }

        public static IEnumerable<XElement> ElementsAnyNs(this XElement source, string localName)
        {
            return source.Elements().Where(e => e.Name.LocalName == localName);
        }

        public static IEnumerable<XElement> DescendantsAnyNs(this XElement source, string localName)
        {
            return source.Descendants().Where(e => e.Name.LocalName == localName);
        }
    }
}
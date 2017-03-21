using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Text.RegularExpressions;
namespace applyxslt
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                MyMain(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        static void MyMain(string[] args)
        {
            args = args.Concat(new string[] {null,null }).ToArray();
            if (args.Length < 1)
            {
                Console.WriteLine("applyxslt {xml} {xsl} {outfile}");
                return;
            }
            if (!System.IO.File.Exists(args[0]))
            {
                Console.WriteLine($"xmlfile '{args[0]}' not found");
                return;
            }
            var xml = System.IO.File.ReadAllText(args[0]);
            var xmlrs = new System.IO.StringReader(xml);
            var xmldoc = new XPathDocument(xmlrs);
            if (string.IsNullOrEmpty(args[1]))
            {
                var nav = xmldoc.CreateNavigator();
                var x = nav.SelectSingleNode("//processing-instruction('xml-stylesheet')");
                var re = new Regex(@"href=""([^""]*)""");
                var m = re.Match(x.Value);
                if (m == null || !m.Success)
                {
                    Console.WriteLine($"no xsl file");
                    return;
                }
                args[1] = m.Groups[1].ToString();
            }
            var xsl = System.IO.File.ReadAllText(args[1]);
            if (!System.IO.File.Exists(args[1]))
            {
                Console.WriteLine($"xslfile '{args[1]}' not found");
                return;
            }
            if (string.IsNullOrEmpty(args[2]))
            {
                var xsldoc = new XPathDocument(new System.IO.StringReader(xsl));
                var nav = xsldoc.CreateNavigator();
                var x = nav.SelectSingleNode("//processing-instruction('applyxslt-output')");
                var re = new Regex(@"extension=""([^""]*)""");
                var m = re.Match(x.Value);
                if (m == null || !m.Success)
                {
                    args[2] = args[0] + ".xml";
                }
                else
                {
                    args[2] = System.IO.Path.ChangeExtension(args[0], m.Groups[1].ToString());
                }
            }

            
            var xslreader = new XmlTextReader(new System.IO.StringReader(xsl));
            var myXslTrans = new XslCompiledTransform();
            myXslTrans.Load(xslreader);
            var sb = new StringBuilder();
            using (var sw = new System.IO.StringWriter(sb))
            {
                XmlTextWriter myWriter = new XmlTextWriter(sw);
                myXslTrans.Transform(xmldoc, null, myWriter);
            }
            System.IO.File.WriteAllText(args[2], sb.ToString());
        }
    }
}

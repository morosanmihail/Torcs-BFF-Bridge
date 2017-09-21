using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace TORCS_Bridge.TorcsIntegration
{
    class XMLIntegration
    {
        public static void BackupFile(string Filename)
        {
            File.Copy(Filename, Filename + ".bak", true);
        }

        public static void RevertBackup(string Filename)
        {
            File.Copy(Filename + ".bak", Filename, true);
            File.Delete(Filename + ".bak");
        }

        public static string GetPathFromXPath(string TORCSInstallPath, string XPath)
        {
            var PathElements = XPath.Split('.');

            string FilePath = "";
            foreach (var P in PathElements.Where(p => p[0] == 'F'))
            {
                var PathElem = P.Split('_')[1];
                FilePath += PathElem + "/";
            }

            FilePath += PathElements.Where(p => p[0] == 'f').ToList()[0].Split('_')[1] + ".xml";

            FilePath = Path.Combine(TORCSInstallPath, FilePath);

            return FilePath;
        }

        public static void ChangeValueInTorcsXML(string TORCSInstallPath, string XPath, double NewValue)
        {
            // SamplePath = "F_cars.F_car1-ow1.f_car1-ow1.S_Car.A_mass.T_val"
            string FilePath = GetPathFromXPath(TORCSInstallPath, XPath);

            BackupFile(FilePath);

            var PathElements = XPath.Split('.');

            string NodePath = "/params/section";

            //XDocument doc2 = XDocument.Load(FilePath, LoadOptions.PreserveWhitespace);
            //doc2.Declaration = new XDeclaration("1.0", "utf-8", null);

            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            
            doc.Load(FilePath);

            //TODO: Backup original file. Restore after all games are done
            
            XmlNodeList aNodes = doc.SelectNodes(NodePath);

            foreach(var P in PathElements.Where(p=>p[0]=='S'))
            {
                var SectionName = P.Split('_')[1];
                foreach(XmlNode aNode in aNodes)
                {
                    var nameAttr = aNode.Attributes["name"];
                    if(nameAttr != null && nameAttr.Value.Equals(SectionName))
                    {
                        aNodes = aNode.ChildNodes;
                        break;
                    }
                }
            }

            var TargetAttribute = PathElements.Where(p => p[0] == 'A').ToList()[0].Split('_')[1];
            var TargetAttributeProperty = PathElements.Where(p => p[0] == 'T').ToList()[0].Split('_')[1];

            foreach (XmlNode aNode in aNodes)
            {
                if (aNode.Attributes != null)
                {
                    var nameAttr = aNode.Attributes["name"];
                    if (nameAttr != null && nameAttr.Value.Equals(TargetAttribute))
                    {
                        double currentValue = double.Parse(aNode.Attributes[TargetAttributeProperty].Value);
                        aNode.Attributes[TargetAttributeProperty].Value = (currentValue + NewValue).ToString();
                        break;
                    }
                }
            }

            File.WriteAllText(FilePath, Beautify(doc));
        }

        public static string GetJSONOfResultsFromXMLResults(string ResultsFilePath, string ResultXMLPath)
        {
            //SamplePath = "S_E-Track 6.S_Results.S_Qualifications.S_Rank.S_1.A_best lap time.T_val"
            var PathElements = ResultXMLPath.Split('.');

            string NodePath = "/params/section";

            XmlDocument doc = new XmlDocument();
            doc.Load(ResultsFilePath);

            XmlNodeList aNodes = doc.SelectNodes(NodePath);

            foreach (var P in PathElements.Where(p => p[0] == 'S'))
            {
                var SectionName = P.Split('_')[1];
                foreach (XmlNode aNode in aNodes)
                {
                    var nameAttr = aNode.Attributes["name"];
                    if (nameAttr != null && nameAttr.Value.Equals(SectionName))
                    {
                        aNodes = aNode.ChildNodes;
                        break;
                    }
                }
            }

            var TargetAttribute = PathElements.Where(p => p[0] == 'A').ToList()[0].Split('_')[1];
            var TargetAttributeProperty = PathElements.Where(p => p[0] == 'T').ToList()[0].Split('_')[1];

            foreach (XmlNode aNode in aNodes)
            {
                if (aNode.Attributes != null)
                {
                    var nameAttr = aNode.Attributes["name"];
                    if (nameAttr != null && nameAttr.Value.Equals(TargetAttribute))
                    {
                        return aNode.Attributes[TargetAttributeProperty].Value;
                    }
                }
            }

            return "0";
        }

        public static string Beautify(XmlDocument doc)
        {
            string xmlString = null;
            using (MemoryStream ms = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Encoding = new UTF8Encoding(false),
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\r\n",
                    NewLineHandling = NewLineHandling.Replace
                };
                using (XmlWriter writer = XmlWriter.Create(ms, settings))
                {
                    doc.Save(writer);
                }
                xmlString = Encoding.UTF8.GetString(ms.ToArray());
            }
            return xmlString;
        }
    }
}

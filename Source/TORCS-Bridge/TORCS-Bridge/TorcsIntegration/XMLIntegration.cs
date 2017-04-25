using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TORCS_Bridge.TorcsIntegration
{
    class XMLIntegration
    {
        public static void ChangeValueInTorcsXML(string TORCSInstallPath, string Path, double NewValue)
        {
            // SamplePath = "F_cars.F_car1-ow1.f_car1-ow1.S_Car.A_mass.T_val"
            var PathElements = Path.Split('.');

            string FilePath = "";
            foreach(var P in PathElements.Where(p => p[0] == 'F'))
            {
                var PathElem = P.Split('_')[1];
                FilePath += PathElem + "/";
            }

            FilePath += PathElements.Where(p => p[0] == 'f').ToList()[0].Split('_')[1] + ".xml";

            FilePath = TORCSInstallPath + FilePath;

            string NodePath = "/params/section";

            XmlDocument doc = new XmlDocument();
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
                        aNode.Attributes[TargetAttributeProperty].Value = NewValue.ToString();
                        break;
                    }
                }
            }
            
            doc.Save(FilePath);
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
    }
}

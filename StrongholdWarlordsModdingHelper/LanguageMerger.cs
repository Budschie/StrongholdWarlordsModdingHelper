using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows;

namespace StrongholdWarlordsModdingHelper
{
    class LanguageMerger
    {
        private Dictionary<string, LanguageData> languageData = new Dictionary<string, LanguageData>();

        public bool LoadOriginal(XmlDocument document)
        {
            languageData.Clear();

            return LoadXml(document, false, false);
        }

        public bool Merge(XmlDocument document)
        {
            return LoadXml(document, true, true);
        }

        private bool LoadXml(XmlDocument document, bool isModified, bool skipModified)
        {
            XmlNodeList rows = document.GetElementsByTagName("Row");

            foreach (XmlNode node in rows)
            {
                if (node.Attributes.GetNamedItem("ss:StyleID") == null)
                {
                    try
                    {
                        // node.ChildNodes[0].ChildNodes[0].InnerText;


                        // string english = node.ChildNodes[1].ChildNodes[0].InnerText;
                        // string translated = node.ChildNodes[2].ChildNodes[0].InnerText;
                        // string changed = node.ChildNodes[3].ChildNodes[0].InnerText;

                        string id = null, english = null, translated = null, changed = null;

                        int cntr = 0;

                        for(int i = 0; i < node.ChildNodes.Count; i++)
                        {
                            string styleId = node.ChildNodes[i].Attributes.GetNamedItem("ss:StyleID").Value;
                            if (styleId == "sReadWrite")
                            {
                                if (cntr == 0)
                                {
                                    translated = node.ChildNodes[i].ChildNodes[0].InnerText;
                                }
                                else if (cntr == 1)
                                {
                                    changed = node.ChildNodes[i].ChildNodes[0].InnerText;
                                }

                                cntr++;
                            }
                            else if (styleId == "sTag")
                                id = node.ChildNodes[i].ChildNodes[0].InnerText;
                            else if (styleId == "sReadOnly")
                                english = node.ChildNodes[i].ChildNodes[0].InnerText;
                        }

                        if (id == null)
                            continue;

                        if (languageData.ContainsKey(id) && languageData[id].Modified && skipModified)
                            continue;

                        languageData[id] = new LanguageData(isModified, english, translated, changed);
                    }
                    catch (Exception ex)
                    {
                        if (MessageBox.Show("There was an error parsing the language files. Do you still wish to continue?\n\nPressing Yes means that there might be some missing data in-game.\n\nCause of error: " + ex.Message, "Error", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.No)
                            return false;
                    }
                }
            }

            return true;
        }

        public void ApplyXMLChanges(XmlDocument toChange)
        {
            toChange.GetElementsByTagName("Row");

            XmlNode parentNode = null;
            XmlNodeList rows = toChange.GetElementsByTagName("Row");

            List<XmlNode> removalList = new List<XmlNode>();

            foreach (XmlNode node in rows)
            {
                if (node.Attributes.GetNamedItem("StyleID") == null)
                {
                    if (parentNode == null)
                        parentNode = node.ParentNode;

                    //node.ParentNode.RemoveChild(node);
                    removalList.Add(node);
                }
            }

            foreach (XmlNode node in removalList)
                node.ParentNode.RemoveChild(node);

            removalList.Clear();

            if (parentNode == null || parentNode.ParentNode == null)
                MessageBox.Show("Unable to merge XML-files.\n\nReason: The parent element for the language file is null or its parent element is null. Please report this as an issue at my GitHub repository.");

            string namespaceURI = toChange.GetElementsByTagName("Workbook")[0].NamespaceURI;

            foreach (string id in languageData.Keys)
            {
                LanguageData data = languageData[id];

                XmlElement row = toChange.CreateElement("Row");

                row.AppendChild(CreateLangRowData("sTag", id, toChange, namespaceURI));

                if(data.English != null)
                    row.AppendChild(CreateLangRowData("sReadOnly", data.English, toChange, namespaceURI));

                if(data.Translated != null)
                    row.AppendChild(CreateLangRowData("sReadWrite", data.Translated, toChange, namespaceURI));

                if(data.Changed != null)
                    row.AppendChild(CreateLangRowData("sReadWrite", data.Changed, toChange, namespaceURI));

                parentNode.AppendChild(row);
            }
        }

        private XmlElement CreateLangRowData(string styleId, string dataValue, XmlDocument document, string namespaceURI)
        {
            XmlElement cell = document.CreateElement("Cell");
            cell.Prefix = "ss";
            XmlAttribute cellStyle = document.CreateAttribute("StyleID", namespaceURI);
            cellStyle.Prefix = "ss";
            cellStyle.Value = styleId;
            cell.Attributes.Append(cellStyle);
            XmlElement cellData = document.CreateElement("Data");
            cellData.Prefix = "ss";
            XmlAttribute cellDataType = document.CreateAttribute("Type", namespaceURI);
            cellDataType.Prefix = "ss";
            cellDataType.Value = "String";
            cellData.InnerText = dataValue;
            cellData.Attributes.Append(cellDataType);
            cell.AppendChild(cellData);

            return cell;
        }
    }

    class LanguageData
    {
        private bool modified;
        private string english;
        private string translated;
        private string changed;

        public LanguageData(bool modified, string english, string translated, string changed)
        {
            this.modified = modified;
            this.english = english;
            this.translated = translated;
            this.changed = changed;
        }

        public string Changed { get => changed; set => changed = value; }
        public string Translated { get => translated; set => translated = value; }
        public string English { get => english; set => english = value; }
        public bool Modified { get => modified; set => modified = value; }
    }
}

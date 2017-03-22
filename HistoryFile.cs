using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static BuildServer.Branch;

namespace BuildServer
{
    public class HistoryFile
    {
        #region Properties and Fields

        private string FilePath { get; set; }

        public TestState Status { get; private set; } = TestState.kUntested;

        #endregion

        public HistoryFile(string filePath)
        {
            FilePath = filePath;
        }

        public void Load()
        {
            XmlDocument document = new XmlDocument();
            document.Load(FilePath);

            XmlNodeList statusNodes = document.GetElementsByTagName("TestingState");
            if (statusNodes.Count == 1)
            {
                string testState = statusNodes[0].InnerText;
                if (testState == bool.TrueString)
                {
                    Status = TestState.kPassed;
                }
                else if (testState == bool.FalseString)
                {
                    Status = TestState.kFailed;
                }
            }
        }

        public void Save(bool passed)
        {
            XmlDocument document = new XmlDocument();
            XmlElement root = document.CreateElement("Root");
            document.AppendChild(root);

            // Write status
            {
                XmlElement status = document.CreateElement("TestingState");
                status.InnerText = passed.ToString();
                root.AppendChild(status);
            }

            document.Save(FilePath);
        }
    }
}

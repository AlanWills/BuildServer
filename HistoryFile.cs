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

        public TestState Status { get; private set; } = TestState.Untested;

        public List<string> FailedTests { get; private set; } = new List<string>();

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
                    Status = TestState.Passed;
                }
                else if (testState == bool.FalseString)
                {
                    Status = TestState.Failed;
                }
            }

            foreach (XmlElement element in document.GetElementsByTagName("TestName"))
            {
                FailedTests.Add(element.InnerText);
            }
        }

        public void Save(bool passed, List<string> failedTestNames)
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

            // Write failed tests
            {
                XmlElement failedTests = document.CreateElement("FailedTests");
                root.AppendChild(failedTests);

                foreach (string failedTestName in failedTestNames)
                {
                    XmlElement testNameElement = document.CreateElement("TestName");
                    testNameElement.InnerText = failedTestName;
                    failedTests.AppendChild(testNameElement);
                }
            }

            document.Save(FilePath);
        }
    }
}

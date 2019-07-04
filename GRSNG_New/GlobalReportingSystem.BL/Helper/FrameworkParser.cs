using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Xml.Serialization;
using GlobalReportingSystem.Core.Abstract.BL.Helper;

namespace GlobalReportingSystem.BL.Helper
{
    public class FrameworkParser
    {
        /// <summary>
        /// parses TestFixtures and Tests
        /// </summary>
        /// <param name="frameworkAssemblyFileName"></param>
        /// <returns>string XML with Namespace->TestFixture->Test</returns>
        public NunitTests GetFrameworkStructure(string frameworkAssemblyFileName, string path)
        {
            var appSetup = new AppDomainSetup();
            appSetup.ApplicationBase = path;
            // Set up the Evidence
            var baseEvidence = AppDomain.CurrentDomain.Evidence; Evidence evidence = new Evidence(baseEvidence);
            var app = AppDomain.CreateDomain("TemporaryAppDomain", evidence, appSetup);
            var remoteFrameworkParser = (RemoteFrameworkParser)app.CreateInstanceAndUnwrap(typeof(RemoteFrameworkParser).Assembly.FullName, typeof(RemoteFrameworkParser).FullName);            //
            var tempFrameworkXml = remoteFrameworkParser.GetFrameworkStructure(frameworkAssemblyFileName);
            AppDomain.Unload(app);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return tempFrameworkXml;
        }

    }

    public class RemoteFrameworkParser : MarshalByRefObject
    {
        public NunitTests GetFrameworkStructure(string frameworkAssemblyFileName)
        {
            //Assembly frameworkAssembly = Assembly.ReflectionOnlyLoadFrom(frameworkAssemblyFileName);
            Assembly frameworkAssembly = Assembly.LoadFrom(frameworkAssemblyFileName);
            List<Type> tempTestFixtures = frameworkAssembly.GetTypes().Where(item => isTestFixture(item)).ToList();
            List<Type> CateGo = frameworkAssembly.GetTypes().Where(item => isTestFixture(item)).ToList();
            if (tempTestFixtures.Count() < 1)
                return null;
            List<MethodInfo> tempMI;
            NunitTests nunitTests = new NunitTests();
            //XDocument frameworkStructureXML = new XDocument();
            // XElement testsFramework = new XElement("framework");
            //testsFramework.Add(new XAttribute("name", frameworkAssembly.FullName));
            nunitTests.FullName = frameworkAssembly.FullName;
            nunitTests.TestFixtures = new List<TestFixture>();
            foreach (Type tf in tempTestFixtures)
            {
                TestFixture TF = new TestFixture();
                TF.Name = tf.Name;
                TF.Namespace = tf.Namespace;
                //XElement testFixture = new XElement("testFixture");
                //  testFixture.Add(new XAttribute("name", tf.Name));
                //                testFixture.Add(new XAttribute("namespace", tf.Namespace));
                //*****************
                //XElement testsList = new XElement("tests");
                tempMI = tf.GetMethods().Where(item => isTestCase(item)).ToList();
                TF.Tests = new List<string>();
                tempMI.ForEach(p => TF.Tests.Add(p.Name));
                //foreach (MethodInfo mi in tempMI)
                //{
                //    TF.Tests.Add(mi.Name);
                //    //XElement tempTest = new XElement("test");
                //    //tempTest.Value = ;
                //    //testFixture.Add(tempTest);
                //}
                var cateGories = tf.GetMethods().Where(item => isCategory(item)).ToList();
                TF.Categories = new List<string>();

                foreach (var category in cateGories)
                {
                    try
                    {
                        dynamic attribute = category.GetCustomAttributes(true).ToList();
                        foreach (var a in attribute)
                        {
                            if (a.ToString().Contains("Category"))
                            {
                                TF.Categories.Add(a.Name);
                            }
                        }
                       
                    }
                    catch { }
                }
               // cateGories.ForEach(p => TF.Categories.Add(p.Name));
                nunitTests.TestFixtures.Add(TF);
            }

            return nunitTests;
        }

        private static bool isTestFixture(Type t)
        {
            if (t.GetCustomAttributes(false).Where(item => item.ToString().Contains("TestFixtureAttribute")).Count() > 0)
                return true;
            else return false;
        }

        private static bool isCategory(MethodInfo t)
        {
            if (t.GetCustomAttributes(false).Where(item => item.ToString().Contains("CategoryAttribute")).Count() > 0)
                return true;
            else return false;
        }

        private static bool isTestCase(MethodInfo mi)
        {
            if (mi.GetCustomAttributes(false).Where(item => (item.ToString().Contains("Test")) & !(item.ToString().Contains("TestFixture"))).Count() > 0)
                return true;
            return false;
        }
    }

    [Serializable]
   // [XmlRoot("NunitTests"), XmlType("NunitTests")]
    public class NunitTests
    {
        public string FullName { get; set; }
        public List<TestFixture> TestFixtures { get; set; }
        public NunitTests Deserialize(string inputXML)
        {
            //XmlRootAttribute xRoot = new XmlRootAttribute();
            //xRoot.ElementName = "NunitTests";           
            //xRoot.IsNullable = true;
            using (TextReader reader = new StringReader(inputXML))
            {
                var xs = new XmlSerializer(typeof(NunitTests));
                return (NunitTests)xs.Deserialize(reader);
            }

        }
        public string Serialize()
        {
            var serializer = new XmlSerializer(typeof(NunitTests));
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, this);
                return writer.ToString();
            }
        }
    }
    [Serializable]
    public class TestFixture
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public List<string> Tests { get; set; }
        public List<string> Categories { get; set; }
    }
}

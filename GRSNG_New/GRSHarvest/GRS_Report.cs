using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GRSHarvest
{
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class GRS_Report
    {

        private GRS_ReportClient_info client_infoField;

        private GRS_ReportTestSet testSetField;

        /// <remarks/>
        public GRS_ReportClient_info client_info
        {
            get
            {
                return this.client_infoField;
            }
            set
            {
                this.client_infoField = value;
            }
        }

        /// <remarks/>
        public GRS_ReportTestSet TestSet
        {
            get
            {
                return this.testSetField;
            }
            set
            {
                this.testSetField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GRS_ReportClient_info
    {

        private string projectField;

        private string test_cycleField;

        private string urlField;

        private string environmentField;

        private string userField;

        private string ipField;

        private string start_timeField;

        private string end_timeField;

        private string browserField;

        private string osField;

        private string free_spaceField;

        /// <remarks/>
        public string project
        {
            get
            {
                return this.projectField;
            }
            set
            {
                this.projectField = value;
            }
        }

        /// <remarks/>
        public string test_cycle
        {
            get
            {
                return this.test_cycleField;
            }
            set
            {
                this.test_cycleField = value;
            }
        }

        /// <remarks/>
        public string url
        {
            get
            {
                return this.urlField;
            }
            set
            {
                this.urlField = value;
            }
        }

        /// <remarks/>
        public string environment
        {
            get
            {
                return this.environmentField;
            }
            set
            {
                this.environmentField = value;
            }
        }

        /// <remarks/>
        public string user
        {
            get
            {
                return this.userField;
            }
            set
            {
                this.userField = value;
            }
        }

        /// <remarks/>
        public string ip
        {
            get
            {
                return this.ipField;
            }
            set
            {
                this.ipField = value;
            }
        }

        /// <remarks/>
        public string start_time
        {
            get
            {
                return this.start_timeField;
            }
            set
            {
                this.start_timeField = value;
            }
        }

        /// <remarks/>
        public string end_time
        {
            get
            {
                return this.end_timeField;
            }
            set
            {
                this.end_timeField = value;
            }
        }

        /// <remarks/>
        public string browser
        {
            get
            {
                return this.browserField;
            }
            set
            {
                this.browserField = value;
            }
        }

        /// <remarks/>
        public string os
        {
            get
            {
                return this.osField;
            }
            set
            {
                this.osField = value;
            }
        }

        /// <remarks/>
        public string free_space
        {
            get
            {
                return this.free_spaceField;
            }
            set
            {
                this.free_spaceField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GRS_ReportTestSet
    {

        private GRS_ReportTestSetTestCase[] testCaseField;

        private string nameField;

        private string start_timeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("TestCase")]
        public GRS_ReportTestSetTestCase[] TestCase
        {
            get
            {
                return this.testCaseField;
            }
            set
            {
                this.testCaseField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string start_time
        {
            get
            {
                return this.start_timeField;
            }
            set
            {
                this.start_timeField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GRS_ReportTestSetTestCase
    {

        private GRS_ReportTestSetTestCaseStep[] stepField;

        private string nameField;

        private string start_timeField;

        private string descriptionField;

        private string criticalityField;

        private string stateField;

        private string end_timeField;

        private string qc_idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("step")]
        public GRS_ReportTestSetTestCaseStep[] step
        {
            get
            {
                return this.stepField;
            }
            set
            {
                this.stepField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string start_time
        {
            get
            {
                return this.start_timeField;
            }
            set
            {
                this.start_timeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string criticality
        {
            get
            {
                return this.criticalityField;
            }
            set
            {
                this.criticalityField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string state
        {
            get
            {
                return this.stateField;
            }
            set
            {
                this.stateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string end_time
        {
            get
            {
                return this.end_timeField;
            }
            set
            {
                this.end_timeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string qc_id
        {
            get
            {
                return this.qc_idField;
            }
            set
            {
                this.qc_idField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GRS_ReportTestSetTestCaseStep
    {

        private GRS_ReportTestSetTestCaseStepSubstep[] substepField;

        private string typeField;

        private string descriptionField;

        private string expectedField;

        private string timeField;

        private string inputdataField;

        private string attachmentField;

        private string videoField;

        private string actualField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("substep")]
        public GRS_ReportTestSetTestCaseStepSubstep[] substep
        {
            get
            {
                return this.substepField;
            }
            set
            {
                this.substepField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string expected
        {
            get
            {
                return this.expectedField;
            }
            set
            {
                this.expectedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string time
        {
            get
            {
                return this.timeField;
            }
            set
            {
                this.timeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string inputdata
        {
            get
            {
                return this.inputdataField;
            }
            set
            {
                this.inputdataField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string attachment
        {
            get
            {
                return this.attachmentField;
            }
            set
            {
                this.attachmentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string video
        {
            get
            {
                return this.videoField;
            }
            set
            {
                this.videoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string actual
        {
            get
            {
                return this.actualField;
            }
            set
            {
                this.actualField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GRS_ReportTestSetTestCaseStepSubstep
    {

        private string validField;

        private string timeField;

        private string tech_infoField;

        private string screenshotField;

        private string screenshot_driverField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string valid
        {
            get
            {
                return this.validField;
            }
            set
            {
                this.validField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string time
        {
            get
            {
                return this.timeField;
            }
            set
            {
                this.timeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string tech_info
        {
            get
            {
                return this.tech_infoField;
            }
            set
            {
                this.tech_infoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string screenshot
        {
            get
            {
                return this.screenshotField;
            }
            set
            {
                this.screenshotField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string screenshot_driver
        {
            get
            {
                return this.screenshot_driverField;
            }
            set
            {
                this.screenshot_driverField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }


}
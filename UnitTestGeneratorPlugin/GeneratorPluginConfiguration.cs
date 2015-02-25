using System.Configuration;

namespace UnitTestGeneratorPlugin.Generator.SpecFlowPlugin
{
    public class GeneratorPluginConfiguration : ConfigurationSection
    {
        private static GeneratorPluginConfiguration _config;

        public static GeneratorPluginConfiguration GetConfig()
        {
            if (_config == null)
            {
                _config = (GeneratorPluginConfiguration)ConfigurationManager.GetSection("GeneratorPluginConfiguration");
            }
            return _config;
        }
        public static ConfigurationSection GetConfig(Configuration configuration)
        {
            if (_config == null)
            {
                _config = (GeneratorPluginConfiguration)configuration.GetSection("GeneratorPluginConfiguration");
            }
            return _config;
        }

        [ConfigurationProperty("AdditionalCategoryAttributes")]
        [ConfigurationCollection(typeof(AdditionalCategoryAttributes), AddItemName = "AdditionalCategoryAttribute")]
        public AdditionalCategoryAttributes AdditionalCategoryAttributes
        {
            get
            {
                object o = this["AdditionalCategoryAttributes"];
                return o as AdditionalCategoryAttributes;
            }
        }

        [ConfigurationProperty("AdditionalTestCaseAttributes")]
        [ConfigurationCollection(typeof(AdditionalTestCaseAttributes), AddItemName = "AdditionalTestCaseAttribute")]
        public AdditionalTestCaseAttributes AdditionalTestCaseAttributes
        {
            get
            {
                object o = this["AdditionalTestCaseAttributes"];
                return o as AdditionalTestCaseAttributes;
            }
        }

        [ConfigurationProperty("Steps")]
        [ConfigurationCollection(typeof(Steps), AddItemName = "Step")]
        public Steps Steps
        {
            get
            {
                object o = this["Steps"];
                return o as Steps;
            }
        }

        [ConfigurationProperty("TestCaseAttributeFilter")]
        public TestCaseAttributeFilter TestCaseAttributeFilter
        {
            get { return this["TestCaseAttributeFilter"] as TestCaseAttributeFilter; }
        }

    }

    public class TestCaseAttributeFilter : ConfigurationElement
    {

        [ConfigurationProperty("classname", IsRequired = true)]
        public string Classname
        {
            get
            {
                return this["classname"] as string;
            }
        }
        [ConfigurationProperty("namespace", IsRequired = true)]
        public string Namespace
        {
            get
            {
                return this["namespace"] as string;
            }
        }
    }

    public class AdditionalCategoryAttribute : ConfigurationElement
    {

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get
            {
                return this["type"] as string;
            }
        }
        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get
            {
                return this["value"] as string;
            }
        }
    }

    public class AdditionalCategoryAttributes : ConfigurationElementCollection
    {
        public AdditionalCategoryAttribute this[int index]
        {
            get
            {
                return BaseGet(index) as AdditionalCategoryAttribute;
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new AdditionalCategoryAttribute this[string responseString]
        {
            get { return (AdditionalCategoryAttribute)BaseGet(responseString); }
            set
            {
                if (BaseGet(responseString) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(responseString)));
                }
                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new AdditionalCategoryAttribute();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AdditionalCategoryAttribute)element).Value;
        }
    }

    public class AdditionalTestCaseAttribute : ConfigurationElement
    {

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get
            {
                return this["type"] as string;
            }
        }
        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get
            {
                return this["value"] as string;
            }
        }
    }

    public class AdditionalTestCaseAttributes : ConfigurationElementCollection
    {
        public AdditionalTestCaseAttribute this[int index]
        {
            get
            {
                return BaseGet(index) as AdditionalTestCaseAttribute;
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new AdditionalTestCaseAttribute this[string responseString]
        {
            get { return (AdditionalTestCaseAttribute)BaseGet(responseString); }
            set
            {
                if (BaseGet(responseString) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(responseString)));
                }
                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new AdditionalTestCaseAttribute();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AdditionalTestCaseAttribute)element).Value;
        }
    }

    public class Step : ConfigurationElement
    {

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get
            {
                return this["type"] as string;
            }
        }
        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get
            {
                return this["value"] as string;
            }
        }
        [ConfigurationProperty("position", IsRequired = true)]
        public string Position
        {
            get
            {
                return this["position"] as string;
            }
        }
    }

    public class Steps : ConfigurationElementCollection
    {
        public Step this[int index]
        {
            get
            {
                return BaseGet(index) as Step;
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new Step this[string responseString]
        {
            get { return (Step)BaseGet(responseString); }
            set
            {
                if (BaseGet(responseString) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(responseString)));
                }
                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new Step();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Step)element).Position;
        }
    }

}


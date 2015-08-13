using System;
using System.Configuration;
using System.Reflection;

namespace UnitTestGeneratorPlugin.Generator.SpecFlowPlugin
{
    public class GeneratorPluginConfiguration : ConfigurationSection
    {
        private Assembly _configurationDefiningAssembly;
        
        public TConfig GetConfig<TConfig>(string configDefiningAssemblyPath,
            string configFilePath, string sectionName) where TConfig : ConfigurationSection
        {
            AppDomain.CurrentDomain.AssemblyResolve += ConfigResolveEventHandler;
            _configurationDefiningAssembly = Assembly.LoadFrom(configDefiningAssemblyPath);
            var exeFileMap = new ExeConfigurationFileMap();
            exeFileMap.ExeConfigFilename = configFilePath;
            var customConfig = ConfigurationManager.OpenMappedExeConfiguration(exeFileMap,
                ConfigurationUserLevel.None);
            var returnConfig = customConfig.GetSection(sectionName) as TConfig;
            AppDomain.CurrentDomain.AssemblyResolve -= ConfigResolveEventHandler;
            return returnConfig;
        }

        public Assembly ConfigResolveEventHandler(object sender, ResolveEventArgs args)
        {
            return _configurationDefiningAssembly;
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

        [ConfigurationProperty("FilterAssembly")]
        public FilterAssembly FilterAssembly
        {
            get { return this["FilterAssembly"] as FilterAssembly; }
        }

        [ConfigurationProperty("AdditionalCategoryAttributeFilter")]
        public AdditionalCategoryAttributeFilter AdditionalCategoryAttributeFilter
        {
            get { return this["AdditionalCategoryAttributeFilter"] as AdditionalCategoryAttributeFilter; }
        }

        [ConfigurationProperty("AdditionalTestCaseAttributeFilter")]
        public AdditionalTestCaseAttributeFilter AdditionalTestCaseAttributeFilter
        {
            get { return this["AdditionalTestCaseAttributeFilter"] as AdditionalTestCaseAttributeFilter; }
        }

        [ConfigurationProperty("StepFilter")]
        public StepFilter StepFilter
        {
            get { return this["StepFilter"] as StepFilter; }
        }

    }

    public class FilterAssembly : ConfigurationElement
    {

        [ConfigurationProperty("filepath", IsRequired = true)]
        public string Filepath
        {
            get
            {
                return this["filepath"] as string;
            }
        }
    }

    public class AdditionalCategoryAttributeFilter : ConfigurationElement
    {

        [ConfigurationProperty("classname", IsRequired = true)]
        public string Classname
        {
            get
            {
                return this["classname"] as string;
            }
        }
        [ConfigurationProperty("method", IsRequired = true)]
        public string Method
        {
            get
            {
                return this["method"] as string;
            }
        }
    }

    public class AdditionalTestCaseAttributeFilter : ConfigurationElement
    {

        [ConfigurationProperty("classname", IsRequired = true)]
        public string Classname
        {
            get
            {
                return this["classname"] as string;
            }
        }
        [ConfigurationProperty("method", IsRequired = true)]
        public string Method
        {
            get
            {
                return this["method"] as string;
            }
        }
    }

    public class StepFilter : ConfigurationElement
    {

        [ConfigurationProperty("classname", IsRequired = true)]
        public string Classname
        {
            get
            {
                return this["classname"] as string;
            }
        }
        [ConfigurationProperty("method", IsRequired = true)]
        public string Method
        {
            get
            {
                return this["method"] as string;
            }
        }
    }

    public class AdditionalCategoryAttribute : ConfigurationElement
    {
        private bool _typeSet;
        private bool _valueSet;
        private string _type;
        private string _value;

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get
            {
                if (_typeSet)
                {
                    return _type;
                }
                return this["type"] as string;
            }
            set
            {
                _type = value;
                _typeSet = true;
            }

        }
        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get
            {
                if (_valueSet)
                {
                    return _value;
                }
                return this["value"] as string;
            }
            set
            {
                _value = value;
                _valueSet = true;
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


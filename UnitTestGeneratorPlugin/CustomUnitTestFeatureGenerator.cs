using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TechTalk.SpecFlow.Generator;
using TechTalk.SpecFlow.Generator.Configuration;
using TechTalk.SpecFlow.Generator.UnitTestConverter;
using TechTalk.SpecFlow.Generator.UnitTestProvider;
using TechTalk.SpecFlow.Parser.SyntaxElements;
using TechTalk.SpecFlow.Utils;

namespace UnitTestGeneratorPlugin.Generator.SpecFlowPlugin
{
    class CustomUnitTestFeatureGenerator : UnitTestFeatureGenerator, IFeatureGenerator
    {
        private readonly List<int> _uniqueIdList;
        private readonly Random _rnd;

        protected CustomUnitTestFeatureGenerator(IUnitTestGeneratorProvider testGeneratorProvider,
                CodeDomHelper codeDomHelper,
                GeneratorConfiguration generatorConfiguration,
                IDecoratorRegistry decoratorRegistry)
            : base(testGeneratorProvider, codeDomHelper, generatorConfiguration, decoratorRegistry)
        {
            _uniqueIdList = new List<int>();
            _rnd = new Random();
        }

        public new CodeNamespace GenerateUnitTestFixture(Feature feature, string testClassName, string targetNamespace)
        {

            var text = new StringBuilder();
            text.AppendLine(" directory: " + Directory.GetCurrentDirectory());

            var fileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = Directory.GetCurrentDirectory() + @"\App.config"
            };

            var appConfig = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            text.AppendLine(" Custom.plugin.generator.configuration value: " + appConfig.AppSettings.Settings["Custom.plugin.generator.configuration"].Value);

            if (appConfig.AppSettings.Settings["Custom.plugin.generator.configuration"] == null)
            {
                foreach (var scenario in feature.Scenarios)
                {
                    scenario.Tags = scenario.Tags ?? new Tags();
                    scenario.Tags.Add(new Tag("uniqueId:" + GenerateuniqueId()));
                }
            }
            else
            {
                try
                {
                    var configurationSection = GeneratorPluginConfiguration.GetConfig(appConfig);
                    var additionalCategoryAttributes = configurationSection.AdditionalCategoryAttributes;
                    var additionalTestCaseAttributes = configurationSection.AdditionalTestCaseAttributes;
                    var steps = configurationSection.Steps;
                    var filterAssembly = configurationSection.FilterAssembly;
                    var categoriesFilter = configurationSection.AdditionalCategoryAttributeFilter;
                    var testcaseattributesFilter = configurationSection.AdditionalTestCaseAttributeFilter;
                    var stepsFilter = configurationSection.StepFilter;

                    foreach (var scenario in feature.Scenarios)
                    {
                        scenario.Tags = scenario.Tags ?? new Tags();

                        text.AppendLine("AdditionalCategoryAttribute are: " + additionalCategoryAttributes.Count);

                        if (filterAssembly != null)
                        {
                            text.AppendLine("filter assembly is not null file path is: " + filterAssembly.Filepath);

                            var assemblyContainingFilter = Assembly.LoadFrom(filterAssembly.Filepath);

                            if (categoriesFilter != null)
                            {
                                text.AppendLine("categories filter is set hava classname : " + categoriesFilter.Classname + "and method" + categoriesFilter.Method);

                                var rawCategories = new List<AdditionalCategoryAttribute>();
                                for (int i = 0; i < additionalCategoryAttributes.Count; i++)
                                {
                                    text.AppendLine("adding categor with type: " + additionalCategoryAttributes[i].Type + "and value" + additionalCategoryAttributes[i].Value);
                                    rawCategories.Add(additionalCategoryAttributes[i]);
                                }
                                var filterType = assemblyContainingFilter.GetType(categoriesFilter.Classname);
                                var filteredCategories = (List<AdditionalCategoryAttribute>)filterType.InvokeMember(categoriesFilter.Method, BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null , filterType, new object[] { rawCategories });
                                if (filteredCategories.Count > 0)
                                {
                                    foreach (AdditionalCategoryAttribute category in filteredCategories)
                                    {
                                        scenario.Tags.Add(category.Type.Contains("unique")
                                            ? new Tag(category.Value + GenerateuniqueId())
                                            : new Tag(category.Value));
                                    }
                                }
                                else
                                {
                                    text.AppendLine("Filtered categories are empty");
                                }
                            }
                            else
                            {
                                if (additionalCategoryAttributes.Count > 0)
                                {
                                    for (int counter = 0; counter < additionalCategoryAttributes.Count; counter++)
                                    {
                                        scenario.Tags.Add(additionalCategoryAttributes[counter].Type.Contains("unique")
                                            ? new Tag(additionalCategoryAttributes[counter].Value + GenerateuniqueId())
                                            : new Tag(additionalCategoryAttributes[counter].Value));
                                    }
                                }
                                else
                                {
                                    text.AppendLine("no categories which need to be added to unit tests found");
                                }
                            }

                        }
                    }
                }
                catch (Exception e)
                {

                    text.Append(e.ToString());
                }
            }

            File.WriteAllText(@"C:\log.txt", text.ToString());
            return base.GenerateUnitTestFixture(feature, testClassName, targetNamespace);
        }

        private int GenerateuniqueId()
        {
            var uniqueId = _rnd.Next(0, 999999);
            if (_uniqueIdList.Contains(uniqueId))
            {
                _uniqueIdList.Sort();
                uniqueId = _uniqueIdList.Last() + 1;
            }
            _uniqueIdList.Add(uniqueId);
            return uniqueId;
        }
    }
}


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
        private Configuration _appConfig;
        private GeneratorPluginConfiguration _customeConfigurationSection;
        private StringBuilder _text = new StringBuilder("");

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
            InitializeConfiguration();
            InitializeCustomConfigurationSection();
            if (!_customeConfigurationSection.ElementInformation.IsPresent)
            {
                foreach (var scenario in feature.Scenarios)
                {
                    scenario.Tags = scenario.Tags ?? new Tags();
                    scenario.Tags.Add(new Tag("uniqueId:" + GenerateuniqueId()));
                }
            }
            else
            {
                foreach (var scenario in feature.Scenarios)
                {
                    if (_customeConfigurationSection.FilterAssembly.ElementInformation.IsPresent)
                    {
                        try
                        {
                            _text.AppendLine("filter assembly is not null file path is: " + _customeConfigurationSection.FilterAssembly.Filepath);
                            var assemblyContainingFilter = Assembly.LoadFrom(_customeConfigurationSection.FilterAssembly.Filepath);
                            var categoriesFilter =_customeConfigurationSection.AdditionalCategoryAttributeFilter;
                            if (categoriesFilter.ElementInformation.IsPresent)
                            {
                                var rawCategories = new List<AdditionalCategoryAttribute>();
                                for (int i = 0; i < _customeConfigurationSection.AdditionalCategoryAttributes.Count; i++)
                                {
                                    _text.AppendLine("adding categor with type: " + _customeConfigurationSection.AdditionalCategoryAttributes[i].Type + "and value" + _customeConfigurationSection.AdditionalCategoryAttributes[i].Value);
                                    rawCategories.Add(_customeConfigurationSection.AdditionalCategoryAttributes[i]);
                                }
                                var filterType = assemblyContainingFilter.GetType(categoriesFilter.Classname);
                                var filteredCategories = (List<AdditionalCategoryAttribute>)filterType.InvokeMember(categoriesFilter.Method, BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, filterType, new object[] { rawCategories });
                                if (filteredCategories.Count > 0)
                                {
                                    foreach (AdditionalCategoryAttribute category in filteredCategories)
                                    {
                                        scenario.Tags = scenario.Tags ?? new Tags();
                                        scenario.Tags.Add(category.Type.Contains("unique")
                                            ? new Tag(category.Value + GenerateuniqueId())
                                            : new Tag(category.Value));
                                    }
                                }
                                else
                                {
                                    _text.AppendLine("Filtered categories are empty");
                                }
                            }
                            else
                            {
                                AddUnFiltratedCategoryAttributes(scenario);
                            }
                        }
                        catch (Exception e)
                        {
                            _text.AppendLine("Error: Appear when trying to use filter classes from assembly with path " + _customeConfigurationSection.FilterAssembly.Filepath +"\n"+e.Message);
                            //throw;
                        }
                    }
                    else
                    {
                        AddUnFiltratedTestCaseAttributes(scenario, feature);
                        AddUnFiltratedCategoryAttributes(scenario);
                        AddUnFiltratedSteps(scenario);
                    }
                }
            }












            //_text.AppendLine(" directory: " + Directory.GetCurrentDirectory());

            //var fileMap = new ExeConfigurationFileMap
            //{
            //    ExeConfigFilename = Directory.GetCurrentDirectory() + @"\App.config"
            //};

            //var appConfig = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            //_text.AppendLine(" Custom.plugin.generator.configuration value: " + appConfig.AppSettings.Settings["Custom.plugin.generator.configuration"]);

            //if (appConfig.AppSettings.Settings["Custom.plugin.generator.configuration"] == null)
            //{
            //    foreach (var scenario in feature.Scenarios)
            //    {
            //        scenario.Tags = scenario.Tags ?? new Tags();
            //        scenario.Tags.Add(new Tag("uniqueId:" + GenerateuniqueId()));
            //    }
            //}
            //else
            //{
            //    try
            //    {
            //        _text.AppendLine("Start loading the configuration -------------");
            //        _text.AppendLine("Start loading the configuration ------------- " + appConfig.AppSettings.Settings["Custom.plugin.generator.configuration"].Value);
            //        var customeConfigurationSection = new GeneratorPluginConfiguration().GetConfig<GeneratorPluginConfiguration>(appConfig.AppSettings.Settings["Custom.plugin.generator.configuration"].Value, @"App.config", "GeneratorPluginConfiguration");
            //        var additionalCategoryAttributes = customeConfigurationSection.AdditionalCategoryAttributes;
            //        _text.AppendLine("additional categories attributes are:  -------------" + additionalCategoryAttributes.Count);
            //        var additionalTestCaseAttributes = customeConfigurationSection.AdditionalTestCaseAttributes;
            //        _text.AppendLine("additional testcase attributes are:  -------------" + additionalTestCaseAttributes.Count);
            //        var steps = customeConfigurationSection.Steps;
            //        var filterAssembly = customeConfigurationSection.FilterAssembly;
            //        var categoriesFilter = customeConfigurationSection.AdditionalCategoryAttributeFilter;
            //        var testcaseattributesFilter = customeConfigurationSection.AdditionalTestCaseAttributeFilter;
            //        var stepsFilter = customeConfigurationSection.StepFilter;
            //        _text.AppendLine("end loading the configuration -------------");

            //        foreach (var scenario in feature.Scenarios)
            //        {
            //            scenario.Tags = scenario.Tags ?? new Tags();

            //            _text.AppendLine("AdditionalCategoryAttribute are: " + additionalCategoryAttributes.Count);

            //            if (filterAssembly.ElementInformation.IsPresent)
            //            {
            //                _text.AppendLine("filter assembly is not null file path is: " + filterAssembly.Filepath);

            //                var assemblyContainingFilter = Assembly.LoadFrom(filterAssembly.Filepath);

            //                if (categoriesFilter != null)
            //                {
            //                    _text.AppendLine("categories filter is set hava classname : " + categoriesFilter.Classname + "and method" + categoriesFilter.Method);

            //                    var rawCategories = new List<AdditionalCategoryAttribute>();
            //                    for (int i = 0; i < additionalCategoryAttributes.Count; i++)
            //                    {
            //                        _text.AppendLine("adding categor with type: " + additionalCategoryAttributes[i].Type + "and value" + additionalCategoryAttributes[i].Value);
            //                        rawCategories.Add(additionalCategoryAttributes[i]);
            //                    }
            //                    var filterType = assemblyContainingFilter.GetType(categoriesFilter.Classname);
            //                    var filteredCategories = (List<AdditionalCategoryAttribute>)filterType.InvokeMember(categoriesFilter.Method, BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, filterType, new object[] { rawCategories });
            //                    if (filteredCategories.Count > 0)
            //                    {
            //                        foreach (AdditionalCategoryAttribute category in filteredCategories)
            //                        {
            //                            scenario.Tags.Add(category.Type.Contains("unique")
            //                                ? new Tag(category.Value + GenerateuniqueId())
            //                                : new Tag(category.Value));
            //                        }
            //                    }
            //                    else
            //                    {
            //                        _text.AppendLine("Filtered categories are empty");
            //                    }
            //                }
            //                else
            //                {
            //                    if (additionalCategoryAttributes.Count > 0)
            //                    {
            //                        for (int counter = 0; counter < additionalCategoryAttributes.Count; counter++)
            //                        {
            //                            scenario.Tags.Add(additionalCategoryAttributes[counter].Type.Contains("unique")
            //                                ? new Tag(additionalCategoryAttributes[counter].Value + GenerateuniqueId())
            //                                : new Tag(additionalCategoryAttributes[counter].Value));
            //                        }
            //                    }
            //                    else
            //                    {
            //                        _text.AppendLine("no categories which need to be added to unit tests found");
            //                    }
            //                }

            //            }
            //            else
            //            {
            //                _text.AppendLine("filter assembly is null");


            //                if (additionalTestCaseAttributes.Count > 0)
            //                {
            //                    var scenarioOutline = scenario as ScenarioOutline ?? new ScenarioOutline
            //                    {
            //                        Description = scenario.Description,
            //                        Keyword = scenario.Keyword,
            //                        Title = scenario.Title,
            //                        Tags = scenario.Tags,
            //                        Steps = scenario.Steps,
            //                        Examples = new Examples(new ExampleSet { Table = new GherkinTable(new GherkinTableRow(new GherkinTableCell[0]), new GherkinTableRow[0]) })
            //                    };

            //                    var tableContents = scenarioOutline.Examples.ExampleSets.First().Table.Body.ToList();

            //                    for (var i = 0; i < additionalTestCaseAttributes.Count; i++)
            //                    {
            //                        _text.AppendLine("tableContents rows are: " + tableContents.Count);
            //                        _text.AppendLine("trying to add '" + additionalTestCaseAttributes[i].Value + "' as test case attribute to scenario: " + scenario.Title);

            //                        if (additionalTestCaseAttributes[i].Type.Contains("unique"))
            //                        {
            //                            tableContents.Add(
            //                                new GherkinTableRow(
            //                                    new GherkinTableCell(additionalTestCaseAttributes[i].Value + GenerateuniqueId())));
            //                        }
            //                        else
            //                        {
            //                            tableContents.Add(
            //                                new GherkinTableRow(
            //                                    new GherkinTableCell(additionalTestCaseAttributes[i].Value)));
            //                        }
            //                        _text.AppendLine("tableContents rows after updates are: " + tableContents.Count);
            //                    }

            //                    scenarioOutline.Examples.ExampleSets.First().Table.Body = tableContents.ToArray();

            //                    alteredScenarios.Add(scenarioOutline);

            //                    feature.Scenarios = alteredScenarios.ToArray();

            //                    foreach (var za in alteredScenarios)
            //                    {
            //                        var zzaa = za as ScenarioOutline;
            //                        var cels = zzaa.Examples.ExampleSets.First().Table.Body.First().Cells;
            //                        foreach (var w in cels)
            //                        {
            //                            Console.WriteLine("---------- " + w.Value);
            //                        }
            //                    }

            //                }

            //                if (additionalCategoryAttributes.Count > 0)
            //                {
            //                    for (var i = 0; i < additionalCategoryAttributes.Count; i++)
            //                    {
            //                        scenario.Tags.Add(additionalCategoryAttributes[i].Type.Contains("unique")
            //                                ? new Tag(additionalCategoryAttributes[i].Value + GenerateuniqueId())
            //                                : new Tag(additionalCategoryAttributes[i].Value));
            //                    }
            //                }
            //                if (steps.Count > 0)
            //                {
            //                    foreach (Step step in steps)
            //                    {
            //                        switch (step.Type.ToLower())
            //                        {
            //                            case "given":
            //                                scenario.Steps.Insert(Convert.ToInt16(step.Position), new Given { Text = step.Value.Replace("{", "<").Replace("}", ">"), Keyword = step.Type });
            //                                break;
            //                            case "when":
            //                                scenario.Steps.Insert(Convert.ToInt16(step.Position), new When { Text = step.Value.Replace("{", "<").Replace("}", ">"), Keyword = step.Type });
            //                                break;
            //                            case "then":
            //                                scenario.Steps.Insert(Convert.ToInt16(step.Position), new Then { Text = step.Value.Replace("{", "<").Replace("}", ">"), Keyword = step.Type });
            //                                break;
            //                            default:
            //                                throw new Exception("Error mentioned type '" + step.Type.ToLower() + "' didn't match any specflow step type (given, when, then,)!");
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception e)
            //    {

            //        _text.Append(e.ToString());
            //    }
            //}
            //try
            //{
            //    File.WriteAllText(@"C:\log.txt", _text.ToString());
            //}
            //catch (Exception)
            //{


            //}

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

        private void InitializeCustomConfigurationSection()
        {
            _customeConfigurationSection = new GeneratorPluginConfiguration().GetConfig<GeneratorPluginConfiguration>(_appConfig.AppSettings.Settings["Custom.plugin.generator.configuration"].Value, @"App.config", "GeneratorPluginConfiguration");
        }
        private void InitializeConfiguration()
        {
            var fileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = Directory.GetCurrentDirectory() + @"\App.config"
            };

            _appConfig = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
        }
        private void AddUnFiltratedCategoryAttributes(Scenario scenario)
        {
            var additionalCategoryAttributes = _customeConfigurationSection.AdditionalCategoryAttributes;

            if (additionalCategoryAttributes.ElementInformation.IsPresent)
            {
                if (additionalCategoryAttributes.Count > 0)
                {
                    for (var i = 0; i < additionalCategoryAttributes.Count; i++)
                    {
                        scenario.Tags = scenario.Tags ?? new Tags();
                        scenario.Tags.Add(additionalCategoryAttributes[i].Type.Contains("unique")
                            ? new Tag(additionalCategoryAttributes[i].Value + GenerateuniqueId())
                            : new Tag(additionalCategoryAttributes[i].Value));
                    }
                }
                else
                {
                    _text.AppendLine("Additional category attributes element is present but it didn't contain any child elements!");
                }
            }
            else
            {
                _text.AppendLine("Additional category attributes element is not present!");
            }
        }

        private void AddUnFiltratedTestCaseAttributes(Scenario scenario, Feature feature)
        {
            var additionalTestCaseAttributes = _customeConfigurationSection.AdditionalTestCaseAttributes;
            var alteredScenarios = new List<ScenarioOutline>();

            if (additionalTestCaseAttributes.ElementInformation.IsPresent)
            {
                if (additionalTestCaseAttributes.Count > 0)
                {
                    var scenarioOutline = scenario as ScenarioOutline ?? new ScenarioOutline
                    {
                        Description = scenario.Description,
                        Keyword = scenario.Keyword,
                        Title = scenario.Title,
                        Tags = scenario.Tags,
                        Steps = scenario.Steps,
                        Examples =
                            new Examples(new ExampleSet
                            {
                                Table =
                                    new GherkinTable(new GherkinTableRow(new GherkinTableCell[0]), new GherkinTableRow[0])
                            })
                    };

                    var tableContents = scenarioOutline.Examples.ExampleSets.First().Table.Body.ToList();

                    for (var i = 0; i < additionalTestCaseAttributes.Count; i++)
                    {
                        _text.AppendLine("tableContents rows are: " + tableContents.Count);
                        _text.AppendLine("trying to add '" + additionalTestCaseAttributes[i].Value +
                                        "' as test case attribute to scenario: " + scenario.Title);

                        if (additionalTestCaseAttributes[i].Type.Contains("unique"))
                        {
                            tableContents.Add(
                                new GherkinTableRow(
                                    new GherkinTableCell(additionalTestCaseAttributes[i].Value + GenerateuniqueId())));
                        }
                        else
                        {
                            tableContents.Add(
                                new GherkinTableRow(
                                    new GherkinTableCell(additionalTestCaseAttributes[i].Value)));
                        }
                        _text.AppendLine("tableContents rows after updates are: " + tableContents.Count);
                    }

                    scenarioOutline.Examples.ExampleSets.First().Table.Body = tableContents.ToArray();

                    alteredScenarios.Add(scenarioOutline);

                    feature.Scenarios = alteredScenarios.ToArray();
                }
                else
                {
                    _text.AppendLine("Additional test case attributes element is present but it didn't contain any child elements!");
                }
            }
            else
            {
                _text.AppendLine("Additional test case attributes element is not present!");
            }
        }
        private void AddUnFiltratedSteps(Scenario scenario)
        {
            var additionalSteps = _customeConfigurationSection.Steps;

            if (additionalSteps.ElementInformation.IsPresent)
            {

                if (additionalSteps.Count > 0)
                {
                    foreach (Step step in additionalSteps)
                    {
                        switch (step.Type.ToLower())
                        {
                            case "given":
                                scenario.Steps.Insert(Convert.ToInt16(step.Position),
                                    new Given
                                    {
                                        Text = step.Value.Replace("{", "<").Replace("}", ">"),
                                        Keyword = step.Type
                                    });
                                break;
                            case "when":
                                scenario.Steps.Insert(Convert.ToInt16(step.Position),
                                    new When
                                    {
                                        Text = step.Value.Replace("{", "<").Replace("}", ">"),
                                        Keyword = step.Type
                                    });
                                break;
                            case "then":
                                scenario.Steps.Insert(Convert.ToInt16(step.Position),
                                    new Then
                                    {
                                        Text = step.Value.Replace("{", "<").Replace("}", ">"),
                                        Keyword = step.Type
                                    });
                                break;
                            default:
                                throw new Exception("Error mentioned type '" + step.Type.ToLower() +
                                                    "' didn't match any specflow step type (given, when, then,)!");
                        }
                    }
                }
                else
                {
                    _text.AppendLine("Additional steps element is present but it didn't contains any child elements!");
                }
            }
            else
            {
                _text.AppendLine("Additional steps element is not present!");
            }
        }
    }
}


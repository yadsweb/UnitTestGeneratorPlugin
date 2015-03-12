using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using TechTalk.SpecFlow.Generator;
using TechTalk.SpecFlow.Generator.Configuration;
using TechTalk.SpecFlow.Generator.UnitTestConverter;
using TechTalk.SpecFlow.Generator.UnitTestProvider;
using TechTalk.SpecFlow.Parser.SyntaxElements;
using TechTalk.SpecFlow.Utils;

namespace UnitTestGeneratorPlugin.Generator.SpecFlowPlugin
{
    internal class CustomUnitTestFeatureGenerator : UnitTestFeatureGenerator, IFeatureGenerator
    {
        private readonly List<int> _uniqueIdList;
        private readonly Random _rnd;
        private Configuration _appConfig;
        private GeneratorPluginConfiguration _customeConfigurationSection;
        private readonly ILog _log = LogManager.GetLogger(typeof(CustomUnitTestFeatureGenerator));

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
            SetUpLogger();
            _log.Info("Starting custom plug in generator.");
            InitializeConfiguration();
            InitializeCustomConfigurationSection();
            if (!_customeConfigurationSection.ElementInformation.IsPresent)
            {
                _log.Info("There is no generator plugin configuration section in app.config.");
                _log.Info("Only unique id category will be added to generate unit tests for feature '" + feature.Title + "'.");
                foreach (var scenario in feature.Scenarios)
                {
                    scenario.Tags = scenario.Tags ?? new Tags();
                    var uniqueTag = "uniqueId:" + GenerateuniqueId();
                    scenario.Tags.Add(new Tag(uniqueTag));
                    _log.Info("Tag '" + uniqueTag + "' added to scenario with name '" + scenario.Title + "'.");
                }
            }
            else
            {
                _log.Info("Generator plugin configuration section in app.config present.");
                _log.Info("Its values will be used during unit tests generation for feature with name '" + feature.Title + "'.");
                foreach (var scenario in feature.Scenarios)
                {
                    if (_customeConfigurationSection.FilterAssembly.ElementInformation.IsPresent)
                    {
                        _log.Info("Filter assembly element is present in the plugin configuration section.");
                        try
                        {
                            _log.Info("Filter assembly file path property is '" + _customeConfigurationSection.FilterAssembly.Filepath + "'.");
                            var assemblyContainingFilter = Assembly.LoadFrom(_customeConfigurationSection.FilterAssembly.Filepath);
                            var categoriesFilter = _customeConfigurationSection.AdditionalCategoryAttributeFilter;
                            var stepsFilter = _customeConfigurationSection.StepFilter;
                            var testCaseAttributeFilter = _customeConfigurationSection.AdditionalTestCaseAttributeFilter;

                            #region Test case attributes filtering

                            if (testCaseAttributeFilter.ElementInformation.IsPresent)
                            {
                                _log.Info("Test case attribute filter element is present with class name '" + categoriesFilter.Classname + "' and method '" + categoriesFilter.Method + "' properties.");

                                var rawTestCaseAttribute = new List<AdditionalTestCaseAttribute>();

                                _log.Info("Creating a temporal list with test case attributes which will be send to the filter method.");

                                foreach (var testCaseAttribute in _customeConfigurationSection.AdditionalTestCaseAttributes)
                                {
                                    _log.Info("Adding test case attribute with type '" + ((AdditionalTestCaseAttribute)testCaseAttribute).Type + "' and value '" + ((AdditionalTestCaseAttribute)testCaseAttribute).Value + " to the temporary list");

                                    rawTestCaseAttribute.Add(((AdditionalTestCaseAttribute)testCaseAttribute));
                                }

                                _log.Info("Loaded filter assembly is '" + assemblyContainingFilter.GetName() + "'.");
                                var filterType = assemblyContainingFilter.GetType(testCaseAttributeFilter.Classname);
                                _log.Info("Test case attribute filter type is '" + filterType.Name + "' creating method info object.");
                                var methodInfo = filterType.GetMethod(testCaseAttributeFilter.Method,
                                    new[] { typeof(List<AdditionalTestCaseAttribute>), typeof(Scenario) });
                                var o = Activator.CreateInstance(filterType);
                                var filteredTestCaseAttributes =
                                    (List<GherkinTableRow>)
                                        methodInfo.Invoke(o, new object[] { rawTestCaseAttribute, scenario });
                                _log.Info("Test case attribute rows returned by the filter method are '" + filteredTestCaseAttributes.Count + "'.");
                                if (filteredTestCaseAttributes.Count > 0)
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
                                                    new GherkinTable(new GherkinTableRow(new GherkinTableCell[0]),
                                                        new GherkinTableRow[0])
                                            })
                                    };

                                    var tableContents = scenarioOutline.Examples.ExampleSets.First().Table.Body.ToList();

                                    _log.Info(tableContents.Count + " test case attributes retrieved from scenario with name '" + scenario.Title + "'");
                                    _log.Info("Clearing the content of retrieve table.");
                                    tableContents.Clear();

                                    foreach (var testCaseAttributeRow in filteredTestCaseAttributes)
                                    {
                                        _log.Info("Trying to add test case attribute row with cells: ");

                                        foreach (var cell in testCaseAttributeRow.Cells)
                                        {
                                            _log.Info(cell.Value);
                                        }

                                        _log.Info("to scenario with name '" + scenario.Title + "'");

                                        tableContents.Add(testCaseAttributeRow);

                                    }
                                    _log.Info(tableContents.Count + " test case attributes retrieved from scenario with name '" + scenario.Title + "' after updating.");

                                    var alteredScenarios = new List<ScenarioOutline>();

                                    scenarioOutline.Examples.ExampleSets.First().Table.Body = tableContents.ToArray();

                                    alteredScenarios.Add(scenarioOutline);

                                    feature.Scenarios = alteredScenarios.ToArray();

                                }
                                else
                                {
                                    _log.Info("Test case attributes returned by the filter type are 0 so no actions will be taken.");
                                }
                            }
                            else
                            {
                                _log.Info("Test case attribute filter attribute is not present.");
                                AddUnFiltratedTestCaseAttributes(scenario, feature);
                            }

                            #endregion

                            #region Categories filtering

                            if (categoriesFilter.ElementInformation.IsPresent)
                            {
                                _log.Info("Categories filter element is present with class name '" + categoriesFilter.Classname + "' and method '" + categoriesFilter.Method + "' properties.");
                                var rawCategories = new List<AdditionalCategoryAttribute>();
                                _log.Info("Creating a temporal list with category attributes which will be send to the filter method.");
                                foreach (var categoryAttribute in _customeConfigurationSection.AdditionalCategoryAttributes)
                                {
                                    _log.Info("Adding category with type '" + ((AdditionalCategoryAttribute)categoryAttribute).Type + "' and value '" + ((AdditionalCategoryAttribute)categoryAttribute).Value + " to the temporary list");
                                    rawCategories.Add(((AdditionalCategoryAttribute)categoryAttribute));
                                }
                                _log.Info("Loaded filter assembly is '" + assemblyContainingFilter.GetName() + "'.");
                                var filterType = assemblyContainingFilter.GetType(categoriesFilter.Classname);
                                _log.Info("Category filter type is '" + filterType.Name + "'.");
                                var methodInfo = filterType.GetMethod(categoriesFilter.Method,
                                    new[] { typeof(List<AdditionalCategoryAttribute>), typeof(Scenario) });
                                var o = Activator.CreateInstance(filterType);
                                var filteredCategories =
                                    (List<AdditionalCategoryAttribute>)
                                        methodInfo.Invoke(o, new object[] { rawCategories, scenario });
                                _log.Info("Categories returned by the filter method are '" + filteredCategories.Count + "'.");
                                if (filteredCategories.Count > 0)
                                {
                                    foreach (var category in filteredCategories)
                                    {
                                        _log.Info("Clearing all category attributes for scenario '" + scenario.Title + "'");
                                        scenario.Tags = new Tags();
                                        _log.Info("Adding category attribute with type '" + category.Type + "' and value '" + category.Value + "'");
                                        scenario.Tags.Add(category.Type.Contains("unique")
                                            ? new Tag(category.Value + GenerateuniqueId())
                                            : new Tag(category.Value));
                                    }
                                }
                                else
                                {
                                    _log.Info("Categories returned by the filter type are 0 so no actions will be taken.");
                                }
                            }
                            else
                            {
                                _log.Info("Category filter attribute is not present.");
                                AddUnFiltratedCategoryAttributes(scenario);
                            }

                            #endregion

                            #region Steps filtering

                            if (stepsFilter.ElementInformation.IsPresent)
                            {
                                _log.Info("Steps filter element is present with class name '" + stepsFilter.Classname + "' and method '" + stepsFilter.Method + "' properties.");
                                var rawSteps = new List<Step>();
                                _log.Info("Creating a temporal list with steps which will be send to the filter method.");
                                foreach (var step in _customeConfigurationSection.Steps)
                                {
                                    _log.Info("Adding step with type '" + ((Step)step).Type + "' and value '" + ((Step)step).Value + " to the temporary list");
                                    rawSteps.Add(((Step)step));
                                }
                                _log.Info("Loaded filter assembly is '" + assemblyContainingFilter.GetName() + "'.");
                                var filterType = assemblyContainingFilter.GetType(stepsFilter.Classname);
                                _log.Info("Step filter type is '" + filterType.Name + "'.");
                                var methodInfo = filterType.GetMethod(stepsFilter.Method,
                                    new[] { typeof(List<Step>), typeof(Scenario) });
                                var o = Activator.CreateInstance(filterType);
                                var filteredSteps =
                                    (List<Step>)
                                        methodInfo.Invoke(o, new object[] { rawSteps, scenario });
                                _log.Info("Steps returned by the filter method are '" + filteredSteps.Count + "'.");
                                if (filteredSteps.Count > 0)
                                {
                                    foreach (var step in filteredSteps)
                                    {
                                        switch (step.Type.ToLower())
                                        {
                                            case "given":
                                                if (Convert.ToInt16(step.Position) > scenario.Steps.Count)
                                                {
                                                    _log.Info("Adding step with final text '" +step.Value.Replace("{", "<").Replace("}", ">") +"' and type 'given' on position '" + scenario.Steps.Count+ 1 +"' to scenario with name '" + scenario.Title + "'.");
                                                    scenario.Steps.Add(
                                                        new Given
                                                        {
                                                            Text = step.Value.Replace("{", "<").Replace("}", ">"),
                                                            Keyword = step.Type
                                                        });
                                                }
                                                else
                                                {
                                                    _log.Info("Adding step with final text '" + step.Value.Replace("{", "<").Replace("}", ">") + "' and type 'given' on position '" + scenario.Steps.Count + 1 + "' to scenario with name '" + scenario.Title + "'.");
                                                    scenario.Steps.Insert(Convert.ToInt16(step.Position),
                                                        new Given
                                                        {
                                                            Text = step.Value.Replace("{", "<").Replace("}", ">"),
                                                            Keyword = step.Type
                                                        });
                                                }
                                                break;
                                            case "when":
                                                if (Convert.ToInt16(step.Position) > scenario.Steps.Count)
                                                {
                                                    _log.Info("Adding step with final text '" + step.Value.Replace("{", "<").Replace("}", ">") + "' and type 'when' on position '" + scenario.Steps.Count + 1 + "' to scenario with name '" + scenario.Title + "'.");
                                                    scenario.Steps.Add(
                                                        new Given
                                                        {
                                                            Text = step.Value.Replace("{", "<").Replace("}", ">"),
                                                            Keyword = step.Type
                                                        });
                                                }
                                                else
                                                {
                                                    _log.Info("Adding step with final text '" + step.Value.Replace("{", "<").Replace("}", ">") + "' and type 'given' on position '" + scenario.Steps.Count + 1 + "' to scenario with name '" + scenario.Title + "'.");
                                                    scenario.Steps.Insert(Convert.ToInt16(step.Position),
                                                        new Given
                                                        {
                                                            Text = step.Value.Replace("{", "<").Replace("}", ">"),
                                                            Keyword = step.Type
                                                        });
                                                }
                                                break;
                                            case "then":
                                                if (Convert.ToInt16(step.Position) > scenario.Steps.Count)
                                                {
                                                    _log.Info("Adding step with final text '" + step.Value.Replace("{", "<").Replace("}", ">") + "' and type 'then' on position '" + scenario.Steps.Count + 1 + "' to scenario with name '" + scenario.Title + "'.");
                                                    scenario.Steps.Add(
                                                        new Given
                                                        {
                                                            Text = step.Value.Replace("{", "<").Replace("}", ">"),
                                                            Keyword = step.Type
                                                        });
                                                }
                                                else
                                                {
                                                    _log.Info("Adding step with final text '" + step.Value.Replace("{", "<").Replace("}", ">") + "' and type 'given' on position '" + scenario.Steps.Count + 1 + "' to scenario with name '" + scenario.Title + "'.");
                                                    scenario.Steps.Insert(Convert.ToInt16(step.Position),
                                                        new Given
                                                        {
                                                            Text = step.Value.Replace("{", "<").Replace("}", ">"),
                                                            Keyword = step.Type
                                                        });
                                                }
                                                break;
                                            default:
                                                throw new Exception("Error mentioned type '" + step.Type.ToLower() +
                                                                    "' didn't match any specflow step type (given, when, then,)!");
                                        }
                                    }
                                }
                                else
                                {
                                    _log.Info("Steps returned by the filter type are 0 so no actions will be taken.");
                                }
                            }
                            else
                            {
                                _log.Info("Steps filter attribute is not present.");
                                AddUnFiltratedSteps(scenario);
                            }

                            #endregion
                        }
                        catch (Exception e)
                        {
                            _log.Error("When trying to use filter classes from assembly with path " + _customeConfigurationSection.FilterAssembly.Filepath);
                            _log.Error(e.Message);
                            _log.Error(e.StackTrace);
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
            _log.Info("Initializing custom configuration section with assembly path '" + _appConfig.AppSettings.Settings["Custom.plugin.generator.configuration"].Value + "' app.config file '" + Directory.GetCurrentDirectory() + @"\App.config" + "' and configuration type 'GeneratorPluginConfiguration'.");
            _customeConfigurationSection =
                new GeneratorPluginConfiguration().GetConfig<GeneratorPluginConfiguration>(
                    _appConfig.AppSettings.Settings["Custom.plugin.generator.configuration"].Value, Directory.GetCurrentDirectory() + @"\App.config",
                    "GeneratorPluginConfiguration");
            _log.Info("Initializing finished");
        }

        private void InitializeConfiguration()
        {
            _log.Info("Initializing app.config file from location '" + Directory.GetCurrentDirectory() + @"\App.config'");
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
                    foreach (AdditionalCategoryAttribute categoryAttribute in additionalCategoryAttributes)
                    {
                        _log.Info("Adding category with type '" + categoryAttribute.Type + "' and value '" + categoryAttribute.Value + "' to scenario with name '" + scenario.Title + "'.");
                        scenario.Tags = scenario.Tags ?? new Tags();
                        scenario.Tags.Add(categoryAttribute.Type.Contains("unique")
                            ? new Tag(categoryAttribute.Value + GenerateuniqueId())
                            : new Tag(categoryAttribute.Value));
                    }
                }
                else
                {
                    _log.Info("Additional category attributes element is present but it didn't contain any child elements!");
                }
            }
            else
            {
                _log.Info("Additional category attributes element is not present!");
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
                                    new GherkinTable(new GherkinTableRow(new GherkinTableCell[0]),
                                        new GherkinTableRow[0])
                            })
                    };

                    var tableContents = scenarioOutline.Examples.ExampleSets.First().Table.Body.ToList();

                    _log.Info(tableContents.Count + " test case attributes retrieved from scenario with name '" + scenario.Title + "'");

                    for (var i = 0; i < additionalTestCaseAttributes.Count; i++)
                    {
                        _log.Info("Trying to add test case attribute with type'" + additionalTestCaseAttributes[i].Type + "' and value '" + additionalTestCaseAttributes[i].Value + "' to scenario with name " + scenario.Title);

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

                    }
                    _log.Info(tableContents.Count + " test case attributes retrieved from scenario with name '" + scenario.Title + "' after updating.");

                    scenarioOutline.Examples.ExampleSets.First().Table.Body = tableContents.ToArray();

                    alteredScenarios.Add(scenarioOutline);

                    feature.Scenarios = alteredScenarios.ToArray();
                }
                else
                {
                    _log.Info("Additional test case attributes element is present but it didn't contain any child elements!");
                }
            }
            else
            {
                _log.Info("Additional test case attributes element is not present!");
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
                                if (Convert.ToInt16(step.Position) > scenario.Steps.Count)
                                {
                                    _log.Info("Adding step with final text '" + step.Value.Replace("{", "<").Replace("}", ">") + "' and type 'given' on position '" + scenario.Steps.Count + 1 + "' to scenario with name '" + scenario.Title + "'.");
                                    scenario.Steps.Add(
                                        new Given
                                        {
                                            Text = step.Value.Replace("{", "<").Replace("}", ">"),
                                            Keyword = step.Type
                                        });
                                }
                                else
                                {
                                    _log.Info("Adding step with final text '" + step.Value.Replace("{", "<").Replace("}", ">") + "' and type 'given' on position '" + scenario.Steps.Count + 1 + "' to scenario with name '" + scenario.Title + "'.");
                                    scenario.Steps.Insert(Convert.ToInt16(step.Position),
                                        new Given
                                        {
                                            Text = step.Value.Replace("{", "<").Replace("}", ">"),
                                            Keyword = step.Type
                                        });
                                }
                                break;
                            case "when":
                                if (Convert.ToInt16(step.Position) > scenario.Steps.Count)
                                {
                                    _log.Info("Adding step with final text '" + step.Value.Replace("{", "<").Replace("}", ">") + "' and type 'when' on position '" + scenario.Steps.Count + 1 + "' to scenario with name '" + scenario.Title + "'.");
                                    scenario.Steps.Add(
                                        new Given
                                        {
                                            Text = step.Value.Replace("{", "<").Replace("}", ">"),
                                            Keyword = step.Type
                                        });
                                }
                                else
                                {
                                    _log.Info("Adding step with final text '" + step.Value.Replace("{", "<").Replace("}", ">") + "' and type 'given' on position '" + scenario.Steps.Count + 1 + "' to scenario with name '" + scenario.Title + "'.");
                                    scenario.Steps.Insert(Convert.ToInt16(step.Position),
                                        new Given
                                        {
                                            Text = step.Value.Replace("{", "<").Replace("}", ">"),
                                            Keyword = step.Type
                                        });
                                }
                                break;
                            case "then":
                                if (Convert.ToInt16(step.Position) > scenario.Steps.Count)
                                {
                                    _log.Info("Adding step with final text '" + step.Value.Replace("{", "<").Replace("}", ">") + "' and type 'then' on position '" + scenario.Steps.Count + 1 + "' to scenario with name '" + scenario.Title + "'.");
                                    scenario.Steps.Add(
                                        new Given
                                        {
                                            Text = step.Value.Replace("{", "<").Replace("}", ">"),
                                            Keyword = step.Type
                                        });
                                }
                                else
                                {
                                    _log.Info("Adding step with final text '" + step.Value.Replace("{", "<").Replace("}", ">") + "' and type 'given' on position '" + scenario.Steps.Count + 1 + "' to scenario with name '" + scenario.Title + "'.");
                                    scenario.Steps.Insert(Convert.ToInt16(step.Position),
                                        new Given
                                        {
                                            Text = step.Value.Replace("{", "<").Replace("}", ">"),
                                            Keyword = step.Type
                                        });
                                }
                                break;
                            default:
                                throw new Exception("Error mentioned type '" + step.Type.ToLower() +
                                                    "' didn't match any specflow step type (given, when, then,)!");
                        }
                    }
                }
                else
                {
                    _log.Info("Additional steps element is present but it didn't contains any child elements!");
                }
            }
            else
            {
                _log.Info("Additional steps element is not present!");
            }
        }

        private void SetUpLogger()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.RemoveAllAppenders(); /*Remove any other appenders*/
            var fileAppender = new FileAppender
            {
                AppendToFile = true,
                LockingModel = new FileAppender.MinimalLock(),
                File = Directory.GetCurrentDirectory() + @"\GeneratorPluginLog.txt"
            };
            var pl = new PatternLayout { ConversionPattern = "%date [%-5level] [%C] [%M] - %message%newline" };
            pl.ActivateOptions();
            fileAppender.Layout = pl;
            fileAppender.ActivateOptions();

            log4net.Config.BasicConfigurator.Configure(fileAppender);
        }
    }
}


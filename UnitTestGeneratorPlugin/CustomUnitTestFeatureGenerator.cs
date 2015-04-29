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
    public class CustomUnitTestFeatureGenerator : UnitTestFeatureGenerator, IFeatureGenerator
    {
        public readonly List<int> UniqueIdList;
        private readonly Random _rnd;
        private Configuration _appConfig;
        public GeneratorPluginConfiguration CustomeConfigurationSection;
        private readonly ILog _log = LogManager.GetLogger(typeof(CustomUnitTestFeatureGenerator));
        public bool SuccessfulInitialization;
        private bool _customConfigurationSectionSuccessfulInitialization;
        public string AppconfigFile = Directory.GetCurrentDirectory() + @"\App.config";
        public bool SuccessfulLoggerConfiguration;

        public CustomUnitTestFeatureGenerator(IUnitTestGeneratorProvider testGeneratorProvider,
            CodeDomHelper codeDomHelper,
            GeneratorConfiguration generatorConfiguration,
            IDecoratorRegistry decoratorRegistry)
            : base(testGeneratorProvider, codeDomHelper, generatorConfiguration, decoratorRegistry)
        {
            UniqueIdList = new List<int>();
            _rnd = new Random();
        }

        public class Container
        {
            public Container(Feature feature, string testClassName, string targetNamespace)
            {
                Feature = feature;
                TestClassName = testClassName;
                TargetNamespace = targetNamespace;
            }

            public Feature Feature { get; private set; }
            public string TestClassName { get; private set; }
            public string TargetNamespace { get; private set; }
        }

        private void AdduniqueIdToScenarios(Feature feature)
        {
            foreach (var scenario in feature.Scenarios)
            {
                scenario.Tags = scenario.Tags ?? new Tags();
                var uniqueTag = "uniqueId:" + GenerateuniqueId();
                scenario.Tags.Add(new Tag(uniqueTag));
                _log.Info("Tag '" + uniqueTag + "' added to scenario with name '" + scenario.Title + "'.");
            }
        }

        public Container TransformFeature(Feature feature, string testClassName, string targetNamespace)
        {
            SetUpLogger();
            _log.Info("Starting custom plugin generator.");
            InitializeConfiguration(AppconfigFile);

            if (!SuccessfulInitialization)
            {
                _log.Warn("Initialization of app.config failed.");
                _log.Warn("Only unique id category will be added to generate unit tests for feature '" + feature.Title + "'.");
                AdduniqueIdToScenarios(feature);
                return new Container(feature, testClassName, targetNamespace);
            }

            InitializeCustomConfigurationSection(AppconfigFile);

            if (!_customConfigurationSectionSuccessfulInitialization)
            {
                _log.Warn("No generator plugin configuration section in app.config.");
                _log.Warn("Only unique id category will be added to generate unit tests for feature '" + feature.Title + "'.");
                AdduniqueIdToScenarios(feature);
                return new Container(feature, testClassName, targetNamespace);
            }
            _log.Info("Generator plugin configuration section in app.config present.");
            _log.Info("Its values will be used during unit tests generation for feature with name '" + feature.Title + "'.");
            foreach (var scenario in feature.Scenarios)
            {
                if (CustomeConfigurationSection.FilterAssembly.ElementInformation.IsPresent)
                {
                    _log.Info("Filter assembly element is present in the plugin configuration section.");
                    try
                    {
                        var filterAssemblyPath = Path.GetFullPath(CustomeConfigurationSection.FilterAssembly.Filepath);
                        _log.Info("Filter assembly file path property is '" + filterAssemblyPath + "'.");
                        Assembly assemblyContainingFilter = null;
                        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                        foreach (var loadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            try
                            {
                                _log.Debug("Assembly with full name '" + loadedAssembly.FullName + "' is loaded in default app domain with path '" + loadedAssembly.Location + "'");

                            }
                            catch (Exception)
                            {
                                _log.Error("Exception appear when trying to get the location of '" + loadedAssembly.FullName + "'!");
                            }
                            var assemblyFileName = loadedAssembly.FullName.Split(",".ToCharArray())[0];
                            var filterAssemblyFileName = Path.GetFileNameWithoutExtension(filterAssemblyPath);
                            if (assemblyFileName.Equals(filterAssemblyFileName))
                            {
                                _log.Debug("Assembly with name '" + assemblyFileName + "', which is same as the filter assembly name already exist in default app domain, so the assembly from the app domain will be used instead of trying to load it again.");
                                assemblyContainingFilter = loadedAssembly;
                            }
                        }

                        if (assemblyContainingFilter == null)
                        {
                            assemblyContainingFilter = Assembly.Load(File.ReadAllBytes(filterAssemblyPath));
                        }

                        var categoriesFilter = CustomeConfigurationSection.AdditionalCategoryAttributeFilter;
                        var stepsFilter = CustomeConfigurationSection.StepFilter;
                        var testCaseAttributeFilter = CustomeConfigurationSection.AdditionalTestCaseAttributeFilter;
                        try
                        {
                            #region Test case attributes filtering

                            if (testCaseAttributeFilter.ElementInformation.IsPresent)
                            {
                                _log.Info("Test case attribute filter element is present with class name '" + testCaseAttributeFilter.Classname + "' and method '" + testCaseAttributeFilter.Method + "' properties.");

                                var rawTestCaseAttribute = new List<AdditionalTestCaseAttribute>();

                                _log.Info("Creating a temporal list with test case attributes which will be send to the filter method.");

                                foreach (var testCaseAttribute in CustomeConfigurationSection.AdditionalTestCaseAttributes)
                                {
                                    _log.Info("Adding test case attribute with type '" + ((AdditionalTestCaseAttribute)testCaseAttribute).Type + "' and value '" + ((AdditionalTestCaseAttribute)testCaseAttribute).Value + " to the temporary list");

                                    rawTestCaseAttribute.Add(((AdditionalTestCaseAttribute)testCaseAttribute));
                                }

                                _log.Info("Loaded filter assembly is '" + assemblyContainingFilter.GetName() + "'.");
                                var filterType = assemblyContainingFilter.GetType(testCaseAttributeFilter.Classname);
                                _log.Info("Test case attribute filter type is '" + filterType.Name + "' creating method info object.");
                                var methodInfo = filterType.GetMethod(testCaseAttributeFilter.Method,
                                    new[] { typeof(List<AdditionalTestCaseAttribute>), typeof(Scenario), typeof(ILog) });
                                var o = Activator.CreateInstance(filterType);
                                var filteredTestCaseAttributes =
                                    (List<GherkinTableRow>)
                                        methodInfo.Invoke(o, new object[] { rawTestCaseAttribute, scenario, _log });
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

                                    var headerRow = scenarioOutline.Examples.ExampleSets.First().Table.Header.Cells.ToList();
                                    var categoryTypes = new List<string>();
                                    var tempAttribute = new List<string>();

                                    foreach (var testCaseAttributeRow in filteredTestCaseAttributes)
                                    {
                                        _log.Info("Trying to add test case attribute row with cells: ");

                                        foreach (var cell in testCaseAttributeRow.Cells)
                                        {
                                            tempAttribute.Add(cell.Value);
                                            _log.Info(cell.Value);
                                        }

                                        _log.Info("to scenario with name '" + scenario.Title + "'");

                                        tableContents.Add(testCaseAttributeRow);

                                    }

                                    foreach (var testCaseAttribute in rawTestCaseAttribute)
                                    {
                                        foreach (var categoryValue in tempAttribute)
                                        {
                                            if (testCaseAttribute.Value.Contains(categoryValue) && !categoryTypes.Contains(testCaseAttribute.Type))
                                            {
                                                _log.Info("Adding '" + testCaseAttribute.Type + "' to the header row cells of scenario '" + scenario.Title + "'.");
                                                categoryTypes.Add(testCaseAttribute.Type);
                                                headerRow.Add(new GherkinTableCell(testCaseAttribute.Type));
                                            }
                                        }
                                    }

                                    scenarioOutline.Examples.ExampleSets.First().Table.Header.Cells = headerRow.ToArray();
                                    _log.Info(tableContents.Count + " test case attributes retrieved from scenario with name '" + scenario.Title + "' after updating.");

                                    scenarioOutline.Examples.ExampleSets.First().Table.Body = tableContents.ToArray();

                                    for (var counter = 0; counter < feature.Scenarios.Count(); counter++)
                                    {
                                        if (feature.Scenarios[counter].Title.Equals(scenario.Title))
                                        {
                                            _log.Info("Scenario with name '" + scenarioOutline.Title + "' should be on '" + counter + "' place in the list of scenarios for feature '" + feature.Title + "'.");
                                            feature.Scenarios[counter] = scenarioOutline;
                                        }
                                    }
                                }
                                else
                                {
                                    _log.Info("Test case attributes returned by the filter type are 0 so no actions will be taken.");
                                }
                            }
                            else
                            {
                                _log.Info("Test case attribute filter element is not present.");
                                AddTestCaseAttributes(null, scenario, feature);
                            }

                            #endregion
                        }
                        catch (Exception e)
                        {
                            _log.Error("When trying to filter test case attributes by filter with class name '" + testCaseAttributeFilter.Classname + "' and method name '" + testCaseAttributeFilter.Method + "'!");
                            _log.Error(e.Message);
                            _log.Error(e.StackTrace);
                        }
                        try
                        {
                            #region Categories filtering

                            if (categoriesFilter.ElementInformation.IsPresent)
                            {
                                _log.Info("Categories filter element is present with class name '" + categoriesFilter.Classname + "' and method '" + categoriesFilter.Method + "' properties.");
                                var rawCategories = new List<AdditionalCategoryAttribute>();
                                _log.Info("Creating a temporal list with category attributes which will be send to the filter method.");
                                foreach (var categoryAttribute in CustomeConfigurationSection.AdditionalCategoryAttributes)
                                {
                                    _log.Info("Adding category with type '" + ((AdditionalCategoryAttribute)categoryAttribute).Type + "' and value '" + ((AdditionalCategoryAttribute)categoryAttribute).Value + " to the temporary list");
                                    rawCategories.Add(((AdditionalCategoryAttribute)categoryAttribute));
                                }
                                _log.Info("Loaded filter assembly is '" + assemblyContainingFilter.GetName() + "'.");
                                var filterType = assemblyContainingFilter.GetType(categoriesFilter.Classname);
                                _log.Info("Category filter type is '" + filterType.Name + "'.");
                                var methodInfo = filterType.GetMethod(categoriesFilter.Method,
                                    new[] { typeof(List<AdditionalCategoryAttribute>), typeof(Scenario), typeof(ILog) });
                                var o = Activator.CreateInstance(filterType);
                                var filteredCategories =
                                    (List<AdditionalCategoryAttribute>)
                                        methodInfo.Invoke(o, new object[] { rawCategories, scenario, _log });
                                _log.Info("Categories returned by the filter method are '" + filteredCategories.Count + "'.");
                                if (filteredCategories.Count > 0)
                                {
                                    _log.Info("Clearing all category attributes for scenario '" + scenario.Title + "'");
                                    scenario.Tags = scenario.Tags ?? new Tags();
                                    scenario.Tags.Clear();
                                    AddCategoryAttributes(filteredCategories, scenario);
                                }
                                else
                                {
                                    _log.Info("Categories returned by the filter type are 0 so no actions will be taken.");
                                }
                            }
                            else
                            {
                                _log.Info("Category filter element is not present.");
                                AddCategoryAttributes(null, scenario);
                            }

                            #endregion
                        }
                        catch (Exception e)
                        {
                            _log.Error("When trying to filter category attributes by filter with class name '" + categoriesFilter.Classname + "' and method name '" + categoriesFilter.Method + "'!");
                            _log.Error(e.Message);
                            _log.Error(e.StackTrace);
                        }
                        try
                        {
                            #region Steps filtering

                            if (stepsFilter.ElementInformation.IsPresent)
                            {
                                _log.Info("Steps filter element is present with class name '" + stepsFilter.Classname + "' and method '" + stepsFilter.Method + "' properties.");
                                var rawSteps = new List<Step>();
                                _log.Info("Creating a temporal list with steps which will be send to the filter method.");
                                foreach (var step in CustomeConfigurationSection.Steps)
                                {
                                    _log.Info("Adding step with type '" + ((Step)step).Type + "' and value '" + ((Step)step).Value + " to the temporary list");
                                    rawSteps.Add(((Step)step));
                                }
                                _log.Info("Loaded filter assembly is '" + assemblyContainingFilter.GetName() + "'.");
                                var filterType = assemblyContainingFilter.GetType(stepsFilter.Classname);
                                _log.Info("Step filter type is '" + filterType.Name + "'.");
                                var methodInfo = filterType.GetMethod(stepsFilter.Method,
                                    new[] { typeof(List<Step>), typeof(Scenario), typeof(ILog) });
                                var o = Activator.CreateInstance(filterType);
                                var filteredSteps =
                                    (List<Step>)
                                        methodInfo.Invoke(o, new object[] { rawSteps, scenario, _log });
                                _log.Info("Steps returned by the filter method are '" + filteredSteps.Count + "'.");
                                if (filteredSteps.Count > 0)
                                {
                                    AddSteps(filteredSteps, scenario);
                                }
                                else
                                {
                                    _log.Info("Steps returned by the filter type are 0 so no actions will be taken.");
                                }
                            }
                            else
                            {
                                _log.Info("Steps filter attribute is not present.");
                                AddSteps(null, scenario);
                            }

                            #endregion
                        }
                        catch (Exception e)
                        {
                            _log.Error("When trying to filter steps by filter with class name '" + stepsFilter.Classname + "' and method name '" + stepsFilter.Method + "'!");
                            _log.Error(e.Message);
                            _log.Error(e.StackTrace);
                        }
                    }
                    catch (Exception e)
                    {
                        _log.Error("When trying to use filter classes from assembly with path " + CustomeConfigurationSection.FilterAssembly.Filepath);
                        _log.Error(e.Message);
                        _log.Error(e.StackTrace);
                    }
                }
                else
                {
                    AddTestCaseAttributes(null, scenario, feature);
                    AddCategoryAttributes(null, scenario);
                    AddSteps(null, scenario);
                }
            }

            return new Container(feature, testClassName, targetNamespace);
        }

        public new CodeNamespace GenerateUnitTestFixture(Feature feature, string testClassName, string targetNamespace)
        {
            var transformedItems = TransformFeature(feature, testClassName, targetNamespace);
            return base.GenerateUnitTestFixture(transformedItems.Feature, transformedItems.TestClassName, transformedItems.TargetNamespace);
        }

        public int GenerateuniqueId()
        {
            var uniqueId = _rnd.Next(0, 999999);
            if (UniqueIdList.Contains(uniqueId))
            {
                UniqueIdList.Sort();
                uniqueId = UniqueIdList.Last() + 1;
            }
            UniqueIdList.Add(uniqueId);
            return uniqueId;
        }

        public void InitializeCustomConfigurationSection(string appConfig)
        {
            try
            {
                _log.Info("Initializing custom configuration section with assembly path '" + _appConfig.AppSettings.Settings["Custom.plugin.generator.configuration"].Value + "' app.config file '" + Directory.GetCurrentDirectory() + @"\App.config" + "' and configuration type 'GeneratorPluginConfiguration'.");
                CustomeConfigurationSection =
                    new GeneratorPluginConfiguration().GetConfig<GeneratorPluginConfiguration>(
                        _appConfig.AppSettings.Settings["Custom.plugin.generator.configuration"].Value, appConfig,
                        "GeneratorPluginConfiguration");
                _log.Info("Initializing finished");
                _customConfigurationSectionSuccessfulInitialization = true;
            }
            catch (Exception e)
            {
                _log.Info("Initialization of custom plugin configuration section failed because of exception! " + e.Message);
                _customConfigurationSectionSuccessfulInitialization = false;
            }

        }

        public void InitializeConfiguration(String appConfigFile)
        {
            _log.Info("Initializing app.config file from location '" + appConfigFile);
            try
            {
                var fileMap = new ExeConfigurationFileMap
                {
                    ExeConfigFilename = appConfigFile
                };

                _appConfig = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

                if (_appConfig.AppSettings.Settings.AllKeys.Any())
                {
                    _log.Info("Loading of app.config finished, checking if 'Custom.plugin.generator.configuration' element is present in app.config.");
                    if (!String.IsNullOrEmpty(_appConfig.AppSettings.Settings["Custom.plugin.generator.configuration"].Value))
                    {
                        _log.Info("Custom.plugin.generator.configuration is present in app.config so initialization was successful.");
                        SuccessfulInitialization = true;
                        return;
                    }
                    _log.Info("Custom.plugin.generator.configuration is not present in app.config so initialization was unsuccessful.");
                    SuccessfulInitialization = false;
                }
                else
                {
                    _log.Info("Initialization failed because created configuration is empty.");
                    SuccessfulInitialization = false;
                }
            }
            catch (Exception)
            {
                _log.Warn("Exception appear when trying to initialize the app.config file successful initialization set to false.");
                SuccessfulInitialization = false;
            }
        }

        public void AddCategoryAttributes(List<AdditionalCategoryAttribute> additionalCategoryAttributes, Scenario scenario)
        {
            if (CustomeConfigurationSection.AdditionalCategoryAttributes.ElementInformation.IsPresent)
            {
                if (additionalCategoryAttributes == null)
                {
                    additionalCategoryAttributes = CustomeConfigurationSection.AdditionalCategoryAttributes.Cast<AdditionalCategoryAttribute>().ToList();
                }

                if (additionalCategoryAttributes.Any())
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

        public void AddTestCaseAttributes(List<AdditionalTestCaseAttribute> additionalTestCaseAttributes, Scenario scenario, Feature feature)
        {
            if (CustomeConfigurationSection.AdditionalTestCaseAttributes.ElementInformation.IsPresent)
            {
                if (additionalTestCaseAttributes == null)
                {
                    additionalTestCaseAttributes = CustomeConfigurationSection.AdditionalTestCaseAttributes.Cast<AdditionalTestCaseAttribute>().ToList();
                }
                if (additionalTestCaseAttributes.Any())
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

                    var headerRow = scenarioOutline.Examples.ExampleSets.First().Table.Header.Cells.ToList();
                    if (!headerRow.Any())
                    {
                        var categoryTypes = new List<string>();
                        foreach (var additionalTestCaseAttribute in additionalTestCaseAttributes)
                        {
                            if (!categoryTypes.Contains(additionalTestCaseAttribute.Type))
                            {
                                _log.Info("Adding '" + additionalTestCaseAttribute.Type +
                                          "' to the header row cells of scenario '" + scenario.Title + "'.");
                                categoryTypes.Add(additionalTestCaseAttribute.Type);
                                headerRow.Add(new GherkinTableCell(additionalTestCaseAttribute.Type));
                            }
                            _log.Info("Type '" + additionalTestCaseAttribute.Type +
                                      "' is already added to the header row cells of scenario '" + scenario.Title + "'.");
                        }
                        scenarioOutline.Examples.ExampleSets.First().Table.Header.Cells = headerRow.ToArray();
                    }

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

                    for (var counter = 0; counter < feature.Scenarios.Count(); counter++)
                    {
                        if (feature.Scenarios[counter].Title.Equals(scenario.Title))
                        {
                            _log.Info("Scenario with name '" + scenarioOutline.Title + "' should be on '" + counter + "' place in the fist of scenarios for feature '" + feature.Title + "'.");
                            feature.Scenarios[counter] = scenarioOutline;
                        }
                    }
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

        public void AddSteps(List<Step> additionalSteps, Scenario scenario)
        {
            if (CustomeConfigurationSection.Steps.ElementInformation.IsPresent)
            {
                if (additionalSteps == null)
                {
                    additionalSteps = CustomeConfigurationSection.Steps.Cast<Step>().ToList();
                }

                if (additionalSteps.Any())
                {
                    foreach (var step in additionalSteps)
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
                                    _log.Info("Adding step with final text '" + step.Value.Replace("{", "<").Replace("}", ">") + "' and type 'given' on position '" + step.Position + "' to scenario with name '" + scenario.Title + "'.");
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
                                        new When
                                        {
                                            Text = step.Value.Replace("{", "<").Replace("}", ">"),
                                            Keyword = step.Type
                                        });
                                }
                                else
                                {
                                    _log.Info("Adding step with final text '" + step.Value.Replace("{", "<").Replace("}", ">") + "' and type 'given' on position '" + step.Position + "' to scenario with name '" + scenario.Title + "'.");
                                    scenario.Steps.Insert(Convert.ToInt16(step.Position),
                                        new When
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
                                        new Then
                                        {
                                            Text = step.Value.Replace("{", "<").Replace("}", ">"),
                                            Keyword = step.Type
                                        });
                                }
                                else
                                {
                                    _log.Info("Adding step with final text '" + step.Value.Replace("{", "<").Replace("}", ">") + "' and type 'given' on position '" + step.Position + "' to scenario with name '" + scenario.Title + "'.");
                                    scenario.Steps.Insert(Convert.ToInt16(step.Position),
                                        new Then
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

        public void SetUpLogger()
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
            SuccessfulLoggerConfiguration = true;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            _log.Debug("Resolving dependency with name : " + args.Name);
            var pathToDependancy = "";
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.Equals(args.Name))
                {
                    pathToDependancy = assembly.Location;
                    break;
                }
            }
            _log.Info("Assembly we are trying to load is located in: " + pathToDependancy);
            return Assembly.LoadFile(pathToDependancy);
        }
    }
}


using BoDi;
using TechTalk.SpecFlow.Configuration;
using TechTalk.SpecFlow.Generator.Configuration;
using TechTalk.SpecFlow.Generator.Plugins;
using TechTalk.SpecFlow.Generator.UnitTestConverter;
using TechTalk.SpecFlow.Infrastructure;
using UnitTestGeneratorPlugin.Generator.SpecFlowPlugin;

[assembly: GeneratorPlugin(typeof(CustomGeneratorPlugin))]
[assembly: RuntimePlugin(typeof(CustomGeneratorPlugin))]
namespace UnitTestGeneratorPlugin.Generator.SpecFlowPlugin
{
    class CustomGeneratorPlugin : IGeneratorPlugin, IRuntimePlugin
    {
        public void RegisterDependencies(ObjectContainer container)
        { }

        public void RegisterCustomizations(ObjectContainer container, SpecFlowProjectConfiguration generatorConfiguration)
        {
            container.RegisterTypeAs<CustomFeatureGeneratorProvider, IFeatureGeneratorProvider>("default");
        }

        public void RegisterConfigurationDefaults(SpecFlowProjectConfiguration specFlowConfiguration)
        { }

        public void RegisterCustomizations(ObjectContainer container, RuntimeConfiguration runtimeConfiguration)
        {

            container.RegisterTypeAs<CustomFeatureGeneratorProvider, IFeatureGeneratorProvider>("default");

        }

        public void RegisterConfigurationDefaults(RuntimeConfiguration runtimeConfiguration)
        { }
    }
}
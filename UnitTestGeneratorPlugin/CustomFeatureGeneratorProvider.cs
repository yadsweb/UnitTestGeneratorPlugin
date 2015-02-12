using BoDi;
using TechTalk.SpecFlow.Generator.UnitTestConverter;
using TechTalk.SpecFlow.Parser.SyntaxElements;

namespace UnitTestGeneratorPlugin.Generator.SpecFlowPlugin
{
    /// <summary>
    /// This class selects the <see cref="CustomUnitTestFeatureGenerator"/> as the implementation of <see cref="IFeatureGenerator"/>
    /// </summary>
    class CustomFeatureGeneratorProvider : IFeatureGeneratorProvider
    {
        private readonly ObjectContainer _container;

        protected CustomFeatureGeneratorProvider(ObjectContainer container)
        {
            _container = container;
        }

        public int Priority
        {
            get { return int.MaxValue; }
        }

        public bool CanGenerate(Feature feature)
        {
            return true;
        }

        public IFeatureGenerator CreateGenerator(Feature feature)
        {
            return _container.Resolve<CustomUnitTestFeatureGenerator>();
        }
    }
}
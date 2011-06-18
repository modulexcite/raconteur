using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Raconteur.Helpers;
using Raconteur.IDE;

namespace Raconteur.Compilers
{
    public interface FeatureCompiler
    {
        void Compile(Feature Feature, FeatureItem FeatureItem);
        IEnumerable<string> StepNamesOf(Feature Feature, FeatureItem FeatureItem);
    }

    public class FeatureCompilerClass : FeatureCompiler 
    {
        public TypeResolver TypeResolver { get; set; }

        public StepCompiler StepCompiler { get; set; }

        List<string> Assemblies;

        Feature Feature { get; set; }

        public void Compile(Feature Feature, FeatureItem FeatureItem)
        {
            if (Feature is InvalidFeature) return;

            this.Feature = Feature;

            LoadAssemblies(FeatureItem);
            CompileFeature();
            CompileSteps();
        }

        public virtual IEnumerable<string> StepNamesOf(Feature Feature, FeatureItem FeatureItem)
        {
            if (Feature is InvalidFeature) return new List<string>();

            this.Feature = Feature;

            LoadAssemblies(FeatureItem);
            return StepDefinitions.SelectMany(type => 
                type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Select(method => method.Name.IdentifierToEnglish()));
        }

        void LoadAssemblies(FeatureItem FeatureItem) 
        {
            Assemblies = new List<string> {FeatureItem.Assembly};
            Assemblies.AddRange(Settings.Libraries);
        }

        void CompileSteps()
        {
            foreach (var Step in Feature.Steps)
                Step.Method = Feature.StepDefinitions
                    .SelectMany(l => l.GetMethods())
                    .Where(Method => StepCompiler.Matches(Method, Step))
                    .FirstOrDefault();
        }


        void CompileFeature() 
        {
            Feature.StepDefinitions = StepDefinitions;
        }

        List<Type> StepDefinitions
        {
            get 
            {
                return new List<string> { Feature.Name }
                    .Union(Feature.DeclaredStepDefinitions)
                    .Union(Settings.StepDefinitions)
                    .Select(TypeOfStepDefinitions)
                    .Where(Type => Type != null)
                    .ToList();
            }
        }

        Type TypeOfStepDefinitions(string ClassName)
        {
            return Assemblies
                .Select(Assembly => TypeResolver.TypeOf(ClassName, Assembly))
                .FirstOrDefault(Type => Type != null);
        }
    }
}
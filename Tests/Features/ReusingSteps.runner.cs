using MbUnit.Framework;
using Features.StepDefinitions;

namespace Features 
{
    [TestFixture]
    public partial class ReusingSteps 
    {
        public FeatureRunner FeatureRunner = new FeatureRunner();
        public HighlightFeature HighlightFeature = new HighlightFeature();

        
        [Test]
        public void ReusingStepDefinitions()
        {         
            FeatureRunner.Given_the_setting__contains("Libraries", "Common");        
            FeatureRunner.Given_the_Feature_contains(
@"
using Step Definitions
Scenario: Reuse a Step
Step from Step Definitions
");        
            FeatureRunner.The_Runner_should_contain(
@"
public StepDefinitions StepDefinitions = new StepDefinitions();
[TestMethod]
public void ReuseAStep()
{
StepDefinitions.Step_from_Step_Definitions();
}
");
        }
        
        [Test]
        public void ReusingStepsFromMultipleStepDefinitions()
        {         
            FeatureRunner.Given_the_setting__contains("Libraries", "Common");        
            FeatureRunner.Given_the_Feature_contains(
@"
using Step Definitions
using another Step Definitions
Scenario: Reuse Steps from multiple Definitions
Step from Step Definitions
Step from another Step Definitions
");        
            FeatureRunner.The_Runner_should_contain(
@"
public StepDefinitions StepDefinitions = new StepDefinitions();
public AnotherStepDefinitions AnotherStepDefinitions = new AnotherStepDefinitions();
[TestMethod]
public void ReuseStepsFromMultipleDefinitions()
{
StepDefinitions.Step_from_Step_Definitions();
AnotherStepDefinitions.Step_from_another_Step_Definitions();
}
");
        }
        
        [Test]
        public void ReusingGlobalStepDefinitions()
        {         
            FeatureRunner.Given_the_setting__contains("Libraries", "Common");        
            FeatureRunner.Given_the_setting__contains("StepDefinitions", "StepDefinitions");        
            FeatureRunner.Given_the_Feature_contains(
@"
Scenario: Reuse a Step in global Step Definition
Step from Step Definitions
");        
            FeatureRunner.The_Runner_should_contain(
@"
public StepDefinitions StepDefinitions = new StepDefinitions();
[TestMethod]
public void ReuseAStepInGlobalStepDefinition()
{
StepDefinitions.Step_from_Step_Definitions();
}
");
        }
        
        [Test]
        public void ReusingStepDefinitionsFromLibraries()
        {         
            FeatureRunner.Given_the_setting__contains("Libraries", "Common");        
            FeatureRunner.Given_the_Feature_contains(
@"
using Step Definitions in Library
Scenario: Reuse Steps from Library
Step from Step Definitions in Library
");        
            FeatureRunner.The_Runner_should_contain(
@"
public StepDefinitionsInLibrary StepDefinitionsInLibrary = new StepDefinitionsInLibrary();
[TestMethod]
public void ReuseStepsFromLibrary()
{
StepDefinitionsInLibrary.Step_from_Step_Definitions_in_Library();
}
");
        }

    }
}
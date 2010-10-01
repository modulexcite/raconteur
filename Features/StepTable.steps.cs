using Specs;

namespace Features 
{
    public partial class StepTable : FeatureRunner
    {
        void When_a_Table_is_declared()
        {
            Feature = 
            @"
                Feature: Feature Name

                Scenario: Scenario Name
                    Verify some values:
                    |X|Y|
                    |1|0|
                    |0|1|
            ";
        }

        void Each_row_should_become_a_Step_with_cols_as_Args()
        {
            Runner.ShouldContainInOrder
            (
                @"Verify_some_values_(1, 0);",
                @"Verify_some_values_(0, 1);"
            );
        }

        void When_a_Table_declaration_has_Args()
        {
            Feature = 
            @"
                Feature: Feature Name

                Scenario: Scenario Name
                    Given stuff in ""X"" place
                    | stuff |
                    |one    |
                    |another|
            ";
        }

        void Each_Step_will_start_with_the_Args()
        {
            Runner.ShouldContainInOrder
            (
                @"Given_stuff_in__place(""X"", ""one"");",
                @"Given_stuff_in__place(""X"", ""another"");"
            );
        }
    }
}
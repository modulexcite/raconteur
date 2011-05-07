﻿using System.Collections.Generic;
using System.Linq;
using Raconteur.Helpers;

namespace Raconteur.Generators.Steps
{
    public class StepWithSimpleTableGenerator : StepCodeGenerator
    {
        public const string StepRowExecutionTemplate = 
            @"        
                new[] {{{0}}},";

        public StepWithSimpleTableGenerator(StepGenerator StepGenerator) : base(StepGenerator) {}

        public override string Code
        {
            get 
            {
                var Table = Step.Table.Rows
                    .Aggregate("", (Steps, Row) => Steps +
                        string.Format(StepRowExecutionTemplate, ArgsFrom(Row)));

                var ArgsCode = Table.RemoveTail(1);

                if (Step.HasArgs)
                    ArgsCode = ArgsCode.Insert(0, "\r\n" + CodeForArgsOnly + ",\r\n");

                return string.Format
                (
                    StepGenerator.MultilineStepExecution,
                    Step.Name,
                    ArgsCode
                );
            }
        }
        
        string ArgsFrom(IEnumerable<string> Row)
        {
            StepGenerator.Row = Row;

            return string.Join(", ", StepGenerator.FormatArgsForTable);
        }

        string CodeForArgsOnly { get { return string.Join(", ", StepGenerator.FormatArgsOnly); } }
    }
}
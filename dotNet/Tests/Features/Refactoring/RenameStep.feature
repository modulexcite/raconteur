﻿Feature: Rename Step
	In order to make changes easier
	it should be possible to rename a step 
	having the changes propagated accordingly

Scenario: Step within Feature

	Given the Feature contains
	"
		Scenario: Name
			old Step
			old Step
	"

	When "old Step" is renamed to "new Step"

	The Feature should contain
	"
		Scenario: Name
			new Step
			new Step
	"

Scenario: Step shared among Features

	Given the Feature "Alpha" contains
	"
		Scenario: Name
			// Step in Alpha
			Step
	"

	And the Feature "Beta" contains
	"
		Scenario: Name
			// Step in Beta
			Step
	"

	Rename "Step" to "new Step"

	The Feature "Alpha" should contain
	"
		Scenario: Name
			// Step in Alpha
			new Step
	"

	and the Feature "Beta" should contain
	"
		Scenario: Name
			// Step in Beta
			new Step
	"

Scenario: Step renamed twice

	Given the Feature "A" contains
	"
		Scenario: Name
			Step
	"

	Rename "Step" to "new Step"

	The Feature "A" should contain
	"
		Scenario: Name
			new Step
	"

	Rename "new Step" to "old Step"

	The Feature "A" should contain
	"
		Scenario: Name
			old Step
	"

Scenario: Step with Args

	Given the Feature contains
	"
		Scenario: Name
			simple Step
			Step ""with Arg""
	"

	When "Step" is renamed to "new Step"

	The Feature should contain
	"
		Scenario: Name
			simple Step
			new Step ""with Arg""
	"

Scenario: Step with Multiline Arg

	Given the Feature contains
	"
		Scenario: With Multiline Arg in Step
			Step 
			""
				Multiline Arg
			""
	"

	When "Step" is renamed to "new Step"

	The Feature should contain
	"
		Scenario: With Multiline Arg in Step
			new Step
			""
				Multiline Arg
			""
	"

Scenario: Step with Table

	Given the Feature contains
	"
		Scenario: Step with Table
			Step 
			|X|0|X|
			|X|X|0|
			|0|0|X|
	"

	When "Step" is renamed to "new Step"

	The Feature should contain
	"
		Scenario: Step with Table
			new Step
			|X|0|X|
			|X|X|0|
			|0|0|X|
	"

Scenario: Step with Header Table

	Given the Feature contains
	"
		Scenario: Step with Header Table
			Step 
			[X|Y]
			|1|0|
			|0|0|
	"

	When "Step" is renamed to "new Step"

	The Feature should contain
	"
		Scenario: Step with Header Table
			new Step
			[X|Y]
			|1|0|
			|0|0|
	"

Scenario: Step with Multiline Arg, Args and Header Table

	Given the Feature contains
	"
		Scenario: Step with Multiline Arg, Args and Header Table
			""
				Multiline Arg
			""
			Step ""Arg""
			[X|Y]
			|1|0|
			|0|0|
			""Arg"" Step ""Arg""
			Step ""Arg"" Step
	"

	When "Step" is renamed to "new Step"

	The Feature should contain
	"
		Scenario: Step with Multiline Arg, Args and Header Table
			""
				Multiline Arg
			""
			new Step ""Arg""
			[X|Y]
			|1|0|
			|0|0|
			""Arg"" new Step ""Arg""
			Step ""Arg"" Step
	"

Scenario: Step in Scenario Outline

	Given the Feature contains
	"
		Scenario Ouline: Step in Scenario Outline

			Step ""X""
			""Y"" Step
			Step ""X"" + ""Y""

			Examples:
			|X|Y|
			|1|0|
			|0|0|
	"

	When "Step" is renamed to "new Step"

	The Feature should contain
	"
		Scenario Ouline: Step in Scenario Outline

			new Step ""X""
			""Y"" new Step
			Step ""X"" + ""Y""

			Examples:
			|X|Y|
			|1|0|
			|0|0|
	"

Scenario: Step mentioned in comments

	Given the Feature contains
	"
		Scenario: Name
			// Step
			Step
	"

	When "Step" is renamed to "new Step"

	The Feature should contain
	"
		Scenario: Name
			// Step
			new Step
	"

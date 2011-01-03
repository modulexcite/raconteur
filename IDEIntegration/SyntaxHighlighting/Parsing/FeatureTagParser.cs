using System;
using System.Collections.Generic;
using System.Linq;
using Raconteur.IDEIntegration.SyntaxHighlighting.Token;

namespace Raconteur.IDEIntegration.SyntaxHighlighting.Parsing
{
    public class FeatureTagParser : TagsParserBase
    {
        readonly TagsParser Keywords;
        readonly TagsParser Args;
        readonly TagsParser Table;
        readonly TagsParser Comments;
        readonly TagsParser Scenarios;
        readonly TagsParser Multiline;
        readonly TagsParser ScenarioTags;

        public FeatureTagParser(TagFactory TagFactory, string Feature) 
            : base(new ParsingState())
        {
            this.TagFactory = TagFactory;
            this.Feature = Feature;

            Keywords = new KeywordParser(ParsingState);
            Args = new ArgsParser(ParsingState);
            Table = new TableParser(ParsingState);
            Comments = new CommentsParser(ParsingState);
            Scenarios = new ScenariosParser(ParsingState);
            Multiline = new MultilineParser(ParsingState);
            ScenarioTags = new ScenarioTagsParser(ParsingState);
        }

        public override IEnumerable<ITagSpanWrap<FeatureTokenTag>> Tags
        {
            get
            {
                Position = 0;
                Action<int> NextPosition = LineLength => Position += LineLength + 2;

                return 
                    Feature
                    .Lines()
                    .ApplyLengthTo(NextPosition)
                    .SelectMany(TagsFromLine);
            }
        }

        IEnumerable<ITagSpanWrap<FeatureTokenTag>> TagsFromLine(string Line)
        {
            FullLine = Line;
            this.Line = Line.Trim();

            return 
                Multiline.Tags ?? 
                Comments.Tags ??
                (
                    ScenarioTags.Tags ??
                    Keywords.Tags ??
                    Table.Tags ??
                    Args.Tags ??
                    new List<ITagSpanWrap<FeatureTokenTag>>()
                )
                .Union(Scenarios.Tags);
        }
    }
}
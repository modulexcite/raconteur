﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using ITags=System.Collections.Generic.IEnumerable<Microsoft.VisualStudio.Text.Tagging.ITagSpan<Raconteur.IDEIntegration.SyntaxHighlighting.Token.FeatureTokenTag>>;
using ITagsWrap=System.Collections.Generic.IEnumerable<Raconteur.IDEIntegration.SyntaxHighlighting.Token.ITagSpanWrap<Raconteur.IDEIntegration.SyntaxHighlighting.Token.FeatureTokenTag>>;
using Tags=System.Collections.Generic.List<Microsoft.VisualStudio.Text.Tagging.ITagSpan<Raconteur.IDEIntegration.SyntaxHighlighting.Token.FeatureTokenTag>>;
using TagsWrap=System.Collections.Generic.List<Raconteur.IDEIntegration.SyntaxHighlighting.Token.ITagSpanWrap<Raconteur.IDEIntegration.SyntaxHighlighting.Token.FeatureTokenTag>>;

namespace Raconteur.IDEIntegration.SyntaxHighlighting.Token
{
    public interface ITagSpanWrap<T> where T : ITag
    {
        TagSpan<T> Core { get; }

        string Text { get; }

        FeatureTokenTypes Type { get; }
    }

    public class TagSpanWrap<T> : ITagSpanWrap<T> where T : ITag
    {
        public TagSpan<T> Core { get; set; }

        public string Text { get { return Core.Span.GetText(); } }

        public FeatureTokenTypes Type { get { return (Core.Tag as FeatureTokenTag).Type; } }
    }

    public class FeatureTokenTagger : ITagger<FeatureTokenTag>
    {
        protected string Feature;
        ITextSnapshot Snapshot;

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged { add {} remove {} }

        public ITags GetTags(NormalizedSnapshotSpanCollection Spans)
        {
//            System.Diagnostics.Debugger.Launch();
            Snapshot = Spans[0].Snapshot;
            Feature = Snapshot.GetText();
            
            return from Tag in Tags select Tag.Core;
        }

        public ITagsWrap Tags
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

        string FullLine;
        string Line;
        int Position;

        ITagsWrap TagsFromLine(string Line)
        {
            FullLine = Line;
            this.Line = Line.Trim();

            return 
                MultiLineTags ?? 
                CommentTags ??
                (KeywordTags ??
                 TableTags ??
                 ArgsTags ??
                 new TagsWrap())
                 .Union(ScenarioTag);
        }

        ITagsWrap TableTags
        {
            get
            {
                if (!Line.StartsWith("|")) return null;
                
                var Index = FullLine.IndexOf('|') + 1;
                var StartPoint = Position;

                return 
                    from Arg in Line.Split('|').Chop(1)
                    select CreateTag
                    (
                        (StartPoint += Arg.Length) - Arg.Length + Index++, 
                        Arg.Length, 
                        FeatureTokenTypes.Arg
                    );
            }
        }

        ITagsWrap ArgsTags
        {
            get
            {
                if (!Line.Contains('"')) return null;

                return 
                    from Arg in Line.Split('"').Odds().Distinct()
                    from Index in FullLine.IndexesOf(Arg)
                    select CreateTag
                    (
                        Position + Index - 1, 
                        Arg.Length + 2, 
                        FeatureTokenTypes.Arg
                    );
            }
        }

        readonly List<string> Keywords = new List<string>
        {
            Settings.Language.Feature,
            Settings.Language.Scenario,
            Settings.Language.ScenarioOutline,
            Settings.Language.Examples,
        };

        TagsWrap KeywordTags
        {
            get
            {
                var Keyword = Line.Split(':')[0];

                return !Keywords.Contains(Keyword) ? null : new TagsWrap
                {
                    CreateTag
                    (
                        Position + FullLine.IndexOf(Keyword), 
                        Keyword.Length + 1, 
                        FeatureTokenTypes.Keyword
                    )
                };
            }
        }

        int ScenarioStart;

        bool IsLastLine
        {
            get
            {
                return Position + FullLine.Length == Feature.Length;
            }
        }

        bool IsEndOfScenario
        {
            get
            {
                if (IsLastLine) return true;

                if (!Line.StartsWith(Settings.Language.Scenario)) return false;

                if (ScenarioStart > 0) return true;
                
                ScenarioStart = Position;

                return false;
            }
        }

        TagsWrap ScenarioTag
        {
            get
            {
                var Tags = new TagsWrap();

                if (IsEndOfScenario)
                {
                    Tags.Add(CreateTag
                    (
                        ScenarioStart,
                        Position - ScenarioStart + (IsLastLine ? FullLine.Length : -2),
                        FeatureTokenTypes.ScenarioBody
                    ));

                    ScenarioStart = IsLastLine ? 0 : Position;
                }

                return Tags;
            }
        }

        TagsWrap CommentTags
        {
            get
            {
                return !Line.StartsWith("//") ? null : new TagsWrap
                {
                    CreateTag
                    (
                        Position + FullLine.IndexOf("//"), 
                        Line.Length, 
                        FeatureTokenTypes.Comment
                    )
                };
            }
        }

        int MultilineTagStart = -1;
        string MultilineSymbol;
        bool IsInsideMultilineTag { get { return MultilineTagStart >= 0; } }

        ITagsWrap MultiLineTags
        {
            get
            {
                return 
                    CloseMultilineTag ??
                    InsideMultilineTag ??
                    StartMultilineTag;
            }
        }

        bool IsMultilineTagStart
        {
            get
            {
                return 
                    !IsInsideMultilineTag && 
                    (Line == "/*" || Line == "\"");
            } 
        }

        TagsWrap StartMultilineTag
        {
            get
            {
                if (!IsMultilineTagStart) return null;

                MultilineTagStart = Position + FullLine.IndexOf(Line);
                MultilineSymbol = Line;

                return new TagsWrap();
            }
        }

        TagsWrap InsideMultilineTag
        {
            get { return IsInsideMultilineTag ? new TagsWrap() : null; } 
        }

        bool IsClosingMultilineTag
        {
            get
            {
                return 
                    IsInsideMultilineTag && 
                    (
                        IsLastLine ||
                        IsClosingMultilineComment || 
                        IsClosingMultilineArg
                    );
            }
        }

        bool IsInsideMultilineComment { get { return MultilineSymbol == "/*"; } }

        bool IsInsideMultilineArg { get { return MultilineSymbol == "\""; } }

        bool IsClosingMultilineComment
        {
            get { return IsInsideMultilineComment && Line == "*/"; }
        }

        bool IsClosingMultilineArg
        {
            get { return IsInsideMultilineArg && Line == "\""; }
        }

        TagsWrap CloseMultilineTag
        {
            get
            {
                if (!IsClosingMultilineTag) return null;

                var Tags = new TagsWrap
                {
                    CreateTag
                    (
                        MultilineTagStart, 
                        Position + FullLine.Length - MultilineTagStart, 
                        MultilineSymbol == "/*" ? 
                            FeatureTokenTypes.Comment : 
                            FeatureTokenTypes.Arg
                    )
                };

                MultilineTagStart = -1;

                return Tags;
            }
        }

        ITagSpanWrap<FeatureTokenTag> EmptyTag
        {
            get { return CreateTag(0, 0, FeatureTokenTypes.Keyword); }
        }

        protected virtual ITagSpanWrap<FeatureTokenTag> CreateTag(int StartLocation, int Length, FeatureTokenTypes Type)
        {
            try 
            {
                var tokenSpan = new SnapshotSpan(Snapshot, new Span(StartLocation, Length));

                return new TagSpanWrap<FeatureTokenTag> 
                {
                    Core = new TagSpan<FeatureTokenTag>(tokenSpan, new FeatureTokenTag(Type))
                };

            } catch { return EmptyTag; }
        }
    }
}
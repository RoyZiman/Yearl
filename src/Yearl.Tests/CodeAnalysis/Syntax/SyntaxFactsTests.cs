﻿using Yearl.CodeAnalysis.Syntax;

namespace Yearl.Tests.CodeAnalysis.Syntax
{
    public class SyntaxFactsTests
    {
        [Theory]
        [MemberData(nameof(GetSyntaxKindData))]
        public void SyntaxFact_GetText_RoundTrips(SyntaxKind kind)
        {
            string text = SyntaxFacts.GetText(kind);
            if (text == null)
                return;

            IEnumerable<SyntaxToken> tokens = SyntaxTree.ParseTokens(text);
            var token = Assert.Single(tokens);
            Assert.Equal(kind, token.Kind);
            Assert.Equal(text, token.Text);
        }

        public static IEnumerable<object[]> GetSyntaxKindData()
        {
            var kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
            foreach (var kind in kinds)
                yield return new object[] { kind };
        }
    }
}
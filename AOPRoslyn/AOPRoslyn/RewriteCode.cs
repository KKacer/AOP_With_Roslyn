﻿using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AOPRoslyn
{
    public class RewriteCode
    {
        public RewriteCode() : this(AOPFormatter.DefaultFormatter)
        {

        }
        public AOPFormatter Formatter { get; internal set; }
        public RewriteOptions Options { get; internal set; }
        public RewriteCode(AOPFormatter formatter)
        {
            Formatter = formatter;
            Options = new RewriteOptions();
        }
        public string Code { get; set; }
        
        public virtual string RewriteCodeMethod()
        {
            var tree = CSharpSyntaxTree.ParseText(Code);
            
            var node = tree.GetRoot();
            node=ModifyRegionToTrivia(node);
            var LG = new MethodRewriter(Formatter, Options);
                    
            var sn = LG.Visit(node);
            var data= sn.ToFullString();
            //BUG - cannot have this space between #line and number
            data = data.Replace(Environment.NewLine + "#line", Environment.NewLine + "#line ");
            return data;
        }

        private SyntaxNode ModifyRegionToTrivia(SyntaxNode syntaxNode)
        {
            while (true)
            {
                var trivia = syntaxNode.DescendantTrivia(null, false)
                    .Select(it=>(SyntaxTrivia)it)
                    .Select(it=>new { it= it,Kind=it.Kind() })
                    .ToArray()
                    .FirstOrDefault(t => t.Kind == SyntaxKind.RegionDirectiveTrivia || t.Kind == SyntaxKind.EndRegionDirectiveTrivia);
                if (trivia == null || trivia.Kind == SyntaxKind.None)
                {
                    break;
                }
                syntaxNode = syntaxNode.ReplaceTrivia(trivia.it, SyntaxFactory.Comment("//was a region " + Environment.NewLine));
            }
            return syntaxNode;
        }
    }
}

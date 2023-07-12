﻿using System.Text.RegularExpressions;
using Avalonia.Input;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OneWare.Shared;
using OneWare.Shared.EditorExtensions;
using OneWare.Shared.LanguageService;
using OneWare.Shared.Services;
using OneWare.Vhdl.Folding;
using OneWare.Vhdl.Indentation;
using Prism.Ioc;

namespace OneWare.Vhdl
{
    internal class TypeAssistanceVhdl : TypeAssistanceLsp
    {
        private readonly Regex _usedWordsRegex = new(@"\w{3,}");
        
        public TypeAssistanceVhdl(IEditor editor, LanguageServiceVhdl ls) : base(editor, ls)
        {
            CodeBox.TextArea.IndentationStrategy = IndentationStrategy = new VhdlIndentationStrategy(CodeBox.Options);
            FoldingStrategy = new RegexFoldingStrategy(FoldingRegexVhdl.FoldingStart, FoldingRegexVhdl.FoldingEnd);
        }
        
        public override string LineCommentSequence => "--";
        

        public override async Task<List<CompletionData>> GetCustomCompletionItemsAsync()
        {
            var items = new List<CompletionData>();

            var text = Editor.CurrentDocument.Text;
            var usedWords = await Task.Run(() => _usedWordsRegex.Matches(text));

            items.Add(new CompletionData("library IEEE;\nuse IEEE.std_logic_1164.all;\nuse IEEE.numeric_std.all; ",
                "ieee", "IEEE Standard Packages",
                TypeAssistanceIconStore.Instance.Icons[CompletionItemKind.Reference], 0, CodeBox.CaretOffset));
            
            items.Add(new CompletionData(
                "entity " + Path.GetFileNameWithoutExtension(Editor.CurrentFile.Header) +
                " is\n    port(\n        [I/Os]$0\n    );\nend entity " +
                Path.GetFileNameWithoutExtension(Editor.CurrentFile.Header) + ";", "entity", "Entity Declaration",
                TypeAssistanceIconStore.Instance.Icons[CompletionItemKind.Class], 0, CodeBox.CaretOffset));
            
            foreach (var word in usedWords)
            {
                if(word.ToString() is { } s)
                    items.Add(new CompletionData(s, s, "Used word in document", TypeAssistanceIconStore.Instance.Icons[CompletionItemKind.Snippet], 0, CodeBox.CaretOffset));
            }

            return items;
        }

        public override void TypeAssistance(TextInputEventArgs e)
        {
            if ((e.Text?.Contains(';') ?? false) && Service.IsLanguageServiceReady)
            {
                var line = CodeBox.Document.GetLineByOffset(CodeBox.CaretOffset).LineNumber;
                Format(line, line);
            }
        }
    }
}
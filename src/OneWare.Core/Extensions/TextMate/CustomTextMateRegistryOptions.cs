﻿using System.Runtime.InteropServices;
using Avalonia.Platform;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Themes;

namespace OneWare.Core.Extensions.TextMate;

public class CustomTextMateRegistryOptions : IAdvancedRegistryOptions
{
    private List<TextMateLanguage> _availableLanguages = new();
    private Dictionary<string, string> _extensionLinks = new();

    private readonly RegistryOptions _defaultRegistryOptions = new(ThemeName.DarkPlus);

    public void RegisterLanguage(string id, string grammarPath, params string[] extensions)
    {
        _availableLanguages.Add(new TextMateLanguage()
        {
            Id = id,
            GrammarPath = grammarPath,
            Extensions = extensions
        });
    }

    public void RegisterExtensionLink(string source, string target)
    {
        _extensionLinks.TryAdd(source, target);
    }
    
    public IRawGrammar GetGrammar(string scopeName)
    {
        //For Browser Textmate is not working as of now
        //if (RuntimeInformation.ProcessArchitecture == Architecture.Wasm) return null!;
            
        var g = _availableLanguages.FirstOrDefault(x => x.Id == scopeName.Split('.').Last());

        if (g == null) return _defaultRegistryOptions.GetGrammar(scopeName);
        using var s = new StreamReader(AssetLoader.Open(new Uri(g.GrammarPath)));
        {
            return GrammarReader.ReadGrammarSync(s);
        }
    }

    public ICollection<string> GetInjections(string scopeName)
    {
        return _defaultRegistryOptions.GetInjections(scopeName);
    }
    
    public Language? GetLanguageByExtension(string extension)
    {
        var def = _availableLanguages.FirstOrDefault(x => x.Extensions.Contains(extension));
        if (def != null) 
            return new Language()
            {
                Id = def.Id,
            };

        _extensionLinks.TryGetValue(extension, out var target);
        return _defaultRegistryOptions.GetLanguageByExtension(target ?? extension);
    }

    public string GetScopeByLanguageId(string languageId)
    {
        var r = _availableLanguages.FirstOrDefault(x => x.Id == languageId);
        if (r != null) return $"source.{r.Id}";
        return _defaultRegistryOptions.GetScopeByLanguageId(languageId);
    }
    
    public IRawTheme GetDefaultTheme()
    {
        return _defaultRegistryOptions.GetDefaultTheme();
    }
    public IRawTheme GetTheme(string scopeName)
    {
        return _defaultRegistryOptions.GetTheme(scopeName);
    }
    public IRawTheme LoadTheme(ThemeName name) => _defaultRegistryOptions.LoadTheme(name);
}
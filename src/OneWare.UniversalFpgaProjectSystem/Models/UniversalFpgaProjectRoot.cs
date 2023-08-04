﻿using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia.Media.Imaging;
using OneWare.ProjectSystem.Models;
using OneWare.Shared.Converters;
using OneWare.Shared.Helpers;
using OneWare.Shared.Models;
using OneWare.Shared.Services;
using Prism.Ioc;

namespace OneWare.UniversalFpgaProjectSystem.Models;

public class UniversalFpgaProjectRoot : ProjectRoot, IProjectRootWithFile
{
    public const string ProjectFileExtension = ".fpgaproj";
    public const string ProjectType = "UniversalFPGAProject";
    public DateTime LastSaveTime { get; set; }
    public override string ProjectPath => ProjectFilePath;
    public override string ProjectTypeId => ProjectType;
    public string ProjectFilePath { get; }
    public JsonObject Properties { get; }

    public UniversalFpgaProjectRoot(string projectFilePath, JsonObject properties) : base(Path.GetDirectoryName(projectFilePath) ?? throw new NullReferenceException("Invalid Project Path"))
    {
        ProjectFilePath = projectFilePath;
        Properties = properties;
        
        Icon = SharedConverters.PathToBitmapConverter.Convert(ContainerLocator.Container.Resolve<IPaths>().AppIconPath, typeof(Bitmap), null, null) as Bitmap;
    }
    
    public override bool IsPathIncluded(string relativePath)
    {
        var includes = Properties["Include"].Deserialize<string[]>();
        var excludes = Properties["Exclude"].Deserialize<string[]>();

        if (includes == null && excludes == null) return true;
        
        return ProjectHelpers.MatchWildCards(relativePath, includes ?? new[] { "*.*" }, excludes);
    }

    public override void IncludePath(string path)
    {
        if(!Properties.ContainsKey("Include")) 
            Properties.Add("Include", new JsonArray());
        
        Properties["Include"]!.AsArray().Add(path);
    }
}
﻿using OneWare.Shared.Models;
using OneWare.Shared.Services;
using OneWare.UniversalFpgaProjectSystem.Models;

namespace OneWare.OssCadSuiteIntegration.Yosys;

public class YosysService
{
    private readonly IChildProcessService _childProcessService;
    
    public YosysService(IChildProcessService childProcessService)
    {
        _childProcessService = childProcessService;
    }

    public async Task SynthAsync(UniversalFpgaProjectRoot project)
    {
        var fpga = "ice40";// project.Properties["Fpga"];
        var top = project.Properties["TopEntity"];

        var verilogFiles = string.Join(" ", project.Files.Where(x => x.Extension == ".v").Select(x => x.RelativePath));
        var yosysFlags = string.Empty;
        
        await _childProcessService.ExecuteShellAsync("yosys", 
            $"-p \"synth_{fpga} -json synth.json\" {yosysFlags}{verilogFiles}",
            project.FullPath, "Yosys Synth");
    }
    
    public async Task CreateNetListJsonAsync(IProjectFile verilog)
    {
        await _childProcessService.ExecuteShellAsync("yosys", 
            $"-p \"hierarchy -auto-top; proc; opt; memory -nomap; wreduce -memx; opt_clean\" -o {verilog.Header}.json {verilog.Header}", 
            Path.GetDirectoryName(verilog.FullPath)!, "Create Netlist...");
    }
}
using CompilerWebApplication.Api.Dto;
using CompilerWebApplication.Services;
using Microsoft.AspNetCore.Mvc;

namespace CompilerWebApplication.Api;

[ApiController]
[Route("/api")]
public class Compile : ControllerBase
{
    private readonly ILogger<Compile> _logger;

    public Compile(ILogger<Compile> logger)
    {
        _logger = logger;
    }

    [HttpPost("compile")]

    public IResult CompileCode([FromBody] SourceCodeDto sourceCode)
    {
        _logger.LogInformation("Received {Request} with {Size} code length", nameof(CompileCode),
            sourceCode.SourceCode.Length);
        var compiler = new CompileService();
        return compiler.ProcessCompilationRequest(sourceCode);
    }

}
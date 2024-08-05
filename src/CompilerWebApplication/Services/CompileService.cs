using System.Diagnostics;
using System.Text;
using CompilerWebApplication.Api.Dto;
using Newtonsoft.Json;

namespace CompilerWebApplication.Services;

public record FailResult
{
    public string ErrorText { get; set; }
}

public record SuccessResult
{
    public string AbstractSyntaxTreeText { get; set; }
    public string BoundAbstractSyntaxTreeText { get; set; }
    public string ResultText { get; set; }
    public string LlvmCode { get; set; }
}

public class CompileService
{
    public CompileService()
    {
    }

    public IResult ProcessCompilationRequest(SourceCodeDto sourceCodeDto)
    {
        var processInfo = new ProcessStartInfo("docker", $"run -it --rm compiler text {sourceCodeDto.SourceCode}")
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };


        int exitCode;
        using var process = new Process();
        process.StartInfo = processInfo;
        process.Start();
        process.WaitForExit(1200000);
        if (!process.HasExited)
        {
            process.Kill();
        }

        var output = process.StandardOutput.ReadToEnd();

        exitCode = process.ExitCode;
        Console.WriteLine(output);
        process.Close();
        var sb = new StringBuilder();
        var start = false;
        foreach (var c in output)
        {
            if (c == '{')
            {
                start = true;
            }

            if (start)
            {
                sb.Append(c);
            }
        }

        while (sb.Length > 0 && sb[^1] != '}')
        {
            sb.Length--;
        }

        try
        {
            // return Results.Ok(sb.ToString());
            var result = JsonConvert.DeserializeObject<SuccessResult>(sb.ToString());
            return Results.Ok(result);
        }
        catch (Exception)
        {
            try
            {
                var err = JsonConvert.DeserializeObject<FailResult>(sb.ToString());
                return Results.BadRequest(err);
            }
            catch (Exception)
            {
                return Results.UnprocessableEntity("Cannot compile code");
            }
        }
    }
}
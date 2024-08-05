using Newtonsoft.Json;

namespace ImperativeCompiler;


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

public class Reporter
{
    public void ReportSuccess()
    {
        string abstractSyntaxTreeText;
        try
        {
            using var abstractSyntaxTreeReader = new StreamReader("AST.txt");
            abstractSyntaxTreeText = abstractSyntaxTreeReader.ReadToEnd();
        }
        catch (Exception)
        {
            abstractSyntaxTreeText = string.Empty;
        }

        string boundSyntaxTreeText;
        try
        {
            using var boundSyntaxTreeReader = new StreamReader("B_AST.txt");
            boundSyntaxTreeText = boundSyntaxTreeReader.ReadToEnd();
        }
        catch (Exception)
        {
            boundSyntaxTreeText = string.Empty;
        }

        string resultText;
        try
        {
            using var resultReader = new StreamReader("Output.txt");
            resultText = resultReader.ReadToEnd();
        }
        catch (Exception)
        {
            resultText = string.Empty;
        }

        string llvmCodeText;
        try
        {
            using var llvmCodeStream = new StreamReader("output.ll");
            llvmCodeText = llvmCodeStream.ReadToEnd();
        }
        catch (Exception)
        {
            llvmCodeText = string.Empty;
        }

        var successResult = new SuccessResult
        {
            AbstractSyntaxTreeText = abstractSyntaxTreeText,
            BoundAbstractSyntaxTreeText = boundSyntaxTreeText,
            ResultText = resultText,
            LlvmCode = llvmCodeText
        };

        var successString = JsonConvert.SerializeObject(successResult);
        Console.ResetColor();
        Console.WriteLine(successString);
    }

    public void ReportFailure()
    {
        string errorText;
        try
        {
            using var errorLogReader = new StreamReader("ERROR_LOG.txt");
            errorText = errorLogReader.ReadToEnd();
        }
        catch (Exception)
        {
            errorText = "Unknown error";
        }

        var failResult = new FailResult
        {
            ErrorText = errorText
        };

        var failText = JsonConvert.SerializeObject(failResult);
        Console.ResetColor();
        Console.WriteLine(failText);
    }
}
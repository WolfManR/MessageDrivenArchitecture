using System.Text;
using Xunit.Abstractions;

namespace TestsExtensions;

public class TestWriter : TextWriter
{
    private readonly ITestOutputHelper _output;
    public override Encoding Encoding { get; } = Encoding.UTF8;

    public TestWriter(ITestOutputHelper output)
    {
        _output = output;
    }

    public override void WriteLine(string? value)
    {
        _output.WriteLine(value);
    }
}
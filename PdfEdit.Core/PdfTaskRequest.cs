namespace PdfEdit.Core;

public sealed class PdfTaskRequest
{
    public PdfOperationType OperationType { get; init; }
    public string QpdfPath { get; init; } = string.Empty;
    public IReadOnlyList<string> InputFiles { get; init; } = Array.Empty<string>();
    public string OutputFolder { get; init; } = string.Empty;
    public string OutputFileName { get; init; } = string.Empty;
    public string SplitOutputPattern { get; init; } = "output-%d.pdf";
    public int RotateAngle { get; init; }
    public string PageOrder { get; init; } = string.Empty;
}

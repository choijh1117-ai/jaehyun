using System.Diagnostics;

namespace PdfEdit.Core;

public sealed class PdfTaskService
{
    public async Task RunAsync(
        PdfTaskRequest request,
        CancellationToken cancellationToken,
        IProgress<double>? progress = null,
        ILogSink? log = null)
    {
        ValidateRequest(request);
        Directory.CreateDirectory(request.OutputFolder);

        var arguments = BuildArguments(request);
        log?.WriteLine($"qpdf args: {arguments}");

        var startInfo = new ProcessStartInfo
        {
            FileName = request.QpdfPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        process.OutputDataReceived += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
            {
                log?.WriteLine(args.Data);
            }
        };

        process.ErrorDataReceived += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
            {
                log?.WriteLine($"ERR: {args.Data}");
            }
        };

        process.Exited += (_, _) => tcs.TrySetResult(process.ExitCode);

        progress?.Report(0);
        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start qpdf process.");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var registration = cancellationToken.Register(() =>
        {
            if (!process.HasExited)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (InvalidOperationException)
                {
                }
            }
        });

        var exitCode = await tcs.Task.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        if (exitCode != 0)
        {
            throw new InvalidOperationException($"qpdf exited with code {exitCode}.");
        }

        progress?.Report(100);
    }

    private static void ValidateRequest(PdfTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.QpdfPath))
        {
            throw new ArgumentException("QPDF path is required.");
        }

        if (!File.Exists(request.QpdfPath))
        {
            throw new FileNotFoundException("QPDF executable not found.", request.QpdfPath);
        }

        if (request.InputFiles.Count == 0)
        {
            throw new ArgumentException("At least one input file is required.");
        }

        if (string.IsNullOrWhiteSpace(request.OutputFolder))
        {
            throw new ArgumentException("Output folder is required.");
        }
    }

    private static string BuildArguments(PdfTaskRequest request)
    {
        var outputPath = Path.Combine(request.OutputFolder, request.OutputFileName);

        return request.OperationType switch
        {
            PdfOperationType.Merge => BuildMergeArgs(request, outputPath),
            PdfOperationType.Split => BuildSplitArgs(request),
            PdfOperationType.Rotate => BuildRotateArgs(request, outputPath),
            PdfOperationType.Reorder => BuildReorderArgs(request, outputPath),
            _ => throw new ArgumentOutOfRangeException(nameof(request.OperationType))
        };
    }

    private static string BuildMergeArgs(PdfTaskRequest request, string outputPath)
    {
        var inputs = string.Join(" ", request.InputFiles.Select(Quote));
        return $"--empty --pages {inputs} -- {Quote(outputPath)}";
    }

    private static string BuildSplitArgs(PdfTaskRequest request)
    {
        var input = Quote(request.InputFiles[0]);
        var outputPattern = Quote(Path.Combine(request.OutputFolder, request.SplitOutputPattern));
        return $"--split-pages {input} {outputPattern}";
    }

    private static string BuildRotateArgs(PdfTaskRequest request, string outputPath)
    {
        var angle = request.RotateAngle switch
        {
            90 => "+90",
            180 => "+180",
            270 => "+270",
            -90 => "-90",
            -180 => "-180",
            -270 => "-270",
            _ => throw new ArgumentException("Rotate angle must be 90, 180, or 270 (or negative).")
        };

        var input = Quote(request.InputFiles[0]);
        return $"--rotate={angle}:1-z {input} {Quote(outputPath)}";
    }

    private static string BuildReorderArgs(PdfTaskRequest request, string outputPath)
    {
        if (string.IsNullOrWhiteSpace(request.PageOrder))
        {
            throw new ArgumentException("Page order is required for reorder.");
        }

        var input = Quote(request.InputFiles[0]);
        return $"--pages {input} {request.PageOrder} -- {Quote(outputPath)}";
    }

    private static string Quote(string value) => $"\"{value}\"";
}

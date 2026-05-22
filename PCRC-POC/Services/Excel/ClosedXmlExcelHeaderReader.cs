using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using PCRC.ServicesInterface.Storage;

namespace PCRC.Services.Excel;

/// Reads sheet 1, row 1 of an .xlsx workbook using ClosedXML. Returns the used cells in column
/// order, trimmed; empty cells produce no entry — that matches the fingerprint convention (the
/// canonical form is sorted + joined, so positional gaps would be invisible anyway).
public sealed class ClosedXmlExcelHeaderReader : IExcelHeaderReader
{
    private readonly ILogger<ClosedXmlExcelHeaderReader> _logger;

    public ClosedXmlExcelHeaderReader(ILogger<ClosedXmlExcelHeaderReader> logger)
    {
        _logger = logger;
    }

    public async Task<IReadOnlyList<string>> ReadFirstRowAsync(Stream content, CancellationToken cancellationToken)
    {
        var workbookStream = await EnsureSeekableAsync(content, cancellationToken);
        try
        {
            using var workbook = new XLWorkbook(workbookStream);
            var sheet = workbook.Worksheets.FirstOrDefault()
                ?? throw new InvalidOperationException("Workbook contains no worksheets.");

            var headers = sheet.Row(1).CellsUsed()
                .OrderBy(c => c.Address.ColumnNumber)
                .Select(c => c.GetString().Trim())
                .Where(s => s.Length > 0)
                .ToList();

            _logger.LogInformation("Read {Count} headers from sheet '{Name}'.", headers.Count, sheet.Name);
            return headers;
        }
        finally
        {
            if (!ReferenceEquals(workbookStream, content))
            {
                await workbookStream.DisposeAsync();
            }
        }
    }

    /// ClosedXML's reader uses random access into the underlying ZIP package, so the stream has to
    /// be seekable. ASP.NET Core's <c>IFormFile.OpenReadStream()</c> already returns a seekable
    /// stream against the buffered request body, so this copy is a safety net for callers that
    /// hand us a forward-only stream (e.g. a raw network read).
    private static async Task<Stream> EnsureSeekableAsync(Stream content, CancellationToken cancellationToken)
    {
        if (content.CanSeek)
        {
            content.Position = 0;
            return content;
        }

        var buffered = new MemoryStream();
        await content.CopyToAsync(buffered, cancellationToken);
        buffered.Position = 0;
        return buffered;
    }
}

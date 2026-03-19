using MediatR;
using Microsoft.AspNetCore.Mvc;
using RagBot.Application.Documents.Commands;

namespace RagBot.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KnowledgeBaseController : ControllerBase
{
    private readonly IMediator _mediator;

    public KnowledgeBaseController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("ingest")]
    public async Task<IActionResult> Ingest([FromBody] IngestRequest request)
    {
        var command = new IngestDocumentCommand(request.Content, request.Metadata);
        var ids = await _mediator.Send(command);
        return Ok(new { Ids = ids });
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        string content = string.Empty;

        if (file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            using var stream = file.OpenReadStream();
            using var document = UglyToad.PdfPig.PdfDocument.Open(stream);
            foreach (var page in document.GetPages())
            {
                content += page.Text + " ";
            }
        }
        else
        {
            using var reader = new StreamReader(file.OpenReadStream());
            content = await reader.ReadToEndAsync();
        }

        var command = new IngestDocumentCommand(content, file.FileName);
        var ids = await _mediator.Send(command);
        return Ok(new { Ids = ids });
    }
}

public record IngestRequest(string Content, string Metadata);

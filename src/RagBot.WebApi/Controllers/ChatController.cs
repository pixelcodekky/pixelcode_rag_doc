using MediatR;
using Microsoft.AspNetCore.Mvc;
using RagBot.Application.Chat.Queries;

namespace RagBot.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] AskRequest request)
    {
        var query = new AskQuestionQuery(request.Question);
        var answer = await _mediator.Send(query);
        return Ok(new { Answer = answer });
    }

    [HttpPost("ask-stream")]
    public async Task AskStream([FromBody] AskRequest request)
    {
        Response.ContentType = "text/plain";
        var query = new AskQuestionStreamQuery(request.Question);
        var stream = _mediator.CreateStream(query);

        await foreach (var chunk in stream)
        {
            await Response.WriteAsync(chunk);
            await Response.Body.FlushAsync();
        }
    }
}

public record AskRequest(string Question);

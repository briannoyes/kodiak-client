using Microsoft.AspNetCore.Mvc;
using PCRC.ServicesInterface.Clients;
using PCRC.ServicesInterface.Clients.Dtos;

namespace PCRC.Api.Controllers;

[ApiController]
[Route("api/clients")]
public sealed class ClientsController : ControllerBase
{
    private readonly IClientService _clients;

    public ClientsController(IClientService clients)
    {
        _clients = clients;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClientDto>>> GetClients(
        CancellationToken cancellationToken)
        => Ok(await _clients.ListAsync(cancellationToken));

    [HttpGet("{externalId:guid}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> GetClient(
        Guid externalId,
        CancellationToken cancellationToken)
    {
        var client = await _clients.GetByExternalIdAsync(externalId, cancellationToken);
        return client is null ? NotFound() : Ok(client);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClientDto>> CreateClient(
        [FromBody] CreateClientRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Name is required.");
        var created = await _clients.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetClient),
            new { externalId = created.ExternalId },
            created);
    }

    [HttpDelete("{externalId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteClient(
        Guid externalId,
        CancellationToken cancellationToken)
        => await _clients.DeleteAsync(externalId, cancellationToken) ? NoContent() : NotFound();

    [HttpGet("{externalId:guid}/uploads")]
    [ProducesResponseType(typeof(IReadOnlyList<ClientUploadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ClientUploadDto>>> GetUploadsForClient(
        Guid externalId,
        CancellationToken cancellationToken)
    {
        var uploads = await _clients.ListUploadsAsync(externalId, cancellationToken);
        return uploads is null ? NotFound() : Ok(uploads);
    }
}

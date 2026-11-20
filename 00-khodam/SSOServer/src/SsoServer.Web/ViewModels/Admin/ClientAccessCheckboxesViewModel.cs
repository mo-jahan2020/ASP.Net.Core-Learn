namespace SsoServer.Web.ViewModels.Admin;

/// <summary>Model of the _ClientAccessCheckboxes partial (user create/edit forms).</summary>
public sealed class ClientAccessCheckboxesViewModel
{
    public required IReadOnlyList<SsoServer.Domain.Models.ClientDto> Clients { get; init; }

    /// <summary>Client ids that must be ticked; empty = no restriction.</summary>
    public required IReadOnlyList<string> Selected { get; init; }
}

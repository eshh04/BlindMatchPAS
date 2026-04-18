// ============================================================================
// File: Models/ErrorViewModel.cs
// Purpose: View model for the shared error page, displaying request identifiers.
// ============================================================================

namespace BlindMatchPAS.Models;

/// <summary>
/// View model used by Views/Shared/Error.cshtml to display error request details.
/// </summary>
public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}

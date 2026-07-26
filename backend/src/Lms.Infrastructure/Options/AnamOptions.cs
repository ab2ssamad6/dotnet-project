namespace Lms.Infrastructure.Options;

/// <summary>Configuration for the Anam.ai AI-trainer provider.</summary>
public class AnamOptions
{
    public const string SectionName = "AiTrainer:Anam";

    public string BaseUrl { get; set; } = "https://api.anam.ai";
    public string ApiKey { get; set; } = string.Empty;

    // Default persona used when opening a session.
    public string PersonaName { get; set; } = "Cara";
    public string AvatarId { get; set; } = string.Empty;
    public string AvatarModel { get; set; } = "cara-4";
    public string VoiceId { get; set; } = string.Empty;
    public string LlmId { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } =
        "You are Cara, a helpful AI training assistant. Be friendly, concise, and helpful.";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}

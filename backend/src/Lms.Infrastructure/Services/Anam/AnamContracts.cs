using System.Text.Json.Serialization;

namespace Lms.Infrastructure.Services.Anam;

internal record AnamSessionTokenRequest([property: JsonPropertyName("personaConfig")] AnamPersonaConfig PersonaConfig);

internal record AnamPersonaConfig(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("avatarId")] string AvatarId,
    [property: JsonPropertyName("avatarModel")] string AvatarModel,
    [property: JsonPropertyName("voiceId")] string VoiceId,
    [property: JsonPropertyName("llmId")] string LlmId,
    [property: JsonPropertyName("systemPrompt")] string SystemPrompt);

internal record AnamSessionTokenResponse([property: JsonPropertyName("sessionToken")] string? SessionToken);

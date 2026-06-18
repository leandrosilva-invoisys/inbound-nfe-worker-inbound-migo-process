namespace Plugin.InboundMigoProcess.Application.DTO;

public sealed class NfeEntradaEventoVinculoRequestDto
{
    public int Id { get; init; }
    public string Descricao { get; init; } = string.Empty;
}

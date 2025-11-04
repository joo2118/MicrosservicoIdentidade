namespace Identidade.Infraestrutura.Interfaces
{
    public interface IEnvironmentAdapter
    {
        int ProcessId { get; }
        string CommandLine { get; }
    }
}

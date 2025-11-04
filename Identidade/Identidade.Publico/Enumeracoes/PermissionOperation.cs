namespace Identidade.Publico.Enumerations
{
    public enum PermissionOperation
    {
        Read = 1,
        Alter = 2,
        Append = 4,
        Delete = 8,
        Simulate = 16,
        Settle = 32,
        Transfer = 64,
        Workflow = 128,
        All = 255
    }
}

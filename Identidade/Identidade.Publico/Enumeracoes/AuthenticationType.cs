using Identidade.Publico.Compatibility;

namespace Identidade.Publico.Enumerations
{
    public enum AuthenticationType
    {
        [CompatibilityStringValue("databaseuser")]
        DatabaseUser = 0,
        [CompatibilityStringValue("activedirectory")]
        ActiveDirectory = 1,
        [CompatibilityStringValue("azureactivedirectory")]
        AzureAD = 4
    }
}

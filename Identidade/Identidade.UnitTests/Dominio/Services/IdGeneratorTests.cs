using Xunit;
using Identidade.Dominio.Servicos;

namespace Identidade.UnitTests.Domain.Services
{
    public class IdGeneratorTests
    {
        [Fact]
        public void GenerateId_WithSuggestedIdStartingWithPrefix_ReturnsSuggestedId()
        {
            var idGenerator = new IdGenerator();
            var prefix = "USR";
            var suggestedId = "USR_12345";

            var result = idGenerator.GenerateId(prefix, suggestedId);

            Assert.Equal(suggestedId, result);
        }

        [Fact]
        public void GenerateId_WithSuggestedIdNotStartingWithPrefix_ReturnsGeneratedId()
        {
            var idGenerator = new IdGenerator();
            var prefix = "USR";
            var suggestedId = "12345";

            var result = idGenerator.GenerateId(prefix, suggestedId);

            Assert.StartsWith(prefix + "_", result);
            Assert.NotEqual(suggestedId, result);
        }

        [Fact]
        public void GenerateId_WithNullSuggestedId_ReturnsGeneratedId()
        {
            var idGenerator = new IdGenerator();
            var prefix = "USR";

            var result = idGenerator.GenerateId(prefix);

            Assert.StartsWith(prefix + "_", result);
        }
    }
}

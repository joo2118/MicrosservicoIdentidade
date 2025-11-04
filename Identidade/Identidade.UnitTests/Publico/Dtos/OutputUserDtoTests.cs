using Identidade.Publico.Dtos;
using System;
using Xunit;

namespace Identidade.UnitTests.Public.Dtos
{
    public class OutputUserDtoTests
    {
        [Fact]
        public void Constructor_ShouldSetPropertiesCorrectly()
        {
            var outputUserDto = new OutputUserDto
            {
                Id = "testId",
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            Assert.Equal("testId", outputUserDto.Id);
            Assert.True((DateTime.UtcNow - outputUserDto.CreatedAt).TotalSeconds < 1);
            Assert.True((DateTime.UtcNow - outputUserDto.LastUpdatedAt).TotalSeconds < 1);
        }
    }
}

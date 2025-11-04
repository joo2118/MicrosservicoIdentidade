using Identidade.Dominio.Modelos;
using System;
using Xunit;

namespace Identidade.UnitTests.Domain.Models
{
    public class ConstantsTests
    {
        [Fact]
        public void DataMaximaTest()
        {
            DateTime dateTimeExpected = new DateTime(2100, 12, 31);

            var dateMaxActual = Constants.DATA_MAXIMA();

            Assert.Equal(dateTimeExpected, dateMaxActual);
        }
    }
}

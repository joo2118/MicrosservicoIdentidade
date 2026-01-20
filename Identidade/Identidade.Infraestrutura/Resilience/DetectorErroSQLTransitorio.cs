using System;
using Microsoft.Data.SqlClient;

namespace Identidade.Infraestrutura.Resilience;

internal static class DetectorErroSQLTransitorio
{
    public static bool ErroTransient(SqlException ex)
    {
        foreach (SqlError error in ex.Errors)
        {
            switch (error.Number)
            {
                case -2:
                case 1205:
                case 233:
                case 4060:
                case 40197:
                case 40501:
                case 40613:
                case 10928:
                case 10929:
                case 10053:
                case 10054:
                case 10060:
                    return true;
            }
        }

        return false;
    }
}

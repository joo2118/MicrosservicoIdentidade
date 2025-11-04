using System;

namespace Identidade.Dominio.Modelos
{
    public static class Constants
    {
        public static class Token
        {
            public static class Scheme
            {
                public const string Bearer = "Bearer";
            }
            public static class Claim
            {
                public static class Type
                {
                    public const string ver = "ver";
                    public const string unique_name = "unique_name";
                    public const string preferred_username = "preferred_username";
                    public const string appname = "app_displayname";
                    public const string appid = "appid";
                }
                public static class Value
                {
                    public const string v1 = "1.0";
                }
            }
        }

        public const string cst_ClientId = "ClientId";
        public const string cst_HealthCheck_Queue = "HealthCheck.Queue";

        public const string cst_SchemaIdentity = "Identity";
        public const string cst_SpAtualizaItemDiretorio = "spAtualizaItemDiretorio @Prefixo, @Codigo, @Nome, @IDClasse, @Vencimento, @Content, @SomenteLeitura, @ID OUTPUT, @CodigoGerado OUTPUT, @FoiAlterado OUTPUT, @Erro OUTPUT";
        public const string cst_SpRemoveItemDiretorio = "spRemoveItemDiretorio @ID, @Codigo, @IDClasse, @FoiAlterado OUTPUT";

        public const string cst_Usr = "USR";
        public const string cst_Ugr = "UGR";
        public const string cst_Id = "@ID";
        public const string cst_CodigoGerado ="@CodigoGerado";
        public const string cst_FoiAlterado = "@FoiAlterado";
        public const string cst_Erro = "@Erro";
        public const string cst_Prefixo = "@Prefixo";
        public const string cst_Codigo = "@Codigo";
        public const string cst_Nome = "@Nome";
        public const string cst_IDClasse = "@IDClasse";
        public const string cst_Vencimento = "@Vencimento";
        public const string cst_Content = "@Content";
        public const string cst_SomenteLeitura = "@SomenteLeitura";
        public const string cst_Tests = "Tests";
        public const string cst_UnitTests = "UnitTests";
        public const string cst_yyyy_MM_dd = "yyyy-MM-dd";

        public const int cst_Usuario = 17;
        public const int cst_GrupoUsuario = 18;

        public static class Exception
        {
            public const string cst_UserCreationFailed = "Could not create User on the Database.";
            public const string cst_UserUpdateFailed= "Could not update User on the Database.";
            public const string cst_UserUpdateFailedNoChange = "Could not update User on the Database. No change on existing user.";
            public const string cst_NullUser = "The user can not be null";
            public const string cst_InvalidLogin = "Login cannot be null, empty or white-space";
        }

        public static class ArcXml
        {
            public const string user = "user";
            public const string informalname = "informalname";
            public const string password = "password";
            public const string passwordneverexpires = "passwordneverexpires";
            public const string userlogon = "userlogon";
            public const string date = "date";
            public const string authentication = "authentication";
            public const string language = "language";
            public const string email = "email";
            public const string usergroup = "usergroup";
            public const string usergroups = "usergroups";
            public const string code = "code";
            public const string passwordshistory = "passwordshistory";
        }

        public static DateTime DATA_MAXIMA()
        {
            return new DateTime(2100, 12, 31);
        }
    }
}

using System.Text.RegularExpressions;

namespace Identidade.Publico.Validators
{
    public static class EmailValidator
    {
        private static readonly Regex _regex;

        static EmailValidator()
        {
            _regex = new Regex(@"^([\w-\.+]+)@(([[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(]?)$", RegexOptions.IgnoreCase);
        }

        public static bool Validate(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return _regex.IsMatch(email);
        }
    }
}

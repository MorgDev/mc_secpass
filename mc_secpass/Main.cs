using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mc_secpass
{
    public class Main : BaseScript
    {
        public Main()
        {
            Exports.Add("hashPassword", new Func<string, string>(password => PasswordUtility.HashPassword(password)));
            Exports.Add("verifyPassword", new Func<string, string, bool>((guess, password) => PasswordUtility.VerifyPassword(guess, password)));
        }
    }
}

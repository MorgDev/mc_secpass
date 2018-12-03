using System;
using System.Security.Cryptography;

namespace mc_secpass
{
    static class PasswordUtility
    {
        /// <summary>
        /// The length of our salt byte array
        /// This is also used to separate out the salt during verification
        /// </summary>
        private const int SaltByteLength = 32;
        /// <summary>
        /// The length of our hashed password byte array
        /// This is also used to separate out the password during verification
        /// </summary>
        private const int DerivedKeyLength = 20;

        /// <summary>
        /// Hashes a given password
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns>A Base64 string containing the salt, hashed password, and iteration count</returns>
        public static string HashPassword(string password)
        {
            /// Get a random salt.
            var salt = GenerateRandomSalt();
            /// Get a number of iterations
            var iterationCount = GetIterationCount();
            /// Generate the hashed password.
            var hashValue = GenerateHashValue(password, salt, iterationCount);
            /// Build our iteration count byte array
            var iterationCountByteArray = BitConverter.GetBytes(iterationCount);
            /// Build our complete save value array
            var saveValue = new byte[SaltByteLength + DerivedKeyLength +
                iterationCountByteArray.Length];
            /// Copy the salt into the save value array
            Buffer.BlockCopy(salt, 0, saveValue, 0, SaltByteLength);
            /// Copy the hashed password into the save value array
            Buffer.BlockCopy(hashValue, 0, saveValue, SaltByteLength, DerivedKeyLength);
            /// Copy the iteration count into the save value array
            Buffer.BlockCopy(iterationCountByteArray, 0, saveValue,
                salt.Length + hashValue.Length, iterationCountByteArray.Length);
            /// Return our save value as a Base64 string
            return Convert.ToBase64String(saveValue);
        }

        /// <summary>
        /// Verifies a given password against a saved hash.
        /// </summary>
        /// <param name="pwdInput">The user's password guess</param>
        /// <param name="savedPwd">The hashed password</param>
        /// <returns>True if verified, false otherwise</returns>
        public static bool VerifyPassword(string pwdInput, string savedPwd)
        {
            /// Set up our salt array
            var salt = new byte[SaltByteLength];
            /// Set up our hashed password array
            var pwd = new byte[DerivedKeyLength];
            /// Set up our savedPwd as a byte array
            var savedBytes = Convert.FromBase64String(savedPwd);
            /// Calculate the number of bytes used for our iteration count
            var iterationCountLength = savedBytes.Length - (salt.Length + pwd.Length);
            /// Set up our iteration count array
            var iterationArr = new byte[iterationCountLength];
            /// Copy the salt to our salt array
            Buffer.BlockCopy(savedBytes, 0, salt, 0, SaltByteLength);
            /// Copy the hashed password to our hashed password array
            Buffer.BlockCopy(savedBytes, SaltByteLength, pwd, 0, pwd.Length);
            /// Copy the iteration count to our iteration count array
            Buffer.BlockCopy(savedBytes, salt.Length + pwd.Length, iterationArr, 0, 
                iterationCountLength);

            /// Hash the password guess using the same salt and iteration count as the stored value
            var pwdGuessArr = GenerateHashValue(pwdInput, salt, BitConverter.ToInt32(iterationArr, 0));

            /// compare the hashed guess to the saved hash
            /// Returns either true or false
            return ConstantTimeComparison(pwdGuessArr, pwd);
        }

        /// <summary>
        /// Generates a random salt
        /// </summary>
        /// <returns>The byte[] for the generated salt</returns>
        private static byte[] GenerateRandomSalt()
        {
            /// Set up our Cryptograpohically Secure prng
            var csprng = new RNGCryptoServiceProvider();
            /// Set up salt array
            var salt = new byte[SaltByteLength];
            /// Fill salt array
            csprng.GetBytes(salt);
            /// Return salt array
            return salt;
        }

        /// <summary>
        /// Calculates a random iteration count between 10000 and 50000
        /// </summary>
        /// <returns>The iteration count</returns>
        private static int GetIterationCount()
        {
            /// Set up our RNG
            Random rng = new Random();

            /// Choose our random iteration count
            return rng.Next(10000, 50001);
        }

        /// <summary>
        /// Generates a hashed password value
        /// </summary>
        /// <param name="password">The string password</param>
        /// <param name="salt">Tha salt to use</param>
        /// <param name="iterationCount">How many iterations to run</param>
        /// <returns>A byte[] hashed password</returns>
        private static byte[] GenerateHashValue(string password, byte[] salt, int iterationCount)
        {
            /// Set up our hashed password array
            byte[] hashValue;
            
            /// Ensure we don't pass a null value.
            /// You could also choose to catch this condition as an error. I didn't.
            var valueToHash = string.IsNullOrEmpty(password) ? string.Empty : password;

            /// Hash our password
            using (var pbkdf2 = new Rfc2898DeriveBytes(valueToHash, salt, iterationCount))
            {
                /// Fill our hashed password array
                hashValue = pbkdf2.GetBytes(DerivedKeyLength);
            }
            
            /// Return our hashed password array
            return hashValue;
        }

        /// <summary>
        /// Compares two byte arrays for equality.
        /// Rather than other options, this is slow by design
        /// which assists with security.
        /// </summary>
        /// <param name="passwordGuess">The user's guess</param>
        /// <param name="actualPassword">The proper password</param>
        /// <returns>True if match, false otherwise</returns>
        private static bool ConstantTimeComparison(byte[] passwordGuess, byte[] actualPassword)
        {
            /// Compares the lengths
            uint difference = (uint)passwordGuess.Length ^ (uint)actualPassword.Length;

            /// Compares the values of each array against each other AND against difference.
            /// If at any point this changes from 0 this check will fail
            for (var i = 0; i < passwordGuess.Length && i < actualPassword.Length; i++)
                difference |= (uint)(passwordGuess[i] ^ actualPassword[i]);

            /// Return true if difference is 0
            return difference == 0;
        }
    }
}
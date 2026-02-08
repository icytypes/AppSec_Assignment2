using System.Security.Cryptography;
using System.Text;

namespace _231046Y_Assignment2.Services
{
    public class TwoFactorService
    {
        public string GenerateSecret()
        {
            var bytes = new byte[20];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Base32Encode(bytes);
        }

        public bool VerifyCode(string secret, string code)
        {
            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(code))
                return false;

            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
            
            // Check current time window and adjacent windows
            for (int i = -1; i <= 1; i++)
            {
                var timeWindow = currentTime + i;
                var expectedCode = GenerateTOTP(secret, timeWindow);
                if (expectedCode == code)
                    return true;
            }

            return false;
        }

        private string GenerateTOTP(string secret, long timeWindow)
        {
            var key = Base32Decode(secret);
            var timeBytes = BitConverter.GetBytes(timeWindow);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(timeBytes);

            using (var hmac = new HMACSHA1(key))
            {
                var hash = hmac.ComputeHash(timeBytes);
                var offset = hash[hash.Length - 1] & 0x0F;
                var binary = ((hash[offset] & 0x7F) << 24) |
                            ((hash[offset + 1] & 0xFF) << 16) |
                            ((hash[offset + 2] & 0xFF) << 8) |
                            (hash[offset + 3] & 0xFF);

                var otp = binary % 1000000;
                return otp.ToString("D6");
            }
        }

        private string Base32Encode(byte[] bytes)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var output = new StringBuilder();
            for (int bitIndex = 0; bitIndex < bytes.Length * 8; bitIndex += 5)
            {
                int dualByte = bytes[bitIndex / 8] << 8;
                if (bitIndex / 8 + 1 < bytes.Length)
                    dualByte |= bytes[bitIndex / 8 + 1];
                dualByte = 0x1F & (dualByte >> (16 - bitIndex % 8 - 5));
                output.Append(alphabet[dualByte]);
            }
            return output.ToString();
        }

        private byte[] Base32Decode(string input)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var output = new List<byte>();
            char[] inputArray = input.ToUpper().ToCharArray();

            for (int bitIndex = 0; bitIndex < inputArray.Length * 5; bitIndex += 8)
            {
                int dualByte = alphabet.IndexOf(inputArray[bitIndex / 5]) << 10;
                if (bitIndex / 5 + 1 < inputArray.Length)
                    dualByte |= alphabet.IndexOf(inputArray[bitIndex / 5 + 1]) << 5;
                if (bitIndex / 5 + 2 < inputArray.Length)
                    dualByte |= alphabet.IndexOf(inputArray[bitIndex / 5 + 2]);

                dualByte = 0xFF & (dualByte >> (15 - bitIndex % 5 - 8));
                output.Add((byte)dualByte);
            }
            return output.ToArray();
        }
    }
}

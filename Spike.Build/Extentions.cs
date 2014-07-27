using System.IO;
using System.Reflection;
using System.Text;

namespace Spike.Build
{
    internal static class Extentions
    {
        internal static string CamelCase(this string text)
        {
            if (text != null && text.Length > 0 && char.IsUpper(text[0]))
            {
                var array = text.ToCharArray();
                array[0] = char.ToLower(array[0]);
                return new string(array);
            }
            return text;
        }

        internal static string PascalCase(this string text)
        {
            if (text != null && text.Length > 0 && char.IsLower(text[0]))
            {
                var array = text.ToCharArray();
                array[0] = char.ToUpper(array[0]);
                return new string(array);
            }
            return text;
        }




        internal static void CopyFromRessources(string source, string destination) {
            using (var sourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(source))
            using (var destinationStream = File.OpenWrite(destination))
                sourceStream.CopyTo(destinationStream);
        }
    }

    internal static class KeyExtensions
    {
        private const uint Seed = 37;

        /// <summary>
        /// Computes MurmurHash3 on this set of bytes and returns the calculated hash value.
        /// </summary>
        /// <param name="data">The data to compute the hash of.</param>
        /// <returns>A 32bit hash value.</returns>
        public static uint GetMurmurHash3(this string signature)
        {
            byte[] data = Encoding.UTF8.GetBytes(signature);

            const uint c1 = 0xcc9e2d51;
            const uint c2 = 0x1b873593;


            int curLength = data.Length; /* Current position in byte array */
            int length = curLength; /* the const length we need to fix tail */
            uint h1 = Seed;
            uint k1 = 0;

            /* body, eat stream a 32-bit int at a time */
            int currentIndex = 0;
            while (curLength >= 4)
            {
                /* Get four bytes from the input into an UInt32 */
                k1 = (uint)(data[currentIndex++]
                  | data[currentIndex++] << 8
                  | data[currentIndex++] << 16
                  | data[currentIndex++] << 24);

                /* bitmagic hash */
                k1 *= c1;
                k1 = Rotl32(k1, 15);
                k1 *= c2;

                h1 ^= k1;
                h1 = Rotl32(h1, 13);
                h1 = h1 * 5 + 0xe6546b64;
                curLength -= 4;
            }

            /* tail, the reminder bytes that did not make it to a full int */
            /* (this switch is slightly more ugly than the C++ implementation
            * because we can't fall through) */
            switch (curLength)
            {
                case 3:
                    k1 = (uint)(data[currentIndex++]
                      | data[currentIndex++] << 8
                      | data[currentIndex++] << 16);
                    k1 *= c1;
                    k1 = Rotl32(k1, 15);
                    k1 *= c2;
                    h1 ^= k1;
                    break;
                case 2:
                    k1 = (uint)(data[currentIndex++]
                      | data[currentIndex++] << 8);
                    k1 *= c1;
                    k1 = Rotl32(k1, 15);
                    k1 *= c2;
                    h1 ^= k1;
                    break;
                case 1:
                    k1 = data[currentIndex++];
                    k1 *= c1;
                    k1 = Rotl32(k1, 15);
                    k1 *= c2;
                    h1 ^= k1;
                    break;
            };

            // finalization, magic chants to wrap it all up
            h1 ^= (uint)length;
            h1 = Mix(h1);

            return h1;
            // convert back to 4 bytes
            /*byte[] key = new byte[4];
            key[0] = (byte)(h1);
            key[1] = (byte)(h1 >> 8);
            key[2] = (byte)(h1 >> 16);
            key[3] = (byte)(h1 >> 24);
            return key;*/
        }

        private static uint Rotl32(uint x, byte r)
        {
            return (x << r) | (x >> (32 - r));
        }

        private static uint Mix(uint h)
        {
            h ^= h >> 16;
            h *= 0x85ebca6b;
            h ^= h >> 13;
            h *= 0xc2b2ae35;
            h ^= h >> 16;
            return h;
        }

    }

}

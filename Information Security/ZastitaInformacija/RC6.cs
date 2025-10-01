using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ZastitaInformacija
{
    public class RC6
    {
        private static byte[] Key = { 0x12, 0x34, 0x56, 0x78, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67, 0x89, 0xFE, 0xDC, 0xBA, 0x98 };
        public static string Encrypt( string plaintext)
        {
            // Generisanje podključeva
            uint[] S = GenerateSubkeys(Key, 32, 20);

            // Inicijalizacija registara
            uint A = BitConverter.ToUInt32(Key, 0);
            uint B = BitConverter.ToUInt32(Key, 4);
            uint C = BitConverter.ToUInt32(Key, 8);
            uint D = BitConverter.ToUInt32(Key, 12);

            A += S[0];
            B += S[1];

            // Konvertovanje teksta u bajtove
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

            // Enkripcija bloka teksta
            for (int i = 0; i < plaintextBytes.Length; i += 16)
            {
                A ^= BitConverter.ToUInt32(plaintextBytes, i);
                B ^= BitConverter.ToUInt32(plaintextBytes, i + 4);
                C ^= BitConverter.ToUInt32(plaintextBytes, i + 8);
                D ^= BitConverter.ToUInt32(plaintextBytes, i + 12);

                EncryptBlock(S, ref A, ref B, ref C, ref D);
            }

            // Konvertovanje enkriptovanih podataka u heksadecimalni string
            return string.Join("", new uint[] { A, B, C, D }.Select(value => value.ToString("X8")));
        }

        private static void EncryptBlock(uint[] S, ref uint A, ref uint B, ref uint C, ref uint D)
        {
            int w = 32;
            int r = 20;

            for (int i = 1; i <= r; i++)
            {
                A = unchecked(A + S[2 * i - 1]);
                B = unchecked(B + S[2 * i]);
                A = RotateLeft((A ^ B), (int)B) + S[2 * i - 1];
                C = RotateLeft((C ^ D), (int)D) + S[2 * i];
                Swap(ref A, ref C);
            }
        }
        private static void Swap(ref uint a, ref uint b)
        {
            uint temp = a;
            a = b;
            b = temp;
        }
        public static uint RotateLeft(uint value, int shift)
        {
            shift &= 31;  // Ograničavamo shift na opseg [0, 31]
            return ((value << shift) | (value >> (32 - shift)));
        }

        public static void Rotate(ref uint A, ref uint B, ref uint C, ref uint D)
        {
            uint temp = A;
            A = B;
            B = C;
            C = D;
            D = temp;
        }

        public static uint[] GenerateSubkeys(byte[] key, int w, int r)
        {
            int b = key.Length; // Broj bajtova u ključu
            int c = (b + 3) / 4; // Broj reči u ključu (gde je svaka reč od 4 bajta)

            uint[] L = new uint[c]; // Reči u ključu
            for (int k = 0; k < b; k++)
            {
                L[k / 4] = (L[k / 4] << 8) + key[k];
            }

            uint P = 0xb7e15163;
            uint Q = 0x9e3779b9;

            int t, u;
            int veci_od = Math.Max(c, 2 * r + 4);
            uint[] S = new uint[veci_od];

            S[0] = P;
            for (int p = 1; p < 2 * r + 4; p++)
            {
                S[p] = S[p - 1] + Q;
            }

            uint A = 0, B = 0;
            int i = 0, j = 0;
            for (int s = 1; s <= 3 * veci_od; s++)
            {
                A = S[i] = (uint)RotateLeft((S[i] + A + B), 3);
                B = L[j] = (uint)RotateLeft((L[j] + A + B), (int)(A + B));
                i = (i + 1) % (2 * r + 4);
                j = (j + 1) % c;
            }

            return S;
        }

        public static byte[] GenerateRandomKey(int size)
        {
            byte[] key = new byte[size];
            new Random().NextBytes(key);
            return key;
        }

        public static string Decrypt(string encryptedHexString)
        {
            // Konvertovanje enkriptovanog stringa u niz uint vrednosti
            uint[] encryptedData = ConvertHexStringToUIntArray(encryptedHexString);

            // Generisanje podključeva
            uint[] S = GenerateSubkeys(Key, 32, 20);

            // Inicijalizacija registara
            uint A = BitConverter.ToUInt32(Key, 0);
            uint B = BitConverter.ToUInt32(Key, 4);
            uint C = BitConverter.ToUInt32(Key, 8);
            uint D = BitConverter.ToUInt32(Key, 12);

            A += S[0];
            B += S[1];

            // Dekripcija bloka teksta
            for (int i = 0; i < encryptedData.Length; i += 4)
            {
                DecryptBlock(S, ref A, ref B, ref C, ref D);

                A ^= encryptedData[i];
                B ^= encryptedData[i + 1];
                C ^= encryptedData[i + 2];
                D ^= encryptedData[i + 3];
            }

            // Konvertovanje dekriptovanih podataka u string
            byte[] decryptedBytes = ConvertUIntArrayToBytes(new uint[] { A, B, C, D });
            return Encoding.UTF8.GetString(decryptedBytes);
        }


        private static void DecryptBlock(uint[] S, ref uint A, ref uint B, ref uint C, ref uint D)
        {
            int w = 32;
            int r = 20;

            for (int i = r; i >= 1; i--)
            {
                Swap(ref A, ref C);
                C = RotateRight((C - S[2 * i - 1]), (int)D) ^ D;
                A = RotateRight((A - S[2 * i]), (int)B) ^ B;
                B = unchecked(B - S[2 * i]);
                A = unchecked(A - S[2 * i - 1]);
            }

            C = unchecked(C - S[0]);
            A = unchecked(A - S[1]);
        }
        private static uint RotateRight(uint value, int shift)
{
    shift &= 31;
    return (value >> shift) | (value << (32 - shift));
}

        // funkcija za obrnuto rotiranje
        public static void ReverseRotate(ref uint A, ref uint B, ref uint C, ref uint D)
        {
            uint temp = D;
            D = C;
            C = B;
            B = A;
            A = temp;
        }
        public static byte[] ConvertUIntArrayToBytes(uint[] array)
        {
            byte[] result = new byte[array.Length * 4];
            for (int i = 0, off = 0; i < array.Length; i++)
            {
                result[off++] = (byte)(array[i] & 0xFF);
                result[off++] = (byte)((array[i] >> 8) & 0xFF);
                result[off++] = (byte)((array[i] >> 16) & 0xFF);
                result[off++] = (byte)((array[i] >> 24) & 0xFF);
            }
            return result;
        }
        public static uint[] ConvertHexStringToUIntArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("Hex string must have an even number of characters.");
            }

            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            uint[] result = new uint[bytes.Length / 4];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = BitConverter.ToUInt32(bytes, i * 4);
            }

            return result;
        }


    }
}

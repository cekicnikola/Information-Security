using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZastitaInformacija
{
    class BifidCipher
    {
        private static string DefaultAlphabet = "abcdefghiklmnopqrstuvwxyz";


        public char[,] Polybus;

        public BifidCipher()
        {
            Polybus = new char[5,5] { { 'a', 'b', 'c', 'd', 'e' },{ 'f', 'g', 'h', 'i', 'k' },
                { 'l','m','n','o','p'},{'q','r','s','t','u' },{'v','w','x','y','z' } };
        }

        //kod za validaciju i kreiranje kljuca


        //vraca koordinate svakog slova u zapisu (vrsta,kolona)
        private Tuple<int,int> GetRowColumn(char Letter)
        {
            if(Letter=='j')
                Letter='i';
            for (int i = 0; i < Polybus.GetLength(0); i++)
            {
                for (int k = 0; k < Polybus.GetLength(1) ; k++)
                {
                    if (Polybus[i,k]==Letter)
                    {
                        return Tuple.Create(i, k);
                    }
                }
            }
            return Tuple.Create(0, 0);
        }

        public string EncryptMessage(string Message)
        {
            //izbacivanje specijalnih karaktera 
            string MessageToEncrypt = CleanString(Message);
            MessageToEncrypt = MessageToEncrypt.ToLower();

            int[] rows = new int[MessageToEncrypt.Length]; 
            int[] columns = new int[MessageToEncrypt.Length];

            for (int i = 0; i < MessageToEncrypt.Length; i++)
            {
                var coords = GetRowColumn(MessageToEncrypt[i]);
                rows[i] = coords.Item1;
                columns[i] = coords.Item2;
            }

            string Ycoords = string.Join(string.Empty, columns);
            string Xcoords = string.Join(string.Empty, rows);

            string ArrayOfCoordinates = Xcoords + Ycoords;

            int count = 0;
            StringBuilder output = new StringBuilder();

            while (count < ArrayOfCoordinates.Length)
            {
                int row = (int)char.GetNumericValue(ArrayOfCoordinates[count]);
                int col = (int)char.GetNumericValue(ArrayOfCoordinates[count + 1]);

                output.Append(Polybus[row, col]);

                count += 2;
            }

            return output.ToString();
        }

        public string Decrypt(string Message)
        {
            string MessageToEncrypt = Message.ToLower();
            int[] rowsColumns = new int[MessageToEncrypt.Length * 2];
            StringBuilder RowsColumns = new StringBuilder();
            for (int i = 0; i < MessageToEncrypt.Length; i++)
            {
                var coords = GetRowColumn(MessageToEncrypt[i]);
                RowsColumns.Append(coords.Item1);
                RowsColumns.Append(coords.Item2);

            }

            //vracanje prvobitniih vrsti i kolona
            string Coodrinates = RowsColumns.ToString();
            string FinalRows = Coodrinates.Substring(0, (int)(Coodrinates.Length / 2));
            string FinalColumns = Coodrinates.Substring((int)(Coodrinates.Length / 2), (int)(Coodrinates.Length / 2));

            //ocitavanje slova na osnovu dekriptovanog para [vrsta][kolona]
            int count = 0;
            StringBuilder output = new StringBuilder();
            while(count<FinalRows.Length)
            {
                int row = (int)char.GetNumericValue(FinalRows[count]);
                int column = (int)char.GetNumericValue(FinalColumns[count]);

                output.Append(Polybus[row,column]);
                count++;
            }

            return output.ToString();

        }

        public string CleanString(string source)
        {
            StringBuilder Cleaner = new StringBuilder();
            for (int i = 0; i <source.Length; i++)
            {
                if (source[i] >= 'A' && source[i] <= 'z')
                {
                    Cleaner.Append(source[i]);
                }
            }
            return Cleaner.ToString();
        }
        public void GeneratePolybus(string key)
        {
            string defaultWithoutRepeating = DefaultAlphabet;
            key = key.ToLower();

            for (int i = 0; i < key.Length; i++)
            {
                if (defaultWithoutRepeating.Contains(key[i]))
                {
                    defaultWithoutRepeating = defaultWithoutRepeating.Replace(key[i].ToString(), "");

                }
            }
            key += defaultWithoutRepeating;

            char[,] output = new char[5, 5];
            for (int i = 0; i < 25; i++)
            {
                
                output[i/5,i%5]=(char) key[i];
                

            }
            Polybus = output;
        }
        public string GetPolybusToString()
        {
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    str.Append(Polybus[i, j]);
                }
            }
            return str.ToString();
        }


    }
    
}

using System;

namespace IdentityPermissionExtension
{
    public class ByteEncoder
    {
        private byte[] _bytes { get; set; }

        public ByteEncoder(byte[] bytes)
        {
            this._bytes = bytes;
        }
        public ByteEncoder(string text)
        {
            this._bytes = System.Text.Encoding.UTF8.GetBytes(text);
        }

        public long ToLong()
        {
            long value = 0;
            var c = (uint)(this._bytes.Length % 4);
            this._bytes = c > 0 ? AddEmptyCells(this._bytes, 4 - c) : this._bytes;
            c = (uint)(this._bytes.Length / 4);
            for (int i = 0; i < c; i++)
            {
                var converted = BitConverter.ToInt32(this._bytes, i * 4);
                long temp = 0;
                bool isInScope = long.TryParse((converted + value).ToString(), out temp);
                if (isInScope)
                    value = temp;
                else
                    value = (value / 2) + converted;
            }
            return value;
        }

        private byte[] AddEmptyCells(byte[] array, uint no)
        {
            var newArray = new byte[array.Length + no];
            array.CopyTo(newArray, 0);
            return newArray;
        }
    }
}

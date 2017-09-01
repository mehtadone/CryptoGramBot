using System;
using CsvHelper.TypeConversion;

namespace CryptoGramBot.Helpers
{
    public class CsvEnumConverter<T> : EnumConverter where T : struct
    {
        public CsvEnumConverter() : base(typeof(T))
        { }

        public override object ConvertFromString(TypeConverterOptions options, string text)
        {
            if (int.TryParse(text, out int parsedValue))
            {
                return (T)(object)parsedValue;
            }
            return base.ConvertFromString(options, text);
        }

        public override string ConvertToString(TypeConverterOptions options, object value)
        {
            return Enum.TryParse(value.ToString(), out T result) ? Convert.ToInt32(result).ToString() : base.ConvertToString(options, value);
        }
    }
}
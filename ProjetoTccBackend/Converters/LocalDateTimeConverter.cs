using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjetoTccBackend.Converters
{
    /// <summary>
    /// Conversor customizado para sempre trabalhar com UTC
    /// </summary>
    public class LocalDateTimeConverter : JsonConverter<DateTime>
    {
        /// <summary>
        /// Lê um valor DateTime do JSON, sempre interpretando como UTC
        /// </summary>
        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            string? dateString = reader.GetString();
            
            if (string.IsNullOrEmpty(dateString))
            {
                return default;
            }

            // Parse respeitando o sufixo Z (UTC) - usa DateTimeStyles.RoundtripKind
            if (DateTime.TryParse(
                dateString, 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.RoundtripKind, 
                out DateTime result))
            {
                // Se parseou com Kind=Utc, retorna como está
                if (result.Kind == DateTimeKind.Utc)
                {
                    return result;
                }
                
                // Se parseou como Local ou Unspecified, força como UTC
                return DateTime.SpecifyKind(result, DateTimeKind.Utc);
            }

            return default;
        }

        /// <summary>
        /// Escreve um valor DateTime para JSON em formato UTC ISO 8601
        /// </summary>
        public override void Write(
            Utf8JsonWriter writer,
            DateTime value,
            JsonSerializerOptions options
        )
        {
            // Como o ValueConverter do EF já garante que é UTC, apenas serializa
            // NUNCA chamar ToUniversalTime() aqui - isso causaria conversão dupla!
            var serialized = value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            Console.WriteLine($"🔍 LocalDateTimeConverter.Write: {value} (Kind={value.Kind}) → {serialized}");
            writer.WriteStringValue(serialized);
        }
    }
}

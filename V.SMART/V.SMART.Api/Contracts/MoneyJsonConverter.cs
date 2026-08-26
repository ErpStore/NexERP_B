using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace V.SMART.Api.Contracts
{
    /// <summary>
    /// Serializes a money-typed <c>decimal</c> (or <c>decimal?</c>) as a JSON <b>string</b>
    /// instead of a JSON number — <b>Q-85</b>'s decision, recorded in
    /// <c>docs/kb/api/controller-conventions.md</c> §8a.
    ///
    /// <para><b>The problem this exists to stop.</b> <c>System.Text.Json</c> writes a
    /// <c>decimal</c> as JSON number *text* with full precision — the wire is exact. But every
    /// JSON parser a browser has, including the one built into every <c>fetch</c>/
    /// <c>HttpClient</c> call, reads a JSON number into an IEEE-754 double, and that conversion
    /// is lossy for values with more significant digits than a double can hold. The precision is
    /// gone the moment <c>JSON.parse</c> runs, before any TypeScript or <c>decimal.js</c> code
    /// ever sees the value — see <b>Q-85</b> in <c>open-questions.md</c> for the full
    /// measurement. Serializing as a string sidesteps native number parsing entirely; the
    /// client reads the exact text and decides how to parse it.</para>
    ///
    /// <para><b>Opt-in, per property, not global.</b> Apply
    /// <c>[JsonConverter(typeof(MoneyJsonConverter))]</c> to a specific money-typed property.
    /// This deliberately does <b>not</b> touch every <c>decimal</c> in the domain — GST rates
    /// and quantities are <c>decimal</c> too and are not money; forcing every one of them
    /// through the same string encoding was considered and rejected (Q-85's option (c) covers
    /// the alternative). Which fields count as money is a per-controller judgement, not a type
    /// inference.</para>
    ///
    /// <para>A <see cref="JsonConverterFactory"/> rather than a plain
    /// <see cref="JsonConverter{T}"/> because attribute-driven converter resolution on a
    /// <c>decimal?</c> property does not automatically unwrap to the non-nullable converter in
    /// <c>System.Text.Json</c> — the factory supplies the right converter for whichever of the
    /// two the property actually is.</para>
    /// </summary>
    public sealed class MoneyJsonConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert == typeof(decimal) || typeToConvert == typeof(decimal?);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            typeToConvert == typeof(decimal)
                ? new DecimalAsStringConverter()
                : new NullableDecimalAsStringConverter();

        private sealed class DecimalAsStringConverter : JsonConverter<decimal>
        {
            public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                // A bare JSON number is still accepted on read — a client that has not adopted
                // the string convention yet (or a hand-written test payload) still deserializes
                // correctly. Only the write side is where precision would otherwise be lost, and
                // this server never emits a number for a money field once annotated.
                if (reader.TokenType == JsonTokenType.Number)
                {
                    return reader.GetDecimal();
                }

                var text = reader.GetString();
                if (text is null || !decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                {
                    throw new JsonException($"'{text}' is not a valid money value.");
                }

                return value;
            }

            public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) =>
                writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
        }

        private sealed class NullableDecimalAsStringConverter : JsonConverter<decimal?>
        {
            public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                {
                    return null;
                }

                if (reader.TokenType == JsonTokenType.Number)
                {
                    return reader.GetDecimal();
                }

                var text = reader.GetString();
                if (text is null)
                {
                    return null;
                }

                if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                {
                    throw new JsonException($"'{text}' is not a valid money value.");
                }

                return value;
            }

            public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
            {
                if (value.HasValue)
                {
                    writer.WriteStringValue(value.Value.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    writer.WriteNullValue();
                }
            }
        }
    }
}

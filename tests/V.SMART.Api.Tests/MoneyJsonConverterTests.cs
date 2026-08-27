using System.Text.Json;
using System.Text.Json.Serialization;
using V.SMART.Api.Contracts;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// <b>Q-85</b> — money crosses the wire as a JSON string, not a JSON number, because a
    /// JSON number is read by the browser into an IEEE-754 double and loses precision at
    /// <c>JSON.parse</c>, before any client-side code runs. These tests prove the converter
    /// actually delivers that: a value with more significant digits than a double can hold
    /// round-trips exactly, which a plain <c>decimal</c> serialized as a JSON number would not
    /// once it reached a JavaScript client.
    /// </summary>
    public class MoneyJsonConverterTests
    {
        private sealed class MoneyHolder
        {
            [JsonConverter(typeof(MoneyJsonConverter))]
            public decimal Amount { get; init; }
        }

        private sealed class NullableMoneyHolder
        {
            [JsonConverter(typeof(MoneyJsonConverter))]
            public decimal? Amount { get; init; }
        }

        // A double has ~15-17 significant decimal digits. This value has 20 — well past what a
        // double can represent exactly, and inside decimal's own 28-29 digit range.
        private const string PrecisionBeyondDoubleText = "79228162514264337.5935";

        [Fact]
        public void Write_EmitsAJsonString_NotANumber()
        {
            var json = JsonSerializer.Serialize(new MoneyHolder { Amount = 1234.5m });

            Assert.Equal("{\"Amount\":\"1234.5\"}", json);
        }

        [Fact]
        public void RoundTrip_PreservesPrecisionBeyondWhatADoubleCanHold()
        {
            var original = decimal.Parse(PrecisionBeyondDoubleText, System.Globalization.CultureInfo.InvariantCulture);

            var json = JsonSerializer.Serialize(new MoneyHolder { Amount = original });
            var roundTripped = JsonSerializer.Deserialize<MoneyHolder>(json);

            Assert.NotNull(roundTripped);
            Assert.Equal(original, roundTripped!.Amount);

            // The point of the whole exercise, made explicit: if this value had crossed as a
            // JSON *number* instead, parsing it as a double and back would NOT reproduce the
            // original — proving the failure mode Q-85 exists to avoid, not just asserting the
            // fix in isolation.
            var asDoubleRoundTrip = (decimal)(double)original;
            Assert.NotEqual(original, asDoubleRoundTrip);
        }

        [Fact]
        public void Read_AlsoAcceptsABareJsonNumber_ForBackwardCompatibility()
        {
            // A hand-written payload or a client that has not adopted the string convention yet
            // must not be rejected outright — only the server's own writes are guaranteed exact.
            var holder = JsonSerializer.Deserialize<MoneyHolder>("{\"Amount\":1234.5}");

            Assert.NotNull(holder);
            Assert.Equal(1234.5m, holder!.Amount);
        }

        [Fact]
        public void NullableVariant_WritesNull_WhenValueIsAbsent()
        {
            var json = JsonSerializer.Serialize(new NullableMoneyHolder { Amount = null });

            Assert.Equal("{\"Amount\":null}", json);
        }

        [Fact]
        public void NullableVariant_RoundTripsAValue()
        {
            var json = JsonSerializer.Serialize(new NullableMoneyHolder { Amount = 99.99m });
            var roundTripped = JsonSerializer.Deserialize<NullableMoneyHolder>(json);

            Assert.Equal("{\"Amount\":\"99.99\"}", json);
            Assert.Equal(99.99m, roundTripped!.Amount);
        }

        [Fact]
        public void Read_ThrowsJsonException_OnAnUnparsableString()
        {
            Assert.Throws<JsonException>(() =>
                JsonSerializer.Deserialize<MoneyHolder>("{\"Amount\":\"not a number\"}"));
        }
    }
}

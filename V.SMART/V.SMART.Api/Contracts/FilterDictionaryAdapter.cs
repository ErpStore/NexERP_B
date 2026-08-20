using System.Globalization;

namespace V.SMART.Api.Contracts
{
    /// <summary>
    /// Turns a typed query DTO into the <c>Dictionary&lt;string, object&gt;</c> that
    /// <c>SearchWithDynamicFilterAsync</c> takes — <b>without changing any business service</b>.
    ///
    /// <para>ADR-002 §Consequences authorises exactly this: <i>"Some service signatures fit REST
    /// awkwardly (e.g. dynamic-filter dictionaries); those get a typed DTO at the controller and
    /// an adapter into the existing service, without changing the service."</i> The dictionary
    /// exists only between this class and the service; it never reaches the HTTP surface.</para>
    ///
    /// <para><b>One explicit method per resource, never reflection.</b> A reflection-driven mapper
    /// binds to property names invisibly: rename a property and the filter stops being applied,
    /// silently, because every <c>*FilterBuilder.ApplyFilter</c> ends in <c>_ =&gt; query</c>
    /// (<c>CurrencyService.cs:206</c>) and discards keys it does not recognise. An explicit method
    /// makes that coupling a compile-time fact and a reviewable diff.</para>
    ///
    /// <para><b>Dictionary keys are the filter-builder's own <c>field switch</c> labels</b>, which
    /// are PascalCase entity property names, not the camel-case wire names. Getting a key wrong
    /// here fails silently — that is why the mapping is written out one line at a time and each
    /// key is quoted against its <c>file:line</c>.</para>
    /// </summary>
    public static class FilterDictionaryAdapter
    {
        /// <summary>
        /// The date format handed to the filter builder. The builder stringifies the value
        /// (<c>CurrencyService.cs:189</c>) and re-parses it with <c>DateTime.TryParse</c>
        /// (<c>:178,180</c>), both of which use the server's current culture. Formatting an
        /// unambiguous ISO date here removes that culture dependency. Only the date part is
        /// carried because both predicates use <c>.Date</c> — so nothing is lost.
        /// </summary>
        public const string FilterDateFormat = "yyyy-MM-dd";

        /// <summary>
        /// Maps <see cref="CurrencyQuery"/> onto the keys <c>CurrencyFilterBuilder</c> understands
        /// (<c>CurrencyService.cs:193-207</c>). Returns <c>null</c> when nothing is filtered, which
        /// is the shape the pre-M2-B02 controller passed and the service's own
        /// <c>filters != null &amp;&amp; filters.Any()</c> guard (<c>:45</c>) expects.
        /// </summary>
        public static Dictionary<string, object>? ForCurrency(CurrencyQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var filters = new Dictionary<string, object>();

            // Whitespace-only values are dropped here as well as in the builder (:164-168), so the
            // dictionary the service sees carries only filters that actually narrow the query.
            if (!string.IsNullOrWhiteSpace(query.CurrName))
                filters["CurrName"] = query.CurrName;

            if (!string.IsNullOrWhiteSpace(query.CreatedBy))
                filters["CreatedBy"] = query.CreatedBy;

            if (query.FromDate.HasValue)
                filters["FromDate"] = Format(query.FromDate.Value);

            if (query.ToDate.HasValue)
                filters["ToDate"] = Format(query.ToDate.Value);

            return filters.Count > 0 ? filters : null;
        }

        private static string Format(DateTime value)
            => value.ToString(FilterDateFormat, CultureInfo.InvariantCulture);
    }
}

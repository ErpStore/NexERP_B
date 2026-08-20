using System.Globalization;

namespace V.SMART.Api.Contracts
{
    /// <summary>One parsed sort term: a wire field name and its direction.</summary>
    public sealed record SortTerm(string Field, bool Descending)
    {
        /// <summary>The canonical wire form of this term (<c>field</c> or <c>-field</c>).</summary>
        public override string ToString() => Descending ? "-" + Field : Field;
    }

    /// <summary>
    /// The API-wide <c>sort</c> parser and allow-list validator (M2-B02, ADR-002 §2 addendum).
    ///
    /// <para><b>Syntax.</b> A comma-separated list of field names, each optionally prefixed with
    /// <c>-</c> for descending: <c>sort=-createdDate,currName</c>. One parameter, survives URL
    /// encoding untouched, and is the form generated clients expect.</para>
    ///
    /// <para><b>Allow-list, not reflection.</b> Every resource declares an explicit list of
    /// sortable wire field names. An unknown name is a <b>400 that lists the permitted values</b>
    /// — never a silently-ignored request. Reflecting an arbitrary string onto an
    /// <see cref="IQueryable{T}"/> is an injection-shaped API surface even through EF, and a
    /// reflection-derived list silently changes shape when a property is renamed.</para>
    ///
    /// <para>Matching is ordinal-ignore-case so <c>currName</c> and <c>CurrName</c> both bind, but
    /// the canonical wire name is camel-case, matching the JSON payload.</para>
    /// </summary>
    public static class SortSpecification
    {
        /// <summary>
        /// Parses and validates <paramref name="sort"/> against <paramref name="allowedFields"/>.
        /// A null/whitespace <paramref name="sort"/> parses successfully to an empty term list,
        /// which means "the resource's existing default ordering" — never "no ordering".
        /// </summary>
        /// <returns><c>true</c> when the whole expression is valid; otherwise <c>false</c> and
        /// <paramref name="error"/> carries a message naming the offending term and listing the
        /// permitted values.</returns>
        public static bool TryParse(
            string? sort,
            IReadOnlyList<string> allowedFields,
            out IReadOnlyList<SortTerm> terms,
            out string? error)
        {
            ArgumentNullException.ThrowIfNull(allowedFields);

            terms = Array.Empty<SortTerm>();
            error = null;

            if (string.IsNullOrWhiteSpace(sort))
                return true;

            var parsed = new List<SortTerm>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var raw in sort.Split(',', StringSplitOptions.TrimEntries))
            {
                if (raw.Length == 0)
                {
                    error = Message("an empty sort term", allowedFields);
                    return false;
                }

                var descending = raw[0] == '-';
                var name = descending ? raw[1..].Trim() : raw.TrimStart('+').Trim();

                if (name.Length == 0)
                {
                    error = Message($"'{raw}'", allowedFields);
                    return false;
                }

                var canonical = allowedFields.FirstOrDefault(
                    f => string.Equals(f, name, StringComparison.OrdinalIgnoreCase));

                if (canonical is null)
                {
                    error = Message($"'{name}'", allowedFields);
                    return false;
                }

                if (!seen.Add(canonical))
                {
                    error = string.Format(
                        CultureInfo.InvariantCulture,
                        "The sort field '{0}' is specified more than once.",
                        canonical);
                    return false;
                }

                parsed.Add(new SortTerm(canonical, descending));
            }

            terms = parsed;
            return true;
        }

        /// <summary>
        /// The canonical string handed to the business service — the validated terms rejoined in
        /// their original order. Returns <c>null</c> for an empty term list so the service takes
        /// its existing default-ordering path unchanged.
        /// </summary>
        public static string? ToServiceSort(IReadOnlyList<SortTerm> terms)
            => terms is null || terms.Count == 0
                ? null
                : string.Join(',', terms.Select(t => t.ToString()));

        private static string Message(string offender, IReadOnlyList<string> allowedFields)
            => string.Format(
                CultureInfo.InvariantCulture,
                "The sort field {0} is not sortable on this resource. Permitted values: {1}. "
                + "Prefix a field with '-' for descending order.",
                offender,
                string.Join(", ", allowedFields));
    }
}

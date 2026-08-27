/**
 * M2-C03 — a small, dependency-free subsequence fuzzy matcher for the command palette.
 * `query`'s characters must appear in `text`, in order, but not necessarily contiguously
 * (`"crm"` matches `"Customer Master"`). Case-insensitive.
 *
 * Returns `null` for no match, or a score where **lower is better** — an earlier, more
 * contiguous match scores lower than one scattered across the whole string, so `"cur"`
 * ranks `"Currency Master"` ahead of `"Customer Requisition Report"`.
 */
export function fuzzyScore(query: string, text: string): number | null {
  if (query.length === 0) {
    return 0;
  }
  const q = query.toLowerCase();
  const t = text.toLowerCase();

  let qi = 0;
  let score = 0;
  let lastMatchIndex = -1;

  for (let ti = 0; ti < t.length && qi < q.length; ti++) {
    if (t[ti] === q[qi]) {
      // A gap since the previous matched character costs more than a contiguous run —
      // the whole point of ranking "cur" -> "Currency" above a scattered match.
      score += lastMatchIndex === -1 ? ti : ti - lastMatchIndex - 1;
      lastMatchIndex = ti;
      qi++;
    }
  }

  return qi === q.length ? score : null;
}

export interface FuzzyMatch<T> {
  readonly item: T;
  readonly score: number;
}

/** Filters and sorts `items` by `fuzzyScore(query, textOf(item))`, best match first. */
export function fuzzyFilter<T>(
  query: string,
  items: readonly T[],
  textOf: (item: T) => string,
): T[] {
  const matches: FuzzyMatch<T>[] = [];
  for (const item of items) {
    const score = fuzzyScore(query, textOf(item));
    if (score !== null) {
      matches.push({ item, score });
    }
  }
  matches.sort((a, b) => a.score - b.score);
  return matches.map((m) => m.item);
}

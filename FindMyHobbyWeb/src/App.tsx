import { FormEvent, useMemo, useState } from 'react';
import { searchCourses, type CourseSearchResponse } from './api';

const defaultState = {
  hobbyDescription: 'beginner pottery classes',
  postcode: 'SW1A 1AA',
  maximumDistanceMiles: 10,
};

function formatDistance(value: number | null | undefined) {
  if (value == null) {
    return '-';
  }

  return `${value.toFixed(1)} mi`;
}

function linkCell(url?: string | null) {
  if (!url) {
    return '-';
  }

  return (
    <a href={url} target="_blank" rel="noreferrer">
      Open
    </a>
  );
}

export function App() {
  const [hobbyDescription, setHobbyDescription] = useState(defaultState.hobbyDescription);
  const [postcode, setPostcode] = useState(defaultState.postcode);
  const [maximumDistanceMiles, setMaximumDistanceMiles] = useState(
    String(defaultState.maximumDistanceMiles),
  );
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<CourseSearchResponse | null>(null);

  const parsedDistance = useMemo(() => {
    const value = Number(maximumDistanceMiles);
    return Number.isFinite(value) ? value : 0;
  }, [maximumDistanceMiles]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const response = await searchCourses({
        hobbyDescription: hobbyDescription.trim(),
        postcode: postcode.trim(),
        maximumDistanceMiles: parsedDistance,
      });

      setResult(response);
    } catch (err) {
      setResult(null);
      setError(err instanceof Error ? err.message : 'Unable to search for courses.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="app-shell">
      <main className="app-main">
        <header className="page-header">
          <div>
            <p className="eyebrow">Find My Hobby</p>
            <h1>Search for UK hobby classes</h1>
            <p className="intro">
              Tell the app what you want to learn, where you are, and how far you are willing
              to travel. It will ask the API for current courses and display the matches below.
            </p>
          </div>
        </header>

        <section className="composer">
          <form className="search-form" onSubmit={handleSubmit}>
            <label>
              Hobby or class
              <textarea
                value={hobbyDescription}
                onChange={(event) => setHobbyDescription(event.target.value)}
                rows={4}
                placeholder="e.g. beginner pottery classes"
              />
            </label>

            <div className="inline-fields">
              <label>
                Postcode
                <input
                  value={postcode}
                  onChange={(event) => setPostcode(event.target.value)}
                  placeholder="e.g. SW1A 1AA"
                />
              </label>

              <label>
                Max distance
                <input
                  type="number"
                  min={1}
                  max={100}
                  step={1}
                  value={maximumDistanceMiles}
                  onChange={(event) => setMaximumDistanceMiles(event.target.value)}
                />
              </label>
            </div>

            <button type="submit" disabled={loading}>
              {loading ? 'Searching...' : 'Search courses'}
            </button>
          </form>

          {error ? <p className="error-banner">{error}</p> : null}
        </section>

        <section className="results-section">
          <div className="section-head">
            <h2>Results</h2>
            {result ? (
              <p className="meta">
                {result.results.length} result{result.results.length === 1 ? '' : 's'} for{' '}
                {result.query.hobbyDescription}
              </p>
            ) : (
              <p className="meta">No search yet</p>
            )}
          </div>

          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Title</th>
                  <th>Provider</th>
                  <th>Location</th>
                  <th>Distance</th>
                  <th>Price</th>
                  <th>Schedule</th>
                  <th>Booking</th>
                  <th>Source</th>
                </tr>
              </thead>
              <tbody>
                {result ? (
                  result.results.length ? (
                    result.results.map((item) => (
                      <tr key={`${item.title}-${item.providerName}`}>
                        <td>
                          <strong>{item.title}</strong>
                          <div className="secondary">{item.description}</div>
                        </td>
                        <td>{item.providerName}</td>
                        <td>
                          <div>{item.address ?? '-'}</div>
                          <div className="secondary">{item.postcode ?? '-'}</div>
                        </td>
                        <td>
                          {formatDistance(item.estimatedDistanceMiles)}
                          {item.distanceIsEstimated ? (
                            <span className="secondary"> est.</span>
                          ) : null}
                        </td>
                        <td>{item.price ?? '-'}</td>
                        <td>{item.schedule ?? '-'}</td>
                        <td>{linkCell(item.bookingUrl)}</td>
                        <td>{linkCell(item.sourceUrl)}</td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={8} className="empty-state">
                        No matching courses were returned for this search.
                      </td>
                    </tr>
                  )
                ) : (
                  <tr>
                    <td colSpan={8} className="empty-state">
                      Run a search to see matching courses here.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          {result?.notes ? (
            <section className="notes">
              <h3>Notes</h3>
              <p>{result.notes}</p>
            </section>
          ) : null}
        </section>
      </main>
    </div>
  );
}

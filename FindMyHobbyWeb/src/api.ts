export interface CourseSearchRequest {
  hobbyDescription: string;
  postcode: string;
  maximumDistanceMiles: number;
}

export interface CourseSearchQuery {
  hobbyDescription: string;
  postcode: string;
  maximumDistanceMiles: number;
}

export interface CourseSearchResult {
  title: string;
  providerName: string;
  description: string;
  address?: string | null;
  postcode?: string | null;
  estimatedDistanceMiles?: number | null;
  distanceIsEstimated: boolean;
  price?: string | null;
  schedule?: string | null;
  bookingUrl?: string | null;
  sourceUrl?: string | null;
}

export interface CourseSearchResponse {
  query: CourseSearchQuery;
  results: CourseSearchResult[];
  notes?: string | null;
}

export interface ApiErrorResponse {
  detail?: string;
  title?: string;
  status?: number;
  rawResponse?: string;
}

function asErrorMessage(payload: unknown, fallback: string): string {
  if (!payload || typeof payload !== 'object') {
    return fallback;
  }

  const maybePayload = payload as Record<string, unknown>;
  const detail = maybePayload.detail;
  const title = maybePayload.title;
  const rawResponse = maybePayload.rawResponse;

  if (typeof detail === 'string' && detail.trim()) {
    return detail;
  }

  if (typeof title === 'string' && title.trim()) {
    return title;
  }

  if (typeof rawResponse === 'string' && rawResponse.trim()) {
    return rawResponse;
  }

  return fallback;
}

export async function searchCourses(
  request: CourseSearchRequest,
): Promise<CourseSearchResponse> {
  const response = await fetch('/api/courses/search', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });

  const text = await response.text();
  let payload: unknown = null;

  if (text) {
    try {
      payload = JSON.parse(text) as unknown;
    } catch {
      payload = { rawResponse: text };
    }
  }

  if (!response.ok) {
    throw new Error(asErrorMessage(payload, 'Unable to search for courses.'));
  }

  return payload as CourseSearchResponse;
}

import { describe, expect, it } from 'vitest';
import { AxiosError } from 'axios';
import { ApiError, toApiError } from './errors';

describe('toApiError', () => {
  it('passes through an existing ApiError', () => {
    const original = new ApiError('boom', 400);
    expect(toApiError(original)).toBe(original);
  });

  it('extracts detail from RFC7807 problem details', () => {
    const axiosErr = new AxiosError('Request failed');
    axiosErr.response = {
      status: 409,
      data: { title: 'Conflict', detail: 'Already enrolled.' },
      statusText: 'Conflict',
      headers: {},
      config: {} as never,
    };
    const result = toApiError(axiosErr);
    expect(result.status).toBe(409);
    expect(result.message).toBe('Already enrolled.');
  });

  it('produces a friendly message on network errors', () => {
    const axiosErr = new AxiosError('Network Error');
    axiosErr.code = 'ERR_NETWORK';
    const result = toApiError(axiosErr);
    expect(result.status).toBe(0);
    expect(result.message).toMatch(/unable to reach/i);
  });
});

import http from 'k6/http';
import { check, sleep } from 'k6';

const baseUrl = __ENV.BASE_URL || 'https://staging-api.sysvet.example';
const tenants = (__ENV.TENANTS || '').split(',').filter(Boolean);
const vusPerTenant = Number(__ENV.VUS_PER_TENANT || '5');
const duration = __ENV.DURATION || '5m';

export const options = {
  scenarios: {
    perTenant: {
      executor: 'per-vu-iterations',
      vus: tenants.length * vusPerTenant,
      iterations: 1,
      maxDuration: duration,
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.01'],
    http_req_duration: ['p(95)<500'],
  },
};

export function setup() {
  if (tenants.length === 0) {
    throw new Error('Set TENANTS to comma-separated bearer tokens (one per tenant).');
  }

  return { tokens: tenants };
}

export default function (data) {
  const index = (__VU - 1) % data.tokens.length;
  const token = data.tokens[index];
  const headers = {
    Authorization: `Bearer ${token}`,
    Accept: 'application/json',
  };

  const routes = [
    '/api/v1/intelligence/dashboard',
    '/api/v1/tutors?page=1&pageSize=20',
    '/api/v1/inventory/products?page=1&pageSize=20',
    '/api/v1/financial-titles?page=1&pageSize=20',
  ];

  for (const path of routes) {
    const res = http.get(`${baseUrl}${path}`, { headers });
    check(res, { 'status is 200': (r) => r.status === 200 });
    sleep(0.2);
  }
}

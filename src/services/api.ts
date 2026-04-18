const API_BASE_URL = 'http://localhost:5009';

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const url = `${API_BASE_URL}${path}`;
  const response = await fetch(url, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers,
    },
    credentials: 'include', // Important for session cookies
  });

  if (!response.ok) {
    if (response.status === 401) {
      // Potentially redirect to login
      if (window.location.pathname !== '/') {
        window.location.href = '/';
      }
    }
    const error = await response.json().catch(() => ({ message: 'An unknown error occurred' }));
    throw new Error(error.message || 'API request failed');
  }

  return response.json();
}

export const api = {
  admin: {
    login: (credentials: any) => request<any>('/api/admin/login', { method: 'POST', body: JSON.stringify(credentials) }),
    logout: () => request<any>('/api/admin/logout'),
    getMetrics: () => request<any>('/api/admin/metrics'),
  },
  students: {
    getAll: () => request<any[]>('/api/student'),
    create: (data: any) => request<any>('/api/student', { method: 'POST', body: JSON.stringify(data) }),
    update: (id: number, data: any) => request<any>(`/api/student/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    delete: (id: number) => request<void>(`/api/student/${id}`, { method: 'DELETE' }),
  },
  researchAreas: {
    getAll: () => request<any[]>('/api/researchareas'),
    create: (area: any) => request<any>('/api/researchareas', { method: 'POST', body: JSON.stringify(area) }),
    delete: (id: number) => request<void>(`/api/researchareas/${id}`, { method: 'DELETE' }),
  },
  supervisors: {
    getAll: () => request<any[]>('/api/supervisors'),
    create: (supervisor: any) => request<any>('/api/supervisors', { method: 'POST', body: JSON.stringify(supervisor) }),
    delete: (id: number) => request<void>(`/api/supervisors/${id}`, { method: 'DELETE' }),
  },
  allocation: {
    getMatrix: () => request<any[]>('/api/allocation/matrix'),
    assign: (data: { projectId: number; supervisorId: number }) => 
      request<any>('/api/allocation/assign', { method: 'POST', body: JSON.stringify(data) }),
    toggleReveal: (projectId: number) => 
      request<any>(`/api/allocation/toggle-reveal/${projectId}`, { method: 'POST' }),
  },
  analytics: {
    getSummary: () => request<any>('/api/analytics/summary'),
  }
};

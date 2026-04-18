export interface AdminUser {
  id: number;
  email: string;
}

export interface Student {
  id: number;
  name: string;
  email: string;
  department: string;
}

export interface ResearchArea {
  id: number;
  name: string;
  description: string;
}

export interface Supervisor {
  id: number;
  name: string;
  email: string;
  department: string;
}

export interface Project {
  id: number;
  title: string;
  abstract?: string;
  studentId: number;
  student?: Student;
  researchAreaId: number;
  supervisorId: number;
  isRevealed: boolean;
}

export interface DashboardMetrics {
  totalProjects: number;
  totalSupervisors: number;
  totalResearchAreas: number;
  revealedCount: number;
}

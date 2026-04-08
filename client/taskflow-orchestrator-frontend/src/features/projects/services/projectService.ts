import { apiClient } from '@/lib/api/client'
import {
  Project, CreateProjectRequest, UpdateProjectRequest, ProjectMember,
  AddMemberRequest, ProjectRole
} from '../types/project.types'
import { ApiResponse } from '@/features/auth/types/auth.types'


export const projectService = {
  async getProjects(): Promise<Project[]> {
    const response = await apiClient.get<Project[]>('/project/api/projects')
    return response.data
  },

  async createProject(data: CreateProjectRequest): Promise<Project> {
    const response = await apiClient.post<Project>('/project/api/projects', data)
    return response.data
  },

  async updateProject(id: string, data: UpdateProjectRequest): Promise<Project> {
    const response = await apiClient.put<Project>(`/project/api/projects/${id}`, data)
    return response.data
  },

  async deleteProject(id: string): Promise<void> {
    await apiClient.delete(`/project/api/projects/${id}`)
  },


  // Управление участниками
  async getProjectMembers(projectId: string): Promise<ProjectMember[]> {
    const response = await apiClient.get<ApiResponse<ProjectMember[]>>(`/project/api/projects/${projectId}/members`)
    return response.data.value
  },

  async addMember(projectId: string, data: AddMemberRequest): Promise<void> {
    await apiClient.post(`/project/api/projects/${projectId}/members`, data)
  },

  async removeMember(projectId: string, userId: string): Promise<void> {
    await apiClient.delete(`/project/api/projects/${projectId}/members/${userId}`)
  },

  async updateMemberRole(projectId: string, userId: string, role: ProjectRole): Promise<void> {
    await apiClient.put(`/project/api/projects/${projectId}/members/${userId}/role`, { role }, {
      headers: { 'Content-Type': 'application/json' }
    })
  },

  async getProjectById(id: string): Promise<Project> {
    const response = await apiClient.get<Project>(`/project/api/projects/${id}`)
    return response.data
  }
}

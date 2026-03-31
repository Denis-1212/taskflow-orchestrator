import { apiClient } from '@/lib/api/client'
import { Project, CreateProjectRequest, UpdateProjectRequest } from '../types/project.types'

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
}

export interface Project {
  id: string
  name: string
  description: string | null
  ownerId: string
  createdAt: string
}

export interface CreateProjectRequest {
  name: string
  description?: string
}

export interface UpdateProjectRequest {
  name?: string
  description?: string
}
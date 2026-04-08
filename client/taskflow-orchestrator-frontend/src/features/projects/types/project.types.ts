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

export type ProjectRole = 'Owner' | 'Member' | 'Viewer'

export interface ProjectMember {
  userId: string
  userEmail: string
  fullName: string
  role: ProjectRole
}

export interface AddMemberRequest {
  userId: string
  role: Exclude<ProjectRole, 'Owner'>
}

export interface ProjectMemberDto {
  role: ProjectRole
}


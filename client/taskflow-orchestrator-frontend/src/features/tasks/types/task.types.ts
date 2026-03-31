export type TaskStatus = 'Todo' | 'InProgress' | 'Completed' | 'Cancelled'
export type TaskPriority = 'Low' | 'Medium' | 'High' | 'Critical'

export interface Task {
  id: string
  projectId: string
  title: string
  description: string | null
  status: TaskStatus
  priority: TaskPriority
  assigneeId: string | null
  assigneeName: string | null
  dueDate: string | null
  createdAt: string
  updatedAt?: string
}

export interface CreateTaskRequest {
  projectId: string
  title: string
  description?: string
  priority: TaskPriority
  assigneeId?: string | null
  dueDate?: string | null
}

export interface UpdateTaskRequest {
  title?: string
  description?: string
  priority?: TaskPriority
  dueDate?: string | null
}

export interface ChangeStatusRequest {
  status: TaskStatus
  comment?: string
}

export interface AssignTaskRequest {
  assigneeId: string
}

export interface TaskStatistics {
  total: number
  todo: number
  inProgress: number
  completed: number
  cancelled: number
}
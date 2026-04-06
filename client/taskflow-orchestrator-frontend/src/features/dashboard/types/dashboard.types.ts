export interface ProjectStats {
    id: string
    name: string
    description: string | null
    totalTasks: number
    completedTasks: number
    progress: number
    ownerId: string
    ownerName?: string
}

export interface UserStats {
    id: string
    email: string
    fullName: string
    projectsCount: number
    tasksCount: number
}

export interface TaskStats {
    total: number
    inProgress: number
    overdue: number
}
import { useProjects } from '@/features/projects/hooks/useProjects'
import { useTasks } from '@/features/tasks/hooks/useTasks'
import { useAuthStore } from '@/features/auth/store/authStore'
import { isAfter, parseISO } from 'date-fns'

export function useMyProjectsStats() {
    const { user } = useAuthStore()
    const { data: projects, isLoading: projectsLoading } = useProjects()
    const { data: allTasks, isLoading: tasksLoading } = useTasks()

    if (projectsLoading || tasksLoading) {
        return { data: [], isLoading: true }
    }

    if (!projects || !allTasks) {
        return { data: [], isLoading: false }
    }

    // Проекты где пользователь является владельцем
    const myProjects = projects.filter(p => p.ownerId === user?.id)

    const stats = myProjects.map(project => {
        const projectTasks = allTasks.filter(t => t.projectId === project.id)
        const completedTasks = projectTasks.filter(t => t.status === 'Completed').length

        return {
            id: project.id,
            name: project.name,
            description: project.description,
            totalTasks: projectTasks.length,
            completedTasks,
            progress: projectTasks.length > 0 ? (completedTasks / projectTasks.length) * 100 : 0,
            ownerId: project.ownerId,
        }
    })

    return { data: stats, isLoading: false }
}

export function useMyTaskStats() {
    const { user } = useAuthStore()
    const { data: tasks, isLoading } = useTasks()

    if (isLoading) {
        return { data: null, isLoading: true }
    }

    if (!tasks) {
        return { data: null, isLoading: false }
    }

    // Задачи назначенные на текущего пользователя
    const myTasks = tasks.filter(t => t.assigneeId === user?.id)

    const now = new Date()
    const stats = {
        total: myTasks.length,
        inProgress: myTasks.filter(t => t.status === 'InProgress').length,
        overdue: myTasks.filter(t =>
            t.dueDate &&
            t.status !== 'Completed' &&
            t.status !== 'Cancelled' &&
            isAfter(now, parseISO(t.dueDate))
        ).length,
    }

    return { data: stats, isLoading: false }
}

export function useRecentTasks(limit: number = 5) {
    const { user } = useAuthStore()
    const { data: tasks, isLoading } = useTasks()

    if (isLoading) {
        return { data: [], isLoading: true }
    }

    if (!tasks) {
        return { data: [], isLoading: false }
    }

    // Задачи назначенные на текущего пользователя
    const myTasks = tasks.filter(t => t.assigneeId === user?.id)

    const recent = [...myTasks]
        .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
        .slice(0, limit)

    return { data: recent, isLoading: false }
}
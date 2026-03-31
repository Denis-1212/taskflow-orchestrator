import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { taskService } from '../services/taskService'
import {
    CreateTaskRequest,
    UpdateTaskRequest,
    ChangeStatusRequest,
    AssignTaskRequest,
} from '../types/task.types'

export const taskKeys = {
    all: ['tasks'] as const,
    lists: () => [...taskKeys.all, 'list'] as const,
    list: (filters?: any) => [...taskKeys.lists(), filters] as const,
    details: () => [...taskKeys.all, 'detail'] as const,
    detail: (id: string) => [...taskKeys.details(), id] as const,
    statistics: (projectId: string) => [...taskKeys.all, 'statistics', projectId] as const,
}

export function useTasks(filters?: {
    projectId?: string
    status?: string
    priority?: string
    assigneeId?: string
}) {
    return useQuery({
        queryKey: taskKeys.list(filters),
        queryFn: () => taskService.getTasks(filters),
        staleTime: 0,
        refetchOnMount: true,
        refetchOnWindowFocus: true,
    })
}

export function useTask(id: string) {
    return useQuery({
        queryKey: taskKeys.detail(id),
        queryFn: () => taskService.getTaskById(id),
        enabled: !!id,
    })
}

export function useCreateTask() {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (data: CreateTaskRequest) => taskService.createTask(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: taskKeys.lists() })
            toast.success('Task created successfully')
        },
        onError: (error: any) => {
            const status = error.response?.status
            if (status === 500) {
                toast.error('Server error. Please try again later.')
            } else if (status === 403) {
                toast.error('You do not have permission to create tasks in this project.')
            } else {
                toast.error(error.response?.data?.message || 'Failed to create task')
            }
        },
    })
}

export function useUpdateTask() {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: ({ id, data }: { id: string; data: UpdateTaskRequest }) =>
            taskService.updateTask(id, data),
        onSuccess: (_, { id }) => {
            queryClient.invalidateQueries({ queryKey: taskKeys.lists() })
            queryClient.invalidateQueries({ queryKey: taskKeys.detail(id) })
            toast.success('Task updated successfully')
        },
        onError: (error: any) => {
            toast.error(error.response?.data?.message || 'Failed to update task')
        },
    })
}

export function useDeleteTask() {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (id: string) => taskService.deleteTask(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: taskKeys.lists() })
            toast.success('Task deleted successfully')
        },
        onError: (error: any) => {
            toast.error(error.response?.data?.message || 'Failed to delete task')
        },
    })
}

export function useChangeStatus() {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: ({ id, data }: { id: string; data: ChangeStatusRequest }) =>
            taskService.changeStatus(id, data),
        onSuccess: (_, { id }) => {
            queryClient.invalidateQueries({ queryKey: taskKeys.lists() })
            queryClient.invalidateQueries({ queryKey: taskKeys.detail(id) })
            toast.success('Status updated')
        },
        onError: (error: any) => {
            console.error('Change status error:', error)
            toast.error(error.response?.data?.message || 'Failed to update status')
        },
    })
}

export function useAssignTask() {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: ({ id, data }: { id: string; data: AssignTaskRequest }) =>
            taskService.assignTask(id, data),
        onSuccess: (_, { id }) => {
            queryClient.invalidateQueries({ queryKey: taskKeys.lists() })
            queryClient.invalidateQueries({ queryKey: taskKeys.detail(id) })
            toast.success('Task assigned successfully')
        },
        onError: (error: any) => {
            toast.error(error.response?.data?.message || 'Failed to assign task')
        },
    })
}

export function useTaskStatistics(projectId: string) {
    return useQuery({
        queryKey: taskKeys.statistics(projectId),
        queryFn: () => taskService.getStatistics(projectId),
        enabled: !!projectId,
    })
}
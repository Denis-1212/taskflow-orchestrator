import { apiClient } from '@/lib/api/client'
import {
    Task,
    CreateTaskRequest,
    UpdateTaskRequest,
    ChangeStatusRequest,
    AssignTaskRequest,
    TaskStatistics,
} from '../types/task.types'

export const taskService = {
    // Получение списка задач с фильтрацией
    async getTasks(params?: {
        projectId?: string
        status?: string
        priority?: string
        assigneeId?: string
    }): Promise<Task[]> {
        const response = await apiClient.get<Task[]>('/task/api/tasks', { params })
        return response.data
    },

    // Получение задачи по ID
    async getTaskById(id: string): Promise<Task> {
        const response = await apiClient.get<Task>(`/task/api/tasks/${id}`)
        return response.data
    },

    // Создание задачи
    async createTask(data: CreateTaskRequest): Promise<Task> {
        const response = await apiClient.post<Task>('/task/api/tasks', data)
        return response.data
    },

    // Обновление задачи
    async updateTask(id: string, data: UpdateTaskRequest): Promise<Task> {
        const response = await apiClient.put<Task>(`/task/api/tasks/${id}`, data)
        return response.data
    },

    // Удаление задачи
    async deleteTask(id: string): Promise<void> {
        await apiClient.delete(`/task/api/tasks/${id}`)
    },

    // Изменение статуса
    async changeStatus(id: string, data: ChangeStatusRequest): Promise<Task> {
        const response = await apiClient.patch<Task>(`/task/api/tasks/${id}/status`, data)
        return response.data
    },

    // Назначение исполнителя
    async assignTask(id: string, data: AssignTaskRequest): Promise<Task> {
        const response = await apiClient.post<Task>(`/task/api/tasks/${id}/assign`, data)
        return response.data
    },

    // Статистика по проекту
    async getStatistics(projectId: string): Promise<TaskStatistics> {
        const response = await apiClient.get<TaskStatistics>(`/task/api/tasks/projects/${projectId}/statistics`)
        return response.data
    },
}
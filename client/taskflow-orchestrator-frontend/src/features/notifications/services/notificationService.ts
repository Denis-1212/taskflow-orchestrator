import { apiClient } from '@/lib/api/client'
import { Notification, NotificationsResponse } from '../types/notification.types'

export const notificationService = {
    async getNotifications(params?: {
        unreadOnly?: boolean
        page?: number
        pageSize?: number
    }): Promise<NotificationsResponse> {
        const response = await apiClient.get<Notification[]>('/notification/api/notifications', {
            params
        })
        return response.data
    },

    async getUnreadCount(): Promise<number> {
        const response = await apiClient.get<number>('/notification/api/notifications/unread-count')
        return response.data
    },

    async markAsRead(id: string): Promise<void> {
        await apiClient.patch(`/notification/api/notifications/${id}/read`)
    },

    async markAllAsRead(): Promise<void> {
        await apiClient.patch('/notification/api/notifications/read-all')
    },
}
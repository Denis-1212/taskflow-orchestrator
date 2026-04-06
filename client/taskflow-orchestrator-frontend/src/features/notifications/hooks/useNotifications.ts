import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { notificationService } from '../services/notificationService'

export const notificationKeys = {
    all: ['notifications'] as const,
    list: (filters?: { unreadOnly?: boolean }) =>
        [...notificationKeys.all, 'list', filters] as const,
    unreadCount: () => [...notificationKeys.all, 'unreadCount'] as const,
}

export function useNotifications(params?: {
    unreadOnly?: boolean
}) {
    return useQuery({
        queryKey: notificationKeys.list(params),
        queryFn: () => notificationService.getNotifications(params),
        staleTime: 1000 * 30,
    })
}

export function useUnreadCount() {
    return useQuery({
        queryKey: notificationKeys.unreadCount(),
        queryFn: () => notificationService.getUnreadCount(),
        refetchInterval: 30000,
    })
}

export function useMarkAsRead() {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (id: string) => notificationService.markAsRead(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: notificationKeys.all })
            toast.success('Marked as read')
        },
        onError: (error: any) => {
            toast.error(error.response?.data?.message || 'Failed to mark as read')
        },
    })
}

export function useMarkAllAsRead() {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: () => notificationService.markAllAsRead(),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: notificationKeys.all })
            toast.success('All notifications marked as read')
        },
        onError: (error: any) => {
            toast.error(error.response?.data?.message || 'Failed to mark all as read')
        },
    })
}
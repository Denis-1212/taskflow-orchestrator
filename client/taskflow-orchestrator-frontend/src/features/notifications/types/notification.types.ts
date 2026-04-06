export type NotificationType = 'TaskAssigned' | 'TaskStatusChanged' | 'TaskCreated' | 'UserRegistered'

export interface Notification {
    id: string
    type: NotificationType
    title: string
    content: string
    metadata: string | null
    isRead: boolean
    createdAt: string
}

export type NotificationsResponse = Notification[]
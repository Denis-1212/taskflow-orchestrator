import { useState } from 'react'
import { Bell } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Popover } from '@/components/ui/popover'
import { useUnreadCount, useNotifications, useMarkAsRead, useMarkAllAsRead } from '../hooks/useNotifications'
import { formatRelativeDate } from '@/lib/utils/dateUtils'

export function NotificationBell() {
    const [open, setOpen] = useState(false)
    const { data: unreadCount = 0, refetch: refetchUnreadCount } = useUnreadCount()
    const { data: notifications = [], refetch: refetchNotifications } = useNotifications({
        unreadOnly: false
    })
    const markAsRead = useMarkAsRead()
    const markAllAsRead = useMarkAllAsRead()

    const handleMarkAsRead = (id: string) => {
        markAsRead.mutate(id, {
            onSuccess: () => {
                refetchNotifications()
                refetchUnreadCount()
            }
        })
    }

    const handleMarkAllAsRead = () => {
        markAllAsRead.mutate(undefined, {
            onSuccess: () => {
                refetchNotifications()
                refetchUnreadCount()
            }
        })
    }

    // Показываем только последние 5
    const recentNotifications = notifications.slice(0, 5)

    const trigger = (
        <Button variant="ghost" size="icon" className="relative">
            <Bell className="h-5 w-5" />
            {unreadCount > 0 && (
                <span className="absolute -top-1 -right-1 h-5 w-5 rounded-full bg-destructive text-destructive-foreground text-xs flex items-center justify-center">
                    {unreadCount > 9 ? '9+' : unreadCount}
                </span>
            )}
        </Button>
    )

    return (
        <Popover
            trigger={trigger}
            open={open}
            onOpenChange={setOpen}
            className="w-96 p-0"
            align="end"
        >
            <div>
                <div className="p-3 border-b">
                    <div className="flex justify-between items-center">
                        <h3 className="font-semibold">Notifications</h3>
                        {unreadCount > 0 && (
                            <Button variant="ghost" size="sm" className="text-xs h-7" onClick={handleMarkAllAsRead}>
                                Mark all as read
                            </Button>
                        )}
                    </div>
                </div>
                <div className="max-h-96 overflow-y-auto">
                    {recentNotifications.length === 0 ? (
                        <div className="p-8 text-center text-muted-foreground">
                            No notifications
                        </div>
                    ) : (
                        recentNotifications.map((notification) => (
                            <div
                                key={notification.id}
                                className={`p-3 border-b hover:bg-muted/50 cursor-pointer transition-colors ${!notification.isRead ? 'bg-muted/30' : ''
                                    }`}
                                onClick={() => handleMarkAsRead(notification.id)}
                            >
                                <div className="flex justify-between items-start gap-2">
                                    <div className="flex-1">
                                        <p className="text-sm font-medium">{notification.title}</p>
                                        <p className="text-xs text-muted-foreground mt-1 line-clamp-2">
                                            {notification.content}
                                        </p>
                                        <p className="text-xs text-muted-foreground mt-2">
                                            {formatRelativeDate(notification.createdAt)}
                                        </p>
                                    </div>
                                    {!notification.isRead && (
                                        <div className="h-2 w-2 rounded-full bg-primary flex-shrink-0 mt-1" />
                                    )}
                                </div>
                            </div>
                        ))
                    )}
                </div>
                <div className="p-2 border-t">
                    <Button
                        variant="ghost"
                        size="sm"
                        className="w-full text-xs"
                        onClick={() => {
                            setOpen(false)
                            window.location.href = '/notifications'
                        }}
                    >
                        View all notifications
                    </Button>
                </div>
            </div>
        </Popover>
    )
}
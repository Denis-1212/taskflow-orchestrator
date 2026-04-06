import { useState } from 'react'
import { RefreshCw, CheckCheck } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { useNotifications, useMarkAsRead, useMarkAllAsRead } from '../hooks/useNotifications'
import { formatRelativeDate } from '@/lib/utils/dateUtils'

export default function NotificationsPage() {
  const [unreadOnly, setUnreadOnly] = useState(false)

  const { data: notifications = [], isLoading, error, refetch } = useNotifications({
    unreadOnly: unreadOnly || undefined
  })
  const markAsRead = useMarkAsRead()
  const markAllAsRead = useMarkAllAsRead()

  const handleMarkAsRead = (id: string) => {
    markAsRead.mutate(id, {
      onSuccess: () => {
        refetch()
      }
    })
  }

  const handleMarkAllAsRead = () => {
    markAllAsRead.mutate(undefined, {
      onSuccess: () => {
        refetch()
      }
    })
  }

  if (error) {
    return (
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <h1 className="text-3xl font-bold">Notifications</h1>
          <Button variant="outline" onClick={() => refetch()}>
            <RefreshCw className="h-4 w-4 mr-2" />
            Retry
          </Button>
        </div>
        <Card className="p-12 text-center">
          <p className="text-destructive">Failed to load notifications</p>
        </Card>
      </div>
    )
  }

  const filteredNotifications = unreadOnly
    ? notifications.filter(n => !n.isRead)
    : notifications

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center flex-wrap gap-4">
        <h1 className="text-3xl font-bold">Notifications</h1>
        <div className="flex gap-2">
          <Button
            variant={unreadOnly ? 'default' : 'outline'}
            size="sm"
            onClick={() => setUnreadOnly(!unreadOnly)}
          >
            Unread only
          </Button>
          <Button variant="outline" onClick={() => refetch()}>
            <RefreshCw className="h-4 w-4 mr-2" />
            Refresh
          </Button>
          <Button variant="outline" onClick={handleMarkAllAsRead}>
            <CheckCheck className="h-4 w-4 mr-2" />
            Mark all as read
          </Button>
        </div>
      </div>

      {isLoading ? (
        <div className="space-y-3">
          {[1, 2, 3, 4, 5].map((i) => (
            <Card key={i} className="p-4 h-24 animate-pulse bg-muted" />
          ))}
        </div>
      ) : filteredNotifications.length === 0 ? (
        <Card className="p-12 text-center">
          <p className="text-muted-foreground">
            {unreadOnly ? 'No unread notifications' : 'No notifications'}
          </p>
        </Card>
      ) : (
        <div className="space-y-3">
          {filteredNotifications.map((notification) => (
            <Card
              key={notification.id}
              className={`p-4 transition-colors ${!notification.isRead ? 'border-l-4 border-l-primary' : ''
                }`}
            >
              <div className="flex justify-between items-start gap-4">
                <div className="flex-1">
                  <h3 className="font-semibold">{notification.title}</h3>
                  <p className="text-sm text-muted-foreground mt-1">
                    {notification.content}
                  </p>
                  <p className="text-xs text-muted-foreground mt-2">
                    {formatRelativeDate(notification.createdAt)}
                  </p>
                </div>
                {!notification.isRead && (
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => handleMarkAsRead(notification.id)}
                  >
                    Mark as read
                  </Button>
                )}
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}
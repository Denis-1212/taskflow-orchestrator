import { Plus, RefreshCw } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { useTasks } from '../hooks/useTasks'

export default function TasksPage() {
  const { data: tasks, isLoading, error, refetch } = useTasks()

  if (error) {
    return (
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <h1 className="text-3xl font-bold">Tasks</h1>
          <Button variant="outline" onClick={() => refetch()}>
            <RefreshCw className="h-4 w-4 mr-2" />
            Retry
          </Button>
        </div>
        <Card className="p-12 text-center">
          <p className="text-destructive">Failed to load tasks</p>
        </Card>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-3xl font-bold">Tasks</h1>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => refetch()}>
            <RefreshCw className="h-4 w-4 mr-2" />
            Refresh
          </Button>
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            New Task
          </Button>
        </div>
      </div>

      {isLoading ? (
        <div className="flex justify-center p-8">Loading tasks...</div>
      ) : !tasks || tasks.length === 0 ? (
        <Card className="p-12 text-center">
          <p className="text-muted-foreground">No tasks yet</p>
          <Button variant="outline" className="mt-4">
            Create your first task
          </Button>
        </Card>
      ) : (
        <div className="space-y-4">
          {tasks.map((task) => (
            <Card key={task.id} className="p-4">
              <div className="flex justify-between items-start">
                <div>
                  <h3 className="font-semibold">{task.title}</h3>
                  <p className="text-sm text-muted-foreground mt-1">
                    {task.description || 'No description'}
                  </p>
                </div>
                <div className="flex gap-2">
                  <span className="text-xs px-2 py-1 rounded bg-muted">
                    {task.status}
                  </span>
                  <span className="text-xs px-2 py-1 rounded bg-muted">
                    {task.priority}
                  </span>
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}
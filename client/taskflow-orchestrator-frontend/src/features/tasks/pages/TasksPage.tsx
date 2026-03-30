import { useState } from 'react'
import { Plus, RefreshCw, Pencil } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { useTasks, useChangeStatus } from '../hooks/useTasks'
import { CreateTaskDialog } from '../components/CreateTaskDialog'
import { EditTaskDialog } from '../components/EditTaskDialog'
import { TaskFilters } from '../components/TaskFilters'
import { Task, TaskStatus } from '../types/task.types'

export default function TasksPage() {
  const [filters, setFilters] = useState<{
    projectId?: string
    status?: string
    priority?: string
    assigneeId?: string
  }>({})
  const [editingTask, setEditingTask] = useState<Task | null>(null)

  const { data: tasks, isLoading, error, refetch } = useTasks(filters)
  const changeStatus = useChangeStatus()

  const handleFilterChange = (newFilters: typeof filters) => {
    setFilters(newFilters)
  }

  const handleStatusChange = (taskId: string, newStatus: TaskStatus) => {
    changeStatus.mutate({ id: taskId, data: { status: newStatus } })
  }

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
          <CreateTaskDialog>
            <Button>
              <Plus className="h-4 w-4 mr-2" />
              New Task
            </Button>
          </CreateTaskDialog>
        </div>
      </div>

      <TaskFilters filters={filters} onFilterChange={handleFilterChange} />

      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {[1, 2, 3, 4, 5, 6].map((i) => (
            <Card key={i} className="p-4 h-32 animate-pulse bg-muted" />
          ))}
        </div>
      ) : !tasks || tasks.length === 0 ? (
        <Card className="p-12 text-center">
          <p className="text-muted-foreground">No tasks found</p>
          {Object.keys(filters).length > 0 && (
            <Button variant="outline" className="mt-4" onClick={() => setFilters({})}>
              Clear filters
            </Button>
          )}
        </Card>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {tasks.map((task) => (
            <Card key={task.id} className="p-4 hover:shadow-md transition-shadow">
              <div className="flex justify-between items-start">
                <h3 className="font-semibold truncate flex-1" title={task.title}>
                  {task.title.length > 40 ? `${task.title.substring(0, 40)}...` : task.title}
                </h3>
                <div className="flex gap-1 ml-2">
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-7 w-7"
                    onClick={() => setEditingTask(task)}
                  >
                    <Pencil className="h-3 w-3" />
                  </Button>
                  <span className={`text-xs px-2 py-1 rounded ${task.priority === 'Critical' ? 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400' :
                    task.priority === 'High' ? 'bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400' :
                      task.priority === 'Medium' ? 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400' :
                        'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400'
                    }`}>
                    {task.priority}
                  </span>
                </div>
              </div>

              <p className="text-sm text-muted-foreground mt-2 line-clamp-2">
                {task.description || 'No description'}
              </p>

              <div className="flex justify-between items-center mt-3">
                <select
                  className="text-xs px-2 py-1 rounded border border-input bg-background"
                  value={task.status}
                  onChange={(e) => handleStatusChange(task.id, e.target.value as TaskStatus)}
                >
                  <option value="Todo">Todo</option>
                  <option value="InProgress">In Progress</option>
                  <option value="Completed">Completed</option>
                  <option value="Cancelled">Cancelled</option>
                </select>
                {task.dueDate && (
                  <span className="text-xs text-muted-foreground">
                    Due: {new Date(task.dueDate).toLocaleDateString()}
                  </span>
                )}
              </div>

              {task.assigneeName && (
                <div className="mt-2 text-xs text-muted-foreground">
                  Assignee: {task.assigneeName}
                </div>
              )}
            </Card>
          ))}
        </div>
      )}

      {editingTask && (
        <EditTaskDialog
          task={editingTask}
          open={!!editingTask}
          onClose={() => setEditingTask(null)}

        />
      )}
    </div>
  )
}
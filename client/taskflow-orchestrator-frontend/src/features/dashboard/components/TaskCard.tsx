import { Card } from '@/components/ui/card'
import { Task } from '@/features/tasks/types/task.types'
import { useNavigate } from 'react-router-dom'
import { formatRelativeDate } from '@/lib/utils/dateUtils'

interface TaskCardProps {
    task: Task
}

export function DashboardTaskCard({ task }: TaskCardProps) {
    const navigate = useNavigate()

    const priorityColors = {
        Low: 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400',
        Medium: 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400',
        High: 'bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400',
        Critical: 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400',
    }

    return (
        <Card
            className="p-4 cursor-pointer hover:shadow-md transition-shadow"
            onClick={() => navigate(`/tasks`)}
        >
            <div className="flex justify-between items-start">
                <div className="flex-1">
                    <h4 className="font-medium">{task.title}</h4>
                    <p className="text-xs text-muted-foreground mt-1">
                        Project: {task.projectId.slice(0, 8)}...
                    </p>
                </div>
                <div className="flex gap-2">
                    <span className={`text-xs px-2 py-1 rounded ${priorityColors[task.priority]}`}>
                        {task.priority}
                    </span>
                </div>
            </div>

            <div className="flex justify-between items-center mt-3">
                <span className="text-xs text-muted-foreground">
                    Created {formatRelativeDate(task.createdAt)}
                </span>
                {task.dueDate && (
                    <span className="text-xs text-muted-foreground">
                        Due: {new Date(task.dueDate).toLocaleDateString()}
                    </span>
                )}
            </div>
        </Card>
    )
}
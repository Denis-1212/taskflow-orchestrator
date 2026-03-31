import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useUpdateTask, useChangeStatus } from '../hooks/useTasks'
import { Task, TaskPriority, TaskStatus } from '../types/task.types'

const editTaskSchema = z.object({
    title: z.string().min(1, 'Title is required'),
    description: z.string().optional(),
    priority: z.enum(['Low', 'Medium', 'High', 'Critical']),
    dueDate: z.string().nullable().optional(),
})

type EditTaskFormData = z.infer<typeof editTaskSchema>

interface EditTaskDialogProps {
    task: Task
    open: boolean
    onClose: () => void
}

export function EditTaskDialog({ task, open, onClose }: EditTaskDialogProps) {
    const updateTask = useUpdateTask()
    const changeStatus = useChangeStatus()
    const [selectedStatus, setSelectedStatus] = useState<TaskStatus>(task.status)
    const [isSaving, setIsSaving] = useState(false)

    const { register, handleSubmit, reset, formState: { errors } } = useForm<EditTaskFormData>({
        resolver: zodResolver(editTaskSchema),
        defaultValues: {
            title: task.title,
            description: task.description || '',
            priority: task.priority,
            dueDate: task.dueDate,
        },
    })

    useEffect(() => {
        if (open) {
            reset({
                title: task.title,
                description: task.description || '',
                priority: task.priority,
                dueDate: task.dueDate,
            })
            setSelectedStatus(task.status)
            setIsSaving(false)
        }
    }, [open, task, reset])

    const onSubmit = async (data: EditTaskFormData) => {
        if (isSaving) return
        setIsSaving(true)

        try {
            // Обновляем поля
            await updateTask.mutateAsync({ id: task.id, data })

            // Если статус изменился, обновляем его
            if (selectedStatus !== task.status) {
                await changeStatus.mutateAsync({ id: task.id, data: { status: selectedStatus } })
            }

            onClose()
        } catch (error) {
            console.error('Failed to save task:', error)
        } finally {
            setIsSaving(false)
        }
    }

    if (!open) return null

    const statuses: TaskStatus[] = ['Todo', 'InProgress', 'Completed', 'Cancelled']
    const priorities: TaskPriority[] = ['Low', 'Medium', 'High', 'Critical']

    return (
        <div className="fixed inset-0 z-50 bg-black/50 flex items-center justify-center" onClick={onClose}>
            <div className="bg-background rounded-lg p-6 w-full max-w-md max-h-[90vh] overflow-y-auto" onClick={(e) => e.stopPropagation()}>
                <h2 className="text-lg font-semibold mb-4">Edit Task</h2>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                    <div>
                        <Label htmlFor="title">Title *</Label>
                        <Input id="title" {...register('title')} />
                        {errors.title && (
                            <p className="text-sm text-destructive mt-1">{errors.title.message}</p>
                        )}
                    </div>

                    <div>
                        <Label htmlFor="description">Description</Label>
                        <textarea
                            id="description"
                            rows={3}
                            className="flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                            {...register('description')}
                        />
                    </div>

                    <div>
                        <Label htmlFor="priority">Priority</Label>
                        <select
                            id="priority"
                            className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                            {...register('priority')}
                        >
                            {priorities.map((p) => (
                                <option key={p} value={p}>{p}</option>
                            ))}
                        </select>
                    </div>

                    <div>
                        <Label htmlFor="dueDate">Due Date</Label>
                        <Input id="dueDate" type="date" {...register('dueDate')} />
                    </div>

                    <div>
                        <Label>Status</Label>
                        <div className="flex flex-wrap gap-2 mt-1">
                            {statuses.map((status) => (
                                <Button
                                    key={status}
                                    type="button"
                                    variant={selectedStatus === status ? 'default' : 'outline'}
                                    size="sm"
                                    onClick={() => setSelectedStatus(status)}
                                    className="flex-1"
                                >
                                    {status === 'InProgress' ? 'In Progress' : status}
                                </Button>
                            ))}
                        </div>
                    </div>

                    <div className="flex justify-end gap-2 pt-4">
                        <Button type="button" variant="outline" onClick={onClose}>
                            Cancel
                        </Button>
                        <Button type="submit" disabled={isSaving}>
                            {isSaving ? 'Saving...' : 'Save Changes'}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    )
}
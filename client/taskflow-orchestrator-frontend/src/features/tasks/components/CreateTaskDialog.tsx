import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useCreateTask } from '../hooks/useTasks'
import { useProjects } from '@/features/projects/hooks/useProjects'
import { TaskPriority } from '../types/task.types'

const createTaskSchema = z.object({
    projectId: z.string().min(1, 'Project is required'),
    title: z.string().min(1, 'Title is required'),
    description: z.string().optional(),
    priority: z.enum(['Low', 'Medium', 'High', 'Critical']),
    assigneeId: z.string().nullable().optional(),
    dueDate: z.string().nullable().optional(),
})

type CreateTaskFormData = z.infer<typeof createTaskSchema>

interface CreateTaskDialogProps {
    children: React.ReactNode
}

export function CreateTaskDialog({ children }: CreateTaskDialogProps) {
    const [open, setOpen] = useState(false)
    const createTask = useCreateTask()
    const { data: projects } = useProjects()

    const { register, handleSubmit, reset, formState: { errors } } = useForm<CreateTaskFormData>({
        resolver: zodResolver(createTaskSchema),
        defaultValues: {
            priority: 'Medium',
            assigneeId: null,
            dueDate: null,
        },
    })

    const onSubmit = async (data: CreateTaskFormData) => {
        await createTask.mutateAsync(data)
        setOpen(false)
        reset()
    }

    if (!open) {
        return <div onClick={() => setOpen(true)}>{children}</div>
    }

    return (
        <div className="fixed inset-0 z-50 bg-black/50 flex items-center justify-center" onClick={() => setOpen(false)}>
            <div className="bg-background rounded-lg p-6 w-full max-w-md max-h-[90vh] overflow-y-auto" onClick={(e) => e.stopPropagation()}>
                <h2 className="text-lg font-semibold mb-4">Create New Task</h2>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                    <div>
                        <Label htmlFor="projectId">Project *</Label>
                        <select
                            id="projectId"
                            className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                            {...register('projectId')}
                        >
                            <option value="">Select project</option>
                            {projects?.map((p) => (
                                <option key={p.id} value={p.id}>{p.name}</option>
                            ))}
                        </select>
                        {errors.projectId && (
                            <p className="text-sm text-destructive mt-1">{errors.projectId.message}</p>
                        )}
                    </div>

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
                            <option value="Low">Low</option>
                            <option value="Medium">Medium</option>
                            <option value="High">High</option>
                            <option value="Critical">Critical</option>
                        </select>
                    </div>

                    <div>
                        <Label htmlFor="dueDate">Due Date</Label>
                        <Input id="dueDate" type="date" {...register('dueDate')} />
                    </div>

                    <div className="flex justify-end gap-2 pt-4">
                        <Button type="button" variant="outline" onClick={() => setOpen(false)}>
                            Cancel
                        </Button>
                        <Button type="submit" disabled={createTask.isPending}>
                            {createTask.isPending ? 'Creating...' : 'Create Task'}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    )
}
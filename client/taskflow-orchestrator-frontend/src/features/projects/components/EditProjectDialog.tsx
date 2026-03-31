import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useUpdateProject } from '../hooks/useProjects'
import { Project } from '../types/project.types'

const editSchema = z.object({
    name: z.string().min(1, 'Name is required'),
    description: z.string().optional(),
})

type EditFormData = z.infer<typeof editSchema>

interface EditProjectDialogProps {
    project: Project
    open: boolean
    onClose: () => void
}

export function EditProjectDialog({ project, open, onClose }: EditProjectDialogProps) {
    const updateProject = useUpdateProject()

    const { register, handleSubmit, reset, formState: { errors } } = useForm<EditFormData>({
        resolver: zodResolver(editSchema),
        defaultValues: {
            name: project.name,
            description: project.description || '',
        },
    })

    useEffect(() => {
        if (open) {
            reset({
                name: project.name,
                description: project.description || '',
            })
        }
    }, [open, project, reset])

    const onSubmit = async (data: EditFormData) => {
        await updateProject.mutateAsync({ id: project.id, data })
        onClose()
    }

    if (!open) return null

    return (
        <div className="fixed inset-0 z-50 bg-black/50 flex items-center justify-center" onClick={onClose}>
            <div className="bg-background rounded-lg p-6 w-full max-w-md" onClick={(e) => e.stopPropagation()}>
                <h2 className="text-lg font-semibold mb-4">Edit Project</h2>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                    <div>
                        <Label htmlFor="name">Project Name *</Label>
                        <Input
                            id="name"
                            {...register('name')}
                            className={errors.name ? 'border-destructive' : ''}
                        />
                        {errors.name && (
                            <p className="text-sm text-destructive mt-1">{errors.name.message}</p>
                        )}
                    </div>

                    <div>
                        <Label htmlFor="description">Description</Label>
                        <textarea
                            id="description"
                            rows={3}
                            className="flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                            {...register('description')}
                        />
                    </div>

                    <div className="flex justify-end gap-2 pt-2">
                        <Button type="button" variant="outline" onClick={onClose}>
                            Cancel
                        </Button>
                        <Button type="submit" disabled={updateProject.isPending}>
                            {updateProject.isPending ? 'Saving...' : 'Save Changes'}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    )
}
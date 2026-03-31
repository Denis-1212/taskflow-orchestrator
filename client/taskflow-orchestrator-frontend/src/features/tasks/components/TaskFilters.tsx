import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { X } from 'lucide-react'
import { useProjects } from '@/features/projects/hooks/useProjects'
import { TaskStatus, TaskPriority } from '../types/task.types'

interface TaskFiltersProps {
    filters: {
        projectId?: string
        status?: string
        priority?: string
        assigneeId?: string
    }
    onFilterChange: (filters: any) => void
}

export function TaskFilters({ filters, onFilterChange }: TaskFiltersProps) {
    const { data: projects } = useProjects()
    const [localFilters, setLocalFilters] = useState(filters)

    const statuses: TaskStatus[] = ['Todo', 'InProgress', 'Completed', 'Cancelled']
    const priorities: TaskPriority[] = ['Low', 'Medium', 'High', 'Critical']

    const handleChange = (key: string, value: string) => {
        const newFilters = { ...localFilters, [key]: value || undefined }
        setLocalFilters(newFilters)
        onFilterChange(newFilters)
    }

    const clearFilters = () => {
        setLocalFilters({})
        onFilterChange({})
    }

    const hasFilters = Object.values(localFilters).some(v => v)

    return (
        <div className="bg-muted/30 p-4 rounded-lg space-y-4">
            <div className="flex justify-between items-center">
                <h3 className="text-sm font-medium">Filters</h3>
                {hasFilters && (
                    <Button variant="ghost" size="sm" onClick={clearFilters} className="h-8 px-2">
                        <X className="h-4 w-4 mr-1" />
                        Clear all
                    </Button>
                )}
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                <div>
                    <Label className="text-xs">Project</Label>
                    <select
                        className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm"
                        value={localFilters.projectId || ''}
                        onChange={(e) => handleChange('projectId', e.target.value)}
                    >
                        <option value="">All projects</option>
                        {projects?.map((p) => (
                            <option key={p.id} value={p.id}>{p.name}</option>
                        ))}
                    </select>
                </div>

                <div>
                    <Label className="text-xs">Status</Label>
                    <select
                        className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm"
                        value={localFilters.status || ''}
                        onChange={(e) => handleChange('status', e.target.value)}
                    >
                        <option value="">All statuses</option>
                        {statuses.map((s) => (
                            <option key={s} value={s}>{s}</option>
                        ))}
                    </select>
                </div>

                <div>
                    <Label className="text-xs">Priority</Label>
                    <select
                        className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm"
                        value={localFilters.priority || ''}
                        onChange={(e) => handleChange('priority', e.target.value)}
                    >
                        <option value="">All priorities</option>
                        {priorities.map((p) => (
                            <option key={p} value={p}>{p}</option>
                        ))}
                    </select>
                </div>

                <div>
                    <Label className="text-xs">Assignee</Label>
                    <Input
                        placeholder="Assignee ID"
                        className="h-9"
                        value={localFilters.assigneeId || ''}
                        onChange={(e) => handleChange('assigneeId', e.target.value)}
                    />
                </div>
            </div>
        </div>
    )
}
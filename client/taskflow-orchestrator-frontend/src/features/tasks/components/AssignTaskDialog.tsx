import { useState, useEffect } from 'react'
import { Button } from '@/components/ui/button'
import { Label } from '@/components/ui/label'
import { useAssignTask } from '../hooks/useTasks'
import { useProjectMembers } from '@/features/projects/hooks/useProjectMembers'
import { Task } from '../types/task.types'
import { ProjectMember } from '../../projects/types/project.types'

interface AssignTaskDialogProps {
    task: Task
    open: boolean
    onClose: () => void
}

export function AssignTaskDialog({ task, open, onClose }: AssignTaskDialogProps) {
    const assignTask = useAssignTask()
    const { data: members, isLoading } = useProjectMembers(task.projectId)
    const [selectedUserId, setSelectedUserId] = useState<string>(task.assigneeId || '')

    useEffect(() => {
        if (open && task.assigneeId) {
            setSelectedUserId(task.assigneeId)
        }
    }, [open, task.assigneeId])

    const handleAssign = async () => {
        if (selectedUserId && selectedUserId !== task.assigneeId) {
            await assignTask.mutateAsync({ id: task.id, data: { assigneeId: selectedUserId } })
        }
        onClose()
    }

    const handleUnassign = async () => {
        await assignTask.mutateAsync({ id: task.id, data: { assigneeId: '' } })
        onClose()
    }

    if (!open) return null

    return (
        <div className="fixed inset-0 z-50 bg-black/50 flex items-center justify-center" onClick={onClose}>
            <div className="bg-background rounded-lg p-6 w-full max-w-md" onClick={(e) => e.stopPropagation()}>
                <h2 className="text-lg font-semibold mb-4">Assign Task</h2>

                <div className="space-y-4">
                    <div>
                        <Label>Current assignee</Label>
                        <p className="text-sm text-muted-foreground mt-1">
                            {task.assigneeName || 'Not assigned'}
                        </p>
                    </div>

                    <div>
                        <Label>Select new assignee</Label>
                        {isLoading ? (
                            <p className="text-sm text-muted-foreground mt-1">Loading members...</p>
                        ) : (
                            <select
                                className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm mt-1"
                                value={selectedUserId}
                                onChange={(e) => setSelectedUserId(e.target.value)}
                            >
                                <option value="">Unassigned</option>
                                {members?.map((member: ProjectMember) => (
                                    <option key={member.userId} value={member.userId}>
                                        {member.fullName} ({member.userEmail})
                                    </option>
                                ))}
                            </select>
                        )}
                    </div>

                    <div className="flex justify-end gap-2 pt-4">
                        <Button type="button" variant="outline" onClick={onClose}>
                            Cancel
                        </Button>
                        {task.assigneeId && (
                            <Button type="button" variant="destructive" onClick={handleUnassign}>
                                Unassign
                            </Button>
                        )}
                        <Button
                            type="button"
                            onClick={handleAssign}
                            disabled={!selectedUserId || selectedUserId === task.assigneeId}
                        >
                            Assign
                        </Button>
                    </div>
                </div>
            </div>
        </div>
    )
}
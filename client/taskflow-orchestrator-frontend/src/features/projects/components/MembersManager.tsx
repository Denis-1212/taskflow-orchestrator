import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { UserPlus, Trash2, Crown, User, Eye, Search } from 'lucide-react'
import { useProjectMembers, useAddMember, useRemoveMember, useUpdateMemberRole } from '../hooks/useProjectMembers'
import { useSearchUsers } from '@/features/auth/hooks/useUsers'
import { useAuthStore } from '@/features/auth/store/authStore'
import { Project, ProjectRole, ProjectMember } from '../types/project.types'

interface MembersManagerProps {
    project: Project
}

export function MembersManager({ project }: MembersManagerProps) {
    const { user } = useAuthStore()
    const { data: members, isLoading: membersLoading } = useProjectMembers(project.id)
    const addMember = useAddMember(project.id)
    const removeMember = useRemoveMember(project.id)
    const updateRole = useUpdateMemberRole(project.id)

    const [showAddForm, setShowAddForm] = useState(false)
    const [searchQuery, setSearchQuery] = useState('')
    const [role, setRole] = useState<Exclude<ProjectRole, 'Owner'>>('Member')
    const [selectedUser, setSelectedUser] = useState<{ id: string; name: string; email: string } | null>(null)

    const { data: searchResults, isLoading: searchLoading } = useSearchUsers(searchQuery)

    const isOwner = project.ownerId === user?.id

    // Фильтруем уже добавленных участников
    const availableUsers = searchResults?.filter(
        (u) => !members?.some((m: ProjectMember) => m.userId === u.id) && u.id !== project.ownerId
    ) || []

    const handleAddMember = async () => {
        if (!selectedUser) return
        await addMember.mutateAsync({ userId: selectedUser.id, role })
        setSelectedUser(null)
        setSearchQuery('')
        setShowAddForm(false)
    }

    const getRoleIcon = (role: ProjectRole) => {
        switch (role) {
            case 'Owner': return <Crown className="h-4 w-4 text-yellow-500" />
            case 'Member': return <User className="h-4 w-4" />
            case 'Viewer': return <Eye className="h-4 w-4" />
            default: return <User className="h-4 w-4" />
        }
    }

    if (membersLoading) {
        return <div className="text-center p-8">Loading members...</div>
    }

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center">
                <h3 className="text-lg font-semibold">Members ({members?.length || 0})</h3>
                {isOwner && (
                    <Button size="sm" onClick={() => setShowAddForm(!showAddForm)}>
                        <UserPlus className="h-4 w-4 mr-2" />
                        Add Member
                    </Button>
                )}
            </div>

            {showAddForm && isOwner && (
                <Card className="p-4 space-y-3">
                    <div>
                        <Label>Search User</Label>
                        <div className="relative">
                            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                            <Input
                                placeholder="Search by name or email..."
                                className="pl-9"
                                value={searchQuery}
                                onChange={(e) => {
                                    setSearchQuery(e.target.value)
                                    setSelectedUser(null)
                                }}
                            />
                        </div>

                        {searchLoading && (
                            <div className="mt-2 text-sm text-muted-foreground">Searching...</div>
                        )}

                        {availableUsers.length > 0 && (
                            <div className="mt-2 border rounded-md divide-y max-h-48 overflow-y-auto">
                                {availableUsers.map((u) => (
                                    <div
                                        key={u.id}
                                        className={`p-2 cursor-pointer hover:bg-muted transition-colors ${selectedUser?.id === u.id ? 'bg-muted' : ''
                                            }`}
                                        onClick={() => setSelectedUser({ id: u.id, name: u.fullName, email: u.email })}
                                    >
                                        <div className="font-medium">{u.fullName}</div>
                                        <div className="text-xs text-muted-foreground">{u.email}</div>
                                    </div>
                                ))}
                            </div>
                        )}

                        {searchQuery.length >= 2 && availableUsers.length === 0 && !searchLoading && (
                            <div className="mt-2 text-sm text-muted-foreground">No users found</div>
                        )}
                    </div>

                    {selectedUser && (
                        <div>
                            <Label>Selected User</Label>
                            <div className="mt-1 p-2 bg-muted rounded-md">
                                <div className="font-medium">{selectedUser.name}</div>
                                <div className="text-xs text-muted-foreground">{selectedUser.email}</div>
                            </div>
                        </div>
                    )}

                    <div>
                        <Label>Role</Label>
                        <select
                            className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                            value={role}
                            onChange={(e) => setRole(e.target.value as Exclude<ProjectRole, 'Owner'>)}
                        >
                            <option value="Member">Member</option>
                            <option value="Viewer">Viewer</option>
                        </select>
                    </div>

                    <div className="flex justify-end gap-2">
                        <Button variant="outline" size="sm" onClick={() => {
                            setShowAddForm(false)
                            setSelectedUser(null)
                            setSearchQuery('')
                        }}>
                            Cancel
                        </Button>
                        <Button size="sm" onClick={handleAddMember} disabled={!selectedUser}>
                            Add Member
                        </Button>
                    </div>
                </Card>
            )}

            <div className="space-y-2">
                {members?.map((member: ProjectMember) => (
                    <Card key={member.userId} className="p-3">
                        <div className="flex justify-between items-center">
                            <div className="flex items-center gap-3">
                                <div className="flex items-center gap-1">
                                    {getRoleIcon(member.role)}
                                    <span className="text-sm font-medium">{member.fullName}</span>
                                </div>
                                <span className="text-xs text-muted-foreground">{member.userEmail}</span>
                                <span className="text-xs px-2 py-0.5 rounded bg-muted">{member.role}</span>
                            </div>

                            {isOwner && member.userId !== project.ownerId && (
                                <div className="flex gap-2">
                                    <select
                                        className="text-xs h-8 px-2 rounded border border-input bg-background"
                                        value={member.role}
                                        onChange={(e) => updateRole.mutate({ userId: member.userId, role: e.target.value as ProjectRole })}
                                    >
                                        <option value="Member">Member</option>
                                        <option value="Viewer">Viewer</option>
                                    </select>
                                    <Button
                                        variant="ghost"
                                        size="icon"
                                        className="h-8 w-8 text-destructive"
                                        onClick={() => removeMember.mutate(member.userId)}
                                    >
                                        <Trash2 className="h-4 w-4" />
                                    </Button>
                                </div>
                            )}
                        </div>
                    </Card>
                ))}
            </div>

            {members?.length === 0 && (
                <p className="text-center text-muted-foreground py-8">No members yet</p>
            )}
        </div>
    )
}
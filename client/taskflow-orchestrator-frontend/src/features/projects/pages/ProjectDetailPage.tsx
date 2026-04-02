import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { Card } from '@/components/ui/card'
import { MembersManager } from '../components/MembersManager'
import { useProject } from '../hooks/useProjects'
import { useUser } from '@/features/auth/hooks/useUsers'

export default function ProjectDetailPage() {
    const { id } = useParams<{ id: string }>()
    const { data: project, isLoading } = useProject(id!)
    const { data: owner } = useUser(project?.ownerId || '')
    const [activeTab, setActiveTab] = useState<'info' | 'members'>('info')



    if (isLoading) {
        return <div className="flex justify-center p-8">Loading...</div>
    }

    if (!project) {
        return <div className="text-center p-8">Project not found</div>
    }

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-3xl font-bold">{project.name}</h1>
                <p className="text-muted-foreground mt-2">{project.description || 'No description'}</p>
            </div>

            <div className="border-b">
                <div className="flex gap-4">
                    <button
                        className={`pb-2 px-1 ${activeTab === 'info' ? 'border-b-2 border-primary text-primary' : 'text-muted-foreground'}`}
                        onClick={() => setActiveTab('info')}
                    >
                        Info
                    </button>
                    <button
                        className={`pb-2 px-1 ${activeTab === 'members' ? 'border-b-2 border-primary text-primary' : 'text-muted-foreground'}`}
                        onClick={() => setActiveTab('members')}
                    >
                        Members
                    </button>
                </div>
            </div>

            {activeTab === 'info' && (
                <Card className="p-6">
                    <div className="space-y-3">
                        <div>
                            <p className="text-sm text-muted-foreground">Created</p>
                            <p className="font-medium">{new Date(project.createdAt).toLocaleDateString()}</p>
                        </div>
                        <div>
                            <p className="text-sm text-muted-foreground">Owner</p>
                            {owner ? (
                                <>
                                    <p className="font-medium">{owner.fullName}</p>
                                    <p className="text-sm text-muted-foreground">{owner.email}</p>
                                </>
                            ) : (
                                <p className="font-medium">Loading owner info...</p>
                            )}
                        </div>
                    </div>
                </Card>
            )}

            {activeTab === 'members' && (
                <Card className="p-6">
                    <MembersManager project={project} />
                </Card>
            )}
        </div>
    )
}
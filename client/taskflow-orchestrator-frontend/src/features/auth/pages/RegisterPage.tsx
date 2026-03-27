import { RegisterForm } from '../components/RegisterForm'

export default function RegisterPage() {
  return (
    <div className="rounded-lg border bg-card p-8 shadow-lg">
      <div className="mb-6 text-center">
        <h1 className="text-2xl font-bold">Create Account</h1>
        <p className="text-sm text-muted-foreground mt-2">
          Sign up to start managing your tasks
        </p>
      </div>
      <RegisterForm />
    </div>
  )
}

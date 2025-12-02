export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <div className="min-h-screen bg-gray-50">
      {/* TODO: Add sidebar and header */}
      <main className="p-8">
        {children}
      </main>
    </div>
  )
}

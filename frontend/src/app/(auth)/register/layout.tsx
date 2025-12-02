import { Footer } from '@/components/layout/Footer'

export default function RegisterLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      {/* Header Section */}
      <header className="py-8 text-center">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">
          Merhaba
        </h1>
        <p className="text-gray-500 max-w-2xl mx-auto px-4">
          Sizinle iş ortağı olarak bir araya gelmekten büyük memnuniyet duyuyoruz.
          Bu iş birliğinin karşılıklı güven ve uyum içinde uzun yıllar devam etmesini,
          birlikte birçok başarılı işlere imza atmayı gönülden diliyoruz.
        </p>
      </header>

      {/* Main Content */}
      <main className="flex-1 px-4 pb-8">
        {children}
      </main>

      {/* Footer */}
      <Footer />
    </div>
  )
}

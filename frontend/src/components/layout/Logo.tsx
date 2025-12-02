import Link from 'next/link'

interface LogoProps {
  href?: string
  className?: string
}

export function Logo({ href = '/', className }: LogoProps) {
  return (
    <Link href={href} className={`inline-flex items-center gap-2 ${className}`}>
      {/* Shield Logo SVG */}
      <svg className="w-8 h-8 text-primary" viewBox="0 0 32 32" fill="currentColor">
        <path d="M16 2L4 7v9c0 7.732 10.039 11.871 11.627 12.432a1 1 0 00.746 0C17.961 27.871 28 23.732 28 16V7L16 2zm0 2.236L26 8.5v7.5c0 5.988-8.044 9.622-10 10.413C14.044 25.622 6 21.988 6 16V8.5l10-4.264z" />
        <path d="M16 7L8 10.5v5.5c0 4.418 6.268 7.082 8 7.822 1.732-.74 8-3.404 8-7.822v-5.5L16 7z" fillOpacity="0.3" />
      </svg>
      <span className="text-xl font-semibold text-gray-900">vesmarket</span>
    </Link>
  )
}

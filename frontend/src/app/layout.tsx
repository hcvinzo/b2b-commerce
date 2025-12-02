import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Vesmarket - B2B İş Ortağı Portalı",
  description: "B2B E-Commerce Platform for Dealer Partners",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="tr">
      <body className="antialiased">
        {children}
      </body>
    </html>
  );
}

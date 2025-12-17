import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  output: "standalone",
  basePath: process.env.NEXT_PUBLIC_BASE_PATH || "",
  assetPrefix: process.env.NEXT_PUBLIC_BASE_PATH || "",
  images: {
    path: `${process.env.NEXT_PUBLIC_BASE_PATH || ""}/_next/image`,
    remotePatterns: [
      {
        protocol: "https",
        hostname: "d1yz7b5ylau9xo.cloudfront.net",
        pathname: "/**",
      },
    ],
  },
};

export default nextConfig;

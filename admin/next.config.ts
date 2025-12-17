import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  images: {
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

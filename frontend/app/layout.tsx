import type { Metadata } from "next";
import { AntdRegistry } from '@ant-design/nextjs-registry';
import { App } from 'antd';
import "./globals.css";

export const metadata: Metadata = {
  title: "IDCOL Loan Repayment Engine",
  description: "Daily-basis accrual loan repayment engine",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body suppressHydrationWarning>
        <AntdRegistry>
          <App>
            {children}
          </App>
        </AntdRegistry>
      </body>
    </html>
  );
}

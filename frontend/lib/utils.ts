import dayjs from 'dayjs';
import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

const EPOCH = Date.UTC(1899, 11, 30);
const DAYMS = 86400000;

export function dateToSerial(date: any): number | null {
  if (!date) return null;
  const d = dayjs(date);
  return Math.round((Date.UTC(d.year(), d.month(), d.date()) - EPOCH) / DAYMS);
}

export function serialToDate(s: number): Date {
  return new Date(EPOCH + s * DAYMS);
}

export function formatSerialDate(s: number): string {
  // Use a fixed locale and UTC to prevent hydration mismatches between server and client
  const date = serialToDate(s);
  return new Intl.DateTimeFormat('en-GB', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    timeZone: 'UTC'
  }).format(date);
}

export function formatCurrency(val: number | null | undefined): string {
  return (val || 0).toLocaleString('en-US', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
}

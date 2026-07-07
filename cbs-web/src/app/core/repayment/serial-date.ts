// Excel-serial-date helpers for display (epoch 1899-12-30), matching the engine's date space.
const EPOCH_MS = Date.UTC(1899, 11, 30);
const DAY_MS = 86_400_000;

export function serialToDate(serial: number): Date {
  return new Date(EPOCH_MS + serial * DAY_MS);
}

export function serialToIso(serial: number): string {
  return serialToDate(serial).toISOString().slice(0, 10);
}

export function dateToSerial(date: Date): number {
  return Math.round((Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()) - EPOCH_MS) / DAY_MS);
}

/** Parses an ISO "yyyy-MM-dd" string directly (no local-timezone Date round-trip pitfalls). */
export function isoToSerial(iso: string): number {
  const [y, m, d] = iso.split('-').map(Number);
  return Math.round((Date.UTC(y, m - 1, d) - EPOCH_MS) / DAY_MS);
}

export function addMonthsToIso(iso: string, months: number): string {
  const [y, m, d] = iso.split('-').map(Number);
  const total = (m - 1) + months;
  const year = y + Math.floor(total / 12);
  const month = ((total % 12) + 12) % 12;
  const lastDay = new Date(Date.UTC(year, month + 1, 0)).getUTCDate();
  const day = Math.min(d, lastDay);
  return new Date(Date.UTC(year, month, day)).toISOString().slice(0, 10);
}

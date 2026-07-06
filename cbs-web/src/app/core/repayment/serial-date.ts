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

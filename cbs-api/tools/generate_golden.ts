// Generates "golden" reference output for all 19 real IDCOL borrower schedules by running the
// validated NestJS EngineService. These become the regression fixtures the C# RepaymentEngine
// port must reproduce (within tolerance), proving the port is faithful to the reference engine
// that was itself validated against IDCOL's real Excel workbooks.
import * as fs from 'fs';
import * as path from 'path';
import { EngineService } from '../../backend/src/engine/engine.service';
import { EX_DATA } from '../../frontend/lib/examples';

const engine = new EngineService();
const outDir = path.join(__dirname, '..', 'tests', 'IDCOL.CBS.RepaymentEngine.RegressionFixtures', 'GoldenData');
fs.mkdirSync(outDir, { recursive: true });

const keys = Object.keys(EX_DATA);
const manifest: { key: string; rows: number }[] = [];

for (const key of keys) {
  // structuredClone-equivalent so the engine's in-place mutations don't corrupt the source data.
  const params = JSON.parse(JSON.stringify(EX_DATA[key]));
  const schedule = engine.generateSchedule(params);

  // Persist both the exact input params and the produced rows so the C# test feeds identical
  // input and compares against identical expected output.
  const golden = {
    key,
    params: EX_DATA[key],
    rows: schedule.map((r: any) => ({
      idx: r.idx,
      payDate: r.payDate,
      openingBal: r.openingBal,
      periodRate: r.periodRate,
      interest: r.interest,
      cashInterest: r.cashInterest,
      capInterest: r.capInterest,
      principal: r.principal,
      tds: r.tds,
      closingBal: r.closingBal,
      days: r.days,
    })),
  };

  fs.writeFileSync(path.join(outDir, `${key}.json`), JSON.stringify(golden, null, 2));
  manifest.push({ key, rows: schedule.length });
  console.log(`${key}: ${schedule.length} rows`);
}

fs.writeFileSync(path.join(outDir, '_manifest.json'), JSON.stringify(manifest, null, 2));
console.log(`\nWrote ${keys.length} golden files to ${outDir}`);

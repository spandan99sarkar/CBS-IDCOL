
import { EngineService } from './backend/src/engine/engine.service';
import { EX_DATA } from './frontend/lib/examples';

const engine = new EngineService();

function compare(key: string) {
    const data = EX_DATA[key];
    if (!data) return;

    console.log(`--- Comparing ${key} ---`);
    const schedule = engine.generateSchedule(data);
    
    const expectedInt = data.interest_payment_amounts;
    const expectedPri = data.principal_schedule_amounts;

    if (!expectedInt) {
        console.log("No expected data for " + key);
        return;
    }

    let maxDiffInt = 0;
    let maxDiffPri = 0;

    for (let i = 0; i < Math.min(schedule.length, expectedInt.length); i++) {
        const row = schedule[i];
        const diffInt = Math.abs(row.interest - expectedInt[i]);
        const diffPri = Math.abs(row.principal - expectedPri[i]);
        
        if (diffInt > 0.1 || diffPri > 0.1) {
            console.log(`Row ${i+1} mismatch:`);
            console.log(`  Int: engine=${row.interest.toFixed(2)}, excel=${expectedInt[i].toFixed(2)} (diff=${diffInt.toFixed(2)})`);
            console.log(`  Pri: engine=${row.principal.toFixed(2)}, excel=${expectedPri[i].toFixed(2)} (diff=${diffPri.toFixed(2)})`);
        }

        maxDiffInt = Math.max(maxDiffInt, diffInt);
        maxDiffPri = Math.max(maxDiffPri, diffPri);
    }

    console.log(`Max Diff - Int: ${maxDiffInt.toFixed(2)}, Pri: ${maxDiffPri.toFixed(2)}`);
}

compare("BPCL");
compare("SCBL");
compare("SKS");
compare("THERMAX");
compare("MCML");
compare("QPSL");

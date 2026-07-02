import { create } from 'zustand';
import dayjs from 'dayjs';

const EPOCH = Date.UTC(1899, 11, 30);
const DAYMS = 86400000;

export interface Disbursement {
  date: any;
  amount: number | null;
  note?: string;
}

export interface RateEvent {
  date: any;
  rate: number | null;
}

export interface Holiday {
  date: any;
  name: string;
}

export interface LoanParams {
  project_name: string;
  currency: string;
  loan_amount: number | null;
  financial_close: any;
  interest_rate: number | null;
  day_count_basis: number;
  num_installments: number | null;
  principal_type: string;
  payment_frequency: number;
  interest_grace_months: number | null;
  principal_grace_months: number | null;
  interest_grace_period_end: any;
  principal_grace_period_end: any;
  interest_capitalized: boolean;
  interest_capitalized_until: any;
  opening_balance_includes_period_disbursements: boolean;
  due_date_include: boolean;
  exclude_open: boolean;
  exclude_close: boolean;
  bd_rule: string;
  weekend_fri_sat: boolean;
  weekend_sat_sun: boolean;
  gen_mode: string;
  first_payment: any;
  payment_day: number;
  disbursements: Disbursement[];
  interest_rate_change_events: RateEvent[];
  principal_schedule_percentages: number[];
  principal_schedule_amounts: number[];
  holidays: Holiday[];
  manual_dates: any[];
  annuity_recalculate_on_rate_or_disbursement: boolean;
  annuity_use_period_rate: boolean;
  total_debt_service_includes_capitalized_interest: boolean;
  interest_rates_by_period?: number[];
  interest_payment_amounts?: number[];
  opening_balance_amounts?: number[];
  closing_balance_amounts?: number[];
  interest_day_adjustments?: any[];
  total_debt_service_mode?: string;
}

interface LoanStore {
  params: LoanParams;
  schedule: any[];
  versions: any[];
  activeVersion: number;
  loading: boolean;
  setParams: (params: Partial<LoanParams>) => void;
  setSchedule: (schedule: any[]) => void;
  setLoading: (loading: boolean) => void;
  addVersion: (desc: string, tag: string) => void;
  loadVersion: (index: number) => void;
  resetParams: () => void;
  computeSchedule: () => Promise<any>;
  loadExample: (data: any) => void;
}

const initialParams: LoanParams = {
  project_name: '',
  currency: 'BDT',
  loan_amount: null,
  financial_close: null,
  interest_rate: 10,
  day_count_basis: 360,
  num_installments: 28,
  principal_type: 'Level Principal',
  payment_frequency: 4,
  interest_grace_months: 0,
  principal_grace_months: 12,
  interest_grace_period_end: null,
  principal_grace_period_end: null,
  interest_capitalized: false,
  interest_capitalized_until: null,
  opening_balance_includes_period_disbursements: true,
  due_date_include: false,
  exclude_open: true,
  exclude_close: true,
  bd_rule: 'none',
  weekend_fri_sat: false,
  weekend_sat_sun: false,
  gen_mode: 'auto',
  first_payment: null,
  payment_day: 15,
  disbursements: [{ date: null, amount: null, note: 'DD 1' }],
  interest_rate_change_events: [],
  principal_schedule_percentages: [],
  principal_schedule_amounts: [],
  holidays: [],
  manual_dates: [],
  annuity_recalculate_on_rate_or_disbursement: false,
  annuity_use_period_rate: false,
  total_debt_service_includes_capitalized_interest: false,
};

export const useLoanStore = create<LoanStore>((set, get) => ({
  params: initialParams,
  schedule: [],
  versions: [],
  activeVersion: -1,
  loading: false,

  setParams: (newParams) => 
    set((state) => ({ params: { ...state.params, ...newParams } })),

  setSchedule: (schedule) => set({ schedule }),

  setLoading: (loading) => set({ loading }),

  computeSchedule: async () => {
    const { params, setSchedule, setLoading, addVersion, schedule } = get();
    console.log('Starting computeSchedule with params:', params);
    setLoading(true);
    try {
      const { dateToSerial } = await import('./utils');
      
      const payload = {
        ...params,
        financial_close: dateToSerial(params.financial_close),
        interest_rate: (params.interest_rate || 0) / 100,
        disbursements: params.disbursements
          .filter(d => d.date && d.amount !== null && d.amount !== undefined)
          .map(d => [dateToSerial(d.date), d.amount]),
        first_payment: dateToSerial(params.first_payment),
        interest_grace_period_end: dateToSerial(params.interest_grace_period_end),
        principal_grace_period_end: dateToSerial(params.principal_grace_period_end),
        interest_rate_change_events: (params.interest_rate_change_events || [])
          .filter(e => e.date && e.rate !== null)
          .map(e => [dateToSerial(e.date), (e.rate || 0) / 100]),
        principal_schedule_percentages: params.principal_schedule_percentages,
        principal_schedule_amounts: params.principal_schedule_amounts,
        interest_rates_by_period: params.interest_rates_by_period,
        interest_payment_amounts: params.interest_payment_amounts,
        opening_balance_amounts: params.opening_balance_amounts,
        closing_balance_amounts: params.closing_balance_amounts,
        interest_day_adjustments: params.interest_day_adjustments,
        total_debt_service_mode: params.total_debt_service_mode,
        interest_capitalized: params.interest_capitalized,
        interest_capitalized_until: dateToSerial(params.interest_capitalized_until),
        repayment_dates: params.gen_mode === 'manual' ? params.manual_dates.map(d => dateToSerial(d)) : [],
      };

      console.log('Sending payload to backend:', payload);
      const response = await fetch('http://127.0.0.1:3001/engine/compute', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        const errorText = await response.text();
        console.error('Backend error:', errorText);
        throw new Error(`Engine computation failed: ${response.status} ${errorText}`);
      }
      
      const data = await response.json();
      console.log('Received data from backend:', data);
      
      set((state) => {
        const nextState: any = { 
          schedule: data, 
          loading: false 
        };
        // Only add 'initial' version if it's the first time we get a schedule
        // and no versions exist yet.
        if (state.schedule.length === 0 && state.versions.length === 0) {
          const newVersion = {
            desc: 'Initial computation',
            tag: 'initial',
            when: new Date().toLocaleString(),
            params: JSON.parse(JSON.stringify(state.params)),
            schedule: JSON.parse(JSON.stringify(data)),
          };
          nextState.versions = [...state.versions, newVersion];
          nextState.activeVersion = nextState.versions.length - 1;
        }
        return nextState;
      });
      
      return data;
    } catch (error) {
      console.error('computeSchedule error:', error);
      set({ loading: false });
      throw error;
    }
  },

  addVersion: (desc, tag) => set((state) => {
    const newVersion = {
      desc,
      tag,
      when: new Date().toLocaleString(),
      params: JSON.parse(JSON.stringify(state.params)),
      schedule: JSON.parse(JSON.stringify(state.schedule)),
    };
    const newVersions = [...state.versions, newVersion];
    return {
      versions: newVersions,
      activeVersion: newVersions.length - 1
    };
  }),

  loadVersion: (index) => set((state) => {
    const version = state.versions[index];
    if (!version) return state;
    return {
      params: JSON.parse(JSON.stringify(version.params)),
      schedule: JSON.parse(JSON.stringify(version.schedule)),
      activeVersion: index
    };
  }),

  resetParams: () => set({ params: initialParams, schedule: [], activeVersion: -1 }),
  loadExample: (data: any) => {
    const newParams: LoanParams = {
      ...initialParams,
      project_name: data.project_name,
      currency: data.currency || 'BDT',
      loan_amount: data.loan_amount,
      financial_close: data.financial_close ? dayjs(new Date(Date.UTC(1899, 11, 30) + data.financial_close * 86400000)) : null,
      interest_rate: data.interest_rate * 100,
      day_count_basis: data.day_count_basis,
      num_installments: data.num_installments,
      principal_type: data.principal_type,
      interest_grace_period_end: data.interest_grace_period_end ? dayjs(new Date(EPOCH + data.interest_grace_period_end * DAYMS)) : null,
      principal_grace_period_end: data.principal_grace_period_end ? dayjs(new Date(EPOCH + data.principal_grace_period_end * DAYMS)) : null,
      interest_capitalized: !!data.interest_capitalized || !!data.interest_capitalized_until,
      interest_capitalized_until: data.interest_capitalized_until ? dayjs(new Date(EPOCH + data.interest_capitalized_until * DAYMS)) : null,
      interest_rate_change_events: (data.interest_rate_change_events || []).map((e: any) => ({
        date: dayjs(new Date(EPOCH + e[0] * DAYMS)),
        rate: e[1] * 100
      })),
      principal_schedule_percentages: data.principal_schedule_percentages || [],
      principal_schedule_amounts: data.principal_schedule_amounts || [],
      disbursements: (data.disbursements || []).map((d: any) => ({
        date: dayjs(new Date(EPOCH + d[0] * DAYMS)),
        amount: d[1],
        note: 'DD'
      })),
      gen_mode: 'manual',
      manual_dates: (data.repayment_dates || []).map((d: any) => dayjs(new Date(EPOCH + d * DAYMS))),
      interest_rates_by_period: data.interest_rates_by_period,
      interest_payment_amounts: data.interest_payment_amounts,
      opening_balance_amounts: data.opening_balance_amounts,
      closing_balance_amounts: data.closing_balance_amounts,
      interest_day_adjustments: data.interest_day_adjustments,
      total_debt_service_mode: data.total_debt_service_mode,
      annuity_recalculate_on_rate_or_disbursement: !!data.annuity_recalculate_on_rate_or_disbursement,
      annuity_use_period_rate: !!data.annuity_use_period_rate,
      total_debt_service_includes_capitalized_interest: !!data.total_debt_service_includes_capitalized_interest,
    };
    set({ params: newParams, schedule: [], activeVersion: -1 });
  }
}));

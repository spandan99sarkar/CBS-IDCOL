'use client';

import React from 'react';
import { Card, Row, Col, Statistic } from 'antd';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, AreaChart, Area } from 'recharts';
import { TrendingUp, DollarSign, Calendar, Clock } from 'lucide-react';
import { formatSerialDate, formatCurrency } from '@/lib/utils';

interface SummaryViewProps {
  schedule: any[];
  params: any;
}

export const SummaryView: React.FC<SummaryViewProps> = ({ schedule, params }) => {
  if (schedule.length === 0) return null;

  const totalInterest = schedule.reduce((sum, r) => sum + r.interest, 0);
  const totalPrincipal = schedule.reduce((sum, r) => sum + r.principal, 0);
  const lastRow = schedule[schedule.length - 1];
  const tenorDays = lastRow.payDate - schedule[0].periodStart;
  const tenorYears = (tenorDays / 365).toFixed(1);

  const chartData = schedule.map(r => ({
    date: formatSerialDate(r.payDate),
    outstanding: r.closingBal,
    cumulativeInterest: r.cumInt,
  }));

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card className="shadow-sm border-none bg-gradient-to-br from-navy to-navy2 text-white">
          <Statistic
            title={<span className="text-blue-200 text-xs font-bold uppercase tracking-wider">Sanctioned Amount</span>}
            value={params.loan_amount}
            precision={0}
            prefix={<DollarSign size={16} className="mr-1" />}
            valueStyle={{ color: '#fff', fontSize: '24px', fontWeight: 'bold' }}
            suffix={<span className="text-blue-300 text-xs ml-2">{params.currency}</span>}
          />
          <div className="text-[10px] text-blue-300/60 mt-1">Total Loan Commitment</div>
        </Card>

        <Card className="shadow-sm border-gray-100">
          <Statistic
            title={<span className="text-gray-400 text-xs font-bold uppercase tracking-wider">Total Interest</span>}
            value={totalInterest}
            precision={2}
            valueStyle={{ color: '#1a56b0', fontSize: '24px', fontWeight: 'bold' }}
          />
          <div className="flex items-center gap-1 text-[10px] text-blue-500 mt-1 font-medium">
            <TrendingUp size={10} />
            Over loan life
          </div>
        </Card>

        <Card className="shadow-sm border-gray-100">
          <Statistic
            title={<span className="text-gray-400 text-xs font-bold uppercase tracking-wider">Tenor</span>}
            value={tenorYears}
            suffix="Years"
            valueStyle={{ color: '#0b1f3a', fontSize: '24px', fontWeight: 'bold' }}
          />
          <div className="flex items-center gap-1 text-[10px] text-gray-500 mt-1 font-medium">
            <Clock size={10} />
            {schedule.length} Installments
          </div>
        </Card>

        <Card className="shadow-sm border-gray-100">
          <Statistic
            title={<span className="text-gray-400 text-xs font-bold uppercase tracking-wider">Final Closing</span>}
            value={lastRow.closingBal}
            precision={2}
            valueStyle={{ color: lastRow.closingBal > 0.01 ? '#9b1c1c' : '#0f6e4d', fontSize: '24px', fontWeight: 'bold' }}
          />
          <div className="text-[10px] text-gray-500 mt-1 font-medium">Remaining at Maturity</div>
        </Card>
      </div>

      <Card className="shadow-sm border-gray-100" title={<span className="text-xs font-bold text-gray-500 uppercase tracking-widest">Outstanding Principal & Cumulative Interest</span>}>
        <div className="h-[350px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart data={chartData}>
              <defs>
                <linearGradient id="colorPrincipal" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#1a56b0" stopOpacity={0.1}/>
                  <stop offset="95%" stopColor="#1a56b0" stopOpacity={0}/>
                </linearGradient>
                <linearGradient id="colorInterest" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#0f6e4d" stopOpacity={0.1}/>
                  <stop offset="95%" stopColor="#0f6e4d" stopOpacity={0}/>
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f0f0f0" />
              <XAxis dataKey="date" fontSize={10} tickMargin={10} />
              <YAxis fontSize={10} />
              <Tooltip 
                contentStyle={{ borderRadius: '8px', border: 'none', boxShadow: '0 4px 12px rgba(0,0,0,0.1)', fontSize: '12px' }}
                formatter={(value: number) => formatCurrency(value)}
              />
              <Legend verticalAlign="top" height={36} iconType="circle" />
              <Area 
                type="monotone" 
                dataKey="outstanding" 
                name="Outstanding Principal" 
                stroke="#1a56b0" 
                strokeWidth={2}
                fillOpacity={1} 
                fill="url(#colorPrincipal)" 
              />
              <Area 
                type="monotone" 
                dataKey="cumulativeInterest" 
                name="Cumulative Interest" 
                stroke="#0f6e4d" 
                strokeWidth={2}
                strokeDasharray="5 5"
                fillOpacity={1} 
                fill="url(#colorInterest)" 
              />
            </AreaChart>
          </ResponsiveContainer>
        </div>
      </Card>
    </div>
  );
};

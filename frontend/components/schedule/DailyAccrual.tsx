'use client';

import React from 'react';
import { Table } from 'antd';
import { formatSerialDate, formatCurrency } from '@/lib/utils';

interface DailyAccrualProps {
  dailyRows: any[];
}

export const DailyAccrual: React.FC<DailyAccrualProps> = ({ dailyRows }) => {
  const columns = [
    {
      title: 'Date',
      dataIndex: 'date',
      key: 'date',
      render: (val: number) => <span className="text-gray-500">{formatSerialDate(val)}</span>,
    },
    {
      title: 'DoW',
      dataIndex: 'dow',
      key: 'dow',
      render: (val: number) => {
        const days = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
        return <span className="text-gray-400 text-[11px]">{days[val]}</span>;
      },
    },
    {
      title: 'Opening Principal',
      dataIndex: 'opening',
      key: 'opening',
      align: 'right' as const,
      render: (val: number) => <span className="num-tabular text-gray-600">{formatCurrency(val)}</span>,
    },
    {
      title: 'Rate %',
      dataIndex: 'rate',
      key: 'rate',
      align: 'right' as const,
      render: (val: number) => <span className="num-tabular text-gray-400">{(val * 100).toFixed(3)}%</span>,
    },
    {
      title: 'Daily Interest',
      dataIndex: 'daily',
      key: 'daily',
      align: 'right' as const,
      render: (val: number) => <span className="num-tabular text-blue-600 font-medium">{formatCurrency(val)}</span>,
    },
    {
      title: 'Disbursement',
      dataIndex: 'disb',
      key: 'disb',
      align: 'right' as const,
      render: (val: number) => val > 0 ? (
        <span className="num-tabular text-green-600 font-bold">+{formatCurrency(val)}</span>
      ) : <span className="text-gray-300">—</span>,
    },
  ];

  return (
    <div className="bg-gray-50/50 p-2 rounded-lg border border-gray-100 my-1 mx-4 shadow-inner">
      <div className="flex items-center gap-2 mb-2 px-2">
        <div className="w-1.5 h-4 bg-blue-400 rounded-full" />
        <span className="text-[11px] font-bold text-gray-500 uppercase tracking-tight">Daily Accrual Breakdown</span>
      </div>
      <Table
        dataSource={dailyRows}
        columns={columns}
        pagination={false}
        size="small"
        rowKey="date"
        className="daily-accrual-table"
      />
    </div>
  );
};

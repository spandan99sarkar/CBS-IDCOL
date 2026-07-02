'use client';

import React, { useState } from 'react';
import { Table, Select, Input, Button } from 'antd';
import { formatSerialDate, formatCurrency, cn } from '@/lib/utils';
import { Search, ChevronLeft, ChevronRight } from 'lucide-react';

interface DailyJournalProps {
  schedule: any[];
}

export const DailyJournal: React.FC<DailyJournalProps> = ({ schedule }) => {
  const [pageSize, setPageSize] = useState(60);
  const [currentPage, setCurrentPage] = useState(1);
  const [searchDate, setSearchSearchDate] = useState('');

  const allDays = schedule.flatMap(r => 
    (r.dailyRows || []).map(d => ({
      ...d,
      periodIdx: r.idx,
      payDate: r.payDate,
      totalDue: d.date === r.payDate ? r.tds : 0,
      principalDue: d.date === r.payDate ? r.principal : 0,
      interestDue: d.date === r.payDate ? r.interest : 0,
    }))
  );

  const filteredDays = searchDate 
    ? allDays.filter(d => formatSerialDate(d.date).toLowerCase().includes(searchDate.toLowerCase()))
    : allDays;

  const paginatedDays = filteredDays.slice((currentPage - 1) * pageSize, currentPage * pageSize);

  const columns = [
    {
      title: 'Date',
      dataIndex: 'date',
      key: 'date',
      render: (val: number) => <span className="font-medium text-gray-700">{formatSerialDate(val)}</span>,
    },
    {
      title: 'Period',
      dataIndex: 'periodIdx',
      key: 'periodIdx',
      render: (val: number) => <span className="text-gray-400 text-[11px]">P{val + 1}</span>,
    },
    {
      title: 'Opening Balance',
      dataIndex: 'opening',
      key: 'opening',
      align: 'right' as const,
      render: (val: number) => <span className="num-tabular text-gray-600">{formatCurrency(val)}</span>,
    },
    {
      title: 'Daily Interest',
      dataIndex: 'daily',
      key: 'daily',
      align: 'right' as const,
      render: (val: number) => <span className="num-tabular text-blue-600 font-semibold">{formatCurrency(val)}</span>,
    },
    {
      title: 'Events / Due',
      key: 'events',
      render: (_: any, record: any) => (
        <div className="flex flex-col gap-1">
          {record.disb > 0 && (
            <div className="text-[10px] text-green-600 font-bold uppercase tracking-tight">
              + Disbursement: {record.disb.toLocaleString()}
            </div>
          )}
          {record.totalDue > 0 && (
            <div className="text-[10px] text-navy font-bold uppercase tracking-tight">
              Payment Due: {record.totalDue.toLocaleString()}
            </div>
          )}
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-4">
      <div className="bg-white p-4 rounded-xl border border-gray-100 shadow-sm flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Input 
            placeholder="Search date..." 
            prefix={<Search size={14} className="text-gray-400" />}
            className="w-64"
            value={searchDate}
            onChange={e => setSearchSearchDate(e.target.value)}
          />
          <div className="flex items-center gap-2">
            <span className="text-xs text-gray-400">Show:</span>
            <Select 
              value={pageSize} 
              onChange={setPageSize}
              options={[
                { label: '60 rows', value: 60 },
                { label: '120 rows', value: 120 },
                { label: '365 rows', value: 365 },
                { label: 'All', value: 1000000 },
              ]}
              size="small"
              className="w-32"
            />
          </div>
        </div>

        <div className="flex items-center gap-2">
          <Button 
            icon={<ChevronLeft size={14} />} 
            disabled={currentPage === 1}
            onClick={() => setCurrentPage(p => p - 1)}
            size="small"
          />
          <span className="text-xs font-medium text-gray-500">
            Page {currentPage} of {Math.ceil(filteredDays.length / pageSize)}
          </span>
          <Button 
            icon={<ChevronRight size={14} />} 
            disabled={currentPage === Math.ceil(filteredDays.length / pageSize)}
            onClick={() => setCurrentPage(p => p + 1)}
            size="small"
          />
        </div>
      </div>

      <div className="bg-white rounded-xl border border-gray-100 shadow-sm overflow-hidden">
        <Table
          dataSource={paginatedDays}
          columns={columns}
          pagination={false}
          size="small"
          rowKey={(record) => `${record.date}-${record.periodIdx}`}
          rowClassName={(record) => cn(
            record.totalDue > 0 && "bg-blue-50/50 font-bold",
            record.disb > 0 && "bg-green-50/50"
          )}
        />
      </div>
    </div>
  );
};

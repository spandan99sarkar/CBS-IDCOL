'use client';

import React, { useState } from 'react';
import { Table, Tag, Tooltip } from 'antd';
import { ChevronRight, ChevronDown, Calendar, Info } from 'lucide-react';
import { formatSerialDate, formatCurrency, cn } from '@/lib/utils';
import { DailyAccrual } from './DailyAccrual';

interface ScheduleTableProps {
  data: any[];
}

export const ScheduleTable: React.FC<ScheduleTableProps> = ({ data }) => {
  const [expandedRowKeys, setExpandedRowKeys] = useState<number[]>([]);

  const columns = [
    {
      title: '#',
      dataIndex: 'idx',
      key: 'idx',
      width: 60,
      fixed: 'left' as const,
      render: (val: number) => <span className="text-gray-400 font-medium">{val + 1}</span>,
    },
    {
      title: 'Payment Date',
      dataIndex: 'payDate',
      key: 'payDate',
      width: 140,
      fixed: 'left' as const,
      render: (val: number) => (
        <div className="flex items-center gap-2">
          <Calendar size={12} className="text-blue-500" />
          <span className="font-semibold">{formatSerialDate(val)}</span>
        </div>
      ),
    },
    {
      title: 'Days',
      dataIndex: 'days',
      key: 'days',
      width: 80,
      align: 'right' as const,
      render: (val: number) => <span className="num-tabular">{val}</span>,
    },
    {
      title: 'Opening Bal',
      dataIndex: 'openingBal',
      key: 'openingBal',
      width: 150,
      align: 'right' as const,
      render: (val: number) => <span className="num-tabular font-medium">{formatCurrency(val)}</span>,
    },
    {
      title: 'Interest',
      dataIndex: 'interest',
      key: 'interest',
      width: 150,
      align: 'right' as const,
      render: (val: number, record: any) => (
        <div className="flex flex-col items-end">
          <span className="num-tabular text-blue-700 font-semibold">{formatCurrency(val)}</span>
          {record.capInterest > 0 && (
            <Tag color="purple" className="text-[9px] px-1 py-0 border-0 leading-none mt-1">CAPITALIZED</Tag>
          )}
        </div>
      ),
    },
    {
      title: 'Principal',
      dataIndex: 'principal',
      key: 'principal',
      width: 150,
      align: 'right' as const,
      render: (val: number) => <span className="num-tabular text-green-700 font-semibold">{formatCurrency(val)}</span>,
    },
    {
      title: 'Total Debt Svc',
      dataIndex: 'tds',
      key: 'tds',
      width: 150,
      align: 'right' as const,
      render: (val: number) => <span className="num-tabular font-bold text-navy">{formatCurrency(val)}</span>,
    },
    {
      title: 'Closing Bal',
      dataIndex: 'closingBal',
      key: 'closingBal',
      width: 150,
      align: 'right' as const,
      render: (val: number) => <span className="num-tabular font-medium text-gray-600">{formatCurrency(val)}</span>,
    },
    {
      title: 'Notes',
      key: 'notes',
      width: 120,
      render: (_: any, record: any) => (
        <div className="flex gap-1">
          {record.isCapRow && <Tag color="orange" className="text-[10px]">CAP</Tag>}
          {record.isGrace && <Tag color="blue" className="text-[10px]">GRACE</Tag>}
          {record.newDisb > 0 && <Tag color="cyan" className="text-[10px]">+DISB</Tag>}
        </div>
      ),
    },
  ];

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100">
      <Table
        dataSource={data}
        columns={columns}
        pagination={false}
        size="small"
        rowKey="idx"
        scroll={{ x: 1200 }}
        sticky
        expandable={{
          expandedRowRender: (record) => <DailyAccrual dailyRows={record.dailyRows} />,
          expandIcon: ({ expanded, onExpand, record }) =>
            expanded ? (
              <ChevronDown className="cursor-pointer text-blue-500" size={14} onClick={(e) => onExpand(record, e)} />
            ) : (
              <ChevronRight className="cursor-pointer text-gray-400" size={14} onClick={(e) => onExpand(record, e)} />
            ),
          expandedRowKeys,
          onExpandedRowsChange: (keys) => setExpandedRowKeys(keys as number[]),
        }}
        rowClassName={(record) => cn(
          "hover:bg-blue-50/30 transition-colors cursor-pointer",
          expandedRowKeys.includes(record.idx) && "bg-blue-50/50"
        )}
        onRow={(record) => ({
          onClick: () => {
            const keys = expandedRowKeys.includes(record.idx)
              ? expandedRowKeys.filter(k => k !== record.idx)
              : [...expandedRowKeys, record.idx];
            setExpandedRowKeys(keys);
          },
        })}
        className="schedule-table"
      />
    </div>
  );
};

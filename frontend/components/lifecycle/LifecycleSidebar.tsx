'use client';

import React from 'react';
import { Tabs, Form, Input, InputNumber, DatePicker, Button, Select } from 'antd';
import { Zap, History, Plus, RotateCcw } from 'lucide-react';
import { useLoanStore } from '@/lib/store';

const { Option } = Select;

export const LifecycleSidebar = () => {
  const { versions, activeVersion, loadVersion } = useLoanStore();

  return (
    <div className="h-full flex flex-col">
      <Tabs
        defaultActiveKey="events"
        className="px-6"
        items={[
          {
            key: 'events',
            label: (
              <div className="flex items-center gap-2 py-1 px-1">
                <Zap size={14} className="text-orange-500" />
                <span className="font-bold text-[11px] uppercase tracking-wider">Events</span>
              </div>
            ),
            children: (
              <div className="p-0 space-y-6 pt-4">
                <div className="bg-orange-50 p-3 rounded-lg border border-orange-100">
                  <h4 className="text-[12px] font-bold text-orange-900 mb-1">Lifecycle Management</h4>
                  <p className="text-[10px] text-orange-800/70 leading-relaxed">
                    Apply an event to recompute the schedule and store a new version.
                  </p>
                </div>

                <div className="space-y-4">
                  <div className="border border-gray-100 rounded-lg p-3 bg-gray-50/50">
                    <h5 className="text-[11px] font-bold text-navy uppercase mb-3 flex items-center gap-2">
                      <div className="w-1 h-3 bg-blue-500 rounded-full" />
                      Rate Change
                    </h5>
                    <Form layout="vertical" size="small">
                      <Form.Item label="Effective From" className="mb-2">
                        <DatePicker className="w-full" />
                      </Form.Item>
                      <Form.Item label="New Rate (%)" className="mb-3">
                        <InputNumber className="w-full" placeholder="9.00" />
                      </Form.Item>
                      <Button type="primary" block className="text-[11px] h-8 bg-navy border-none hover:bg-navy2 font-bold">
                        Apply Rate Change
                      </Button>
                    </Form>
                  </div>

                  <div className="border border-gray-100 rounded-lg p-3 bg-gray-50/50">
                    <h5 className="text-[11px] font-bold text-navy uppercase mb-3 flex items-center gap-2">
                      <div className="w-1 h-3 bg-green-500 rounded-full" />
                      Additional Disbursement
                    </h5>
                    <Form layout="vertical" size="small">
                      <Form.Item label="Date" className="mb-2">
                        <DatePicker className="w-full" />
                      </Form.Item>
                      <Form.Item label="Amount" className="mb-3">
                        <InputNumber className="w-full" placeholder="0.00" />
                      </Form.Item>
                      <Button type="primary" block className="text-[11px] h-8 bg-navy border-none hover:bg-navy2 font-bold">
                        Apply Disbursement
                      </Button>
                    </Form>
                  </div>

                  <div className="border border-gray-100 rounded-lg p-3 bg-gray-50/50">
                    <h5 className="text-[11px] font-bold text-navy uppercase mb-3 flex items-center gap-2">
                      <div className="w-1 h-3 bg-purple-500 rounded-full" />
                      Restructure
                    </h5>
                    <Button block icon={<RotateCcw size={12} />} className="text-[11px] h-8 font-bold text-navy border-gray-200">
                      Open Restructure Wizard
                    </Button>
                  </div>
                </div>
              </div>
            ),
          },
          {
            key: 'versions',
            label: (
              <div className="flex items-center gap-2 py-1 px-1">
                <History size={14} className="text-blue-500" />
                <span className="font-bold text-[11px] uppercase tracking-wider">History</span>
              </div>
            ),
            children: (
              <div className="p-0 space-y-3 pt-4">
                {versions.length === 0 ? (
                  <div className="text-center py-12">
                    <div className="text-gray-300 text-3xl mb-2">🕰️</div>
                    <div className="text-gray-400 text-xs italic font-medium">No versions saved yet</div>
                  </div>
                ) : (
                  versions.map((v, i) => (
                    <div 
                      key={i}
                      onClick={() => loadVersion(i)}
                      className={`p-3 rounded-lg border cursor-pointer transition-all ${
                        activeVersion === i 
                        ? 'bg-blue-50 border-blue-200 shadow-sm' 
                        : 'bg-white border-gray-100 hover:border-blue-100 hover:bg-gray-50'
                      }`}
                    >
                      <div className="flex justify-between items-start mb-1">
                        <span className="text-[12px] font-bold text-navy">v{i + 1} · {v.tag}</span>
                        {activeVersion === i && (
                          <div className="px-1.5 py-0.5 bg-blue-500 text-white text-[8px] font-bold rounded">ACTIVE</div>
                        )}
                      </div>
                      <div className="text-[10px] text-gray-500 line-clamp-1 mb-2">{v.desc}</div>
                      <div className="text-[9px] text-gray-400 font-medium">{v.when}</div>
                    </div>
                  ))
                )}
              </div>
            ),
          },
        ]}
      />
    </div>
  );
};

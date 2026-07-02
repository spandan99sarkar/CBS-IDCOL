'use client';

import React from 'react';
import { Select, Button, ConfigProvider, theme, App } from 'antd';
import { PlayCircle, Save, Database } from 'lucide-react';
import { useLoanStore } from '@/lib/store';
import { EX_LABELS, EX_DATA } from '@/lib/examples';

export const Header = () => {
  const { message } = App.useApp();
  const { params, versions, activeVersion, loadVersion, computeSchedule, loading, addVersion, loadExample } = useLoanStore();

  const handleCompute = async () => {
    try {
      const data = await computeSchedule();
      if (data && data.length > 0) {
        message.success('Schedule computed successfully');
      } else {
        message.warning('Engine returned empty schedule. Please check parameters (disbursements and repayment dates).');
      }
    } catch (error: any) {
      message.error(error.message || 'Computation failed');
    }
  };

  const handleSaveVersion = () => {
    const desc = prompt('Version description:', 'Manual save');
    if (desc) {
      addVersion(desc, 'manual');
      message.success('Version saved');
    }
  };

  const handleLoadExample = async (key: string) => {
    if (!key) return;
    const data = EX_DATA[key];
    if (data) {
      loadExample(data);
      message.info(`Loaded parameters for ${data.project_name}`);
      try {
        await computeSchedule();
      } catch (e) {
        // ignore auto-compute error
      }
    }
  };

  return (
    <header className="bg-gradient-to-b from-navy to-navy2 text-white h-[52px] flex items-center px-4 gap-4 shadow-lg z-30 flex-shrink-0">
      <div className="flex items-center gap-3">
        <div className="w-7 h-7 bg-gradient-to-br from-blue-500 to-blue-400 rounded-lg flex items-center justify-center text-sm font-extrabold shadow-inner">
          ৳
        </div>
        <div className="flex flex-col">
          <span className="font-bold text-[14px] tracking-wide leading-tight">
            IDCOL Repayment Engine
          </span>
          <span className="text-[10px] text-blue-200/80 font-medium">
            Daily-basis accrual · holiday-aware · versioned
          </span>
        </div>
      </div>

      <div className="flex-1" />

      <div className="flex items-center gap-2">
        <ConfigProvider
          theme={{
            token: {
              colorBgContainer: 'rgba(255, 255, 255, 0.08)',
              colorBorder: 'rgba(255, 255, 255, 0.15)',
              colorText: '#ffffff',
              colorTextPlaceholder: 'rgba(255, 255, 255, 0.5)',
              colorIcon: 'rgba(255, 255, 255, 0.5)',
              colorIconHover: '#ffffff',
              colorBgElevated: '#13294b',
            },
            components: {
              Select: {
                optionSelectedBg: 'rgba(255, 255, 255, 0.1)',
                selectorBg: 'transparent',
              }
            }
          }}
        >
          <Select
            placeholder="Load example"
            className="w-[240px]"
            onChange={handleLoadExample}
            options={Object.entries(EX_LABELS).filter(([k]) => EX_DATA[k]).map(([k, v]) => ({ label: v, value: k }))}
            style={{ height: 32 }}
          />
          
          {versions.length > 0 && (
            <Select
              value={activeVersion}
              className="w-[180px]"
              options={versions.map((v, i) => ({ label: `v${i+1} · ${v.tag}`, value: i }))}
              onChange={loadVersion}
              style={{ height: 32 }}
            />
          )}
        </ConfigProvider>

        <Button 
          icon={<Save size={14} />} 
          className="bg-navy2/50 border-blue-900/50 text-white hover:bg-navy2 hover:text-white"
          size="small"
          onClick={handleSaveVersion}
        >
          Save Version
        </Button>

        <Button 
          type="primary" 
          icon={<PlayCircle size={14} />} 
          className="bg-accent2 border-none hover:bg-blue-500 font-bold"
          size="small"
          onClick={handleCompute}
          loading={loading}
        >
          Compute Schedule
        </Button>
      </div>
    </header>
  );
};

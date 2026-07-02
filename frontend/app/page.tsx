'use client';

import React, { useState } from 'react';
import { Layout, Tabs, Spin, Button as AntButton, App } from 'antd';
import { LayoutDashboard, FileText, PieChart, Info, ChevronLeft, ChevronRight } from 'lucide-react';
import { Header } from '@/components/layout/Header';
import { LoanForm } from '@/components/loan/LoanForm';
import { ScheduleTable } from '@/components/schedule/ScheduleTable';
import { SummaryView } from '@/components/summary/SummaryView';
import { DailyJournal } from '@/components/schedule/DailyJournal';
import { LifecycleSidebar } from '@/components/lifecycle/LifecycleSidebar';
import { useLoanStore } from '@/lib/store';
import { dateToSerial } from '@/lib/utils';

const { Sider, Content } = Layout;

export default function Dashboard() {
  const { message } = App.useApp();
  const { params, schedule, loading, computeSchedule } = useLoanStore();
  const [activeTab, setActiveTab] = useState('periodic');
  const [rightSiderCollapsed, setRightSiderCollapsed] = useState(false);
  const [mounted, setMounted] = useState(false);

  React.useEffect(() => {
    setMounted(true);
  }, []);

  const handleCompute = async () => {
    try {
      await computeSchedule();
    } catch (error: any) {
      message.error(error.message || 'Error communicating with engine');
    }
  };

  if (!mounted) return null;

  return (
    <Layout className="h-screen flex flex-col overflow-hidden">
      <Header />
      
      <Layout className="flex-1 min-h-0">
        <Sider width={340} theme="light" className="border-r border-gray-100 overflow-y-auto custom-scrollbar shadow-sm">
          <LoanForm />
        </Sider>
        
        <Content className="flex flex-col bg-[#f8fafc] overflow-hidden relative">
          <div className="bg-white border-b border-gray-100 px-6 flex items-center justify-between">
            <Tabs
              activeKey={activeTab}
              onChange={setActiveTab}
              className="dashboard-tabs"
              items={[
                { 
                  key: 'periodic', 
                  label: (
                    <div className="flex items-center gap-2 py-3 px-1">
                      <LayoutDashboard size={14} className={activeTab === 'periodic' ? 'text-blue-500' : 'text-gray-400'} />
                      <span className="font-bold text-[12px] uppercase tracking-wide">Periodic Schedule</span>
                    </div>
                  )
                },
                { 
                  key: 'daily', 
                  label: (
                    <div className="flex items-center gap-2 py-3 px-1">
                      <FileText size={14} className={activeTab === 'daily' ? 'text-blue-500' : 'text-gray-400'} />
                      <span className="font-bold text-[12px] uppercase tracking-wide">Daily Journal</span>
                    </div>
                  )
                },
                { 
                  key: 'summary', 
                  label: (
                    <div className="flex items-center gap-2 py-3 px-1">
                      <PieChart size={14} className={activeTab === 'summary' ? 'text-blue-500' : 'text-gray-400'} />
                      <span className="font-bold text-[12px] uppercase tracking-wide">Executive Summary</span>
                    </div>
                  )
                },
              ]}
            />
            
            <div className="flex items-center gap-4">
              {schedule.length > 0 && (
                <div className="flex items-center gap-4 text-[11px] text-gray-400 font-medium mr-4">
                  <div className="flex items-center gap-1.5">
                    <div className="w-2 h-2 rounded-full bg-green-500" />
                    <span>Engine: <b>Online</b></span>
                  </div>
                  <div className="flex items-center gap-1.5">
                    <Info size={12} />
                    <span>Basis: <b>Actual/{params.day_count_basis}</b></span>
                  </div>
                </div>
              )}
              
              <AntButton 
                type="text" 
                size="small" 
                icon={rightSiderCollapsed ? <ChevronLeft size={16} /> : <ChevronRight size={16} />}
                onClick={() => setRightSiderCollapsed(!rightSiderCollapsed)}
                className="hover:bg-gray-100 flex items-center gap-1 text-gray-500 font-medium"
              >
                {rightSiderCollapsed ? "Show Events" : "Hide"}
              </AntButton>
            </div>
          </div>

          <div className="flex-1 overflow-y-auto relative custom-scrollbar">
            <div className="p-6">
              {loading && schedule.length > 0 && (
                <div className="absolute top-0 left-0 w-full h-1 bg-blue-500 animate-pulse z-50" />
              )}
              {schedule.length === 0 ? (
                <Spin spinning={loading} description="Computing schedule...">
                  <div className="h-full flex flex-col items-center justify-center text-center max-w-md mx-auto py-20">
                    <div className="w-20 h-20 bg-blue-50 rounded-full flex items-center justify-center mb-6 shadow-sm border border-blue-100">
                      <PieChart size={40} className="text-blue-400 animate-pulse" />
                    </div>
                    <h2 className="text-navy font-extrabold text-2xl mb-3 tracking-tight">Ready to Compute</h2>
                    <p className="text-gray-500 text-sm leading-relaxed mb-8">
                      Configure your loan parameters in the left sidebar, including disbursements and interest rates, then press <b>Compute Schedule</b> to generate the repayment plan.
                    </p>
                    <div className="grid grid-cols-2 gap-4 w-full">
                      <div className="p-4 bg-white border border-gray-100 rounded-xl text-left shadow-sm">
                        <div className="font-bold text-navy text-[12px] mb-1">Holiday Aware</div>
                        <div className="text-[10px] text-gray-400 italic">Adjusts for BD banking holidays automatically.</div>
                      </div>
                      <div className="p-4 bg-white border border-gray-100 rounded-xl text-left shadow-sm">
                        <div className="font-bold text-navy text-[12px] mb-1">Version Control</div>
                        <div className="text-[10px] text-gray-400 italic">Save and compare different loan scenarios.</div>
                      </div>
                    </div>
                  </div>
                </Spin>
              ) : (
                <div className="max-w-[1400px] mx-auto animate-in fade-in duration-500">
                  {activeTab === 'periodic' && <ScheduleTable data={schedule} />}
                  {activeTab === 'summary' && <SummaryView schedule={schedule} params={params} />}
                  {activeTab === 'daily' && <DailyJournal schedule={schedule} />}
                </div>
              )}
            </div>
          </div>
        </Content>

        <Sider 
          width={300} 
          theme="light" 
          collapsible 
          collapsed={rightSiderCollapsed}
          trigger={null}
          collapsedWidth={0}
          className="border-l border-gray-100 overflow-y-auto custom-scrollbar shadow-sm"
        >
          <LifecycleSidebar />
        </Sider>
      </Layout>
    </Layout>
  );
}
